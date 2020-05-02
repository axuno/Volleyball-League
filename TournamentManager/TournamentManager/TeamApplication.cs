using System;
using System.Linq;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;

namespace TournamentManager.Data
{
	public class TeamApplication
	{
		public enum RoundType
		{
			Ladies3Gents3 = 1, 
			Arbitrary = 2
		}

		private DateTime _now = DateTime.Now;
		private TeamEntity _team;

	    private readonly AppDb _appDb;

	    public TeamApplication(IAppDb appDb)
	    {
	        _appDb = (AppDb) appDb;
	    }

        public TeamApplication(IAppDb appDb, long plannedTournamentId) : this(appDb)
		{
			PlannedTournament = GetTournamentWithPreviousAndRounds(plannedTournamentId);
		}

		public static TournamentEntity PreviousTournament
		{
			get;
			private set;
		}

		public static TournamentEntity PlannedTournament
		{
			get;
			private set;
		}

		public TeamEntity TeamEntity
		{
			get { return _team; }

			set
			{
				_team = value;

				if (OnTeamEntityChanged != null)
					OnTeamEntityChanged(this, new EventArgs());
			}
		}

		/// <summary>
		/// Event rising after the TeamEntity property was changed.
		/// </summary>
		public event EventHandler<EventArgs> OnTeamEntityChanged;

		public ManagerOfTeamEntity GetContact()
		{
			return TeamEntity.ManagerOfTeams.FirstOrDefault();
		}

		public void CreateNewTeam(RoundType roundType)
		{
			var t = new TeamEntity();
			DeriveNewRoundFromPreviousTournament(ref t, roundType);
			t.Venue = null;
			t.MatchDayOfWeek = 1; // Monday
			t.Name = t.ClubName = string.Empty;
			t.CreatedOn = t.ModifiedOn = _now;
			TeamEntity = t;
		}


		public void AddExistingTeamToNewTournament(long teamId, RoundType roundType)
		{
			var team = GetTeamsWithRelatedEntities(null, teamId, false).FirstOrDefault();
			DeriveNewRoundFromPreviousTournament(ref team, roundType);
			team.ModifiedOn = _now;
			TeamEntity = team;
		}

		private void DeriveNewRoundFromPreviousTournament(ref TeamEntity team, RoundType roundType)
		{
			var t = team;
			RoundEntity newRound = null;

			if (t.TeamInRounds.Any())
			{
                // get the round name of the latest tournament the team took part
			    var oldRoundName = t.TeamInRounds.First(tir => tir.Round.TournamentId == t.TeamInRounds.Max(tir2 => tir2.Round.TournamentId)).Round.Name;
                newRound = PlannedTournament.Rounds.FirstOrDefault(r => r.Name == oldRoundName && r.TypeId == (long) roundType);
			}

			//Note: team.TeamInRounds.Add(new TeamInRoundEntity() { Team = team, Round = newRound, CreatedOn = _now, ModifiedOn = _now }) will add twice!!
			if (newRound != null)
			{
				var tir = new TeamInRoundEntity() { Team = team, Round = newRound, CreatedOn = _now, ModifiedOn = _now };
			}
			else
			{
				var tir = new TeamInRoundEntity() { Team = team, Round = PlannedTournament.Rounds.Last(r => r.TypeId == (long)roundType), CreatedOn = _now, ModifiedOn = _now };
			}
		}

		public UserEntity SubmittingManager { get; set; }

		
		public void GetTeamWithRelatedEntities(TeamEntity team)
		{
			using (var da = _appDb.DbContext.GetNewAdapter())
			{
				da.FetchEntity(team, GetTeamsWithRelatedEntitiesPrefetchPath(null));
			}
		}

		public EntityCollection<TeamEntity> GetTeamsWithRelatedEntities(long? tournamentId, long? teamId = null, bool refillCache = false)
		{
			var teams = new EntityCollection<TeamEntity>();
			GetTeamsWithRelatedEntities(teams, tournamentId, teamId, refillCache);
			return teams;
		}

		public void GetTeamsWithRelatedEntities(EntityCollection<TeamEntity> teams, long? tournamentId, long? teamId = null, bool refillCache = false)
		{
			using (var da = _appDb.DbContext.GetNewAdapter())
			{
				IRelationPredicateBucket bucket = new RelationPredicateBucket();
				if (tournamentId.HasValue) bucket.PredicateExpression.AddWithAnd(TournamentFields.Id == tournamentId.Value);
				if (teamId.HasValue) bucket.PredicateExpression.AddWithAnd(TeamInRoundFields.TeamId == teamId.Value);

				bucket.Relations.Add(TeamEntity.Relations.ManagerOfTeamEntityUsingTeamId);
				bucket.Relations.Add(TeamEntity.Relations.TeamInRoundEntityUsingTeamId);
				bucket.Relations.Add(TeamInRoundEntity.Relations.RoundEntityUsingRoundId);
				bucket.Relations.Add(RoundEntity.Relations.TournamentEntityUsingTournamentId);

				var pfp = GetTeamsWithRelatedEntitiesPrefetchPath(tournamentId);
				var parameters = new QueryParameters(0, 0, 0, bucket)
				{
					OverwriteIfPresent = refillCache,
					CollectionToFetch = teams,
					PrefetchPathToUse = pfp,
					CacheResultset = true,
					CacheDuration = new TimeSpan(0, 30, 0)
				};

				da.FetchEntityCollection(parameters);

				da.CloseConnection();
			}
		}

		private IPrefetchPath2 GetTeamsWithRelatedEntitiesPrefetchPath(long? tournamentId)
		{
			IPrefetchPath2 pfp = new PrefetchPath2(EntityType.TeamEntity);
			pfp.Add(TeamEntity.PrefetchPathManagerOfTeams);
			pfp[0].SubPath.Add(ManagerOfTeamEntity.PrefetchPathUser);
			pfp.Add(TeamEntity.PrefetchPathVenue);
			pfp.Add(TeamEntity.PrefetchPathTeamInRounds);
			pfp[2].SubPath.Add(TeamInRoundEntity.PrefetchPathRound);
			// if (tournamentId.HasValue)  With this filter Round stay empty!
			//	pfp[2].SubPath[0].Filter.AddWithAnd(RoundFields.TournamentId == tournamentId);
			pfp[2].SubPath[0].SubPath.Add(RoundEntity.PrefetchPathRoundType);
			pfp[2].SubPath[0].SubPath.Add(RoundEntity.PrefetchPathTournament);

			return pfp;
		}

        public TournamentEntity GetTournamentWithPreviousAndRounds(long tournamentId)
        {
            using (var da = _appDb.DbContext.GetNewAdapter())
            {
                var tournament = new TournamentEntity(tournamentId);
                var pfp = new PrefetchPath2(EntityType.TournamentEntity);
                pfp.Add(TournamentEntity.PrefetchPathRounds);
                pfp[0].SubPath.Add(RoundEntity.PrefetchPathRoundType);

                da.FetchEntity(tournament, pfp);
                return tournament;
            }
        }
	}
}