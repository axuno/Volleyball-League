
namespace League.Emailing.TemplateModels;

public class RemindMatchResultModel
{
    public TournamentManager.DAL.TypedViewClasses.PlannedMatchRow Fixture { get; set; } = new();
}
