using System;
using System.Collections.Generic;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using SD.LLBLGen.Pro.ORMSupportClasses;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TournamentManager.Data;

namespace TournamentManager.Plan
{
    public class AvailableMatchDates
    {
        private readonly OrganizationContext _organizationContext;
        private readonly TournamentManager.MultiTenancy.AppDb _appDb;
        private readonly Axuno.Tools.DateAndTime.TimeZoneConverter _timeZoneConverter;

        private readonly ILogger<AvailableMatchDates> _logger;

        // available match dates from database
        private readonly EntityCollection<AvailableMatchDateEntity> _availableMatchDateEntities =
            new EntityCollection<AvailableMatchDateEntity>();

        // programmatically generated available match dates
        private readonly EntityCollection<AvailableMatchDateEntity> _generatedAvailableMatchDateEntities =
            new EntityCollection<AvailableMatchDateEntity>();

        // excluded dates
        private readonly EntityCollection<ExcludeMatchDateEntity> _excludedMatchDateEntities =
            new EntityCollection<ExcludeMatchDateEntity>();

        internal AvailableMatchDates(OrganizationContext organizationContext,
            Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter, ILogger<AvailableMatchDates> logger)
        {
            _organizationContext = organizationContext;
            _appDb = organizationContext.AppDb;
            _timeZoneConverter = timeZoneConverter;
            _logger = logger;
        }

        private async Task Initialize(CancellationToken cancellationToken)
        {
            _excludedMatchDateEntities.Clear();
            _excludedMatchDateEntities.AddRange(
                await _appDb.ExcludedMatchDateRepository.GetExcludedMatchDatesAsync(
                    _organizationContext.MatchPlanTournamentId, cancellationToken));

            _availableMatchDateEntities.Clear();
            _availableMatchDateEntities.AddRange(
                await _appDb.AvailableMatchDateRepository.GetAvailableMatchDatesAsync(
                    _organizationContext.MatchPlanTournamentId, cancellationToken));

            _generatedAvailableMatchDateEntities.Clear();
        }


        /// <summary>
        /// Removes entries in AvailableMatchDates database table.
        /// </summary>
        /// <param name="clear">Which entries to delete for the tournament.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns the number of deleted records.</returns>
        internal async Task<int> ClearAsync(ClearMatchDates clear, CancellationToken cancellationToken)
        {
            var deleted = 0;

            // tournament is always in the filter
            var filterAvailable = new RelationPredicateBucket();
            filterAvailable.PredicateExpression.Add(AvailableMatchDateFields.TournamentId ==
                                                    _organizationContext.MatchPlanTournamentId);

            if (clear == ClearMatchDates.All)
            {
                deleted = await _appDb.GenericRepository.DeleteEntitiesDirectlyAsync(typeof(AvailableMatchDateEntity),
                    null!, cancellationToken);
                _generatedAvailableMatchDateEntities.Clear();
            }
            else if (clear == ClearMatchDates.OnlyAutoGenerated)
            {
                filterAvailable.PredicateExpression.AddWithAnd(AvailableMatchDateFields.IsGenerated == true);
                deleted = await _appDb.GenericRepository.DeleteEntitiesDirectlyAsync(typeof(AvailableMatchDateEntity),
                    filterAvailable, cancellationToken);
                _generatedAvailableMatchDateEntities.Clear();
            }
            else if (clear == ClearMatchDates.OnlyManual)
            {
                filterAvailable.PredicateExpression.AddWithAnd(AvailableMatchDateFields.IsGenerated == false);
                deleted = await _appDb.GenericRepository.DeleteEntitiesDirectlyAsync(typeof(AvailableMatchDateEntity),
                    filterAvailable, cancellationToken);
            }

            return deleted;
        }

        /// <summary>
        /// Generate available match dates for teams where 
        /// <see cref="TeamEntity.MatchDayOfWeek"/>, <see cref="TeamEntity.MatchTime"/>, <see cref="TeamEntity.VenueId"/>
        /// are not <see langword="null"/>.
        /// </summary>
        /// <param name="round"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal async Task GenerateNewAsync(RoundEntity round, CancellationToken cancellationToken)
        {
            await Initialize(cancellationToken);
            var teamIdProcessed = new List<long>();
            var listTeamsWithSameVenue = new List<EntityCollection<TeamEntity>>();

            // Make a list of teams of the same round and with the same venue AND weekday AND match time
            // Venues will later be assigned to these teams alternately
            foreach (var team in round.TeamCollectionViaTeamInRound)
            {
                // the collection will contain at least one team
                var teams = GetTeamsWithSameVenueAndMatchTime(team, round);
                if (teamIdProcessed.Contains(teams[0].Id)) continue;

                listTeamsWithSameVenue.Add(teams);
                foreach (var t in teams)
                    if (!teamIdProcessed.Contains(t.Id))
                        teamIdProcessed.Add(t.Id);
            }

            foreach (var roundLeg in round.RoundLegs)
            {
                var startDate = DateTime.SpecifyKind(roundLeg.StartDateTime, DateTimeKind.Utc);
                var endDate = DateTime.SpecifyKind(roundLeg.EndDateTime, DateTimeKind.Utc);

                foreach (var teamsWithSameVenue in listTeamsWithSameVenue)
                {
                    var teamIndex = 0;

                    // Make sure these values are not null
                    if (!teamsWithSameVenue[teamIndex].MatchDayOfWeek.HasValue ||
                        !teamsWithSameVenue[teamIndex].MatchTime.HasValue ||
                        !teamsWithSameVenue[teamIndex].VenueId.HasValue)
                        continue;

                    // Create Tuple for non-nullable context
                    var team = (Id: teamsWithSameVenue[teamIndex].Id,
                        MatchDayOfWeek: (DayOfWeek) teamsWithSameVenue[teamIndex].MatchDayOfWeek!.Value,
                        MatchTime: teamsWithSameVenue[teamIndex].MatchTime!.Value,
                        VenueId: teamsWithSameVenue[teamIndex].VenueId!.Value);
                    
                    // get the first possible match date equal or after the leg's starting date
                    var matchDate = IncrementDateUntilDayOfWeek(startDate, team.MatchDayOfWeek);

                    // process the period of a leg
                    while (matchDate <= endDate)
                    {
                        // if there is more than one team per venue with same weekday and match time,
                        // match dates will be assigned alternately
                        var matchDateAndTimeUtc = _timeZoneConverter.ToUtc(matchDate.Date.Add(team.MatchTime));

                        // check whether the calculated date 
                        // is within the borders of round legs (if any) and is not marked as excluded
                        if (IsDateWithinRoundLegDateTime(roundLeg, matchDateAndTimeUtc)
                            && !IsExcludedDate(matchDateAndTimeUtc, round.Id, team.Id)
                            && !await IsVenueOccupiedByMatchAsync(
                                new DateTimePeriod(matchDateAndTimeUtc,
                                    matchDateAndTimeUtc.Add(_organizationContext.FixtureRuleSet
                                        .PlannedDurationOfMatch)), team.VenueId, cancellationToken))
                        {
                            var av = new AvailableMatchDateEntity
                            {
                                TournamentId = _organizationContext.MatchPlanTournamentId,
                                HomeTeamId = team.Id,
                                VenueId = team.VenueId,
                                MatchStartTime = matchDateAndTimeUtc,
                                MatchEndTime =
                                    matchDateAndTimeUtc.Add(_organizationContext.FixtureRuleSet.PlannedDurationOfMatch),
                                IsGenerated = true
                            };

                            _generatedAvailableMatchDateEntities.Add(av);
                            teamIndex = ++teamIndex >= teamsWithSameVenue.Count ? 0 : teamIndex;
                        }

                        matchDate = matchDate.Date.AddDays(7);
                    }
                }
            }

            _logger.LogTrace("Generated {Count} UTC dates for HomeTeams:", _generatedAvailableMatchDateEntities.Count);
            _logger.LogTrace("{Generated}\n", _generatedAvailableMatchDateEntities.Select(gen => (gen.HomeTeamId, gen.MatchStartTime)));

            // save to the persistent storage
            // await _appDb.GenericRepository.SaveEntitiesAsync(_generatedAvailableMatchDateEntities, true, false, cancellationToken);
        }

        private async Task<bool> IsVenueOccupiedByMatchAsync(DateTimePeriod matchTime, long venueId,
            CancellationToken cancellationToken)
        {
            return (await _appDb.VenueRepository.GetOccupyingMatchesAsync(venueId, matchTime,
                _organizationContext.MatchPlanTournamentId, cancellationToken)).Any();
        }

        private bool IsDateWithinRoundLegDateTime(RoundLegEntity leg, DateTime queryDate)
        {
            return queryDate.Date >= leg.StartDateTime.Date && queryDate.Date <= leg.EndDateTime.Date;
        }
        
        private DateTime IncrementDateUntilDayOfWeek(DateTime date, DayOfWeek dayOfWeek)
        {
            var count = 0;
            while (date.DayOfWeek != dayOfWeek && count++ <= 7)
                date = date.AddDays(1);

            return date;
        }

        /// <summary>
        /// Checks whether the <paramref name="queryDate"/> is excluded either for a tournament OR optionally for a round or team.
        /// If the excluded table row contains a <paramref name="roundId"/> or <paramref name="teamId"/>, it is NOT excluded for the tournament,
        /// but only for the team or round.
        /// </summary>
        /// <remarks>
        /// Same behavior as with <see cref="ExcludedMatchDateRepository.GetExcludedMatchDateAsync"/>.
        /// </remarks>
        /// <param name="queryDate">Date to test, whether it is excluded.</param>
        /// <param name="roundId">OR excluded on the round level. If <see langword="null" />, there is no round restriction.</param>
        /// <param name="teamId">OR excluded on the team level. If <see langword="null" />, there is no team restriction.</param>
        /// <returns>Returns <see langword="true"/>, if criteria match, else <see langword="false"/>.</returns>
        private bool IsExcludedDate(DateTime queryDate, long? roundId, long? teamId)
        {
            return
                // Excluded for the whole tournament...
                _excludedMatchDateEntities.Any(
                    excl => queryDate >= excl.DateFrom && queryDate <= excl.DateTo &&
                            excl.TournamentId == _organizationContext.MatchPlanTournamentId && !excl.RoundId.HasValue &&
                            !excl.TeamId.HasValue)
                ||
                // OR excluded for a round...
                _excludedMatchDateEntities.Any(
                    excl => queryDate >= excl.DateFrom && queryDate <= excl.DateTo &&
                            excl.TournamentId == _organizationContext.MatchPlanTournamentId && excl.RoundId.HasValue &&
                            roundId.HasValue && excl.RoundId == roundId)
                ||
                // OR excluded for a team
                _excludedMatchDateEntities.Any(
                    excl => queryDate >= excl.DateFrom && queryDate <= excl.DateTo &&
                            excl.TournamentId == _organizationContext.MatchPlanTournamentId && excl.TeamId.HasValue &&
                            teamId.HasValue && excl.TeamId == teamId)
                ;
        }

        internal List<DateTime> GetGeneratedAndManualAvailableMatchDateDays(RoundLegEntity leg)
        {
            var result = _generatedAvailableMatchDateEntities.Union(_availableMatchDateEntities)
                .Where(gen => gen.TournamentId == _organizationContext.MatchPlanTournamentId
                              && gen.MatchStartTime >= leg.StartDateTime.Date &&
                              gen.MatchStartTime <= leg.EndDateTime.AddDays(1).AddSeconds(-1))
                .OrderBy(gen => gen.MatchStartTime)
                .Select(gen => gen.MatchStartTime.Date)
                .Distinct()
                .ToList();
            return result;
        }

        internal List<AvailableMatchDateEntity> GetGeneratedAndManualAvailableMatchDates(long homeTeamId,
            DateTimePeriod datePeriod, List<DateTime>? excludedDates)
        {
            if (!(datePeriod.Start.HasValue && datePeriod.End.HasValue)) throw new ArgumentNullException(nameof(datePeriod));

            var result = _generatedAvailableMatchDateEntities.Union(_availableMatchDateEntities)
                .Where(gen => gen.TournamentId == _organizationContext.MatchPlanTournamentId
                              && gen.HomeTeamId == homeTeamId && gen.MatchStartTime >= datePeriod.Start.Value.Date &&
                              gen.MatchStartTime <=
                              datePeriod.End.Value.Date.AddDays(1).AddSeconds(-1))
                .OrderBy(gen => gen.MatchStartTime);

            if (excludedDates != null && excludedDates.Count > 0)
                return result.Where(dates => !excludedDates.Contains(dates.MatchStartTime.Date))
                    .OrderBy(dates => dates.MatchStartTime).ToList();

            return result.ToList();
        }

        private EntityCollection<TeamEntity> GetTeamsWithSameVenueAndMatchTime(TeamEntity team, RoundEntity round)
        {
            var resultTeams = new EntityCollection<TeamEntity>();

            var teamStartTime = team.MatchTime;
            var teamEndTime = teamStartTime?.Add(_organizationContext.FixtureRuleSet.PlannedDurationOfMatch);

            // get a list of other teams in this round with same venue and match day-of-the-week
            var otherTeams = round.TeamCollectionViaTeamInRound.FindMatches((TeamFields.VenueId == team.VenueId) &
                                                                            (TeamFields.MatchDayOfWeek ==
                                                                             team.MatchDayOfWeek) &
                                                                            (TeamFields.Id != team.Id));

            foreach (var index in otherTeams)
            {
                var otherStartTime = round.TeamCollectionViaTeamInRound[index].MatchTime;
                var otherEndTime = otherStartTime?.Add(_organizationContext.FixtureRuleSet.PlannedDurationOfMatch);

                if (otherStartTime <= teamStartTime && otherEndTime >= teamStartTime ||
                    otherStartTime <= teamEndTime && otherEndTime >= teamEndTime)
                    resultTeams.Add(round.TeamCollectionViaTeamInRound[index]);
            }

            // list is expected to contain at least one team
            resultTeams.Add(team);

            return resultTeams;
        }
    }
}