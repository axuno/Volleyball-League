namespace TournamentManager.Data
{
    /// <summary>
    /// Provides access to database repositories.
    /// </summary>
    public class AppDb : IAppDb
    {
        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="dbContext"></param>
        public AppDb(IDbContext dbContext)
        {
            DbContext = dbContext;
        }

        /// <summary>
        /// Provides database-specific settings.
        /// Usually instances are part of a <see cref="DbContextList"/>, created from a configuration file.
        /// </summary>
        public virtual IDbContext DbContext { get; private set; }

        public virtual GenericRepository GenericRepository => new GenericRepository(DbContext);
        public virtual ManagerOfTeamRepository ManagerOfTeamRepository => new ManagerOfTeamRepository(DbContext);
        public virtual MatchRepository MatchRepository => new MatchRepository(DbContext);
        public virtual PlayerInTeamRepository PlayerInTeamRepository => new PlayerInTeamRepository(DbContext);
        public virtual RoundRepository RoundRepository => new RoundRepository(DbContext);
        public virtual TeamInRoundRepository TeamInRoundRepository => new TeamInRoundRepository(DbContext);
        public virtual TeamRepository TeamRepository => new TeamRepository(DbContext);
        public virtual TournamentRepository TournamentRepository => new TournamentRepository(DbContext);
        public virtual RankingRepository RankingRepository => new RankingRepository(DbContext);
        public virtual RoleRepository RoleRepository => new RoleRepository(DbContext);
        public virtual UserRepository UserRepository => new UserRepository(DbContext);
        public virtual UserRoleRepository UserRoleRepository => new UserRoleRepository(DbContext);
        public virtual UserClaimRepository UserClaimRepository => new UserClaimRepository(DbContext);
        public virtual UserLoginRepository UserLoginRepository => new UserLoginRepository(DbContext);
        public virtual UserTokenRepository UserTokenRepository => new UserTokenRepository(DbContext);
        public virtual VenueRepository VenueRepository => new VenueRepository(DbContext);
        public virtual ExcludedMatchDateRepository ExcludedMatchDateRepository => new ExcludedMatchDateRepository(DbContext);
        public virtual AvailableMatchDateRepository AvailableMatchDateRepository => new AvailableMatchDateRepository(DbContext);
    }
}
