using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using League.Controllers;
using League.DI;
using League.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.Data;

namespace League.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("{organization:ValidOrganizations}/[area]/[controller]")]
    public class Impersonation : AbstractController
    {
        private readonly ILogger<Language> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly SiteContext _siteContext;

        public Impersonation(SignInManager<ApplicationUser> signInManager, SiteContext siteContext, ILogger<Language> logger)
        {
            _signInManager = signInManager;
            _siteContext = siteContext;
            _logger = logger;
        }

        [Authorize(Roles = Constants.RoleName.SystemManager)]
        [HttpGet("")]
        public async Task<IActionResult> Index(string search, int limit = 20)
        {
            limit = Math.Abs(limit);
            var users = new List<UserEntity>();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var pe = new PredicateExpression();
                pe.AddWithOr(new FieldLikePredicate(UserFields.Email, null, null, search){CaseSensitiveCollation = true});
                pe.AddWithOr(new FieldLikePredicate(UserFields.FirstName, null, null, search){CaseSensitiveCollation = true});
                pe.AddWithOr(new FieldLikePredicate(UserFields.LastName, null, null, search){CaseSensitiveCollation = true});
                pe.AddWithOr(new FieldLikePredicate(UserFields.Nickname, null, null, search){CaseSensitiveCollation = true});
                users = await _siteContext.AppDb.UserRepository.FindUserAsync(pe, limit + 1, CancellationToken.None);
            }
            ViewData.Add("Limit", limit);
            return View(ViewNames.Area.Admin.Impersonation.Index, users);
        }

        [Authorize(Roles = Constants.RoleName.SystemManager)]
        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Start(long id)
        {
            var currentUser = User;
            var targetUser = await _signInManager.UserManager.FindByIdAsync(id.ToString());

            var targetClaimsPrincipal = await _signInManager.CreateUserPrincipalAsync(targetUser);
            if (targetClaimsPrincipal != null && targetClaimsPrincipal.Identity is ClaimsIdentity targetClaimsIdentity)
            {
                targetClaimsIdentity.AddClaim(new Claim(Constants.ClaimType.ImpersonatedByUser, User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value));
            }
            
            // sign out the current user
            await _signInManager.SignOutAsync();

            // impersonate the target user
            await _signInManager.Context.SignInAsync(IdentityConstants.ApplicationScheme, targetClaimsPrincipal);
            _logger.LogInformation($"User '{currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value}' now impersonates user '{targetClaimsPrincipal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value}'.");

            return RedirectToLocal("/" + _siteContext.UrlSegmentValue);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Stop()
        {
            var originalUserId = User.Claims.FirstOrDefault(x => x.Type == Constants.ClaimType.ImpersonatedByUser)?.Value;
            if (originalUserId != null)
            {
                var appUser = await _signInManager.UserManager.FindByIdAsync(originalUserId);
                await _signInManager.SignInAsync(appUser, true);
            }
            else
            {
                await _signInManager.SignOutAsync();
            }

            return RedirectToLocal("/" + _siteContext.UrlSegmentValue);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            returnUrl = WebUtility.UrlDecode(returnUrl);
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect("/");
        }
    }
}