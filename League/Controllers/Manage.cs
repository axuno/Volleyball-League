using Axuno.Web;
using League.BackgroundTasks;
using League.Emailing.Creators;
using League.Identity;
using League.Helpers;
using League.Models.ManageViewModels;
using League.TagHelpers;
using League.Views;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DI;
using TournamentManager.MultiTenancy;
using League.MultiTenancy;
using League.Routing;

namespace League.Controllers;

[Authorize]
[Route(TenantRouteConstraint.Template + "/[controller]")]
public class Manage : AbstractController
{
    private const string UserNotFoundInRepository = "Username '{UserName}' not found in repository";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IStringLocalizer<Manage> _localizer;
    private readonly Axuno.Tools.DateAndTime.TimeZoneConverter _timeZoneConverter;
    private readonly Axuno.BackgroundTask.IBackgroundQueue _queue;
    private readonly SendEmailTask _sendEmailTask;
    private readonly IOptions<DataProtectionTokenProviderOptions> _dataProtectionTokenProviderOptions;
    private readonly MetaDataHelper _metaData;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<Manage> _logger;
    private readonly PhoneNumberService _phoneNumberService;
    private readonly RegionInfo _regionInfo;

    public Manage(
        SignInManager<ApplicationUser> signInManager, IStringLocalizer<Manage> localizer,
        Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter,
        Axuno.BackgroundTask.IBackgroundQueue queue,
        SendEmailTask sendEmailTask,
        IOptions<DataProtectionTokenProviderOptions> dataProtectionTokenProviderOptions,
        MetaDataHelper metaData, ITenantContext tenantContext, 
        RegionInfo regionInfo, PhoneNumberService phoneNumberService,
        ILogger<Manage> logger)
    {
        _userManager = signInManager.UserManager;
        _signInManager = signInManager;
        _localizer = localizer;
        _timeZoneConverter = timeZoneConverter;
        _queue = queue;
        _sendEmailTask = sendEmailTask;
        _dataProtectionTokenProviderOptions = dataProtectionTokenProviderOptions;
        _metaData = metaData;
        _tenantContext = tenantContext;
        _phoneNumberService = phoneNumberService;
        _regionInfo = regionInfo;
        _logger = logger;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var ue = new TournamentManager.DAL.EntityClasses.UserEntity();

        ue.Fields[(int) TournamentManager.DAL.UserFieldIndex.UserName].Alias = _metaData.GetDisplayName<ChangeUsernameViewModel>(nameof(ChangeUsernameViewModel.Username));

        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return Redirect(TenantLink.Action(nameof(Account.SignIn), nameof(Account))!);
        }
        
        var model = new IndexViewModel(_timeZoneConverter)
        {
            ApplicationUser = user,
            HasPassword = await _userManager.HasPasswordAsync(user),
            IsTeamManager = User.Claims.Any(c => c.Type.Equals(Constants.ClaimType.ManagesTeam, StringComparison.InvariantCultureIgnoreCase)),
            Logins = await _userManager.GetLoginsAsync(user),
            ManageMessage = TempData.Get<ManageMessage>(nameof(ManageMessage))
        };
        // Display phone numbers in the format of the current region
        model.ApplicationUser.PhoneNumber = _phoneNumberService.Format(model.ApplicationUser.PhoneNumber ?? string.Empty,
            _regionInfo.TwoLetterISORegionName);
        model.ApplicationUser.PhoneNumber2 = _phoneNumberService.Format(model.ApplicationUser.PhoneNumber2,
            _regionInfo.TwoLetterISORegionName);
        
        return View(model);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> ChangeUserName()
    {
        var model = new ChangeUsernameViewModel();
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            _logger.LogError(UserNotFoundInRepository, HttpContext.User.Identity?.Name);
            ModelState.AddModelError(string.Empty, _localizer["'{0}' not found", _metaData.GetDisplayName<ChangeUsernameViewModel>(nameof(ChangeUsernameViewModel.Username)) ?? string.Empty]);
            return PartialView(ViewNames.Manage._ChangeUsernameModalPartial, model);
        }

        model.Username = user.Name;
        return PartialView(ViewNames.Manage._ChangeUsernameModalPartial, model);
    }

    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeUserName(ChangeUsernameViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return PartialView(ViewNames.Manage._ChangeUsernameModalPartial, model);
        }
        var user = await GetCurrentUserAsync();
        if (user != null)
        {
            if (user.NormalizedUserName == _userManager.KeyNormalizer.NormalizeName(model.Username))
            {
                // do nothing and display success
                TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Success, MessageId = MessageId.ChangeUsernameSuccess });
                return JsonResponseRedirect(TenantLink.Action(nameof(Index), nameof(Manage)));
            }
            user.UserName = model.Username ?? string.Empty;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation("User Id '{UserId}' changed the username successfully.", user.Id);
                TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Success, MessageId = MessageId.ChangeUsernameSuccess });
                return JsonResponseRedirect(TenantLink.Action(nameof(Index), nameof(Manage)));
            }
            AddErrors(result);
            
            return PartialView(ViewNames.Manage._ChangeUsernameModalPartial, model);
        }

        TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Danger, MessageId = MessageId.ChangeUsernameFailure });
        return JsonResponseRedirect(TenantLink.Action(nameof(Index), nameof(Manage)));
    }

        

    [HttpGet("[action]")]
    public async Task<IActionResult> ChangeEmail()
    {
        var model = new ChangeEmailViewModel();
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            _logger.LogError("User id '{UserId}' not found in repository", GetCurrentUserId());
            ModelState.AddModelError(string.Empty, _localizer["'{0}' not found", _metaData.GetDisplayName<ChangeEmailViewModel>(nameof(ChangeEmailViewModel.Email)) ?? string.Empty]);
            return PartialView(ViewNames.Manage._ChangeEmailModalPartial, model);
        }

        model.Email = user.Email;
        return PartialView(ViewNames.Manage._ChangeEmailModalPartial, model);
    }
 
    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return PartialView(ViewNames.Manage._ChangeEmailModalPartial, model);
        }

        var user = await GetCurrentUserAsync();

        if (user == null)
        {
            TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Danger, MessageId = MessageId.ChangeEmailFailure });
            return JsonResponseRedirect(TenantLink.Action(nameof(Index), nameof(Manage)));
        }

        if (user.NormalizedEmail == _userManager.KeyNormalizer.NormalizeEmail(model.Email))
        {
            _logger.LogInformation("Current and new email are equal ('{Email}').", model.Email);
            ModelState.AddModelError(nameof(ChangeEmailViewModel.Email), _localizer["Current and new email must be different"]);
            return PartialView(ViewNames.Manage._ChangeEmailModalPartial, model);
        }

        if (model.Email != null) await SendEmail(user, model.Email);

        TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Success, MessageId = MessageId.ChangeEmailConfirmationSent });
        return JsonResponseRedirect(TenantLink.Action(nameof(Index), nameof(Manage)));
    }

    [AllowAnonymous]
    [HttpGet("[action]/{id}")]
    public async Task<IActionResult> ConfirmNewPrimaryEmail(string id, string code, string e)
    {
        if (!ModelState.IsValid) return BadRequest();

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Danger, MessageId = MessageId.ChangeEmailFailure });
            return Redirect(TenantLink.Action(nameof(Index), nameof(Manage))!);
        }

        // ChangeEmailAsync also sets EmailConfirmed = true
        var result = await _userManager.ChangeEmailAsync(user, e.Base64UrlDecode()!, code.Base64UrlDecode()!);
        if (result != IdentityResult.Success)
        {
            TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Danger, MessageId = MessageId.ChangeEmailFailure });
            return Redirect(TenantLink.Action(nameof(Index), nameof(Manage))!);
        }

        TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Success, MessageId = MessageId.ChangeEmailSuccess });
        return Redirect(TenantLink.Action(nameof(Index), nameof(Manage))!);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> EditEmail2()
    {
        var model = new EditEmail2ViewModel();
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            _logger.LogError("User id '{USserId}' not found in repository", GetCurrentUserId());
            ModelState.AddModelError(string.Empty, _localizer["'{0}' not found", _metaData.GetDisplayName<EditEmail2ViewModel>(nameof(EditEmail2ViewModel.Email2)) ?? string.Empty]);
            return PartialView(ViewNames.Manage._EditEmail2ModalPartial, model);
        }

        model.Email2 = user.Email2;
        return PartialView(ViewNames.Manage._EditEmail2ModalPartial, model);
    }

    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEmail2(EditEmail2ViewModel model, CancellationToken cancellationToken)
    {
        // Empty field is used to remove data
        if (string.IsNullOrEmpty(model.Email2))
        {
            ModelState.Clear();
        }

        if (!ModelState.IsValid)
        {
            return PartialView(ViewNames.Manage._EditEmail2ModalPartial, model);
        }

        var userEntity = await _tenantContext.DbContext.AppDb.UserRepository.GetLoginUserByUserNameAsync(User.Identity?.Name ?? string.Empty, cancellationToken);
        if (userEntity == null)
        {
            TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Danger, MessageId = MessageId.ChangeEmail2Failure });
            return JsonResponseRedirect(TenantLink.Action(nameof(Index), nameof(Manage)));
        }

        if (_userManager.KeyNormalizer.NormalizeEmail(userEntity.Email2) == _userManager.KeyNormalizer.NormalizeEmail(model.Email2))
        {
            // do nothing and display success
            TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Success, MessageId = MessageId.ChangeEmail2Success });
            return JsonResponseRedirect(TenantLink.Action(nameof(Index), nameof(Manage)));
        }

        if (_userManager.KeyNormalizer.NormalizeEmail(userEntity.Email) == _userManager.KeyNormalizer.NormalizeEmail(model.Email2))
        {
            _logger.LogInformation("Primary and additional email are equal ('{UserEmail}').", userEntity.Email);
            ModelState.AddModelError(nameof(EditEmail2ViewModel.Email2), _localizer["'{0}' and '{1}' must be different", _metaData.GetDisplayName<EditEmail2ViewModel>(nameof(EditEmail2ViewModel.Email2)) ?? string.Empty, _metaData.GetDisplayName<ChangeEmailViewModel>(nameof(ChangeEmailViewModel.Email)) ?? string.Empty]);
            return PartialView(ViewNames.Manage._EditEmail2ModalPartial, model);
        }

        userEntity.Email2 = model.Email2 ?? string.Empty;
        try
        {
            await _tenantContext.DbContext.AppDb.GenericRepository.SaveEntityAsync(userEntity, false, false, cancellationToken);
            TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Success, MessageId = MessageId.ChangeEmail2Success });
            return JsonResponseRedirect(TenantLink.Action(nameof(Index), nameof(Manage)));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Save user name '{UserName}' failed", userEntity.UserName);
            return PartialView(ViewNames.Manage._EditEmail2ModalPartial, model);
        }
    }

    [HttpGet("[action]")]
    public IActionResult ChangePassword()
    {
        return PartialView(ViewNames.Manage._ChangePasswordModalPartial);
    }

    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return PartialView(ViewNames.Manage._ChangePasswordModalPartial, model);
        }
        var user = await GetCurrentUserAsync();
        if (user != null && model is { CurrentPassword: not null, NewPassword: not null })
        {
            if (!await _userManager.CheckPasswordAsync(user, model.CurrentPassword))
            {
                // Tell the user, which password field is invalid
                ModelState.AddModelError(nameof(ChangePasswordViewModel.CurrentPassword), _userManager.ErrorDescriber.PasswordMismatch().Description);
                return PartialView(ViewNames.Manage._ChangePasswordModalPartial, model);
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation("User Id '{UserId}' changed the password successfully.", user.Id);
                TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Success, MessageId = MessageId.ChangePasswordSuccess });
                return JsonResponseRedirect(TenantLink.Action(nameof(Index), nameof(Manage)));
            }
            AddErrors(result);
            return PartialView(ViewNames.Manage._ChangePasswordModalPartial, model);
        }

        TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Danger, MessageId = MessageId.ChangePasswordFailure });
        return JsonResponseRedirect(TenantLink.Action(nameof(Index), nameof(Manage)));
    }

    [HttpGet("[action]")]
    public IActionResult SetPassword()
    {
        return PartialView(ViewNames.Manage._SetPasswordModalPartial);
    }

    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return PartialView(ViewNames.Manage._SetPasswordModalPartial, model);
        }

        var user = await GetCurrentUserAsync();
        if (user != null && model.NewPassword != null)
        {
            var result = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Success, MessageId = MessageId.SetPasswordSuccess });
                return JsonResponseRedirect(TenantLink.Action(nameof(Index), nameof(Manage)));
            }
            AddErrors(result);
            return PartialView(ViewNames.Manage._SetPasswordModalPartial, model);
        }

        TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Danger, MessageId = MessageId.SetPasswordFailure });
        return JsonResponseRedirect(TenantLink.Action(nameof(Index), nameof(Manage)));
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> EditPersonalDetails(CancellationToken cancellationToken)
    {
        var model = new PersonalDetailsViewModel();
        var user = await _tenantContext.DbContext.AppDb.UserRepository.GetLoginUserByUserNameAsync(HttpContext.User.Identity?.Name ?? string.Empty, cancellationToken);
        if (user == null)
        {
            _logger.LogError("User id '{UserId}' not found in repository", GetCurrentUserId());
            TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Danger, MessageId = MessageId.ChangePersonalDetailsFailure });
            ModelState.AddModelError(string.Empty, _localizer["Personal details not found"]);
            return PartialView(ViewNames.Manage._EditPersonalDetailsModalPartial, model);
        }

        model.Gender = user.Gender;
        model.FirstName = user.FirstName;
        model.LastName = user.LastName;
        model.Nickname = user.Nickname;

        return PartialView(ViewNames.Manage._EditPersonalDetailsModalPartial, model);
    }

    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPersonalDetails(PersonalDetailsViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return PartialView(ViewNames.Manage._EditPersonalDetailsModalPartial, model);
        }

        var user = await _tenantContext.DbContext.AppDb.UserRepository.GetLoginUserByUserNameAsync(HttpContext.User.Identity?.Name ?? string.Empty, cancellationToken);
        if (user == null)
        {
            _logger.LogError(UserNotFoundInRepository, HttpContext.User.Identity?.Name ?? string.Empty);
            TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Danger, MessageId = MessageId.ChangePersonalDetailsFailure });
            return JsonResponseRedirect(TenantLink.Action(nameof(Index), nameof(Manage)));
        }

        user.Gender = model.Gender;
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Nickname = model.Nickname ?? string.Empty;
        try
        {
            await _tenantContext.DbContext.AppDb.GenericRepository.SaveEntityAsync(user, false, false, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failure saving personal data for user id '{UserId}'", GetCurrentUserId());
            TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Danger, MessageId = MessageId.ChangePersonalDetailsFailure });
            return JsonResponseRedirect(TenantLink.Action(nameof(Index), nameof(Manage)));
        }

        _logger.LogInformation("Personal data for user id '{UserId}' updated", GetCurrentUserId());
        TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Success, MessageId = MessageId.ChangePersonalDetailsSuccess });
        return JsonResponseRedirect(TenantLink.Action(nameof(Index), nameof(Manage)));
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> EditPhoneNumber(CancellationToken cancellationToken)
    {
        var model = new EditPhoneViewModel();
        var user = await _tenantContext.DbContext.AppDb.UserRepository.GetLoginUserByUserNameAsync(HttpContext.User.Identity?.Name ?? string.Empty, cancellationToken);
        if (user == null)
        {
            _logger.LogError(UserNotFoundInRepository, HttpContext.User.Identity?.Name ?? string.Empty);
            TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Danger, MessageId = MessageId.ChangePhoneFailure });
            ModelState.AddModelError(string.Empty, _localizer["Primary phone number not found"]);
            return PartialView(ViewNames.Manage._EditPhoneModalPartial, model);
        }

        model.PhoneNumber = _phoneNumberService.Format(user.PhoneNumber, _regionInfo.TwoLetterISORegionName);
        return PartialView(ViewNames.Manage._EditPhoneModalPartial, model);
    }

    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPhoneNumber(EditPhoneViewModel model, CancellationToken cancellationToken)
    {
        UserEntity? userEntity;

        async Task<IActionResult> Save()
        {
            // Save in unformatted, international format
            userEntity.PhoneNumber = _phoneNumberService.FormatForStorage(model.PhoneNumber, _regionInfo.TwoLetterISORegionName);

            try
            {
                await _tenantContext.DbContext.AppDb.GenericRepository.SaveEntityAsync(userEntity, false, false, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Save user name '{UserName}' failed", userEntity.UserName);
                TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Danger, MessageId = MessageId.ChangePhoneFailure });
                return JsonResponseRedirect(TenantLink.Action(nameof(Index), nameof(Manage)));
            }

            TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Success, MessageId = MessageId.ChangePhoneSuccess });
            return JsonResponseRedirect(TenantLink.Action(nameof(Index), nameof(Manage)));
        }

        if (!ModelState.IsValid)
        {
            return PartialView(ViewNames.Manage._EditPhoneModalPartial, model);
        }
        ModelState.Clear();

        userEntity = await _tenantContext.DbContext.AppDb.UserRepository.GetLoginUserByUserNameAsync(User.Identity?.Name ?? string.Empty, cancellationToken);

        if (userEntity == null)
        {
            TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Danger, MessageId = MessageId.ChangePhoneFailure });
            return JsonResponseRedirect(TenantLink.Action(nameof(Index), nameof(Manage)));
        }

        // Remove the phone number
        if (string.IsNullOrEmpty(model.PhoneNumber))
        {
            model.PhoneNumber = string.Empty;
            return await Save();
        }

        if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
        {
            var validator = new TournamentManager.ModelValidators.PhoneNumberValidator(model.PhoneNumber, (_phoneNumberService, _regionInfo));
            await validator.CheckAsync(cancellationToken);
            validator.GetFailedFacts().ForEach(f => ModelState.AddModelError(nameof(model.PhoneNumber), f.Message));
        }

        if (!ModelState.IsValid)
        {
            return PartialView(ViewNames.Manage._EditPhoneModalPartial, model);
        }

        if (_phoneNumberService.IsMatch(userEntity.PhoneNumber, model.PhoneNumber, _regionInfo.TwoLetterISORegionName))
        {
            _logger.LogInformation("Current and new primary phone number are equal ('{PhoneNumber}').", userEntity.PhoneNumber);
            ModelState.AddModelError(nameof(EditPhoneViewModel.PhoneNumber), _localizer["Current and new primary phone number must be different"]);
            return PartialView(ViewNames.Manage._EditPhoneModalPartial, model);
        }

        if (_phoneNumberService.IsMatch(userEntity.PhoneNumber2, model.PhoneNumber, _regionInfo.TwoLetterISORegionName))
        {
            _logger.LogInformation("Primary and additional phone number are equal ('{PhoneNumber}').", userEntity.PhoneNumber);
            ModelState.AddModelError(nameof(EditPhone2ViewModel.PhoneNumber2), _localizer["'{0}' and '{1}' must be different", _metaData.GetDisplayName<EditPhoneViewModel>(nameof(EditPhoneViewModel.PhoneNumber)) ?? string.Empty, _metaData.GetDisplayName<EditPhone2ViewModel>(nameof(EditPhone2ViewModel.PhoneNumber2)) ?? string.Empty]);
            return PartialView(ViewNames.Manage._EditPhoneModalPartial, model);
        }

        return await Save();
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> EditPhoneNumber2(CancellationToken cancellationToken)
    {
        var model = new EditPhone2ViewModel();
        var user = await _tenantContext.DbContext.AppDb.UserRepository.GetLoginUserByUserNameAsync(HttpContext.User.Identity?.Name ?? string.Empty, cancellationToken);
        if (user == null)
        {
            _logger.LogError(UserNotFoundInRepository, HttpContext.User.Identity?.Name ?? string.Empty);
            TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Danger, MessageId = MessageId.ChangePhone2Failure });
            ModelState.AddModelError(string.Empty, _localizer["'{0}' not found", _metaData.GetDisplayName<EditPhone2ViewModel>(nameof(EditPhone2ViewModel.PhoneNumber2)) ?? string.Empty]);
            return PartialView(ViewNames.Manage._EditPhone2ModalPartial, model);
        }

        model.PhoneNumber2 = _phoneNumberService.Format(user.PhoneNumber2, _regionInfo.TwoLetterISORegionName);
        return PartialView(ViewNames.Manage._EditPhone2ModalPartial, model);
    }

    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPhoneNumber2(EditPhone2ViewModel model, CancellationToken cancellationToken)
    {
        UserEntity? userEntity;

        async Task<IActionResult> Save()
        {
            // Save in unformatted, international format
            userEntity.PhoneNumber2 = _phoneNumberService.FormatForStorage(model.PhoneNumber2, _regionInfo.TwoLetterISORegionName);
            try
            {
                await _tenantContext.DbContext.AppDb.GenericRepository.SaveEntityAsync(userEntity, false, false, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Save user name '{UserName}' failed", userEntity.UserName);
                TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Danger, MessageId = MessageId.ChangePhone2Failure });
                return JsonResponseRedirect(TenantLink.Action(nameof(Index), nameof(Manage)));
            }

            TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Success, MessageId = MessageId.ChangePhone2Success });
            return JsonResponseRedirect(TenantLink.Action(nameof(Index), nameof(Manage)));
        }

        if (!ModelState.IsValid)
        {
            return PartialView(ViewNames.Manage._EditPhone2ModalPartial, model);
        }
        ModelState.Clear();

        userEntity = await _tenantContext.DbContext.AppDb.UserRepository.GetLoginUserByUserNameAsync(User.Identity?.Name ?? string.Empty, cancellationToken);

        if (userEntity == null)
        {
            TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Danger, MessageId = MessageId.ChangePhone2Failure });
            return JsonResponseRedirect(TenantLink.Action(nameof(Index), nameof(Manage)));
        }

        // Remove the phone number
        if (string.IsNullOrEmpty(model.PhoneNumber2))
        {
            model.PhoneNumber2 = string.Empty;
            return await Save();
        }
            
        if (!string.IsNullOrWhiteSpace(model.PhoneNumber2))
        {
            var validator = new TournamentManager.ModelValidators.PhoneNumberValidator(model.PhoneNumber2, (_phoneNumberService, _regionInfo));
            await validator.CheckAsync(cancellationToken);
            validator.GetFailedFacts().ForEach(f => ModelState.AddModelError(nameof(model.PhoneNumber2), f.Message));
        }

        if (!ModelState.IsValid)
        {
            return PartialView(ViewNames.Manage._EditPhone2ModalPartial, model);
        }

        if (_phoneNumberService.IsMatch(userEntity.PhoneNumber, model.PhoneNumber2, _regionInfo.TwoLetterISORegionName))
        {
            _logger.LogInformation("Primary and additional phone number are equal ('{PhoneNumber}').", userEntity.PhoneNumber);
            ModelState.AddModelError(nameof(EditPhone2ViewModel.PhoneNumber2), _localizer["'{0}' and '{1}' must be different", _metaData.GetDisplayName<EditPhone2ViewModel>(nameof(EditPhone2ViewModel.PhoneNumber2)) ?? string.Empty, _metaData.GetDisplayName<EditPhoneViewModel>(nameof(EditPhoneViewModel.PhoneNumber)) ?? string.Empty]);
            return PartialView(ViewNames.Manage._EditPhone2ModalPartial, model);
        }

        return await Save();
    }

    //GET: /Manage/ManageLogins
    [HttpGet("[action]")]
    public async Task<IActionResult> ManageLogins()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Danger, MessageId = MessageId.ManageLoginFailure });
            return JsonResponseRedirect(TenantLink.Action(nameof(Index), nameof(Manage)));
        }
        var userLogins = await _userManager.GetLoginsAsync(user);
        var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
        var otherLogins = schemes.Where(auth => userLogins.All(ul => auth.Name != ul.LoginProvider)).ToList();
        return PartialView(ViewNames.Manage._ManageLoginsModalPartial, new ManageLoginsViewModel
        {
            CurrentLogins = userLogins,
            ShowRemoveButton = !string.IsNullOrEmpty(user.PasswordHash) || userLogins.Count > 1,
            OtherLogins = otherLogins
        });
    }

    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveLogin(RemoveLoginViewModel account)
    {
        var user = await GetCurrentUserAsync();
        if (ModelState.IsValid && user != null)
        {
            var result = await _userManager.RemoveLoginAsync(user, account.LoginProvider, account.ProviderKey);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Success, MessageId = MessageId.RemoveLoginSuccess });
                return JsonResponseRedirect(TenantLink.Action(nameof(Index), nameof(Manage)));
            }
        }

        TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Danger, MessageId = MessageId.RemoveLoginFailure });
        return JsonResponseRedirect(TenantLink.Action(nameof(Index), nameof(Manage)));
    }


    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public IActionResult LinkLogin(string provider)
    {
        if (!ModelState.IsValid) return BadRequest();

        // Request a redirect to the external login provider to link a login for the current user
        var redirectUrl = TenantLink.Action(nameof(LinkLoginCallback), nameof(Manage));
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
        return Challenge(properties, provider);
    }

    [HttpGet("[action]")]
    public async Task<ActionResult> LinkLoginCallback()
    {
        var user = await GetCurrentUserAsync();
        if (!ModelState.IsValid || user == null)
        {
            TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Danger, MessageId = MessageId.AddLoginFailure });
            return Redirect(GeneralLink.GetUriByAction(HttpContext, nameof(Index))!);
        }
        var info = await _signInManager.GetExternalLoginInfoAsync(await _userManager.GetUserIdAsync(user));
        if (info == null)
        {
            TempData.Put<ManageMessage>(nameof(ManageMessage), new() { AlertType = SiteAlertTagHelper.AlertType.Danger, MessageId = MessageId.AddLoginFailure });
            return Redirect(TenantLink.Action(nameof(Index), nameof(Manage))!);
        }
        var result = await _userManager.AddLoginAsync(user, info);
        TempData.Put<ManageMessage>(nameof(ManageMessage), result.Succeeded
            ? new() {AlertType = SiteAlertTagHelper.AlertType.Success, MessageId = MessageId.AddLoginSuccess}
            : new ManageMessage
                {AlertType = SiteAlertTagHelper.AlertType.Danger, MessageId = MessageId.AddLoginFailure});

        return Redirect(TenantLink.Action(nameof(Index),nameof(Manage))!);
    }

    [HttpGet("[action]")]
    public ActionResult DeleteAccount()
    {
        return PartialView(ViewNames.Manage._DeleteAccountModalPartial);
    }

    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> DeleteAccountConfirmed()
    {
        var user = await GetCurrentUserAsync();
        if (user != null)
        {
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError("Account for user id '{UserId}' could not be deleted.", user.Id);
            }
        }
        await _signInManager.SignOutAsync();
        return JsonResponseRedirect("/");
    }

    #region *** Helpers ***

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
 
    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        return await _userManager.GetUserAsync(HttpContext.User);
    }

    /// <summary>
    /// Sends mail messages to current and new email address
    /// </summary>
    /// <param name="user">The <see cref="ApplicationUser"/> as the recipient of the notification email.</param>
    /// <param name="newEmail">The new email address for the user that must be confirmed.</param>
    private async Task SendEmail(ApplicationUser user, string newEmail)
    {
        var deadline = DateTime.UtcNow.Add(_dataProtectionTokenProviderOptions.Value.TokenLifespan);
        // round down to full hours
        deadline = new(deadline.Year, deadline.Month, deadline.Day, deadline.Hour, 0, 0, DateTimeKind.Utc);
        var code = (await _userManager.GenerateChangeEmailTokenAsync(user, newEmail)).Base64UrlEncode();

        _sendEmailTask.SetMessageCreator(new ChangePrimaryUserEmailCreator
        {
            Parameters =
            {
                Email = user.Email,
                NewEmail = newEmail,
                CallbackUrl = TenantLink.ActionLink(nameof(ConfirmNewPrimaryEmail), nameof(Manage), new {id = user.Id, code, e = newEmail.Base64UrlEncode()}, scheme: HttpContext.Request.Scheme),
                DeadlineUtc = deadline,
                CultureInfo = CultureInfo.CurrentUICulture,
            }
        });
 
        _queue.QueueTask(_sendEmailTask);
    }
    #endregion
}
