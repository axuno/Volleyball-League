using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Axuno.Web;
using League.BackgroundTasks.Email;
using League.DI;
using League.Identity;
using League.Models.AccountViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TournamentManager.ModelValidators;

namespace League.Controllers
{
    [Authorize]
    [Route("{organization:ValidOrganizations}/[controller]")]
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
            ConfirmYourEmail,
            /// <summary>
            /// Email to reset the password
            /// </summary>
            ForgotPassword
        }

        #endregion

        #region *** Private variables **

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IStringLocalizer<Account> _localizer;
        private readonly IOptions<DataProtectionTokenProviderOptions> _dataProtectionTokenProviderOptions;
        private readonly ILogger<Account> _logger;
        private readonly OrganizationSiteContext _organizationSiteContext;
        private readonly Axuno.BackgroundTask.IBackgroundQueue _queue;
        private readonly UserEmailTask _userEmailTask;
        private readonly DataProtection.DataProtector _dataProtector;
        private readonly TournamentManager.DI.PhoneNumberService _phoneNumberService;
        private readonly RegionInfo _regionInfo;

        #endregion

        public Account(
            SignInManager<ApplicationUser> signInManager,
            IStringLocalizer<Account> localizer,
            IOptions<DataProtectionTokenProviderOptions> dataProtectionTokenProviderOptions,
            ILogger<Account> logger,
            OrganizationSiteContext organizationSiteContext,
            Axuno.BackgroundTask.IBackgroundQueue queue,
            UserEmailTask userEmailTask,
            DataProtection.DataProtector dataProtector,
            TournamentManager.DI.PhoneNumberService phoneNumberService,
            RegionInfo regionInfo)
        {
            _signInManager = signInManager;
            _localizer = localizer;
            _dataProtectionTokenProviderOptions = dataProtectionTokenProviderOptions;
            _logger = logger;
            _organizationSiteContext = organizationSiteContext;
            _queue = queue;
            _userEmailTask = userEmailTask;
            _dataProtector = dataProtector;
            _phoneNumberService = phoneNumberService;
            _regionInfo = regionInfo;
        }

        [HttpGet("sign-in")]
        [AllowAnonymous]
        public IActionResult SignIn(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("sign-in")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignInViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);
            
            ApplicationUser user = null;
            if (model.EmailOrUsername.Contains('@'))
            {
                user = await _signInManager.UserManager.FindByEmailAsync(model.EmailOrUsername);
            }

            if (user == null)
            {
                user = await _signInManager.UserManager.FindByNameAsync(model.EmailOrUsername);
            }

            if (user == null)
            {
                _logger.LogInformation($"No account found for '{model.EmailOrUsername}'.");
                ModelState.AddModelError(string.Empty, _localizer["No account found for these credentials."]);
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation($"User Id '{user.Id}' signed in.");
                return RedirectToLocal(returnUrl ?? "/" + _organizationSiteContext.UrlSegmentValue);
            }

            if (result.IsLockedOut || result.IsNotAllowed)
            {
                if (!await _signInManager.UserManager.IsEmailConfirmedAsync(user) &&
                    _signInManager.UserManager.Options.SignIn.RequireConfirmedEmail)
                {
                    _logger.LogInformation($"Sign-in not allowed: Email for user id '{user.Id}' is not confirmed");
                    return RedirectToAction(nameof(Message), new { messageTypeText = MessageType.SignInRejectedEmailNotConfirmed });
                }

                _logger.LogInformation($"Account for user id '{user.Id}': {result}");
                return RedirectToAction(nameof(Message), new { messageTypeText = MessageType.SignInRejected });
            }

            // PasswordSignIn failed
            _logger.LogInformation($"Wrong password for '{model.EmailOrUsername}'.");
            ModelState.AddModelError(string.Empty, _localizer["No account found for these credentials."]);
            return View(model);
        }

        [HttpGet("sign-out")]
        public async Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User signed out.");
            return RedirectToAction(nameof(Home.Index), nameof(Controllers.Home));
        }

        [HttpGet("create")]
        [AllowAnonymous]
        public IActionResult CreateAccount(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("create")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateAccount(CreateAccountViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (model.Captcha != HttpContext.Session.GetString(CaptchaImageGenerator.CaptchaSessionKeyName))
            {
                ModelState.AddModelError(nameof(CreateAccountViewModel.Captcha), _localizer["Math task result was incorrect"]);
            }

            if (await _signInManager.UserManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError(nameof(CreateAccountViewModel.Email), _localizer["This email is not available for a new account"]);
            }

            if (!ModelState.IsValid)
            {
                ModelState[nameof(CreateAccountViewModel.Captcha)].RawValue = string.Empty;
                return View(model);
            }

            await SendCodeByEmail(new ApplicationUser {Email = model.Email}, EmailPurpose.ConfirmYourEmail);
            return RedirectToAction(nameof(Message), new { messageTypeText = MessageType.PleaseConfirmEmail });
        }

        [HttpGet("register")]
        [AllowAnonymous]
        public IActionResult Register(string code = null, string returnUrl = null)
        {
            if (!_dataProtector.TryDecrypt(code?.Base64UrlDecode() ?? string.Empty, out var email, out var expiration))
            {
                return RedirectToAction(nameof(Message), new { messageTypeText = MessageType.ConfirmEmailError });
            }

            // Note: We check, whether the email is available only during POST
            var model = new RegisterViewModel {Email = email};

            ViewData["ReturnUrl"] = returnUrl;
            return View(model); 
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!_dataProtector.TryDecrypt(model.Code?.Base64UrlDecode() ?? string.Empty, out var email, out var expiration))
            {
                return RedirectToAction(nameof(Message), new { messageTypeText = MessageType.ConfirmEmailError });
            }
            model.Email = email;
            if (await _signInManager.UserManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError(nameof(RegisterViewModel.Email), _localizer["The email is no longer available for registration. Have you registered already?"]);
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
                UserName = await _organizationSiteContext.AppDb.UserRepository.GenerateUniqueUsernameAsync(model.Email,
                    _signInManager.UserManager.Options.User.AllowedUserNameCharacters, "User"),
                Email = model.Email,
                EmailConfirmed = true,
                Gender = model.Gender,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber)
                    ? string.Empty
                    : _phoneNumberService.FormatForStorage(model.PhoneNumber, _regionInfo.TwoLetterISORegionName),
                PhoneNumberConfirmed = false
            };
            
            var result = await _signInManager.UserManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation($"User (email: {user.Email}, username: {user.UserName}) created a new account with password.");
                if (await _signInManager.CanSignInAsync(user))
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                }

                return RedirectToAction(nameof(Manage.Index), nameof(Manage));
            }

            _logger.LogError($"User (email: {user.Email}) could not be created.", result.Errors);
            AddErrors(result);
            return View(model);
        }

        [HttpPost(nameof(ExternalSignIn))]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalSignIn(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalSignInCallback), nameof(Account), new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return Challenge(properties, provider);
        }

        [HttpGet(nameof(ExternalSignInCallback))]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalSignInCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                _logger.LogInformation($"{nameof(ExternalSignInCallback)} failed: {remoteError}.");
                return SocialMediaSignInFailure($"Remote error: {remoteError}",
                    _localizer["Failed to sign-in with the social network account"] + $" ({remoteError})");
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogInformation($"{nameof(ExternalSignInCallback)} failed to get external sign-in information");
                return SocialMediaSignInFailure("Failed to get external sign-in information",
                    _localizer["Failed to sign-in with the social network account"]);
            }
            
            // Sign-in user if provider represents a valid already registered user

            // There is no external login stored for this user?
            if (await _signInManager.UserManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey) == null)
            {
                ApplicationUser existingUser = null;
                // if the the current user is signed-in
                if (User.Identity.IsAuthenticated)
                {
                    existingUser = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);
                }
                else
                {
                    // see if there is a user account for any external provider email
                    var allExternalEmails = info.Principal.FindAll(ClaimTypes.Email).Select(ct => ct.Value);
                    foreach (var externalEmail in allExternalEmails)
                    {
                        existingUser = await _signInManager.UserManager.FindByEmailAsync(externalEmail);
                        if (existingUser != null) break;
                    }
                }
                if (existingUser != null)
                {
                    // Add external user login for this user.
                    if (await _signInManager.UserManager.AddLoginAsync(existingUser, info) != IdentityResult.Success)
                    {
                        _logger.LogInformation($"{nameof(ExternalSignInCallback)} {existingUser.Email} is already associated with another account");
                        return SocialMediaSignInFailure($"Social network account for {existingUser.Email} is already associated with another account",
                            _localizer.GetString("Your {0} account is already associated with another account at {1}", info.LoginProvider, _organizationSiteContext.ShortName));
                    }

                    await UpdateUserFromExternalLogin(existingUser, info);
                }
            }

            // sign in the user with this external login provider if the user already has a registered external login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                // Update any authentication tokens if login succeeded
                await _signInManager.UpdateExternalAuthenticationTokensAsync(info);

                _logger.LogInformation($"User signed-in in with login provider '{info.LoginProvider}'.");
                return RedirectToLocal(returnUrl ?? "/" + _organizationSiteContext.UrlSegmentValue);
            }
            if (result.IsLockedOut || result.IsNotAllowed)
            {
                _logger.LogInformation($"Account for username '{info.Principal.Identity.Name}': {result}");
                return View(ViewNames.Account.SignInRejected, result);
            }

            // If the user does not have an account, then ask the user to create an account.
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["LoginProvider"] = info.LoginProvider;
            // We use the first email returned by the provider for the new account

            return View(ViewNames.Account.ExternalSignInConfirmation,
                new ExternalSignConfirmationViewModel
                {
                    Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                    Gender = string.Empty,
                    FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty,
                    LastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty
                });
        }

        [HttpPost(nameof(ExternalSignInConfirmation))]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalSignInConfirmation(ExternalSignConfirmationViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogInformation($"{nameof(ExternalSignInConfirmation)} failed to get external sign-in information");
                return RedirectToAction(nameof(Message), new { messageTypeText = MessageType.ExternalSignInFailure });
            }
            var user = new ApplicationUser
            {
                UserName = await _organizationSiteContext.AppDb.UserRepository.GenerateUniqueUsernameAsync(model.Email, _signInManager.UserManager.Options.User.AllowedUserNameCharacters, "User"), 
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
                        $"User created an account for email '{user.Email}' using {info.LoginProvider} provider.");

                    // Update any authentication tokens as well
                    if (await _signInManager.UpdateExternalAuthenticationTokensAsync(info) !=
                        IdentityResult.Success)
                        _logger.LogWarning(
                            $"External authentication tokens could not be updated for email '{user.Email}' using {info.LoginProvider} provider.");

                    // email is flagged as confirmed if local email and external email are equal
                    if (user.EmailConfirmed)
                    {
                        return RedirectToLocal(returnUrl ?? "/" + _organizationSiteContext.UrlSegmentValue);
                    }

                    return RedirectToAction(nameof(Message),
                        new {messageTypeText = MessageType.PleaseConfirmEmail});
                }
            }

            AddErrors(result);

            ViewData["ReturnUrl"] = returnUrl;
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

            ApplicationUser user = null;
            if (model.EmailOrUsername.Contains('@'))
            {
                user = await _signInManager.UserManager.FindByEmailAsync(model.EmailOrUsername);
            }

            if (user == null)
            {
                user = await _signInManager.UserManager.FindByNameAsync(model.EmailOrUsername);
            }

            if (user == null || (!await _signInManager.UserManager.IsEmailConfirmedAsync(user) && _signInManager.UserManager.Options.SignIn.RequireConfirmedEmail))
            {
                _logger.LogInformation($"No account found for '{model.EmailOrUsername}'.");
                ModelState.AddModelError(string.Empty, _localizer["No account found for these credentials."]);
                await Task.Delay(5000);
                return View(model);
            }
            
            await SendCodeByEmail(user, EmailPurpose.ForgotPassword);

            return RedirectToAction(nameof(Message), new { messageTypeText = MessageType.ForgotPassword });
        }

        [HttpGet(nameof(ResetPassword))]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
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
            if (model.Code == null)
            {
                return RedirectToAction(nameof(ResetPassword));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ApplicationUser user = null;
            if (model.EmailOrUsername.Contains('@'))
            {
                user = await _signInManager.UserManager.FindByEmailAsync(model.EmailOrUsername);
            }

            if (user == null)
            {
                user = await _signInManager.UserManager.FindByNameAsync(model.EmailOrUsername);
            }

            if (user == null)
            {
                _logger.LogInformation($"No account found for '{model.EmailOrUsername}'.");
                ModelState.AddModelError(string.Empty, _localizer["No account found for these credentials."]);
                await Task.Delay(5000);
                return View();
            }
            var result = await _signInManager.UserManager.ResetPasswordAsync(user, model.Code.Base64UrlDecode(), model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Message), new { messageTypeText = MessageType.PasswordChanged });
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

            _logger.LogWarning($"Undefined {nameof(MessageType)}: '{messageTypeText}'");
            return RedirectToLocal("/" + _organizationSiteContext.UrlSegmentValue);
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
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            return RedirectToAction(nameof(Home.Index), nameof(Controllers.Home));
        }

        private IActionResult SocialMediaSignInFailure(string logErrorText, string errorMessage)
        {
            _logger.LogWarning(logErrorText);
            // using POST-REDIRECT-GET (PRG) design pattern might be better: https://andrewlock.net/post-redirect-get-using-tempdata-in-asp-net-core/
            ModelState.AddModelError(string.Empty, errorMessage);
            ViewData["Form"] = "SocialMedia"; // select the form for the validation summary
            return View(nameof(SignIn));
        }

        private async Task UpdateUserFromExternalLogin(ApplicationUser user, ExternalLoginInfo info)
        {
            bool requiresUpdate = false;

            // Only update names when they are not yet set
            if (string.IsNullOrEmpty(user.FirstName) && string.IsNullOrEmpty(user.LastName))
            {
                user.FirstName = user.Nickname = info.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? string.Empty;
                user.LastName = info.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? string.Empty;
                // ClaimTypes.Gender is mostly not available from social logins
                // We don't set the gender and let the database set its default value
                // user.Gender = info.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Gender)?.Value;
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
            string code;
            _userEmailTask.Timeout = TimeSpan.FromMinutes(1);
            _userEmailTask.EmailCultureInfo = CultureInfo.CurrentUICulture;
            _userEmailTask.ToEmail = user.Email;

            switch (purpose)
            {
                case EmailPurpose.ConfirmYourEmail:
                    _userEmailTask.Subject = _localizer["Please confirm your email address"].Value;
                    _userEmailTask.ViewNames = new [] { ViewNames.Emails.EmailPleaseConfirmEmail, ViewNames.Emails.EmailPleaseConfirmEmailTxt };
                    _userEmailTask.LogMessage = "Send email confirmation mail";
                    code = _dataProtector.Encrypt(user.Email, DateTimeOffset.UtcNow.Add(_dataProtectionTokenProviderOptions.Value.TokenLifespan)).Base64UrlEncode();
                    _userEmailTask.Model = (Email: user.Email, CallbackUrl: Url.Action(nameof(Register), nameof(Account), new { code }, protocol: HttpContext.Request.Scheme), _organizationSiteContext);
                    break;
                case EmailPurpose.ForgotPassword:
                    _userEmailTask.Subject = _localizer["This is your password recovery key"].Value;
                    _userEmailTask.ViewNames = new [] { ViewNames.Emails.EmailPasswordReset, ViewNames.Emails.EmailPasswordResetTxt };
                    _userEmailTask.LogMessage = "Password recovery email";
                    code = (await _signInManager.UserManager.GeneratePasswordResetTokenAsync(user)).Base64UrlEncode();
                    _userEmailTask.Model = (Email: user.Email, CallbackUrl: Url.Action(nameof(ResetPassword), nameof(Account), new { id = user.Id, code }, protocol: HttpContext.Request.Scheme), _organizationSiteContext);
                    break;
                default:
                    _logger.LogError($"Illegal enum type for {nameof(EmailPurpose)}");
                    break;
            }

            _queue.QueueTask(_userEmailTask);
        }

        #endregion
    }
}
