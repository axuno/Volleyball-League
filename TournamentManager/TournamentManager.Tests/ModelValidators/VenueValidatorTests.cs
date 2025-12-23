using System.Globalization;
using Axuno.Tools.GeoSpatial;
using NUnit.Framework;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.ModelValidators;
using TournamentManager.DAL.TypedViewClasses;

namespace TournamentManager.Tests.ModelValidators;

[TestFixture]
public class VenueValidatorTests
{
    public VenueValidatorTests()
    { }

    [TestCase("The venue name", true)]
    [TestCase(null, false)]
    public async Task Venue_Name_Should_Be_Set(string? venueName, bool expected)
    {
        var venue = new VenueEntity{Name = venueName};

        var vv = new VenueValidator(venue, (new(), []));

        var factResult = await vv.CheckAsync(VenueValidator.FactId.NameIsSet, CancellationToken.None);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(factResult.Enabled, Is.True);
            Assert.That(factResult.Success, Is.EqualTo(expected));
            Assert.That(factResult.Exception, Is.Null);
        }
    }

    [TestCase("12345", "City", "Street", true)]
    [TestCase("12345", "City", null, false)]
    [TestCase("12345", null, null,false)]
    [TestCase(null, null, null, false)]
    public async Task Address_Fields_Should_Be_Set(string? postalCode, string? city, string? street, bool expected)
    {
        var venue = new VenueEntity { PostalCode = postalCode, City = city, Street = street };

        var vv = new VenueValidator(venue, (new(), []));

        var factResult = await vv.CheckAsync(VenueValidator.FactId.AddressFieldsAreSet, CancellationToken.None);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(factResult.Enabled, Is.True);
            Assert.That(factResult.Success, Is.EqualTo(expected));
            Assert.That(factResult.Exception, Is.Null);
        }
    }


    [Test]
    public async Task StatusText_Should_Be_Set()
    {
        var venue = new VenueEntity();
        var vv = new VenueValidator(venue, (new() {Success = true, Found = true, StatusText = "OK", Exception = null, GeoLocation = new() {LocationType = GoogleGeo.LocationType.Approximate}}, []));
        _ = await vv.CheckAsync(VenueValidator.FactId.CanBeLocated, CancellationToken.None);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(vv.Data.GeoResponse.StatusText, Is.EqualTo("OK"));
            Assert.That(vv.Data.GeoResponse.Exception, Is.Null);
        }
    }

    [Test]
    public async Task Exception_Should_Be_Set()
    {
        var venue = new VenueEntity();
        var vv = new VenueValidator(venue, (new() {Success = false, Found = false, Exception = new ArgumentException(), GeoLocation = new() {LocationType = GoogleGeo.LocationType.Approximate}}, []));
        _ = await vv.CheckAsync(VenueValidator.FactId.CanBeLocated, CancellationToken.None);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(vv.Data.GeoResponse.Exception, Is.InstanceOf<ArgumentException>());
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Address_Should_Be_Locatable(bool found)
    {
        var venue = new VenueEntity();
            
        var vv = new VenueValidator(venue, (new() {Success = true, Found = found, StatusText = "OK", Exception = null, GeoLocation = new() {LocationType = GoogleGeo.LocationType.Approximate}}, []));
            
        var factResult = await vv.CheckAsync(VenueValidator.FactId.CanBeLocated, CancellationToken.None);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(factResult.Enabled, Is.True);
            Assert.That(factResult.Success, Is.EqualTo(found));
            Assert.That(factResult.Exception, Is.Null);
        }
    }

    [TestCase(GoogleGeo.LocationType.Approximate, false)]
    [TestCase(GoogleGeo.LocationType.GeometricCenter, false)]
    [TestCase(GoogleGeo.LocationType.RangeInterpolated, false)]
    [TestCase(GoogleGeo.LocationType.RoofTop, true)]
    public async Task Position_should_be_found(GoogleGeo.LocationType locationType, bool expected)
    {
        var venue = new VenueEntity();

        var vv = new VenueValidator(venue, (new() {Success = true, Found = true, GeoLocation = new() {LocationType = locationType}}, []));

        var factResult = await vv.CheckAsync(VenueValidator.FactId.LocationIsPrecise, CancellationToken.None);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(factResult.Enabled, Is.True);
            Assert.That(factResult.Success, Is.EqualTo(expected));
            Assert.That(factResult.Exception, Is.Null);
        }
    }

    [TestCase(GoogleGeo.LocationType.Approximate, true)]
    [TestCase(GoogleGeo.LocationType.GeometricCenter, true)]
    [TestCase(GoogleGeo.LocationType.RangeInterpolated, true)]
    [TestCase(GoogleGeo.LocationType.RoofTop, true)]
    public async Task No_Close_By_Other_Venue_Should_be_disabled(GoogleGeo.LocationType locationType, bool enabled)
    {
        var venue = new VenueEntity();

        var vv = new VenueValidator(venue, (new() {Success = true, Found = true, GeoLocation = new() {LocationType = locationType}}, []));

        var factResult = await vv.CheckAsync(VenueValidator.FactId.NotExistingGeoLocation, CancellationToken.None);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(enabled, Is.EqualTo(factResult.Enabled));
            Assert.That(factResult.Success, Is.EqualTo(enabled));
            Assert.That(factResult.Exception, Is.Null);
        }
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

        var vv = new VenueValidator(venue, (new() {Success = true, Found = true, GeoLocation = new() {LocationType = locationType}}, closeByVenues));

        var factResult = await vv.CheckAsync(VenueValidator.FactId.NotExistingGeoLocation, CancellationToken.None);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(factResult.Enabled, Is.True);
            Assert.That(factResult.Success, Is.EqualTo(success));
            if(!success) Assert.That(factResult.Message, Does.Contain((closeByVenues.Max(v => v.Distance)*1000).ToString(CultureInfo.InvariantCulture)));
            Assert.That(factResult.Exception, Is.Null);
        }
    }
}
