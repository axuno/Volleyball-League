namespace TournamentManager.MultiTenancy
{
    /// <summary>
    /// Interface for accessing the repositories.
    /// </summary>
    public interface IAppDb
    {
        /// <summary>
        /// The <see cref="MultiTenancy.IDbContext"/> instance to be used to access the repositories.
        /// </summary>
        MultiTenancy.IDbContext DbContext { get; }
    }
}