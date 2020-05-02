using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using League.DI;
using League.Identity;
using League.Models.HomeViewModels;
using MailMergeLib;
using MailMergeLib.AspNet;
using Microsoft.Extensions.Logging;

namespace League.BackgroundTasks.Email
{
    public class ContactEmailTask : AbstractEmailTask
    {
        /// <summary>
        /// CTOR. None of the dependency injection parameters must have scoped lifetime.
        /// </summary>
        /// <param name="backgroundWebHost"></param>
        /// <param name="mailMergeService"></param>
        /// <param name="logger"></param>
        public ContactEmailTask(BackgroundWebHost backgroundWebHost, IMailMergeService mailMergeService,
            ILogger<ContactEmailTask> logger)
            : base(backgroundWebHost, mailMergeService, logger)
        { }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        public (ContactViewModel Form, OrganizationSiteContext SiteContext) Model { get; set; }

        /// <summary>
        /// The view names for the HTML and plain text part of the email.
        /// </summary>
        public string[] ViewNames { get; set; }

        /// <summary>
        /// Sends the email message to the <see cref="ApplicationUser"/>.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task RunAsync(CancellationToken cancellationToken)
        {
            SetThreadCulture();

            // For empty OrganizationKey only IOrganizationSite members are available
            var siteContext = Model.SiteContext;
            var emailTo = !string.IsNullOrEmpty(siteContext?.Email?.ContactTo?.Address)
                ? siteContext.Email.ContactTo.Address
                : "contact@volleyball-liga.de";
            var emailFrom = !string.IsNullOrEmpty(siteContext?.Email?.ContactFrom?.Address)
                ? siteContext.Email.ContactFrom.Address
                : "noreply@volleyball-liga.de";

            MailMessage.Subject = Model.Form.Subject;
            MailMessage.MailMergeAddresses.Clear();
            MailMessage.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.From, emailFrom));
            
            MailMessage.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.To, emailTo));
            // no HtmlText
            MailMessage.PlainText = await GetRenderer().RenderViewToStringAsync(ViewNames[1], Model);

            // send the message
            await base.RunAsync(cancellationToken);
        }
    }
}
