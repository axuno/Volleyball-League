using System;
using System.Collections.Generic;
using System.Linq;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.TypedViewClasses;

namespace League.Models.MatchViewModels
{
    public class ResultsViewModel
    {
        private readonly Axuno.Tools.DateAndTime.TimeZoneConverter _timeZoneConverter;

        public ResultsViewModel(Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter)
        {
            _timeZoneConverter = timeZoneConverter;
        }

        public Axuno.Tools.DateAndTime.TimeZoneConverter TimeZoneConverter => _timeZoneConverter;

        public TournamentEntity Tournament { get; set; }

        public List<CompletedMatchRow> CompletedMatches { get; set; }

        public long? ActiveRoundId { get; set; }

        /// <summary>
        /// Model used when result was entered
        /// </summary>
        public EnterResultViewModel.MatchResultMessage MatchResultMessage { get; set; }
    }
}
