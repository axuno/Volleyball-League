using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TournamentManager.MultiTenancy;

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