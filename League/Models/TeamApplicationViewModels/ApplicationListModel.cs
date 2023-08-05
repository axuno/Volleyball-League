using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.TypedViewClasses;

namespace League.Models.TeamApplicationViewModels;

public class ApplicationListModel
{
    public ApplicationListModel(Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter)
    {
        TimeZoneConverter = timeZoneConverter;
    }

    public TournamentEntity? Tournament { get; set; }

    public List<LatestTeamTournamentRow> TournamentRoundTeams { get; set; } = new();

    public Dictionary<long, DateTime> TeamRegisteredOn { get; set; } = new();

    public Axuno.Tools.DateAndTime.TimeZoneConverter TimeZoneConverter { get; }
}
