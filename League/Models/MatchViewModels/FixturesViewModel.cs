using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.TypedViewClasses;

namespace League.Models.MatchViewModels;

public class FixturesViewModel
{
    public FixturesViewModel(Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter)
    {
        TimeZoneConverter = timeZoneConverter;
    }

    public Axuno.Tools.DateAndTime.TimeZoneConverter TimeZoneConverter { get; }

    public TournamentEntity Tournament { get; set; }

    public List<PlannedMatchRow> PlannedMatches { get; set; }

    public long? ActiveRoundId { get; set; }

    /// <summary>
    /// Model used when fixtures were updated
    /// </summary>
    public EditFixtureViewModel.FixtureMessage FixtureMessage { get; set; }
}