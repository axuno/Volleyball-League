using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axuno.Tools.GeoSpatial;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.Data;
using TournamentManager.ExtensionMethods;

namespace TournamentManager.ModelValidators
{
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
        /// <param name="data">A <see cref="ValueTuple"/> with <see cref="GoogleGeo.GeoLocation"/> (may be NULL) and and <see cref="IList{T}"/> of <see cref="VenueDistanceResultRow"/> (may be empty).</param>
        public VenueValidator(VenueEntity model, (GoogleGeo.GeoResponse GeoResponse, IList<VenueDistanceResultRow> VenueDistanceList) data) : base(model, data)
        {
            CreateFacts();
        }

        private void CreateFacts()
        {
            Facts.Add(
                new Fact<FactId>
                {
                    Id = FactId.NameIsSet,
                    FieldNames = new[] { nameof(Model.Name) },
                    Enabled = true,
                    Type = FactType.Error,
                    CheckAsync = (cancellationToken) => Task.FromResult(
                        new FactResult
                        {
                            Message = VenueValidatorResource.NameIsSet,
                            Success = !string.IsNullOrWhiteSpace(Model.Name)
                        })
                });

            Facts.Add(
                new Fact<FactId>
                {
                    Id = FactId.AddressFieldsAreSet,
                    FieldNames = new[] { nameof(Model.PostalCode), nameof(Model.City), nameof(Model.Street) },
                    Enabled = true,
                    Type = FactType.Error,
                    CheckAsync = (cancellationToken) => Task.FromResult(
                        new FactResult
                        {
                            Message = VenueValidatorResource.AddressFieldsAreSet,
                            Success = new[] {Model.PostalCode, Model.City, Model.Street}.All(f => !string.IsNullOrWhiteSpace(f))
                        })
                });

            Facts.Add(
                new Fact<FactId>
                {
                    Id = FactId.CanBeLocated,
                    FieldNames = new[] { nameof(Model.PostalCode), nameof(Model.City), nameof(Model.Street) },
                    Enabled = Data.GeoResponse.Success,
                    Type = FactType.Warning,
                    CheckAsync = (cancellationToken) => Task.FromResult(
                        new FactResult
                        {
                            Message = VenueValidatorResource.CanBeLocated,
                            Success = Data.GeoResponse.Found
                        })
                });

            Facts.Add(
                new Fact<FactId>
                {
                    Id = FactId.LocationIsPrecise,
                    FieldNames = new[] { nameof(Model.PostalCode), nameof(Model.City), nameof(Model.Street) },
                    Enabled = Data.GeoResponse.Success && Data.GeoResponse.Found,
                    Type = FactType.Warning,
                    CheckAsync = (cancellationToken) => Task.FromResult(
                        new FactResult
                        {
                            Message = VenueValidatorResource.LocationIsPrecise,
                            Success = !new[] {GoogleGeo.LocationType.Approximate, GoogleGeo.LocationType.GeometricCenter, GoogleGeo.LocationType.RangeInterpolated}.Contains(Data.GeoResponse.GeoLocation.LocationType)
                        })
                });

            Facts.Add(
                new Fact<FactId>
                {
                    Id = FactId.NotExistingGeoLocation,
                    FieldNames = new[] { nameof(Model.Name) },
                    Enabled = Data.GeoResponse.Success && Data.GeoResponse.Found,
                    Type = FactType.Warning,
                    CheckAsync = cancellationToken =>
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
                });
        }
    }
}
