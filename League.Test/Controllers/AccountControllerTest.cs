using System.Threading.Tasks;
using League.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;

namespace League.Test.Controllers;

[Ignore("Test does not work for Razor Class Library")]
[TestFixture]
public class AccountControllerTest : ControllerTestBase
{
    private readonly IStringLocalizer<Account> _mockLocalizer;
    private readonly Account _controller;

    public AccountControllerTest()
    {
        var mockLocalizer = new Mock<IStringLocalizer<Account>>();
        var key = "Please confirm your email address";
        var localizedString = new LocalizedString(key, "Bitte die E-Mail-Adresse bestätigen");
        mockLocalizer.Setup(_ => _[key]).Returns(localizedString);
        _mockLocalizer = mockLocalizer.Object;

        _controller = new League.Controllers.Account(null, _mockLocalizer, null, null, null, null, null, null, null, null)
        {
            ControllerContext = new ControllerContext {HttpContext = new DefaultHttpContext()}
        };
    }

    [Test]
    public void SignIn()
    {
        var actionResult = _controller.SignIn();
        Assert.IsNotNull(actionResult);
    }

    [Test]
    public void Localization()
    {
        // https://stackoverflow.com/questions/43460880/asp-net-core-testing-controller-with-istringlocalizer
        var result = _mockLocalizer["Please confirm your email address"].Value;
        Assert.AreEqual("Bitte die E-Mail-Adresse bestätigen", result);
    }

    [Test]
    public void Index()
    {
        _controller.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";
        var result = _controller.SignIn();
        Assert.IsNotNull(result);
    }

    [Test]
    public async Task Index_From_Server()
    {
        var response = await Client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("type=\"image/png\""));
    }
}