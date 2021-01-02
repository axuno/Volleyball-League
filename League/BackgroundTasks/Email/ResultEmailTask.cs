using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using League.Identity;
using MailMergeLib;
using MailMergeLib.AspNet;
using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.Data;
using TournamentManager.MultiTenancy;

namespace League.BackgroundTasks.Email
{
    public class ResultEmailTask : AbstractEmailTask
    {
        private readonly Axuno.Tools.DateAndTime.TimeZoneConverter _timeZoneConverter;

        /// <summary>
        /// CTOR. None of the dependency injection parameters must have scoped lifetime.
        /// </summary>
        /// <param name="backgroundWebHost"></param>
        /// <param name="mailMergeService"></param>
        /// <param name="timeZoneConverter"></param>
        /// <param name="logger"></param>
        public ResultEmailTask(BackgroundWebHost backgroundWebHost, IMailMergeService mailMergeService,
            Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter,
            ILogger<ResultEmailTask> logger)
            : base(backgroundWebHost, mailMergeService, logger)
        {
            _timeZoneConverter = timeZoneConverter;
        }

        /// <summary>
        /// Creates a new <see cref="ResultEmailTask"/> instance initialized like after dependency injection
        /// </summary>
        /// <returns>Return new <see cref="ResultEmailTask"/> instance initialized like after dependency injection</returns>
        public ResultEmailTask CreateNew()
        {
            return new ResultEmailTask(BackgroundWebHost, MailMergeService, _timeZoneConverter, (ILogger<ResultEmailTask>) Logger) {Model = Model};
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
        public (ClaimsPrincipal ChangedByUser, MatchEntity Match, List<TeamUserRoundRow> TeamUserRound, ITenantContext TenantContext) Model { get; set; }

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
            
            model.TeamUserRound = await Model.TenantContext.DbContext.AppDb.TeamRepository.GetTeamUserRoundInfosAsync(
                new PredicateExpression(TeamUserRoundFields.TeamId == model.Match.HomeTeamId |
                                        TeamUserRoundFields.TeamId == model.Match.GuestTeamId), cancellationToken);

            var recipients = new List<object>();
            foreach (var teamUser in model.TeamUserRound)
            {
                recipients.Add(new  {CompleteName = getCompleteUsername(teamUser), Email = teamUser.Email, Email2 = teamUser.Email2});
            }
            recipients.Add( new { CompleteName = Model.TenantContext.SiteContext.Email.GeneralTo.DisplayName, Email = Model.TenantContext.SiteContext.Email.GeneralTo.Address, Email2 = string.Empty });

            MailData = recipients;

            MailMessage.Subject = string.Format(Subject,
                _timeZoneConverter.ToZonedTime(model.Match.RealStart)?
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
