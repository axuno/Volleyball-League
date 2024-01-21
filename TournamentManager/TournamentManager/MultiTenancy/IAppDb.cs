using TournamentManager.Data;

namespace TournamentManager.MultiTenancy;

public interface IAppDb
{
    /// <summary>
    /// Provides database-specific settings.
    /// </summary>
    IDbContext DbContext { get; }

    GenericRepository GenericRepository { get; }
    ManagerOfTeamRepository ManagerOfTeamRepository { get; }
    MatchRepository MatchRepository { get; }
    PlayerInTeamRepository PlayerInTeamRepository { get; }
    RoundRepository RoundRepository { get; }
    TeamInRoundRepository TeamInRoundRepository { get; }
    TeamRepository TeamRepository { get; }
    TournamentRepository TournamentRepository { get; }
    RankingRepository RankingRepository { get; }
    RoleRepository RoleRepository { get; }
    UserRepository UserRepository { get; }
    UserRoleRepository UserRoleRepository { get; }
    UserClaimRepository UserClaimRepository { get; }
    UserLoginRepository UserLoginRepository { get; }
    UserTokenRepository UserTokenRepository { get; }
    VenueRepository VenueRepository { get; }
    ExcludedMatchDateRepository ExcludedMatchDateRepository { get; }
    AvailableMatchDateRepository AvailableMatchDateRepository { get; }
}
