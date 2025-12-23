using Microsoft.AspNetCore.Authentication;

namespace League.Models.ManageViewModels;

public class ManageLoginsViewModel
{
    public IList<UserLoginInfo> CurrentLogins { get; set; } = [];

    public bool ShowRemoveButton { get; set; }

    public IList<AuthenticationScheme> OtherLogins { get; set; } = [];
}
