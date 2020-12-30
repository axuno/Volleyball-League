using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.MultiTenancy;

namespace League.Components
{
    public class RoundSelector : ViewComponent
    {
        private readonly ITenantContext _tenantContext;
        private readonly AppDb _appDb;
        private readonly ILogger<RoundSelector> _logger;

        public RoundSelector(ITenantContext tenantContext, ILogger<RoundSelector> logger)
        {
            _tenantContext = tenantContext;
            _appDb = tenantContext.DbContext.AppDb;
            _logger = logger;
        }

        /// <summary>
        /// Creates the model for the component and renders it.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<IViewComponentResult> InvokeAsync(RoundSelectorComponentModel model)
        {
            return View(await FillModelWithRound(model));
        }

        private async Task<RoundSelectorComponentModel> FillModelWithRound(RoundSelectorComponentModel model)
        {
            try
            {
                model.RoundWithTypeList =
                    (await _appDb.RoundRepository.GetRoundsWithTypeAsync(new PredicateExpression(RoundFields.TournamentId == model.TournamentId), CancellationToken.None)).OrderBy(l => l.RoundType.Name).ThenBy(l => l.Name).ToList();

                return model;
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, $"Error creating view model for component '{nameof(RoundSelector)}'");
                return null;
            }
        }
    }
}
