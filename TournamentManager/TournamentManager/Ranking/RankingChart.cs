using System;
using System.Collections.Generic;
using ZedGraph;
using System.Drawing;
using System.Linq;

namespace TournamentManager.Ranking
{
	/// <summary>
	/// Class for creating a <see cref="RankingChart"/> image from a <see cref="Ranking"/>.
	/// </summary>
	public class RankingChart
	{
        private int _width = 800;
		private int _height = 600;
		private Color _fillColor;
		private string _title;
		private string _xTitle;
		private string _yTitle;
        private RankingHistory _rankingHistory;
		private List<(long TeamId, string TeamName)> _teams;

        public RankingChart(Ranking ranking, List<(long TeamId, string TeamName)> teams) : this(ranking, teams, 1.5f, "Chart", "Match Day", "Rank")
		{}

		public RankingChart(Ranking ranking, List<(long TeamId, string TeamName)> teams, float scaleFactor, string title, string xTitle, string yTitle)
        {
            Ranking = ranking;
            _rankingHistory = ranking.GetRankingHistory();
            _teams = teams ?? throw new ArgumentNullException(nameof(teams));
			_width = (int)(_width / scaleFactor);
			_height = (int)(_height / scaleFactor);
            _fillColor = Color.FromArgb(0xEF, 0xFF, 0xEF);
			_xTitle = xTitle ?? throw new ArgumentNullException(nameof(xTitle)); ;
			_yTitle = yTitle ?? throw new ArgumentNullException(nameof(yTitle)); ;
			_title = title ?? throw new ArgumentNullException(nameof(title)); ;
		}

		public Image GetImage()
		{
            var dateHistory = _rankingHistory.GetByMatchDay();
            var maxNumOfEntries = dateHistory.Max(dh => dh.Count);

            var graphPane = new GraphPane(new RectangleF(0, 0, _width, _height), _title, _xTitle, _yTitle);
			graphPane.Fill.Color = _fillColor;
			graphPane.Chart.Fill.Color = _fillColor;
			graphPane.Title.FontSpec.Family = "Arial";
			graphPane.Title.FontSpec.Size = 16f;
			graphPane.Title.FontSpec.IsAntiAlias = true;
			graphPane.Title.FontSpec.IsBold = true;
			graphPane.Title.FontSpec.IsDropShadow = false;

			graphPane.XAxis.MajorGrid.IsVisible = true;
			graphPane.XAxis.Scale.Min = 1;
			graphPane.XAxis.Scale.Max = dateHistory.Count + Ranking.MatchesToPlay.GroupBy(u => u.MatchDate?.Date ?? DateTime.MinValue).Count();
            graphPane.XAxis.Scale.MajorStep = 1.0;
			graphPane.XAxis.Scale.MinorStep = 1.0;
			graphPane.XAxis.Scale.FontSpec.Family = "Arial";
			graphPane.XAxis.Title.FontSpec.Family = "Arial";
			graphPane.XAxis.Title.FontSpec.Size = 14f;
			graphPane.XAxis.Title.FontSpec.IsAntiAlias = true;
			graphPane.XAxis.Title.FontSpec.IsBold = false;
			graphPane.XAxis.Title.FontSpec.IsDropShadow = false;

			graphPane.YAxis.MajorGrid.IsVisible = true;
			graphPane.YAxis.Scale.Min = 0.5f;
			graphPane.YAxis.Scale.Max = (double)maxNumOfEntries + 0.5f;
			graphPane.YAxis.Scale.MajorStep = 1.0;
			graphPane.YAxis.Scale.MinorStep = 1.0;
			graphPane.YAxis.Scale.IsReverse = true;
			graphPane.YAxis.Scale.FontSpec.Family = "Arial";
			graphPane.YAxis.Title.FontSpec.Family = "Arial";
			graphPane.YAxis.Title.FontSpec.Size = 14f;
			graphPane.YAxis.Title.FontSpec.IsAntiAlias = true;
			graphPane.YAxis.Title.FontSpec.IsBold = false;
			graphPane.YAxis.Title.FontSpec.IsDropShadow = false;

			graphPane.Legend.Position = LegendPos.Bottom;
			graphPane.Legend.IsHStack = false;
			graphPane.Legend.FontSpec.Family = "Arial";
			graphPane.Legend.FontSpec.Size = 12f;
			graphPane.Legend.FontSpec.IsAntiAlias = true;
			graphPane.Legend.FontSpec.IsBold = false;
			graphPane.Legend.FontSpec.IsDropShadow = false;

            var color = new List<Color>(new[]
            {
                Color.Red, Color.Green, Color.Orange, Color.Blue, Color.Magenta, Color.Gray, Color.Gold, Color.Brown,
                Color.Aqua, Color.Black
            });

			foreach (var lastRank in Ranking.GetList(out var lastUpdatedOn))
			{
				var ppl = new PointPairList();

				var count = 1;
                var teamRankingHistory = _rankingHistory.GetByTeam(lastRank.TeamId).Values;
                // add point pairs for completed matches
                foreach (var rank in teamRankingHistory)
				{
					ppl.Add(count++, rank.Number);
				}

                // add point pairs for uncompleted matches
                for (var i = count; i <= graphPane.XAxis.Scale.Max; i++)
                {
                    ppl.Add(i, teamRankingHistory.Last().Number);
                }

                var teamName = _teams.FirstOrDefault(t => t.TeamId == lastRank.TeamId).TeamName;
                if (string.IsNullOrEmpty(teamName)) teamName = "?";

                var curve = graphPane.AddCurve(teamName, ppl, color[lastRank.Number - 1], SymbolType.None);
				curve.Line.Width = 4.0f;
				curve.Line.IsAntiAlias = true;
				curve.Symbol.Size = 14.0f;
			}
			
            // Mark the last completed match day with a vertical line
            if (UseMatchDayMarker)
            {
                double todayX = dateHistory.Count;

                var pplToday = new PointPairList
                {
                    {todayX, graphPane.YAxis.Scale.Min}, {todayX, graphPane.YAxis.Scale.Max}
                };
                var curveToday = graphPane.AddCurve(string.Empty, pplToday, Color.Red, SymbolType.None);
                curveToday.Line.Width = 1.0f;
                curveToday.Line.IsAntiAlias = true;

                // Add a text box with date at the right bottom of the GraphPane
                if (ShowUpperDateLimit)
                {
                    var todayText = new TextObj(
                        dateHistory[dateHistory.Count - 1].UpperDateLimit.ToShortDateString(),
                        .99, .99, CoordType.PaneFraction, AlignH.Right, AlignV.Bottom)
                    {
                        FontSpec =
                        {
                            StringAlignment = StringAlignment.Near,
                            Family = "Arial",
                            Size = 16f,
                            IsAntiAlias = true,
                            IsBold = false,
                            IsDropShadow = false,
                            FontColor = Color.Black,
                            Fill = {IsVisible = false},
                            Border = {IsVisible = false}
                        }
                    };
                    graphPane.GraphObjList.Add(todayText);
                }
            }

			return graphPane.GetImage();
            // Example:
            // graphPane.GetImage().Save(Response.OutputStream, ImageFormat.Png);
        }

        public bool UseMatchDayMarker { get; set; } = true;

        public bool ShowUpperDateLimit { get; set; } = false;

        public Ranking Ranking { get; }
    }
}