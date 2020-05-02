using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.LinqSupportClasses;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.QuerySpec;
using SD.LLBLGen.Pro.QuerySpec.Adapter;
using TournamentManager.DAL;
using TournamentManager.DAL.DatabaseSpecific;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.FactoryClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.Linq;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.Plan;

namespace TournamentManager.Data
{
    public class MatchRepository
    {
        private static readonly ILogger _logger = AppLogging.CreateLogger<MatchRepository>();
        private readonly IDbContext _dbContext;

        public MatchRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public virtual async Task<List<CompletedMatchRow>> GetCompletedMatchesAsync(IPredicateExpression filter,
            CancellationToken cancellationToken)
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                return await da.FetchQueryAsync<CompletedMatchRow>(
                    new QueryFactory().CompletedMatch.Where(filter), cancellationToken);
            }
        }

        public virtual async Task<List<MatchToPlayRawRow>> GetMatchesToPlayAsync(IPredicateExpression filter,
            CancellationToken cancellationToken)
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                var mtp = await da.FetchQueryAsync<MatchToPlayRawRow>(
                    new QueryFactory().MatchToPlayRaw.Where(filter), cancellationToken);

                return mtp.ToList();
            }
        }

        public virtual async Task<List<MatchCompleteRawRow>> GetMatchesCompleteAsync(IPredicateExpression filter,
            CancellationToken cancellationToken)
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                var mtp = await da.FetchQueryAsync<MatchCompleteRawRow>(
                    new QueryFactory().MatchCompleteRaw.Where(filter), cancellationToken);

                return mtp.ToList();
            }
        }

        public virtual async Task<List<PlannedMatchRow>> GetPlannedMatchesAsync(IPredicateExpression filter,
            CancellationToken cancellationToken)
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                return await da.FetchQueryAsync<PlannedMatchRow>(
                    new QueryFactory().PlannedMatch.Where(filter), cancellationToken);

                // return await Task.Run(() => GetPlannedMatches(tournamentId), cancellationToken);
            }
        }

        public virtual async Task<MatchReportSheetRow> GetMatchReportSheetAsync(long tournamentId, long id,
            CancellationToken cancellationToken)
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                return (await da.FetchQueryAsync(
                    new QueryFactory().MatchReportSheet.Where(
                        MatchReportSheetFields.TournamentId == tournamentId & MatchReportSheetFields.Id == id),
                    cancellationToken)).FirstOrDefault();
            }
        }

        public virtual async Task<List<CalendarRow>> GetMatchCalendarAsync(long tournamentId, long? matchId,
            long? teamId, long? roundId, CancellationToken cancellationToken)
        {
            Predicate filter;
            if (matchId.HasValue)
            {
                filter = CalendarFields.Id == matchId.Value;
            }
            else if (teamId.HasValue)
            {
                filter = CalendarFields.HomeTeamId == teamId.Value | CalendarFields.GuestTeamId == teamId.Value;
            }
            else if (roundId.HasValue)
            {
                filter = CalendarFields.RoundId == roundId.Value;
            }
            else
            {
                filter = CalendarFields.TournamentId == tournamentId;
            }

            using (var da = _dbContext.GetNewAdapter())
            {
                return (await da.FetchQueryAsync(
                    new QueryFactory().Calendar.Where(filter), cancellationToken));
            }
        }

        public virtual EntityCollection<MatchEntity> GetMatches(long tournamentId)
        {
            var rounds = new TournamentRepository(_dbContext).GetTournamentRounds(tournamentId);

            var roundId = new List<long>(rounds.Count);
            roundId.AddRange(rounds.Select(round => round.Id));

            IRelationPredicateBucket bucket = new RelationPredicateBucket();
            IPredicateExpression roundFilter =
                new PredicateExpression(new FieldCompareRangePredicate(MatchFields.RoundId, null, false,
                    roundId.ToArray()));
            bucket.PredicateExpression.AddWithAnd(roundFilter);

            var matches = new EntityCollection<MatchEntity>();
            using (var da = _dbContext.GetNewAdapter())
            {
                da.FetchEntityCollection(matches, bucket);
                da.CloseConnection();
            }

            return matches;
        }

        public virtual EntityCollection<MatchEntity> GetMatches(RoundEntity round)
        {
            IRelationPredicateBucket bucket = new RelationPredicateBucket();
            IPredicateExpression roundFilter =
                new PredicateExpression(new FieldCompareRangePredicate(MatchFields.RoundId, null, false,
                    new[] {round.Id}));
            bucket.PredicateExpression.AddWithAnd(roundFilter);

            var matches = new EntityCollection<MatchEntity>();
            using (var da = _dbContext.GetNewAdapter())
            {
                da.FetchEntityCollection(matches, bucket);
                da.CloseConnection();
            }

            return matches;
        }

        public virtual RoundLegEntity GetLeg(MatchEntity match)
        {
            // if leg does not exist or no match.SequenceNo: result will be NULL!

            if (!match.LegSequenceNo.HasValue)
                return null;

            if (match.Round != null && match.Round.RoundLegs != null)
            {
                return match.Round.RoundLegs.First(l => l.SequenceNo == match.LegSequenceNo);
            }

            using (var da = _dbContext.GetNewAdapter())
            {
                var metaData = new LinqMetaData(da);
                IQueryable<RoundLegEntity> q = from l in metaData.RoundLeg
                    where l.RoundId == match.RoundId && l.SequenceNo == match.LegSequenceNo
                    select l;

                var result = q.First<RoundLegEntity>();
                da.CloseConnection();
                return result;
            }
        }

        public virtual MatchEntity GetMatchWithDateRelatedEntities(long matchId, long tournamentId)
        {
            return GetMatchWithDateRelatedEntities(MatchFields.Id == matchId, tournamentId);
        }

        private MatchEntity GetMatchWithDateRelatedEntities(IPredicate filter, long tournamentId)
        {
            var prefetchPathMatch = new PrefetchPath2(EntityType.MatchEntity);
            prefetchPathMatch.Add(MatchEntity.PrefetchPathHomeTeam);
            prefetchPathMatch[prefetchPathMatch.Count - 1].SubPath.Add(TeamEntity.PrefetchPathVenue);
            prefetchPathMatch[prefetchPathMatch.Count - 1].SubPath.Add(TeamEntity.PrefetchPathManagerOfTeams);
            prefetchPathMatch[prefetchPathMatch.Count - 1].SubPath[1].SubPath.Add(ManagerOfTeamEntity.PrefetchPathUser);
            prefetchPathMatch.Add(MatchEntity.PrefetchPathGuestTeam);
            prefetchPathMatch[prefetchPathMatch.Count - 1].SubPath.Add(TeamEntity.PrefetchPathVenue);
            prefetchPathMatch[prefetchPathMatch.Count - 1].SubPath.Add(TeamEntity.PrefetchPathManagerOfTeams);
            prefetchPathMatch[prefetchPathMatch.Count - 1].SubPath[1].SubPath.Add(ManagerOfTeamEntity.PrefetchPathUser);
            prefetchPathMatch.Add(MatchEntity.PrefetchPathSets);
            prefetchPathMatch.Add(MatchEntity.PrefetchPathVenue);
            prefetchPathMatch.Add(MatchEntity.PrefetchPathOrigVenue);
            prefetchPathMatch.Add(MatchEntity.PrefetchPathRound);
            prefetchPathMatch[prefetchPathMatch.Count - 1].SubPath.Add(RoundEntity.PrefetchPathRoundLegs);
            prefetchPathMatch[prefetchPathMatch.Count - 1].SubPath.Add(RoundEntity.PrefetchPathTournament);

            MatchEntity match;
            using (var da = _dbContext.GetNewAdapter())
            {
                // filter based for MatchEntity
                var bucket = new RelationPredicateBucket(filter);
                // filter based relation to RoundEntity
                bucket.Relations.Add(MatchEntity.Relations.RoundEntityUsingRoundId);
                bucket.PredicateExpression.AddWithAnd(RoundFields.TournamentId == tournamentId);
                match = da.FetchNewEntity<MatchEntity>(bucket, prefetchPathMatch);
                da.CloseConnection();
            }

            return match;
        }

        public virtual async Task<MatchEntity> GetMatchWithSetsAsync(long matchId, CancellationToken cancellationToken)
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                var metaData = new LinqMetaData(da);
                var match = await (from m in metaData.Match
                        where m.Id == matchId
                        select m).WithPath(new PathEdge<MatchEntity>(MatchEntity.PrefetchPathSets))
                    .FirstOrDefaultAsync<MatchEntity>(cancellationToken);
                return match;
            }
        }

        /// <summary>
        /// Finds any not completed matches, where home or guest team of the given match are involved,
        /// and where the match date/time are overlapping.
        /// </summary>
        /// <param name="match">The match to use for finding.</param>
        /// <param name="onlyUseDatePart">If true, only the date part is used, otherwise date and time.</param>
        /// <param name="tournamentId">The tournament id to filter the result.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns team ID of any matches, where home or guest team of the given match are involved,
        /// and where the match date/time are overlapping.</returns>
        public virtual async Task<long[]> AreTeamsBusyAsync(MatchEntity match, bool onlyUseDatePart, long tournamentId,
            CancellationToken cancellationToken)
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                var filter = new PredicateExpression(MatchFields.Id != match.Id & MatchFields.IsComplete == false &
                                                     (MatchFields.HomeTeamId == match.HomeTeamId |
                                                      MatchFields.GuestTeamId == match.HomeTeamId |
                                                      MatchFields.HomeTeamId == match.GuestTeamId |
                                                      MatchFields.GuestTeamId == match.GuestTeamId))
                    .AddWithAnd(RoundFields.TournamentId == tournamentId)
                    .AddWithAnd(
                        onlyUseDatePart
                            ? new PredicateExpression(MatchFields.PlannedStart.Date()
                                .Between(match.PlannedStart?.Date, match.PlannedEnd?.Date).Or(MatchFields.PlannedEnd
                                    .Date().Between(match.PlannedStart?.Date, match.PlannedEnd?.Date)))
                            : new PredicateExpression(MatchFields.PlannedStart
                                .Between(match.PlannedStart, match.PlannedEnd)
                                .Or(MatchFields.PlannedEnd.Between(match.PlannedStart, match.PlannedEnd))));

                var qf = new QueryFactory();
                var q = qf.Match.From(QueryTarget.LeftJoin(qf.Round).On(MatchFields.RoundId == RoundFields.Id))
                    .Where(filter).Select(() => new
                    {
                        Id = MatchFields.Id.ToValue<long>(), HomeTeamId = MatchFields.HomeTeamId.ToValue<long>(),
                        GuestTeamId = MatchFields.GuestTeamId.ToValue<long>()
                    });
                var matches = await da.FetchQueryAsync(q, cancellationToken);

                var teamIds = new List<long>();
                matches.ForEach(m =>
                {
                    _logger.LogTrace(
                        $"{nameof(AreTeamsBusyAsync)}: {(onlyUseDatePart ? match.PlannedStart?.ToString("'Date='yyyy-MM-dd") : match.PlannedStart?.ToString("'DateTime='yyyy-MM-dd HH:mm:ss"))} MatchId={m.Id}, HomeTeamId={m.HomeTeamId}, GuestTeamId={m.GuestTeamId}");
                    teamIds.Add(m.HomeTeamId);
                    teamIds.Add(m.GuestTeamId);
                });
                return teamIds.Where(tid => tid.Equals(match.HomeTeamId) || tid.Equals(match.GuestTeamId)).Distinct()
                    .ToArray();
            }
        }

        /// <summary>
        /// Gets the first entry from a list of dates that are excluded as match dates, or NULL if no date is found.
        /// Excluded dates may be related to a tournament, OR a round OR the home team OR the guest team.
        /// </summary>
        /// <param name="match">The <see cref="MatchEntity"/> where round and team IDs are taken.</param>
        /// <param name="onlyUseDatePart">If true, only the date part is used, otherwise date and time.</param>
        /// <param name="tournamentId">The tournament id to filter the result.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns the first <see cref="ExcludeMatchDateEntity"/> which matches the criteria, else NULL.</returns>
        public virtual async Task<ExcludeMatchDateEntity> GetExcludedMatchDateAsync(MatchEntity match,
            bool onlyUseDatePart, long tournamentId, CancellationToken cancellationToken)
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                var filter = new PredicateExpression(
                        ExcludeMatchDateFields.TournamentId == tournamentId |
                        ExcludeMatchDateFields.RoundId == match.RoundId |
                        ExcludeMatchDateFields.TeamId == match.HomeTeamId)
                    .AddWithAnd(
                        onlyUseDatePart
                            ? new PredicateExpression(ExcludeMatchDateFields.DateFrom.Date()
                                .Between(match.PlannedStart?.Date, match.PlannedEnd?.Date)
                                .Or(ExcludeMatchDateFields.DateTo.Date()
                                    .Between(match.PlannedStart?.Date, match.PlannedEnd?.Date)))
                            : new PredicateExpression(ExcludeMatchDateFields.DateFrom
                                .Between(match.PlannedStart, match.PlannedEnd)
                                .Or(ExcludeMatchDateFields.DateFrom
                                    .Between(match.PlannedStart, match.PlannedEnd))));

                return (await da.FetchQueryAsync(new QueryFactory().ExcludeMatchDate.Where(filter).Limit(1),
                    cancellationToken)).Cast<ExcludeMatchDateEntity>().FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the number of match records, matching the specified filter.
        /// </summary>
        /// <param name="filter">The filter <see cref="IPredicateExpression"/> may contain <see cref="MatchFields"/> and <see cref="RoundFields"/>.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns the number of match records, matching the specified filter.</returns>
        /// <example>
        /// Predicate for a certain tournament:
        /// <code>new PredicateExpression(RoundFields.TournamentId == 22)</code>
        /// Predicate for not completed matches of a round:
        /// <code>new PredicateExpression(RoundFields.Id == 123 & MatchFields.IsComplete == false)</code>
        /// </example>
        public virtual async Task<int> GetMatchCountAsync(PredicateExpression filter, CancellationToken cancellationToken)
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                var qf = new QueryFactory();
                var q = qf.Create()
                    .Select(MatchFields.Id.Count())
                    .From(qf.Match.InnerJoin(qf.Round)
                        .On(MatchFields.RoundId == RoundFields.Id))
                    .Where(filter);

                return await da.FetchScalarAsync<int>(q, cancellationToken);
            }
        }

        /// <summary>
        /// Find out whether there are already complete matches stored for any
        /// of the tournament rounds.
        /// </summary>
        /// <returns>Returns true if matches were found, else false.</returns>
        public virtual bool AnyCompleteMatchesExist(long tournamentId)
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                var metaData = new LinqMetaData(da);
                if ((from matchEntity in metaData.Match
                    where matchEntity.Round.TournamentId == tournamentId && matchEntity.IsComplete
                    select matchEntity.Id).AsEnumerable().Any())
                {
                    da.CloseConnection();
                    return true;
                }

                da.CloseConnection();
            }

            return false;
        }

        /// <summary>
        /// Find out whether there are already complete matches stored for 
        /// a tournament round.
        /// </summary>
        /// <param name="round">RoundEntity (only Id will be used)</param>
        /// <returns>Returns true if matches were found, else false.</returns>
        public virtual bool AnyCompleteMatchesExist(RoundEntity round)
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                var metaData = new LinqMetaData(da);
                if ((from matchEntity in metaData.Match
                    where matchEntity.Round.Id == round.Id && matchEntity.IsComplete
                    select matchEntity.Id).AsEnumerable().Any())
                {
                    da.CloseConnection();
                    return true;
                }

                da.CloseConnection();
            }

            return false;
        }

        /// <summary>
        /// Find out whether all matches of a tournament with a given Id are completed
        /// </summary>
        /// <returns>Returns true if all matches are completed, else false.</returns>
        public virtual bool AllMatchesCompleted(TournamentEntity tournament)
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                var metaData = new LinqMetaData(da);

                if ((from matchEntity in metaData.Match
                    where matchEntity.Round.TournamentId == tournament.Id && !matchEntity.IsComplete
                    select matchEntity.Id).AsEnumerable().Any())
                {
                    da.CloseConnection();
                    return false;
                }

                da.CloseConnection();
            }

            return true;
        }

        /// <summary>
        /// Find out whether all matches of a round with a gived Id are completed
        /// </summary>
        /// <returns>Returns true if all matches are completed, else false.</returns>
        public virtual bool AllMatchesCompleted(RoundEntity round)
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                var metaData = new LinqMetaData(da);

                if ((from matchEntity in metaData.Match
                    where matchEntity.Round.Id == round.Id && !matchEntity.IsComplete
                    select matchEntity.Id).AsEnumerable().Any())
                {
                    da.CloseConnection();
                    return false;
                }

                da.CloseConnection();
            }

            return true;
        }

        /// <summary>
        /// Saves the <see cref="MatchEntity"/> and the containing <see cref="SetEntity"/>s to the database
        /// using a transaction.
        /// </summary>
        /// <param name="matchEntity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns <see langword="true"/> if the transactions was successful, else <see langword="false"/>.</returns>
        public virtual async Task<bool> SaveMatchResult(MatchEntity matchEntity, CancellationToken cancellationToken)
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                da.StartTransaction(IsolationLevel.ReadCommitted,
                    string.Concat(nameof(MatchRepository), nameof(SaveMatchResult), Guid.NewGuid().ToString()));

                if (matchEntity.Sets.RemovedEntitiesTracker != null)
                {
                    await da.DeleteEntityCollectionAsync(matchEntity.Sets.RemovedEntitiesTracker, cancellationToken);
                    matchEntity.Sets.RemovedEntitiesTracker.Clear();
                }

                var success = await da.SaveEntityAsync(matchEntity, false, true, cancellationToken);
                da.Commit();
                return success;
            }
        }
    }
}