#nullable enable

namespace TournamentManager.MultiTenancy;

/// <summary>
/// The class contains all configuration data for an organization.
/// </summary>
public class OrganizationContext : IOrganizationContext
{
    /// <summary>
    /// The <see cref="TournamentManager.MultiTenancy.ITenant"/> corresponding to this context.
    /// </summary>
    [YAXLib.Attributes.YAXDontSerialize]
    public ITenant? Tenant { get; set; }
        
    /// <summary>
    /// The full name of the organization.
    /// </summary>
    [YAXLib.Attributes.YAXComment("The full name of the organization.")]
    public virtual string Name { get; set; } = string.Empty;

    /// <summary>
    /// The short version of the organization's name.
    /// </summary>
    [YAXLib.Attributes.YAXComment("The short version of the organization's name.")]
    public virtual string ShortName { get; set; } = string.Empty;

    /// <summary>
    /// A description of the organization.
    /// </summary>
    [YAXLib.Attributes.YAXComment("")]
    public virtual string Description { get; set; } = string.Empty;
        
    /// <summary>
    /// The homepage for the organization (NOT the league website).
    /// </summary>
    [YAXLib.Attributes.YAXComment("The homepage for the organization (NOT the league website).")]
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
    [YAXLib.Attributes.YAXComment("If true, bank details are part of the confirmation email when registering a team.")]
    public bool ShowBankDetailsInConfirmationEmail { get; set; }
    /// <summary>
    /// The name of the payment recipient, usually the organization name.
    /// </summary>
    [YAXLib.Attributes.YAXComment("The name of the payment recipient, usually the organization name.")]
    public string Recipient { get; set; } = string.Empty;
    /// <summary>
    /// The name of the bank where a payment is directed.
    /// </summary>
    [YAXLib.Attributes.YAXComment("The name of the bank where a payment is directed.")]
    public string BankName { get; set; } = string.Empty;
    /// <summary>
    /// The BIC number of the bank account.
    /// </summary>
    [YAXLib.Attributes.YAXComment("The BIC number of the bank account.")]
    public string Bic { get; set; } = string.Empty;
    /// <summary>
    /// The IBAN number of the bank.
    /// </summary>
    [YAXLib.Attributes.YAXComment("The IBAN number of the bank.")]
    public string Iban { get; set; } = string.Empty;
    /// <summary>
    /// The participation fee, may be zero.
    /// </summary>
    [YAXLib.Attributes.YAXComment("The participation fee, may be zero.")]
    public decimal Amount { get; set; }
    /// <summary>
    /// The currency for the participation fee.
    /// </summary>
    [YAXLib.Attributes.YAXComment("The currency for the participation fee.")]
    public string Currency { get; set; } = string.Empty;
}