using System.Globalization;
using Axuno.TextTemplating;
using League.Emailing.TemplateModels;
using League.Models.ContactViewModels;
using League.Templates.Email;
using League.Templates.Email.Localization;
using League.Tests.TestComponents;
using League.TextTemplatingModule;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NUnit.Framework;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.MultiTenancy;

namespace League.Tests.TextTemplating;

/// <summary>
/// Tests for all email templates.
/// </summary>
/// <remarks>
/// Unit tests must cover ALL conditional expressions inside a template
/// </remarks>
[TestFixture]
public class EmailTemplateTests
{
    private readonly ServiceProvider _services;
    private readonly LeagueTemplateRenderer _renderer;
    private readonly ITenantContext _tenantContext;
    private readonly IStringLocalizer<EmailResource> _localizer;

    public EmailTemplateTests()
    {
        _tenantContext = new TenantContext
        {
            OrganizationContext =
            {
                Name = "League Long Name", ShortName = "Short Name",
                Bank = new BankDetails
                {
                    ShowBankDetailsInConfirmationEmail = true, Amount = (decimal) 12.34, Currency = "Euro",
                    BankName = "The Bank Name",
                    Recipient = "axuno gGmbH", Bic = "Bic-Number", Iban = "IBAN-Number"
                }
            }
        };
        _services = UnitTestHelpers.GetTextTemplatingServiceProvider(_tenantContext);
        _renderer = (LeagueTemplateRenderer) _services.GetRequiredService<ITemplateRenderer>();
        _localizer = _services.GetRequiredService<IStringLocalizer<EmailResource>>();
    }

    [OneTimeTearDown]
    public void DisposeObjects()
    {
        _services.Dispose();
    }

    private string L(string toTranslate, string cultureName)
    {
        using var cs = new CultureSwitcher(new CultureInfo(cultureName), new CultureInfo(cultureName));
        return _localizer[toTranslate];
    }

    [Test]
    public void LeagueAssembly_Should_Contain_Embedded_Email_Templates()
    {
        var resources = System.Reflection.Assembly.GetAssembly(typeof(League.LeagueStartup))?.GetManifestResourceNames();
        Assert.That(resources != null && resources.Any(r => r.ToString().Contains("League.Templates.Email")));
    }

    [Test]
    [TestCase("en")]
    [TestCase("de")]
    public void ConfirmNewPrimaryEmail_Test(string cultureName)
    {
        var m = new ChangeUserAccountModel
        {
            Email = "to@email.com", CallbackUrl = "https://axuno.net/callback",
            Deadline = new DateTime(2021, 01, 01, 12, 00, 00)
        };
            
        string text = string.Empty, html = string.Empty;
        Assert.Multiple( ()  =>
        {
            Assert.DoesNotThrowAsync(async () =>
            {
                text = await _renderer.RenderAsync(TemplateName.ConfirmNewPrimaryEmailTxt, m, cultureName);
                Console.WriteLine($"*** {TemplateName.ConfirmNewPrimaryEmailTxt} ***");
                Console.WriteLine(text);
                html = await _renderer.RenderAsync(TemplateName.ConfirmNewPrimaryEmailHtml, m, cultureName);
                Console.WriteLine($"*** {TemplateName.ConfirmNewPrimaryEmailHtml} ***");
                Console.WriteLine(html);
            } );

            Assert.That(text, Does.Contain(L("Change your primary email address", cultureName)));
            Assert.That(html, Does.Contain(L("Change your primary email address", cultureName)));
        });
    }
        
    [Test]
    [TestCase("en", true, false)]
    [TestCase("en", true, true)]
    [TestCase("en", false, false)]
    [TestCase("en", false, true)]
    [TestCase("de", true, false)]
    [TestCase("de", true, true)]
    [TestCase("de", false, false)]
    [TestCase("de", false, true)]
    public void ConfirmTeamApplication_Test(string cultureName, bool showBank, bool isRegisteringUser)
    {
        string text = string.Empty, html = string.Empty;
        Assert.Multiple( ()  =>
        {
            Assert.DoesNotThrowAsync(async () =>
            {
                _tenantContext.OrganizationContext.Bank.ShowBankDetailsInConfirmationEmail = showBank;
                text = await _renderer.RenderAsync(TemplateName.ConfirmTeamApplicationTxt,
                    new ConfirmTeamApplicationModel
                    {
                        IsNewApplication = true,
                        TournamentName = "Summer Saison",
                        RoundDescription = "Round description",
                        RoundTypeDescription = "6 ladies",
                        TeamName = "Team name",
                        RegisteredByName = "Registered-by-name",
                        RegisteredByEmail = "email@registered-by.net",
                        IsRegisteringUser = isRegisteringUser,
                        UrlToEditApplication = "https://volleyball-liga.de/augsburg",
                    }, cultureName
                );
                Console.WriteLine($"*** {TemplateName.ConfirmTeamApplicationTxt} ***");
                Console.WriteLine(text);
            });

            Assert.That(text, Does.Contain(L("Sporting greetings", cultureName)));
                    
            if(isRegisteringUser) Assert.That(text.Contains("Link", StringComparison.CurrentCultureIgnoreCase));
            if(isRegisteringUser && showBank) Assert.That(text.Contains("Bank", StringComparison.CurrentCultureIgnoreCase) && text.Contains(_tenantContext.OrganizationContext.Bank.Amount.ToString(CultureInfo.GetCultureInfo(cultureName))));
        });
    }
        
    [Test]
    [TestCase("en")]
    [TestCase("de")]
    public void ContactForm_Test(string cultureName)
    {
        var m = new { Form = new ContactViewModel
        {
            Gender = "m", FirstName = "John", LastName = "Specimen", Email = "my@email.com", PhoneNumber = "",
            Subject = "The subject", Message = "Something to tell..."
        }};

        Console.WriteLine();
            
        string text = string.Empty, html = string.Empty;
        Assert.Multiple( ()  =>
        {
            Assert.DoesNotThrowAsync(async () =>
            {
                text = await _renderer.RenderAsync(TemplateName.ContactFormTxt, m, cultureName);
                Console.WriteLine($"*** {TemplateName.ContactFormTxt} ***");
                Console.WriteLine(text);
            } );

            Assert.That(text, Does.Contain(L("Mr.", cultureName)));
        });
    }
        
    [Test]
    [TestCase("en", "2021-02-02 19:00:00", null)]
    [TestCase("en", "2021-02-02 19:00:00", 1234)]
    [TestCase("en", null, null)]
    [TestCase("en", null, 1234)]
    [TestCase("de", "2021-02-02 19:00:00", null)]
    [TestCase("de", "2021-02-02 19:00:00", 1234)]
    [TestCase("de", null, null)]
    [TestCase("de", null, 1234)]
    public void ChangeFixture_Test(string cultureName, DateTime? origPlannedStart, long? origVenue)
    {
        var m = new ChangeFixtureModel
        {
            Username = "Changed-by-name",
            Fixture = new TournamentManager.DAL.TypedViewClasses.PlannedMatchRow
            {
                Id = 9876, ChangeSerial = 2,
                RoundDescription = "Round descr.", HomeTeamNameForRound = "Home Team Name",
                GuestTeamNameForRound = "Guest Team Name", PlannedStart = new DateTime(2021, 03, 01, 19, 00, 00),
                OrigPlannedStart = origPlannedStart,
                OrigVenueId = origVenue, VenueName = "Venue", OrigVenueName = "Orig-Venue"
            }
        };
            
        string text = string.Empty, html = string.Empty;
        Assert.Multiple( ()  =>
        {
            Assert.DoesNotThrowAsync(async () =>
            {
                text = await _renderer.RenderAsync(TemplateName.ChangeFixtureTxt, m, cultureName);
                Console.WriteLine($"*** {TemplateName.ChangeFixtureTxt} ***");
                Console.WriteLine(text);
            } );

            Assert.That(text, Does.Contain(L("Season fixture date", cultureName)));
            if (origPlannedStart.HasValue) Assert.That(text, Does.Contain(L("Replacement fixture date", cultureName))); else Assert.That(text, Does.Not.Contain(L("Replacement fixture date", cultureName)));
            Assert.That(text, Does.Contain(L("Season fixture venue", cultureName)));
            if (origVenue.HasValue) Assert.That(text, Does.Contain(L("Replacement venue", cultureName))); else Assert.That(text, Does.Not.Contain(L("Replacement venue", cultureName)));
        });
    }
        
    [Test]
    [TestCase("en")]
    [TestCase("de")]
    public void NotifyCurrentPrimaryEmail_Test(string cultureName)
    {
        var m = new ChangeUserAccountModel
        {
            Email = "to@email.com", CallbackUrl = "https://axuno.net/callback",
            Deadline = new DateTime(2021, 01, 01, 12, 00, 00)
        };
            
        string text = string.Empty, html = string.Empty;
        Assert.Multiple( ()  =>
        {
            Assert.DoesNotThrowAsync(async () =>
            {
                text = await _renderer.RenderAsync(TemplateName.NotifyCurrentPrimaryEmailTxt, m, cultureName);
                Console.WriteLine($"*** {TemplateName.NotifyCurrentPrimaryEmailTxt} ***");
                Console.WriteLine(text);
                html = await _renderer.RenderAsync(TemplateName.NotifyCurrentPrimaryEmailHtml, m, cultureName);
                Console.WriteLine($"*** {TemplateName.NotifyCurrentPrimaryEmailTxt} ***");
                Console.WriteLine(html);
            } );

            Assert.That(text, Does.Contain(L("Your primary email is about to be changed to", cultureName)));
            Assert.That(html, Does.Contain(L("Your primary email is about to be changed to", cultureName)));
        });
    }        
        
    [Test]
    [TestCase("en")]
    [TestCase("de")]
    public void PasswordReset_Test(string cultureName)
    {
        var m = new ChangeUserAccountModel
        {
            Email = "to@email.com", CallbackUrl = "https://axuno.net/callback",
            Deadline = new DateTime(2021, 01, 01, 12, 00, 00)
        };
            
        string text = string.Empty, html = string.Empty;
            
        Assert.Multiple( ()  =>
        {

            Assert.DoesNotThrowAsync(async () =>
            {
                text = await _renderer.RenderAsync(TemplateName.PasswordResetTxt, m, cultureName);
                Console.WriteLine($"*** {TemplateName.PasswordResetTxt} ***");
                Console.WriteLine(text);
                html = await _renderer.RenderAsync(TemplateName.PasswordResetHtml, m, cultureName);
                Console.WriteLine($"*** {TemplateName.PasswordResetHtml} ***");
                Console.WriteLine(html);
            } );
            Assert.That(text, Does.Contain(L("Here is your password recovery code", cultureName)));
            Assert.That(html, Does.Contain(L("Here is your password recovery code", cultureName)));
        });
    }
        
    [TestCase("en")]
    [TestCase("de")]
    public void PleaseConfirmEmail_Test(string cultureName)
    {
        var m = new ChangeUserAccountModel
        {
            Email = "to@email.com", CallbackUrl = "https://axuno.net/callback",
            Deadline = new DateTime(2021, 01, 01, 12, 00, 00)
        };
            
        string text = string.Empty, html = string.Empty;
            
        Assert.Multiple( ()  =>
        {

            Assert.DoesNotThrowAsync(async () =>
            {
                text = await _renderer.RenderAsync(TemplateName.PleaseConfirmEmailTxt, m, cultureName);
                Console.WriteLine($"*** {TemplateName.PleaseConfirmEmailTxt} ***");
                Console.WriteLine(text);
                html = await _renderer.RenderAsync(TemplateName.PleaseConfirmEmailHtml, m, cultureName);
                Console.WriteLine($"*** {TemplateName.PleaseConfirmEmailHtml} ***");
                Console.WriteLine(html);
            } );

            Assert.That(text, Does.Contain(L("Thank you for creating an account", cultureName)));
            Assert.That(html, Does.Contain(L("Thank you for creating an account", cultureName)));
        });
    }
        
    [TestCase("en", false, true)]
    [TestCase("en", false, false)]
    [TestCase("en", true, true)]
    [TestCase("en", true, false)]
    [TestCase("de", false, true)]
    [TestCase("de", false, false)]
    [TestCase("de", true, true)]
    [TestCase("de", true, false)]
    public void ResultEntered_Test(string cultureName, bool isOrigStartSet, bool withRemarks)
    {
        var realStart = new DateTime(2021, 12, 24, 20, 00, 00);

        var m = new ResultEnteredModel
        {
            Username = "User who entered",
            RoundDescription = "Round description",
            HomeTeamName = "Home Team",
            GuestTeamName = "Guest Team",
            Match = new TournamentManager.DAL.EntityClasses.MatchEntity
            {
                Id = 12345,
                PlannedStart = new DateTime(2020, 12, 24, 20, 00, 00),
                OrigPlannedStart = isOrigStartSet ? realStart.AddDays(-60) : (DateTime?) null,
                RealStart = realStart,
                RealEnd = realStart.AddHours(2),
                HomePoints = 2, GuestPoints = 0,
                Remarks = withRemarks ? "Some remarks" : string.Empty,
                ChangeSerial = 7
            }
        };
        m.Match.Sets.AddRange(new []
        {
            new TournamentManager.DAL.EntityClasses.SetEntity {HomeBallPoints = 25, GuestBallPoints = 1, HomeSetPoints = 1, GuestSetPoints = 0},
            new TournamentManager.DAL.EntityClasses.SetEntity {HomeBallPoints = 25, GuestBallPoints = 2, HomeSetPoints = 1, GuestSetPoints = 0},
            new TournamentManager.DAL.EntityClasses.SetEntity {HomeBallPoints = 25, GuestBallPoints = 3, HomeSetPoints = 1, GuestSetPoints = 0}
        });
            
        string text = string.Empty, html = string.Empty;
            
        Assert.Multiple( ()  =>
        {
            Assert.DoesNotThrowAsync(async () =>
            {
                text = await _renderer.RenderAsync(TemplateName.ResultEnteredTxt, m, cultureName);
                Console.WriteLine($@"*** {TemplateName.ResultEnteredTxt} ***");
                Console.WriteLine(text);
            } );

            Assert.That(text, Does.Contain(L("Result", cultureName)));
            if (withRemarks) Assert.That(text, Does.Contain(m.Match.Remarks));
            if (isOrigStartSet) Assert.That(text, Does.Contain(m.Match.OrigPlannedStart?.ToString("d", new CultureInfo(cultureName))!));
        });
    }

    [TestCase("en", false, true)]
    [TestCase("en", false, false)]
    [TestCase("en", true, true)]
    [TestCase("en", true, false)]
    [TestCase("de", false, true)]
    [TestCase("de", false, false)]
    [TestCase("de", true, true)]
    [TestCase("de", true, false)]
    public void ResultRemoved_Test(string cultureName, bool isOrigStartSet, bool withRemarks)
    {
        var realStart = new DateTime(2023, 11, 30, 20, 00, 00);

        var m = new ResultEnteredModel
        {
            Username = "User who entered",
            RoundDescription = "Round description",
            HomeTeamName = "Home Team",
            GuestTeamName = "Guest Team",
            Match = new TournamentManager.DAL.EntityClasses.MatchEntity
            {
                Id = 12345,
                PlannedStart = new DateTime(2023, 11, 24, 20, 00, 00),
                OrigPlannedStart = isOrigStartSet ? realStart.AddDays(-60) : (DateTime?) null,
                RealStart = realStart,
                RealEnd = realStart.AddHours(2),
                HomePoints = 2, GuestPoints = 0,
                Remarks = withRemarks ? "Some remarks" : string.Empty,
                ChangeSerial = 7
            }
        };
        m.Match.Sets.AddRange(new[]
        {
            new TournamentManager.DAL.EntityClasses.SetEntity {HomeBallPoints = 25, GuestBallPoints = 1, HomeSetPoints = 1, GuestSetPoints = 0},
            new TournamentManager.DAL.EntityClasses.SetEntity {HomeBallPoints = 25, GuestBallPoints = 2, HomeSetPoints = 1, GuestSetPoints = 0},
            new TournamentManager.DAL.EntityClasses.SetEntity {HomeBallPoints = 25, GuestBallPoints = 3, HomeSetPoints = 1, GuestSetPoints = 0}
        });

        string text = string.Empty, html = string.Empty;

        Assert.Multiple(() =>
        {
            Assert.DoesNotThrowAsync(async () =>
            {
                text = await _renderer.RenderAsync(TemplateName.ResultRemovedTxt, m, cultureName);
                Console.WriteLine($@"*** {TemplateName.ResultRemovedTxt} ***");
                Console.WriteLine(text);
            });

            Assert.That(text, Does.Contain(L("Match result removed", cultureName)));
            if (withRemarks) Assert.That(text, Does.Contain(m.Match.Remarks));
            if (isOrigStartSet) Assert.That(text, Does.Contain(m.Match.OrigPlannedStart?.ToString("d", new CultureInfo(cultureName))!));
        });
    }

    [TestCase("en", true)]
    [TestCase("en", false)]
    [TestCase("de", true)]
    [TestCase("de", false)]
    public void AnnounceNextMatch_Test(string cultureName, bool venueIsSet)
    {
        var m = new AnnounceNextMatchModel
        {
            Fixture = new PlannedMatchRow
            {
                Id = 1000,
                PlannedStart = new DateTime(2021, 05, 05, 19, 00, 00),
                HomeTeamNameForRound = "HomeTeam",
                GuestTeamNameForRound = "GuestTeam"
            },
            Venue = new TournamentManager.DAL.EntityClasses.VenueEntity
            {
                Name = "Venue Name",
                Extension = "Venue Ext.",
                Street = "Venue Street",
                PostalCode = "PostalCode",
                City = "Venue City"
            }
        };

        m.IcsCalendarUrl = $"https://ics-calendar-url/{m.Fixture.Id}";
        if (!venueIsSet) m.Venue = null;
            
        string text = string.Empty, html = string.Empty;
            
        Assert.Multiple( ()  =>
        {
            Assert.DoesNotThrowAsync(async () =>
            {
                text = await _renderer.RenderAsync(TemplateName.AnnounceNextMatchTxt, m, cultureName);
                Console.WriteLine($"*** {TemplateName.AnnounceNextMatchTxt} ***");
                Console.WriteLine(text);
            } );

            Assert.That(text, Does.Contain(cultureName == "en" ? "Hello" : "Hallo"));
            Assert.That(text, Does.Contain(m.IcsCalendarUrl));
        });
    }

    [TestCase("en")]
    [TestCase("de")]

    public void RemindMatchResult_Test(string cultureName)
    {
        var m = new RemindMatchResultModel
        {
            Fixture = new PlannedMatchRow
            {
                Id = 1000,
                PlannedStart = new DateTime(2021, 05, 05, 19, 00, 00),
                HomeTeamNameForRound = "HomeTeam",
                GuestTeamNameForRound = "GuestTeam"
            }
        };

        string text = string.Empty, html = string.Empty;
            
        Assert.Multiple( ()  =>
        {
            Assert.DoesNotThrowAsync(async () =>
            {
                text = await _renderer.RenderAsync(TemplateName.RemindMatchResultTxt, m, cultureName);
                Console.WriteLine($"*** {TemplateName.RemindMatchResultTxt} ***");
                Console.WriteLine(text);
            } );

            Assert.That(text, Does.Contain(cultureName == "en" ? "Hello" : "Hallo"));
            Assert.That(text, Does.Contain(m.Fixture.HomeTeamNameForRound));
        });
    }
        
    [TestCase("en")]
    [TestCase("de")]

    public void UrgeMatchResult_Test(string cultureName)
    {
        var m = new UrgeMatchResultModel
        {
            Fixture = new PlannedMatchRow
            {
                Id = 1000,
                PlannedStart = new DateTime(2021, 05, 05, 19, 00, 00),
                HomeTeamNameForRound = "HomeTeam",
                GuestTeamNameForRound = "GuestTeam"
            }
        };
            
        string text = string.Empty, html = string.Empty;
            
        Assert.Multiple( ()  =>
        {
            Assert.DoesNotThrowAsync(async () =>
            {
                text = await _renderer.RenderAsync(TemplateName.UrgeMatchResultTxt, m, cultureName);
                Console.WriteLine($"*** {TemplateName.UrgeMatchResultTxt} ***");
                Console.WriteLine(text);
            } );

            Assert.That(text, Does.Contain(cultureName == "en" ? "Hello" : "Hallo"));
        });
    }
}
