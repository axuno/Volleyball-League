using Microsoft.AspNetCore.Authorization.Infrastructure;
using TournamentManager.DAL.EntityClasses;

namespace League.Authorization;

public static class MatchOperations
{
    public static readonly OperationAuthorizationRequirement ChangeFixture = new() { Name = nameof(ChangeFixture) };
    public static readonly OperationAuthorizationRequirement EnterResult = new() { Name = nameof(EnterResult) };
    public static readonly OperationAuthorizationRequirement OverruleResult = new() { Name = nameof(OverruleResult) };
}

public class MatchAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, MatchEntity>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, MatchEntity match)
    {
        if (new [] {nameof(MatchOperations.ChangeFixture), nameof(MatchOperations.EnterResult)}.Contains(requirement.Name)
            && context.User.IsInRole(Identity.Constants.RoleName.TeamManager) &&
            context.User.HasClaim(Identity.Constants.ClaimType.ManagesTeam, match.HomeTeamId.ToString()))
        {
            context.Succeed(requirement);
        }

        // all operations permitted
        if (context.User.IsInRole(Identity.Constants.RoleName.SystemManager)
            || context.User.IsInRole(Identity.Constants.RoleName.TournamentManager))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
