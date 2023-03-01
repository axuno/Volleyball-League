namespace League.Models.TeamApplicationViewModels;

public class ApplicationConfirmModel
{
    public ApplicationSessionModel SessionModel { get; set; } = new();

    public string RoundDescription { get; set; } = string.Empty;

    public string RoundTypeDescription { get; set; } = string.Empty;
}
