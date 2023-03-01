using System;

namespace League.Models.TeamViewModels;

public struct MyTeamMessageModel
{
    public class MyTeamMessage
    {
        public MessageId MessageId { get; set; }
        public TagHelpers.SiteAlertTagHelper.AlertType AlertType { get; set; }
    }

    public enum MessageId
    {
        TeamDataSuccess,
        TeamDataFailure,
        MemberAddSuccess,
        MemberAddFailure,
        MemberRemoveSuccess,
        MemberRemoveFailure,
        MemberCannotRemoveLastTeamManager,
        VenueEditSuccess,
        VenueEditFailure,
        VenueCreateSuccess,
        VenueCreateFailure,
        VenueSelectSuccess,
        VenueSelectFailure
    }
}
