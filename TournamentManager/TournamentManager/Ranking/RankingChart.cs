using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace TournamentManager.Ranking;

/// <summary>
/// Class for creating a <see cref="RankingChart"/> image from a <see cref="Ranking"/> instance.
/// </summary>
public class RankingChart
{
    private readonly RankingHistory _rankingHistory;
    private readonly List<(long TeamId, string TeamName)> _teams;

    public class ChartSettings
    {
        public ChartSettings()
        {
            GraphBackgroundColorArgb = OxyColor.FromRgb(0xEF, 0xFF, 0xEF).ToByteString();
        }
        /// <summary>
        /// Gets or sets the width of the graph.
        /// </summary>
        public int Width { get; set; } = 800;
        /// <summary>
        /// Get or sets the height of the graph.
        /// </summary>
        public int Height { get; set; } = 600;
        /// <summary>
        /// Gets or sets the font name to use for rendering text.
        /// </summary>
        public string FontName { get; set; } = "Arial";

        /// <summary>
        /// Gets or sets the graph background color as a decimal byte string with decimal format &quot;{A},{R},{G},{B}&quot; or &quot;#AARRGGBB&quot; in hex format.
        /// </summary>
        public string GraphBackgroundColorArgb { get; set; }

        /// <summary>
        /// Gets or sets the plot area background color as a decimal byte string with decimal format &quot;{A},{R},{G},{B}&quot; or &quot;#AARRGGBB&quot; in hex format.
        /// </summary>
        public string PlotAreaBackgroundColorArgb { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the legend will be rendered.
        /// </summary>
        public bool ShowLegend { get; set; } = true;

        /// <summary>
        /// Gets or sets the title of the graph.
        /// </summary>
        public string Title { get; set; } = "Chart";

        /// <summary>
        /// Gets or sets the title of the x-axes.
        /// </summary>
        public string XTitle { get; set; } = "Match Days";

        /// <summary>
        /// Gets or sets the title of the y-axes.
        /// </summary>
        public string YTitle { get; set; } = "Rank";
    }

    /// <summary>
    /// CTOR.
    /// </summary>
    /// <param name="ranking"></param>
    /// <param name="teams"></param>
    /// <param name="settings"></param>
    public RankingChart(Ranking ranking, List<(long TeamId, string TeamName)> teams, ChartSettings settings)
    {
        Ranking = ranking;
        Settings = settings;
        _rankingHistory = ranking.GetRankingHistory();
        _teams = teams ?? throw new ArgumentNullException(nameof(teams));
    }

    /// <summary>
    /// Gets the list of colors used for the lines in the chart from rank 1 (first in the list) to rank 10 (last) as ARGB integer.
    /// Only 20 colors are defined, all others will be rendered in gray.
    /// </summary>
    public static readonly List<uint> LineColors = new(new[]
    {
        // 10 best
        OxyColors.Red.ToUint(), OxyColors.Green.ToUint(), OxyColors.Orange.ToUint(), 
        OxyColors.Blue.ToUint(), OxyColors.Magenta.ToUint(), OxyColors.Gray.ToUint(), 
        OxyColors.Gold.ToUint(), OxyColors.Brown.ToUint(), OxyColors.Aqua.ToUint(), 
        OxyColors.Black.ToUint(),
        // second best
        OxyColors.CadetBlue.ToUint(), OxyColors.Chartreuse.ToUint(), OxyColors.Chocolate.ToUint(), 
        OxyColors.DarkCyan.ToUint(), OxyColors.DarkOrchid.ToUint(), OxyColors.LightPink.ToUint(), 
        OxyColors.Goldenrod.ToUint(), OxyColors.HotPink.ToUint(), OxyColors.Purple.ToUint(), 
        OxyColors.SlateGray.ToUint()
    });

    /// <summary>
    /// Gets the settings for creating the chart.
    /// </summary>
    public ChartSettings Settings { get; private set; }

    public PlotModel CreatePlotModel()
    {
        var dateHistory = _rankingHistory.GetByMatchDay();
        var maxNumOfEntries = dateHistory.Max(dh => dh.Count);
            
        var model = new PlotModel
        {
            Title = Settings.Title,
            TitleFontSize = 18.0,
            TitleFontWeight = 400.0,
            DefaultFont = Settings.FontName,
            PlotAreaBackground = OxyColor.Parse(Settings.PlotAreaBackgroundColorArgb),
            Background = OxyColor.Parse(Settings.GraphBackgroundColorArgb)
        };
        model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom });
        model.Axes.Add(new LinearAxis { Position = AxisPosition.Left });

        model.Legends.Add(new Legend
        {
            LegendBackground = OxyColor.FromAColor(220, OxyColors.White),
            LegendBorder = OxyColors.Black,
            LegendBorderThickness = 1.0D, 
            LegendPlacement = LegendPlacement.Outside,
            LegendPosition = LegendPosition.BottomLeft,
            LegendOrientation = LegendOrientation.Horizontal,
            LegendLineSpacing = 6D,
            LegendFontSize = 12D
        });

        model.Axes[0].Title = Settings.XTitle;
        model.Axes[0].TitleFontSize = 18.0;
        model.Axes[0].TitleFontWeight = 400.0;
        model.Axes[0].AxisTitleDistance = 12D;
        model.Axes[0].MajorStep = 1D;
        model.Axes[0].MajorGridlineStyle = LineStyle.Dot;
        model.Axes[0].MajorGridlineColor = OxyColors.LightGray;
        model.Axes[0].MajorGridlineThickness = .25D;
        model.Axes[0].MinorGridlineThickness = 0;
        model.Axes[0].AbsoluteMinimum = 1D;
        model.Axes[0].AbsoluteMaximum = dateHistory.Count + Ranking.MatchesToPlay.GroupBy(u => u.MatchDate?.Date ?? DateTime.MinValue).Count(); 
        model.Axes[0].TextColor = OxyColors.Black;

        model.Axes[1].Title = Settings.YTitle;
        model.Axes[1].TitleFontSize = 18.0;
        model.Axes[1].TitleFontWeight = 400.0;
        model.Axes[1].AxisTitleDistance = 12D;
        model.Axes[1].MajorStep = 1D;
        model.Axes[1].MajorGridlineThickness = .25D;
        model.Axes[1].MajorGridlineStyle = LineStyle.Dot;
        model.Axes[1].MajorGridlineColor = OxyColors.LightGray;
        model.Axes[1].MinorGridlineThickness = 0;
        model.Axes[1].AbsoluteMinimum = 0D;
        model.Axes[1].AbsoluteMaximum = maxNumOfEntries + .5D; // slightly bigger, so that bottom horizontal lines don't get cut
        model.Axes[1].StartPosition = .93D;
        model.Axes[1].EndPosition = .07D;
        model.Axes[1].TextColor = OxyColors.Black;

        model.Legends.Clear(); // remove auto-generated legend
        model.Legends.Add(new Legend
        {
            LegendBackground = OxyColor.FromAColor(220, OxyColors.White),
            LegendBorder = OxyColors.Black,
            LegendBorderThickness = 1.0D, 
            LegendPlacement = LegendPlacement.Outside,
            LegendPosition = LegendPosition.BottomLeft,
            LegendOrientation = LegendOrientation.Vertical,
            LegendLineSpacing = 6D,
            LegendFontSize = 14D
        });

        foreach (var lastRank in Ranking.GetList(out var lastUpdatedOn))
        {
            var lineSeries = new LineSeries
            {
                RenderInLegend = Settings.ShowLegend, 
                Color = lastRank.Number < LineColors.Count ? OxyColor.FromUInt32(LineColors[lastRank.Number - 1]) : OxyColors.Gray,
                LineStyle = LineStyle.Solid,
                StrokeThickness = 3.0D
            };

            var count = 1;
            var teamRankingHistory = _rankingHistory.GetByTeam(lastRank.TeamId).Values;
            // add data points for completed matches
            foreach (var rank in teamRankingHistory)
            {
                lineSeries.Points.Add(new DataPoint(count++, rank.Number));
            }

            // add data points for uncompleted matches
            for (var i = count; i <= model.Axes[0].AbsoluteMaximum; i++)
            {
                lineSeries.Points.Add(new DataPoint(i, teamRankingHistory.Last().Number));
            }

            var teamName = _teams.FirstOrDefault(t => t.TeamId == lastRank.TeamId).TeamName;
            if (string.IsNullOrEmpty(teamName)) teamName = "?";
            lineSeries.Title = teamName;
            model.Series.Add(lineSeries);
        }
			
        // Mark the last completed match day with a vertical line,
        // unless there are no open matches
        if (UseMatchDayMarker && dateHistory.Count < model.Axes[0].AbsoluteMaximum)
        {
            var lastCompletedMatchLine = new LineAnnotation()
            {
                StrokeThickness = 2D,
                Color = OxyColors.Gray, 
                LineStyle = LineStyle.Dash,
                Type = LineAnnotationType.Vertical,
                Layer = AnnotationLayer.AboveSeries,    // use full y axes height
                ClipByYAxis = false,                    // use full y axes height
                X = dateHistory.Count
            };
            if (ShowUpperDateLimit)
            {
                lastCompletedMatchLine.Text = dateHistory[^1].UpperDateLimit.ToShortDateString(); // ^1 = dateHistory.Count - 1
            }
            model.Annotations.Add(lastCompletedMatchLine);
        }

        return model;
    }

    /// <summary>
    /// Gets a <see cref="Stream"/> of an image of type PNG.
    /// </summary>
    /// <returns></returns>
    public Stream GetPng()
    {
        var stream = new MemoryStream();
        var exporter = new OxyPlot.SkiaSharp.PngExporter { Width = Settings.Width, Height = Settings.Height };
        exporter.Export(CreatePlotModel(), stream);
        return stream;
    }

    /// <summary>
    /// Gets a <see cref="Stream"/> of an image of type SVG.
    /// </summary>
    /// <returns></returns>
    public Stream GetSvg()
    {
        var stream = new MemoryStream();
        var exporter = new OxyPlot.SkiaSharp.SvgExporter { Width = Settings.Width, Height = Settings.Height };
        exporter.Export(CreatePlotModel(), stream);
        return stream;
    }

    /// <summary>
    /// If <see langword="true"/> a vertical line will be inserted at the day where the last match took place.
    /// </summary>
    public bool UseMatchDayMarker { get; set; } = true;

    /// <summary>
    /// If <see langword="true"/>, and <see cref="UseMatchDayMarker"/> is also <see langword="true"/>, the last match date is appended to the vertical line.
    /// </summary>
    public bool ShowUpperDateLimit { get; set; } = false;

    /// <summary>
    /// Gets the <see cref="Ranking"/> instance used by the <see cref="RankingChart"/>.
    /// </summary>
    public Ranking Ranking { get; }
}