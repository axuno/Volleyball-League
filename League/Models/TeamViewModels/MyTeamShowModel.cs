using System.Collections.Generic;
using League.Models.UploadViewModels;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.TypedViewClasses;

namespace League.Models.TeamViewModels;

public class MyTeamShowModel
{
    public MyTeamShowModel(Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter)
    {
        TimeZoneConverter = timeZoneConverter;
    }

    public TournamentEntity? Tournament { get; set; }

    public List<TeamVenueRoundRow> TeamVenueRoundInfos { get; set; } = new();

    public List<TeamUserRoundRow> TeamUserRoundInfos { get; set; } = new();

    public long ActiveTeamId { get; set; }

    public TeamPhotoStaticFile? TeamPhotoStaticFile { get; set; }

    public Axuno.Tools.DateAndTime.TimeZoneConverter TimeZoneConverter { get; }
}
