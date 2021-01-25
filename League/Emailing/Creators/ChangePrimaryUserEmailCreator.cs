using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Axuno.TextTemplating;
using Axuno.TextTemplating.Localization;
using League.Emailing.TemplateModels;
using League.Templates.Email;
using League.Templates.Email.Localization;
using MailMergeLib;
using MailMergeLib.AspNet;
using Microsoft.Extensions.Localization;
using TournamentManager.MultiTenancy;
#nullable enable

namespace League.Emailing.Creators
{
    /// <summary>
    /// Creates <see cref="MailMergeMessage"/>s for the given input <see cref="Parameters"/>.
    /// </summary>
    public class ChangePrimaryUserEmailCreator : IMailMessageCreator
    {
        /// <summary>
        /// Gets the <see cref="ChangePrimaryEmailParameters"/> used as input for creating <see cref="MailMergeMessage"/>s.
        /// </summary>
        public ChangePrimaryEmailParameters Parameters { get; } = new ChangePrimaryEmailParameters();

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
            if (Parameters.Email is null || Parameters.NewEmail is null || Parameters.CallbackUrl is null ||
                Parameters.DeadlineUtc is null)
            {
                throw new ArgumentNullException(nameof(Parameters), "One or more required parameters are null");
            }
            
            {
                using var cs = new CultureSwitcher(Parameters.CultureInfo, Parameters.CultureInfo);

                // First email goes to the new email address
                
                var model = new ChangeUserAccountModel()
                {
                    Email = Parameters.NewEmail,
                    CallbackUrl = Parameters.CallbackUrl,
                    Deadline = Parameters.DeadlineUtc
                };
                
                var mailMergeMessage = mailMergeService.CreateStandardMessage();
                mailMergeMessage.EnableFormatter = false;
                mailMergeMessage.Subject = localizer["Please confirm your new primary email"].Value;
                
                mailMergeMessage.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.From, tenantContext.SiteContext.Email.GeneralFrom.DisplayName, tenantContext.SiteContext.Email.GeneralFrom.Address));
                mailMergeMessage.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.To, Parameters.NewEmail));

                mailMergeMessage.PlainText = await renderer.RenderAsync(TemplateName.ConfirmNewPrimaryEmailTxt, model,
                    Parameters.CultureInfo.TwoLetterISOLanguageName);
                mailMergeMessage.HtmlText = await renderer.RenderAsync(TemplateName.ConfirmNewPrimaryEmailHtml, model,
                    Parameters.CultureInfo.TwoLetterISOLanguageName);

                yield return mailMergeMessage;
                
                // Second email goes to the current email address
                
                model = new ChangeUserAccountModel()
                {
                    Email = Parameters.NewEmail,
                    CallbackUrl = string.Empty, // not used in the template
                    Deadline = null  // not used in the template
                };
                
                mailMergeMessage = mailMergeService.CreateStandardMessage();
                mailMergeMessage.EnableFormatter = false;
                mailMergeMessage.Subject = localizer["Your primary email is about to be changed"].Value;
                
                mailMergeMessage.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.From, tenantContext.SiteContext.Email.GeneralFrom.DisplayName, tenantContext.SiteContext.Email.GeneralFrom.Address));
                mailMergeMessage.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.To, Parameters.Email));

                mailMergeMessage.PlainText = await renderer.RenderAsync(TemplateName.NotifyCurrentPrimaryEmailTxt, model,
                    Parameters.CultureInfo.TwoLetterISOLanguageName);
                mailMergeMessage.HtmlText = await renderer.RenderAsync(TemplateName.NotifyCurrentPrimaryEmailHtml, model,
                    Parameters.CultureInfo.TwoLetterISOLanguageName);

                yield return mailMergeMessage;
            }
        }
        
        /// <summary>
        /// Input parameters for <see cref="ChangePrimaryUserEmailCreator"/>.
        /// </summary>
        public class ChangePrimaryEmailParameters
        {
            /// <summary>
            /// The current user email address
            /// </summary>
            public string? Email { get; set; }
            /// <summary>
            /// The new user email address, yet to be confirmed
            /// </summary>
            public string? NewEmail { get; set; }
            /// <summary>
            /// The confirmation url, used for the new email address
            /// </summary>
            public string? CallbackUrl { get; set; }
            /// <summary>
            /// The confirmation deadline, used for the new email address
            /// </summary>
            public DateTime? DeadlineUtc { get; set; }
            /// <summary>
            /// <see cref="CultureInfo"/> used for the emails
            /// </summary>
            public CultureInfo CultureInfo { get; set; } = CultureInfo.CurrentUICulture;
        }
    }
}
