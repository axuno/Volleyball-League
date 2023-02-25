using League.WebApp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace League.Tests;

public class DemoAppFactory : WebApplicationFactory<Program>
{
    public DemoAppFactory()
    {
    }

    protected override IWebHostBuilder CreateWebHostBuilder()
    {
        var builder = base.CreateWebHostBuilder();
        return builder!;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);
        base.ConfigureWebHost(builder);

        builder
            .ConfigureServices(services =>
            {

            }).ConfigureAppConfiguration(configbuilder =>
            {
                
            });
    }
}
