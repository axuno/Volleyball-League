using League.Templates.Email;

namespace League.Emailing.TemplateModels;

/// <summary>
/// The model is used for template <see cref="TemplateName.ConfirmTeamApplicationTxt"/>
/// </summary>
public class ConfirmTeamApplicationModel
{
    /// <summary>
    /// If <see langword="true"/>, it is a new application, otherwise
    /// an existing application was updated.
    /// </summary>
    public bool IsNewApplication { get; set; }
    public string TournamentName { get; set; } = string.Empty;
    public string RoundDescription { get; set; } = string.Empty;
    public string RoundTypeDescription { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    /// <summary>
    /// The ID that should be used for the bank transfer to pay the registration fee.
    /// </summary>
    public string BankTransferId { get; set; } = string.Empty;
    /// <summary>
    /// If <see langword="true"/>, specific content for the registering person is shown.
    /// </summary>
    public bool IsRegisteringUser { get; set; }
    public string RegisteredByName { get; set; } = string.Empty;
    public string RegisteredByEmail { get; set; } = string.Empty;
    public string UrlToEditApplication { get; set; } = string.Empty;
}
