using System.Runtime.CompilerServices;
using Axuno.TextTemplating;
using Axuno.TextTemplating.Localization;
using League.Emailing.TemplateModels;
using League.Templates.Email.Localization;
using MailMergeLib;
using MailMergeLib.AspNet;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.MultiTenancy;

namespace League.Emailing.Creators;

/// <summary>
/// Creates <see cref="MailMergeMessage"/>s for the given input <see cref="Parameters"/>.
/// </summary>
public class ConfirmTeamApplicationCreator : IMailMessageCreator
{
    /// <summary>
    /// Gets the <see cref="ConfirmTeamApplicationParameters"/> used as input for creating <see cref="MailMergeMessage"/>s.
    /// </summary>
    public ConfirmTeamApplicationParameters Parameters { get; } = new();

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
        var roundWithType = (await tenantContext.DbContext.AppDb.RoundRepository.GetRoundsWithTypeAsync(
            new PredicateExpression(
                RoundFields.TournamentId == tenantContext.TournamentContext.ApplicationTournamentId &
                RoundFields.Id == Parameters.RoundId), cancellationToken))[0];

        var teamUserRoundInfos = await tenantContext.DbContext.AppDb.TeamRepository.GetTeamUserRoundInfosAsync(
            new PredicateExpression(TeamUserRoundFields.TeamId == Parameters.TeamId &
                                    TeamUserRoundFields.TournamentId ==
                                    tenantContext.TournamentContext.ApplicationTournamentId), cancellationToken);
        var tournament =
            await tenantContext.DbContext.AppDb.TournamentRepository.GetTournamentAsync(new PredicateExpression(TournamentFields.Id ==
                tenantContext.TournamentContext.ApplicationTournamentId), cancellationToken);
            
        var registeredBy = teamUserRoundInfos.First(tur => tur.UserId == Parameters.RegisteredByUserId);

        var model = new ConfirmTeamApplicationModel
        {
            IsNewApplication = Parameters.IsNewApplication,
            RegisteredByName = registeredBy.CompleteName,
            RegisteredByEmail = registeredBy.Email,
            TeamName = teamUserRoundInfos.First(tur => tur.TeamId == Parameters.TeamId).TeamNameForRound,
            BankTransferId = $"A{tournament!.Id}-T{Parameters.TeamId}-U{registeredBy.UserId}",
            TournamentName = tournament.Name,
            RoundDescription = roundWithType.Description,
            RoundTypeDescription = roundWithType.RoundType.Description,
            UrlToEditApplication = Parameters.UrlToEditApplication
        };

        foreach (var tur in teamUserRoundInfos)
        {
            using var cs = new CultureSwitcher(Parameters.CultureInfo, Parameters.CultureInfo);
               
            model.IsRegisteringUser = Parameters.RegisteredByUserId == tur.UserId;

            var mailMergeMessage = mailMergeService.CreateStandardMessage();
            mailMergeMessage.EnableFormatter = false;
            mailMergeMessage.Subject = model.IsNewApplication
                ? localizer["Confirmation of the team registration"].Value
                : localizer["Update of team registration"].Value;

            mailMergeMessage.MailMergeAddresses.Add(MailKind.AutoMailFrom, tenantContext);
            mailMergeMessage.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.To,
                $"{tur.CompleteName}, Team '{tur.TeamName}'", tur.Email));
            if (!string.IsNullOrEmpty(tur.Email2))
            {
                mailMergeMessage.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.CC,
                    $"{tur.CompleteName}, Team '{tur.TeamName}'", tur.Email2));
            }

            if (model.IsRegisteringUser)
            {
                mailMergeMessage.MailMergeAddresses.Add(MailKind.AutoMailTo, tenantContext);
                mailMergeMessage.MailMergeAddresses.Add(MailKind.AutoMailBcc, tenantContext);
            }

            mailMergeMessage.PlainText = await renderer.RenderAsync(Templates.Email.TemplateName.ConfirmTeamApplicationTxt, model,
                Parameters.CultureInfo.TwoLetterISOLanguageName);

            yield return mailMergeMessage;
        }
    }
        
    /// <summary>
    /// Input parameters for <see cref="ConfirmTeamApplicationCreator"/>.
    /// </summary>
    public class ConfirmTeamApplicationParameters
    {
        public bool IsNewApplication { get; set; }
        public long RoundId { get; set; }
        public long TeamId { get; set; }
        public long RegisteredByUserId { get; set; }
        public string UrlToEditApplication { get; set; } = string.Empty;
        public CultureInfo CultureInfo { get; set; } = CultureInfo.CurrentCulture;
    }
}
