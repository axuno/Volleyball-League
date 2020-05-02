using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using League.Identity;
using MailMergeLib;
using MailMergeLib.AspNet;
using Microsoft.Extensions.Logging;
using TournamentManager.Data;

namespace League.BackgroundTasks.Email
{
    public class UserEmailTask : AbstractEmailTask
    {
        /// <summary>
        /// CTOR. None of the dependency injection parameters must have scoped lifetime.
        /// </summary>
        /// <param name="backgroundWebHost"></param>
        /// <param name="mailMergeService"></param>
        /// <param name="logger"></param>
        public UserEmailTask(BackgroundWebHost backgroundWebHost, IMailMergeService mailMergeService,
            ILogger<UserEmailTask> logger)
            : base(backgroundWebHost, mailMergeService, logger)
        { }

        /// <summary>
        /// Creates a new <see cref="UserEmailTask"/> instance initialized like after dependency injection
        /// </summary>
        /// <returns>Return new <see cref="UserEmailTask"/> instance initialized like after dependency injection</returns>
        public UserEmailTask CreateNew()
        {
            return new UserEmailTask(BackgroundWebHost, MailMergeService, (ILogger<UserEmailTask>) Logger) {Model = (string.Empty, string.Empty, Model.OrganizationContext)};
        }

        /// <summary>
        /// Gets or sets the subject field of the email.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// The view names for the HTML and plain text part of the email.
        /// </summary>
        public string[] ViewNames { get; set; }

        /// <summary>
        /// Gets or sets the Model for creating the email content.
        /// </summary>
        public (string Email, string CallbackUrl, OrganizationContext OrganizationContext) Model { get; set; }

        /// <summary>
        /// Gets or sets the recipient's email address.
        /// </summary>
        public string ToEmail { get; set; }

        /// <summary>
        /// Sends the email message to the <see cref="ApplicationUser"/>.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task RunAsync(CancellationToken cancellationToken)
        {
            SetThreadCulture();

            MailMessage.Subject = Subject;
            // FROM address is already set!
            MailMessage.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.To, ToEmail));
            MailMessage.HtmlText = await GetRenderer().RenderViewToStringAsync(ViewNames[0], Model);
            MailMessage.PlainText = await GetRenderer().RenderViewToStringAsync(ViewNames[1], Model);

            // send the message
            await base.RunAsync(cancellationToken);
        }
    }
}
