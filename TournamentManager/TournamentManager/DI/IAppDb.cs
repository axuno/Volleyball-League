namespace TournamentManager.Data
{
    /// <summary>
    /// Interface for accessing the repositories.
    /// </summary>
    public interface IAppDb
    {
        /// <summary>
        /// The <see cref="IDbContext"/> instance to be used to access the repositories.
        /// </summary>
        IDbContext DbContext { get; }
    }
}