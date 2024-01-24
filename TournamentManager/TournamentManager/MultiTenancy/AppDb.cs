using TournamentManager.Data;

namespace TournamentManager.MultiTenancy;

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
    /// </summary>
    public virtual IDbContext DbContext { get; }

    public virtual GenericRepository GenericRepository => new(DbContext);
    public virtual ManagerOfTeamRepository ManagerOfTeamRepository => new(DbContext);
    public virtual MatchRepository MatchRepository => new(DbContext);
    public virtual PlayerInTeamRepository PlayerInTeamRepository => new(DbContext);
    public virtual RoundRepository RoundRepository => new(DbContext);
    public virtual TeamInRoundRepository TeamInRoundRepository => new(DbContext);
    public virtual TeamRepository TeamRepository => new(DbContext);
    public virtual TournamentRepository TournamentRepository => new(DbContext);
    public virtual RankingRepository RankingRepository => new(DbContext);
    public virtual RoleRepository RoleRepository => new(DbContext);
    public virtual UserRepository UserRepository => new(DbContext);
    public virtual UserRoleRepository UserRoleRepository => new(DbContext);
    public virtual UserClaimRepository UserClaimRepository => new(DbContext);
    public virtual UserLoginRepository UserLoginRepository => new(DbContext);
    public virtual UserTokenRepository UserTokenRepository => new(DbContext);
    public virtual VenueRepository VenueRepository => new(DbContext);
    public virtual ExcludedMatchDateRepository ExcludedMatchDateRepository => new(DbContext);
    public virtual AvailableMatchDateRepository AvailableMatchDateRepository => new(DbContext);
}
