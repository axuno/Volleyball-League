
namespace League.Emailing.TemplateModels;

public class ResultEnteredModel
{
    public string Username { get; set; } = string.Empty;
    public string RoundDescription { get; set; } = string.Empty;
    public string HomeTeamName { get; set; } = string.Empty;
    public string GuestTeamName { get; set; } = string.Empty;
    public TournamentManager.DAL.EntityClasses.MatchEntity? Match { get; set; }
}
