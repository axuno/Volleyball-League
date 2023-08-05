namespace League.Models.TeamApplicationViewModels;

public struct TeamApplicationMessageModel
{
    public class TeamApplicationMessage
    {
        public MessageId MessageId { get; set; }
        public TagHelpers.SiteAlertTagHelper.AlertType AlertType { get; set; }
    }

    public enum MessageId
    {
        ApplicationSuccess,
        ApplicationFailure
    }
}
