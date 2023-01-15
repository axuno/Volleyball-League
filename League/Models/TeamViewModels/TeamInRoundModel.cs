using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;

namespace League.Models.TeamViewModels;

public class TeamInRoundModel
{
    public bool IsNew { get; set; }
    public long Id { get; set; }
    public long TeamId { get; set; }
    public long RoundId { get; set; }
    public string TeamNameForRound { get; set; }

    public void MapEntityToFormFields(TeamInRoundEntity teamInRoundEntity)
    {
        IsNew = teamInRoundEntity.IsNew;
        Id = teamInRoundEntity.Id;
        TeamId = teamInRoundEntity.TeamId;
        RoundId = teamInRoundEntity.RoundId;
        TeamNameForRound = teamInRoundEntity.TeamNameForRound;
    }

    public void MapFormFieldsToEntity(TeamInRoundEntity teamInRoundEntity)
    {
        teamInRoundEntity.IsNew = IsNew;
        teamInRoundEntity.TeamId = TeamId;
        teamInRoundEntity.RoundId = RoundId;
        teamInRoundEntity.TeamNameForRound = TeamNameForRound;
    }
}