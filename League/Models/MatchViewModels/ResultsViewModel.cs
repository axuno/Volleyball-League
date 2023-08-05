using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.TypedViewClasses;

namespace League.Models.MatchViewModels;

public class ResultsViewModel
{
    public ResultsViewModel(Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter)
    {
        TimeZoneConverter = timeZoneConverter;
    }

    public Axuno.Tools.DateAndTime.TimeZoneConverter TimeZoneConverter { get; }

    public TournamentEntity? Tournament { get; set; }

    public List<CompletedMatchRow> CompletedMatches { get; set; } = new();

    public long? ActiveRoundId { get; set; }

    /// <summary>
    /// Model used when result was entered
    /// </summary>
    public EnterResultViewModel.MatchResultMessage? MatchResultMessage { get; set; }
}
