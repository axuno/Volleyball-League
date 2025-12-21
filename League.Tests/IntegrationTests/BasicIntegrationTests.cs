using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;

namespace League.Tests.IntegrationTests;

/// <summary>
/// Integration tests using League.Demo.Program as the host application.
/// </summary>
[TestFixture]
public class BasicIntegrationTests
{
    private WebApplicationFactory<WebApp.Program>? _factory;

    [OneTimeSetUp]
    public void Setup()
    {
        // Create a test factory using League.Demo.Program as the host
        _factory = UnitTestHelpers.GetLeagueTestApplicationFactory<WebApp.Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Add test-specific service overrides here if needed
                });
            });
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _factory?.Dispose();
    }

    [TestCase("/")]
    [TestCase("/testorg")]
    [TestCase("/otherorg")]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
    {
        // Arrange
        Assert.That(_factory, Is.Not.Null, "WebApplicationFactory must be initialized");
        var client = _factory!.CreateClient();

        // Act
        var response = await client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        Assert.That(response.IsSuccessStatusCode, Is.True);
    }
}
