using System.Runtime.CompilerServices;
using Axuno.TextTemplating;
using Axuno.TextTemplating.Localization;
using League.Emailing.TemplateModels;
using League.Templates.Email.Localization;
using MailMergeLib;
using MailMergeLib.AspNet;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.QuerySpec;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.MultiTenancy;

namespace League.Emailing.Creators;

/// <summary>
/// Creates <see cref="MailMergeMessage"/>s for the given input <see cref="Parameters"/>.
/// </summary>
public class ResultEnteredCreator : IMailMessageCreator
{
    /// <summary>
    /// Gets the <see cref="ResultEnteredParameters"/> used as input for creating <see cref="MailMergeMessage"/>s.
    /// </summary>
    public ResultEnteredParameters Parameters { get; } = new();

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
        if (Parameters.Match is null) throw new InvalidOperationException($"Input parameter {nameof(ResultEnteredModel.Match)} must not be null");;

        var teamUserRoundInfos = await tenantContext.DbContext.AppDb.TeamRepository.GetTeamUserRoundInfosAsync(
            new PredicateExpression((TeamUserRoundFields.TeamId == Parameters.Match.HomeTeamId |
                                     TeamUserRoundFields.TeamId == Parameters.Match.GuestTeamId)
                .And(TeamUserRoundFields.TournamentId == tenantContext.TournamentContext.MatchResultTournamentId)),
            cancellationToken);

        var username = teamUserRoundInfos.Find(tur => tur.UserId == Parameters.ChangedByUserId)?.CompleteName;
        // User is not a team member, maybe an admin
        username ??= (await tenantContext.DbContext.AppDb.UserRepository.FindUserAsync(
                    new PredicateExpression(UserFields.Id == Parameters.ChangedByUserId), 1, cancellationToken))[0]
                .CompleteName;

        var model = new ResultEnteredModel
        {
            Match = Parameters.Match,
            RoundDescription = teamUserRoundInfos.First(tur => tur.RoundId == Parameters.Match.RoundId).RoundDescription,
            Username = username,
            HomeTeamName = teamUserRoundInfos.First(tur => tur.TeamId == Parameters.Match.HomeTeamId).TeamNameForRound,
            GuestTeamName = teamUserRoundInfos.First(tur => tur.TeamId == Parameters.Match.GuestTeamId).TeamNameForRound
        };

        var plainTextContent = Parameters.IsResultRemoved
            ? await renderer.RenderAsync(Templates.Email.TemplateName.ResultRemovedTxt, model,
                Parameters.CultureInfo.TwoLetterISOLanguageName)
            : await renderer.RenderAsync(Templates.Email.TemplateName.ResultEnteredTxt, model,
                Parameters.CultureInfo.TwoLetterISOLanguageName);
            

        var recipientGroups = new[]
        {
            teamUserRoundInfos.Where(tur => tur.TeamId == model.Match.HomeTeamId), // home team users
            teamUserRoundInfos.Where(tur => tur.TeamId == model.Match.GuestTeamId) // guest team users
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
                        mailMergeMessage.Subject = Parameters.IsResultRemoved
                            ? localizer["Match result removed: '{0}' vs. '{1}'", model.HomeTeamName, model.GuestTeamName].Value
                            : localizer["Match result '{0}' vs. '{1}'", model.HomeTeamName, model.GuestTeamName].Value;
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
                
            // Send info also to league administration
            mailMergeMessage.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.Bcc,
                tenantContext.SiteContext.Email.GeneralBcc.DisplayName,
                tenantContext.SiteContext.Email.GeneralBcc.Address));
                
            mailMergeMessage.PlainText = plainTextContent;

            yield return mailMergeMessage;
        }
    }
        
    /// <summary>
    /// Input parameters for <see cref="ResultEnteredCreator"/>.
    /// </summary>
    public class ResultEnteredParameters
    {
        public long ChangedByUserId { get; set; }
        public bool IsResultRemoved { get; set; } = false;
        public MatchEntity? Match { get; set; }
        public CultureInfo CultureInfo { get; set; } = CultureInfo.CurrentCulture;
    }
}
