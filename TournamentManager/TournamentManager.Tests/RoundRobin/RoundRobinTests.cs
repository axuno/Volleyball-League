using NUnit.Framework;
using TournamentManager.RoundRobin;

namespace TournamentManager.Tests.RoundRobin;

[TestFixture]
internal class RoundRobinTests
{
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    [TestCase(6)]
    [TestCase(7)]
    [TestCase(8)]
    [TestCase(9)]
    [TestCase(10)]
    [TestCase(11)]
    [TestCase(12)]
    [TestCase(13)]
    [TestCase(14)]
    [TestCase(15)]
    [TestCase(16)]
    public void EachCombinationIsUnique(int numOfParticipants)
    {
        var participants = GetParticipants(numOfParticipants);

        var roundRobin = new RoundRobinSystem<long>(participants);
        var matches = roundRobin.GenerateMatches();

        var uniqueCombinations = new HashSet<string>();
        foreach (var match in matches)
        {
            var combination = $"{match.Home}-{match.Guest}";
            uniqueCombinations.Add(combination);
        }

        Assert.That(matches, Has.Count.EqualTo(uniqueCombinations.Count));
        Assert.That(matches.First().Turn, Is.EqualTo(1)); // first turn is 1
    }
    
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    [TestCase(6)]
    [TestCase(7)]
    [TestCase(8)]
    [TestCase(9)]
    [TestCase(10)]
    [TestCase(11)]
    [TestCase(12)]
    [TestCase(13)]
    [TestCase(14)]
    [TestCase(15)]
    [TestCase(16)]
    public void CreateRoundRobinTournaments(int numOfParticipants)
    {
        var participants = GetParticipants(numOfParticipants);

        var roundRobin = new RoundRobinSystem<long>(participants);
        var matches = roundRobin.GenerateMatches();

        Assert.That(matches, Has.Count.EqualTo((numOfParticipants - 1) * numOfParticipants / 2));
    }

    [TestCase(4)]
    [TestCase(6)]
    [TestCase(7)]
    [TestCase(8)]
    [TestCase(9)]
    [TestCase(10)]
    [TestCase(11)]
    [TestCase(12)]
    [TestCase(13)]
    [TestCase(16)]
    public void MaxConsecutiveHomeGuest(int numOfParticipants)
    {
        var participants = GetParticipants(numOfParticipants);

        var roundRobin = new RoundRobinSystem<long>(participants);
        var matches = roundRobin.GenerateMatches();

        var participantsWithConsecutiveHomeGuestMatches = new List<long>();

        foreach (var participant in participants)
        {
            var maxConsecutiveHomeGuest = MatchesAnalyzer<long>.GetMaxConsecutiveHomeGuestCount(participant, matches);
            if (maxConsecutiveHomeGuest.HomeCount == 2 || maxConsecutiveHomeGuest.GuestCount == 2)
            {
                participantsWithConsecutiveHomeGuestMatches.Add(participant);
            }

            using (Assert.EnterMultipleScope())
            {
                Assert.That(maxConsecutiveHomeGuest.HomeCount, Is.LessThanOrEqualTo(2));
                Assert.That(maxConsecutiveHomeGuest.GuestCount, Is.LessThanOrEqualTo(2));
            }
        }

        Assert.That(participantsWithConsecutiveHomeGuestMatches, Has.Count.EqualTo(numOfParticipants % 2 == 0 ? numOfParticipants - 2 : numOfParticipants - 1));
    }

    [Test]
    public void CreateRoundRobinTournamentWithCustomStruct()
    {
        var participants = new List<CustomTestStruct> {
            new() { A = 1, B = 1, C = 1 }, new() { A = 2, B = 2, C = 2 },
            new() { A = 3, B = 3, C = 3 }, new() { A = 4, B = 4, C = 4 }
        };

        var roundRobin = new RoundRobinSystem<CustomTestStruct>(participants);
        var matches = roundRobin.GenerateMatches();

        Assert.That(matches, Has.Count.EqualTo((participants.Count - 1) * participants.Count / 2));
    }

    [Test]
    public void CreateIdealRoundRobinTournamentWithCustomStruct()
    {
        var participants = new List<CustomTestStruct> {
            new() { A = 1, B = 1, C = 1 }, new() { A = 2, B = 2, C = 2 },
            new() { A = 3, B = 3, C = 3 }, new() { A = 4, B = 4, C = 4 },
            new() { A = 5, B = 5, C = 5 }
        };

        var roundRobin = new RoundRobinSystem<CustomTestStruct>(participants);
        var matches = roundRobin.GenerateMatches();

        Assert.That(matches, Has.Count.EqualTo((participants.Count - 1) * participants.Count / 2));
    }

    private static List<long> GetParticipants(int numOfParticipants)
    {
        var participants = new List<long>(numOfParticipants);
        for (var i = 1; i <= numOfParticipants; i++)
        {
            participants.Add(i);
        }

        return participants;
    }
}

