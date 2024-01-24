using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using TournamentManager.MultiTenancy;
using TournamentManager.Plan;

namespace TournamentManager.Tests.Plan;

[TestFixture]
internal class MatchCreatorTests
{
    [Test]
    public void InstantiateMatchCreatorWithIncompatibleTypesShouldThrow()
    {
        var tenantContext = new TenantContext();

        // long is not assignable from char
        Assert.Throws<ArgumentException>(() =>
            _ = new MatchCreator<long, char>(tenantContext, NullLogger<MatchCreator<long, char>>.Instance));
    }

    [TestCase(RefereeType.None)]
    [TestCase(RefereeType.Home)]
    [TestCase(RefereeType.Guest)]
    [TestCase(RefereeType.OtherFromRound)]
    public void NumberOfMatchesOfFirstAndReturnLegShouldBeEqual(RefereeType refereeType)
    {
        var participants = GetParticipants(6);
        var tenantContext = new TenantContext();
        tenantContext.TournamentContext.RefereeRuleSet.RefereeType = refereeType;

        var matchCreator = new MatchCreator<long, long>(tenantContext, NullLogger<MatchCreator<long, long>>.Instance);
        var combinationsFirstLeg =
            matchCreator.SetParticipants(participants).GetCombinations(LegType.First);
        var combinationsReturnLeg =
            matchCreator.SetParticipants(participants).GetCombinations(LegType.Return);

        Assert.That(combinationsFirstLeg.Count, Is.EqualTo(combinationsReturnLeg.Count));
    }


    [TestCase(RefereeType.None)]
    [TestCase(RefereeType.Home)]
    [TestCase(RefereeType.Guest)]
    [TestCase(RefereeType.OtherFromRound)]
    public void CreateMatchesWithRefereeType(RefereeType refereeType)
    {
        var participants = GetParticipants(5);

        var tenantContext = new TenantContext();
        tenantContext.TournamentContext.RefereeRuleSet.RefereeType = refereeType;

        // build up match combinations for the teams of round
        var matchCreator = new MatchCreator<long, long>(tenantContext, NullLogger<MatchCreator<long, long>>.Instance);

        var combinations =
            matchCreator.SetParticipants(participants).GetCombinations(LegType.First);

        var firstCombination = combinations.First();
        var expectedReferee = refereeType switch
        {
            RefereeType.None => default(long?),
            RefereeType.Home => firstCombination.Home,
            RefereeType.Guest => firstCombination.Guest,
            RefereeType.OtherFromRound => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(refereeType))
        };  

        Assert.That(combinations.Count, Is.EqualTo(10));
        Assert.That(matchCreator.CombinationsPerLeg, Is.EqualTo(4));
        Assert.That(firstCombination.Referee, Is.EqualTo(expectedReferee));
    }

    [Test]
    public void OtherOfRoundRefereeShouldNeverBeHomeOrGuest()
    {
        var participants = GetParticipants(5);
        var tenantContext = new TenantContext();
        tenantContext.TournamentContext.RefereeRuleSet.RefereeType = RefereeType.OtherFromRound;

        var matchCreator = new MatchCreator<long, long>(tenantContext, NullLogger<MatchCreator<long, long>>.Instance);
        var combinations =
            matchCreator.SetParticipants(participants).GetCombinations(LegType.First)
                .Union(matchCreator.SetParticipants(participants).GetCombinations(LegType.Return));

        Assert.That(combinations.All(c => c.Home != c.Referee && c.Guest != c.Referee), Is.True, "Referee is never home or guest");
    }

    [Test]
    public void CreateMatchesWithUndefinedRefereeTypeShouldThrow()
    {
        var participants = GetParticipants(5);
        var tenantContext = new TenantContext();
        tenantContext.TournamentContext.RefereeRuleSet.RefereeType = (RefereeType) 12345;

        var matchCreator = new MatchCreator<long, long>(tenantContext, NullLogger<MatchCreator<long, long>>.Instance);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _ = matchCreator.SetParticipants(participants).GetCombinations(LegType.First);
        });
    }

    [Test]
    public void SwappingHomeAndGuestInParticipantCombinationShouldSucceed()
    {
        var matchCreator = GetMatchCreator(5, RefereeType.Home);
        var combination = matchCreator.GetCombinations(LegType.First).First();

        var (home, guest) = (combination.Home, combination.Guest);
        var stringBeforeSwap = combination.ToString();
        combination.SwapHomeGuest();

        Assert.Multiple(() =>
        {
            Assert.That(combination.Home, Is.EqualTo(guest));
            Assert.That(combination.Guest, Is.EqualTo(home));
            Assert.That(combination.ToString(), Is.Not.EqualTo(stringBeforeSwap));
        });
    }

    [Test]
    public void NumberOfCombinationsAndTurns()
    {
        const int numOfParticipants = 5;
        var matchCreator = GetMatchCreator(numOfParticipants, RefereeType.Home);
        var firstLeg = matchCreator.GetCombinations(LegType.First);
        var firstTurn = firstLeg.GetCombinations(1);
        var allTurns = firstLeg.GetTurns().ToList();
        foreach(var turn in allTurns)
        {
            firstLeg.TurnDateTimePeriods.Add(turn, null);
        }

        Assert.Multiple(() =>
        {
            Assert.That(firstLeg.Count, Is.EqualTo(numOfParticipants * 2));
            Assert.That(firstTurn.Count, Is.EqualTo(2));
            Assert.That(allTurns.Count, Is.EqualTo(numOfParticipants));
            Assert.That(firstLeg.TurnDateTimePeriods.Count, Is.EqualTo(allTurns.Count));
        });
    }

    private static Collection<long> GetParticipants(int numOfParticipants)
    {
        var participants = new Collection<long>();
        for (var i = 1; i <= numOfParticipants; i++)
        {
            participants.Add(i);
        }

        return participants;
    }

    private static MatchCreator<long, long> GetMatchCreator(int numOfParticipants, RefereeType refereeType)
    {
        var participants = GetParticipants(numOfParticipants);
        var tenantContext = new TenantContext();
        tenantContext.TournamentContext.RefereeRuleSet.RefereeType = refereeType;

        return new MatchCreator<long, long>(tenantContext, NullLogger<MatchCreator<long, long>>.Instance)
            .SetParticipants(participants);
    }
}

