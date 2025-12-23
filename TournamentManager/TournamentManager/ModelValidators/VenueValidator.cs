using Axuno.Tools.GeoSpatial;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.TypedViewClasses;

namespace TournamentManager.ModelValidators;

public class VenueValidator : AbstractValidator<VenueEntity, (GoogleGeo.GeoResponse GeoResponse, IList<VenueDistanceResultRow> VenueDistanceList), VenueValidator.FactId>
{
    public enum FactId
    {
        NameIsSet,
        AddressFieldsAreSet,
        CanBeLocated,
        LocationIsPrecise,
        NotExistingGeoLocation
    }

    /// <summary>
    /// CTOR.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="data">A <see cref="ValueTuple"/> with <see cref="GoogleGeo.GeoLocation"/> (maybe NULL) and <see cref="IList{T}"/> of <see cref="VenueDistanceResultRow"/> (maybe empty).</param>
    public VenueValidator(VenueEntity model, (GoogleGeo.GeoResponse GeoResponse, IList<VenueDistanceResultRow> VenueDistanceList) data) : base(model, data)
    {
        CreateFacts();
    }

    private void CreateFacts()
    {
        Facts.Add(NameIsSet());
        Facts.Add(AddressFieldsAreSet());
        Facts.Add(CanBeLocated());
        Facts.Add(LocationIsPrecise());
        Facts.Add(NotExistingGeoLocation());
    }

    private Fact<FactId> NotExistingGeoLocation()
    {
        return new()
        {
            Id = FactId.NotExistingGeoLocation,
            FieldNames = [nameof(Model.Name)],
            Enabled = Data.GeoResponse is { Success: true, Found: true },
            Type = FactType.Warning,
            CheckAsync = cancellationToken => FactResult()
        };

        Task<FactResult> FactResult()
        {
            var result = new FactResult
            {
                Message = VenueValidatorResource.NotExistingGeoLocation,
                Success = true
            };
                        
            // If the list contains other venues close-by then the current one, then create a warning
            if (Data.VenueDistanceList.Any(v => Model.IsNew || v.Id != Model.Id))
            {
                // express maximum distance in meters, rounded to next 100
                var max = (int) (Math.Round(Data.VenueDistanceList.Max(v => v.Distance),1) * 1000);
                result.Message = string.Format(VenueValidatorResource.NotExistingGeoLocation, max);
                result.Success = false;
            }

            return Task.FromResult(result);
        }
    }

    private Fact<FactId> LocationIsPrecise()
    {
        return new()
        {
            Id = FactId.LocationIsPrecise,
            FieldNames = [nameof(Model.PostalCode), nameof(Model.City), nameof(Model.Street)],
            Enabled = Data.GeoResponse is { Success: true, Found: true },
            Type = FactType.Warning,
            CheckAsync = (cancellationToken) => Task.FromResult(
                new FactResult
                {
                    Message = VenueValidatorResource.LocationIsPrecise,
                    Success = !new[] {GoogleGeo.LocationType.Approximate, GoogleGeo.LocationType.GeometricCenter, GoogleGeo.LocationType.RangeInterpolated}.Contains(Data.GeoResponse.GeoLocation.LocationType)
                })
        };
    }

    private Fact<FactId> CanBeLocated()
    {
        return new()
        {
            Id = FactId.CanBeLocated,
            FieldNames = [nameof(Model.PostalCode), nameof(Model.City), nameof(Model.Street)],
            Enabled = Data.GeoResponse.Success,
            Type = FactType.Warning,
            CheckAsync = (cancellationToken) => Task.FromResult(
                new FactResult
                {
                    Message = VenueValidatorResource.CanBeLocated,
                    Success = Data.GeoResponse.Found
                })
        };
    }

    private Fact<FactId> AddressFieldsAreSet()
    {
        return new()
        {
            Id = FactId.AddressFieldsAreSet,
            FieldNames = [nameof(Model.PostalCode), nameof(Model.City), nameof(Model.Street)],
            Enabled = true,
            Type = FactType.Error,
            CheckAsync = (cancellationToken) => Task.FromResult(
                new FactResult
                {
                    Message = VenueValidatorResource.AddressFieldsAreSet,
                    Success = new List<string> {Model.PostalCode, Model.City, Model.Street}.TrueForAll(f => !string.IsNullOrWhiteSpace(f))
                })
        };
    }

    private Fact<FactId> NameIsSet()
    {
        return new()
        {
            Id = FactId.NameIsSet,
            FieldNames = [nameof(Model.Name)],
            Enabled = true,
            Type = FactType.Error,
            CheckAsync = (cancellationToken) => Task.FromResult(
                new FactResult
                {
                    Message = VenueValidatorResource.NameIsSet,
                    Success = !string.IsNullOrWhiteSpace(Model.Name)
                })
        };
    }
}
