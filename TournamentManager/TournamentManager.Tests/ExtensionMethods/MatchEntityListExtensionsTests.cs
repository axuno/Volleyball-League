using NUnit.Framework;
using FluentAssertions;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.ExtensionMethods;

namespace TournamentManager.Tests.ExtensionMethods;

[TestFixture]
public class MatchEntityListExtensionTests
{
    private readonly List<MatchEntity> _matches = new()
    {
        new MatchEntity {Id = 1, HomeTeamId = 1, GuestTeamId = 2, PlannedStart = DateTime.UtcNow.AddDays(-1), PlannedEnd = DateTime.UtcNow.AddDays(-1).AddHours(2), VenueId = 1},
        new MatchEntity {Id = 2, HomeTeamId = 1, GuestTeamId = 3, PlannedStart = DateTime.UtcNow.AddDays(-1), PlannedEnd = DateTime.UtcNow.AddDays(-1).AddHours(2), VenueId = 1},
        new MatchEntity {Id = 3, HomeTeamId = 2, GuestTeamId = 3, PlannedStart = DateTime.UtcNow.AddDays(-1), PlannedEnd = DateTime.UtcNow.AddDays(-1).AddHours(2), VenueId = 1},
        new MatchEntity {Id = 4, HomeTeamId = 1, GuestTeamId = 4, PlannedStart = DateTime.UtcNow.AddDays(-1), PlannedEnd = DateTime.UtcNow.AddDays(-1).AddHours(2), VenueId = 1},
        new MatchEntity {Id = 5, HomeTeamId = 4, GuestTeamId = 2, PlannedStart = DateTime.UtcNow.AddDays(-1), PlannedEnd = DateTime.UtcNow.AddDays(-1).AddHours(2), VenueId = 1},
        new MatchEntity {Id = 6, HomeTeamId = 3, GuestTeamId = 4, PlannedStart = DateTime.UtcNow.AddDays(-1), PlannedEnd = DateTime.UtcNow.AddDays(-1).AddHours(2), VenueId = 1},
        new MatchEntity {Id = 7, HomeTeamId = 1, GuestTeamId = 5, PlannedStart = DateTime.UtcNow.AddDays(-1), PlannedEnd = DateTime.UtcNow.AddDays(-1).AddHours(2), VenueId = 1},
        new MatchEntity {Id = 8, HomeTeamId = 2, GuestTeamId = 5, PlannedStart = DateTime.UtcNow.AddDays(-1), PlannedEnd = DateTime.UtcNow.AddDays(-1).AddHours(2), VenueId = 1},
        new MatchEntity {Id = 9, HomeTeamId = 3, GuestTeamId = 5, PlannedStart = DateTime.UtcNow.AddDays(-1), PlannedEnd = DateTime.UtcNow.AddDays(-1).AddHours(2), VenueId = 1},
        new MatchEntity {Id = 10, HomeTeamId = 4, GuestTeamId = 5, PlannedStart = DateTime.UtcNow.AddDays(-1), PlannedEnd = DateTime.UtcNow.AddDays(-1).AddHours(2), VenueId = 1},
        new MatchEntity {Id = 11, HomeTeamId = 10, GuestTeamId = 11, PlannedStart = null, PlannedEnd = null, VenueId = null},
        new MatchEntity {Id = 12, HomeTeamId = 12, GuestTeamId = 10, PlannedStart = null, PlannedEnd = DateTime.UtcNow, VenueId = null},
        new MatchEntity {Id = 13, HomeTeamId = 11, GuestTeamId = 13, PlannedStart = DateTime.UtcNow, PlannedEnd = null, VenueId = null}
    };

    [TestCase(1, -1, 0, false, 0)]
    [TestCase(1, -1, 5, false, 3)]
    [TestCase(1, 2, 5, false, 5)]
    [TestCase(10, 13, 12, false, 0)]
    [TestCase(10, 13, 12, true, 2)]
    public void Previous_Matches_Relative_To_Index_Should_Be_Found(long team1, long team2, int startIndex, bool includeUndefined, int expected)
    {
        var teamIds = new[] {team1, team2};
        var matches = _matches.GetPreviousMatches(startIndex, teamIds, includeUndefined).ToList();

        Assert.That(matches, Has.Count.EqualTo(expected));

    }

    [TestCase(-1)]
    [TestCase(13)]
    public void Invalid_StartIndex_For_Previous_Should_Throw(int startIndex)
    {
        var teamIds = new long[] { 1 };

        Assert.That(() => _matches.GetPreviousMatches(startIndex, teamIds, true).ToList(), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [TestCase(1, -1, 12, false, 0)]
    [TestCase(1, -1, 0, false, 3)]
    [TestCase(1, 2, 1, false, 5)]
    [TestCase(10, 13, 0, false, 0)]
    [TestCase(10, 13, 0, true, 3)]
    public void Next_Matches_Relative_To_Index_Should_Be_Found(long team1, long team2, int startIndex, bool includeUndefined, int expected)
    {
        var teamIds = new[] { team1, team2 };
        var matches = _matches.GetNextMatches(startIndex, teamIds, includeUndefined).ToList();

        Assert.That(matches, Has.Count.EqualTo(expected));
    }

    [TestCase(-1)]
    [TestCase(13)]
    public void Invalid_StartIndex_For_Next_Should_Throw(int startIndex)
    {
        var teamIds = new long[] { 1 };

        Assert.That(() => _matches.GetNextMatches(startIndex, teamIds, true).ToList(), Throws.TypeOf<ArgumentOutOfRangeException>());
    }
}
