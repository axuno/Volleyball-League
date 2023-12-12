using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using TournamentManager.MultiTenancy;
using TournamentManager.RoundRobin;

namespace TournamentManager.Plan;

/// <summary>
/// Class to create matches for a group of participants.
/// The round robin system is applied, i.e. all participants in the group play each other.
/// </summary>
/// <typeparam name="T">The type of the participant objects. Objects must have <see cref="IEquatable{T}"/> implemented.</typeparam>
internal class MatchCreator<T> where T : IEquatable<T>
{
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<MatchCreator<T>> _logger;
    private int _maxNumOfCombinations;
    private readonly ParticipantCombinations<T> _participantCombinationsFirstLeg = new();
    private readonly ParticipantCombinations<T> _participantCombinationsReturnLeg = new();

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="tenantContext">The <see cref="ITenantContext"/>.</param>
    /// <param name="logger">The logger.</param>
    public MatchCreator(ITenantContext tenantContext, ILogger<MatchCreator<T>> logger)
    {
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public MatchCreator<T> WithParticipants(Collection<T> participants)
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

        if (Participants.Count < 3 && refereeType == RefereeType.OtherOfGroup)
            throw new ArgumentOutOfRangeException(nameof(refereeType), refereeType,@"Round-robin system with referee from round requires at least 3 participants.");

        _logger.LogDebug("Creating combinations for {participantCount} participants.", Participants.Count);

        _maxNumOfCombinations = Participants.Count * (Participants.Count - 1) / 2;
        CombinationsPerLeg = Participants.Count - 1;

        _participantCombinationsFirstLeg.Clear();

        var roundRobinMatches = GetRoundRobinSystem().GenerateMatches();
        _maxNumOfCombinations = roundRobinMatches.Count;

        for (var count = 0; count < _maxNumOfCombinations; count++)
        {
            var match = roundRobinMatches[count];
            _participantCombinationsFirstLeg.Add(new ParticipantCombination<T>(match.Turn, match.Home, match.Guest, default));

            // re-assign referee according to settings
            _participantCombinationsFirstLeg[count].Referee = refereeType switch {
                RefereeType.None => default,
                RefereeType.Home => match.Home,
                RefereeType.Guest => match.Guest,
                RefereeType.OtherOfGroup => GetReferee(match.Home, match.Guest),
                _ => throw new ArgumentOutOfRangeException(nameof(refereeType), refereeType, null)
            };
        }

        CreateCombinationsReturnLeg(refereeType);
    }

    private IRoundRobinSystem<T> GetRoundRobinSystem()
    {
        return Participants.Count is >= 5 and <= 14 
            ? new IdealRoundRobinSystem<T>(Participants)
            : new RoundRobinSystem<T>(Participants);
    }

    /// <summary>
    /// Creates the return leg based on the previously created first leg
    /// by swapping home / guest participants and assigning the referee.
    /// </summary>
    private void CreateCombinationsReturnLeg(RefereeType refereeType)
    {
        _participantCombinationsReturnLeg.Clear();

        foreach (var match in _participantCombinationsFirstLeg)
        {
            // re-assign referee according to settings:
            var referee = refereeType switch {
                RefereeType.None => default,
                RefereeType.Home => match.Guest,
                RefereeType.Guest => match.Home,
                RefereeType.OtherOfGroup => match.Referee,
                _ => throw new ArgumentOutOfRangeException(nameof(refereeType), refereeType, null)
            };
            _participantCombinationsReturnLeg.Add(new ParticipantCombination<T>(match.Turn, match.Guest, match.Home, referee));
        }
    }

    /// <summary>
    /// Determines the referee for a match.
    /// </summary>
    /// <param name="home">The home participant of the match.</param>
    /// <param name="guest">The guest participant of the match.</param>
    /// <returns></returns>
    private T GetReferee(T home, T guest)
    {
        var lastMaxRefereeCount = int.MaxValue;
        var lastMaxReferee = Participants[0];

        foreach (var participant in Participants)
        {
            var currentRefereeCount = GetNumOfRefereeCombinations(participant);
            if (currentRefereeCount >= lastMaxRefereeCount ||
                participant.Equals(home) || participant.Equals(guest) ||
                IsLastReferee(participant)) continue;

            lastMaxRefereeCount = currentRefereeCount;
            lastMaxReferee = participant;
        }

        return lastMaxReferee;
    }

    /// <summary>
    /// Checks whether the <paramref name="participant"/> was referee in the last match.
    /// Always returns false if the match collection is empty.
    /// </summary>
    /// <param name="participant">The participant to check whether it was referee in the last match.</param>
    /// <returns>Return true, if the participant was referee in the last match, false otherwise.</returns>
    private bool IsLastReferee(T participant)
    {
        return _participantCombinationsFirstLeg.Count != 0 && participant.Equals(_participantCombinationsFirstLeg[^1].Referee);
    }

    /// <summary>
    /// Calculates the number of matches a participant was assigned referee.
    /// </summary>
    /// <param name="participant">The participant to calculate the number of referee matches.</param>
    /// <returns>The number of referee matches for the participant.</returns>
    private int GetNumOfRefereeCombinations(T participant)
    {
        return _participantCombinationsFirstLeg.Count(match => match.Referee != null && match.Referee.Equals(participant));
    }

    /// <summary>
    /// Gets all combinations for the given participants, using the round-robin system.
    /// </summary>
    /// <param name="refereeType">Determines how referees will be assigned for the matches.</param>
    /// <param name="legType">First leg or return leg.</param>
    /// <returns>Return a collection of participant combinations.</returns>
    public ParticipantCombinations<T> GetParticipantCombinations(RefereeType refereeType, LegType legType)
    {
        CreateCombinations(refereeType);

        return (legType == LegType.First) ? _participantCombinationsFirstLeg : _participantCombinationsReturnLeg;
    }

    /// <summary>
    /// Gets the number of matches per participant.
    /// </summary>
    public int CombinationsPerLeg { get; private set; }

    /// <summary>
    /// Gets the participants.
    /// </summary>
    public Collection<T> Participants { get; private set; } = new();
}
