using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.MultiTenancy;

namespace TournamentManager;

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
    private readonly ILogger _logger = AppLogging.CreateLogger<TournamentCreator>();
    private readonly IAppDb _appDb;
    private static TournamentCreator? _instance;

    private TournamentCreator(IAppDb appDb)
    {
        _appDb = appDb;
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
    /// <param name="cancellationToken"></param>
    /// <returns>True, if creation was successful, false otherwise.</returns>
    public async Task<bool> CopyTournament (long fromTournamentId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var tournament =
            await _appDb.TournamentRepository.GetTournamentAsync(
                new PredicateExpression(TournamentFields.Id == fromTournamentId), CancellationToken.None)
            ?? throw new InvalidOperationException($"'{fromTournamentId}' not found.");

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

        await _appDb.GenericRepository.SaveEntityAsync(newTournament, true, false, cancellationToken);

        tournament.NextTournamentId = newTournament.Id;
        tournament.ModifiedOn = now;

        // save last tournament
        return await _appDb.GenericRepository.SaveEntityAsync(tournament, true, false, cancellationToken);
    }


    /// <summary>
    /// Copies the round basic data and the round leg data
    /// from the source to an existing target tournament. The new tournament id must
    /// already exist. Leg data for each round is taken over from target tournament legs
    /// on a 1:1 base (same number of legs, dates/times).
    /// </summary>
    /// <param name="fromTournamentId">Existing source tournament id.</param>
    /// <param name="toTournamentId">Existing target tournament id.</param>
    /// <param name="excludeRoundId">List of round id's to be excluded (empty list for 'none')</param>
    /// <param name="cancellationToken"></param>
    /// <returns>True, if creation was successful, false otherwise.</returns>
    public async Task<bool> CopyRound(long fromTournamentId, long toTournamentId, IList<long> excludeRoundId, CancellationToken cancellationToken)
    {
        const string transactionName = "CopyRounds";
        var now = DateTime.UtcNow;
        
        // get the rounds of SOURCE tournament
        var roundIds = (await _appDb.TournamentRepository.GetTournamentRoundsAsync(fromTournamentId, cancellationToken)).Select(r => r.Id).ToList();

        using var da = _appDb.DbContext.GetNewAdapter();

        var roundsWithLegs = new Queue<RoundEntity>();
        foreach (var r in roundIds)
        {
            var round = await _appDb.RoundRepository.GetRoundWithLegsAsync(r, cancellationToken);
            if (round != null) roundsWithLegs.Enqueue(round);
        }

        try
        {
            await da.StartTransactionAsync(System.Data.IsolationLevel.ReadUncommitted, transactionName, cancellationToken);

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
                    SetRuleId = round.SetRuleId,
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

                // save recursively (new round with the new round legs)
                await da.SaveEntityAsync(newRound, true, true, cancellationToken);
            }

            // commit after all rounds are processed successfully
            await da.CommitAsync(cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Error cloning rounds in transaction: New TournamentId={tournamentId}", toTournamentId);

            if (da.IsTransactionInProgress)
                da.Rollback(transactionName);

            return false;
        }
    }

    public async Task<bool> SetLegDates(IEnumerable<RoundEntity> rounds , int sequenceNo, DateTime start, DateTime end, CancellationToken cancellationToken)
    {
        const string transactionName = "SetLegDates";
        var now = DateTime.UtcNow;

        var roundEntities = (rounds as RoundEntity[] ?? rounds.ToArray()).ToList();

        if (roundEntities.Count == 0)
            return false;

        var roundIds = roundEntities.Select(r => r.Id).ToList();
        roundEntities.Clear();
        foreach (var rid in roundIds)
        {
            roundEntities.Add((await _appDb.RoundRepository.GetRoundWithLegsAsync(rid, cancellationToken))!);
        }
			
        var tournamentId = roundEntities.First().TournamentId;
        if (!tournamentId.HasValue)
            return false;

        using var da = _appDb.DbContext.GetNewAdapter();

        try
        {
            await da.StartTransactionAsync(System.Data.IsolationLevel.ReadUncommitted, transactionName, cancellationToken);

            foreach (var round in roundEntities)
            {
                foreach (var leg in round.RoundLegs.Where(l => l.SequenceNo == sequenceNo))
                {
                    leg.StartDateTime = start;
                    leg.EndDateTime = end;
                    leg.ModifiedOn = now;

                    await da.SaveEntityAsync(leg, false, false, cancellationToken);
                }
            }
            await da.CommitAsync(cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Error updating round legs in transaction: TournamentId={tournamentId}", tournamentId);

            if (da.IsTransactionInProgress)
                da.Rollback();
            return false;
        }
    }

    /// <summary>
    /// If all matches of a tournament are completed, the rounds and the tournament are set to &quot;completed&quot;.
    /// </summary>
    /// <param name="tournamentId">The Tournament to be set as &quot;completed&quot;</param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="ArgumentException">Throws an exception if any match of the tournament is not completed yet.</exception>
    public async Task SetTournamentCompleted(long tournamentId, CancellationToken cancellationToken)
    {
        if (!await _appDb.MatchRepository.AllMatchesCompletedAsync(new TournamentEntity(tournamentId), cancellationToken))
        {
            throw new InvalidOperationException($@"Tournament {tournamentId} contains incomplete matches.");
        }

        var tournament = await _appDb.TournamentRepository.GetTournamentWithRoundsAsync(tournamentId, CancellationToken.None) ?? throw new InvalidOperationException($"Tournament with Id '{tournamentId}' not found.");
        var now = DateTime.UtcNow;

        foreach (var round in tournament.Rounds)
        {
            await SetRoundCompleted(round, cancellationToken);
        }

        tournament.IsComplete = true;
        tournament.ModifiedOn = now;

        using var da = _appDb.DbContext.GetNewAdapter();
        if (!await da.SaveEntityAsync(tournament, true, true, cancellationToken))
        {
            throw new InvalidOperationException($"Tournament Id {tournamentId} could not be saved to persistent storage.");
        }
    }

    public virtual async Task SetRoundCompleted(RoundEntity round, CancellationToken cancellationToken)
    {
        if (!await _appDb.MatchRepository.AllMatchesCompletedAsync(round, cancellationToken))
            throw new InvalidOperationException($"Round {round.Id} has uncompleted matches.");

        using var da = _appDb.DbContext.GetNewAdapter();
        da.FetchEntity(round);
        round.IsComplete = true;
        round.ModifiedOn = DateTime.UtcNow;
        await da.SaveEntityAsync(round, cancellationToken);
    }
}
