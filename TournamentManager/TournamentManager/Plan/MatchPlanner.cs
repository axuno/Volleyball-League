using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SD.LLBLGen.Pro.LinqSupportClasses;
using TournamentManager.DAL;
using TournamentManager.DAL.Linq;
using TournamentManager.Data;
using TournamentManager.DAL.DatabaseSpecific;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.FactoryClasses;
using TournamentManager.DAL.HelperClasses;
using SD.LLBLGen.Pro.ORMSupportClasses;
using System.Collections.ObjectModel;

namespace TournamentManager.Plan
{
	public class MatchPlanner
    {
        private readonly OrganizationContext _organizationContext;
	    private readonly AppDb _appDb;
		private AvailableMatchDates _availableMatchDates;
		private static TournamentEntity _tournament;
		private EntityCollection<VenueEntity> _venue;
		
		private bool _entitiesLoaded = false;

		private StringBuilder _log = new StringBuilder();


		public MatchPlanner(AppDb appDb, OrganizationContext organizationContext, long tournamentId)
        {
            _organizationContext = organizationContext;
		    _appDb = appDb;
			_tournament = new TournamentEntity(tournamentId);
			_venue = new EntityCollection<VenueEntity>(new VenueEntityFactory());
			_availableMatchDates = new AvailableMatchDates(_appDb, this);
		}

		private void LoadEntities()
		{
			var bucket = new RelationPredicateBucket(TournamentFields.Id == _tournament.Id);
			bucket.Relations.Add(TournamentEntity.Relations.RoundEntityUsingTournamentId);
			bucket.Relations.Add(RoundEntity.Relations.TeamInRoundEntityUsingRoundId);
			bucket.Relations.Add(TeamInRoundEntity.Relations.TeamEntityUsingTeamId);
			bucket.Relations.Add(TeamEntity.Relations.VenueEntityUsingVenueId);

			var prefetchPathTournament = new PrefetchPath2(EntityType.TournamentEntity);
			prefetchPathTournament.Add(TournamentEntity.PrefetchPathRounds);
			prefetchPathTournament[1].SubPath.Add(RoundEntity.PrefetchPathRoundLegs);
			prefetchPathTournament[1].SubPath.Add(RoundEntity.PrefetchPathTeamCollectionViaTeamInRound).SubPath.Add(TeamEntity.PrefetchPathVenue);

			var t = new EntityCollection<TournamentEntity>();
			_appDb.DbContext.GetNewAdapter().FetchEntityCollection(t, bucket, prefetchPathTournament);
			_tournament = t.FirstOrDefault();

			_entitiesLoaded = true;
		}

		internal IDataAccessAdapter Adapter
		{
			get { return _appDb.DbContext.GetNewAdapter(); }
		}

		internal TournamentEntity Tournament
		{
			get { return _tournament; }
		}

		internal EntityCollection<VenueEntity> Venue
		{
			get { return _venue; }
		}

		internal TimeSpan PlannedDurationOfMatch
        {
            get => _organizationContext.FixtureRuleSet.PlannedDurationOfMatch;
        }

		public AvailableMatchDates AvailableMatchDates
		{
			get { return _availableMatchDates; }
		}

        public void GenerateExcludedDates(string specialHolidaysXmlFile)
        {
            if (!_entitiesLoaded) LoadEntities();
            // this will fill table exclude_match_days for the current tournament
            // existing dates will be removed!
            _availableMatchDates.GenerateExcludedDates(specialHolidaysXmlFile);
        }

		public void GenerateAvailableMatchDates(ClearMatchDates clearMatchDates, RoundEntity round)
		{
			int deleted = _availableMatchDates.Clear(clearMatchDates);
			// long numRec = (long)_adapter.GetScalar(AvailableMatchDateFields.Id, AggregateFunction.CountRow);
			if (! _entitiesLoaded) LoadEntities();
			// GenerateNew will (re)load AvailableMatchDateEntity
			_availableMatchDates.GenerateNew(round);
		}

		/// <summary>
		/// Generates tournament match combinations for the Round Robin system, 
		/// assignes optimized match dates and stores the matches to
		/// the persistent storage.
		/// </summary>
		public void GeneratePlannedMatchesForTournament(bool keepExisting = false)
		{
			if (!_entitiesLoaded)
				LoadEntities();

			if (_appDb.MatchRepository.AnyCompleteMatchesExist(_tournament.Id))
				throw new Exception("Completed matches exist for this tournament. Generating new planned matches aborted.");

			foreach (var round in _tournament.Rounds)
			{
				GeneratePlannedMatchesForRound(round, keepExisting);
			}
		}
		
		/// <summary>
		/// Generates round match combinations for the Round Robin system, 
		/// assignes optimized match dates and stores the matches to
		/// the persistent storage.
		/// </summary>
		public void GeneratePlannedMatchesForRound(RoundEntity round, bool keepExisting = false)
		{
			if (!_entitiesLoaded) 
				LoadEntities();

		    round = _tournament.Rounds.First(r => r.Id == round.Id);

			if (_appDb.MatchRepository.AnyCompleteMatchesExist(round))
				throw new Exception("Completed matches exist for this round. Generating new planned matches aborted.");
			
			// generated matches will be stored here
			var roundMatches = new EntityCollection<MatchEntity>();

			if (keepExisting)
			{
				roundMatches = _appDb.MatchRepository.GetMatches(round);
			}
			else
			{
				var bucket = new RelationPredicateBucket(new PredicateExpression(new FieldCompareRangePredicate(MatchFields.RoundId, null, false, new[] {round.Id})));
				Adapter.DeleteEntitiesDirectly(typeof (MatchEntity).Name, bucket);
			}

			GenerateAvailableMatchDates(ClearMatchDates.OnlyAutoGenerated, round);
			
			// get the team ids because TeamEntity lacks of IComparable
			// and cannot be used directly
			var teams = new Collection<long>(round.TeamCollectionViaTeamInRound.Select(t => t.Id).ToList());

			// now calculate matches for each leg of a round
			foreach (var roundLeg in round.RoundLegs)
			{	
				// build up match combinations for the teams of round
				var roundRobin = new RoundRobinSystem<long>(teams);
				Collection<TeamCombinationGroup<long>> bundledGroups =
					roundRobin.GetBundledGroups(RefereeType.HomeTeam, roundLeg.SequenceNo % 2 == 1 ? LegType.First : LegType.Return, CombinationGroupOptimization.GroupWithAlternatingHomeGuest);

/*
 **********************************************************************************************************
 **********************************************************************************************************
 
 // TODO: Diese Spezialanpassung für die Beachrunde 2018 entfernen!!!!!!!!!!!!!!!!!!!!
foreach (var teamCombinationGroup in bundledGroups)
{
	// TSG Hochzoll hat keine Heimespiele
	for (int index = 0; index < teamCombinationGroup.Count; index++)
	{
		TeamCombination<long> combination = teamCombinationGroup[index];
		if ((new[] {12L, 13L, 14L}).Contains(combination.HomeTeam))
		{
			combination.HomeTeam = combination.GuestTeam;
			combination.GuestTeam = combination.Referee;
			combination.Referee = combination.HomeTeam;
		}
	}
}

 **********************************************************************************************************
 **********************************************************************************************************
 */


                // assign desired from/to dates to bundled groups for later orientation
                // in which period matches should take place
                AssignRoundDateSegments(roundLeg, bundledGroups);

				if (!bundledGroups.All(g => g.DateTimePeriod.Start.HasValue))
					throw new Exception("Not all bundled groups have a date segment assigned. Probably not enough dates available for assingment.");

				// process each team combination (match) that shall take place in the same week (if possible)
				foreach (var teamCombinationGroup in bundledGroups)
				{
				    // get match dates for every combination of a group.
                    // matches in the same teamCombinationGroup can even take place the same day.
                    // matchDates contains calculated dates in the same order as combinations,
                    // so the index can be used for both.
                    List<AvailableMatchDateEntity> availableDates = GetMatchDates(roundLeg, teamCombinationGroup, roundMatches);

					for (int index = 0; index < teamCombinationGroup.Count; index++ )
					{
						TeamCombination<long> combination = teamCombinationGroup[index];

						// If existing matches were loaded from database, we have to skip such combinations!
						// Note: Home team and guest team of combinations could have been swapped manually after automatic generation!!
						if (roundMatches.Any(rm => (rm.HomeTeamId == combination.HomeTeam && rm.GuestTeamId == combination.GuestTeam && rm.LegSequenceNo == roundLeg.SequenceNo) || (rm.GuestTeamId == combination.HomeTeam && rm.HomeTeamId == combination.GuestTeam && rm.LegSequenceNo == roundLeg.SequenceNo)))
							continue;

						var now = DateTime.Now;
						var match = new MatchEntity
							            {
							            	HomeTeamId = combination.HomeTeam,
							            	GuestTeamId = combination.GuestTeam,
							            	RefereeId = combination.Referee,
							            	PlannedStart = availableDates[index].MatchStartTime,
							            	PlannedEnd = availableDates[index].MatchStartTime.Add(PlannedDurationOfMatch),
							            	VenueId = availableDates[index].VenueId,
							            	RoundId = round.Id,
							            	CreatedOn = now,
							            	ModifiedOn = now,
							            	IsComplete = false,
							            	LegSequenceNo = roundLeg.SequenceNo,
                                            ChangeSerial = 0,
							            	Remarks = string.Empty
							            };
						roundMatches.Add(match);
					}
				}
			}
			// save the matches for the group
			Adapter.SaveEntityCollection(roundMatches, true, false);

			_availableMatchDates.Clear(ClearMatchDates.OnlyAutoGenerated);
		}

		private List<DateTime> GetOccupiedMatchDates(TeamCombination<long> combination, IEnumerable<MatchEntity> matches)
		{
			return (from match in matches
			        where
			        	match.HomeTeamId == combination.HomeTeam || match.GuestTeamId == combination.GuestTeam ||
			        	match.GuestTeamId == combination.HomeTeam || match.GuestTeamId == combination.GuestTeam
			        select match.PlannedStart.Value.Date).ToList();
		}


		private List<AvailableMatchDateEntity> GetMatchDates(RoundLegEntity roundLeg, TeamCombinationGroup<long> teamCombinationGroup, EntityCollection<MatchEntity> groupMatches)
		{
			var maxDateOfCombination = DateTime.MinValue;

			// here the resulting match dates are stored:
			var matchDatePerCombination = new List<AvailableMatchDateEntity>();
			
			// these are possible date alternatives per combination:
			var matchDates = new List<List<AvailableMatchDateEntity>>();
			
			for (int index = 0; index < teamCombinationGroup.Count; index++)
			{
				TeamCombination<long> combination = teamCombinationGroup[index];

				var availableDates = _availableMatchDates.GetAvailableMatchDays(combination.HomeTeam, teamCombinationGroup.DateTimePeriod, GetOccupiedMatchDates(combination, groupMatches));
				if (availableDates.Count == 0)
				{
					availableDates = _availableMatchDates.GetAvailableMatchDays(combination.HomeTeam, new DateTimePeriod(roundLeg.StartDateTime, roundLeg.EndDateTime), GetOccupiedMatchDates(combination, groupMatches));
				}

				availableDates.ForEach(amd => amd.MinTimeDiff = TimeSpan.MaxValue);
				matchDates.Add(availableDates);

				// Check whether there is a match of this combination and if yes, store the max PlannedStart for later optimization
				var lastMatchOfCombination = groupMatches.OrderBy(gm => gm.PlannedStart).LastOrDefault(gm => gm.HomeTeamId == combination.HomeTeam || gm.GuestTeamId == combination.GuestTeam);
				if (lastMatchOfCombination != null)
				{
					maxDateOfCombination = lastMatchOfCombination.PlannedStart.Value.Date;
				}
			}

			// only 1 match date found, so optimization is not possible
			// and the following "i-loop" will be skipped
			if (matchDates.Count == 1)
			{
				matchDatePerCombination.Add(matchDates[0][0]);
			}
			
			// cross-compute the number of dates between between group pairs.
			// goal: found match dates should be as close together as possible

			// start with 1st dates, end with last but one dates
			for (int i = 0; i < matchDates.Count - 1; i++)
			{
				// start with 2nd dates, end with last dates
				for (int j = 1; j < matchDates.Count; j++)
				{
					// compare each date in the first list...
					foreach (var dates1 in matchDates[i])
					{
						// ... with the dates in the second list
						foreach (var dates2 in matchDates[j])
						{
							var diff = Math.Abs((dates1.MatchStartTime.Date - dates2.MatchStartTime.Date).Days);
							
							// save minimum dates found for later reference
							if (diff < dates1.MinTimeDiff.Days)
								dates1.MinTimeDiff = new TimeSpan(diff, 0, 0, 0);

							if (diff < dates2.MinTimeDiff.Days)
								dates2.MinTimeDiff = new TimeSpan(diff, 0, 0, 0);
						}  // end row2
					} // end row1
				} // end j

				// get the date that has least distance to smallest date in other group(s)
				var bestDate = matchDates[i].Where(md => md.MinTimeDiff == matchDates[i].Min(d => d.MinTimeDiff)).OrderBy(md => md.MinTimeDiff).First();
				//var bestDate = matchDates[i].Where(md => md.MinTimeDiff == matchDates[i].Min(d => d.MinTimeDiff) && md.MatchStartTime > maxDateOfCombination.AddDays(14)).OrderBy(md => md.MinTimeDiff).First();

				matchDatePerCombination.Add(bestDate);
				
				// process the last combination

				// in case comparisons took place,
				// now the "j-loop" group is not processed yet:
				if (i + 1 >= matchDates.Count - 1)
				{
					bestDate = matchDates[matchDates.Count - 1].Where(md => md.MinTimeDiff == matchDates[matchDates.Count - 1].Min(d => d.MinTimeDiff)).OrderBy(md => md.MinTimeDiff).First();
					//bestDate = matchDates[matchDates.Count - 1].Where(md => md.MinTimeDiff == matchDates[matchDates.Count - 1].Min(d => d.MinTimeDiff) && md.MatchStartTime > maxDateOfCombination.AddDays(14)).OrderBy(md => md.MinTimeDiff).First();
					// the last "j-increment" is always the same as "matchDates.Count - 1" (loop condition)
					matchDatePerCombination.Add(bestDate);
				}
			} // end i

			return matchDatePerCombination;			
		}


		private void AssignRoundDateSegments(RoundLegEntity roundLeg, Collection<TeamCombinationGroup<long>> bundledGroups)
		{
			var allMatchDaysOfRound = _availableMatchDates.GetAllAvailableMatchDays(roundLeg);

			int segmentDaysCount = allMatchDaysOfRound.Count / (bundledGroups.Count + 1);

			int start = 0;
			int index = 0;

			_log.AppendFormat("*** {0} - {1}\n", roundLeg.Round.Name, roundLeg.Description);
			while (start < allMatchDaysOfRound.Count && index < bundledGroups.Count)
			{
				
				//TODO: Es könnte ein Rest an Tagen übrigbleiben wegen Integer-Division!
				int end = (start + segmentDaysCount) < allMatchDaysOfRound.Count ? start + segmentDaysCount : allMatchDaysOfRound.Count - 1;
				bundledGroups[index].DateTimePeriod = new DateTimePeriod(allMatchDaysOfRound[start].Date, allMatchDaysOfRound[end].Date);

				_log.AppendFormat("{0:dd.MM.yyyy} bis {1:dd.MM.yyyy} - {2} Tage\n", bundledGroups[index].DateTimePeriod.Start,
											   bundledGroups[index].DateTimePeriod.End, (bundledGroups[index].DateTimePeriod.End.Value - bundledGroups[index].DateTimePeriod.Start.Value).Days);

				start = end + 1;
				index++;
			}
		}

		[Obsolete("GeneratePlannedMatchesFor... methods are now able to create missing matches for new teams with all rules applied", true)]
		public void AddPlannedMatchesForTeam(long teamId)
		{
			using (var da = _appDb.DbContext.GetNewAdapter())
			{
				var metaData = new LinqMetaData(da);

				// team must be new in matches table
				var teamExists = metaData.Match.Any(m => (m.HomeTeamId == teamId) || (m.GuestTeamId == teamId));
				
				if (teamExists)
					throw new ArgumentException("Planned matches can only be added if no matches exist for the team.");

				// get team
				var team = metaData.Team.First(t => t.Id == teamId);
				// get round with its legs for this team
				var round = (metaData.TeamInRound.Where(tir => tir.TeamId == teamId).Select(tir => tir.Round).WithPath(new PathEdge<VenueEntity>(RoundEntity.PrefetchPathRoundLegs))).First();
				// get OTHER teams in the round including venue

				IEnumerable<TeamEntity> otherTeams = (from tirEntity in metaData.TeamInRound
													  where tirEntity.RoundId == round.Id && tirEntity.TeamId != team.Id
													  select tirEntity.Team).WithPath(new PathEdge<VenueEntity>(TeamEntity.PrefetchPathVenue));

				var matches = new EntityCollection<MatchEntity>();

				foreach (var leg in round.RoundLegs)
				{
					var dummyStartDate = leg.StartDateTime;

					bool? firstMatchOfLegIsHomeForTeam = !matches.Any() || matches.First(m => m.LegSequenceNo == leg.SequenceNo - 1).GuestTeamId == team.Id;
					
					foreach (var t in otherTeams)
					{
						// If last match was home match for team, make external match for next match
						long homeTeamId;
						long guestTeamId;
						long venueId;
						if ((firstMatchOfLegIsHomeForTeam.HasValue && firstMatchOfLegIsHomeForTeam.Value) || (matches[matches.Count() - 1].HomeTeamId != team.Id && matches[matches.Count() - 1].LegSequenceNo == leg.SequenceNo))
						{
							homeTeamId = team.Id;
							guestTeamId = t.Id;
							venueId = team.VenueId.Value;
							firstMatchOfLegIsHomeForTeam = null;
						}
						else
						{
							homeTeamId = t.Id;
							guestTeamId = team.Id;
							venueId = t.VenueId.Value;
						}

						var match = new MatchEntity()
						{
							HomeTeamId = homeTeamId,
							GuestTeamId = guestTeamId,
							RefereeId = homeTeamId,
							RoundId = round.Id,
							VenueId = venueId,
							LegSequenceNo = leg.SequenceNo,
							IsComplete = false,
							Remarks = string.Empty,
							PlannedStart = dummyStartDate.Date.Add(team.Id == homeTeamId ? team.MatchTime.Value : otherTeams.First(ot => ot.Id == homeTeamId).MatchTime.Value),
							CreatedOn = DateTime.Now,
						};
						match.PlannedEnd = match.PlannedStart.Value.Add(PlannedDurationOfMatch);
						match.ModifiedOn = match.CreatedOn;

						matches.Add(match);
						dummyStartDate = dummyStartDate.AddDays(1);
					}
				}

				var count = da.SaveEntityCollection(matches);
				da.CloseConnection();
			}
		}

		[Obsolete("Only for test purposes!!")]
		private void TestRoundRobin()
		{
			var teams = new Collection<long>();
			foreach (var team in _tournament.Rounds[0].TeamCollectionViaTeamInRound)
			{
				teams.Add(team.Id);
			}

			var roundRobin = new RoundRobinSystem<long>(teams);
			Collection<TeamCombinationGroup<long>> bundledGroups =
				roundRobin.GetBundledGroups(RefereeType.HomeTeam, LegType.First, CombinationGroupOptimization.GroupWithAlternatingHomeGuest);

		}
	}
}