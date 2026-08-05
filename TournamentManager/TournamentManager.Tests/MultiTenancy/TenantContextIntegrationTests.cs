using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using TournamentManager.JsonCSerializer;
using TournamentManager.MultiTenancy;

namespace TournamentManager.Tests.MultiTenancy;

[TestFixture]
public class TenantContextIntegrationTests
{
    [Test]
    public async Task SerializeToJsonC_AndLoadWithAspNetCoreConfiguration_RoundTripsCorrectly()
    {
        // Create a complete TenantContext with all nested properties
        var originalContext = new TenantContext
        {
            Identifier = "test-tenant",
            Guid = Guid.Parse("12345678-1234-1234-1234-123456789012"),
            IsDefault = true,
            SiteContext = new SiteContext
            {
                Position = 1,
                UrlSegmentValue = "test",
                FolderName = "TestFolder",
                IdentityCookieName = "TestCookie",
                SessionName = "TestSession",
                HideInMenu = false
            },
            OrganizationContext = new OrganizationContext
            {
                Name = "Test Organization",
                ShortName = "TestOrg",
                Description = "Test Description",
                HomepageUrl = "https://test.example.com"
            },
            DbContext = new DbContext
            {
                ConnectionKey = "TestConnection",
                Schema = "dbo",
                CommandTimeOut = 30
            },
            TournamentContext = new TournamentContext
            {
                ApplicationTournamentId = 123,
                ApplicationStart = new DateTimeOffset(2024, 6, 1, 12, 0, 0, TimeSpan.Zero),
                ApplicationEnd = new DateTimeOffset(2024, 12, 15, 12, 0, 0, TimeSpan.Zero),
                MapTournamentId = 456,
                TeamTournamentId = 789,
                MatchPlanTournamentId = 101,
                MatchResultTournamentId = 102,
                MaxDaysForResultCorrection = 7,
                FixtureRuleSet = new FixtureRuleSet
                {
                    PlannedMatchDateTimeMustBeSet = true,
                    CheckForExcludedMatchDateTime = true,
                    PlannedVenueMustBeSet = true,
                    PlannedDurationOfMatch = TimeSpan.FromHours(2),
                    RegularMatchStartTime = new RegularMatchStartTime
                    {
                        MinDayTime = new TimeOnly(18, 0, 0),
                        MaxDayTime = new TimeOnly(21, 0, 0)
                    }
                },
                TeamRuleSet = new TeamRules
                {
                    HomeMatchTime = new HomeMatchTime
                    {
                        IsEditable = true,
                        MustBeSet = true,
                        DaysOfWeekRange = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday],
                        ErrorIfNotInDaysOfWeekRange = true
                    },
                    HomeVenue = new HomeVenue
                    {
                        MustBeSet = true
                    }
                }
            }
        };

        var tempFile = Path.Combine(Path.GetTempPath(), $"tenant-test-{Guid.NewGuid()}.jsonc");

        try
        {
            // Serialize to JSONC file
            var options = new JsonCSerializerOptions
            {
                WriteRootComment = true,
                WriteRootAsNamedProperty = false, // expected by ConfigurationBuilder
                Indent = "  "
            };

            var jsonC = JsonCSerializer<TenantContext>.Serialize(originalContext, options);
            await File.WriteAllTextAsync(tempFile, jsonC, Encoding.UTF8);

            // Verify JSONC file was created with comments
            Assert.That(File.Exists(tempFile), Is.True);
            var jsoncContent = await File.ReadAllTextAsync(tempFile, Encoding.UTF8);
            Assert.That(jsoncContent, Does.Contain("//"), "JSONC should contain comments");

            // Load JSONC directly with ASP.NET Core Configuration (supports comments)
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(tempFile, optional: false, reloadOnChange: false)
                .Build();

            var loadedContext = configuration.Get<TenantContext>();

            // Assert - Use FluentAssertions for deep comparison
            loadedContext.Should().NotBeNull();
            loadedContext.Should().BeEquivalentTo(originalContext, o => o
                // exclude explicitly unserialized properties
                // that would cause a circular reference for TenantContext or DbContext
                .Excluding(ctx => ctx.SiteContext.Tenant)
                .Excluding(ctx => ctx.OrganizationContext.Tenant)
                .Excluding(ctx => ctx.TournamentContext.Tenant)
                .Excluding(ctx => ctx.DbContext.Tenant)
                .Excluding(ctx => ctx.DbContext.AppDb));
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}
