using System.Data;
using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.LinqSupportClasses;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.QuerySpec;
using SD.LLBLGen.Pro.QuerySpec.Adapter;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.FactoryClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.Linq;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.Ranking;

namespace TournamentManager.Data;

/// <summary>
/// Class for Ranking related data operations
/// </summary>
public class RankingRepository
{
    private readonly ILogger _logger = AppLogging.CreateLogger<RankingRepository>();
    private readonly MultiTenancy.IDbContext _dbContext;

    public RankingRepository(MultiTenancy.IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public virtual async Task ReplaceAsync(RankingList rankingList, long roundId, CancellationToken cancellationToken)
    {
        var rankingColl = new EntityCollection<RankingEntity>(new RankingEntityFactory());
        using var da = _dbContext.GetNewAdapter();

        var transactionName =
            string.Concat(nameof(RankingRepository), nameof(ReplaceAsync), Guid.NewGuid().ToString("N"));

        try
        {
            // Todo: TournamentId is defined via the Round - remove TournamentId from the Ranking table. View RankingList does not depend on TournamentId already.
            var tournamentId = await GetTournamentIdOfRoundAsync(roundId, cancellationToken);

            // Fetch existing ranking entities for the round for deletion
            var existingEntities = new EntityCollection<RankingEntity>
            {
                RemovedEntitiesTracker = new EntityCollection()
            };
            var qp = new QueryParameters
            {
                CollectionToFetch = existingEntities,
                FilterToUse = RankingFields.RoundId == roundId,
                ExcludedIncludedFields = new IncludeFieldsList(RankingFields.Id)
            };
            await da.FetchEntityCollectionAsync(qp, cancellationToken);

            // Create ranking entities
            foreach (var rank in rankingList)
            {
                rankingColl.Add(new RankingEntity
                {
                    TournamentId = tournamentId,
                    RoundId = roundId,
                    Rank = rank.Number,
                    ValuationDate = rankingList.UpperDateLimit,
                    TeamId = rank.TeamId,
                    MatchPointsWon = rank.MatchPoints.Home,
                    MatchPointsLost = rank.MatchPoints.Guest,
                    SetPointsWon = rank.SetPoints.Home,
                    SetPointsLost = rank.SetPoints.Guest,
                    BallPointsWon = rank.BallPoints.Home,
                    BallPointsLost = rank.BallPoints.Guest,
                    MatchesPlayed = rank.MatchesPlayed,
                    MatchesToPlay = rank.MatchesToPlay,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedOn = rankingList.LastUpdatedOn
                });
            }

            // Start a transaction to store in the database
            await da.StartTransactionAsync(IsolationLevel.ReadCommitted, transactionName, cancellationToken);

            // Remove all existing ranking entities for the round
            await da.DeleteEntityCollectionAsync(existingEntities, cancellationToken);

            // Save the new ranking entities
            await da.SaveEntityCollectionAsync(rankingColl, false, false, cancellationToken);
            await da.CommitAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Error updating Ranking in transaction: RoundId={roundId}", roundId);

            if (da.IsTransactionInProgress)
                da.Rollback();

            throw;
        }
    }

    private async Task<long> GetTournamentIdOfRoundAsync(long roundId, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        var metaData = new LinqMetaData(da);
        return await (from r in metaData.Round
            where r.Id == roundId
            select r.TournamentId).ExecuteAsync<long>(cancellationToken);
    }

    public virtual async Task<List<RankingListRow>> GetRankingListAsync(IPredicateExpression filter, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        return (await da.FetchQueryAsync(
            new QueryFactory().RankingList.Where(filter).OrderBy(RankingListFields.TournamentId.Ascending(), RankingListFields.RoundName.Ascending(), RankingListFields.Rank.Ascending()), cancellationToken));
    }

    public virtual async Task<List<RankingEntity>> GetRankingAsync(IPredicateExpression filter,
        CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        return (await da.FetchQueryAsync<RankingEntity>(
            new QueryFactory().Ranking.Where(filter)
            ,cancellationToken)).Cast<RankingEntity>().Distinct().ToList();
    }
}
