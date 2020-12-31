namespace TournamentManager.MultiTenancy
{
    /// <summary>
    /// The class keeps all tenant-specific data for the <see cref="TournamentManager"/>.
    /// </summary>
    public interface IManagerTenantContext : ITenant
    {
        /// <summary>
        /// Gets or sets the <see cref="IDbContext"/> for the <see cref="IManagerTenantContext"/>.
        /// </summary>
        public DbContext DbContext { get; set; }
        
        /// <summary>
        /// Gets or sets the <see cref="ITournamentContext"/> for the <see cref="IManagerTenantContext"/>.
        /// </summary>
        public TournamentContext TournamentContext { get; set; }

        /// <summary>
        /// Stores the current tenant instance to the file system.
        /// </summary>
        /// <param name="path"></param>
        public void SerializeToFile(string path);
    }
}