namespace TournamentManager.Data
{
    /// <summary>
    /// Interface used for DB access resolvers./>
    /// </summary>
    public interface IDbContextResolver
    {
        /// <summary>
        /// Resolves the <see cref="IDbContext"/> from the <see cref="DbContextList"/> which fits to the organization key.
        /// </summary>
        /// <param name="organizationKey"></param>
        /// <returns></returns>
        IDbContext Resolve(string organizationKey);
    }
}