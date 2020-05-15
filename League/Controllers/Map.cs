using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using League.ConfigurationPoco;
using League.Models.MapViewModels;
using League.DI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.Data;

namespace League.Controllers
{
    [Route("{organization:ValidOrganizations}/[controller]")]
    public class Map : AbstractController
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Map> _logger;
        private readonly SiteContext _siteContext;
        private readonly AppDb _appDb;
        private readonly IStringLocalizer<Map> _localizer;
        private readonly GoogleConfiguration _googleConfig;

        public Map(SiteContext siteContext, IConfiguration configuration, IStringLocalizer<Map> localizer, ILogger<Map> logger)
        {
            _siteContext = siteContext;
            _appDb = siteContext.AppDb;
            _configuration = configuration;
            _localizer = localizer;
            _logger = logger;

            _googleConfig = new GoogleConfiguration();
            _configuration.Bind(nameof(GoogleConfiguration), _googleConfig);
        }

        [Route("")]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var venues = await _appDb.VenueRepository.GetVenueTeamRowsAsync(
                 new PredicateExpression(VenueTeamFields.TournamentId == _siteContext.MapTournamentId), cancellationToken);

            var model = new MapModel(venues)
            {
                GoogleConfiguration = _googleConfig,
                Tournament = await _appDb.TournamentRepository.GetTournamentAsync(new PredicateExpression(TournamentFields.Id == _siteContext.MapTournamentId),
                    cancellationToken),
            };
            if (model.Tournament == null)
            {
                _logger.LogCritical($"{nameof(_siteContext.MapTournamentId)} '{_siteContext.MapTournamentId}' does not exist");
            }
            return View(ViewNames.Map.Index, model);
        }

        [Route("[action]/{id}")]
        public async Task<IActionResult> Venue(long id, CancellationToken cancellationToken)
        {
            var venue = (await _appDb.VenueRepository.GetVenueTeamRowsAsync(
                new PredicateExpression(VenueTeamFields.VenueId == id & VenueTeamFields.TournamentId == _siteContext.MapTournamentId), cancellationToken)).FirstOrDefault();

            if (venue == null) return NotFound();

            var model = new MapModel(venue)
            {
                Tournament = await _appDb.TournamentRepository.GetTournamentAsync(
                    new PredicateExpression(TournamentFields.Id == _siteContext.MapTournamentId),
                    cancellationToken),
                GoogleConfiguration = _googleConfig,
            };
            if (model.Tournament == null)
            {
                _logger.LogCritical($"{nameof(_siteContext.MapTournamentId)} '{_siteContext.MapTournamentId}' does not exist");
            }

            return View(ViewNames.Map.Index, model);
        }
    }
}
