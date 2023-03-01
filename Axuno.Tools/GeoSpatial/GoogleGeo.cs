using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace Axuno.Tools.GeoSpatial;

/// <summary>
/// Library using the Google Geocoding API
/// Docs for Google Geocoding API: https://developers.google.com/maps/documentation/geocoding/
/// </summary>
public class GoogleGeo
{
    /// <summary>
    /// The location type returned by the Google geo coding API.
    /// </summary>
    public enum LocationType
    {
        /// <summary>
        /// The addresses for which Google has location information accurate down to street address precision
        /// </summary>
        RoofTop,
        /// <summary>
        /// The addresses that reflect an approximation (usually on a road) interpolated between two precise points
        /// (such as intersections). An interpolated range generally indicates that rooftop geo codes are unavailable
        /// for a street address
        /// </summary>
        RangeInterpolated,
        /// <summary>
        /// Only geometric centers of a location such as a poly-line (for example, a street) or polygon (region)
        /// </summary>
        GeometricCenter,
        /// <summary>
        /// Addresses that are characterized as approximate
        /// </summary>
        Approximate
    }

    /// <summary>
    /// A geographic location.
    /// </summary>
    public class GeoLocation
    {
        /// <summary>
        /// Gets or sets the latitude of a location.
        /// </summary>
        public Latitude? Latitude { get; set; }
        /// <summary>
        /// Gets or sets the longitude of a location.
        /// </summary>
        public Longitude? Longitude { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="LocationType"/> of a location, indicating the precision.
        /// </summary>
        public LocationType LocationType { get; set; }
    }

    /// <summary>
    /// The response from a Google Geocoding API 
    /// </summary>
    public class GeoResponse
    {
        /// <summary>
        /// Gets or sets the <see cref="GeoLocation"/>.
        /// </summary>
        public GeoLocation GeoLocation { get; set; } = new();
        /// <summary>
        /// Gets or sets the status text returned from the server API.
        /// </summary>
        public string? StatusText { get; set; }
        /// <summary>
        /// Is <see langword="true"/>, if the request was successful, else <see langword="false"/>
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// <see langword="true"/>, if the request was successful, and an address was found, else <see langword="false"/>.
        /// If <see langword="true"/>, the <see cref="GeoLocation"/> property will be filled.
        /// </summary>
        public bool Found { get; set; }
        /// <summary>
        /// Gets the exception that occurred while processing the request, or is null if successful.
        /// </summary>
        public Exception? Exception { get; set; }
        /// <summary>
        /// The <see cref="XmlDocument"/> returned from the server, if the request was successful.
        /// </summary>
        internal XmlDocument? XmlDocument { get; set; }
    }

    /// <summary>
    /// Gets the geo data for a postal address.
    /// </summary>
    /// <param name="country">The two letter ISO 3166-1 country code (e.g. DE=Germany, FR=France, US=USA, IT=Italy).</param>
    /// <param name="address">Full postal address, including country</param>
    /// <param name="apiKey">Google API key</param>
    /// <param name="timeout">The <see cref="TimeSpan"/> to wait until the request times out.</param>
    /// <returns>Returns the <see cref="GeoResponse"/> from the Google API. If <see cref="GeoResponse.Success"/> the <see cref="GeoLocation"/> members will be set.</returns>
    public static async Task<GeoResponse> GetLocation(string country, string address, string apiKey, TimeSpan timeout)
    {
        using var client = new HttpClient();
        return await GetLocation(client, country, address, apiKey, timeout);
    }

    /// <summary>
    /// Gets the geo data for a postal address for unit tests.
    /// </summary>
    /// <param name="client">The <see cref="HttpClient"/> to use (mocked for unit tests).</param>
    /// <param name="country">The two letter ISO 3166-1 country code (e.g. DE=Germany, FR=France, US=USA, IT=Italy).</param>
    /// <param name="address">Full postal address, including country</param>
    /// <param name="apiKey">Google API key</param>
    /// <param name="timeout">The <see cref="TimeSpan"/> to wait until the request times out.</param>
    /// <returns>Returns the <see cref="GeoResponse"/> from the Google API.</returns>
    internal static async Task<GeoResponse> GetLocation(HttpClient client, string country, string address, string apiKey, TimeSpan timeout)
    {
        try
        {
            client.Timeout = timeout;
            return GetLocation(await EvaluateResponse(await CallServerApi(client, country, address, apiKey)));
        }
        catch (Exception e)
        {
            return new GeoResponse { Success = false, StatusText = string.Empty, Exception = e };
        }
    }

    private static async Task<HttpResponseMessage> CallServerApi(HttpClient client, string country, string address, string apiKey)
    {
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
        client.BaseAddress =
            new Uri(
                $"https://maps.googleapis.com/maps/api/geocode/xml?address={HttpUtility.UrlEncode(address)}&components=country:{country}&key={apiKey}");
        return await client.GetAsync(client.BaseAddress);        }

    private static async Task<GeoResponse> EvaluateResponse(HttpResponseMessage httpResponse)
    {
        const string statusNode = "/GeocodeResponse/status";
        var geoResponse = new GeoResponse { Success = httpResponse.IsSuccessStatusCode };

        if (!geoResponse.Success) return geoResponse;

        geoResponse.XmlDocument = new XmlDocument();
        geoResponse.XmlDocument.LoadXml(await httpResponse.Content.ReadAsStringAsync());
        geoResponse.StatusText = geoResponse.XmlDocument.SelectSingleNode(statusNode)?.InnerText;

        switch (geoResponse.StatusText)
        {
            case null:
                throw new Exception($"XML node {statusNode} not found");
            case "OK":
                geoResponse.Found = true;
                geoResponse.Success = true;
                return geoResponse;
            case "ZERO_RESULTS":
                geoResponse.Found = false;
                geoResponse.Success = true;
                return geoResponse;
            case "INVALID_REQUEST":
            case "REQUEST_DENIED":
            case "OVER_DAILY_LIMIT":
            case "OVER_QUERY_LIMIT":
            case "UNKNOWN_ERROR":
            default:
                geoResponse.Success = false;
                geoResponse.Found = false;
                return geoResponse;
        }
    }

    private static GeoResponse GetLocation(GeoResponse geoResponse)
    {
        if (geoResponse.StatusText != null && !(geoResponse.Success && geoResponse.StatusText.Equals("OK", StringComparison.InvariantCulture)))
        {
            return geoResponse;
        }

        var latNode = geoResponse.XmlDocument?.SelectSingleNode("/GeocodeResponse/result/geometry/location/lat");
        var lngNode = geoResponse.XmlDocument?.SelectSingleNode("/GeocodeResponse/result/geometry/location/lng");
        var locTypeNode = geoResponse.XmlDocument?.SelectSingleNode("/GeocodeResponse/result/geometry/location_type");

        if (latNode == null || lngNode == null || locTypeNode == null)
        {
            throw new Exception("XML child nodes in /GeocodeResponse/result/geometry not found");
        }

        switch (locTypeNode.InnerText)
        {
            case "ROOFTOP":
                geoResponse.GeoLocation.LocationType = LocationType.RoofTop;
                break;
            case "RANGE_INTERPOLATED":
                geoResponse.GeoLocation.LocationType = LocationType.RangeInterpolated;
                break;
            case "GEOMETRIC_CENTER":
                geoResponse.GeoLocation.LocationType = LocationType.GeometricCenter;
                break;
            case "APPROXIMATE":
                geoResponse.GeoLocation.LocationType = LocationType.Approximate;
                break;
            default:
                geoResponse.Success = false;
                return geoResponse;
        }

        geoResponse.GeoLocation.Latitude = new Latitude(Angle.FromDegrees(double.Parse(latNode.InnerText,
            CultureInfo.InvariantCulture.NumberFormat)));
        geoResponse.GeoLocation.Longitude = new Longitude(Angle.FromDegrees(double.Parse(lngNode.InnerText,
            CultureInfo.InvariantCulture.NumberFormat)));

        return geoResponse;
    }
}
