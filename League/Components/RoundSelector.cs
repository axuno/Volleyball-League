using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using League.DI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.Data;

namespace League.Components
{
    public class RoundSelector : ViewComponent
    {
        private readonly SiteContext _siteContext;
        private readonly AppDb _appDb;
        private readonly ILogger<RoundSelector> _logger;

        public RoundSelector(SiteContext siteContext, ILogger<RoundSelector> logger)
        {
            _siteContext = siteContext;
            _appDb = siteContext.AppDb;
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
