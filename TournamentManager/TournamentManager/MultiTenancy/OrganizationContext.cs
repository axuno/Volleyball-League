#nullable enable

namespace TournamentManager.MultiTenancy
{
    /// <summary>
    /// The class contains all configuration data for an organization.
    /// </summary>
    public class OrganizationContext : IOrganizationContext
    {
        /// <summary>
        /// The <see cref="TournamentManager.MultiTenancy.ITenant"/> corresponding to this context.
        /// </summary>
        [YAXLib.YAXDontSerialize]
        public ITenant? Tenant { get; set; }
        
        /// <summary>
        /// The full name of the organization.
        /// </summary>
        public virtual string Name { get; set; } = string.Empty;

        /// <summary>
        /// The short version of the organization's name.
        /// </summary>
        public virtual string ShortName { get; set; } = string.Empty;

        /// <summary>
        /// A description of the organization.
        /// </summary>
        public virtual string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// The homepage for the organization. E.g. used in emails, ics-calendars
        /// </summary>
        public string HomepageUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// Bank details of the organization, e.g. for payments of participation fees.
        /// </summary>
        public BankDetails Bank { get; set; } = new BankDetails();
    }
    
    /// <summary>
    /// Bank details of the organization, e.g. for payments of participation fees.
    /// </summary>
    public class BankDetails
    {
        /// <summary>
        /// If <see langword="true"/>, bank details are part of the confirmation email when registering a team.
        /// </summary>
        public bool ShowBankDetailsInConfirmationEmail { get; set; }
        /// <summary>
        /// The name of the payment recipient, usually the organization name.
        /// </summary>
        public string Recipient { get; set; } = string.Empty;
        /// <summary>
        /// The name of the bank where a payment is directed.
        /// </summary>
        public string BankName { get; set; } = string.Empty;
        /// <summary>
        /// The BIC number of the bank account.
        /// </summary>
        public string Bic { get; set; } = string.Empty;
        /// <summary>
        /// The IBAN number of the bank.
        /// </summary>
        public string Iban { get; set; } = string.Empty;
        /// <summary>
        /// The participation fee, may be zero.
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// The currency for the participation fee.
        /// </summary>
        public string Currency { get; set; } = string.Empty;
    }
}
