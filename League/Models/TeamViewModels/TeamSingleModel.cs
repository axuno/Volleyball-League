using System;
using System.Collections.Generic;
using System.Linq;
using League.Models.UploadViewModels;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.TypedViewClasses;

namespace League.Models.TeamViewModels
{
    public class TeamSingleModel
    {
        public TeamSingleModel(Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter)
        {
            TimeZoneConverter = timeZoneConverter;
        }

        public TournamentEntity Tournament { get; set; }

        public TeamVenueRoundRow TeamVenueRoundInfo { get; set; }

        public List<TeamUserRoundRow> TeamUserRoundInfos { get; set; }

        public bool ShowContactInfos { get; set; }

        public Axuno.Tools.DateAndTime.TimeZoneConverter TimeZoneConverter { get; }

        public (string Uri, DateTime Date) PhotoUriInfo { get; set; }
    }
}
