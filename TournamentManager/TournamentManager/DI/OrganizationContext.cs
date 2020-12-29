using System;
using System.Collections.Generic;
using NLog;
using TournamentManager.MultiTenancy;

namespace TournamentManager.Data
{
    /// <summary>
    /// The class contains all configuration data for an organization.
    /// </summary>
    [Obsolete("Implement with ITenantContext instead.")]
    [YAXLib.YAXSerializeAs("OrganizationContext")]
    public class OrganizationContext
    {
        #region *** Serialization ***

        /// <summary>
        /// Deserializes an XML file to an instance of <see cref="OrganizationContext"/>.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static OrganizationContext DeserializeFromFile(string filename)
        {
            var s = new YAXLib.YAXSerializer(typeof(OrganizationContext));
            return (OrganizationContext)s.DeserializeFromFile(filename);
        }

        /// <summary>
        /// Deserializes an instance of <see cref="OrganizationContext"/> to an XML file.
        /// </summary>
        /// <param name="filename"></param>
        public void SerializeToFile(string filename)
        {
            var s = new YAXLib.YAXSerializer(typeof(OrganizationContext));
            s.SerializeToFile(this, filename);
        }

        #endregion

        /// <summary>
        /// The identifier for this organization.
        /// </summary>
        public virtual Guid Guid { get; set; } = new Guid();

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
        /// Gives access to the repositories.
        /// </summary>
        [YAXLib.YAXDontSerialize]
        public virtual AppDb AppDb { get; set; } = null!;

        /// <summary>
        /// Organization key which can be used as the first path segment (the 'organization' segment) of a request path.
        /// </summary>
        [YAXLib.YAXDontSerialize]
        public virtual string OrganizationKey { get; set; } = string.Empty;

        /// <summary>
        /// The homepage for the organization. E.g. used in emails, ics-calendars
        /// </summary>
        public string HomepageUrl { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the tournament which will be used for new teams' applications.
        /// </summary>
        public long ApplicationTournamentId { get; set; }

        /// <summary>
        /// Return true, if teams' applications are allowed, otherwise false.
        /// </summary>
        [YAXLib.YAXAttributeFor(nameof(ApplicationTournamentId))]
        [YAXLib.YAXSerializeAs("IsAllowed")]
        public bool ApplicationAllowed { get; set; }

        /// <summary>
        /// The ID of the tournament which will be used for to display maps.
        /// </summary>
        public long MapTournamentId { get; set; }
        /// <summary>
        /// The ID of the tournament which will be used to display team data.
        /// </summary>
        public long TeamTournamentId { get; set; }
        /// <summary>
        /// The ID of the tournament which will be used to display the match plan.
        /// </summary>
        public long MatchPlanTournamentId { get; set; }
        /// <summary>
        /// The ID of the tournament which will be used to display match results and tables.
        /// </summary>
        public long MatchResultTournamentId { get; set; }
        /// <summary>
        /// Details for team photos.
        /// </summary>
        public Photos Photos { get; set; } = new Photos();
        /// <summary>
        /// Bank details of the organization, e.g. for payments of participation fees.
        /// </summary>
        public BankDetails Bank { get; set; } = new BankDetails();
        /// <summary>
        /// The deadline for new teams' applications.
        /// </summary>
        public DateTime ApplicationDeadline { get; set; }

        /// <summary>
        /// A set of rules for creating and editing fixtures.
        /// </summary>
        public FixtureRuleSet FixtureRuleSet { get; set; } = new FixtureRuleSet();

        /// <summary>
        ///	The max. number of days after RealStart where results may be changed.
        /// </summary>
        public int MaxDaysForResultCorrection { get; set; }

        /// <summary>
        /// Rules for team master data
        /// </summary>
        public TeamRules TeamRuleSet { get; set; } = new TeamRules();

        /// <summary>
        /// Email contact details for an organization.
        /// </summary>
        public Email Email { get; set; } = new Email();
    }

    /// <summary>
    /// The folders and file names for photos.
    /// </summary>
    public class Photos
    {
        /// <summary>
        /// The Folder for team photos.
        /// </summary>
        public string TeamPhotoFolder { get; set; } = string.Empty;
        /// <summary>
        /// The name of the image file, which will be displayed if no team photo exists.
        /// </summary>
        public string TeamDefaultFilename { get; set; } = string.Empty;
        /// <summary>
        /// The Folder for person photos.
        /// </summary>
        public string PersonPhotoFolder { get; set; } = string.Empty;
        /// <summary>
        /// The name of the image file, which will be displayed if no person photo exists.
        /// </summary>
        public string PersonDefaultFilename { get; set; } = string.Empty;
    }

    /// <summary>
    /// Email contact details for an organization.
    /// </summary>
    public class Email
    {
        /// <summary>
        /// "From" mailbox address for the contact form
        /// </summary>
        public MailAddress ContactFrom { get; set; } = new MailAddress();
        /// <summary>
        /// "To" mailbox address for the contact form
        /// </summary>
        public MailAddress ContactTo { get; set; } = new MailAddress();
        /// <summary>
        /// General "To" mailbox address for emails generated programmatically
        /// </summary>
        public MailAddress GeneralTo { get; set; } = new MailAddress();
        /// <summary>
        /// General "From" mailbox address for emails generated programmatically
        /// </summary>
        public MailAddress GeneralFrom { get; set; } = new MailAddress();
        /// <summary>
        /// General "BCC" mailbox address for emails generated programmatically
        /// </summary>
        [YAXLib.YAXErrorIfMissed(YAXLib.YAXExceptionTypes.Ignore)]
        public MailAddress GeneralBcc { get; set; } = new MailAddress();
    }

    /// <summary>
    /// Email display name and address.
    /// </summary>
    public class MailAddress
    {
        /// <summary>
        /// The display name.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// The email address
        /// </summary>
        public string Address { get; set; } = string.Empty;
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
        [YAXLib.YAXSerializeAs("BIC")] public string Bic { get; set; } = string.Empty;
        /// <summary>
        /// The IBAN number of the bank.
        /// </summary>
        [YAXLib.YAXSerializeAs("IBAN")] public string Iban { get; set; } = string.Empty;
        /// <summary>
        /// The participation fee, may be zero.
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// The currency for the participation fee.
        /// </summary>
        public string Currency { get; set; } = string.Empty;
    }

    public class FixtureRuleSet
    {
        /// <summary>
        /// The time when matches start and end normally (e.g. from 18:00 - 21:00 h)
        /// </summary>
        public RegularMatchStartTime RegularMatchStartTime { get; set; } = new RegularMatchStartTime();

        /// <summary>
        /// The duration which is used to generate fixtures and to determine periods
        /// where a venue is occupied. Default is 2 hours.
        /// </summary>
        public TimeSpan PlannedDurationOfMatch { get; set; } = new TimeSpan(0, 2,0,0);

        /// <summary>
        /// If set to true, when editing a fixture the match time must be set (cannot be null)
        /// </summary>
        public bool PlannedMatchDateTimeMustBeSet { get; set; } = true;

        /// <summary>
        /// If set to true, the planned match time must no include any found <see cref="TournamentManager.DAL.EntityClasses.ExcludeMatchDateEntity"/> entries.
        /// </summary>
        public bool CheckForExcludedMatchDateTime { get; set; } = true;

        /// <summary>
        /// If set to true, the planned match time must stay within the current leg date boundaries.
        /// If false, the planned time must stay with in any leg date boundaries.
        /// </summary>
        public bool PlannedMatchTimeMustStayInCurrentLegBoundaries { get; set; }

        /// <summary>
        /// If set to true, when editing a fixture the venue must be set (cannot be null)
        /// </summary>
        public bool PlannedVenueMustBeSet { get; set; } = true;

        /// <summary>
        /// If true, when checking whether teams already have a match at a certain moment,
        /// only the date will be used (i.e. only 1 match per calendar date).
        /// </summary>
        public bool UseOnlyDatePartForTeamFreeBusyTimes { get; set; } = false;
    }

    /// <summary>
    /// The time when matches should normally start (e.g. from 18:00 - 21:00 h).
    /// To disable checks, set MinDayTime to 00:00:00 and MaxDayTime to 23:59:59
    /// </summary>
    public class RegularMatchStartTime
    {
        /// <summary>
        /// Earliest start time for a match.
        /// </summary>
        public TimeSpan MinDayTime { get; set; } = new TimeSpan(0,18,0,0);
        /// <summary>
        /// Latest start time for a match.
        /// </summary>
        public TimeSpan MaxDayTime { get; set; } = new TimeSpan(0, 21,0,0);
    }

    /// <summary>
    /// Rules for teams' master data
    /// </summary>
    public class TeamRules
    {
        /// <summary>
        /// Rules for teams' home match time
        /// </summary>
        public HomeMatchTime HomeMatchTime { get; set; } = new HomeMatchTime();
    }

    /// <summary>
    /// Rules for the <see cref="HomeMatchTime"/> of a team.
    /// </summary>
    public class HomeMatchTime
    {
        /// <summary>
        /// <see cref="HomeMatchTime"/> will be shown on team forms.
        /// If <see langword="false"/>, <see cref="IsEditable"/>, <see cref="DaysOfWeekRange"/> and <see cref="DisallowOutOfDaysOfWeekRange"/> are irrelevant.
        /// </summary>
        public bool IsEditable { get; set; } = true;
        /// <summary>
        /// The <see cref="HomeMatchTime"/> must be set, i.e. cannot be null/unspecified.
        /// </summary>
        public bool MustBeSet { get; set; } = true;
        /// <summary>
        /// Allowed days of a week
        /// </summary>
        public List<DayOfWeek> DaysOfWeekRange { get; set; } = new List<DayOfWeek>(new[]{DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday});
        /// <summary>
        /// Entries not in <see cref="DaysOfWeekRange"/> are errors (else: warning)
        /// </summary>
        public bool ErrorIfNotInDaysOfWeekRange { get; set; } = false;
    }
}