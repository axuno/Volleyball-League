using Microsoft.AspNetCore.Identity;
using System.Data;
using League.Identity;
using NUnit.Framework;
using TournamentManager.DAL.DatabaseSpecific;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.MultiTenancy;
using SD.LLBLGen.Pro.ORMSupportClasses;

namespace League.Tests.Identity;

/// <summary>
/// Integration tests
/// </summary>
[TestFixture]
public class UserStoreTests
{
    private readonly UnitTestHelpers _uth = new();
    private readonly IAppDb _appDb;
    private readonly UserStore _store;
    private readonly RoleStore _roleStore;

    public UserStoreTests()
    {
        _appDb = _uth.GetTenantContext().DbContext.AppDb;
        _store = _uth.GetUserStore();
        _roleStore = _uth.GetRoleStore();
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

    [SetUp]
    public async Task Setup()
    {
        // delete all user rows
        await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<UserEntity>(new PredicateExpression(), CancellationToken.None);
        // delete all role rows
        await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<IdentityRoleEntity>(new PredicateExpression(), CancellationToken.None);
    }

    [TearDown]
    public async Task Cleanup()
    {
        // delete all user rows
        await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<UserEntity>(new PredicateExpression(), CancellationToken.None);
        // delete all role rows
        await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<IdentityRoleEntity>(new PredicateExpression(), CancellationToken.None);
    }

    [Test]
    public void ApplicationUser()
    {
        var user = GetNewUser();
        user.CompleteName = user.FirstName + " " + user.LastName;
        Assert.AreEqual(user.CompleteName, user.FirstName + " " + user.LastName);
        user.IsAuthenticated = true;
        Assert.IsTrue(user.IsAuthenticated);
        user.AuthenticationType = "Test";
        Assert.AreEqual("Test", user.AuthenticationType);
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
                $"SELECT 1 FROM [{_appDb.DbContext.Schema}].[{da.GetPersistentTableName(new UserEntity())}] WITH (TABLOCKX)");
            // Trying to update will fail because the table is locked
            _appDb.DbContext.CommandTimeOut = 2;
            Assert.AreNotEqual(IdentityResult.Success, await _store.CreateAsync(new ApplicationUser { Email = "any@email.test" }, CancellationToken.None));
            Assert.AreNotEqual(IdentityResult.Success, await _store.UpdateAsync(new ApplicationUser { Email = "any@email.test" }, CancellationToken.None));
            Assert.AreNotEqual(IdentityResult.Success, await _store.DeleteAsync(new ApplicationUser { Email = "any@email.test" }, CancellationToken.None));
            da.Rollback("transaction1");
        }
        _appDb.DbContext.CommandTimeOut = currentTimeOut;
    }

    [Test]
    public async Task Create_User()
    {
        var user = GetNewUser();
        Assert.AreEqual(IdentityResult.Success, await _store.CreateAsync(user, CancellationToken.None));

        user = await _store.FindByNameAsync(_testUser.UserName, CancellationToken.None);
        Assert.AreNotEqual(0, user.Id);
        Assert.AreEqual(user.UserName, _testUser.Name);
        Assert.AreEqual(user.Name, _testUser.UserName);
        Assert.AreEqual(user.Email, _testUser.Email);
        Assert.AreEqual(user.EmailConfirmed, _testUser.EmailConfirmed);
        Assert.AreEqual(user.EmailConfirmedOn, _testUser.EmailConfirmedOn);
        Assert.AreEqual(user.Email2, _testUser.Email2);
        Assert.AreEqual(user.PasswordHash, _testUser.PasswordHash);
        Assert.AreEqual(user.PhoneNumber, _testUser.PhoneNumber);
        Assert.AreEqual(user.PhoneNumberConfirmed, _testUser.PhoneNumberConfirmed);
        Assert.AreEqual(user.PhoneNumberConfirmedOn, _testUser.PhoneNumberConfirmedOn);
        Assert.AreEqual(user.PhoneNumber2, _testUser.PhoneNumber2);
        Assert.AreEqual(user.Gender, _testUser.Gender);
        Assert.AreEqual(user.FirstName, _testUser.FirstName);
        Assert.AreEqual(user.LastName, _testUser.LastName);
        Assert.AreEqual(user.Nickname, _testUser.Nickname);
        Assert.AreEqual(DateTime.UtcNow.Date, user.ModifiedOn.Date);

        // trying to create a user with same email again should fail
        user = GetNewUser();
        Assert.AreNotEqual(IdentityResult.Success, await _store.CreateAsync(user, CancellationToken.None));

        // create another user only with email
        var anotherUserMail = "another@email.test";
        user = new ApplicationUser {Email = anotherUserMail};
        Assert.AreEqual(IdentityResult.Success, await _store.CreateAsync(user, CancellationToken.None));

        // Empty username should be replaced with Guid
        user = await _store.FindByEmailAsync(anotherUserMail, CancellationToken.None);
        Assert.IsTrue(Guid.TryParse(user.UserName, out var guidUsername));

        // trying to create a user with the same username again should fail
        user.Email = "onemore." + anotherUserMail;
        Assert.AreNotEqual(IdentityResult.Success, await _store.CreateAsync(user, CancellationToken.None));


    }

    [Test]
    public async Task Update_User()
    {
        // prepare for the update
        var user = GetNewUser();
        await _store.CreateAsync(user, CancellationToken.None);
        user = await _store.FindByEmailAsync(_testUser.Email, CancellationToken.None);
        Assert.IsNotNull(user);

        var changedLastName = "Changed";
        user.LastName = changedLastName;
        var result = await _store.UpdateAsync(user, CancellationToken.None);
        Assert.IsTrue(result == IdentityResult.Success);
        user = await _store.FindByEmailAsync(_testUser.Email, CancellationToken.None);
        Assert.AreEqual(user.LastName, changedLastName);
    }

    [Test]
    public async Task Delete_User()
    {
        // prepare for the deletion
        var user = GetNewUser();
        await _store.CreateAsync(user, CancellationToken.None);
        user = await _store.FindByEmailAsync(_testUser.Email, CancellationToken.None);
        Assert.IsNotNull(user);

        var result = await _store.DeleteAsync(user, CancellationToken.None);
        Assert.IsTrue(result == IdentityResult.Success);
        Assert.IsNull(await _store.FindByEmailAsync(_testUser.Email, CancellationToken.None));
    }

    [Test]
    public async Task FindUser()
    {
        var user = GetNewUser();

        var result = await _store.CreateAsync(user, CancellationToken.None);
        Assert.IsTrue(result == IdentityResult.Success);
        Assert.AreEqual(user.UserName, (await _store.FindByIdAsync(user.Id.ToString(), CancellationToken.None)).UserName);

        user = await _store.FindByEmailAsync(_testUser.Email, CancellationToken.None);
        Assert.AreEqual(_testUser.UserName, user.UserName);
        Assert.ThrowsAsync<ArgumentNullException>(() => _store.FindByEmailAsync(null, CancellationToken.None));

        user = await _store.FindByNameAsync(_testUser.UserName, CancellationToken.None);
        Assert.AreEqual(_testUser.Email, user.Email);
        Assert.ThrowsAsync<ArgumentNullException>(() => _store.FindByNameAsync(null, CancellationToken.None));
        Assert.IsNull(await _store.FindByNameAsync("does-not-exist", CancellationToken.None));
        Assert.IsNull(await _store.FindByIdAsync("0", CancellationToken.None));
        Assert.IsNull(await _store.FindByIdAsync("a", CancellationToken.None));

        Assert.AreEqual(user.Id.ToString(), await _store.GetUserIdAsync(user, CancellationToken.None));
        Assert.AreEqual(user.UserName, await _store.GetUserNameAsync(user, CancellationToken.None));
        Assert.AreEqual(user.UserName.ToUpperInvariant(), await _store.GetNormalizedUserNameAsync(user, CancellationToken.None));

        Assert.AreEqual(user.Email.ToUpperInvariant(), await _store.GetNormalizedEmailAsync(user, CancellationToken.None));
 }

    [Test]
    public async Task SetUserName()
    {
        var user = GetNewUser();

        var newUserName = "abcdefg";
        var securityStamp = await _store.GetSecurityStampAsync(user, CancellationToken.None);
        await _store.SetUserNameAsync(user, newUserName, CancellationToken.None);
        Assert.AreNotEqual(securityStamp, await _store.GetSecurityStampAsync(user, CancellationToken.None));
        Assert.AreEqual(newUserName, await _store.GetUserNameAsync(user, CancellationToken.None));
        Assert.AreEqual(newUserName, user.UserName);
        Assert.AreEqual(newUserName.ToUpperInvariant(), await _store.GetNormalizedUserNameAsync(user, CancellationToken.None));
    }

    [Test]
    public async Task UserEmailStore()
    {
        var user = GetNewUser();
        user.EmailConfirmed = false;
        user.EmailConfirmedOn = null;

        var newEmail = "userstore@changetest.org";

        var securityStamp = await _store.GetSecurityStampAsync(user, CancellationToken.None);
        await _store.SetEmailAsync(user, newEmail, CancellationToken.None);
        Assert.AreNotEqual(securityStamp, await _store.GetSecurityStampAsync(user, CancellationToken.None));
        Assert.AreEqual(newEmail, await _store.GetEmailAsync(user, CancellationToken.None));
        Assert.AreEqual(newEmail, user.Email);
        Assert.AreEqual(newEmail.ToUpperInvariant(), await _store.GetNormalizedEmailAsync(user, CancellationToken.None));

        await _store.SetEmailConfirmedAsync(user, true, CancellationToken.None);
        Assert.AreEqual(true, user.EmailConfirmed);
        Assert.AreEqual(true, await _store.GetEmailConfirmedAsync(user, CancellationToken.None));
        Assert.IsNotNull(user.EmailConfirmedOn);
    }

    [Test]
    public async Task UserPhoneNumberStore()
    {
        var user = GetNewUser();
        user.PhoneNumberConfirmed = false;
        user.PhoneNumberConfirmedOn = null;

        var newPhone = "98761234";
        var securityStamp = await _store.GetSecurityStampAsync(user, CancellationToken.None);

        await _store.SetPhoneNumberAsync(user, string.Empty, CancellationToken.None);
        Assert.AreEqual(string.Empty, user.PhoneNumber);

        await _store.SetPhoneNumberAsync(user, newPhone, CancellationToken.None);
        Assert.AreNotEqual(securityStamp, await _store.GetSecurityStampAsync(user, CancellationToken.None));
        Assert.AreEqual(newPhone, await _store.GetPhoneNumberAsync(user, CancellationToken.None));

        Assert.AreEqual(user.PhoneNumberConfirmed, await _store.GetPhoneNumberConfirmedAsync(user, CancellationToken.None));
        await _store.SetPhoneNumberConfirmedAsync(user, true, CancellationToken.None);
        Assert.AreEqual(true, user.PhoneNumberConfirmed);
        Assert.AreEqual(true, await _store.GetPhoneNumberConfirmedAsync(user, CancellationToken.None));
        Assert.IsNotNull(user.PhoneNumberConfirmedOn);

        // Empty phone number should not be set confirmed
        user.PhoneNumber = string.Empty;
        user.PhoneNumberConfirmed = true;
        await _store.SetPhoneNumberConfirmedAsync(user, true, CancellationToken.None);
        Assert.IsFalse(user.PhoneNumberConfirmed);
    }

    [Test]
    public async Task UserPasswordStore()
    {
        var user = GetNewUser();
        user.PasswordHash = string.Empty;

        Assert.IsNull(await _store.GetPasswordHashAsync(user, CancellationToken.None));
        var newPwHash = "passwordhash";
        var securityStamp = await _store.GetSecurityStampAsync(user, CancellationToken.None);
        await _store.SetPasswordHashAsync(user, newPwHash, CancellationToken.None);
        Assert.AreNotEqual(securityStamp, await _store.GetSecurityStampAsync(user, CancellationToken.None));
        Assert.AreEqual(newPwHash, await _store.GetPasswordHashAsync(user, CancellationToken.None));
        Assert.IsTrue(await _store.HasPasswordAsync(user, CancellationToken.None));
    }

    [Test]
    public async Task UserLockoutStore()
    {
        var user = GetNewUser();
        var nonExistentUser = new ApplicationUser {Id = 0};

        Assert.AreEqual(IdentityResult.Success, await _store.CreateAsync(user, CancellationToken.None));
        Assert.DoesNotThrowAsync(() => _store.SetLockoutEnabledAsync(user, true, CancellationToken.None));

        // users for except SystemManagers lock-out should be enabled
        Assert.IsTrue(await _store.GetLockoutEnabledAsync(user, CancellationToken.None));

        // not set before, so it should be null
        Assert.IsNull(await _store.GetLockoutEndDateAsync(user, CancellationToken.None));
        var lockoutEndDate = new DateTimeOffset?(new DateTime(2020, 05, 30, 14, 15, 16));

        // Test setting lockout end date
        await _store.SetLockoutEndDateAsync(user, lockoutEndDate, CancellationToken.None);
        Assert.AreEqual(lockoutEndDate.Value.UtcDateTime, (await _store.GetLockoutEndDateAsync(user, CancellationToken.None)).Value.DateTime);

        // Nothing should happen for non-existent users
        Assert.ThrowsAsync<ArgumentException>(() => _store.SetLockoutEndDateAsync(nonExistentUser, lockoutEndDate, CancellationToken.None));
        Assert.DoesNotThrowAsync(() => _store.GetLockoutEndDateAsync(nonExistentUser, CancellationToken.None));

        // Test access failures
        Assert.AreEqual(0, await _store.GetAccessFailedCountAsync(user, CancellationToken.None));
        Assert.AreEqual(1, await _store.IncrementAccessFailedCountAsync(user, CancellationToken.None));
        Assert.AreEqual(2, await _store.IncrementAccessFailedCountAsync(user, CancellationToken.None));
        Assert.ThrowsAsync<ArgumentException>(() => _store.IncrementAccessFailedCountAsync(nonExistentUser, CancellationToken.None));

        // Test after resetting failed count and lockout date
        await _store.ResetAccessFailedCountAsync(user, CancellationToken.None);
        await _store.SetLockoutEndDateAsync(user, null, CancellationToken.None);
        Assert.AreEqual(0, await _store.GetAccessFailedCountAsync(user, CancellationToken.None));
        Assert.DoesNotThrowAsync(() => _store.ResetAccessFailedCountAsync(nonExistentUser, CancellationToken.None));
        Assert.AreEqual(0, await _store.GetAccessFailedCountAsync(nonExistentUser, CancellationToken.None));

        // Tests for user as SystemManager
        var role = new ApplicationRole { Name = Constants.RoleName.SystemManager };
        Assert.AreEqual(IdentityResult.Success, await _roleStore.CreateAsync(role, CancellationToken.None));
            
        // SystemManagers should not be locked out
        await _store.AddToRoleAsync(user, Constants.RoleName.SystemManager, CancellationToken.None);
        Assert.IsFalse(await _store.GetLockoutEnabledAsync(user, CancellationToken.None));

        // Test setting lockout end date
        await _store.SetLockoutEndDateAsync(user, lockoutEndDate, CancellationToken.None);
        Assert.AreEqual(null, await _store.GetLockoutEndDateAsync(user, CancellationToken.None));
        Assert.AreEqual(0, await _store.IncrementAccessFailedCountAsync(user, CancellationToken.None));
        Assert.AreEqual(0, await _store.IncrementAccessFailedCountAsync(user, CancellationToken.None));
    }

    [Test]
    public void DisposeTest()
    {
        Assert.DoesNotThrow(() =>_store.Dispose());
    }
}
