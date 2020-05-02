using System.Collections.Generic;
using System.Linq;
using System.Text;
using League.ConfigurationPoco;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.TypedViewClasses;

namespace League.Models.MapViewModels
{
	public class MapModel
	{
		const string _format = "{{ Lat: {0}, Lng: {1}, title: \"{2}\", Descr: \"{3}\" }},\n";

        public MapModel()
        {
            MaxLatitude = MaxLongitude = MinLatitude = MinLongitude = "0";
        }

	    public MapModel(VenueTeamRow venue) : this()
	    {
	        Venues.Add(venue);
            PrepareSingleVenue();
        }

        public MapModel(IEnumerable<VenueTeamRow> venues) : this()
        {
            Venues.AddRange(venues);
            PrepareAllVenues();
        }

	    private void PrepareSingleVenue()
        {
            var venue = Venues.FirstOrDefault();

			if (venue?.Latitude == null || venue?.Longitude == null)
				return;
			
			Locations = string.Format(_format,
											  venue.Latitude.Value.ToString("###.########", System.Globalization.CultureInfo.InvariantCulture),
											  venue.Longitude.Value.ToString("###.########", System.Globalization.CultureInfo.InvariantCulture),
                                              $"{venue.VenueName}, {venue.City}",
                                              $"<b>{venue.VenueName}</b><br />{venue.Street}<br />{venue.PostalCode} {venue.City}<br />");

			Locations = Locations.TrimEnd(new[] { ',', '\n' });
		}

		private void PrepareAllVenues()
        {
			var locationJsObject = new StringBuilder("\n");

			if (!Venues.Any(v => v.Longitude.HasValue && v.Latitude.HasValue))
				return;
            
			MaxLongitude = Venues.Max(l => l.Longitude)?.ToString("###.########", System.Globalization.CultureInfo.InvariantCulture);
			MinLongitude = Venues.Min(l => l.Longitude)?.ToString("###.########", System.Globalization.CultureInfo.InvariantCulture);
			MaxLatitude = Venues.Max(l => l.Latitude)?.ToString("###.########", System.Globalization.CultureInfo.InvariantCulture);
			MinLatitude = Venues.Min(l => l.Latitude)?.ToString("###.########", System.Globalization.CultureInfo.InvariantCulture);
            
			foreach (var venue in Venues)
			{
				var teamDescription = new StringBuilder();

                var teamsOfVenue = Venues.Where(v => v.VenueId == venue.VenueId).ToList();
                foreach (var tov in teamsOfVenue)
				{
					teamDescription.AppendFormat("{0} ({1})<br />", tov.TeamName, tov.TeamClubName).Replace("()", string.Empty); // remove () if ClubName is empty
				}
				teamDescription.Remove(teamDescription.Length - 6, 6);  // remove last <br />

				var venueDescription =
                    $"<b>{venue.VenueName}</b><br />{venue.Street}<br />{venue.PostalCode} {venue.City}<br /><br /><b>Team{(teamsOfVenue.Count() > 1 ? "s" : string.Empty)}:</b><br />";

				if (venue.Latitude.HasValue && venue.Longitude.HasValue)
				{
					var teamsCount = teamsOfVenue.Count;
					locationJsObject.AppendFormat(_format,
													venue.Latitude.Value.ToString("###.########", System.Globalization.CultureInfo.InvariantCulture),
													venue.Longitude.Value.ToString("###.########", System.Globalization.CultureInfo.InvariantCulture),
                                                    $"{venue.VenueName}, {venue.City} - {teamsCount} Team{(teamsCount > 1 ? "s" : string.Empty)}",
													venueDescription + teamDescription);
				}
			}

			Locations = locationJsObject.ToString().TrimEnd(new[] { ',', '\n' });
		}

		public string Locations { get; set; }
		public string MaxLongitude { get; set; }
		public string MinLongitude { get; set; }
		public string MaxLatitude { get; set; }
		public string MinLatitude { get; set; }
        public bool IsSingleValue => Venues.Count == 1;
		public TournamentEntity Tournament { get; set; }
        public List<VenueTeamRow> Venues { get; set; } = new List<VenueTeamRow>();
        public GoogleConfiguration GoogleConfiguration { get; set; }
	}
}
