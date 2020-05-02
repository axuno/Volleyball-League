using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace TournamentManager.Data
{
    /// <summary>
    /// Resolves the <see cref="OrganizationContext"/> for a given organization key.
    /// </summary>
    public class OrganizationContextResolver : List<OrganizationContext>, IOrganizationContextResolver
    {
        private readonly DbContextResolver _dbContextResolver;
        private readonly ILogger<OrganizationContextResolver> _logger;
        private readonly string _configPath;

        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="dbContextResolver">An instance of <see cref="DbContextResolver"/>, filled with a list of type <see cref="DbContextList"/>.</param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/> to use.</param>
        /// <param name="configPath">Path to the organization configuration files.</param>
        public OrganizationContextResolver(DbContextResolver dbContextResolver, ILogger<OrganizationContextResolver> logger, string configPath)
        {
            _dbContextResolver = dbContextResolver;
            _logger = logger;
            _configPath = configPath;
            FillOrganizationContext();
        }

        private void FillOrganizationContext()
        {
            try
            {
                foreach (var dbContext in _dbContextResolver.DbContextList)
                {
                    var appDb = new AppDb(_dbContextResolver.Resolve(dbContext.OrganizationKey));

                    var orgCtx = !string.IsNullOrEmpty(dbContext.OrganizationKey)
                        ? OrganizationContext.DeserializeFromFile(Path.Combine(_configPath,
                            $"LeagueSettings.{dbContext.OrganizationKey}.config"))
                        : new OrganizationContext();

                    orgCtx.OrganizationKey = dbContext.OrganizationKey;
                    orgCtx.AppDb = appDb;

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
                var organizationContext = this.FirstOrDefault(c => c.OrganizationKey == organizationKey) ??
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
        public T CopyContextTo<T>(T derivedClassInstance, string organizationKey) where T:OrganizationContext
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