using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using League.DI;
using League.Identity;
using MailMergeLib;
using MailMergeLib.AspNet;
using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.Data;

namespace League.BackgroundTasks.Email
{
    public class TeamApplicationEmailTask : AbstractEmailTask
    {
        private readonly SiteContext _siteContext;

        /// <summary>
        /// CTOR. None of the dependency injection parameters must have scoped lifetime.
        /// </summary>
        /// <param name="backgroundWebHost"></param>
        /// <param name="mailMergeService"></param>
        /// <param name="logger"></param>
        public TeamApplicationEmailTask(BackgroundWebHost backgroundWebHost, IMailMergeService mailMergeService,
            SiteContext siteContext,
            ILogger<TeamApplicationEmailTask> logger)
            : base(backgroundWebHost, mailMergeService, logger)
        {
            _siteContext = siteContext;
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
        public Models.TeamApplicationViewModels.ApplicationEmailViewModel Model { get; set; }

        /// <summary>
        /// Sends the email message to the <see cref="ApplicationUser"/>.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task RunAsync(CancellationToken cancellationToken)
        {
            SetThreadCulture();

            var roundWithType = (await _siteContext.AppDb.RoundRepository.GetRoundsWithTypeAsync(
                new PredicateExpression(RoundFields.TournamentId == _siteContext.ApplicationTournamentId &
                                        RoundFields.Id == Model.RoundId), cancellationToken)).First();

            var teamUserRoundInfos = await _siteContext.AppDb.TeamRepository.GetTeamUserRoundInfosAsync(
                new PredicateExpression(TeamUserRoundFields.TeamId == Model.TeamId & TeamUserRoundFields.TournamentId == _siteContext.ApplicationTournamentId), cancellationToken);

            var registeredBy = teamUserRoundInfos.First(tur => tur.UserId == Model.RegisteredByUserId);
            Model.RegisteredByName = $"{registeredBy.CompleteName} <{registeredBy.Email}>";
            Model.RoundDescription = roundWithType.Description;
            Model.RoundTypeDescription = roundWithType.RoundType.Description;

            foreach (var tur in teamUserRoundInfos)
            {
                Model.IsRegisteringUser = Model.RegisteredByUserId == tur.UserId;
                
                // FROM address is already set with _siteContext.Email.GeneralFrom
                MailMessage = MailMergeService.CreateStandardMessage();
                MailMessage.Subject = Subject;
                MailMessage.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.To, $"{tur.CompleteName}, Team '{Model.TeamName}'", tur.Email));
                if (!string.IsNullOrEmpty(tur.Email2))
                {
                    MailMessage.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.To,
                        $"{tur.CompleteName}, Team '{Model.TeamName}'", tur.Email2));
                }

                if (Model.IsRegisteringUser)
                {
                    // Send registration info to league administration
                    MailMessage.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.Bcc,
                        _siteContext.Email.GeneralTo.DisplayName, _siteContext.Email.GeneralTo.Address));
                }
                MailMessage.PlainText = await GetRenderer().RenderViewToStringAsync(ViewNames[1], Model);
                // send the message
                await base.RunAsync(cancellationToken);
            }
        }
    }
}
