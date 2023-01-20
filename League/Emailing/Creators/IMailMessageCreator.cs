using System.Collections.Generic;
using System.Threading;
using Axuno.TextTemplating;
using League.Templates.Email.Localization;
using MailMergeLib;
using MailMergeLib.AspNet;
using Microsoft.Extensions.Localization;
using TournamentManager.MultiTenancy;

namespace League.Emailing.Creators;

public interface IMailMessageCreator
{
    IAsyncEnumerable<MailMergeMessage> GetMailMergeMessages(ITenantContext tenantContext, ITemplateRenderer renderer, IMailMergeService mailMergeService, IStringLocalizer<EmailResource> localizer, CancellationToken cancellationToken);
}
