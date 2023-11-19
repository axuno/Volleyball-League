// All scripts go into the same namespace
if (Site === undefined) {
    var Site = {};
}

/* Handling of forms inside Bootstrap 5 modals */
Site.ModalForm = function () {
    const loggerName = 'JsLogger.Site.ModalForm';

    // language setting is taken from <html lang="iso-code">
    const translations = {
        'en': {
            'modal-form-error-occurred': 'Oops... an error occurred. Please try again.'
        },
        'de': {
            'modal-form-error-occurred': 'Ups... ein Fehler ist aufgetreten. Bitte nochmals versuchen.'
        }
    };
    function getLocalized(key) {
        const locale = document.documentElement.lang;
        const fbLocale = locale.split('-')[0];
        if (translations[locale]) {
            return translations[locale][key];
        } else if (translations[fbLocale]) {
            return translations[fbLocale][key];
        } else if (translations['en']) {
            return translations['en'][key];
        }
    }

    let submittingElement;

    // create a dynamic DIV element and insert it as first child of BODY
    const modalContainer = document.createElement('div');
    modalContainer.id = 'modal-container-' + Math.random().toString(36).substring(2, 16);
    document.body.insertAdjacentElement('afterbegin', modalContainer);

    // see also: modalFullTemplate
    const modalDialogTemplate = `<div class="modal-dialog">
	<div class="modal-content">
		<div class="modal-header">
			<i class="fas fa-2x fa-bomb text-danger"></i>
			<button type="button" class="btn btn-close" data-bs-dismiss="modal"></button>
		</div>
		<div class="modal-body">
			<div class="text-danger"><span class="h4 text-danger">$0</span> $1</div>
		</div>
	</div>
</div>`;

    // see also: modalDialogTemplate
    //data-keyboard=true allows to close the modal with ESC key
    const modalFullTemplate = `<div class="modal" data-bs-keyboard="true" tabindex="-1">
    ${modalDialogTemplate}
</div>`;

    const fillErrorTemplate = function (template, errorText, errorNo) {
        return template.replace('$0', errorText).replace('$1', errorNo);
    };

    const showLoading = function(btnElement, isOn) {
        if (!btnElement.classList.contains('btn')) {
            // It's not a bootstrap 4 element displayed as a button
            return;
        }
        if (isOn === true) {
            btnElement.insertAdjacentHTML('afterbegin', '<span id="site-loading-icon" class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>');
        } else {
            btnElement.querySelector('#site-loading-icon').remove();
        }
    };

    /**
     * Fetches a url using a timeout
     * https://dmitripavlutin.com/timeout-fetch-request/
     * @param {string} url - The Url to fetch
     * @param {any} options - The RequestInit options
     * @returns {Response} - The server response
     * @throws {AbortError} on timeout
     */
    async function fetchWithTimeout(url = '', options = {}) {
        const timeout = options.timeout || 5000;

        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), timeout);

        const response = await fetch(url, {
            ...options,
            signal: controller.signal
        });
        clearTimeout(timeoutId);

        if (response.status >= 200 && response.status < 300) {
            return response;
        } else {
            throw new Error(response.statusText)
        }
    }
    /**
     * 
     * @param {any} url - The RequestInfo | Url for the Post
     * @param {any} data - The form data to post
     * @param {any} options - The RequestInit options
     * @returns {Response} - The server response
     * @throws {AbortError} on timeout
     */
    async function postWithTimeout(url = '', data = {}, options = {}) {
        const timeout = options.timeout || 5000;
        
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), timeout);

        const response = await fetch(url, {
            method: options.method,
            mode: 'cors',
            cache: 'no-cache',
            credentials: 'same-origin',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded'  // = default; application/json would trigger another ModelBinder! 
            },
            redirect: 'follow',
            referrerPolicy: 'no-referrer',
            // For form submit, this works:
            //body: data, // Object.keys(data).length !== 0 ? data : undefined,
            body: options.method && (options.method.toLowerCase() === "post") ? data : undefined,
            signal: controller.signal
        });

        clearTimeout(timeoutId);

        if (response.status >= 200 && response.status < 300) {
            return response;
        } else {
            throw new Error(response.statusText)
        }
    }

    // requires an element like <button> or <a>, containing data-toggle="site-ajax-modal"
    // e.g. <button type="button" data-toggle="site-ajax-modal" data-target="#id-in-partial-view" data-url="url-to-partial-view">do sth.</button >
    // load the partial view into the placeholder and show the modal
    document.querySelectorAll('[data-toggle="site-ajax-modal"]').forEach(item => {
        item.addEventListener('click', async event => {
            event.preventDefault();
            submittingElement = event.target;
            submittingElement.setAttribute('disabled', 'disabled');
            submittingElement.style.cursor = 'not-allowed';
            showLoading(submittingElement, true);
            // The HTMLElement.dataset property allows access, both in reading and writing mode, 
            // to all the custom data attributes (data-*) set on the element.
            await fetchModalData(item.dataset.url);
        });
    });

    function tryHandleJson(data) {
        if (isJson(data)) {
            if (Object.keys(data).length === 0) {
                JL(loggerName).warn({ 'msg': 'JSON result is empty' });
                return false;
            } else {
                if (data.redirectUrl) {
                    window.location.href = data.redirectUrl;
                } else {
                    JL(loggerName).warn({ 'msg': 'JSON redirectUrl is empty' });
                }
                return true;
            }

            return false;
        }
    }

    function ensurePartialView(data) {
        // Server should return a PARTIAL view with the form after server side validation.
        // A full page view (identified by BODY element, e.g. caused by a bad action url) would mess up the browser
        if (typeof data === 'string' && !data.match(/<body[^>]*>/gi)) {
            return true;
        }

        JL(loggerName).error({
            'msg': 'Server response is not a partial view',
            'url': actionUrl
        });
        modalContainer.innerHTML = fillErrorTemplate(modalFullTemplate, getLocalized('modal-form-error-occurred'), '');
                
        const theModal = new bootstrap.Modal(modalContainer.querySelector('.modal'), { focus: true, keyboard: true });
        theModal.show();

        return false;
    }

    /**
     * Fetch the data and show it in the modal container
     * @param {any} actionUrl
     */
    async function fetchModalData(actionUrl) {
        try {
            const response = await fetchWithTimeout(actionUrl, { method: 'GET', timeout: 5000 });
            const data = await handleResponseContentType(response);
            
            if (tryHandleJson(data)) return;
            if (!ensurePartialView(data)) return;
            setInnerHtmlWithScripts(modalContainer, data);
            modalContainer.addEventListener('shown.bs.modal', function () {
                const autofocus = document.querySelector('input[autofocus]');
                if (autofocus) autofocus.focus();
            });
            
            const theModal = new bootstrap.Modal(modalContainer.querySelector('.modal'), { focus: true, keyboard: true });
            theModal.show();
        } catch (error) {
            JL(loggerName).error({
                'msg': "Server error response",
                'errorThrown': error,
                'url': actionUrl
            });
            const errorText = error === 'timeout' || error.name === 'AbortError' ? 'Timeout' : error.status;
            modalContainer.innerHTML = fillErrorTemplate(modalFullTemplate, getLocalized('modal-form-error-occurred'), errorText);
            const theModal = new bootstrap.Modal(modalContainer.querySelector('.modal'), { focus: true, keyboard: true });
            theModal.show();
        } finally {
            submittingElement.removeAttribute('disabled');
            submittingElement.style.cursor = '';
            showLoading(submittingElement, false);
        }
    }

    // Enter key in forms with more than one input field will also trigger 'submit'
    modalContainer.addEventListener('keypress', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            modalContainer.querySelector('[type="submit"]').click();
        }
    });

    document.addEventListener('click', function (event) {
        if (event == null) return;
        submittingElement = event.target;

        if (event.target.matches('[site-data="submit"]')) {
            event.preventDefault();
                handleSiteDataSubmit();
        }
    });

    // A TagHelper creates a button <button type="submit" site-data="submit">Save</button>
    //modalContainer.querySelector('[site-data="submit"]').addEventListener('click', function(event) {
    async function handleSiteDataSubmit() {
                
        // first search the form where the submitting element is in.
        let form = submittingElement.closest('form');
        // If not found, take the first form inside the modal
        if (!(form instanceof HTMLFormElement)) {
            form = submittingElement.closest('.modal').querySelector('form');
        }
        if (!(form instanceof HTMLFormElement)) {
            // Try to access the first form in the document
            form = document.forms[0];
        }
        if (!(form instanceof HTMLFormElement)) {
            JL(loggerName).error({
                'msg': 'No form found'
            });
            return;
        }

        const elements = document.querySelectorAll('.modal-footer button, .modal-footer input[type="button"], .modal-footer input[type="submit"]');
        for (let i = 0; i < elements.length; i++) {
            elements[i].setAttribute('disabled', 'disabled');
            elements[i].style.cursor = 'not-allowed';
        } // disable elements in footer

        showLoading(submittingElement, true);

        const actionUrl = form.getAttribute('action');
        const formData = new FormData(form); // would be used for 'multipart/form-data '
        const dataToSend = new URLSearchParams(formData); // needed for 'x-www-form-urlencoded'
        const method = form.getAttribute('method');

        await postModalFormData(actionUrl, dataToSend, method);
    }

    async function postModalFormData(actionUrl, postData, method) {
        try {
            const options = { method: method };
            const response = await postWithTimeout(actionUrl, postData, options)
            const data = await handleResponseContentType(response);
            
            if (tryHandleJson(data)) return;
            if (!ensurePartialView(data)) return;
            
            // extract the div containing the form
            const tempElement = document.createElement('div'); // this is not added to the DOM
            setInnerHtmlWithScripts(tempElement, data);
            const newModalDialog = tempElement.querySelector('.modal-dialog');
            // requires <input name="IsValid" type="hidden" value="@ViewData.ModelState.IsValid.ToString()" />
            const isValid = newModalDialog.querySelector('[name="IsValid"]');
            // replace body with partial view body from server
            modalContainer.querySelector('.modal-dialog').replaceWith(newModalDialog);
            // remove the modal if the form was processed without model errors
            if (isValid && isValid.value === 'true') {
                bootstrap.Modal.getInstance('.modal').dispose();
            } else {
                const autofocus = document.querySelector('input[autofocus]');
                if (autofocus) autofocus.focus();
            }
        } catch (error) {
            JL(loggerName).error({
                'msg': 'Server error response',
                'errorThrown': error,
                'url': actionUrl,
                'requestData': postData
            });
            // The modal container is still visible, and only the modal-dialog part will be replaced with the error message:
            modalContainer.querySelector('.modal-dialog').replaceWith(fillErrorTemplate(modalDialogTemplate, getLocalized('modal-form-error-occurred'), error.name));

        } finally {
            const elements = document.querySelectorAll('.modal-footer button, .modal-footer input[type="button"], .modal-footer input[type="submit"]');
            for (let i = 0; i < elements.length; i++) {
                elements[i].removeAttribute('disabled');
                elements[i].style.cursor = '';
            } // re-enable elements in footer

            showLoading(submittingElement, false);
        }
    }
    
    async function handleResponseContentType(response) {
        
        const contentType = response.headers.get('content-type');

        if (contentType === null || contentType.startsWith('text/')) return await response.text();
        else if (contentType.startsWith('application/json;')) return await response.json();
        else throw new Error(`Unsupported response content-type: ${contentType}`);
    }

    /**
     * Sets the inner HTML of the target element in a way,
     * that code in included <script> elements will execute.
     * Background: Just setting Element.innerHTML will not activate scripts.
     * Note: The scripts will run, in the moment when
     *   * the target element already is in the DOM, or
     *   * the target will be added to the DOM e.g. with Element.appendChild(target)
     *     or Element.replaceWith(target)
     * Source: https://stackoverflow.com/questions/2592092/executing-script-elements-inserted-with-innerhtml
     * Demo: http://plnkr.co/edit/MMegiu?p=preview
     * @param {HTMLElement} elem - Target HTMLElement
     * @param {string} html - HTML string
     */
    function setInnerHtmlWithScripts(elem, html) {
        elem.innerHTML = html;
        [].forEach.call(elem.querySelectorAll('script'), oldScript => {
            const newScript = document.createElement('script');
            [].forEach.call(oldScript.attributes, attr => 
                newScript.setAttribute(attr.name, attr.value));
            
            newScript.appendChild(document.createTextNode(oldScript.innerHTML));
            oldScript.parentNode.replaceChild(newScript, oldScript);
        });
    }

    function isJson(m) {
        try {
            return (typeof m === 'object' && typeof JSON.stringify(m) === 'string');
        }
        catch {
            return false;
        }

        return true;
    }

};
Site.ModalForm();
