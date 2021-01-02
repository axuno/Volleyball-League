using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TournamentManager.MultiTenancy;

namespace League.Components
{
    public class VenueEditor : ViewComponent
    {
        private readonly ITenantContext _tenantContext;
        private readonly TournamentManager.MultiTenancy.AppDb _appDb;
        private readonly ILogger<VenueEditor> _logger;

        public VenueEditor(ITenantContext tenantContext, ILogger<VenueEditor> logger)
        {
            _tenantContext = tenantContext;
            _appDb = tenantContext.DbContext.AppDb;
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
