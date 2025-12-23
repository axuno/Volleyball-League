using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.QuerySpec;
using SD.LLBLGen.Pro.QuerySpec.Adapter;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL;
using TournamentManager.DAL.FactoryClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.TypedViewClasses;

namespace TournamentManager.Data;

public class TeamRepository
{
    private readonly ILogger _logger = AppLogging.CreateLogger<TeamRepository>();
    private readonly MultiTenancy.IDbContext _dbContext;
    public TeamRepository(MultiTenancy.IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public virtual async Task<List<TeamVenueRoundRow>> GetTeamVenueRoundInfoAsync(IPredicateExpression filter, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        return await da.FetchQueryAsync<TeamVenueRoundRow>(
            new QueryFactory().TeamVenueRound.Where(filter), cancellationToken);
    }

    /// <summary>
    /// Gets team with players, managers, the team in round mapping, and the round.
    /// Abandoned teams (without players or managers) are excluded.
    /// </summary>
    public virtual async Task<List<TeamUserRoundRow>> GetTeamUserRoundInfosAsync(IPredicateExpression filter, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        return await da.FetchQueryAsync<TeamUserRoundRow>(
            new QueryFactory().TeamUserRound.Where(filter), cancellationToken);
    }

    /// <summary>
    /// Gets the latests tournament a team has participated, along with other
    /// information about the round the team was playing.
    /// <remarks>
    /// <list type="bullet">
    /// <item>For each team in a tournament, it is checked if that team does NOT participate in the "next" tournament (as defined by NextTournamentId).</item>
    /// <item>If the team does participate in the next tournament, the current row is excluded.</item>
    /// <item>This way we get the latest tournament a team has played in.</item>
    /// </list>
    /// <code>
    /// -- SQL Query to get the latest tournament a team has played in
    /// 
    /// WITH TeamTournaments AS (
    ///   SELECT
    ///     t.Id AS TeamId,
    ///     r.TournamentId
    ///   FROM TeamInRound tir
    ///     JOIN Team t ON tir.TeamId = t.Id
    ///     JOIN Round r ON tir.RoundId = r.Id
    /// )
    /// SELECT
    ///   tt.TournamentId,
    ///   tt.TeamId
    /// FROM TeamTournaments tt
    /// WHERE NOT EXISTS (
    ///   SELECT 1
    ///   FROM TeamInRound tir2
    ///     JOIN Round r2 ON tir2.RoundId = r2.Id
    ///     JOIN Tournament t2 ON r2.TournamentId = t2.Id
    ///   WHERE tir2.TeamId = tt.TeamId
    ///     AND t2.Id = (SELECT NextTournamentId FROM Tournament WHERE Id = tt.TournamentId)
    /// )
    /// GROUP BY tt.TeamId, tt.TournamentId
    /// </code>
    /// </remarks>
    /// </summary>
    public virtual async Task<List<LatestTeamTournamentRow>> GetLatestTeamTournamentAsync(IPredicateExpression filter, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        return await da.FetchQueryAsync<LatestTeamTournamentRow>(
            new QueryFactory().LatestTeamTournament.Where(filter), cancellationToken);
    }

    public virtual async Task<List<TeamEntity>> GetTeamEntitiesAsync(PredicateExpression filter, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        return (await da.FetchQueryAsync<TeamEntity>(
            new QueryFactory().Team.Where(filter), cancellationToken)).Cast<TeamEntity>().ToList();
    }

    public virtual async Task<TeamEntity?> GetTeamEntityAsync(PredicateExpression filter, CancellationToken cancellationToken)
    {
        return (await GetTeamEntitiesAsync(filter, cancellationToken)).FirstOrDefault();
    }

    /// <summary>
    /// Gets all rounds and their teams for a tournament prefetched.
    /// </summary>
    /// <param name="tournament">Tournament entity to use.</param>
    /// <param name="cancellationToken"></param>
    /// <example><![CDATA[
    /// RoundEntity round = tir.Where(t => t.Round.Name == "A").First().Round;
    /// TeamEntity team = tir.Where(t => t.Team.Name == "Die Unglaublichen").First().Team;
    /// RoundEntity[] rounds = tir.Select(x => x.Round).Distinct().ToArray();
    /// TeamEntity[] teamsOfRound = tir.Where(x => x.Round.Name == "F").Select(y => y.Team).ToArray();
    /// ]]></example>
    /// <returns>Returns the TeamInRoundEntity for the tournament id.</returns>
    public virtual async Task<EntityCollection<TeamInRoundEntity>> GetTeamsAndRoundsAsync(TournamentEntity tournament, CancellationToken cancellationToken)
    {
        IPrefetchPath2 prefetchPathTeamInRound = new PrefetchPath2(EntityType.TeamInRoundEntity)
        {
            TeamInRoundEntity.PrefetchPathRound,
            TeamInRoundEntity.PrefetchPathTeam
        };

        var filter = new RelationPredicateBucket();
        filter.Relations.Add(TeamInRoundEntity.Relations.RoundEntityUsingRoundId);
        filter.PredicateExpression.Add(RoundFields.TournamentId == tournament.Id);

        var tir = new EntityCollection<TeamInRoundEntity>();

        var qp = new QueryParameters
        {
            CollectionToFetch = tir,
            PrefetchPathToUse = prefetchPathTeamInRound,
            RelationsToUse = filter.Relations,
            FilterToUse = filter.PredicateExpression
        };

        using var da = _dbContext.GetNewAdapter();
        await da.FetchEntityCollectionAsync(qp, cancellationToken);

        _logger.LogDebug("{TeamCount} found for {TournamentId}", tir.Count, tournament.Id);

        return tir;
    }

    public virtual async Task<bool> TeamExistsAsync(long teamId, CancellationToken cancellationToken)
    {
        return (await GetTeamNamesAsync(new(TeamFields.Id == teamId), cancellationToken)).Any();
    }

    public virtual async Task<IList<string>> GetTeamNamesAsync(PredicateExpression filter, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        return await da.FetchQueryAsync<string>(new QueryFactory().Create().Select(() => TeamFields.Name.ToValue<string>()).Where(filter), cancellationToken);
    }

    /// <summary>
    /// Gets the first <see cref="TeamEntity.Name"/> from the database, which equals the sanitized representation
    /// of the team name and which does not have the same <see cref="TeamEntity.Id"/>.
    /// </summary>
    /// <param name="teamEntity">The <see cref="TeamEntity"/> to use for the query.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns the first <see cref="TeamEntity.Name"/> from the database, which equals the sanitized representation
    /// of the team name and which does not have the same <see cref="TeamEntity.Id"/> </returns>
    public virtual async Task<string?> TeamNameExistsAsync(TeamEntity teamEntity, CancellationToken cancellationToken)
    {
        static string Sanitize(string name)
        {
            var result = new System.Text.StringBuilder();
            foreach (var c in name)
            {
                if(char.IsLetterOrDigit(c)) result.Append(char.ToUpperInvariant(c));
            }
            return result.ToString();
        }

        using var da = _dbContext.GetNewAdapter();
        var teamNames = await da.FetchQueryAsync<string>(new QueryFactory().Create().Select(() => TeamFields.Name.ToValue<string>()).Where(TeamFields.Id != teamEntity.Id), cancellationToken);
        return teamNames.Find(tn => Sanitize(tn).Equals(Sanitize(teamEntity.Name)));
    }
}
