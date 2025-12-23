using System.Globalization;
using NUnit.Framework;

namespace Axuno.Tools.Tests.DateAndTime;

[TestFixture]
public class TimeZoneConverterTests
{
    private static Axuno.Tools.DateAndTime.TimeZoneConverter GetTimeZoneConverter(string culture)
    {
        var tzc = new Axuno.Tools.DateAndTime.TimeZoneConverter("Europe/Berlin", CultureInfo.GetCultureInfo(culture));
        return tzc;
    }

    [Test]
    public void UnknownTimeZoneId_ShouldThrow()
    {
        // Act, Assert
        Assert.That(() =>
        {
            _ = new Axuno.Tools.DateAndTime.TimeZoneConverter("unknown-time-zone", CultureInfo.CurrentCulture);
        }, Throws.Exception.TypeOf<TimeZoneNotFoundException>());
    }

    [Test]
    public void UseCurrentCulture_IfCultureIsMissing()
    {
        // Arrange
        var utcDateTime = new DateTime(2022, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var convertedDateTime = new Axuno.Tools.DateAndTime.TimeZoneConverter("Europe/Berlin").ToZonedTime(utcDateTime)!;

        // Assert
        Assert.That(convertedDateTime.CultureInfo.TwoLetterISOLanguageName, Is.EqualTo(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName));
    }

    [Test]
    public void ConvertUtcToTimeZoneStandard_ShouldReturnCorrectDateTime()
    {
        // Arrange
        var utcDateTime = new DateTime(2022, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var expectedDateTime = new DateTime(2022, 1, 1, 13, 0, 0, DateTimeKind.Local);
        
        // Act
        var convertedDateTime = GetTimeZoneConverter("de-DE").ToZonedTime(utcDateTime)!;

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(convertedDateTime.DateTimeOffset.DateTime, Is.EqualTo(expectedDateTime));
            Assert.That(convertedDateTime.TimeZoneId, Is.EqualTo("Europe/Berlin"));
            Assert.That(convertedDateTime.CultureInfo.TwoLetterISOLanguageName, Is.EqualTo("de"));
            Assert.That(convertedDateTime.GenericName, Is.EqualTo("Mitteleuropäische Zeit"));
            Assert.That(convertedDateTime.GenericAbbreviation, Is.EqualTo("MEZ"));
            Assert.That(convertedDateTime.DisplayName, Is.EqualTo("(UTC+01:00) Amsterdam, Berlin, Bern, Rom, Stockholm, Wien"));
            Assert.That(convertedDateTime.Name, Is.EqualTo("Mitteleuropäische Normalzeit"));
            Assert.That(convertedDateTime.Abbreviation, Is.EqualTo("MEZ"));
            Assert.That(convertedDateTime.IsDaylightSavingTime, Is.False);
            Assert.That(convertedDateTime.DateTimeOffset.Offset, Is.EqualTo(new TimeSpan(0, 1, 0, 0)));
            Assert.That(convertedDateTime.BaseUtcOffset, Is.EqualTo(new TimeSpan(0, 1, 0, 0)));
        }
    }

    [Test]
    public void ConvertUtcToTimeZoneDaylight_ShouldReturnCorrectDateTime()
    {
        // Arrange
        var utcDateTime = new DateTime(2022, 7, 1, 12, 0, 0, DateTimeKind.Utc);
        var expectedDateTime = new DateTime(2022, 7, 1, 14, 0, 0, DateTimeKind.Local);

        // Act
        var convertedDateTime = GetTimeZoneConverter("de-DE").ToZonedTime(utcDateTime)!;

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(convertedDateTime.DateTimeOffset.DateTime, Is.EqualTo(expectedDateTime));
            Assert.That(convertedDateTime.Name, Is.EqualTo("Mitteleuropäische Sommerzeit"));
            Assert.That(convertedDateTime.Abbreviation, Is.EqualTo("MESZ"));
            Assert.That(convertedDateTime.IsDaylightSavingTime, Is.True);
            Assert.That(convertedDateTime.DateTimeOffset.Offset, Is.EqualTo(new TimeSpan(0, 2, 0, 0)));
            Assert.That(convertedDateTime.BaseUtcOffset, Is.EqualTo(new TimeSpan(0, 1, 0, 0)));
        }
    }

    [Test]
    public void ConvertUnspecifiedKindToTimeZone_ShouldReturnCorrectDateTime()
    {
        // Arrange
        var unspecifiedDateTime = new DateTime(2022, 1, 1, 12, 0, 0, DateTimeKind.Unspecified);
        var expectedDateTime = new DateTime(2022, 1, 1, 13, 0, 0, DateTimeKind.Local);

        // Act
        var convertedDateTime = GetTimeZoneConverter("de-DE").ToZonedTime(unspecifiedDateTime)!;

        // Assert
        Assert.That(convertedDateTime.DateTimeOffset.DateTime, Is.EqualTo(expectedDateTime));
    }

    [Test]
    public void ConvertKindLocalToTimeZone_ShouldReturnCorrectDateTime()
    {
        // Arrange
        var localDateTime = new DateTime(2022, 1, 1, 12, 0, 0, DateTimeKind.Local);
        var expectedDateTime = new DateTime(2022, 1, 1, 12, 0, 0, DateTimeKind.Local);

        // Act
        var convertedDateTime = GetTimeZoneConverter("de-DE").ToZonedTime(localDateTime)!;

        // Assert
        Assert.That(convertedDateTime.DateTimeOffset.DateTime, Is.EqualTo(expectedDateTime));
    }

    [Test]
    public void ConvertTimeZoneToUtc_ShouldReturnCorrectDateTime()
    {
        // Arrange
        var localDateTime = new DateTime(2022, 1, 1, 8, 0, 0, DateTimeKind.Local);
        var expectedDateTime = new DateTime(2022, 1, 1, 7, 0, 0, DateTimeKind.Utc);

        // Act
        var convertedDateTime = GetTimeZoneConverter("en-US").ToUtc(localDateTime);

        // Assert
        Assert.That(convertedDateTime, Is.EqualTo(expectedDateTime));
    }

    [Test]
    public void ConvertTimeWithZoneIdToUtc_ShouldReturnCorrectDateTime()
    {
        // Arrange
        var localDateTime = new DateTime(2022, 1, 1, 8, 0, 0, DateTimeKind.Local);
        var expectedDateTime = new DateTime(2022, 1, 1, 7, 0, 0, DateTimeKind.Utc);

        // Act
        var convertedDateTime = Axuno.Tools.DateAndTime.TimeZoneConverter.ToUtc(localDateTime, "Europe/Berlin");

        // Assert
        Assert.That(convertedDateTime, Is.EqualTo(expectedDateTime));
    }

    [Test]
    public void ConvertUtcToTimeZone_ShouldReturnNull_WhenUtcDateTimeIsNull()
    {
        // Arrange
        DateTime? utcDateTime = null;

        // Act
        var convertedDateTime = GetTimeZoneConverter("de-DE").ToZonedTime(utcDateTime);

        // Assert
        Assert.That(convertedDateTime, Is.Null);
    }

    [Test]
    public void ConvertTimeZoneToUtc_ShouldReturnNull_WhenLocalDateTimeIsNull()
    {
        // Arrange
        DateTime? localDateTime = null;

        // Act
        var convertedDateTime = GetTimeZoneConverter("en-US").ToUtc(localDateTime);

        // Assert
        Assert.That(convertedDateTime, Is.Null);
    }

    [Test]
    public void ConvertUtcToTimeZone_ShouldReturnNull_WhenDateTimeOfAnyKindIsNull()
    {
        // Arrange
        DateTime? dateTimeOfAnyKind = null;

        // Act
        var convertedDateTime = GetTimeZoneConverter("en-US").ToZonedTime(dateTimeOfAnyKind);

        // Assert
        Assert.That(convertedDateTime, Is.Null);
    }

    [Test]
    public void ConvertTimeZoneToUtc_ShouldReturnNull_WhenDateTimeOfAnyKindIsNull()
    {
        // Arrange
        DateTime? dateTimeOfAnyKind = null;

        // Act
        var convertedDateTime = Axuno.Tools.DateAndTime.TimeZoneConverter.ToUtc(dateTimeOfAnyKind, "Europe/Berlin");

        // Assert
        Assert.That(convertedDateTime, Is.Null);
    }

    [Test]
    public void GetTimeZoneList_ShouldReturnTimeZoneList_WhenTimeZoneProviderIsNull()
    {
        // Act
        var timeZoneList = Axuno.Tools.DateAndTime.TimeZoneConverter.GetSystemTimeZoneList();

        // Assert
        Assert.That(timeZoneList, Is.InstanceOf<IReadOnlyCollection<string>>());
    }

    [Test]
    public void GetIanaTimeZoneList_ShouldReturnTimeZoneList_WhenTimeZoneProviderIsNull()
    {
        // Act
        var timeZoneList = Axuno.Tools.DateAndTime.TimeZoneConverter.GetIanaTimeZoneList();

        // Assert
        Assert.That(timeZoneList, Is.InstanceOf<IReadOnlyCollection<string>>());
    }

    [Test]
    public void CanMapToIanaTimeZone_ShouldReturnTrue_WhenTimeZoneInfoIsValid()
    {
        // Arrange
        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");

        // Act
        var canMap = Axuno.Tools.DateAndTime.TimeZoneConverter.CanMapToIanaTimeZone(timeZoneInfo);

        // Assert
        Assert.That(canMap, Is.True);
    }
}

