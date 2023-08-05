using Axuno.BackgroundTask;
using Axuno.TextTemplating;
using League.Emailing.Creators;
using League.Templates.Email.Localization;
using MailMergeLib.AspNet;
using TournamentManager.MultiTenancy;

namespace League.BackgroundTasks;

/// <summary>
/// The class to send out email messages using a <see cref="Microsoft.Extensions.Hosting.BackgroundService"/> implementation.
/// </summary>
public class SendEmailTask : IBackgroundTask
{
    private readonly IMailMergeService _mailMergeService;
    private readonly ITenantContext _tenantContext;
    private readonly IStringLocalizer<EmailResource> _localizer;
    private readonly ILogger<SendEmailTask> _logger;
    private readonly ITemplateRenderer _renderer;
    private IMailMessageCreator? _mailMessageCreator;
        
    /// <summary>
    /// CTOR.
    /// </summary>
    /// <param name="mailMergeService"></param>
    /// <param name="renderer"></param>
    /// <param name="tenantContext"></param>
    /// <param name="localizer"></param>
    /// <param name="logger"></param>
    public SendEmailTask(IMailMergeService mailMergeService, ITemplateRenderer renderer, ITenantContext tenantContext, IStringLocalizer<EmailResource> localizer, ILogger<SendEmailTask> logger)
    {
        _mailMergeService = mailMergeService;
        _renderer = renderer;
        _localizer = localizer;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new <see cref="SendEmailTask"/> instance (shallow-copy) initialized like after dependency injection
    /// </summary>
    /// <returns>Return new <see cref="SendEmailTask"/> instance initialized like after dependency injection</returns>
    public SendEmailTask CreateNewInstance()
    {
        return new SendEmailTask(_mailMergeService, _renderer, _tenantContext, _localizer, _logger) {Timeout = Timeout};
    }
        
    /// <summary>
    /// Set the <see cref="IMailMessageCreator"/> that will create the mail messages that <see cref="SendEmailTask"/> will send.
    /// </summary>
    /// <param name="mailMessageCreator">The <see cref="IMailMessageCreator"/>.</param>
    public void SetMessageCreator(IMailMessageCreator mailMessageCreator)
    {
        _mailMessageCreator = mailMessageCreator;
    }
        
    /// <summary>
    /// Invokes the <see cref="IMailMessageCreator"/> and sends the mail messages.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns><see cref="Task"/></returns>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        if(_mailMessageCreator is null) return;
            
        await foreach (var mmm in _mailMessageCreator.GetMailMergeMessages(_tenantContext, _renderer, _mailMergeService, _localizer, cancellationToken))
        {
            try
            {
                await _mailMergeService.Sender.SendAsync(mmm, null);
            }
            catch (Exception e) when (e is TaskCanceledException || e is OperationCanceledException)
            {
                _logger.LogError(e, "Sending mail canceled. {recipients}\nSubject: {subject}\nMessage: {message}",
                    mmm.MailMergeAddresses, mmm.Subject, mmm.PlainText);
                _mailMergeService.Sender.SendCancel();
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Mail sender failure. {recipients}\nSubject: {subject}\nMessage: {message}",
                    mmm.MailMergeAddresses, mmm.Subject, mmm.PlainText);
                throw;
            }
        }
    }

    /// <summary>
    /// The timeout that is used by an <see cref="IBackgroundQueue"/> to limit the duration of execution.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(1);
}
