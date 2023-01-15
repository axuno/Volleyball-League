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
using TournamentManager.MultiTenancy;
#nullable enable

namespace League.Emailing.Creators;

/// <summary>
/// Creates <see cref="MailMergeMessage"/>s for the given input <see cref="Parameters"/>.
/// </summary>
public class ChangeUserAccountCreator : IMailMessageCreator
{
    /// <summary>
    /// Gets the <see cref="ChangeUserAccountParameters"/> used as input for creating <see cref="MailMergeMessage"/>s.
    /// </summary>
    public ChangeUserAccountParameters Parameters { get; } = new();

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
        if (Parameters.Email is null || Parameters.Subject is null || Parameters.CallbackUrl is null ||
            Parameters.TemplateNameTxt is null || Parameters.TemplateNameHtml is null ||
            Parameters.DeadlineUtc is null)
        {
            throw new ArgumentNullException(nameof(Parameters), "One or more required parameters are null");
        }
            
        var model = new ChangeUserAccountModel()
        {
            Email = Parameters.Email,
            CallbackUrl = Parameters.CallbackUrl,
            Deadline = Parameters.DeadlineUtc
        };
            
        {
            using var cs = new CultureSwitcher(Parameters.CultureInfo, Parameters.CultureInfo);

            var mailMergeMessage = mailMergeService.CreateStandardMessage();
            mailMergeMessage.EnableFormatter = false;
            mailMergeMessage.Subject = Parameters.Subject; // already localized
                
            mailMergeMessage.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.From, tenantContext.SiteContext.Email.GeneralFrom.DisplayName, tenantContext.SiteContext.Email.GeneralFrom.Address));
            mailMergeMessage.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.To, Parameters.Email));

            mailMergeMessage.PlainText = await renderer.RenderAsync(Parameters.TemplateNameTxt, model,
                Parameters.CultureInfo.TwoLetterISOLanguageName);
            mailMergeMessage.HtmlText = await renderer.RenderAsync(Parameters.TemplateNameHtml, model,
                Parameters.CultureInfo.TwoLetterISOLanguageName);

            yield return mailMergeMessage;
        }
    }
        
    /// <summary>
    /// Input parameters for <see cref="ChangeFixtureCreator"/>.
    /// </summary>
    public class ChangeUserAccountParameters
    {
        public string? Email { get; set; }
        public string? CallbackUrl { get; set; }
        public DateTime? DeadlineUtc { get; set; }
        public string? Subject { get; set; }
        public string? TemplateNameTxt { get; set; }
        public string? TemplateNameHtml { get; set; }
        public CultureInfo CultureInfo { get; set; } = CultureInfo.CurrentUICulture;
    }
}