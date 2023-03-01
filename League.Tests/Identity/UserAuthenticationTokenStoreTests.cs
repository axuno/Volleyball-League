using Microsoft.AspNetCore.Identity;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Threading;
using League.Identity;
using NUnit.Framework;
using TournamentManager.DAL.DatabaseSpecific;
using TournamentManager.DAL.EntityClasses;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.MultiTenancy;

namespace League.Test.Identity;

/// <summary>
/// Integration tests
/// </summary>
[TestFixture]
public class UserAuthenticationTokenStoreTests
{
    private readonly UnitTestHelpers _uth = new();
    private readonly AppDb _appDb;
    private readonly UserStore _store;

    public UserAuthenticationTokenStoreTests()
    {
        _appDb = _uth.GetTenantContext().DbContext.AppDb;
        _store = _uth.GetUserStore();
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

    private async Task<ApplicationUser> CreateNewUser()
    {
        var user = GetNewUser();
        Assert.AreEqual(IdentityResult.Success, await _store.CreateAsync(user, CancellationToken.None));
        return user;
    }

    private static UserLoginInfo GetUserLoginInfo()
    {
        return new UserLoginInfo("loginProvider", "providerKey", "displayName");
    }

    [Test]
    public async Task Set_Get_and_Remove_AuthToken()
    {
        var user = await CreateNewUser();
        var userLoginInfo = GetUserLoginInfo();
        Assert.DoesNotThrowAsync(() => _store.AddLoginAsync(user, userLoginInfo, CancellationToken.None));

        Assert.DoesNotThrowAsync(() => _store.SetTokenAsync(user, userLoginInfo.LoginProvider, "TokenName", "TheValue",  CancellationToken.None));
        Assert.AreEqual("TheValue", await _store.GetTokenAsync(user, userLoginInfo.LoginProvider, "TokenName", CancellationToken.None));

        Assert.DoesNotThrowAsync(() => _store.RemoveTokenAsync(user, userLoginInfo.LoginProvider, "TokenName", CancellationToken.None));
        Assert.IsNull(await _store.GetTokenAsync(user, userLoginInfo.LoginProvider, "TokenName", CancellationToken.None));
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
            Assert.ThrowsAsync<ORMQueryExecutionException>(async () => await _store.SetTokenAsync(GetNewUser(), "Provider", "TokenName", "TheValue", CancellationToken.None));
            Assert.ThrowsAsync<Exception>(async () => await _store.RemoveTokenAsync(GetNewUser(), "Provider", "TokenName", CancellationToken.None));
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
