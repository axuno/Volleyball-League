using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.QuerySpec;
using SD.LLBLGen.Pro.QuerySpec.Adapter;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.FactoryClasses;
using TournamentManager.DAL.HelperClasses;

namespace TournamentManager.Data;

/// <summary>
/// Class for ExcludedMatchDate related data selections
/// </summary>
public class ExcludedMatchDateRepository
{
    private static readonly ILogger _logger = AppLogging.CreateLogger<ExcludedMatchDateRepository>();
    private readonly MultiTenancy.IDbContext _dbContext;
    public ExcludedMatchDateRepository(MultiTenancy.IDbContext dbContext)
    {
        _dbContext = dbContext;
        _logger.LogDebug("Repository created. {Repository} {Identifier}", nameof(TournamentRepository), dbContext.Tenant?.Identifier);
    }

    /// <summary>
    /// Gets the <see cref="EntityCollection{TEntity}"/> of type <see cref="ExcludeMatchDateEntity"/> for a tournament.
    /// </summary>
    /// <param name="tournamentId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns the <see cref="EntityCollection{TEntity}"/> of type <see cref="ExcludeMatchDateEntity"/> for a tournament.</returns>
    public virtual async Task<EntityCollection<ExcludeMatchDateEntity>> GetExcludedMatchDatesAsync (long tournamentId, CancellationToken cancellationToken)
    {
        var excluded = new EntityCollection<ExcludeMatchDateEntity>();
        using var da = _dbContext.GetNewAdapter();
        var qp = new QueryParameters
        {
            CollectionToFetch = excluded,
            FilterToUse = ExcludeMatchDateFields.TournamentId == tournamentId
        };
        await da.FetchEntityCollectionAsync(qp, cancellationToken);
        da.CloseConnection();

        return excluded;
    }

    /// <summary>
    /// Gets the first entry from a list of dates that are excluded as match dates, or <see langword="null"/> if no date is found.
    /// Excluded dates may be related to a tournament, OR a round OR the home team OR the guest team.
    /// If the excluded table row contains a RoundId or a TeamId, it is NOT excluded for the tournament,
    /// but only for the team or round.
    /// </summary>
    /// <remarks>
    /// Same conditions as with <see cref="TournamentManager.Plan.AvailableMatchDates.IsExcludedDate"/>
    /// which uses a cached list of <see cref="ExcludeMatchDateEntity"/>s.
    /// </remarks>
    /// <param name="match">The <see cref="MatchEntity"/> where RoundId and TeamId are taken.</param>
    /// <param name="tournamentId">The TournamentId to filter the result.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns the first <see cref="ExcludeMatchDateEntity"/> which matches the criteria, else <see langword="null"/>.</returns>
    public virtual async Task<ExcludeMatchDateEntity?> GetExcludedMatchDateAsync(MatchEntity match,
        long tournamentId, CancellationToken cancellationToken)
    {
        if (match is not { PlannedStart: not null, PlannedEnd: not null }) return null;

        using var da = _dbContext.GetNewAdapter();
        var tournamentFilter = new PredicateExpression(
                ExcludeMatchDateFields.TournamentId == tournamentId)
            .AddWithAnd(ExcludeMatchDateFields.RoundId.IsNull())
            .AddWithAnd(ExcludeMatchDateFields.TeamId.IsNull())
            // dates overlap
            .AddWithAnd(ExcludeMatchDateFields.DateFrom.LesserEqual(match.PlannedEnd))
            .AddWithAnd(ExcludeMatchDateFields.DateTo.GreaterEqual(match.PlannedStart));

        var roundFilter = new PredicateExpression(
                ExcludeMatchDateFields.TournamentId == tournamentId)
            .AddWithAnd(ExcludeMatchDateFields.RoundId == match.RoundId)
            // dates overlap
            .AddWithAnd(ExcludeMatchDateFields.DateFrom.LesserEqual(match.PlannedEnd))
            .AddWithAnd(ExcludeMatchDateFields.DateTo.GreaterEqual(match.PlannedStart));

        var teamFilter = new PredicateExpression(
                ExcludeMatchDateFields.TournamentId == tournamentId)
            .AddWithAnd(ExcludeMatchDateFields.TeamId == match.HomeTeamId | ExcludeMatchDateFields.TeamId == match.GuestTeamId)
            // dates overlap
            .AddWithAnd(ExcludeMatchDateFields.DateFrom.LesserEqual(match.PlannedEnd))
            .AddWithAnd(ExcludeMatchDateFields.DateTo.GreaterEqual(match.PlannedStart));

        return (await da.FetchQueryAsync(
            new QueryFactory().ExcludeMatchDate.Where(tournamentFilter
                .AddWithOr(roundFilter).AddWithOr(teamFilter)).Limit(1),
            cancellationToken)).Cast<ExcludeMatchDateEntity>().FirstOrDefault();
    }
}
