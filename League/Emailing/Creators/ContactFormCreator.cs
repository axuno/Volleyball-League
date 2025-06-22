using System.Runtime.CompilerServices;
using Axuno.TextTemplating;
using Axuno.TextTemplating.Localization;
using League.Templates.Email.Localization;
using MailMergeLib;
using MailMergeLib.AspNet;
using TournamentManager.MultiTenancy;

namespace League.Emailing.Creators;

/// <summary>
/// Creates <see cref="MailMergeMessage"/>s for the given input <see cref="Parameters"/>.
/// </summary>
public class ContactFormCreator : IMailMessageCreator
{
    /// <summary>
    /// Gets the <see cref="ContactFormParameters"/> used as input for creating <see cref="MailMergeMessage"/>s.
    /// </summary>
    public ContactFormParameters Parameters { get; } = new();

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
        if (Parameters.ContactForm is null)
        {
            throw new InvalidOperationException($"Required parameters {nameof(Parameters.ContactForm)} is null");
        }

        var model = new {Form = Parameters.ContactForm};
            
        {
            using var cs = new CultureSwitcher(Parameters.CultureInfo, Parameters.CultureInfo);

            var mailMergeMessage = mailMergeService.CreateStandardMessage();
            mailMergeMessage.EnableFormatter = false;
            mailMergeMessage.Subject = model.Form.Subject!; // user-generated, cannot localize!

            mailMergeMessage.MailMergeAddresses.Add(MailKind.ContactFrom, tenantContext);
            mailMergeMessage.MailMergeAddresses.Add(MailKind.ContactTo, tenantContext);
            mailMergeMessage.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.ReplyTo, model.Form.Email));

            mailMergeMessage.PlainText = await renderer.RenderAsync(Templates.Email.TemplateName.ContactFormTxt, model,
                Parameters.CultureInfo.TwoLetterISOLanguageName);

            yield return mailMergeMessage;
        }
    }
        
    /// <summary>
    /// Input parameters for <see cref="ChangeFixtureCreator"/>.
    /// </summary>
    public class ContactFormParameters
    {
        public Models.ContactViewModels.ContactViewModel? ContactForm { get; set; }
        public CultureInfo CultureInfo { get; set; } = CultureInfo.CurrentUICulture;
    }
}
