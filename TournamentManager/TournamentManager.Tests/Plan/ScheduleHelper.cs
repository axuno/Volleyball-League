using NUnit.Framework;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;

namespace TournamentManager.Tests.Plan;
internal class ScheduleHelper
{
    internal static TournamentEntity? GetTournamentNullable()
    {
        var t = GetTournament();
        // Condition will always be true
        return t.Fields.Count > 0 ? t : null;
    }

    internal static TournamentEntity GetTournament()
    {
        var teams = new EntityCollection<TeamEntity> {
            { new (1) { Venue = new VenueEntity(1){ IsDirty = false, IsNew = false }, MatchDayOfWeek = 5, MatchTime = new TimeSpan(18, 0, 0) } },
            { new (2) { Venue = new VenueEntity(2){ IsDirty = false, IsNew = false }, MatchDayOfWeek = 4, MatchTime = new TimeSpan(18, 30, 0) } },
            { new (3) { Venue = new VenueEntity(3){ IsDirty = false, IsNew = false }, MatchDayOfWeek = 3, MatchTime = new TimeSpan(19, 0, 0) } },
            { new (4) { Venue = new VenueEntity(4){ IsDirty = false, IsNew = false }, MatchDayOfWeek = 2, MatchTime = new TimeSpan(19, 30, 0) } },
            { new (5) { Venue = new VenueEntity(5){ IsDirty = false, IsNew = false }, MatchDayOfWeek = 1, MatchTime = new TimeSpan(20, 0, 0) } }
        };
        foreach (var teamEntity in teams)
        {
            teamEntity.Fields.State = EntityState.Fetched;
            teamEntity.IsNew = teamEntity.IsDirty = false;
        }

        var round = new RoundEntity(1)
        {
            RoundLegs = { new RoundLegEntity { Id = 1, RoundId = 1, SequenceNo = 1, StartDateTime = new DateTime(2024, 1, 1), EndDateTime = new DateTime(2024, 4, 30) } }, IsNew = false, IsDirty = false
        };

        var teamInRounds = new EntityCollection<TeamInRoundEntity> {
            new() { Round = round, Team = teams[0], IsNew = false, IsDirty = false },
            new() { Round = round, Team = teams[1], IsNew = false, IsDirty = false },
            new() { Round = round, Team = teams[2], IsNew = false, IsDirty = false },
            new() { Round = round, Team = teams[3], IsNew = false, IsDirty = false },
            new() { Round = round, Team = teams[4], IsNew = false, IsDirty = false }
        };

        round.TeamInRounds.AddRange(teamInRounds);

        foreach (var teamInRound in teamInRounds)
        {
            teamInRound.Fields.State = EntityState.Fetched;
            teamInRound.IsNew = teamInRound.IsDirty = false;
        }

        var tournament = new TournamentEntity(1) { IsNew = false, IsDirty = false };
        round.Tournament = tournament;
        // Must be set to false, otherwise teams cannot be added to the collection
        round.TeamCollectionViaTeamInRound.IsReadOnly = false;
        round.TeamCollectionViaTeamInRound.AddRange(teams);
        round.Fields.State = EntityState.Fetched;

        tournament.Rounds.Add(round);

        return tournament;
    }
}
