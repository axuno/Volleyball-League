using System;
using System.Linq;
using League.DI;
using MailMergeLib;
using MailMergeLib.MessageStore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace MailMergeLib.AspNet
{
    public interface IMailMergeService
    {
        IMessageStore MessageStore { get; }
        MailMergeSender Sender { get; }
        MailMergeMessage CreateStandardMessage();
    }

    public class MailMergeServiceConfig
    {
        public Settings Settings { get; set; }
        public IMessageStore MessageStore { get; set; }
    }

    public class MailMergeService : IMailMergeService
    {
        private readonly SiteContext _siteContext;

        public MailMergeService(IOptions<MailMergeServiceConfig> serviceConfig, SiteContext siteContext )
        {
            Settings = serviceConfig.Value.Settings;
            MessageStore = serviceConfig.Value.MessageStore;
            _siteContext = siteContext;
        }
        public Settings Settings { get; }
        public IMessageStore MessageStore { get; }
        public MailMergeSender Sender => new MailMergeSender { Config = Settings.SenderConfig };
        public MailMergeMessage CreateStandardMessage()
        {
            var mmm = new MailMergeMessage
            {
                Config = Settings.MessageConfig,
                PlainText = string.Empty,
                HtmlText = string.Empty,
                Subject = string.Empty,
            };
            mmm.MailMergeAddresses.Clear();

            if (_siteContext?.Email?.GeneralFrom?.Address != null)
            {
                mmm.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.From,
                    _siteContext.Email.GeneralFrom.DisplayName,
                    _siteContext.Email.GeneralFrom.Address));
            }

            if (_siteContext?.Email?.GeneralBcc != null)
            {
                mmm.MailMergeAddresses.Add(new MailMergeAddress(MailAddressType.Bcc,
                    _siteContext.Email.GeneralBcc.DisplayName,
                    _siteContext.Email.GeneralBcc.Address));
            }

            return mmm;
        }
    }

    /// <summary>
    /// Extension methods for adding MailMerge services to the DI container.
    /// </summary>
    public static class MailMergeServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a transient <see cref="IMailMergeService"/> to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> the service will be added to.</param>
        /// <param name="config">The <see cref="MailMergeServiceConfig"/> which will configure the <see cref="IMailMergeService"/>.</param>
        /// <returns>The <see cref="T:IServiceCollection" /> so that additional calls can be chained.</returns>
        public static IServiceCollection AddMailMergeService(this IServiceCollection services, Action<MailMergeServiceConfig> config)
        {
            // Null-checks are part the extension methods:
            services.Configure<MailMergeServiceConfig>(config);
            services.TryAddTransient<IMailMergeService, MailMergeService>();
            // RazorViewToStringRenderer must be used with the current HttpContext
            services.TryAddTransient<RazorViewToStringRenderer>();
            return services;
        }
    }
}
