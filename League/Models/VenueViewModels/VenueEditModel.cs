using System.ComponentModel.DataAnnotations;
using Axuno.Tools.GeoSpatial;
using League.Components;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.ModelValidators;

namespace League.Models.VenueViewModels;

public class VenueEditModel
{
    [BindProperty]
    public VenueEditorComponentModel Venue { get; set; } = new();

    [HiddenInput]
    public string ReturnUrl { get; set; } = string.Empty;

    [HiddenInput]
    public string? Hash { get; set; }

    /// <summary>
    /// This property will be set, if a new venue shall be assigned to an existing team
    /// </summary>
    [HiddenInput]
    public long? TeamId { get; set; }

    /// <summary>
    /// This property will be set, if a new venue shall be assigned to an existing team
    /// </summary>
    [BindNever]
    public string? ForTeamName { get; set; }

    [Display(Name = "Ignore notices")]
    public bool OverrideWarnings { get; set; }

    [BindNever]
    public IList<string> TeamsUsingTheVenue { get; set; } = new List<string>();

    [BindNever]
    public VenueEntity? VenueEntity { get; set; }

    [BindNever]
    public IList<VenueDistanceResultRow> VenuesForDistance { get; set; } = new List<VenueDistanceResultRow>();

    [BindNever]
    public bool IsWarning { get; set; }

    public bool ShouldAutoUpdateLocation()
    {
        var latLngIsChanged = Venue.ShowLatLng && (VenueEntity!.Fields[VenueFields.Longitude.FieldIndex].IsChanged ||
                                                   VenueEntity.Fields[VenueFields.Latitude.FieldIndex].IsChanged);

        var latLngIsNull = !Venue.Latitude.HasValue || !Venue.Longitude.HasValue;

        var addressIsChanged = VenueEntity!.Fields.Any(f =>
            new[] {VenueFields.PostalCode.Name, VenueFields.City.Name, VenueFields.Street.Name}
                .Contains(f.Name) && f.IsChanged);

        // geo location fields are NOT changed (i.e. set by the user), but address fields ARE changed
        return !latLngIsChanged && (addressIsChanged || latLngIsNull);
    }

    public async Task<GoogleGeo.GeoResponse> TrySetGeoLocation(string googleServiceApiKey, string countryCode, TimeSpan timeout)
    {
        var geoResponse = await GoogleGeo.GetLocation(countryCode,
            string.Join(",", VenueEntity!.PostalCode, VenueEntity.City, VenueEntity.Street),
            googleServiceApiKey, timeout);

        // Update model, so we can include values in an input or hidden field
        VenueEntity.Longitude = Venue.Longitude =
            geoResponse.Success ? geoResponse.GeoLocation.Longitude?.TotalDegrees : default;
        VenueEntity.Latitude = Venue.Latitude =
            geoResponse.Success ? geoResponse.GeoLocation.Latitude?.TotalDegrees : default;
        VenueEntity.PrecisePosition = geoResponse is { Success: true, GeoLocation.LocationType: GoogleGeo.LocationType.RoofTop };

        return geoResponse;
    }

    public async Task<bool> ValidateAsync(VenueValidator venueValidator, ModelStateDictionary modelState, CancellationToken cancellationToken)
    {
        await venueValidator.CheckAsync(cancellationToken);
            
        foreach (var fact in venueValidator.GetFailedFacts())
        {
            if (fact.Exception != null)
            {
                throw new InvalidOperationException(null, fact.Exception);
            }

            if (fact.Type == FactType.Critical || fact.Type == FactType.Error)
            {
                foreach (var fieldName in fact.FieldNames)
                {
                    modelState.AddModelError(string.Join('.', Venue.HtmlFieldPrefix, fieldName), fact.Message);
                }
            }
            else
            {
                modelState.AddModelError(string.Empty, fact.Message);
                // Validator generates FactType.Warning only, if no errors exist
                IsWarning = true;
            }
        }

        // The Hash is re-calculated with the new submitted values.
        // We have to compare to the original hidden Hash field value,
        // because to override warnings, form fields must be unchanged since last post
        var newHash = ComputeInputHash();
        if (IsWarning && OverrideWarnings && newHash == Hash)
        {
            modelState.Clear();
            IsWarning = false;
        }

        if (!modelState.IsValid)
        {
            // Show checkbox unchecked
            OverrideWarnings = false;
            // Set hash field value to latest form fields content
            Hash = newHash;
        }

        return modelState.IsValid;
    }

    private string ComputeInputHash()
    {
        return Axuno.Tools.Hash.Md5.GetHash(string.Join(string.Empty,
            Venue.Id.ToString(),
            Venue.IsNew.ToString(),
            Venue.Name ?? string.Empty,
            Venue.Extension ?? string.Empty,
            Venue.PostalCode ?? string.Empty,
            Venue.City ?? string.Empty,
            Venue.Street ?? string.Empty,
            // don't include Venue.Direction because it's not part of validation
            Venue.Latitude.HasValue ? Venue.Latitude.ToString() : string.Empty,
            Venue.Longitude.HasValue ? Venue.Longitude.ToString() : string.Empty));
    }
}
