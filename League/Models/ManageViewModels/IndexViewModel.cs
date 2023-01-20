using System.Collections.Generic;
using League.Identity;
using Microsoft.AspNetCore.Identity;

namespace League.Models.ManageViewModels;

public class IndexViewModel
{
    public IndexViewModel(Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter)
    {
        TimeZoneConverter = timeZoneConverter;
    }

    public Axuno.Tools.DateAndTime.TimeZoneConverter TimeZoneConverter { get; }

    public ApplicationUser? ApplicationUser { get; set; }
    public bool HasPassword { get; set; }
    public IList<UserLoginInfo> Logins { get; set; } = new List<UserLoginInfo>();
    public ManageMessage? ManageMessage { get; set; }
}

public class ManageMessage
{
    public MessageId MessageId { get; set; }
    public TagHelpers.SiteAlertTagHelper.AlertType AlertType { get; set; }
}

public enum MessageId
{
    ManageLoginFailure,
    AddLoginSuccess,
    AddLoginFailure,
    RemoveLoginSuccess,
    RemoveLoginFailure,
    ChangeUsernameSuccess,
    ChangeUsernameFailure,
    ChangePasswordSuccess,
    ChangePasswordFailure,
    ChangeEmailConfirmationSent,
    ChangeEmailSuccess,
    ChangeEmailFailure,
    ChangeEmail2Success,
    ChangeEmail2Failure,
    ChangePhoneSuccess,
    ChangePhoneFailure,
    ChangePhone2Success,
    ChangePhone2Failure,
    SetPasswordSuccess,
    SetPasswordFailure,
    ChangePersonalDetailsSuccess,
    ChangePersonalDetailsFailure
}
