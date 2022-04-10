using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.TypedViewClasses;

namespace League.Authorization
{
    public static class VenueOperations
    {
        public static readonly OperationAuthorizationRequirement CreateVenue = new() { Name = nameof(CreateVenue) };
        public static readonly OperationAuthorizationRequirement EditVenue = new() { Name = nameof(EditVenue) };
        public static readonly OperationAuthorizationRequirement RemoveVenue = new() { Name = nameof(RemoveVenue) };
    }

    public class VenueAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, IList<VenueTeamRow>>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, IList<VenueTeamRow> venueTeamRows)
        {
            // All team managers may add venues
            if (requirement.Name == VenueOperations.CreateVenue.Name && context.User.IsInRole(Identity.Constants.RoleName.TeamManager))
            {
                context.Succeed(requirement);
            }

            // The IDs of the teams the current user is managing
            var managingTeamIds = context.User.Claims.Where(c => c.Type == Identity.Constants.ClaimType.ManagesTeam).Select(c => c.Value);

            // The manager must be managing a team which is using this venue
            if (new[] {nameof(VenueOperations.EditVenue), nameof(VenueOperations.RemoveVenue)}.Contains(
                    requirement.Name)
                && context.User.IsInRole(Identity.Constants.RoleName.TeamManager) && venueTeamRows
                    .Select(tvr => tvr.TeamId.ToString()).Intersect(managingTeamIds).Any())
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

