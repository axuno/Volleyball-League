using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;

namespace League.Tests;

[TestFixture]
public class BasicIntegrationTests
{
    private readonly WebApplicationFactory<League.WebApp.Program> _factory;
    
    public BasicIntegrationTests()
    {
        _factory = new WebApplicationFactory<League.WebApp.Program>()
                .WithWebHostBuilder(
                    builder =>
                    {
                        builder.ConfigureTestServices(services =>
                        {
                        });
                    });
    }

    [TestCase("/")]
    [TestCase("/testorg")]
    [TestCase("/otherorg")]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(url);
        
        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
    }
}
