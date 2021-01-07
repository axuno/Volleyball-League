using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Axuno.TextTemplating;
using Axuno.TextTemplating.Localization;
using League.Emailing.TemplateModels;
using League.Templates.Email.Localization;
using MailMergeLib;
using MailMergeLib.AspNet;
using Microsoft.Extensions.Localization;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.MultiTenancy;
#nullable enable

namespace League.Emailing.Creation
{
    /// <summary>
    /// Creates <see cref="MailMergeMessage"/>s for the given input <see cref="Parameters"/>.
    /// </summary>
    public class ChangeFixtureCreator : IMailMessageCreator
    {
        /// <summary>
        /// Gets the <see cref="ChangeFixtureParameters"/> used as input for creating <see cref="MailMergeMessage"/>s.
        /// </summary>
        public ChangeFixtureParameters Parameters { get; } = new ChangeFixtureParameters();

        /// <summary>
        /// Gets all email messages matching the criteria in <see cref="Parameters"/>.
        /// </summary>
        /// <param name="tenantContext"></param>
        /// <param name="renderer"></param>
        /// <param name="mailMergeService"></param>
        /// <param name="localizer"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="MailMergeMessage"/></returns>
        public async IAsyncEnumerable<MailMergeMessage> GetMailMergeMessages(ITenantContext tenantContext, ITemplateRenderer renderer, IMailMergeService mailMergeService, IStringLocalizer<EmailResource> localizer, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var model = new ChangeFixtureModel
            {
                Fixture = (await tenantContext.DbContext.AppDb.MatchRepository.GetPlannedMatchesAsync(
                        new PredicateExpression(PlannedMatchFields.Id == Parameters.MatchId), cancellationToken))
                    .FirstOrDefault()
            };
            
            if (model.Fixture is null) throw new Exception($"No fixture found for match id '{Parameters.MatchId}'");;
            
            var teamUserRoundInfos = await tenantContext.DbContext.AppDb.TeamRepository.GetTeamUserRoundInfosAsync(
                new PredicateExpression(TeamUserRoundFields.TeamId == model.Fixture.HomeTeamId |
                                        TeamUserRoundFields.TeamId == model.Fixture.GuestTeamId), cancellationToken);

            model.Username = teamUserRoundInfos.First(tur => tur.UserId == Parameters.ChangedByUserId).CompleteName;
            
            foreach (var tur in teamUserRoundInfos)
            {
                using var cs = new CultureSwitcher(Parameters.CultureInfo, Parameters.CultureInfo);

                var mailMergeMessage = mailMergeService.CreateStandardMessage();
                mailMergeMessage.EnableFormatter = false;
                mailMergeMessage.Subject = localizer["Fixture Change"].Value;
                
                mailMergeMessage.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.From, tenantContext.SiteContext.Email.GeneralFrom.DisplayName, tenantContext.SiteContext.Email.GeneralFrom.Address));
                mailMergeMessage.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.To,
                    $"{tur.CompleteName}, Team '{tur.TeamNameForRound}'", tur.Email));
                if (!string.IsNullOrEmpty(tur.Email2))
                {
                    mailMergeMessage.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.CC,
                        $"{tur.CompleteName}, Team '{tur.TeamNameForRound}'", tur.Email2));
                }

                // Send registration info also to league administration
                mailMergeMessage.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.Bcc,
                    tenantContext.SiteContext.Email.GeneralBcc.DisplayName,
                    tenantContext.SiteContext.Email.GeneralBcc.Address));

                mailMergeMessage.PlainText = await renderer.RenderAsync(Templates.Email.TemplateName.ChangeFixtureTxt, model,
                    Parameters.CultureInfo.TwoLetterISOLanguageName);

                yield return mailMergeMessage;
            }
        }
        
        /// <summary>
        /// Input parameters for <see cref="ChangeFixtureCreator"/>.
        /// </summary>
        public class ChangeFixtureParameters
        {
            public long ChangedByUserId { get; set; }
            public long MatchId { get; set; }
            public CultureInfo CultureInfo { get; set; } = CultureInfo.CurrentCulture;
        }
    }
}
