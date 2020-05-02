// All scripts go into the same namespace
if (Site === undefined) {
    var Site = {};
}

/*
Make characters of an <input type="password"/> field visible/invisible. Designed for Bootstrap 4.
Bound click events get lost after jquery validation, so we call this function with "onclick=Site.ShowHidePassword($(this))"

Example:
<div class="form-group">
    <label asp-for="Password"></label>
==> <div class="input-group">
==>     <input asp-for="Password" class="form-control" />
==>     <span class="input-group-append input-group-text" style="cursor: pointer" onclick="Site.ShowHidePassword($(this))">
==>         <i class="fas fa-eye"></i>
        </span>
    </div>
    <span asp-validation-for="Password" class="text-danger"></span>
</div>
*/
Site.ShowHidePassword = function (eyeButton) {
    if (eyeButton.find('.fas').hasClass('fa-eye')) {
        eyeButton.find('.fas').removeClass('fa-eye').addClass('fa-eye-slash');
        eyeButton.parent().find('input').attr('type', 'text');
    } else {
        eyeButton.find('.fas').removeClass('fa-eye-slash').addClass('fa-eye');
        eyeButton.parent().find('input').attr('type', 'password');
    }
};
