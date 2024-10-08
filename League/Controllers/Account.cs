using System.Security.Claims;
using Axuno.Web;
using League.BackgroundTasks;
using League.Emailing.Creators;
using League.Identity;
using League.Models.AccountViewModels;
using League.MultiTenancy;
using League.Routing;
using League.Templates.Email;
using League.Views;
using TournamentManager.ModelValidators;
using TournamentManager.MultiTenancy;

namespace League.Controllers;

[Authorize]
[Route(TenantRouteConstraint.Template + "/[controller]")]
public class Account : AbstractController
{
    #region *** Private Enums ***

    /// <summary>
    /// Messages which will be shown after the redirect from a <see cref="HttpMethods.Post"/>.
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Confirm reset email sent
        /// </summary>
        ForgotPassword,
        /// <summary>
        /// Confirm password was changed
        /// </summary>
        PasswordChanged,
        /// <summary>
        /// An email confirmation mail was sent
        /// </summary>
        PleaseConfirmEmail,
        /// <summary>
        /// The email address could not be confirmed
        /// </summary>
        ConfirmEmailError,
        /// <summary>
        /// Sign-in with the local account rejected
        /// </summary>
        SignInRejected,
        /// <summary>
        /// Sign-in rejected because the email address is not confirmed, although this is required
        /// </summary>
        SignInRejectedEmailNotConfirmed,
        /// <summary>
        /// Sign-in with a social network account failed
        /// </summary>
        ExternalSignInFailure
    }

    private enum EmailPurpose
    {
        /// <summary>
        /// Confirm the email address before creating the account
        /// </summary>
        PleaseConfirmEmail,
        /// <summary>
        /// Email to reset the password
        /// </summary>
        PasswordReset
    }

    #endregion

    #region *** Private variables **

    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IStringLocalizer<Account> _localizer;
    private readonly IOptions<DataProtectionTokenProviderOptions> _dataProtectionTokenProviderOptions;
    private readonly ILogger<Account> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly Axuno.BackgroundTask.IBackgroundQueue _queue;
    private readonly SendEmailTask _sendEmailTask;
    private readonly DataProtection.DataProtector _dataProtector;
    private readonly TournamentManager.DI.PhoneNumberService _phoneNumberService;
    private readonly RegionInfo _regionInfo;

    #endregion

    private const string ReturnUrl = nameof(ReturnUrl);
    private const string NoAccountFoundForCredentials = "No account found for these credentials.";

    public Account(
        SignInManager<ApplicationUser> signInManager,
        IStringLocalizer<Account> localizer,
        IOptions<DataProtectionTokenProviderOptions> dataProtectionTokenProviderOptions,
        ILogger<Account> logger,
        ITenantContext siteContext,
        Axuno.BackgroundTask.IBackgroundQueue queue,
        SendEmailTask sendEmailTask,
        DataProtection.DataProtector dataProtector,
        TournamentManager.DI.PhoneNumberService phoneNumberService,
        RegionInfo regionInfo)
    {
        _signInManager = signInManager;
        _localizer = localizer;
        _dataProtectionTokenProviderOptions = dataProtectionTokenProviderOptions;
        _logger = logger;
        _tenantContext = siteContext;
        _queue = queue;
        _sendEmailTask = sendEmailTask;
        _dataProtector = dataProtector;
        _phoneNumberService = phoneNumberService;
        _regionInfo = regionInfo;
    }

    [HttpGet("sign-in")]
    [AllowAnonymous]
    public IActionResult SignIn(string? returnUrl = null)
    {
        if (!ModelState.IsValid) return BadRequest();

        ViewData[ReturnUrl] = returnUrl;
        return View();
    }

    [HttpPost("sign-in")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignIn(SignInViewModel model, string? returnUrl = null)
    {
        ViewData[ReturnUrl] = returnUrl;
        if (!ModelState.IsValid) return View(model);
            
        ApplicationUser? user = null;
        if (model.EmailOrUsername.Contains('@'))
        {
            user = await _signInManager.UserManager.FindByEmailAsync(model.EmailOrUsername);
        }

        user ??= await _signInManager.UserManager.FindByNameAsync(model.EmailOrUsername);

        if (user == null)
        {
            _logger.LogInformation("No account found for '{User}'.", model.EmailOrUsername);
            ModelState.AddModelError(string.Empty, _localizer[NoAccountFoundForCredentials].Value);
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user.UserName ?? string.Empty, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("User Id '{UserId}' signed in.", user.Id);
            return RedirectToLocal(returnUrl ?? $"/{_tenantContext.SiteContext.UrlSegmentValue}");
        }

        if (result.IsLockedOut || result.IsNotAllowed)
        {
            if (!await _signInManager.UserManager.IsEmailConfirmedAsync(user) &&
                _signInManager.UserManager.Options.SignIn.RequireConfirmedEmail)
            {
                _logger.LogInformation("Sign-in not allowed: Email for user id '{UserId}' is not confirmed", user.Id);
                return Redirect(TenantLink.Action(nameof(Message), nameof(Account), new { messageTypeText = MessageType.SignInRejectedEmailNotConfirmed })!);
            }
            
            _logger.LogInformation("Account for user id '{UserId}': {Result}", user.Id, result);
            return Redirect(TenantLink.Action(nameof(Message), nameof(Account), new { messageTypeText = MessageType.SignInRejected })!);
        }

        // PasswordSignIn failed
        _logger.LogInformation("Wrong password for '{User}'.", model.EmailOrUsername);
        ModelState.AddModelError(string.Empty, _localizer[NoAccountFoundForCredentials].Value);
        return View(model);
    }

    [HttpGet("sign-out")]
    public new async Task<IActionResult> SignOut()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User signed out.");

        return RedirectToLocal("/"); // Redirects to the tenant's default page
    }

    [HttpGet("create")]
    [AllowAnonymous]
    public IActionResult CreateAccount(string? returnUrl = null)
    {
        if (!ModelState.IsValid) return BadRequest();

        ViewData[ReturnUrl] = returnUrl;
        return View(); // return the view to create a new account
    }

    [HttpPost("create")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAccount(CreateAccountViewModel model, string? returnUrl = null)
    {
        ViewData[ReturnUrl] = returnUrl;

        if (model.Captcha != HttpContext.Session.GetString(CaptchaSvgGenerator.CaptchaSessionKeyName))
        {
            ModelState.AddModelError(nameof(CreateAccountViewModel.Captcha), _localizer["Math task result was incorrect"].Value);
        }

        if (model.Email == null || await _signInManager.UserManager.FindByEmailAsync(model.Email) != null)
        {
            ModelState.AddModelError(nameof(CreateAccountViewModel.Email), _localizer["This email is not available for a new account"].Value);
        }

        if (!ModelState.IsValid && ModelState[nameof(CreateAccountViewModel.Captcha)] != null)
        {
            ModelState[nameof(CreateAccountViewModel.Captcha)]!.RawValue = string.Empty;
            return View(model);
        }

        await SendCodeByEmail(new ApplicationUser {Email = model.Email}, EmailPurpose.PleaseConfirmEmail);
        return Redirect(TenantLink.Action(nameof(Message), nameof(Account), new { messageTypeText = MessageType.PleaseConfirmEmail })!);
    }

    [HttpGet("register")]
    [AllowAnonymous]
    public IActionResult Register(string? code = null, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return BadRequest();

        if (!_dataProtector.TryDecrypt(code?.Base64UrlDecode() ?? string.Empty, out var email, out var expiration))
        {
            return Redirect(TenantLink.Action(nameof(Message), nameof(Account), new { messageTypeText = MessageType.ConfirmEmailError })!);
        }

        // Note: We check, whether the email is available only during POST
        var model = new RegisterViewModel {Email = email};

        ViewData[ReturnUrl] = returnUrl;
        return View(model); 
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken, string? returnUrl = null)
    {
        ViewData[ReturnUrl] = returnUrl;

        if (!_dataProtector.TryDecrypt(model.Code?.Base64UrlDecode() ?? string.Empty, out var email, out var expiration))
        {
            return Redirect(TenantLink.Action(nameof(Message), nameof(Account), new { messageTypeText = MessageType.ConfirmEmailError })!);
        }
        model.Email = email;
        if (await _signInManager.UserManager.FindByEmailAsync(model.Email) != null)
        {
            ModelState.AddModelError(nameof(RegisterViewModel.Email), _localizer["The email is no longer available for registration. Have you registered already?"].Value);
        }

        if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
        {
            var validator = new PhoneNumberValidator(model.PhoneNumber, (_phoneNumberService, _regionInfo));
            await validator.CheckAsync(cancellationToken);
            validator.GetFailedFacts().ForEach(f => ModelState.AddModelError(nameof(model.PhoneNumber), f.Message));
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = await _tenantContext.DbContext.AppDb.UserRepository.GenerateUniqueUsernameAsync(model.Email,
                _signInManager.UserManager.Options.User.AllowedUserNameCharacters, "User"),
            Email = model.Email,
            EmailConfirmed = true,
            Gender = model.Gender,
            FirstName = model.FirstName ?? string.Empty,
            LastName = model.LastName ?? string.Empty,
            PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber)
                ? string.Empty
                : _phoneNumberService.FormatForStorage(model.PhoneNumber, _regionInfo.TwoLetterISORegionName),
            PhoneNumberConfirmed = false
        };
            
        var result = await _signInManager.UserManager.CreateAsync(user, model.Password!);

        if (result.Succeeded)
        {
            _logger.LogInformation("User (email: {UserEmail}, username: {UserName}) created a new account with password.", user.Email, user.UserName);
            if (await _signInManager.CanSignInAsync(user))
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
            }

            return Redirect(TenantLink.Action(nameof(Manage.Index), nameof(Manage))!);
        }

        _logger.LogError("User (email: {UserEmail}) could not be created. {Errors}", user.Email, result.Errors);
        AddErrors(result);
        return View(model);
    }

    [HttpPost(nameof(ExternalSignIn))]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult ExternalSignIn(string provider, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return BadRequest();

        // Request a redirect to the external login provider.
        var redirectUrl = TenantLink.Action(nameof(ExternalSignInCallback), nameof(Account), new { ReturnUrl = returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        return Challenge(properties, provider);
    }

    [HttpGet(nameof(ExternalSignInCallback))]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalSignInCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (remoteError != null || !ModelState.IsValid)
        {
            _logger.LogInformation("{Method} failed. Remote error was supplied.", nameof(ExternalSignInCallback));
            return SocialMediaSignInFailure(_localizer["Failed to sign-in with the social network account"].Value + $" ({remoteError})");
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            _logger.LogInformation("{Method} failed to get external sign-in information", nameof(ExternalSignInCallback));
            return SocialMediaSignInFailure(_localizer["Failed to sign-in with the social network account"].Value);
        }

        if (await IsExternalLoginStoredAsync(info))
        {
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                await _signInManager.UpdateExternalAuthenticationTokensAsync(info);

                _logger.LogInformation("User signed-in in with login provider '{LoginProvider}'.", info.LoginProvider);
                return RedirectToLocal(returnUrl ?? $"/{_tenantContext.SiteContext.UrlSegmentValue}");
            }

            if (result.IsLockedOut || result.IsNotAllowed)
            {
                _logger.LogInformation("Account for username '{IdentityName}': {Result}", info.Principal.Identity?.Name ?? "(null)", result);
                return View(ViewNames.Account.SignInRejected, result);
            }
        }

        ViewData[ReturnUrl] = returnUrl;
        ViewData["LoginProvider"] = info.LoginProvider;
        var model = CreateExternalSignInConfirmationViewModelAsync(info.Principal);

        return View(ViewNames.Account.ExternalSignInConfirmation, model);
    }

    [HttpPost(nameof(ExternalSignInConfirmation))]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExternalSignInConfirmation(ExternalSignConfirmationViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            ViewData[ReturnUrl] = returnUrl;
            return View(model);
        }

        // Get the information about the user from the external login provider
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            _logger.LogInformation("{Method} failed to get external sign-in information", nameof(ExternalSignInConfirmation));
            return Redirect(TenantLink.Action(nameof(Message), nameof(Account), new { messageTypeText = MessageType.ExternalSignInFailure })!);
        }
        var user = new ApplicationUser
        {
            UserName = await _tenantContext.DbContext.AppDb.UserRepository.GenerateUniqueUsernameAsync(model.Email, _signInManager.UserManager.Options.User.AllowedUserNameCharacters, "User"), 
            Email = info.Principal.FindFirstValue(ClaimTypes.Email),
            Gender = model.Gender,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Nickname = model.Nickname,
            PhoneNumber = model.PhoneNumber
        };
            
        var result = await _signInManager.UserManager.CreateAsync(user);

        if (result.Succeeded)
        {
            await UpdateUserFromExternalLogin(user, info);

            result = await _signInManager.UserManager.AddLoginAsync(user, info);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation(
                    "User created an account for email '{UserEmail}' using {LoginProvider} provider.", user.Email, info.LoginProvider);

                // Update any authentication tokens as well
                if (await _signInManager.UpdateExternalAuthenticationTokensAsync(info) !=
                    IdentityResult.Success)
                    _logger.LogWarning(
                        "External authentication tokens could not be updated for email '{UserEmail}' using {LoginProvider} provider.", user.Email, info.LoginProvider);

                // email is flagged as confirmed if local email and external email are equal
                if (user.EmailConfirmed)
                {
                    return RedirectToLocal(returnUrl ?? $"/{_tenantContext.SiteContext.UrlSegmentValue}");
                }

                return Redirect(TenantLink.Action(nameof(Message), nameof(Account),
                    new { messageTypeText = MessageType.PleaseConfirmEmail})!);
            }
        }

        AddErrors(result);

        ViewData[ReturnUrl] = returnUrl;
        return View(model);
    }

    [HttpGet(nameof(ForgotPassword))]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost(nameof(ForgotPassword))]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        ApplicationUser? user = null;
        if (model.EmailOrUsername != null && model.EmailOrUsername.Contains('@'))
        {
            user = await _signInManager.UserManager.FindByEmailAsync(model.EmailOrUsername);
            user ??= await _signInManager.UserManager.FindByNameAsync(model.EmailOrUsername);
        }
        
        if (user == null || (!await _signInManager.UserManager.IsEmailConfirmedAsync(user) && _signInManager.UserManager.Options.SignIn.RequireConfirmedEmail))
        {
            _logger.LogInformation("No account found for '{User}'.", model.EmailOrUsername);
            ModelState.AddModelError(string.Empty, _localizer[NoAccountFoundForCredentials]);
            await Task.Delay(5000);
            return View(model);
        }
            
        await SendCodeByEmail(user, EmailPurpose.PasswordReset);

        return Redirect(TenantLink.Action(nameof(Message), nameof(Account), new { messageTypeText = MessageType.ForgotPassword })!);
    }

    [HttpGet(nameof(ResetPassword))]
    [AllowAnonymous]
    public IActionResult ResetPassword(string? code = null)
    {
        if (!ModelState.IsValid) return BadRequest();

        var model = new ResetPasswordViewModel {Code = code};
        if (code == null)
        {
            ModelState.AddModelError(string.Empty, _localizer["The password reset code is missing from the link. Please make sure to use the link we sent unchanged."].Value);
        }
        return View(model);
    }

    [HttpPost(nameof(ResetPassword))]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid || model.Code is null)
        {
            return Redirect(TenantLink.Action(nameof(ResetPassword), nameof(Account))!);
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        ApplicationUser? user = null;
        if (model.EmailOrUsername != null && model.EmailOrUsername.Contains('@'))
        {
            user = await _signInManager.UserManager.FindByEmailAsync(model.EmailOrUsername);
            user ??= await _signInManager.UserManager.FindByNameAsync(model.EmailOrUsername);
        }
        
        if (user == null)
        {
            _logger.LogInformation("No account found for '{User}'.", model.EmailOrUsername);
            ModelState.AddModelError(string.Empty, _localizer[NoAccountFoundForCredentials]);
            await Task.Delay(5000);
            return View();
        }
        var result = await _signInManager.UserManager.ResetPasswordAsync(user, model.Code.Base64UrlDecode() ?? string.Empty, model.Password ?? string.Empty);
        if (result.Succeeded)
        {
            return Redirect(TenantLink.Action(nameof(Message), nameof(Account), new { messageTypeText = MessageType.PasswordChanged })!);
        }
            
        var invalidCode = result.Errors.FirstOrDefault(e => e.Code == _signInManager.UserManager.ErrorDescriber.InvalidToken().Code);
        if (invalidCode != null)
            invalidCode.Description =
                _localizer[
                        "The password reset code is invalid. Please make sure to use the link we sent unchanged, and only once."]
                    .Value;

        AddErrors(result);
        return View();
    }

    /// <summary>
    /// Messages which will be shown after the redirect from a <see cref="HttpMethods.Post"/>.
    /// </summary>
    /// <param name="messageTypeText"></param>
    /// <returns></returns>
    [HttpGet("message/{messageTypeText}")]
    [AllowAnonymous]
    public IActionResult Message(string messageTypeText)
    {
        if (!ModelState.IsValid) return BadRequest();

        if (Enum.TryParse(messageTypeText, true, out MessageType messageType))
        {
            switch (messageType)
            {
                case MessageType.PasswordChanged:
                    return View(ViewNames.Account.ResetPasswordConfirmation);
                case MessageType.ForgotPassword:
                    return View(ViewNames.Account.ForgotPasswordConfirmation);
                case MessageType.PleaseConfirmEmail:
                    return View(ViewNames.Account.PleaseConfirmEmail);
                case MessageType.ConfirmEmailError:
                    return View(ViewNames.Account.ConfirmEmailError);
                case MessageType.SignInRejectedEmailNotConfirmed:
                    return View(ViewNames.Account.SignInRejectedEmailNotConfirmed);
                case MessageType.SignInRejected:
                    return View(ViewNames.Account.SignInRejected);
                case MessageType.ExternalSignInFailure:
                    return View(ViewNames.Account.ExternalSignInFailure);
            }
        }

        _logger.LogWarning("Undefined {MessageType}: '{MessageText}'", nameof(MessageType), messageTypeText);
        return RedirectToLocal($"/{_tenantContext.SiteContext.UrlSegmentValue}");
    }

    private async Task<bool> IsExternalLoginStoredAsync(ExternalLoginInfo info)
    {
        var existingUser = await FindExistingUserAsync(info);
        if (existingUser == null) return true;

        await AssociateExternalLoginAsync(existingUser, info);
        return false;
    }
    private async Task<ApplicationUser?> FindExistingUserAsync(ExternalLoginInfo info)
    {
        if ((User.Identity?.IsAuthenticated ?? false) && User.Identity.Name != null)
        {
            return await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
        }

        var allExternalEmails = info.Principal.FindAll(ClaimTypes.Email).Select(ct => ct.Value);
        foreach (var externalEmail in allExternalEmails)
        {
            var user = await _signInManager.UserManager.FindByEmailAsync(externalEmail);
            if (user != null) return user;
        }

        return null;
    }

    private async Task AssociateExternalLoginAsync(ApplicationUser user, ExternalLoginInfo info)
    {
        var result = await _signInManager.UserManager.AddLoginAsync(user, info);
        if (result != IdentityResult.Success)
        {
            _logger.LogInformation("{Method} {Email} is already associated with another account", nameof(ExternalSignInCallback), user.Email);
            throw new InvalidOperationException(_localizer.GetString("Your {0} account is already associated with another account at {1}", info.LoginProvider, _tenantContext.OrganizationContext.ShortName).Value);
        }
        await UpdateUserFromExternalLogin(user, info);
    }

    private static ExternalSignConfirmationViewModel CreateExternalSignInConfirmationViewModelAsync(ClaimsPrincipal principal)
    {
        return new ExternalSignConfirmationViewModel
        {
            Email = principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            Gender = string.Empty,
            FirstName = principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty,
            LastName = principal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty
        };
    }

    #region ** Helpers **

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    private IActionResult RedirectToLocal(string returnUrl)
    {
        return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : string.Empty);
    }

    private IActionResult SocialMediaSignInFailure(string errorMessage)
    {
        // using POST-REDIRECT-GET (PRG) design pattern might be better: https://andrewlock.net/post-redirect-get-using-tempdata-in-asp-net-core/
        ModelState.AddModelError(string.Empty, errorMessage);
        ViewData["Form"] = "SocialMedia"; // select the form for the validation summary
        return View(nameof(SignIn));
    }

    private async Task UpdateUserFromExternalLogin(ApplicationUser user, ExternalLoginInfo info)
    {
        var requiresUpdate = false;

        // Only update names when they are not yet set
        if (string.IsNullOrEmpty(user.FirstName) && string.IsNullOrEmpty(user.LastName))
        {
            user.FirstName = user.Nickname = info.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? string.Empty;
            user.LastName = info.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? string.Empty;
            // ClaimTypes.Gender is mostly not available from social logins
            // We don't set the gender and let the database set its default value using Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Gender)?.Value
            requiresUpdate = true;
        }

        // leave flag unchanged for existing users
        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            requiresUpdate = true;
        }

        if (requiresUpdate)
        {
            await _signInManager.UserManager.UpdateAsync(user);
        }
    }

    /// <summary>
    /// Sends an email for the given <see cref="EmailPurpose"/> to the <see cref="ApplicationUser"/>
    /// </summary>
    /// <param name="user">The <see cref="ApplicationUser"/> as the recipient of the email.</param>
    /// <param name="purpose">The <see cref="EmailPurpose"/> of the email.</param>
    private async Task SendCodeByEmail(ApplicationUser user, EmailPurpose purpose)
    {
        if (user.Email is null)
        {
            _logger.LogError("Unexpected missing Email for user {User}", user);
            return;
        }

        string code;
        var deadline = DateTime.UtcNow.Add(_dataProtectionTokenProviderOptions.Value.TokenLifespan);
        // round down to full hours
        deadline = new DateTime(deadline.Year, deadline.Month, deadline.Day, deadline.Hour, 0, 0, DateTimeKind.Utc);
            
        switch (purpose)
        {
            case EmailPurpose.PleaseConfirmEmail:
                code = _dataProtector.Encrypt(user.Email, DateTimeOffset.UtcNow.Add(_dataProtectionTokenProviderOptions.Value.TokenLifespan)).Base64UrlEncode();
                _sendEmailTask.SetMessageCreator(new ChangeUserAccountCreator
                {
                    Parameters =
                    {
                        Email = user.Email,
                        Subject = _localizer["Please confirm your email address"].Value,
                        CallbackUrl = TenantLink.ActionLink(nameof(Register), nameof(Account),
                            new {code}, scheme: HttpContext.Request.Scheme),
                        DeadlineUtc = deadline,
                        CultureInfo = CultureInfo.CurrentUICulture,
                        TemplateNameTxt = TemplateName.PleaseConfirmEmailTxt,
                        TemplateNameHtml = TemplateName.PleaseConfirmEmailHtml
                    }
                });
                break;
            case EmailPurpose.PasswordReset:
                code = (await _signInManager.UserManager.GeneratePasswordResetTokenAsync(user)).Base64UrlEncode();
                _sendEmailTask.SetMessageCreator(new ChangeUserAccountCreator
                {
                    Parameters =
                    {
                        Email = user.Email,
                        Subject = _localizer["Please confirm your email address"].Value,
                        CallbackUrl = TenantLink.ActionLink(nameof(ResetPassword), nameof(Account),
                            new {id = user.Id, code}, scheme: HttpContext.Request.Scheme),
                        DeadlineUtc = deadline,
                        CultureInfo = CultureInfo.CurrentUICulture,
                        TemplateNameTxt = TemplateName.PasswordResetTxt,
                        TemplateNameHtml = TemplateName.PasswordResetHtml
                    }
                });
                break;
            default:
                _logger.LogError("Illegal enum type for {Purpose}", nameof(EmailPurpose));
                break;
        }

        _queue.QueueTask(_sendEmailTask);
    }

    #endregion
}
