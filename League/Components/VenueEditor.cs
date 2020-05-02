using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using League.DI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TournamentManager.Data;

namespace League.Components
{
    public class VenueEditor : ViewComponent
    {
        private readonly OrganizationSiteContext _siteContext;
        private readonly AppDb _appDb;
        private readonly ILogger<VenueEditor> _logger;

        public VenueEditor(OrganizationSiteContext siteContext, ILogger<VenueEditor> logger)
        {
            _siteContext = siteContext;
            _appDb = siteContext.AppDb;
            _logger = logger;
        }

        /// <summary>
        /// Creates the model for the component and renders it.
        /// </summary>
        /// <returns></returns>
        public IViewComponentResult Invoke(VenueEditorComponentModel venue)
        {
            return View(venue);
        }
    }
}
