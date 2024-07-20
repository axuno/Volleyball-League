using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.MultiTenancy;

namespace TournamentManager;

/// <summary>
/// The TournamentCreator class is responsible for copying an existing tournament to a new one.
/// It provides methods to copy the tournament, rounds, and round legs from the source tournament to the target tournament.
/// </summary>
public class TournamentCreator
{
    /// <summary>
    /// Arguments for the <see cref="CopyTournament"/> method.
    /// </summary>
    /// <param name="SourceTournamentId">The ID of the source tournament.</param>
    /// <param name="SetSourceTournamentCompleted">Try to set the source tournament as completed.</param>
    /// <param name="TargetName">The name for the target tournament.</param>
    /// <param name="TargetDescription">The description for the target tournament.</param>
    /// <param name="TargetLegDates">The leg dates to use for the target tournament round legs. The list cover maximum number of legs across akk rounds.</param>
    /// <param name="RoundsToExclude">The round IDs to exclude from copy.</param>
    /// <param name="DisableChecks">Disable safety checks.</param>
    /// <param name="ModifiedOn">The <see cref="DateTime"/> to use for CreatedOn / ModifiedOn of entities.</param>
    public record CopyTournamentArgs(
        long SourceTournamentId,
        bool SetSourceTournamentCompleted,
        string TargetName,
        string? TargetDescription,
        IList<(DateTime Start, DateTime End)> TargetLegDates,
        IList<long> RoundsToExclude,
        bool DisableChecks,
        DateTime ModifiedOn);

    private readonly ILogger<TournamentCreator> _logger;
    private readonly IAppDb _appDb;
    private DateTime _modifiedOn;
    private static TournamentCreator? _instance;


    private TournamentCreator(IAppDb appDb, ILogger<TournamentCreator> logger)
    {
        _appDb = appDb;
        _logger = logger;
    }

    /// <summary>
    /// Returns the singleton instance of the <see cref="TournamentCreator"/> class.
    /// </summary>
    /// <param name="appDb"></param>
    /// <param name="logger"></param>
    /// <returns>The singleton instance of the <see cref="TournamentCreator"/> class.</returns>
    public static TournamentCreator Instance(IAppDb appDb, ILogger<TournamentCreator> logger)
    {
        _instance ??= new TournamentCreator(appDb, logger);
        return _instance;
    }

    /// <summary>
    /// Creates a new tournament from the source tournament.
    /// </summary>
    /// <param name="copyArgs">Definition of changes from source to new tournament.</param>
    /// <param name="cancellationToken"></param>
    /// <code>
    /// var copyArgs = new TournamentCreator.CopyTournamentArgs(25, false, "Tournament 2024/25", null,
    /// new List&lt;(DateTime Start, DateTime End)&gt; {
    ///    (new DateTime(2024, 9, 23), new DateTime(2025, 2, 1)), // 1st leg
    ///    (new DateTime(2025, 2, 3), new DateTime(2025, 5, 30))  // 2nd leg
    /// }, Array.Empty&lt;long&gt;(), false, DateTime.UtcNow);
    ///         
    /// var success = await TournamentCreator
    ///    .Instance(AppDb, AppLogging.CreateLogger&lt;TournamentCreator&gt;())
    ///    .CreateNewFromSourceTournament(copyArgs, CancellationToken.None);
    /// </code>
    /// <returns><see langword="true"/>, if the new tournament was created successfully.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<bool> CreateNewFromSourceTournament(CopyTournamentArgs copyArgs, CancellationToken cancellationToken)
    {
        if (copyArgs.SetSourceTournamentCompleted) await SetTournamentCompleted(copyArgs.SourceTournamentId, cancellationToken);

        var (sourceTournament, targetTournament) = await CopyTournament(copyArgs, cancellationToken);

        var success = await CopyRoundsWithLegsToTarget(copyArgs, targetTournament, cancellationToken);

        if (!success) return success;

        try
        {
            await _appDb.TournamentRepository.SaveTournamentsAsync(sourceTournament, targetTournament, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Error saving Tournaments from {TournamentCreator}", nameof(TournamentCreator));
            return false;
        }

        return true;
    }


    /// <summary>
    /// Copies the tournament basic data from the source to a new target tournament <see cref="TournamentEntity"/>.
    /// </summary>
    /// <param name="copyArgs">The <see cref="CopyTournamentArgs"/> to be used.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="ValueTuple"/> with the Source (unchanged) and the new Target <see cref="TournamentEntity"/>.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal async Task<(TournamentEntity Source, TournamentEntity Target)> CopyTournament (CopyTournamentArgs copyArgs, CancellationToken cancellationToken)
    {
        _modifiedOn = copyArgs.ModifiedOn;
        var sourceTournament =
            await _appDb.TournamentRepository.GetTournamentAsync(
                new PredicateExpression(TournamentFields.Id == copyArgs.SourceTournamentId), CancellationToken.None)
            ?? throw new InvalidOperationException($"'{copyArgs.SourceTournamentId}' not found.");

        if(copyArgs.DisableChecks)
        {
            _logger.LogWarning("Safety checks disabled.");
        }
        else
        {
            if (sourceTournament.IsPlanningMode)
            {
                throw new InvalidOperationException($"Source tournament {sourceTournament.Id} is in planning mode.");
            }

            if (sourceTournament.NextTournamentId.HasValue)
            {
                throw new InvalidOperationException($"Source tournament {sourceTournament.Id} has already a next tournament.");
            }
        }

        // Create the target tournament
        var targetTournament = new TournamentEntity
        {
            IsPlanningMode = true,
            Name = copyArgs.TargetName,
            Description = copyArgs.TargetDescription,
            TypeId = sourceTournament.TypeId,
            IsComplete = false,
            CreatedOn = _modifiedOn,
            ModifiedOn = _modifiedOn,
        };

        // Update the source tournament
        // sourceTournament.NextTournamentId (and sourceTournament.ModifiedOn) can be set immediately
        // after the target tournament is saved, and NextTournamentId is NULL

        return (sourceTournament, targetTournament);
    }

    /// <summary>
    /// Copies the round basic data and the round leg data
    /// from the source to an existing target tournament. Leg basic data for each round is copied from the source to the target tournament.
    /// </summary>
    /// <param name="args"></param>
    /// <param name="targetTournament"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The <paramref name="targetTournament"/> <see cref="TournamentEntity"/> with rounds and round legs added.</returns>
    internal async Task<bool> CopyRoundsWithLegsToTarget(CopyTournamentArgs args, TournamentEntity targetTournament, CancellationToken cancellationToken)
    {
        // get the round IDs of the SOURCE tournament
        var sourceRoundIds = await _appDb.TournamentRepository.GetTournamentRoundIdsAsync(args.SourceTournamentId, cancellationToken);
        
        foreach (var sourceRoundId in sourceRoundIds)
        {
            var sourceRound = await _appDb.RoundRepository.GetRoundWithLegsAsync(sourceRoundId, cancellationToken);

            if (sourceRound is null)
            {
                _logger.LogCritical("Round {RoundId} not found.", sourceRoundId);
                return false;
            }

            // skip excluded round id's
            if (args.RoundsToExclude.Contains(sourceRoundId))
            {
                _logger.LogDebug("Round {RoundId} excluded from copy.", sourceRoundId);
                continue;
            }

            // Create target round from the source round
            var targetRound = new RoundEntity
            {
                Tournament = targetTournament, // this adds the round to the target tournament
                Name = sourceRound.Name,
                Description = sourceRound.Description,
                TypeId = sourceRound.TypeId,
                NumOfLegs = sourceRound.NumOfLegs,
                MatchRuleId = sourceRound.MatchRuleId,
                SetRuleId = sourceRound.SetRuleId,
                IsComplete = false,
                CreatedOn = _modifiedOn,
                ModifiedOn = _modifiedOn,
                NextRoundId = null
            };

            var legDates = args.TargetLegDates;

            if (sourceRound.RoundLegs.Count > legDates.Count)
            {
                _logger.LogCritical("Round {RoundId} has {Legs} legs, but only {LegDates} leg dates provided.", sourceRoundId, sourceRound.RoundLegs.Count, legDates.Count);
                return false;
            }

            // Create the round leg records based on the TARGET tournament legs, but use new log dates
            for (var index = 0; index < sourceRound.RoundLegs.Count; index++)
            {
                var rl = sourceRound.RoundLegs[index];
                _ = new RoundLegEntity
                {
                    Round = targetRound, // this adds the round leg to the target round
                    SequenceNo = rl.SequenceNo,
                    Description = rl.Description,
                    StartDateTime = legDates[index].Start,
                    EndDateTime = legDates[index].End,
                    CreatedOn = _modifiedOn,
                    ModifiedOn = _modifiedOn
                };
            }

            _logger.LogDebug("Round {RoundId} with {Legs} legs copied to target tournament.", sourceRoundId, sourceRound.RoundLegs.Count);
        }

        _logger.LogDebug("Target tournament contains {RoundCount} rounds.", targetTournament.Rounds.Count);

        return true;
    }

    /// <summary>
    /// Sets the start and end dates for the legs having the <paramref name="sequenceNo"/> of the specified <paramref name="rounds"/>.
    /// </summary>
    /// <param name="rounds"></param>
    /// <param name="sequenceNo"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="cancellationToken"></param>
    /// <returns><see langword="true"/>, if successful.</returns>
    public async Task<bool> SetLegDates(ICollection<RoundEntity> rounds , int sequenceNo, DateTime start, DateTime end, CancellationToken cancellationToken)
    {
        if (rounds.Count == 0)
            return false;

        var roundIds = rounds.Select(r => r.Id).ToList();
        rounds.Clear();
        foreach (var rid in roundIds)
        {
            rounds.Add((await _appDb.RoundRepository.GetRoundWithLegsAsync(rid, cancellationToken))!);
        }
			
        var tournamentId = rounds.First().TournamentId;
        if (!tournamentId.HasValue)
            return false;

        try
        {
            foreach (var round in rounds)
            {
                foreach (var leg in round.RoundLegs.Where(l => l.SequenceNo == sequenceNo))
                {
                    leg.StartDateTime = start;
                    leg.EndDateTime = end;
                    leg.ModifiedOn = _modifiedOn;

                    await _appDb.GenericRepository.SaveEntityAsync(leg, false, false, cancellationToken);
                    _logger.LogDebug("RoundLeg {RoundLegId} updated with new dates.", leg.Id);
                }
            }

            return true;
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Error updating round legs: TournamentId={TournamentId}", tournamentId);

            return false;
        }
    }

    /// <summary>
    /// If all matches of a tournament are completed, the rounds and the tournament are set to &quot;completed&quot;.
    /// </summary>
    /// <param name="tournamentId">The Tournament to be set as &quot;completed&quot;</param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="InvalidOperationException">Throws an exception if any match of the tournament is not completed yet.</exception>
    public async Task SetTournamentCompleted(long tournamentId, CancellationToken cancellationToken)
    {
        if (!await _appDb.MatchRepository.AllMatchesCompletedAsync(new TournamentEntity(tournamentId), cancellationToken))
        {
            var ex = new InvalidOperationException($@"Tournament {tournamentId} contains incomplete matches.");
            _logger.LogCritical(ex,@"Tournament {TournamentId} contains incomplete matches.", tournamentId);
            throw ex;
        }

        var tournament = await _appDb.TournamentRepository.GetTournamentWithRoundsAsync(tournamentId, CancellationToken.None) ?? throw new InvalidOperationException($"Tournament with Id '{tournamentId}' not found.");

        // Note: Setting rounds and tournament as completed also removes inconsistencies
        // (e.g. a tournament is marked as completed, but not all rounds are completed)

        foreach (var round in tournament.Rounds)
        {
            await SetRoundCompleted(round, cancellationToken);
        }

        if (!tournament.IsComplete)
        {
            tournament.IsComplete = true;
            tournament.ModifiedOn = _modifiedOn;
            await _appDb.GenericRepository.SaveEntityAsync(tournament, false, false, cancellationToken);
            _logger.LogDebug("Tournament {Tournament} set as completed.", tournament);
        }
        else
        {
            _logger.LogDebug("Tournament {Tournament} was already set as completed.", tournament);
        }
    }

    /// <summary>
    /// Sets the specified round as completed if all matches of the round are completed.
    /// </summary>
    /// <param name="round">The round to be set as completed.</param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="InvalidOperationException">Thrown if any match of the round is not completed yet.</exception>
    public virtual async Task SetRoundCompleted(RoundEntity round, CancellationToken cancellationToken)
    {
        if (!await _appDb.MatchRepository.AllMatchesCompletedAsync(round, cancellationToken))
        {
            var ex = new InvalidOperationException($"Round {round.Id} has uncompleted matches.");
            _logger.LogCritical(ex,@"Round {RoundId} has uncompleted matches.", round.Id);
            throw ex;
        }

        if (round.IsDirty || round.IsNew)
        {
            round = (await _appDb.RoundRepository.GetRoundWithLegsAsync(round.Id, cancellationToken))!;
        }

        if (!round.IsComplete)
        {
            round.IsComplete = true;
            round.ModifiedOn = _modifiedOn;

            await _appDb.GenericRepository.SaveEntityAsync(round, false, false, cancellationToken);
            _logger.LogDebug("Round {RoundId} set as complete.", round.Id);
        }
        else
        {
            _logger.LogDebug("Round {RoundId} was already set as complete.", round.Id);
        }
    }
}
