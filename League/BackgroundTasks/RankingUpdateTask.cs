using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Axuno.BackgroundTask;
using League.DI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.QuerySpec;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.Data;
using TournamentManager.Ranking;

namespace League.BackgroundTasks
{
    /// <summary>
    /// Processes all matches, updates the ranking table and generates ranking chart files.
    /// </summary>
    public class RankingUpdateTask : IBackgroundTask
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<RankingUpdateTask> _logger;

        /// <summary>
        /// Folder name relative to <see cref="IWebHostEnvironment.WebRootPath"/>.
        /// </summary>
        public const string RankingImageFolder = "ranking-images";

        /// <summary>
        /// Template for chart images. {0}: organization key, {1}: round number {2}: <see cref="string.Empty"/> or * for searching files.
        /// </summary>
        public const string RankingChartFilenameTemplate = "chart_{0}_round_{1}_t{2}.png";

        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="webHostEnvironment"></param>
        /// <param name="logger"></param>
        public RankingUpdateTask(IWebHostEnvironment webHostEnvironment, ILogger<RankingUpdateTask> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        /// <summary>
        /// The task will be started asynchronously. The caller is responsible for implementing,
        /// that the <see cref="Timeout"/> set for the task is respected.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            await UpdateRankingAsync(cancellationToken);
        }

        /// <summary>
        /// Gets or sets the timeout, after which e.g. a <see cref="TimeoutException"/> should be thrown by the caller,
        /// or any other appropriate action.
        /// Set the timeout to TimeSpan.FromMilliseconds(-1) indicating an infinite timeout.
        /// </summary>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// Gets or sets the tournament, for which ranking data should be updated.
        /// </summary>
        public long? TournamentId { get; set; }

        /// <summary>
        /// Gets or sets the round, for which ranking data should be updated.
        /// If <see cref="TournamentId"/> and <see cref="RoundId"/> are set, <see cref="TournamentId"/> has precedence.
        /// </summary>
        public long? RoundId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="OrganizationSiteContext"/> to use for the update.
        /// </summary>
        public OrganizationSiteContext OrganizationSiteContext { get; set; }

        private async Task UpdateRankingAsync(CancellationToken cancellationToken)
        {
            var roundIds = new HashSet<long>();

            if (TournamentId == null && RoundId == null)
            {
                _logger.LogError($"Both {nameof(TournamentId)} and {nameof(RoundId)} are null. Cannot update ranking.");
                return;
            }

            if (OrganizationSiteContext == null)
            {
                _logger.LogError($"{nameof(OrganizationSiteContext)} is null. Cannot update ranking.");
                return;
            }
#if DEBUG            
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
#endif
            try
            {
                var matchesPlayed = await OrganizationSiteContext.AppDb.MatchRepository.GetMatchesCompleteAsync(
                    new PredicateExpression(TournamentId != null
                        ? MatchCompleteRawFields.TournamentId == TournamentId
                        : MatchCompleteRawFields.RoundId == RoundId), cancellationToken);
                var matchesToPlay = await OrganizationSiteContext.AppDb.MatchRepository.GetMatchesToPlayAsync(
                    new PredicateExpression(TournamentId != null
                        ? MatchToPlayRawFields.TournamentId == TournamentId
                        : MatchToPlayRawFields.RoundId == RoundId), cancellationToken);
                matchesPlayed.ForEach(m => roundIds.Add(m.RoundId));
                // matches to play is required for generation of ranking chart files
                matchesToPlay.ForEach(m => roundIds.Add(m.RoundId));
                
                var teamsInRound =
                    await OrganizationSiteContext.AppDb.TeamInRoundRepository.GetTeamInRoundAsync(
                        new PredicateExpression(TeamInRoundFields.RoundId.In(roundIds)), cancellationToken);

                foreach (var roundId in roundIds)
                {
                    /***** Ranking table update *****/

                    // rules can be different for every round
                    var matchRule =
                        await OrganizationSiteContext.AppDb.RoundRepository.GetMatchRuleAsync(roundId, cancellationToken);
                    // filter matches to only contain a single round
                    var ranking = new Ranking(matchesPlayed.Where(mp => mp.RoundId == roundId),
                        matchesToPlay.Where(mtp => mtp.RoundId == roundId), (RankComparerEnum) matchRule.RankComparer);
                    // Update the ranking table
                    await OrganizationSiteContext.AppDb.RankingRepository.SaveAsync(ranking.GetList(out var lastUpdated),
                        roundId, cancellationToken);

                    /***** Chart file generation *****/

                    // without played matches, no chart can be generated
                    if(ranking.MatchesPlayed.Count == 0) break;

                    var chart = new RankingChart(ranking,
                            teamsInRound.Select(tir => (tir.TeamId, tir.TeamNameForRound)).ToList(), 1.5f, "", "MD",
                            "R")
                        {UseMatchDayMarker = true};

                    using var image = chart.GetImage();
                    image.Save(
                        Path.Combine(_webHostEnvironment.WebRootPath, RankingImageFolder,
                            string.Format(RankingChartFilenameTemplate, OrganizationSiteContext.FolderName, roundId,
                                DateTime.UtcNow.Ticks)), System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Could not update ranking table and/or chart files");
            }

            DeleteObsoleteChartImageFiles(roundIds);
#if DEBUG
            stopWatch.Stop();
            _logger.LogInformation("{0} completed in {1}ms", nameof(RankingUpdateTask), stopWatch.ElapsedMilliseconds);
#endif
        }

        private void DeleteObsoleteChartImageFiles(IEnumerable<long> roundIds)
        {
            try
            {
                var chartDirectory = new DirectoryInfo(Path.Combine(_webHostEnvironment.WebRootPath, RankingImageFolder));
                foreach (var roundId in roundIds)
                {
                    var fileInfos = chartDirectory.GetFiles(string.Format(RankingChartFilenameTemplate,
                        OrganizationSiteContext.FolderName, roundId, "*")).OrderByDescending(fi => fi.LastWriteTimeUtc);

                    // Remove all files except for the most recent
                    foreach (var fileInfo in fileInfos.Skip(1))
                    {
                        File.Delete(fileInfo.FullName);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not delete one or more obsolete chart image files");
            }
        }
    }
}
