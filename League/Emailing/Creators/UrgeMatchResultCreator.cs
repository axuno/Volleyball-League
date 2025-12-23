using System.Runtime.CompilerServices;
using Axuno.TextTemplating;
using Axuno.TextTemplating.Localization;
using League.Emailing.TemplateModels;
using League.Templates.Email.Localization;
using MailMergeLib;
using MailMergeLib.AspNet;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.QuerySpec;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.MultiTenancy;

namespace League.Emailing.Creators;

/// <summary>
/// Creates <see cref="MailMergeMessage"/>s for the given input <see cref="Parameters"/>.
/// </summary>
public class UrgeMatchResultCreator : IMailMessageCreator
{
    /// <summary>
    /// Gets the <see cref="UrgeMatchResultParameters"/> used as input for creating <see cref="MailMergeMessage"/>s.
    /// </summary>
    public UrgeMatchResultParameters Parameters { get; } = new();
        
    public async IAsyncEnumerable<MailMergeMessage> GetMailMergeMessages(ITenantContext tenantContext, ITemplateRenderer renderer, IMailMergeService mailMergeService, IStringLocalizer<EmailResource> localizer, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var fixtures = await tenantContext.DbContext.AppDb.MatchRepository.GetPlannedMatchesAsync(
            new PredicateExpression(PlannedMatchFields.PlannedStart
                .Between(Parameters.ReferenceDateUtc, Parameters.ReferenceDateUtc.AddDays(1).AddSeconds(-1))
                .And(PlannedMatchFields.PlannedStart.IsNotNull()
                    .And(PlannedMatchFields.PlannedEnd.IsNotNull().And(PlannedMatchFields.TournamentId ==
                                                                       tenantContext.TournamentContext
                                                                           .MatchPlanTournamentId)))),
            cancellationToken);

        if(fixtures.Count == 0) yield break;
            
        var teamIds = new HashSet<long>();
        fixtures.ForEach(f =>
        {
            teamIds.Add(f.HomeTeamId);
            teamIds.Add(f.GuestTeamId);
        });
            
        var teamUserRoundInfos = await tenantContext.DbContext.AppDb.TeamRepository.GetTeamUserRoundInfosAsync(
            new PredicateExpression(TeamUserRoundFields.TeamId.In(teamIds).And(TeamUserRoundFields.TournamentId == tenantContext.TournamentContext.MatchPlanTournamentId)), cancellationToken);
            
        foreach (var fixture in fixtures)
        {
            var model = new RemindMatchResultModel
            {
                Fixture = fixture,
            };

            var recipientGroups = new[]
            {
                teamUserRoundInfos.Where(tur => tur.TeamId == fixture.HomeTeamId || tur.TeamId == fixture.GuestTeamId && tur.IsManager), // all team managers
            };
                
            var plainTextContent = await renderer.RenderAsync(
                Templates.Email.TemplateName.UrgeMatchResultTxt, model,
                Parameters.CultureInfo.TwoLetterISOLanguageName);

            // each recipient group will get the same email text
            foreach (var recipients in recipientGroups)
            {
                var mailMergeMessage = mailMergeService.CreateStandardMessage();
                mailMergeMessage.EnableFormatter = false;
                mailMergeMessage.MailMergeAddresses.Add(MailKind.AutoMailFrom, tenantContext);

                foreach (var tur in recipients)
                {
                    if (string.IsNullOrWhiteSpace(mailMergeMessage.Subject))
                    {
                        using (new CultureSwitcher(Parameters.CultureInfo, Parameters.CultureInfo))
                        {
                            mailMergeMessage.Subject = localizer["Canceled match day '{0}' vs. '{1}", model.Fixture.HomeTeamNameForRound, model.Fixture.GuestTeamNameForRound].Value;
                        }
                    }
                        
                    mailMergeMessage.MailMergeAddresses.Add(new(MailAddressType.To,
                        $"{tur.CompleteName}", tur.Email));    
                        
                    if (!string.IsNullOrEmpty(tur.Email2))
                    {
                        mailMergeMessage.MailMergeAddresses.Add(new(MailAddressType.CC,
                            $"{tur.CompleteName}", tur.Email2));
                    }
                }

                mailMergeMessage.MailMergeAddresses.Add(MailKind.AutoMailTo, tenantContext);
                mailMergeMessage.MailMergeAddresses.Add(MailKind.AutoMailBcc, tenantContext);

                mailMergeMessage.PlainText = plainTextContent;

                yield return mailMergeMessage;
            }
        }
    }
        
    /// <summary>
    /// Input parameters for <see cref="UrgeMatchResultCreator"/>.
    /// </summary>
    public class UrgeMatchResultParameters
    {
        /// <summary>
        /// <see cref="PlannedMatchRow"/>s will be selected,
        /// if <see cref="PlannedMatchRow.PlannedStart"/> date is between
        /// <see cref="ReferenceDateUtc"/>.Date and <see cref="ReferenceDateUtc"/>.AddDays(1).AddSeconds(-1).
        /// </summary>
        public DateTime ReferenceDateUtc { get; set; }
        public CultureInfo CultureInfo { get; set; } = CultureInfo.CurrentCulture;
    }
}
