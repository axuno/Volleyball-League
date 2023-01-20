using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Axuno.BackgroundTask;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.QuerySpec;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.MultiTenancy;
using TournamentManager.Ranking;

namespace League.BackgroundTasks;

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
    /// If set to <see langword="true"/>, ranking tables and ranking charts will updated,
    /// never mind whether <see cref="MatchEntity.ModifiedOn"/> &gt; <see cref="RankingEntity.CreatedOn"/> or not.
    /// </summary>
    public bool EnforceUpdate { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="TournamentManager.MultiTenancy.TenantContext"/> to use for the update.
    /// </summary>
    public ITenantContext? TenantContext { get; set; }

    private async Task UpdateRankingAsync(CancellationToken cancellationToken)
    {
        var roundIds = new HashSet<long>();
        var rankingWasUpdated = false;

        if (TournamentId == null && RoundId == null)
        {
            _logger.LogError($"Both {nameof(TournamentId)} and {nameof(RoundId)} are null. Cannot update ranking.");
            return; 
        }

        if (TenantContext == null)
        {
            _logger.LogError($"{nameof(TenantContext)} is null. Cannot update ranking.");
            return;
        }
#if DEBUG            
        var stopWatch = new System.Diagnostics.Stopwatch();
        stopWatch.Start();
#endif
        try
        {
            var matchesPlayed = await TenantContext.DbContext.AppDb.MatchRepository.GetMatchesCompleteAsync(
                new PredicateExpression(TournamentId != null
                    ? MatchCompleteRawFields.TournamentId == TournamentId
                    : MatchCompleteRawFields.RoundId == RoundId), cancellationToken);
            var matchesToPlay = await TenantContext.DbContext.AppDb.MatchRepository.GetMatchesToPlayAsync(
                new PredicateExpression(TournamentId != null
                    ? MatchToPlayRawFields.TournamentId == TournamentId
                    : MatchToPlayRawFields.RoundId == RoundId), cancellationToken);
            // Note: RankingRepository.GetRankingAsync is much cheaper than RankingRepository.GetRankingListAsync
            var currentRanking = await TenantContext.DbContext.AppDb.RankingRepository.GetRankingAsync(
                new PredicateExpression(TournamentId != null
                    ? RankingFields.TournamentId == TournamentId
                    : RankingFields.RoundId == RoundId), cancellationToken);

            #region * Identify rounds for which the ranking table must be updated *
                
            // Matches played / to play may contain a single round or all tournament rounds!
            matchesPlayed.ForEach(m => roundIds.Add(m.RoundId));

            // matches to play are required for generation of ranking chart files (remaining match days)
            matchesToPlay.ForEach(m => roundIds.Add(m.RoundId));
                
            #endregion

            var teamsInRound =
                await TenantContext.DbContext.AppDb.TeamInRoundRepository.GetTeamInRoundAsync(
                    new PredicateExpression(TeamInRoundFields.RoundId.In(roundIds)), cancellationToken);

            foreach (var roundId in roundIds)
            {
                /***** Ranking table update *****/

                // without played matches, neither ranking nor chart can be generated
                if (matchesPlayed.All(mp => mp.RoundId != roundId))
                {
                    // Remove an existing ranking list for the round
                    await TenantContext.DbContext.AppDb.RankingRepository.ReplaceAsync(new RankingList(), roundId, cancellationToken);
                    continue;
                }
                    
                // rules can be different for every round
                var matchRule =
                    await TenantContext.DbContext.AppDb.RoundRepository.GetMatchRuleAsync(roundId, cancellationToken);
                // filter matches to only contain a single round
                var newRanking = new Ranking(matchesPlayed.Where(mp => mp.RoundId == roundId),
                    matchesToPlay.Where(mtp => mtp.RoundId == roundId), (RankComparerEnum) matchRule.RankComparer);
                // Save the current last update
                var currentLastUpdated = currentRanking.Any() ? currentRanking.Where(l => l.RoundId == roundId).Max(l => l.ModifiedOn) : DateTime.MinValue;
                var newRankingList = newRanking.GetList(out var newLastUpdated);
                    
                // Has there been a change to the ranking?
                if (newLastUpdated == currentLastUpdated && !EnforceUpdate) continue;
                rankingWasUpdated = true;

                // Update the ranking table
                await TenantContext.DbContext.AppDb.RankingRepository.ReplaceAsync(newRankingList,
                    roundId, cancellationToken);

                /***** Chart file generation *****/

                var chart = new RankingChart(newRanking,
                        teamsInRound.Select(tir => (tir.TeamId, tir.TeamNameForRound)).ToList(),
                        new RankingChart.ChartSettings
                        {
                            Title = string.Empty, XTitle = "MD", YTitle = "R", Width = 700, Height = 400,
                            GraphBackgroundColorArgb = "#FFEFFFEF", PlotAreaBackgroundColorArgb = "#FFFFFFFF",
                            FontName = "Arial, Helvetica, sans-serif", ShowLegend = false
                        })
                    {UseMatchDayMarker = true};

                await using var chartStream = chart.GetPng();
                await using var fileStream = File.Create(Path.Combine(_webHostEnvironment.WebRootPath, RankingImageFolder,
                    string.Format(RankingChartFilenameTemplate, TenantContext.Identifier, roundId,
                        DateTime.UtcNow.Ticks)));
                chartStream.Seek(0, SeekOrigin.Begin);
                await chartStream.CopyToAsync(fileStream, cancellationToken);
            }
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Could not update ranking table and/or chart files");
        }

        if (rankingWasUpdated) DeleteObsoleteChartImageFiles(roundIds);
#if DEBUG
        stopWatch.Stop();
        _logger.LogInformation("{RankingTask} completed in {ElapsedTime}ms", nameof(RankingUpdateTask), stopWatch.ElapsedMilliseconds);
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
                    TenantContext!.Identifier, roundId, "*")).OrderByDescending(fi => fi.LastWriteTimeUtc);

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
