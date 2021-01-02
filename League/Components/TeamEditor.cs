using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TournamentManager.MultiTenancy;

namespace League.Components
{
    public class TeamEditor : ViewComponent
    {
        private readonly ITenantContext _tenantContext;
        private readonly TournamentManager.MultiTenancy.AppDb _appDb;
        private readonly ILogger<TeamEditor> _logger;

        public TeamEditor(ITenantContext tenantContext, ILogger<TeamEditor> logger)
        {
            _tenantContext = tenantContext;
            _appDb = tenantContext.DbContext.AppDb;
            _logger = logger;
        }

        /// <summary>
        /// Creates the model for the component and renders it.
        /// </summary>
        /// <returns></returns>
        public IViewComponentResult Invoke(TeamEditorComponentModel team)
        {
            return View(team);
        }
    }
}
