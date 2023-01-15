using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using League.Controllers;
using League.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.MultiTenancy;

namespace League.Areas.Admin.Controllers;

[Area("Admin")]
[Route("{organization:MatchingTenant}/[area]/[controller]")]
public class Impersonation : AbstractController
{
    private readonly ILogger<Language> _logger;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITenantContext _tenantContext;

    public Impersonation(SignInManager<ApplicationUser> signInManager, ITenantContext tenantContext, ILogger<Language> logger)
    {
        _signInManager = signInManager;
        _tenantContext = tenantContext;
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
            users = await _tenantContext.DbContext.AppDb.UserRepository.FindUserAsync(pe, limit + 1, CancellationToken.None);
        }
        ViewData.Add("Limit", limit);
        return View(Views.ViewNames.Area.Admin.Impersonation.Index, users);
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
        _logger.LogInformation("User '{user}' now impersonates user '{targetUser}'.",
            currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
            targetClaimsPrincipal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

        return RedirectToLocal("/" + _tenantContext.SiteContext.UrlSegmentValue);
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

        return RedirectToLocal("/" + _tenantContext.SiteContext.UrlSegmentValue);
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