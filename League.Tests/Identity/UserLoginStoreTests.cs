using Microsoft.AspNetCore.Identity;
using System.Data;
using League.Identity;
using NUnit.Framework;
using TournamentManager.DAL.DatabaseSpecific;
using TournamentManager.DAL.EntityClasses;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.MultiTenancy;

namespace League.Tests.Identity;

/// <summary>
/// Integration tests
/// </summary>
[TestFixture]
public class UserLoginStoreTests
{
    private readonly UnitTestHelpers _uth = new();
    private readonly IAppDb _appDb;
    private readonly UserStore _store;
        
    public UserLoginStoreTests()
    {
        _appDb = _uth.GetTenantContext().DbContext.AppDb;
        _store = _uth.GetUserStore();
    }

    [OneTimeTearDown]
    public void DisposeObjects()
    {
        _store.Dispose();
    }

    private readonly ApplicationUser _testUser = new() {
        Email = "user@store.test",
        EmailConfirmed = true,
        EmailConfirmedOn = new DateTime(2019, 04, 25, 12, 00, 00),
        Email2 = "user2@store.test",
        Name = Guid.NewGuid().ToString("N"),
        PasswordHash = "PasswordHash",
        PhoneNumber = "0123 456789",
        PhoneNumberConfirmedOn = new DateTime(2019, 04, 24, 14, 00, 00),
        PhoneNumber2 = "0321 9876543",
        Gender = "f",
        FirstName = "Firstname",
        LastName = "Lastname",
        Nickname = "Nickname"
    };

    private ApplicationUser GetNewUser()
    {
        return new ApplicationUser
        {
            Name = _testUser.Name,
            UserName = _testUser.UserName,
            Email = _testUser.Email,
            EmailConfirmedOn = _testUser.EmailConfirmedOn,
            Email2 = _testUser.Email2,
            PasswordHash = _testUser.PasswordHash,
            PhoneNumber = _testUser.PhoneNumber,
            PhoneNumberConfirmedOn = _testUser.PhoneNumberConfirmedOn,
            PhoneNumber2 = _testUser.PhoneNumber2,
            Gender = _testUser.Gender,
            FirstName = _testUser.FirstName,
            LastName = _testUser.LastName,
            Nickname = _testUser.Nickname
        };
    }

    private static UserLoginInfo GetUserLoginInfo()
    {
        return new UserLoginInfo("provider", "providerKey", "displayName");
    }

    private async Task<ApplicationUser> CreateNewUser()
    {
        var user = GetNewUser();
        Assert.That(await _store.CreateAsync(user, CancellationToken.None), Is.EqualTo(IdentityResult.Success));
        return user;
    }

    [Test]
    public async Task Add_and_Remove_UserLoginInfo()
    {
        var user = await CreateNewUser();
        var login = GetUserLoginInfo();
        Assert.DoesNotThrowAsync(() => _store.AddLoginAsync(user, login, CancellationToken.None));

        var userByLogin = await _store.FindByLoginAsync(login.LoginProvider, login.ProviderKey, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(userByLogin, Is.Not.Null);
            Assert.That(user.Id, Is.EqualTo(userByLogin.Id));
        });

        var login2 = new UserLoginInfo("provider2", "providerKey2", "displayName2");
        Assert.DoesNotThrowAsync(() => _store.AddLoginAsync(user, login2, CancellationToken.None));

        var logins = await _store.GetLoginsAsync(user, CancellationToken.None);
        Assert.That(logins, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(logins.First(l => l.LoginProvider == login.LoginProvider).ProviderKey, Is.EqualTo(login.ProviderKey));
            Assert.That(logins.First(l => l.LoginProvider == login.LoginProvider).ProviderDisplayName, Is.EqualTo(login.ProviderDisplayName));
            Assert.That(logins.First(l => l.LoginProvider == login2.LoginProvider).ProviderKey, Is.EqualTo(login2.ProviderKey));
            Assert.That(logins.First(l => l.LoginProvider == login2.LoginProvider).ProviderDisplayName, Is.EqualTo(login2.ProviderDisplayName));
        });

        Assert.DoesNotThrowAsync(() => _store.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey, CancellationToken.None));
        userByLogin = await _store.FindByLoginAsync(login.LoginProvider, login.ProviderKey, CancellationToken.None);
        Assert.That(userByLogin, Is.Null);
    }

    [Test]
    public async Task ShouldThrow_WritingTo_Database()
    {
        // Creating the user should fail because of a table lock
        var currentTimeOut = _appDb.DbContext.CommandTimeOut;
        using (var da = (DataAccessAdapter)_appDb.DbContext.GetNewAdapter())
        {
            //await da.ExecuteSQLAsync(CancellationToken.None, "BEGIN TRAN SELECT 1 FROM [testorg].[User] WITH (TABLOCKX) WAITFOR DELAY '00:00:05' ROLLBACK TRAN");
            await da.StartTransactionAsync(IsolationLevel.Serializable, "transaction1");
            da.SaveTransaction("transaction1");
            // lock the table for completely
            await da.ExecuteSQLAsync(CancellationToken.None,
                $"SELECT 1 FROM [{_appDb.DbContext.Schema}].[{da.GetPersistentTableName(new IdentityUserLoginEntity())}] WITH (TABLOCKX)");
            // Trying to update will fail because the table is locked
            _appDb.DbContext.CommandTimeOut = 2;
            Assert.ThrowsAsync<ORMQueryExecutionException>(async () => await _store.AddLoginAsync(GetNewUser(), GetUserLoginInfo() , CancellationToken.None));
            Assert.ThrowsAsync<ORMQueryExecutionException>(async () => await _store.RemoveLoginAsync(GetNewUser(), "x", "y", CancellationToken.None));
            da.Rollback("transaction1");
        }
        _appDb.DbContext.CommandTimeOut = currentTimeOut;
    }
        

    [SetUp]
    public async Task Setup()
    {
        // delete all user rows - will delete user logins and user tokens as well
        await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<UserEntity>(new PredicateExpression(), CancellationToken.None);
    }

    [TearDown]
    public async Task Cleanup()
    {
        // delete all user rows - will delete user logins and user tokens as well
        await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<UserEntity>(new PredicateExpression(), CancellationToken.None);
    }
}
