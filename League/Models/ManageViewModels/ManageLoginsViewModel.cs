using Microsoft.AspNetCore.Authentication;

namespace League.Models.ManageViewModels;

public class ManageLoginsViewModel
{
    public IList<UserLoginInfo> CurrentLogins { get; set; } = new List<UserLoginInfo>();

    public bool ShowRemoveButton { get; set; }

    public IList<AuthenticationScheme> OtherLogins { get; set; } = new List<AuthenticationScheme>();
}
