// All scripts go into the same namespace
if (Site === undefined) {
    var Site = {};
}

/* Handling of forms inside Bootstrap 4 modals */
Site.ModalForm = function ($) {
    var loggerName = 'JsLogger.Site.ModalForm';

    // create a dynamic DIV element and insert it as first child of BODY
    var modalContainer = $('<div/>', { id: 'modal-container-' + Math.random().toString(36).substr(2, 16) });
    $('body').prepend(modalContainer);

    var modalDialogTemplate = [
        // see also: modalFullTemplate
        '<div class="modal-dialog">',
            '<div class="modal-content">',
                '<div class="modal-header">',
                    '<i class="fas fa-2x fa-bomb text-danger"></i>',
                    '<button type="button" class="close" data-dismiss="modal">&times;</button>',
                '</div>',
                '<div class="modal-body">',
                    '<div class="text-danger"><span class="h4 text-danger">$0</span> $1</div>',
                '</div>',
            '</div>',
        '</div>'
    ].join('');

    var modalFullTemplate = [
        '<div class="modal" data-keyboard="true" tabindex="-1">', //data-keyboard=true allows to close the modal with ESC key
            modalDialogTemplate,
        '</div>'
    ].join('');

    var fillTemplate = function (template, errorText, errorNo) {
        return template.replace('$0', errorText).replace('$1', errorNo);
    };

    var showLoading = function (jqElement, on) {
        if (!jqElement.hasClass('btn')) {
            // It's not a bootstrap 4 element displayed as a button
            return;
        }
        if (on === true) {
            jqElement.prepend(
                '<span id="ajax-loading-icon" class="spinner-border spinner-border-sm mr-2" role="status" aria-hidden="true"></span>');
        } else {
            jqElement.find('#ajax-loading-icon').remove();
        }
    };
    
    // language setting is taken from <html lang="iso-code">
    $.i18n().load({
        'en': {
            'modal-form-error-occurred': 'Oops... an error occurred.'
        },
        'de': {
            'modal-form-error-occurred': 'Ups... ein Fehler ist aufgetreten.'
        }
    });

    // requires an element like <button> or <a>, containing data-toggle="site-ajax-modal"
    // e.g. <button type="button" data-toggle="site-ajax-modal" data-target="#id-in-partial-view" data-url="url-to-partial-view">do sth.</button >
    // load the partial view into the placeholder and show the modal
    $('[data-toggle="site-ajax-modal"]').click(function(event) {
        event.preventDefault();
        var submittingElement = $(event.target);
        submittingElement.attr('disabled', 'disabled').css('cursor', 'not-allowed');
        showLoading(submittingElement, true);
        var actionUrl = $(this).data('url');
        $.ajax(actionUrl,
            {
                'async': true,  // important for 'always' to be called
                'type': 'get',
                'timeout': 10000
            })
            .done(function (data, textStatus, jqXHR) {
                // server returns JSON with redirectUrl value
                if (isJson(data)) {
                    if ($.isEmptyObject(data)) {
                        JL(loggerName).warn({ 'msg': 'JSON result is empty' });
                    } else {
                        if (data.redirectUrl) { window.location.href = data.redirectUrl; }
                        else { JL(loggerName).warn({ 'msg': 'JSON redirectUrl is empty' });}
                    }
                }
                // Server should return a PARTIAL view with the form after server side validation.
                // A full page view (identified by BODY element, e.g. caused by a bad action url) would mess up the browser
                if ($.type(data) === 'string' && data.match(/<body[^>]*>/gi)) {
                    JL(loggerName).error({
                        'msg': 'AJAX response is not a partial view',
                        'url': actionUrl
                    });
                    modalContainer.html(fillTemplate(modalFullTemplate, $.i18n('modal-form-error-occurred'), ''));
                    modalContainer.find('.modal').modal('show');
                    return;
                }
                modalContainer.html(data);
                $(modalContainer).on('shown.bs.modal', function () {
                    $('input[autofocus]:first').trigger('focus');
                });
                modalContainer.find('.modal').modal('show');
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                JL(loggerName).error({
                    'msg': "AJAX error response",
                    'errorThrown': errorThrown,
                    'url': actionUrl
                });
                modalContainer.html(fillTemplate(modalFullTemplate, $.i18n('modal-form-error-occurred'), textStatus === 'timeout' ? 'Timeout' : jqXHR.status));
                modalContainer.find('.modal').modal('show');
            })
            .always(function (data, textStatus, jqXHR) { /* parameters in case of error: jqXHR, textStatus, errorThrown */
                submittingElement.removeAttr('disabled').css('cursor', '');
                showLoading(submittingElement, false);
            });
    });

    // Enter key in forms with more than one input field will also trigger 'submit'
    modalContainer.on('keypress', function (e) {
        if (e.keyCode === 13) {
            e.preventDefault();
            $('[type="submit"]').trigger('click');
        }
    });

    // A TagHelper creates a button <button type="submit" site-data="submit">Save</button>
    modalContainer.on('click',
        '[site-data="submit"]',
        function (event) {
            event.preventDefault();
            var submittingElement = $(event.target);
            // first search the form where the submitting element is in.
            var form = submittingElement.parents('form:first');
            // If not found, take the first form inside the modal
            if (form.length === 0) form = $(this).parents('.modal').find('form:first');
            if (form.length === 0) {
                JL(loggerName).error({
                    'msg': 'No form found'
                });
            }
            // form.valid() is not defined without jquery validation
            if (form.valid && !form.valid()) { 
                // jquery (unobtrusive) validation failed
                return;
            }
            
            $('.modal-footer button, .modal-footer input[type="button"], .modal-footer input[type="submit"]').attr('disabled', 'disabled').css('cursor', 'not-allowed'); // disable all buttons in footer
            showLoading(submittingElement, true);

            var actionUrl = form.attr('action');
            var dataToSend = form.serialize();
            var method = form.attr('method');

            $.ajax(actionUrl,
                {
                    'async': true, // important for 'always' to be called
                    'data': dataToSend,
                    headers: {
                        'content-type': 'application/x-www-form-urlencoded'  // = default; application/json would trigger another ModelBinder! 
                    },
                    'method': method,
                    'timeout': 10000
                })
                .done(function (data, textStatus, jqXHR) {
                    // server returns JSON with redirectUrl value
                    if (isJson(data)) {
                        if ($.isEmptyObject(data)) {
                            JL(loggerName).warn({ 'msg': 'JSON result is empty' });
                        } else {
                            if (data.redirectUrl) { window.location.href = data.redirectUrl; }
                            else { JL(loggerName).warn({ 'msg': 'JSON redirectUrl is empty' }); }
                        }
                    }
                    // Server should return a PARTIAL view with the form after server side validation.
                    // A full page view (identified by BODY element, e.g. caused by a bad action url) would mess up the browser
                    if ($.type(data) === 'string' && data.match(/<body[^>]*>/gi)) {
                        JL(loggerName).error({
                            'msg': 'AJAX response is not a partial view',
                            'url': actionUrl
                        });
                        // The modal container is still visible, and only the modal-dialog part will be replaced:
                        modalContainer.find('.modal-dialog').replaceWith(fillTemplate(modalDialogTemplate, $.i18n('modal-form-error-occurred'), ''));
                        return;
                    }

                    // extract the div containing the form
                    var newModalDialog = $('.modal-dialog', data);
                    // replace body with partial view body from server
                    modalContainer.find('.modal-dialog').replaceWith(newModalDialog);
                    
                    // requires <input name="IsValid" type="hidden" value="@ViewData.ModelState.IsValid.ToString()" />
                    var isValid = newModalDialog.find('[name="IsValid"]').val() === 'true';
                    // hide the modal if the form was processed without model errors
                    if (isValid) {
                        modalContainer.find('.modal').modal('hide').modal('dispose');
                    } else {
                        $('input[autofocus]:first').trigger('focus');
                    }
                })
                .fail(function (jqXHR, textStatus, errorThrown) {
                    JL(loggerName).error({
                        'msg': 'AJAX error response',
                        'errorThrown': errorThrown,
                        'url': actionUrl,
                        'requestData': dataToSend
                    });
                    // The modal container is still visible, and only the modal-dialog part will be replaced with the error message:
                    modalContainer.find('.modal-dialog').replaceWith(fillTemplate(modalDialogTemplate, $.i18n('modal-form-error-occurred'), textStatus === 'timeout' ? 'Timeout' : jqXHR.status));
                })
                .always(function (data, textStatus, jqXHR) { /* parameters in case of error: jqXHR, textStatus, errorThrown */
                    $('.modal-footer button, .modal-footer input[type="button"], .modal-footer input[type="submit"]').removeAttr('disabled').css('cursor', ''); // re-enable all footer buttons
                    showLoading(submittingElement, false);
                });
        });

    function isJson(m) {

        if (typeof m === 'object') {
            try {
                m = JSON.stringify(m);
            } catch (err) {
                return false;
            }
        }

        if (typeof m === 'string') {
            try {
                m = JSON.parse(m);
            } catch (err) {
                return false;
            }
        }

        if (typeof m !== 'object') {
            return false;
        }
        return true;
    }

};
Site.ModalForm(window.jQuery);
