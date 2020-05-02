using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using TournamentManager.DAL.EntityClasses;

namespace League.Authorization
{
    public static class TeamOperations
    {
        public static readonly OperationAuthorizationRequirement ChangePhoto =
            new OperationAuthorizationRequirement { Name = nameof(ChangePhoto) };
        public static readonly OperationAuthorizationRequirement EditTeam =
            new OperationAuthorizationRequirement { Name = nameof(EditTeam) };
        public static readonly OperationAuthorizationRequirement AddTeamMember =
            new OperationAuthorizationRequirement { Name = nameof(AddTeamMember) };
        public static readonly OperationAuthorizationRequirement RemoveTeamMember =
            new OperationAuthorizationRequirement { Name = nameof(RemoveTeamMember) };
        public static readonly OperationAuthorizationRequirement SignUpForSeason =
            new OperationAuthorizationRequirement { Name = nameof(SignUpForSeason) };
    }

    public class TeamAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, TeamEntity>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, TeamEntity team)
        {
            if (new [] {nameof(TeamOperations.ChangePhoto), nameof(TeamOperations.EditTeam), nameof(TeamOperations.AddTeamMember), nameof(TeamOperations.RemoveTeamMember), nameof(TeamOperations.SignUpForSeason) }.Contains(requirement.Name)
                && context.User.IsInRole(Identity.Constants.RoleName.TeamManager) &&
                    context.User.HasClaim(Identity.Constants.ClaimType.ManagesTeam, team.Id.ToString()))
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
}

