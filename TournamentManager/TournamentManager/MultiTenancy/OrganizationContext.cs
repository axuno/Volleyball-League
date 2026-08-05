using System.Text.Json.Serialization;
using TournamentManager.JsonCSerializer;

namespace TournamentManager.MultiTenancy;

/// <summary>
/// All configuration data for an organization.
/// </summary>
[JsonComment("All configuration data for an organization.")]
public class OrganizationContext : IOrganizationContext
{
    /// <summary>
    /// The <see cref="TournamentManager.MultiTenancy.ITenant"/> corresponding to this context.
    /// </summary>
    [JsonIgnore]
    public ITenant? Tenant { get; set; }
        
    /// <summary>
    /// The full name of the organization.
    /// </summary>
        [JsonComment("The full name of the organization.")]
    public virtual string Name { get; set; } = string.Empty;

    /// <summary>
    /// The short version of the organization's name.
    /// </summary>
    [JsonComment("The short version of the organization's name.")]
    public virtual string ShortName { get; set; } = string.Empty;

    /// <summary>
    /// A description of the organization.
    /// </summary>
    [JsonComment("A description of the organization.")]
    public virtual string Description { get; set; } = string.Empty;
        
    /// <summary>
    /// The homepage for the organization (NOT the league website).
    /// </summary>
    [JsonComment("The homepage for the organization (NOT the league website).")]
    public string HomepageUrl { get; set; } = string.Empty;
        
    /// <summary>
    /// Bank details of the organization, e.g. for payments of participation fees.
    /// </summary>
    public BankDetails Bank { get; set; } = new();
}
    
/// <summary>
/// Bank details of the organization, e.g. for payments of participation fees.
/// </summary>
public class BankDetails
{
    /// <summary>
    /// If <see langword="true"/>, bank details are part of the confirmation email when registering a team.
    /// </summary>
    [JsonComment("If true, bank details are part of the confirmation email when registering a team.")]
    public bool ShowBankDetailsInConfirmationEmail { get; set; }

    /// <summary>
    /// The name of the payment recipient, usually the organization name.
    /// </summary>
    [JsonComment("The name of the payment recipient, usually the organization name.")]
    public string Recipient { get; set; } = string.Empty;

    /// <summary>
    /// The name of the bank where a payment is directed.
    /// </summary>
    [JsonComment("The name of the bank where a payment is directed.")]
    public string BankName { get; set; } = string.Empty;

    /// <summary>
    /// The BIC number of the bank account.
    /// </summary>
    [JsonComment("The BIC number of the bank account.")]
    public string Bic { get; set; } = string.Empty;

    /// <summary>
    /// The IBAN number of the bank.
    /// </summary>
    [JsonComment("The IBAN number of the bank.")]
    public string Iban { get; set; } = string.Empty;

    /// <summary>
    /// The participation fee, may be zero.
    /// </summary>

    [JsonComment("The participation fee, may be zero.")]
    public decimal Amount { get; set; }

    /// <summary>
    /// The currency for the participation fee.
    /// </summary>
    [JsonComment("The currency for the participation fee.")]
    public string Currency { get; set; } = string.Empty;
}
