
namespace League.Emailing.TemplateModels;

public class AnnounceNextMatchModel
{
    public string IcsCalendarUrl { get; set; } = string.Empty;
    public TournamentManager.DAL.TypedViewClasses.PlannedMatchRow Fixture { get; set; } = new();
    public TournamentManager.DAL.EntityClasses.VenueEntity? Venue { get; set; }
}
