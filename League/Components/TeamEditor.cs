using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TournamentManager.MultiTenancy;

namespace League.Components;

public class TeamEditor : ViewComponent
{
    /// <summary>
    /// Creates the model for the component and renders it.
    /// </summary>
    /// <returns></returns>
    public IViewComponentResult Invoke(TeamEditorComponentModel team)
    {
        return View(team);
    }
}