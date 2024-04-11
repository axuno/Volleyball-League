// All scripts go into the same namespace
if (typeof Site === 'undefined') {
    var Site = {};
}
/*
Make characters of an <input type="password"/> field visible/invisible.

Example:
<div class="form-group">
    <label asp-for="Password" class="form-label"></label>
==> <div class="input-group">
==>     <input asp-for="Password" class="form-control" />
==>     <span class="input-group-append input-group-text" style="cursor: pointer" onclick="Site.ShowHidePassword(this)">
==>         <i class="fas fa-eye"></i>
        </span>
    </div>
    <span asp-validation-for="Password" class="text-danger"></span>
</div>
*/
Site.ShowHidePassword = function(eyeButton) {
    var fas = eyeButton.querySelector('.fas');
    if (fas.classList.contains('fa-eye')) {
        fas.classList.remove('fa-eye');
        fas.classList.add('fa-eye-slash');
        eyeButton.parentNode.querySelector('input').setAttribute('type', 'text');
    } else {
        fas.classList.remove('fa-eye-slash');
        fas.classList.add('fa-eye');
        eyeButton.parentNode.querySelector('input').setAttribute('type', 'password');
    }
};

