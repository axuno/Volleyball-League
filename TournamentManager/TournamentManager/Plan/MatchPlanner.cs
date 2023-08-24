using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using SD.LLBLGen.Pro.ORMSupportClasses;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using TournamentManager.MultiTenancy;

namespace TournamentManager.Plan;

/// <summary>
/// Generates fixtures for all teams of a tournament or round.
/// If the home team has no venue or home match time defined, it will only have away matches.
/// </summary>
public class MatchPlanner
{
    private readonly ITenantContext _tenantContext;
    private readonly AppDb _appDb;
    private static TournamentEntity _tournament = new();
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<MatchPlanner> _logger;
    private readonly AvailableMatchDates _availableMatchDates;
    private readonly Axuno.Tools.DateAndTime.TimeZoneConverter _timeZoneConverter;

    private static bool AreEntitiesLoaded { get; set; } = false;

    /// <summary>
    /// CTOR.
    /// </summary>
    /// <param name="tenantContext"></param>
    /// <param name="timeZoneConverter"></param>
    /// <param name="loggerFactory"></param>
    public MatchPlanner(ITenantContext tenantContext,
        Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter, ILoggerFactory loggerFactory)
    {
        _tenantContext = tenantContext;
        _appDb = tenantContext.DbContext.AppDb;
        _timeZoneConverter = timeZoneConverter;
        _availableMatchDates = new AvailableMatchDates(tenantContext, timeZoneConverter, loggerFactory.CreateLogger<AvailableMatchDates>());
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<MatchPlanner>();
    }

    private async Task LoadEntitiesAsync(CancellationToken cancellationToken)
    {
        _tournament = await _appDb.TournamentRepository.GetTournamentEntityForMatchPlannerAsync(
            _tenantContext.TournamentContext.MatchPlanTournamentId, cancellationToken) ?? throw new InvalidOperationException($"Could not load entity {nameof(TournamentEntity)}");
        AreEntitiesLoaded = true;
    }

    /// <summary>
    /// Generates dates which are excluded for the tournament with <see cref="TournamentContext.MatchPlanTournamentId"/>
    /// and saves them to persistent storage. Existing entries for the tournament are removed.
    /// </summary>
    /// <param name="excelImportFile"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task GenerateExcludedDates(string excelImportFile, CancellationToken cancellationToken)
    {
        var roundLegPeriods = await _appDb.RoundRepository.GetRoundLegPeriodAsync(new PredicateExpression(
            RoundLegPeriodFields.TournamentId == _tenantContext.TournamentContext.MatchPlanTournamentId),
            cancellationToken);

        var minDate = roundLegPeriods.Min(leg => leg.StartDateTime);
        var maxDate = roundLegPeriods.Max(leg => leg.EndDateTime);

        // remove all existing excluded dates for the tournament
        var filter = new RelationPredicateBucket(ExcludeMatchDateFields.TournamentId ==
                                                 _tenantContext.TournamentContext.MatchPlanTournamentId);
        await _appDb.GenericRepository.DeleteEntitiesDirectlyAsync(typeof(ExcludeMatchDateEntity), filter,
            cancellationToken);

        var excludedDates = new EntityCollection<ExcludeMatchDateEntity>(
            new TournamentManager.Importers.ExcludedDates.ExcelImporter(
                    _timeZoneConverter,
                    _loggerFactory.CreateLogger<Importers.ExcludedDates.ExcelImporter>())
                .Import(excelImportFile, new DateTimePeriod(minDate, maxDate)));

        foreach (var excludeMatchDateEntity in excludedDates)
            excludeMatchDateEntity.TournamentId = _tenantContext.TournamentContext.MatchPlanTournamentId;
            
        await _appDb.GenericRepository.SaveEntitiesAsync(excludedDates, false, false, cancellationToken);
    }

    public async Task GenerateAvailableMatchDatesAsync(ClearMatchDates clearMatchDates, RoundEntity round,
        CancellationToken cancellationToken)
    {
        _ = await _availableMatchDates.ClearAsync(clearMatchDates, cancellationToken);
        if (!AreEntitiesLoaded) await LoadEntitiesAsync(cancellationToken);
        await _availableMatchDates.GenerateNewAsync(round, cancellationToken);
    }

    /// <summary>
    /// Generates tournament match combinations for the Round Robin system, 
    /// assigns optimized match dates and stores the matches to
    /// the persistent storage.
    /// </summary>
    public async Task GenerateFixturesForTournament(bool keepExisting, CancellationToken cancellationToken)
    {
        if (!AreEntitiesLoaded) await LoadEntitiesAsync(cancellationToken);

        if (_appDb.MatchRepository.AnyCompleteMatchesExist(_tenantContext.TournamentContext.MatchPlanTournamentId))
            throw new InvalidOperationException("Completed matches exist for this tournament. Generating fixtures aborted.");

        foreach (var round in _tournament.Rounds)
            await GenerateFixturesForRound(round, keepExisting, cancellationToken);
    }

    /// <summary>
    /// Generates round match combinations for the Round Robin system, 
    /// assigns optimized match dates and stores the matches to
    /// the persistent storage.
    /// </summary>
    public async Task GenerateFixturesForRound(RoundEntity round, bool keepExisting,
        CancellationToken cancellationToken)
    {
        if (!AreEntitiesLoaded) await LoadEntitiesAsync(cancellationToken);

        if (_appDb.MatchRepository.AnyCompleteMatchesExist(round))
            throw new InvalidOperationException($"Completed matches exist for round '{round.Id}'. Generating fixtures aborted.");

        // generated matches will be stored here
        var roundMatches = new EntityCollection<MatchEntity>();

        if (keepExisting)
        {
            roundMatches = _appDb.MatchRepository.GetMatches(round);
        }
        else
        {
            var bucket = new RelationPredicateBucket(new PredicateExpression(
                new FieldCompareRangePredicate(MatchFields.RoundId, null, false, new[] {round.Id})));
            await _appDb.GenericRepository.DeleteEntitiesDirectlyAsync(typeof(MatchEntity), bucket,
                cancellationToken);
        }

        await GenerateAvailableMatchDatesAsync(ClearMatchDates.OnlyAutoGenerated, round, cancellationToken);

        // get the team ids because TeamEntity lacks IComparable
        // and cannot be used directly
        var teams = new Collection<long>(round.TeamCollectionViaTeamInRound.Select(t => t.Id).ToList());

        // now calculate matches for each leg of a round
        foreach (var roundLeg in round.RoundLegs)
        {
            // build up match combinations for the teams of round
            var roundRobin = new RoundRobinSystem<long>(teams);
            var bundledGroups =
                roundRobin.GetBundledGroups(RefereeType.HomeTeam,
                    roundLeg.SequenceNo % 2 == 1 ? LegType.First : LegType.Return,
                    CombinationGroupOptimization.GroupWithAlternatingHomeGuest);

            /*
             * Special treatment for teams which do not have home matches
             */
            var teamsWithoutHomeMatches = GetTeamsWithoutHomeMatches(round).ToList();

            foreach (var teamCombinationGroup in bundledGroups)
            foreach (var combination in teamCombinationGroup)
            {
                if (!teamsWithoutHomeMatches.Contains(combination.HomeTeam)) continue;

                _logger.LogDebug("Team cannot have home matches - {TeamId}", combination.HomeTeam);

                // swap home and guest team, keep referee unchanged
                (combination.HomeTeam, combination.GuestTeam) = (combination.GuestTeam, combination.HomeTeam);
            }

            /*
             * Assign desired from/to dates to bundled groups for later orientation
             * in which period matches should take place
             */
            AssignRoundDatePeriods(roundLeg, bundledGroups);

            if (bundledGroups.Any(g => !g.DateTimePeriod.Start.HasValue))
                throw new InvalidOperationException(
                    "Not all bundled groups got a date period assigned. Probably not enough dates available for assignment.");

            // process each team combination (match) that shall take place in the same week (if possible)
            foreach (var teamCombinationGroup in bundledGroups)
            {
                // get match dates for every combination of a group.
                // matches in the same teamCombinationGroup can even take place on the same day.
                // matchDates contains calculated dates in the same order as combinations,
                // so the index can be used for both.
                var availableDates = GetMatchDates(roundLeg, teamCombinationGroup, roundMatches);
                _logger.LogDebug("Available dates for combination: {dates}", string.Join(", ", availableDates.OrderBy(bd => bd?.MatchStartTime).Select(bd => bd?.MatchStartTime.ToShortDateString())).TrimEnd(',', ' '));

                for (var index = 0; index < teamCombinationGroup.Count; index++)
                {
                    var combination = teamCombinationGroup[index];

                    // If existing matches were loaded from database, we have to skip such combinations!
                    // Note: Home team and guest team of combinations could have been swapped for TeamsWithoutHomeMatches
                    if (roundMatches.Any(rm =>
                            rm.HomeTeamId == combination.HomeTeam && rm.GuestTeamId == combination.GuestTeam &&
                            rm.LegSequenceNo == roundLeg.SequenceNo || rm.GuestTeamId == combination.HomeTeam &&
                            rm.HomeTeamId == combination.GuestTeam && rm.LegSequenceNo == roundLeg.SequenceNo))
                        continue;

                    var match = new MatchEntity
                    {
                        HomeTeamId = combination.HomeTeam,
                        GuestTeamId = combination.GuestTeam,
                        RefereeId = combination.Referee,
                        PlannedStart = availableDates[index] != null ? availableDates[index]!.MatchStartTime : default(DateTime?),
                        PlannedEnd = availableDates[index] != null ? availableDates[index]!.MatchStartTime
                            .Add(_tenantContext.TournamentContext.FixtureRuleSet.PlannedDurationOfMatch) : default(DateTime?),
                        VenueId = availableDates[index] != null 
                            ? availableDates[index]!.VenueId
                            // take over the venue stored in the team entity (may also be null!)
                            : _tournament.Rounds[_tournament.Rounds.FindMatches(RoundFields.Id == roundLeg.RoundId).First()].TeamCollectionViaTeamInRound.First(t => t.Id == combination.HomeTeam).VenueId,
                        RoundId = round.Id,
                        IsComplete = false,
                        LegSequenceNo = roundLeg.SequenceNo,
                        ChangeSerial = 0,
                        Remarks = string.Empty
                    };

                    _logger.LogDebug("Fixture: {HomeTeam} - {GuestTeam}: {PlannedStart}", match.HomeTeamId, match.GuestTeamId, match.PlannedStart);

                    roundMatches.Add(match);
                }
            }
        }

        // save the matches for the group
        await _appDb.GenericRepository.SaveEntitiesAsync(roundMatches, true, false, cancellationToken);

        await _availableMatchDates.ClearAsync(ClearMatchDates.OnlyAutoGenerated, cancellationToken);
    }

    private static List<DateTime> GetOccupiedMatchDates(TeamCombination<long> combination,
        IEnumerable<MatchEntity> matches)
    {
        return (from match in matches
            where
                match.PlannedStart.HasValue && match.PlannedEnd.HasValue &&
                (match.HomeTeamId == combination.HomeTeam || match.GuestTeamId == combination.GuestTeam ||
                 match.GuestTeamId == combination.HomeTeam || match.GuestTeamId == combination.GuestTeam)
            select match.PlannedStart!.Value.Date).ToList();
    }


    private List<AvailableMatchDateEntity?> GetMatchDates(RoundLegEntity roundLeg,
        TeamCombinationGroup<long> teamCombinationGroup, EntityCollection<MatchEntity> groupMatches)
    {
        // here the resulting match dates are stored:
        var matchDatePerCombination = new List<AvailableMatchDateEntity?>();

        // these are possible date alternatives per combination:
        var matchDates = new List<List<AvailableMatchDateEntity>>();

        for (var index = 0; index < teamCombinationGroup.Count; index++)
        {
            var combination = teamCombinationGroup[index];

            var availableDates = _availableMatchDates.GetGeneratedAndManualAvailableMatchDates(combination.HomeTeam,
                teamCombinationGroup.DateTimePeriod, GetOccupiedMatchDates(combination, groupMatches));
            // initialize MinTimeDiff for the whole list
            availableDates.ForEach(amd => amd.MinTimeDiff = TimeSpan.MaxValue);
            if (availableDates.Count == 0)
            {
                availableDates = _availableMatchDates.GetGeneratedAndManualAvailableMatchDates(combination.HomeTeam,
                    new DateTimePeriod(roundLeg.StartDateTime, roundLeg.EndDateTime),
                    GetOccupiedMatchDates(combination, groupMatches));
            }
            
            matchDates.Add(availableDates);

#if DEBUG
            // Check whether there is a match of this combination
            var lastMatchOfCombination = groupMatches.OrderBy(gm => gm.PlannedStart).LastOrDefault(gm =>
                gm.HomeTeamId == combination.HomeTeam || gm.GuestTeamId == combination.GuestTeam);
            if (lastMatchOfCombination != null)
            {
                _logger.LogDebug("Last match date found for home team '{homeTeam}' and guest team '{guestTeam}' is '{plannedStart}'", combination.HomeTeam, combination.GuestTeam, lastMatchOfCombination.PlannedStart?.ToShortDateString() ?? "none");
            }
            else
            {
                _logger.LogDebug("No last match found for home team '{homeTeam}' and guest team '{guestTeam}'", combination.HomeTeam, combination.GuestTeam);
            }
#endif
        }

        // we can't proceed without any match dates found
        if (matchDates.Count == 0) return matchDatePerCombination;

        // only 1 match date found, so optimization is not possible
        // and the following "i-loop" will be skipped
        if (matchDates.Count == 1)
        {
            matchDatePerCombination.Add(matchDates[0][0]);
            return matchDatePerCombination;
        }

        // cross-compute the number of dates between between group pairs.
        // goal: found match dates should be as close together as possible

        // start with 1st dates, end with last but one dates
        for (var i = 0; i < matchDates.Count - 1; i++)
        {
            // start with 2nd dates, end with last dates
            for (var j = 1; j < matchDates.Count; j++)
            {
                // compare each date in the first list...
                foreach (var dates1 in matchDates[i])
                {
                    // ... with the dates in the second list
                    foreach (var dates2 in matchDates[j])
                    {
                        var daysDiff = Math.Abs((dates1.MatchStartTime.Date - dates2.MatchStartTime.Date).Days);

                        // save minimum dates found for later reference
                        if (daysDiff < dates1.MinTimeDiff.Days)
                            dates1.MinTimeDiff = new TimeSpan(daysDiff, 0, 0, 0);

                        if (daysDiff < dates2.MinTimeDiff.Days)
                            dates2.MinTimeDiff = new TimeSpan(daysDiff, 0, 0, 0);
                    } // end dates2
                } // end dates1
            } // end j

            // get the date that has least distance to smallest date in other group(s)
            // Note: If no match dates could be determined for a team, bestDate will be null.
            var bestDate = matchDates[i].Where(md => md.MinTimeDiff == matchDates[i].Min(d => d.MinTimeDiff))
                .OrderBy(md => md.MinTimeDiff).FirstOrDefault();
            matchDatePerCombination.Add(bestDate);

            // process the last combination

            // in case comparisons took place,
            // now the "j-loop" group is not processed yet:
            if (i + 1 >= matchDates.Count - 1)
            {
                bestDate = matchDates[^1].Where(md => md.MinTimeDiff == matchDates[^1].Min(d => d.MinTimeDiff))
                    .MinBy(md => md.MinTimeDiff);
                // the last "j-increment" is always the same as "matchDates[^1]" (loop condition)
                matchDatePerCombination.Add(bestDate);
            }
        } // end i

        return matchDatePerCombination;
    }

    /// <summary>
    /// Date periods are assigned to bundled groups purely mathematically,
    /// spreading match dates equally across the <see cref="RoundLegEntity"/>'s <see cref="RoundLegEntity.StartDateTime"/> and <see cref="RoundLegEntity.EndDateTime"/>.
    /// </summary>
    /// <param name="roundLeg"></param>
    /// <param name="bundledGroups"></param>
    private void AssignRoundDatePeriods(RoundLegEntity roundLeg,
        Collection<TeamCombinationGroup<long>> bundledGroups)
    {
        var allMatchDaysOfRound =
            _availableMatchDates
                .GetGeneratedAndManualAvailableMatchDateDays(
                    roundLeg); // _appDb.AvailableMatchDateRepository.GetAvailableMatchDateDays(roundLeg);

        var periodDaysCount = allMatchDaysOfRound.Count / (bundledGroups.Count + 1);

        var start = 0;
        var index = 0;

        _logger.LogDebug("*** Round: {roundName} - RoundLeg: {legDescription}\n", roundLeg.Round.Name, roundLeg.Description);
        while (start < allMatchDaysOfRound.Count && index < bundledGroups.Count)
        {
            //TODO: There could be a remainder of days because of integer division!
            var end = start + periodDaysCount < allMatchDaysOfRound.Count
                ? start + periodDaysCount
                : allMatchDaysOfRound.Count - 1;
            bundledGroups[index].DateTimePeriod =
                new DateTimePeriod(allMatchDaysOfRound[start].Date, allMatchDaysOfRound[end].Date);

            _logger.LogDebug("Bundle date period: From={from}, To={to}, {days} days",
                bundledGroups[index].DateTimePeriod.Start?.ToShortDateString(),
                bundledGroups[index].DateTimePeriod.End?.ToShortDateString(),
                (bundledGroups[index].DateTimePeriod.End - bundledGroups[index].DateTimePeriod.Start)
                ?.Days);

            start = end + 1;
            index++;
        }
    }

    /// <summary>
    /// Gets teams IDs for the <param name="round"></param> where no home venue or no home match time set for a team.
    /// </summary>
    private static IEnumerable<long> GetTeamsWithoutHomeMatches(RoundEntity round)
    {
        foreach (var team in round.TeamCollectionViaTeamInRound)
        {
            if (team.VenueId == null || team.MatchTime == null || team.MatchDayOfWeek == null)
            {
                yield return team.Id;
            }
        }
    }
}
