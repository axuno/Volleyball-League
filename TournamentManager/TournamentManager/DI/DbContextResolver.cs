using System;
using System.Linq;

namespace TournamentManager.Data
{
    /// <summary>
    /// Finds the <see cref="IDbContext"/> in a <see cref="DbContextList"/> which fits to the organization's key.
    /// </summary>
    public class DbContextResolver : IDbContextResolver
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="dbContextList"></param>
        public DbContextResolver(DbContextList dbContextList)
        {
            DbContextList = dbContextList;
        }

        /// <summary>
        /// Resolves the organization key from the <see cref="DbContextList"/>.
        /// </summary>
        /// <returns>Returns the <see cref="IDbContext"/> for the key, if found. Otherwise a default <see cref="IDbContext"/> (key is empty string) is returned.</returns>
        /// <exception cref="ArgumentException">Throws, neither the key nor the empty string default can be resolved.</exception>
        public IDbContext Resolve(string organizationKey)
        {
            return Resolve(organizationKey, DbContextList);
        }

        private static IDbContext Resolve(string organizationKey, DbContextList dbContextList)
        {
            var dbContext = dbContextList.FirstOrDefault(m => m.OrganizationKey == organizationKey) ??
                           dbContextList.FirstOrDefault(m => m.OrganizationKey == string.Empty);

            if (dbContext == null) throw new ArgumentException($"{nameof(organizationKey)} '{organizationKey}' is unknown and no default existing.");

            return dbContext;
        }

        /// <summary>
        /// Checks whether the <see cref="organizationKey"/> value exists in the <see cref="DbContextList"/> list.
        /// </summary>
        /// <returns>Returns true, if the <see cref="organizationKey"/>  value exists in the <see cref="DbContextList"/> list, else false.</returns>
        public bool IsValid(string organizationKey)
        {
            try
            {
                return Resolve(organizationKey) != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the <see cref="DbContextList"/> which the <see cref="DbContextResolver"/> uses to resolve the organization key to corresponding <see cref="IDbContext"/> list entry.
        /// </summary>
        public DbContextList DbContextList { get; }
    }
}