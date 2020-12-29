using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using TournamentManager.MultiTenancy;

namespace TournamentManager.Data
{
    /// <summary>
    /// Resolves the <see cref="OrganizationContext"/> for a given organization key.
    /// </summary>
    public class OrganizationContextResolver : List<Data.OrganizationContext>, IOrganizationContextResolver
    {
        private readonly ILogger<OrganizationContextResolver> _logger;
        private readonly TenantStore _tenantStore;

        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="tenantStore">An instance of <see cref="TournamentManager.MultiTenancy.TenantStore"/>.</param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/> to use.</param>
        public OrganizationContextResolver(TenantStore tenantStore, ILogger<OrganizationContextResolver> logger)
        {
            _logger = logger;
            _tenantStore = tenantStore;
            FillOrganizationContext();
        }

        private void FillOrganizationContext()
        {
            try
            {
                foreach (var tenant in _tenantStore.GetTenants().Values)
                {
                    var orgCtx = new TournamentManager.Data.OrganizationContext
                    {
                        OrganizationKey = tenant.Identifier,
                        Guid = tenant.Guid,
                        Name = tenant.OrganizationContext.Name,
                        ShortName = tenant.OrganizationContext.ShortName,
                        Description = tenant.OrganizationContext.Description,
                        HomepageUrl = tenant.OrganizationContext.HomepageUrl,
                        ApplicationAllowed = tenant.TournamentContext.ApplicationAllowed,
                        ApplicationDeadline = tenant.TournamentContext.ApplicationDeadline,
                        ApplicationTournamentId = tenant.TournamentContext.ApplicationTournamentId,
                        MapTournamentId = tenant.TournamentContext.MapTournamentId,
                        MatchPlanTournamentId = tenant.TournamentContext.MatchPlanTournamentId,
                        MatchResultTournamentId = tenant.TournamentContext.MatchPlanTournamentId,
                        TeamTournamentId = tenant.TournamentContext.TeamTournamentId,
                        Bank = new BankDetails
                        {
                            Amount = tenant.OrganizationContext.Bank.Amount,
                            BankName = tenant.OrganizationContext.Bank.BankName,
                            Bic = tenant.OrganizationContext.Bank.Bic,
                            Currency = tenant.OrganizationContext.Bank.Currency,
                            Iban = tenant.OrganizationContext.Bank.Iban,
                            Recipient = tenant.OrganizationContext.Bank.Recipient,
                            ShowBankDetailsInConfirmationEmail = tenant.OrganizationContext.Bank.ShowBankDetailsInConfirmationEmail
                        },
                        Email = new Email
                        {
                            ContactFrom = new MailAddress { Address = tenant.SiteContext.Email.ContactFrom.Address, DisplayName = tenant.SiteContext.Email.ContactFrom.DisplayName},
                            ContactTo = new MailAddress { Address = tenant.SiteContext.Email.ContactTo.Address, DisplayName = tenant.SiteContext.Email.ContactTo.DisplayName},
                            GeneralBcc = new MailAddress { Address = tenant.SiteContext.Email.GeneralBcc.Address, DisplayName = tenant.SiteContext.Email.GeneralBcc.DisplayName},
                            GeneralFrom = new MailAddress { Address = tenant.SiteContext.Email.GeneralFrom.Address, DisplayName = tenant.SiteContext.Email.GeneralFrom.DisplayName},
                            GeneralTo = new MailAddress { Address = tenant.SiteContext.Email.GeneralTo.Address, DisplayName = tenant.SiteContext.Email.GeneralTo.DisplayName}
                        },
                        FixtureRuleSet = new FixtureRuleSet
                        {
                            CheckForExcludedMatchDateTime = tenant.TournamentContext.FixtureRuleSet.CheckForExcludedMatchDateTime,
                            PlannedDurationOfMatch = tenant.TournamentContext.FixtureRuleSet.PlannedDurationOfMatch,
                            PlannedMatchDateTimeMustBeSet = tenant.TournamentContext.FixtureRuleSet.PlannedMatchDateTimeMustBeSet,
                            PlannedMatchTimeMustStayInCurrentLegBoundaries = tenant.TournamentContext.FixtureRuleSet.PlannedMatchTimeMustStayInCurrentLegBoundaries,
                            PlannedVenueMustBeSet = tenant.TournamentContext.FixtureRuleSet.PlannedVenueMustBeSet,
                            RegularMatchStartTime = new RegularMatchStartTime {MinDayTime = tenant.TournamentContext.FixtureRuleSet.RegularMatchStartTime.MinDayTime, MaxDayTime = tenant.TournamentContext.FixtureRuleSet.RegularMatchStartTime.MaxDayTime},
                            UseOnlyDatePartForTeamFreeBusyTimes = tenant.TournamentContext.FixtureRuleSet.UseOnlyDatePartForTeamFreeBusyTimes
                        },
                        MaxDaysForResultCorrection = tenant.TournamentContext.MaxDaysForResultCorrection,
                        TeamRuleSet = new TeamRules
                        {
                            HomeMatchTime = new HomeMatchTime
                            {
                                DaysOfWeekRange = tenant.TournamentContext.TeamRuleSet.HomeMatchTime.DaysOfWeekRange,
                                ErrorIfNotInDaysOfWeekRange = tenant.TournamentContext.TeamRuleSet.HomeMatchTime.ErrorIfNotInDaysOfWeekRange,
                                IsEditable = tenant.TournamentContext.TeamRuleSet.HomeMatchTime.IsEditable,
                                MustBeSet = tenant.TournamentContext.TeamRuleSet.HomeMatchTime.MustBeSet
                            }
                        },
                        Photos = new Photos
                        {
                            PersonDefaultFilename = tenant.SiteContext.Photos.PeopleDefaultFilename,
                            PersonPhotoFolder = tenant.SiteContext.Photos.PeoplePhotoFolder,
                            TeamDefaultFilename = tenant.SiteContext.Photos.TeamDefaultFilename,
                            TeamPhotoFolder = tenant.SiteContext.Photos.TeamPhotoFolder
                        },
                        AppDb = new AppDb(tenant.DbContext)
                    };
                    Add(orgCtx);
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, $"Error initializing the {nameof(OrganizationContext)}");
                throw;
            }
        }

        /// <summary>
        /// Resolves the <see cref="OrganizationContext"/> for a given organization key.
        /// </summary>
        /// <param name="organizationKey">The organization key referring to the <see cref="OrganizationContext"/></param>
        /// <returns>Returns the <see cref="OrganizationContext"/> for the organization key.</returns>
        /// <exception cref="ArgumentException">Throws, when the organization key cannot be mapped and no default <see cref="OrganizationContext"/> exists.</exception>
        public OrganizationContext Resolve(string organizationKey)
        {
            try
            {
                var organizationContext = this.FirstOrDefault(c => c.OrganizationKey.Equals(organizationKey, StringComparison.InvariantCultureIgnoreCase)) ??
                                          this.FirstOrDefault(c => c.OrganizationKey == string.Empty);

                if (organizationContext == null) throw new ArgumentException($"{nameof(organizationKey)} '{organizationKey}' is unknown and no default existing.");
                _logger.LogTrace($"{nameof(OrganizationContext)} resolved for key '{organizationContext.OrganizationKey}'.");
                return organizationContext;
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, $"Error resolving the {nameof(OrganizationContext)}");
                throw;
            }
        }

        /// <summary>
        /// Shallow-copies all properties of <see cref="OrganizationContext"/> to a class derived from <see cref="OrganizationContext"/>.
        /// </summary>
        /// <typeparam name="T">The class which must be derived from <see cref="OrganizationContext"/>.</typeparam>
        /// <param name="derivedClassInstance">A class instance derived from <see cref="OrganizationContext"/>.</param>
        /// <param name="organizationKey"></param>
        /// <returns>Returns the <see cref="derivedClassInstance"/> with all properties from the base class set.</returns>
        public T CopyContextTo<T>(T derivedClassInstance, string organizationKey) where T:TournamentManager.Data.OrganizationContext
        {
            var organizationContext = Resolve(organizationKey);
            derivedClassInstance.AppDb = organizationContext.AppDb;
            derivedClassInstance.ApplicationAllowed = organizationContext.ApplicationAllowed;
            derivedClassInstance.ApplicationDeadline = organizationContext.ApplicationDeadline;
            derivedClassInstance.ApplicationTournamentId = organizationContext.ApplicationTournamentId;
            derivedClassInstance.Bank = organizationContext.Bank;
            derivedClassInstance.Description = organizationContext.Description;
            derivedClassInstance.Email = organizationContext.Email;
            derivedClassInstance.Guid = organizationContext.Guid;
            derivedClassInstance.HomepageUrl = organizationContext.HomepageUrl;
            derivedClassInstance.MapTournamentId = organizationContext.MapTournamentId;
            derivedClassInstance.MatchPlanTournamentId = organizationContext.MatchPlanTournamentId;
            derivedClassInstance.MatchResultTournamentId = organizationContext.MatchResultTournamentId;
            derivedClassInstance.FixtureRuleSet = organizationContext.FixtureRuleSet;
            derivedClassInstance.MaxDaysForResultCorrection = organizationContext.MaxDaysForResultCorrection;
            derivedClassInstance.TeamRuleSet = organizationContext.TeamRuleSet;
            derivedClassInstance.OrganizationKey = organizationContext.OrganizationKey;
            derivedClassInstance.Name = organizationContext.Name;
            derivedClassInstance.Photos = organizationContext.Photos;
            derivedClassInstance.ShortName = organizationContext.ShortName;
            derivedClassInstance.TeamTournamentId = organizationContext.TeamTournamentId;

            return derivedClassInstance;
        }
    }
}