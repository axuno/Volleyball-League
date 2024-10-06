using League.ConfigurationPoco;
using League.Models.MapViewModels;
using League.Routing;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.MultiTenancy;

namespace League.Controllers;

[Route(TenantRouteConstraint.Template + "/[controller]")]
public class Map : AbstractController
{
    private readonly ILogger<Map> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly IAppDb _appDb;
    private readonly GoogleConfiguration _googleConfig;

    public Map(ITenantContext tenantContext, IConfiguration configuration, ILogger<Map> logger)
    {
        _tenantContext = tenantContext;
        _appDb = tenantContext.DbContext.AppDb;
        _logger = logger;
        _googleConfig = new GoogleConfiguration();
        configuration.Bind(nameof(GoogleConfiguration), _googleConfig);
    }

    [Route("")]
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var venues = await _appDb.VenueRepository.GetVenueTeamRowsAsync(
            new PredicateExpression(VenueTeamFields.TournamentId == _tenantContext.TournamentContext.MapTournamentId), cancellationToken);

        var model = new MapModel(venues)
        {
            GoogleConfiguration = _googleConfig,
            Tournament = await _appDb.TournamentRepository.GetTournamentAsync(new PredicateExpression(TournamentFields.Id == _tenantContext.TournamentContext.MapTournamentId),
                cancellationToken),
        };
        if (model.Tournament == null)
        {
            _logger.LogError("{Variable} '{Value}' does not exist", nameof(_tenantContext.TournamentContext.MapTournamentId), _tenantContext.TournamentContext.MapTournamentId);
        }
        return View(Views.ViewNames.Map.Index, model);
    }

    [Route("[action]/{id}")]
    [HttpGet]
    public async Task<IActionResult> Venue(long id, CancellationToken cancellationToken)
    {
        var venue = (await _appDb.VenueRepository.GetVenueTeamRowsAsync(
            new PredicateExpression(VenueTeamFields.VenueId == id & VenueTeamFields.TournamentId == _tenantContext.TournamentContext.MapTournamentId), cancellationToken)).FirstOrDefault();

        if (venue == null) return NotFound();

        var model = new MapModel(venue)
        {
            Tournament = await _appDb.TournamentRepository.GetTournamentAsync(
                new PredicateExpression(TournamentFields.Id == _tenantContext.TournamentContext.MapTournamentId),
                cancellationToken),
            GoogleConfiguration = _googleConfig,
        };
        if (model.Tournament == null)
        {
            _logger.LogError("{Variable} '{Value}' does not exist", nameof(_tenantContext.TournamentContext.MapTournamentId), _tenantContext.TournamentContext.MapTournamentId);
        }

        return View(Views.ViewNames.Map.Index, model);
    }
}
