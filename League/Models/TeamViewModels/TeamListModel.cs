using System.Collections.Generic;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.TypedViewClasses;

namespace League.Models.TeamViewModels;

public class TeamListModel
{
    public TeamListModel(Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter)
    {
        TimeZoneConverter = timeZoneConverter;
    }

    public TournamentEntity? Tournament { get; set; }

    public List<RoundTeamRow> RoundsWithTeams { get; set; } = new();

    public long? ActiveRoundId { get; set; }

    public Axuno.Tools.DateAndTime.TimeZoneConverter TimeZoneConverter { get; }
}
