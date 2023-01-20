using Microsoft.AspNetCore.Mvc;

namespace League.Components;

public class VenueEditor : ViewComponent
{
    /// <summary>
    /// Creates the model for the component and renders it.
    /// </summary>
    /// <returns></returns>
    public IViewComponentResult Invoke(VenueEditorComponentModel venue)
    {
        return View(venue);
    }
}
