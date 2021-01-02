using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.MultiTenancy;


namespace TournamentManager.Data
{
	/// <summary>
	/// The Copy class is used to copy an existing tournament
	/// to a new one. Usually, the sequence is as follows:
	/// 1. Copy the tournament e.g. from id 10 to 11:
	///    Copy.Tournament(10, 11);
	/// 2. Copy the rounds of a tournament:
	///    Copy.Round(10, 11, null);
	///    (Rounds not needed can be given in the list of 3rd parameter)
	/// 3. Copy the teams of tournament 
	///    and assign the teams to the rounds created in step 2:
	///    Copy.TeamsWithPersons(10, 11, null);
	///    (Teams not needed can be given in the list of 3rd parameter)
	/// </summary>
	public class TournamentCreator
	{
	    private readonly AppDb _appDb;
	    private static TournamentCreator? _instance;

        TournamentCreator(IAppDb appDb)
	    {
	        _appDb = (AppDb) appDb;
	    }

        public static TournamentCreator Instance(IAppDb appDb)
	    {
            _instance ??= new TournamentCreator(appDb);
	        return _instance;
	    }

	    /// <summary>
		/// Copies the tournament basic data and the tournament leg data
		/// from the source to a new target tournament. The new tournament id must
		/// not exist. For start and end date of leg data 1 year is added.
		/// </summary>
		/// <param name="fromTournamentId">Existing source tournament id.</param>
		/// <returns>True, if creation was successful, false otherwise.</returns>
		public async Task<bool> CopyTournament (long fromTournamentId)
		{
			var now = DateTime.Now;
			var tournament = await _appDb.TournamentRepository.GetTournamentAsync(new PredicateExpression(TournamentFields.Id == fromTournamentId), CancellationToken.None);
            if (tournament is null) throw new NullReferenceException($"'{fromTournamentId}' not found.");
            
			var newTournament = new TournamentEntity
             	{
             		IsPlanningMode = true,
             		Name = tournament.Name,
					Description = tournament.Description.Length == 0 ? null : tournament.Description,
             		TypeId = tournament.TypeId,
             		IsComplete = false,
             		CreatedOn = now,
             		ModifiedOn = now,
             	};

            _appDb.GenericRepository.SaveEntity(newTournament, true, false);

		    tournament.NextTournamentId = newTournament.Id;
		    tournament.ModifiedOn = now;

            // save last tournament
            return _appDb.GenericRepository.SaveEntity(tournament, true, false);
		}


		/// <summary>
		/// Copies the round basic data and the round leg data
		/// from the source to an existing target tournament. The new tournament id must
		/// already exist. Leg data for each round is taken over from target tournament legs
		/// on a 1:1 base (same number of legs, dates/times).
		/// </summary>
		/// <param name="fromTournamentId">Existing source tournament id.</param>
		/// <param name="toTournamentId">Existing target tournament id.</param>
		/// <param name="excludeRoundId">List of round id's to be excluded (may be null for none)</param>
		/// <returns>True, if creation was successful, false otherwise.</returns>
		public bool CopyRound(long fromTournamentId, long toTournamentId, IEnumerable<long> excludeRoundId)
		{
			const string transactionName = "CloneRounds";
			if (excludeRoundId == null) excludeRoundId = new List<long>();
			DateTime now = DateTime.Now;

			// get the rounds of SOURCE tournament
			var roundIds = _appDb.TournamentRepository.GetTournamentRounds(fromTournamentId).Select(r => r.Id).ToList();

			using (var da = _appDb.DbContext.GetNewAdapter())
			{
				// da.StartTransaction(System.Data.IsolationLevel.ReadUncommitted, transactionName);
				var roundsWithLegs = new Queue<RoundEntity>();
                foreach (var r in roundIds)
                {
					roundsWithLegs.Enqueue(_appDb.RoundRepository.GetRoundWithLegs(r));
                }
				
				foreach (var r in roundIds)
                {
                    var round = roundsWithLegs.Dequeue();

                    // skip excluded round id's
					if (excludeRoundId.Contains(r))
						continue;

					// create new round and overtake data of source round
					var newRound = new RoundEntity()
					{
						TournamentId = toTournamentId,
						Name = round.Name,
						Description = round.Description,
						TypeId = round.TypeId,
						NumOfLegs = round.NumOfLegs,
						MatchRuleId = round.MatchRuleId,
						SetRuleId = round.MatchRuleId,
						IsComplete = false,
						CreatedOn = now,
						ModifiedOn = now,
						NextRoundId = null
					};

					// create the round leg records based on the TARGET tournament legs
					foreach (var rl in round.RoundLegs)
					{
						var newRoundLeg = new RoundLegEntity()
						{
							SequenceNo = rl.SequenceNo,
							Description = rl.Description,
							StartDateTime = rl.StartDateTime,
							EndDateTime = rl.EndDateTime,
							CreatedOn = now,
							ModifiedOn = now
						};
						newRound.RoundLegs.Add(newRoundLeg);
					}

					// save recursively (new round with its new round legs)
					if (! da.SaveEntity(newRound, true, true))
					{
						// roll back if any round fails
						da.Rollback(transactionName);
						return false;
					}
				}

				// commit only after all rounds are processed successfully
				da.Commit();
			}
			return true;
		}

		public bool SetLegDates(IEnumerable<RoundEntity> rounds , int sequenceNo, DateTime start, DateTime end)
		{
			//const string transactionName = "SetLegDates";
			var now = DateTime.Now;

            var roundEntities = (rounds as RoundEntity[] ?? rounds.ToArray()).ToList();

			if (!roundEntities.Any())
				return false;

            var roundIds = roundEntities.Select(r => r.Id).ToList();
			roundEntities.Clear();
            foreach (var rid in roundIds)
            {
                roundEntities.Add(_appDb.RoundRepository.GetRoundWithLegs(rid));
            }
			
			var tournamentId = roundEntities.First().TournamentId;
			if (!tournamentId.HasValue)
				return false;

			using (var da = _appDb.DbContext.GetNewAdapter())
			{
                //da.StartTransaction(System.Data.IsolationLevel.ReadUncommitted, transactionName);
                
				foreach (var round in roundEntities)
				{
					foreach (var leg in round.RoundLegs.Where(l => l.SequenceNo == sequenceNo))
					{
						leg.StartDateTime = start;
						leg.EndDateTime = end;
						leg.ModifiedOn = now;

						if (!da.SaveEntity(leg, false, false))
						{
							da.Rollback();
							return false;
						}
					}
				}
				//da.Commit();
			}
			
			return true;
		}

        /// <summary>
        /// If all matches of a tournament are completed, the rounds and the tournament are set to &quot;completed&quot;.
        /// </summary>
        /// <param name="tournamentId">The Tournament to be set as &quot;completed&quot;</param>
        /// <exception cref="ArgumentException">Throws an exception if any match of the tournament is not completed yet.</exception>
        public async Task SetTournamentCompleted(long tournamentId)
        {
            if (!new MatchRepository(_appDb.DbContext).AllMatchesCompleted(new TournamentEntity(tournamentId)))
            {
                throw new ArgumentException($"Tournament {tournamentId} contains incomplete matches.");
            }

            var tournament = await new TournamentRepository(_appDb.DbContext).GetTournamentWithRoundsAsync(tournamentId, CancellationToken.None);
            
            var now = DateTime.Now;

            foreach (var round in tournament.Rounds)
            {
                SetRoundCompleted(round);
            }

            tournament.IsComplete = true;
            tournament.ModifiedOn = now;

            using (var da = _appDb.DbContext.GetNewAdapter())
            {
                if (!da.SaveEntity(tournament))
                {
                    throw new ArgumentException($"Tournament Id {tournamentId} could not be saved to persistent storage.");
                }
            }
        }

        public virtual void SetRoundCompleted(RoundEntity round)
        {
            if (!new MatchRepository(_appDb.DbContext).AllMatchesCompleted(round))
                throw new ArgumentException($"Round {round.Id} has uncompleted matches.");

            using (var da = _appDb.DbContext.GetNewAdapter())
            {
                da.FetchEntity(round);
                round.IsComplete = true;
                round.ModifiedOn = DateTime.Now;
                da.SaveEntity(round);
                da.CloseConnection();
            }
        }
	}
}
