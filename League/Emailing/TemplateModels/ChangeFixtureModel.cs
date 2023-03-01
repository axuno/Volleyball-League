
namespace League.Emailing.TemplateModels;

public class ChangeFixtureModel
{
    public string? Username { get; set; }

    public TournamentManager.DAL.TypedViewClasses.PlannedMatchRow? Fixture { get; set; }
}
