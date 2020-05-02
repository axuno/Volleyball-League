using System;
using System.Collections.Generic;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.TypedViewClasses;

namespace League.Models.TeamApplicationViewModels
{
    public class ApplicationListModel
    {
        private readonly Axuno.Tools.DateAndTime.TimeZoneConverter _timeZoneConverter;

        public ApplicationListModel(Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter)
        {
            _timeZoneConverter = timeZoneConverter;
        }

        public TournamentEntity Tournament { get; set; }

        public List<LatestTeamTournamentRow> TournamentRoundTeams { get; set; }

        public Dictionary<long, DateTime> TeamRegisteredOn { get; set; } = new Dictionary<long, DateTime>();

        public Axuno.Tools.DateAndTime.TimeZoneConverter TimeZoneConverter => _timeZoneConverter;
    }
}
