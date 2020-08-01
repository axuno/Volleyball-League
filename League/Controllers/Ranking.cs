using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using League.BackgroundTasks;
using League.DI;
using League.Models.RankingViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.QuerySpec;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.Data;
using TournamentManager.Ranking;

namespace League.Controllers
{
    /// <summary>
    /// The <see cref="Controller"/> for ranking tables.
    /// </summary>
    [Route("{organization:ValidOrganizations}/[controller]")]
    public class Ranking : AbstractController
    {
        private readonly SiteContext _siteContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AppDb _appDb;
        private readonly ILogger<Ranking> _logger;
        private readonly IMemoryCache _memoryCache;

        public Ranking(SiteContext siteContext, IWebHostEnvironment webHostEnvironment, IStringLocalizer<Ranking> localizer, ILogger<Ranking> logger, IMemoryCache memoryCache)
        {
            _siteContext = siteContext;
            _appDb = siteContext.AppDb;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public IActionResult Index()
        {
            return Redirect(Url.Action(nameof(Table), nameof(Ranking), new { Organization = _siteContext.UrlSegmentValue }));
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
                    new PredicateExpression(RankingListFields.TournamentId == _siteContext.MatchResultTournamentId)
                    , cancellationToken);

                var model = new RankingListModel
                {
                    Tournament =
                        await _appDb.TournamentRepository.GetTournamentAsync(new PredicateExpression(TournamentFields.Id == _siteContext.MatchResultTournamentId),
                            cancellationToken),
                    RankingList = rankingList,
                    ChartFileInfos = GetChartFileInfos(rankingList.Select(rl => rl.RoundId).Distinct())
                };

                if (model.Tournament == null)
                {
                    throw new Exception($"{nameof(_siteContext.MatchResultTournamentId)} '{_siteContext.MatchResultTournamentId}' does not exist");
                }

                return View(ViewNames.Ranking.Table, model);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Error when creating the ranking table");
                throw;
            }
        }

        [HttpGet("all-time/tournament/{id?}")]
        public async Task<IActionResult> AllTimeTournament(long?id, CancellationToken cancellationToken)
        {
            try
            {
                var rankingList = await GetRankingListCached(cancellationToken);
                var roundLegPeriods = await GetRoundLegPeriodsCached(rankingList, cancellationToken);

                id ??= roundLegPeriods.Max(rlp => rlp.TournamentId);
                if (roundLegPeriods.Count > 0 && roundLegPeriods.All(rlp => rlp.TournamentId != id))
                    return RedirectToAction(nameof(AllTimeTournament), nameof(Ranking), new { Organization = _siteContext.UrlSegmentValue, id = string.Empty });

                var model = new AllTimeTournamentModel(rankingList, roundLegPeriods) { SelectedTournamentId = id };
                return View(ViewNames.Ranking.AllTimeForTournament, model);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, $"Error when creating the {nameof(AllTimeTournament)} table");
                throw;
            }
        }

        [HttpGet("all-time/team/{id?}")]
        public async Task<IActionResult> AllTimeTeam(long? id, CancellationToken cancellationToken)
        {
            try
            {
                var rankingList = await GetRankingListCached(cancellationToken);
                var roundLegPeriods = await GetRoundLegPeriodsCached(rankingList, cancellationToken);

                if (rankingList.Count > 0 && rankingList.All(rl => rl.TeamId != id))
                    return RedirectToAction(nameof(AllTimeTournament), nameof(Ranking), new { Organization = _siteContext.UrlSegmentValue, id = string.Empty });

                var model = new AllTimeTeamModel(rankingList, roundLegPeriods) { SelectedTeamId = id };
                return View(ViewNames.Ranking.AllTimeForTeam, model);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, $"Error when creating the {nameof(AllTimeTeam)} table");
                throw;
            }
        }

        private async Task<List<RankingListRow>> GetRankingListCached(CancellationToken cancellationToken) => await _memoryCache.GetOrCreateAsync(
            string.Join("_", _siteContext.OrganizationKey, typeof(Ranking).FullName, nameof(RankingListRow)),
            cache =>
            {
                cache.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30);
                var tokenSource = new CancellationTokenSource();
                var token = new CancellationChangeToken(tokenSource.Token);
                cache.AddExpirationToken(token);
                return _appDb.RankingRepository.GetRankingListAsync(
                    new PredicateExpression(RankingListFields.TournamentIsComplete == true),
                    cancellationToken);
            }
        );

        private async Task<List<RoundLegPeriodRow>> GetRoundLegPeriodsCached(List<RankingListRow> rankingList, CancellationToken cancellationToken) =>
            await _memoryCache.GetOrCreateAsync(
                string.Join("_", _siteContext.OrganizationKey, typeof(Ranking).FullName,
                    nameof(RoundLegPeriodRow)), cache =>
                {
                    cache.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30);
                    var tokenSource = new CancellationTokenSource();
                    var token = new CancellationChangeToken(tokenSource.Token);
                    cache.AddExpirationToken(token);
                    return (_appDb.RoundRepository.GetRoundLegPeriodAsync(
                        new PredicateExpression(
                            RoundLegPeriodFields.TournamentId.In(rankingList.Select(rl => rl.TournamentId))),
                        cancellationToken));
                }
            );

        private Dictionary<long, FileInfo> GetChartFileInfos(IEnumerable<long> roundIds)
        {
            var chartFileInfos = new Dictionary<long, FileInfo>();
            try
            {
                var chartDirectory = new DirectoryInfo(Path.Combine(_webHostEnvironment.WebRootPath, RankingUpdateTask.RankingImageFolder));
                foreach (var roundId in roundIds)
                {
                    var fileInfo = chartDirectory.GetFiles(string.Format(RankingUpdateTask.RankingChartFilenameTemplate,
                        _siteContext.FolderName, roundId, "*")).OrderByDescending(fi => fi.LastWriteTimeUtc).FirstOrDefault();

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
}