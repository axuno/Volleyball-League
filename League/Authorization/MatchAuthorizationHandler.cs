﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using TournamentManager.DAL.EntityClasses;

namespace League.Authorization
{
    public static class MatchOperations
    {
        public static readonly OperationAuthorizationRequirement ChangeFixture =
            new OperationAuthorizationRequirement { Name = nameof(ChangeFixture) };
        public static readonly OperationAuthorizationRequirement EnterResult =
            new OperationAuthorizationRequirement { Name = nameof(EnterResult) };
        public static readonly OperationAuthorizationRequirement OverrideResult =
            new OperationAuthorizationRequirement { Name = nameof(OverrideResult) };
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
}

