using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using League.Identity;
using MailMergeLib;
using MailMergeLib.AspNet;
using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.Data;
using TournamentManager.MultiTenancy;

namespace League.BackgroundTasks.Email
{
    public class FixtureEmailTask : AbstractEmailTask
    {
        private readonly Axuno.Tools.DateAndTime.TimeZoneConverter _timeZoneConverter;

        /// <summary>
        /// CTOR. None of the dependency injection parameters must have scoped lifetime.
        /// </summary>
        /// <param name="backgroundWebHost"></param>
        /// <param name="mailMergeService"></param>
        /// <param name="timeZoneConverter"></param>
        /// <param name="logger"></param>
        public FixtureEmailTask(BackgroundWebHost backgroundWebHost, IMailMergeService mailMergeService,
            Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter,
            ILogger<FixtureEmailTask> logger)
            : base(backgroundWebHost, mailMergeService, logger)
        {
            _timeZoneConverter = timeZoneConverter;
        }

        /// <summary>
        /// Creates a new <see cref="FixtureEmailTask"/> instance initialized like after dependency injection
        /// </summary>
        /// <returns>Return new <see cref="FixtureEmailTask"/> instance initialized like after dependency injection</returns>
        public FixtureEmailTask CreateNew()
        {
            return new FixtureEmailTask(BackgroundWebHost, MailMergeService, _timeZoneConverter, (ILogger<FixtureEmailTask>) Logger) {Model = Model};
        }

        /// <summary>
        /// Gets or sets the subject field of the email.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// The view names for the HTML and plain text part of the email.
        /// </summary>
        public string[] ViewNames { get; set; }

        /// <summary>
        /// Gets or sets the Model for creating the email content.
        /// </summary>
        public (ClaimsPrincipal ChangedByUser, long MatchId, ITenantContext TenantContext, PlannedMatchRow Fixture) Model { get; set; }

        /// <summary>
        /// Sends the email message to the <see cref="ApplicationUser"/>.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task RunAsync(CancellationToken cancellationToken)
        {
            SetThreadCulture();

            var getCompleteUsername = new Func<TeamUserRoundRow, string>(row => string.Join(" ", new[] { row.Title, row.FirstName, row.MiddleName, row.LastName }).Replace("  ", " ").Trim());

            var model = Model; // Necessary to re-assign structure members

            model.Fixture = (await Model.TenantContext.DbContext.AppDb.MatchRepository.GetPlannedMatchesAsync(
                new PredicateExpression(PlannedMatchFields.Id == Model.MatchId), cancellationToken)).FirstOrDefault();

            if (model.Fixture == null)
            {
                Logger.LogError($"No fixture found for ID {Model.MatchId}");
                return;
            }

            var teamUsers = await Model.TenantContext.DbContext.AppDb.TeamRepository.GetTeamUserRoundInfosAsync(
                new PredicateExpression(TeamUserRoundFields.TeamId == model.Fixture.HomeTeamId |
                                        TeamUserRoundFields.TeamId == model.Fixture.GuestTeamId), cancellationToken);

            var recipients = new List<object>();
            foreach (var teamUser in teamUsers)
            {
                recipients.Add(new  {CompleteName = getCompleteUsername(teamUser), Email = teamUser.Email, Email2 = teamUser.Email2});
            }
            recipients.Add( new { CompleteName = Model.TenantContext.SiteContext.Email.GeneralTo.DisplayName, Email = Model.TenantContext.SiteContext.Email.GeneralTo.Address, Email2 = string.Empty });

            MailData = recipients;

            MailMessage.Subject = string.Format(Subject,
                _timeZoneConverter.ToZonedTime(model.Fixture.OrigPlannedStart ?? model.Fixture.PlannedStart)
                    .DateTimeOffset.Date.ToShortDateString());
            MailMessage.Config.IgnoreIllegalRecipientAddresses = true;
            // FROM address is already set!
            MailMessage.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.To, "{CompleteName}", "{Email}"));
            MailMessage.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.CC, "{CompleteName}", "{Email2}"));
            //MailMessage.HtmlText = await GetRenderer().RenderViewToStringAsync(ViewNames[0], model);
            MailMessage.PlainText = await GetRenderer().RenderViewToStringAsync(ViewNames[1], model);

            // send the message
            await base.RunAsync(cancellationToken);
        }
    }
}
