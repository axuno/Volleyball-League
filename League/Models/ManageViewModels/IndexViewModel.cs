using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Axuno.Tools.DateAndTime;
using League.Identity;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace League.Models.ManageViewModels
{
    public class IndexViewModel
    {
        private readonly Axuno.Tools.DateAndTime.TimeZoneConverter _timeZoneConverter;

        public IndexViewModel(Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter)
        {
            _timeZoneConverter = timeZoneConverter;
        }

        public Axuno.Tools.DateAndTime.TimeZoneConverter TimeZoneConverter => _timeZoneConverter;
        public ApplicationUser ApplicationUser { get; set; }
        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public ManageMessage ManageMessage { get; set; }
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
}
