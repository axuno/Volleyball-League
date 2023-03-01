using Microsoft.AspNetCore.Mvc;

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
