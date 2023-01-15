using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Axuno.Tools.DateAndTime;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.TypedViewClasses;


namespace League.Models.TeamViewModels;

public class TeamListModel
{
    public TeamListModel(Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter)
    {
        TimeZoneConverter = timeZoneConverter;
    }

    public TournamentEntity Tournament { get; set; }

    public List<RoundTeamRow> RoundsWithTeams { get; set; }

    public long? ActiveRoundId { get; set; }

    public Axuno.Tools.DateAndTime.TimeZoneConverter TimeZoneConverter { get; }
}