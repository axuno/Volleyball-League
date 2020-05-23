using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Axuno.BackgroundTask;
using MailMergeLib;
using MailMergeLib.AspNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace League.BackgroundTasks.Email
{
    public class AbstractEmailTask : IBackgroundTask
    {
        public AbstractEmailTask(BackgroundWebHost backgroundWebHost, IMailMergeService mailMergeService, ILogger<AbstractEmailTask> logger)
        {
            BackgroundWebHost = backgroundWebHost;
            MailMergeService = mailMergeService;
            Logger = logger;
            Timeout = TimeSpan.FromMinutes(1);
            MailMessage = MailMergeService.CreateStandardMessage();
        }

        /// <summary>
        /// Gets or sets the <see cref="object"/> that will be used by<see cref="MailMergeSender.SendAsync"/>
        /// in order to replace {Placeholders} with variable content.
        /// </summary>
        protected object MailData { get; set; }
        /// <summary>
        /// Gets the the <see cref="MailMergeMessage"/> created by <see cref="MailMergeLib.AspNet.MailMergeService.CreateStandardMessage"/>.
        /// </summary>
        protected MailMergeMessage MailMessage { get; set; }

        protected BackgroundWebHost BackgroundWebHost { get; }
        protected IMailMergeService MailMergeService { get; }
        protected ILogger<AbstractEmailTask> Logger { get; }

        public CultureInfo EmailCultureInfo { get; set; }
        public string LogMessage { get; set; }

        public TimeSpan Timeout { get; set; }
        
        protected RazorViewToStringRenderer GetRenderer()
        {
            return BackgroundWebHost.Instance.Services.GetRequiredService<RazorViewToStringRenderer>();
        }

        public virtual async Task RunAsync(CancellationToken cancellationToken)
        {
            var to = new InternetAddressList();
            var cc = new InternetAddressList();
            var bcc = new InternetAddressList();

            var mailSender = MailMergeService.Sender; // there's always a new instance returned!
            
            var onBeforeSend = new EventHandler<MailSenderBeforeSendEventArgs>((sender, args) =>
            {
                to.AddRange(args.MimeMessage.To);
                cc.AddRange(args.MimeMessage.Cc);
                bcc.AddRange(args.MimeMessage.Bcc);
            });

            mailSender.OnBeforeSend += onBeforeSend;

            try
            {
                if (MailData is IEnumerable<object> mailData)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await mailSender.SendAsync<object>(MailMessage, mailData);
                }
                else
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await mailSender.SendAsync(MailMessage, MailData);
                }

                Logger.LogInformation($"Mail sent. TO: {to} - CC: {cc} - BCC: {bcc}");
            }
            catch (Exception e) when (e is TaskCanceledException || e is OperationCanceledException)
            {
                Logger.LogError(e,"Mail failure. TO: {to} - CC: {cc} - BCC: {bcc}\nSubject: {subject}\nMessage: {message}", to, cc, bcc, MailMessage.Subject, MailMessage.PlainText);
                mailSender.SendCancel();
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e,"Mail failure. TO: {to} - CC: {cc} - BCC: {bcc}\nSubject: {subject}\nMessage: {message}", to, cc, bcc, MailMessage.Subject, MailMessage.PlainText);
                throw;
            }
            finally
            {
                MailMergeService.Sender.OnBeforeSend -= onBeforeSend;
                to.Clear();
                cc.Clear();
                bcc.Clear();
            }
        }

        protected void SetThreadCulture()
        {
            (Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentUICulture) = (EmailCultureInfo, EmailCultureInfo);
        }
    }
}
