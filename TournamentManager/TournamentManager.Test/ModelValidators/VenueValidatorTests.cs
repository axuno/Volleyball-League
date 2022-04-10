using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Axuno.Tools.GeoSpatial;
using NUnit.Framework;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.Data;
using TournamentManager.ModelValidators;
using Moq;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.ExtensionMethods;
using TournamentManager.Tests.TestComponents;

namespace TournamentManager.Tests.ModelValidators
{
    [TestFixture]
    public class VenueValidatorTests
    {
        public VenueValidatorTests()
        { }

        [TestCase("The venue name", true)]
        [TestCase(null, false)]
        public async Task Venue_Name_Should_Be_Set(string venueName, bool expected)
        {
            var venue = new VenueEntity{Name = venueName};

            var vv = new VenueValidator(venue, (new GoogleGeo.GeoResponse(), new List<VenueDistanceResultRow>()));

            var factResult = await vv.CheckAsync(VenueValidator.FactId.NameIsSet, CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.IsTrue(factResult.Enabled);
                Assert.AreEqual(expected, factResult.Success);
                Assert.IsNull(factResult.Exception);
            });
        }

        [TestCase("12345", "City", "Street", true)]
        [TestCase("12345", "City", null, false)]
        [TestCase("12345", null, null,false)]
        [TestCase(null, null, null, false)]
        public async Task Address_Fields_Should_Be_Set(string postalCode, string city, string street, bool expected)
        {
            var venue = new VenueEntity { PostalCode = postalCode, City = city, Street = street };

            var vv = new VenueValidator(venue, (new GoogleGeo.GeoResponse(), new List<VenueDistanceResultRow>()));

            var factResult = await vv.CheckAsync(VenueValidator.FactId.AddressFieldsAreSet, CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.IsTrue(factResult.Enabled);
                Assert.AreEqual(expected, factResult.Success);
                Assert.IsNull(factResult.Exception);
            });
        }


        [Test]
        public async Task StatusText_Should_Be_Set()
        {
            var venue = new VenueEntity();
            var vv = new VenueValidator(venue, (new GoogleGeo.GeoResponse{Success = true, Found = true, StatusText = "OK", Exception = null, GeoLocation = new GoogleGeo.GeoLocation{LocationType = GoogleGeo.LocationType.Approximate}}, new List<VenueDistanceResultRow>()));
            
            var factResult = await vv.CheckAsync(VenueValidator.FactId.CanBeLocated, CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.AreEqual("OK", vv.Data.GeoResponse.StatusText);
                Assert.IsNull(vv.Data.GeoResponse.Exception);
            });
        }

        [Test]
        public async Task Exception_Should_Be_Set()
        {
            var venue = new VenueEntity();
            var vv = new VenueValidator(venue, (new GoogleGeo.GeoResponse{Success = false, Found = false, Exception = new ArgumentException(), GeoLocation = new GoogleGeo.GeoLocation{LocationType = GoogleGeo.LocationType.Approximate}}, new List<VenueDistanceResultRow>()));
            
            var factResult = await vv.CheckAsync(VenueValidator.FactId.CanBeLocated, CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<ArgumentException>(vv.Data.GeoResponse.Exception);
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task Address_Should_Be_Locatable(bool found)
        {
            var venue = new VenueEntity();
            
            var vv = new VenueValidator(venue, (new GoogleGeo.GeoResponse{Success = true, Found = found, StatusText = "OK", Exception = null, GeoLocation = new GoogleGeo.GeoLocation{LocationType = GoogleGeo.LocationType.Approximate}}, new List<VenueDistanceResultRow>()));
            
            var factResult = await vv.CheckAsync(VenueValidator.FactId.CanBeLocated, CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.IsTrue(factResult.Enabled);
                Assert.AreEqual(found, factResult.Success);
                Assert.IsNull(factResult.Exception);
            });
        }

        [TestCase(GoogleGeo.LocationType.Approximate, false)]
        [TestCase(GoogleGeo.LocationType.GeometricCenter, false)]
        [TestCase(GoogleGeo.LocationType.RangeInterpolated, false)]
        [TestCase(GoogleGeo.LocationType.RoofTop, true)]
        public async Task Position_should_be_found(GoogleGeo.LocationType locationType, bool expected)
        {
            var venue = new VenueEntity();

            var vv = new VenueValidator(venue, (new GoogleGeo.GeoResponse{Success = true, Found = true, GeoLocation = new GoogleGeo.GeoLocation{LocationType = locationType}}, new List<VenueDistanceResultRow>()));

            var factResult = await vv.CheckAsync(VenueValidator.FactId.LocationIsPrecise, CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.IsTrue(factResult.Enabled);
                Assert.AreEqual(expected, factResult.Success);
                Assert.IsNull(factResult.Exception);
            });
        }

        [TestCase(GoogleGeo.LocationType.Approximate, true)]
        [TestCase(GoogleGeo.LocationType.GeometricCenter, true)]
        [TestCase(GoogleGeo.LocationType.RangeInterpolated, true)]
        [TestCase(GoogleGeo.LocationType.RoofTop, true)]
        public async Task No_Close_By_Other_Venue_Should_be_disabled(GoogleGeo.LocationType locationType, bool enabled)
        {
            var venue = new VenueEntity();

            var vv = new VenueValidator(venue, (new GoogleGeo.GeoResponse{Success = true, Found = true, GeoLocation = new GoogleGeo.GeoLocation{LocationType = locationType}}, new List<VenueDistanceResultRow>()));

            var factResult = await vv.CheckAsync(VenueValidator.FactId.NotExistingGeoLocation, CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(factResult.Enabled, enabled);
                Assert.AreEqual(enabled, factResult.Success);
                Assert.IsNull(factResult.Exception);
            });
        }

        [TestCase(GoogleGeo.LocationType.RoofTop, false, false, false)]
        [TestCase(GoogleGeo.LocationType.Approximate, false, false, false)]
        [TestCase(GoogleGeo.LocationType.GeometricCenter, false, true, true)]
        [TestCase(GoogleGeo.LocationType.RangeInterpolated, true, true, true)]
        public async Task No_Close_By_Other_Venue(GoogleGeo.LocationType locationType, bool isNew, bool emptyList, bool success)
        {
            var venue = new VenueEntity {Id = 1, IsNew = isNew};

            var closeByVenues = new List<VenueDistanceResultRow>
            {
                new() {Id = 1, Distance = .3},
                new() {Id = 2, Distance = .4},
                new() {Id = 3, Distance = .7},
                new() {Id = 4, Distance = .9}
            };
            
            if (emptyList) closeByVenues.Clear();

            var vv = new VenueValidator(venue, (new GoogleGeo.GeoResponse{Success = true, Found = true, GeoLocation = new GoogleGeo.GeoLocation{LocationType = locationType}}, closeByVenues));

            var factResult = await vv.CheckAsync(VenueValidator.FactId.NotExistingGeoLocation, CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.IsTrue(factResult.Enabled);
                Assert.AreEqual(success, factResult.Success);
                if(!success) Assert.IsTrue(factResult.Message.Contains((closeByVenues.Max(v => v.Distance)*1000).ToString(CultureInfo.InvariantCulture)));
                Assert.IsNull(factResult.Exception);
            });
        }
    }
}
