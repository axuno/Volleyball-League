using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using TournamentManager.MultiTenancy;
using TournamentManager.RoundRobin;

namespace TournamentManager.Plan;

/// <summary>
/// Class to create matches for a group of participants.
/// The round robin system is applied, i.e. all participants in the group play each other.
/// </summary>
/// <typeparam name="TP">The <see langword="struct"/> participant type.</typeparam>
/// <typeparam name="TR">The <see langword="struct"/> referee type.</typeparam>
internal class MatchCreator<TP, TR> where TP : struct, IEquatable<TP> where TR : struct, IEquatable<TR>
{
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<MatchCreator<TP, TR>> _logger;
    private int _maxNumOfCombinations;
    private readonly ParticipantCombinations<TP, TR> _participantCombinationsFirstLeg = new();
    private readonly ParticipantCombinations<TP, TR> _participantCombinationsReturnLeg = new();

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="tenantContext">The <see cref="ITenantContext"/>.</param>
    /// <param name="logger">The logger.</param>
    public MatchCreator(ITenantContext tenantContext, ILogger<MatchCreator<TP, TR>> logger)
    {
        if (!typeof(TR).IsAssignableFrom(typeof(TP)))
            throw new ArgumentException($"Type {typeof(TR).Name} must be assignable from {typeof(TP).Name}.");

        _tenantContext = tenantContext;
        _logger = logger;
    }

    /// <summary>
    /// Sets the participants.
    /// </summary>
    /// <param name="participants"></param>
    /// <returns>The current instance of the <see cref="MatchCreator{TP,TR}"/>.</returns>
    public MatchCreator<TP, TR> SetParticipants(Collection<TP> participants)
    {
        Participants = participants;
        return this;
    }

    /// <summary>
    /// Creates the match combinations for all participants.
    /// </summary>
    private void CreateCombinations(RefereeType refereeType)
    {
        if (Participants.Count < 2)
            throw new InvalidOperationException("Round-robin system requires at least 2 participants.");

        if (Participants.Count < 3 && refereeType == RefereeType.OtherFromRound)
            throw new ArgumentOutOfRangeException(nameof(refereeType), refereeType,@"Round-robin system with referee from round requires at least 3 participants.");

        _logger.LogDebug("Creating combinations for {participantCount} participants.", Participants.Count);

        _maxNumOfCombinations = Participants.Count * (Participants.Count - 1) / 2;
        CombinationsPerLeg = Participants.Count - 1;

        _participantCombinationsFirstLeg.Clear();

        var roundRobinMatches = GetRoundRobinSystem().GenerateMatches();
        _maxNumOfCombinations = roundRobinMatches.Count;

        // re-assign referee according to settings:
        var referees = new List<TR>();
        referees.AddRange((IEnumerable<TR>) Participants);

        var refereeAssigner = RefereeAssigners<TP, TR>.GetRefereeAssigner(refereeType, referees);

        for (var count = 0; count < _maxNumOfCombinations; count++)
        {
            var match = roundRobinMatches[count];
            var combination = new ParticipantCombination<TP, TR>(match.Turn, match.Home, match.Guest, default);
            combination.Referee = (TR?) (object?) refereeAssigner.GetReferee((combination.Turn, combination.Home, combination.Guest));

            _participantCombinationsFirstLeg.Add(combination);
        }

        CreateCombinationsReturnLeg(refereeType);
    }

    private IRoundRobinSystem<TP> GetRoundRobinSystem()
    {
        return Participants.Count is >= 5 and <= 14 
            ? new IdealRoundRobinSystem<TP>(Participants)
            : new RoundRobinSystem<TP>(Participants);
    }

    /// <summary>
    /// Creates the return leg based on the previously created first leg
    /// by swapping home / guest participants and assigning the referee.
    /// </summary>
    private void CreateCombinationsReturnLeg(RefereeType refereeType)
    {
        _participantCombinationsReturnLeg.Clear();

        // re-assign referee according to settings:
        var referees = new List<TR>();
        referees.AddRange((IEnumerable<TR>) Participants);
        var refereeAssigner = RefereeAssigners<TP, TR>.GetRefereeAssigner(refereeType, referees);

        foreach (var match in _participantCombinationsFirstLeg)
        {
            _participantCombinationsReturnLeg.Add(new ParticipantCombination<TP, TR>(match.Turn, match.Guest, match.Home, (TR?) (object?) refereeAssigner.GetReferee((match.Turn, match.Guest, match.Home))));
        }
    }

    /// <summary>
    /// Gets all combinations for the given participants, using the round-robin system.
    /// </summary>
    /// <param name="legType">First leg or return leg.</param>
    /// <returns>Return a collection of participant combinations.</returns>
    public ParticipantCombinations<TP, TR> GetCombinations(LegType legType)
    {
        CreateCombinations(_tenantContext.TournamentContext.RefereeRuleSet.RefereeType);

        return ((legType == LegType.First) ? _participantCombinationsFirstLeg : _participantCombinationsReturnLeg);
    }

    /// <summary>
    /// Gets the number of matches per participant.
    /// </summary>
    public int CombinationsPerLeg { get; private set; }

    /// <summary>
    /// Gets the participants.
    /// </summary>
    public Collection<TP> Participants { get; private set; } = new();
}
