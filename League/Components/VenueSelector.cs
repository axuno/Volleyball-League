using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.MultiTenancy;

namespace League.Components;

public class VenueSelector : ViewComponent
{
    private readonly ITenantContext _tenantContext;
    private readonly TournamentManager.MultiTenancy.AppDb _appDb;
    private readonly ILogger<VenueSelector> _logger;

    public VenueSelector(ITenantContext tenantContext, ILogger<VenueSelector> logger)
    {
        _tenantContext = tenantContext;
        _appDb = tenantContext.DbContext.AppDb;
        _logger = logger;
    }

    public async Task<IViewComponentResult> InvokeAsync(ValueTuple<long, IList<long>, VenueSelectorComponentModel.Criteria, VenueSelectorComponentModel.Criteria, long?> model)
    {
        return View(await GetVenueSelectorModel(model));
    }

    private async Task<VenueSelectorComponentModel?> GetVenueSelectorModel((long TournamentId, IList<long> TeamIds, VenueSelectorComponentModel.Criteria Filter, VenueSelectorComponentModel.Criteria Group, long? VenueNotSpecifiedKey) tuple)
    {
        // The TournamentId will only affect grouping of venues. All venues will be shown for selection.
        var model = new VenueSelectorComponentModel {Filter = tuple.Filter, Group = tuple.Group, VenueNotSpecifiedKey = tuple.VenueNotSpecifiedKey};
        try
        {
            // Get all venues and teams for a tournament and select in-memory is 40% faster compared to database selections
            var venuesWithTeams = await _appDb.VenueRepository.GetVenueTeamRowsAsync(new PredicateExpression(VenueTeamFields.TournamentId == tuple.TournamentId),
                CancellationToken.None);
            model.AllVenues = (await _appDb.VenueRepository.GetVenuesAsync(new PredicateExpression(), CancellationToken.None))
                .OrderBy(v => v.City).ThenBy(v => v.Name).ThenBy(v => v.Extension).ToList();

            // get venue entities of match teams
            var venueIdsOfTeams = venuesWithTeams
                .Where(vwt => tuple.TeamIds.Contains(vwt.TeamId)).Select(vwt => vwt.VenueId).Distinct();
            model.VenuesOfTeams = model.AllVenues.Where(v => venueIdsOfTeams.Contains(v.Id)).ToList();

            // get venue entities of current tournament
            var activeVenueIds = venuesWithTeams.Select(vwt => vwt.VenueId).Distinct();
            model.ActiveVenues = model.AllVenues.Where(v => activeVenueIds.Except(venueIdsOfTeams).Contains(v.Id)).OrderBy(v => v.City).ThenBy(v => v.Name).ToList();

            // get remaining venues (currently inactive)
            model.UnusedVenues = model.AllVenues.Where(v => !activeVenueIds.Contains(v.Id)).OrderBy(v => v.City).ThenBy(v => v.Name).ToList();

            return model;
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, $"Error creating view model for component '{nameof(VenueSelector)}'");
            return null;
        }
    }
}
