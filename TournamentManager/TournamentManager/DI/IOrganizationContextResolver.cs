using System.Collections.Generic;

namespace TournamentManager.Data
{
    /// <summary>
    /// Interface used for DB access resolvers./>
    /// </summary>
    public interface IOrganizationContextResolver
    {
        /// <summary>
        /// Resolves the <see cref="OrganizationContext"/> from a <see cref="List{OrganizationContext}"/> which fits to the organization key.
        /// </summary>
        /// <param name="organizationKey"></param>
        /// <returns></returns>
        OrganizationContext Resolve(string organizationKey);
    }
}