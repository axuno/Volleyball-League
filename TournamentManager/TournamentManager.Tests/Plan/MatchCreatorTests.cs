using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using TournamentManager.MultiTenancy;
using TournamentManager.Plan;
using TournamentManager.RoundRobin;

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
        var matchCreator = new MatchCreator<long, long>(tenantContext, NullLogger<MatchCreator<long, long>>.Instance);
        var combinationsFirstLeg =
            matchCreator.SetParticipants(participants).GetCombinations(refereeType, LegType.First);
        var combinationsReturnLeg =
            matchCreator.SetParticipants(participants).GetCombinations(refereeType, LegType.Return);

        Assert.That(combinationsFirstLeg.Count, Is.EqualTo(combinationsReturnLeg.Count));
    }


    [TestCase(RefereeType.None)]
    [TestCase(RefereeType.Home)]
    [TestCase(RefereeType.Guest)]
    [TestCase(RefereeType.OtherFromRound)]
    public void CreateMatchesWithRefereeType(RefereeType refereeType)
    {
        var participants = GetParticipants(5);

        // TODO: RefereeType should be configurable via ITenantContext
        var tenantContext = new TenantContext();
        // build up match combinations for the teams of round
        var matchCreator = new MatchCreator<long, long>(tenantContext, NullLogger<MatchCreator<long, long>>.Instance);

        var combinations =
            matchCreator.SetParticipants(participants).GetCombinations(refereeType, LegType.First);

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
        var matchCreator = new MatchCreator<long, long>(tenantContext, NullLogger<MatchCreator<long, long>>.Instance);
        var combinations =
            matchCreator.SetParticipants(participants).GetCombinations(RefereeType.OtherFromRound, LegType.First)
                .Union(matchCreator.SetParticipants(participants).GetCombinations(RefereeType.OtherFromRound, LegType.Return));

        Assert.That(combinations.All(c => c.Home != c.Referee && c.Guest != c.Referee), Is.True, "Referee is never home or guest");
    }

    [Test]
    public void CreateMatchesWithUndefinedRefereeTypeShouldThrow()
    {
        var participants = GetParticipants(5);
        var tenantContext = new TenantContext();
        var matchCreator = new MatchCreator<long, long>(tenantContext, NullLogger<MatchCreator<long, long>>.Instance);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _ = matchCreator.SetParticipants(participants).GetCombinations((RefereeType) 12345, LegType.First);
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

}

