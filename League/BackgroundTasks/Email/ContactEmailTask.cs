using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using League.Identity;
using League.Models.HomeViewModels;
using MailMergeLib;
using MailMergeLib.AspNet;
using Microsoft.Extensions.Logging;
using TournamentManager.MultiTenancy;

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
        public (ContactViewModel Form, ITenantContext TenantContext) Model { get; set; }

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

            // For empty OrganizationKey only ISite members are available
            var tenantContext = Model.TenantContext;
            var emailTo = !string.IsNullOrEmpty(tenantContext?.SiteContext.Email.ContactTo.Address)
                ? tenantContext.SiteContext.Email.ContactTo.Address
                : "contact@volleyball-liga.de";
            var emailFrom = !string.IsNullOrEmpty(tenantContext?.SiteContext.Email.ContactFrom.Address)
                ? tenantContext.SiteContext.Email.ContactFrom.Address
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
