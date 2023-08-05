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
using TournamentManager.MultiTenancy;

namespace League.Emailing.Creators;

/// <summary>
/// Creates <see cref="MailMergeMessage"/>s for the given input <see cref="Parameters"/>.
/// </summary>
public class ChangeFixtureCreator : IMailMessageCreator
{
    /// <summary>
    /// Gets the <see cref="ChangeFixtureParameters"/> used as input for creating <see cref="MailMergeMessage"/>s.
    /// </summary>
    public ChangeFixtureParameters Parameters { get; } = new();

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
            new PredicateExpression((TeamUserRoundFields.TeamId == model.Fixture.HomeTeamId |
                                     TeamUserRoundFields.TeamId == model.Fixture.GuestTeamId)
                .And(TeamUserRoundFields.TournamentId == tenantContext.TournamentContext.MatchPlanTournamentId)),
            cancellationToken);

        model.Username = teamUserRoundInfos.FirstOrDefault(tur => tur.UserId == Parameters.ChangedByUserId)?.CompleteName;
        // User is not a team member, maybe an admin
        model.Username ??= (await tenantContext.DbContext.AppDb.UserRepository.FindUserAsync(
                new PredicateExpression(UserFields.Id == Parameters.ChangedByUserId), 1, cancellationToken)).First()
            .CompleteName;
            
        var plainTextContent = await renderer.RenderAsync(Templates.Email.TemplateName.ChangeFixtureTxt, model,
            Parameters.CultureInfo.TwoLetterISOLanguageName);

        var recipientGroups = new[]
        {
            teamUserRoundInfos.Where(tur => tur.TeamId == model.Fixture.HomeTeamId), // home team users
            teamUserRoundInfos.Where(tur => tur.TeamId == model.Fixture.GuestTeamId) // guest team users
        };
            
        // each recipient group will get the same email text
        foreach (var recipients in recipientGroups)
        {
            var mailMergeMessage = mailMergeService.CreateStandardMessage();
            mailMergeMessage.EnableFormatter = false;
            mailMergeMessage.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.From, tenantContext.SiteContext.Email.GeneralFrom.DisplayName, tenantContext.SiteContext.Email.GeneralFrom.Address));

            foreach (var tur in recipients)
            {
                if (string.IsNullOrWhiteSpace(mailMergeMessage.Subject))
                {
                    using (new CultureSwitcher(Parameters.CultureInfo, Parameters.CultureInfo))
                    {
                        mailMergeMessage.Subject = localizer["Fixture change for team '{0}'", tur.TeamNameForRound].Value;
                    }
                }
                    
                mailMergeMessage.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.To,
                    $"{tur.CompleteName}", tur.Email));    
                        
                if (!string.IsNullOrEmpty(tur.Email2))
                {
                    mailMergeMessage.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.CC,
                        $"{tur.CompleteName}", tur.Email2));
                }
            }
                
            // Send registration info also to league administration
            mailMergeMessage.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.Bcc,
                tenantContext.SiteContext.Email.GeneralBcc.DisplayName,
                tenantContext.SiteContext.Email.GeneralBcc.Address));
                
            mailMergeMessage.PlainText = plainTextContent;

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
