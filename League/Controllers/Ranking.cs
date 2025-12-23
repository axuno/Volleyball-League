using League.BackgroundTasks;
using League.Models.RankingViewModels;
using League.MultiTenancy;
using League.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.QuerySpec;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.MultiTenancy;

namespace League.Controllers;

/// <summary>
/// The <see cref="Controller"/> for ranking tables.
/// </summary>
[Route(TenantRouteConstraint.Template + "/[controller]")]
public class Ranking : AbstractController
{
    private readonly ITenantContext _tenantContext;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IAppDb _appDb;
    private readonly ILogger<Ranking> _logger;
    private readonly IMemoryCache _memoryCache;

    public Ranking(ITenantContext tenantContext, IWebHostEnvironment webHostEnvironment, ILogger<Ranking> logger, IMemoryCache memoryCache)
    {
        _tenantContext = tenantContext;
        _appDb = tenantContext.DbContext.AppDb;
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
        _memoryCache = memoryCache;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return Redirect(TenantLink.Action(nameof(Table), nameof(Ranking)) ?? string.Empty);
    }

    /// <summary>
    /// Displays the ranking table for the currently active tournament.
    /// </summary>
    /// <returns></returns>
    [HttpGet("[action]")]
    public async Task<IActionResult> Table(CancellationToken cancellationToken)
    {
        try
        {
            var rankingList = await _appDb.RankingRepository.GetRankingListAsync(
                new PredicateExpression(RankingListFields.TournamentId == _tenantContext.TournamentContext.MatchResultTournamentId)
                , cancellationToken);

            var model = new RankingListModel
            {
                Tournament =
                    await _appDb.TournamentRepository.GetTournamentAsync(new(TournamentFields.Id == _tenantContext.TournamentContext.MatchResultTournamentId),
                        cancellationToken),
                RankingList = rankingList,
                ChartFileInfos = GetChartFileInfos(rankingList.Select(rl => rl.RoundId).Distinct())
            };

            if (model.Tournament == null)
            {
                _logger.LogError("{Name} '{Id}' does not exist. User ID '{CurrentUser}'.", nameof(_tenantContext.TournamentContext.MatchPlanTournamentId), _tenantContext.TournamentContext.MatchResultTournamentId, GetCurrentUserId());
                return NotFound();
            }

            return View(Views.ViewNames.Ranking.Table, model);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when creating the ranking table");
            return NotFound();
        }
    }

    [HttpGet("all-time/tournament/{id?}")]
    public async Task<IActionResult> AllTimeTournament(long?id, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) id = null;

        try
        {
            var rankingList = await GetRankingListCached(cancellationToken);
            var roundLegPeriods = await GetRoundLegPeriodsCached(rankingList, cancellationToken);

            id ??= roundLegPeriods.Max(rlp => rlp.TournamentId);
            if (roundLegPeriods.Count > 0 && roundLegPeriods.TrueForAll(rlp => rlp.TournamentId != id))
                return Redirect(TenantLink.Action(nameof(AllTimeTournament), nameof(Ranking), new { id = string.Empty })!);

            var model = new AllTimeTournamentModel(rankingList, roundLegPeriods) { SelectedTournamentId = id };
            return View(Views.ViewNames.Ranking.AllTimeForTournament, model);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when creating the {AllTimeTournament} table", nameof(AllTimeTournament));
            return NotFound();
        }
    }

    [HttpGet("all-time/team/{id?}")]
    public async Task<IActionResult> AllTimeTeam(long? id, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) id = null;

        try
        {
            var rankingList = await GetRankingListCached(cancellationToken);
            var roundLegPeriods = await GetRoundLegPeriodsCached(rankingList, cancellationToken);

            if (rankingList.Count > 0 && rankingList.TrueForAll(rl => rl.TeamId != id))
                return Redirect(TenantLink.Action(nameof(AllTimeTournament), nameof(Ranking), new { id = string.Empty })!);

            var model = new AllTimeTeamModel(rankingList, roundLegPeriods) { SelectedTeamId = id };
            return View(Views.ViewNames.Ranking.AllTimeForTeam, model);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when creating the {AllTimeTeam} table", nameof(AllTimeTeam));
            return NotFound();
        }
    }

    private async Task<List<RankingListRow>> GetRankingListCached(CancellationToken cancellationToken)
    {
        var rankingList = await _memoryCache.GetOrCreateAsync(
            string.Join("_", _tenantContext.Identifier, typeof(Ranking).FullName, nameof(RankingListRow)),
            cache =>
            {
                cache.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30);
                using var tokenSource = new CancellationTokenSource();
                var token = new CancellationChangeToken(tokenSource.Token);
                cache.AddExpirationToken(token);
                return _appDb.RankingRepository.GetRankingListAsync(
                    new PredicateExpression(RankingListFields.TournamentIsComplete == true),
                    cancellationToken);
            }
        );

        if (rankingList != null) return rankingList;

        _logger.LogError("Could not get or create round leg periods");
        return [];
    }

    private async Task<List<RoundLegPeriodRow>> GetRoundLegPeriodsCached(List<RankingListRow> rankingList, CancellationToken cancellationToken)
    {
        var roundLegPeriods = await _memoryCache.GetOrCreateAsync(
            string.Join("_", _tenantContext.Identifier, typeof(Ranking).FullName,
                nameof(RoundLegPeriodRow)), cache =>
            {
                cache.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30);
                using var tokenSource = new CancellationTokenSource();
                var token = new CancellationChangeToken(tokenSource.Token);
                cache.AddExpirationToken(token);
                return (_appDb.RoundRepository.GetRoundLegPeriodAsync(
                    new PredicateExpression(
                        RoundLegPeriodFields.TournamentId.In(rankingList.Select(rl => rl.TournamentId))),
                    cancellationToken));
            }
        );
        if (roundLegPeriods != null) return roundLegPeriods;

        _logger.LogError("Could not get or create round leg periods");
        return [];
    }

    private Dictionary<long, FileInfo> GetChartFileInfos(IEnumerable<long> roundIds)
    {
        var chartFileInfos = new Dictionary<long, FileInfo>();
        try
        {
            var chartDirectory = new DirectoryInfo(Path.Combine(_webHostEnvironment.WebRootPath, RankingUpdateTask.RankingImageFolder));
            foreach (var roundId in roundIds)
            {
                var fileInfo = chartDirectory.GetFiles(string.Format(RankingUpdateTask.RankingChartFilenameTemplate,
                    _tenantContext.SiteContext.FolderName, roundId, "*")).OrderByDescending(fi => fi.LastWriteTimeUtc).FirstOrDefault();

                if (fileInfo != null)
                {
                    chartFileInfos.Add(roundId, fileInfo);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not get one ore more chart image files");
        }

        return chartFileInfos;
    }
}
