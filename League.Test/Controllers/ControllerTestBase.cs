using System;
using System.Net.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace League.Test.Controllers;

[TestFixture]
public class ControllerTestBase : IDisposable
{
    #region ** Initialize test environment **

    // see: https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/testing?view=aspnetcore-2.1 and
    // https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-nunit?view=aspnetcore-2.1
    public ControllerTestBase()
    {
        var appFactory = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // can now add/remove additional services
            });
        });
        Client = appFactory.CreateClient(new WebApplicationFactoryClientOptions()
            {BaseAddress = new Uri("http://localhost")});

        Server = appFactory.Server;
    }

    protected TestServer Server { get; }
    protected HttpClient Client { get; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Client.Dispose();
        Server.Dispose();
    }

    #endregion
}