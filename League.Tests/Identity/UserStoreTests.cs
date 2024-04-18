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

    [OneTimeTearDown]
    public void DisposeObjects()
    {
        _store.Dispose();
        _roleStore.Dispose();
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
        Assert.That(user.FirstName + " " + user.LastName, Is.EqualTo(user.CompleteName));
        user.IsAuthenticated = true;
        Assert.That(user.IsAuthenticated, Is.True);
        user.AuthenticationType = "Test";
        Assert.That(user.AuthenticationType, Is.EqualTo("Test"));
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
            Assert.Multiple(async () =>
            {
                Assert.That(await _store.CreateAsync(new ApplicationUser { Email = "any@email.test" }, CancellationToken.None), Is.Not.EqualTo(IdentityResult.Success));
                Assert.That(await _store.UpdateAsync(new ApplicationUser { Email = "any@email.test" }, CancellationToken.None), Is.Not.EqualTo(IdentityResult.Success));
                Assert.That(await _store.DeleteAsync(new ApplicationUser { Email = "any@email.test" }, CancellationToken.None), Is.Not.EqualTo(IdentityResult.Success));
            });
            da.Rollback("transaction1");
        }
        _appDb.DbContext.CommandTimeOut = currentTimeOut;
    }

    [Test]
    public async Task Create_User()
    {
        var user = GetNewUser();
        Assert.That(await _store.CreateAsync(user, CancellationToken.None), Is.EqualTo(IdentityResult.Success));

        user = await _store.FindByNameAsync(_testUser.UserName, CancellationToken.None);
        Assert.That(user.Id, Is.Not.EqualTo(0));
        Assert.Multiple(() =>
        {
            Assert.That(_testUser.Name, Is.EqualTo(user.UserName));
            Assert.That(_testUser.UserName, Is.EqualTo(user.Name));
            Assert.That(_testUser.Email, Is.EqualTo(user.Email));
            Assert.That(_testUser.EmailConfirmed, Is.EqualTo(user.EmailConfirmed));
            Assert.That(_testUser.EmailConfirmedOn, Is.EqualTo(user.EmailConfirmedOn));
            Assert.That(_testUser.Email2, Is.EqualTo(user.Email2));
            Assert.That(_testUser.PasswordHash, Is.EqualTo(user.PasswordHash));
            Assert.That(_testUser.PhoneNumber, Is.EqualTo(user.PhoneNumber));
            Assert.That(_testUser.PhoneNumberConfirmed, Is.EqualTo(user.PhoneNumberConfirmed));
            Assert.That(_testUser.PhoneNumberConfirmedOn, Is.EqualTo(user.PhoneNumberConfirmedOn));
            Assert.That(_testUser.PhoneNumber2, Is.EqualTo(user.PhoneNumber2));
            Assert.That(_testUser.Gender, Is.EqualTo(user.Gender));
            Assert.That(_testUser.FirstName, Is.EqualTo(user.FirstName));
            Assert.That(_testUser.LastName, Is.EqualTo(user.LastName));
            Assert.That(_testUser.Nickname, Is.EqualTo(user.Nickname));
            Assert.That(user.ModifiedOn.Date, Is.EqualTo(DateTime.UtcNow.Date));
        });

        // trying to create a user with same email again should fail
        user = GetNewUser();
        Assert.That(await _store.CreateAsync(user, CancellationToken.None), Is.Not.EqualTo(IdentityResult.Success));

        // create another user only with email
        var anotherUserMail = "another@email.test";
        user = new ApplicationUser {Email = anotherUserMail};
        Assert.That(await _store.CreateAsync(user, CancellationToken.None), Is.EqualTo(IdentityResult.Success));

        // Empty username should be replaced with Guid
        user = await _store.FindByEmailAsync(anotherUserMail, CancellationToken.None);
        Assert.That(Guid.TryParse(user.UserName, out var guidUsername), Is.True);

        // trying to create a user with the same username again should fail
        user.Email = "onemore." + anotherUserMail;
        Assert.That(await _store.CreateAsync(user, CancellationToken.None), Is.Not.EqualTo(IdentityResult.Success));


    }

    [Test]
    public async Task Update_User()
    {
        // prepare for the update
        var user = GetNewUser();
        await _store.CreateAsync(user, CancellationToken.None);
        user = await _store.FindByEmailAsync(_testUser.Email, CancellationToken.None);
        Assert.That(user, Is.Not.Null);

        var changedLastName = "Changed";
        user.LastName = changedLastName;
        var result = await _store.UpdateAsync(user, CancellationToken.None);
        Assert.That(result, Is.EqualTo(IdentityResult.Success));
        user = await _store.FindByEmailAsync(_testUser.Email, CancellationToken.None);
        Assert.That(changedLastName, Is.EqualTo(user.LastName));
    }

    [Test]
    public async Task Delete_User()
    {
        // prepare for the deletion
        var user = GetNewUser();
        await _store.CreateAsync(user, CancellationToken.None);
        user = await _store.FindByEmailAsync(_testUser.Email, CancellationToken.None);
        Assert.That(user, Is.Not.Null);

        var result = await _store.DeleteAsync(user, CancellationToken.None);
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(result, Is.EqualTo(IdentityResult.Success));
            Assert.That(await _store.FindByEmailAsync(_testUser.Email, CancellationToken.None), Is.Null);
        });
    }

    [Test]
    public async Task FindUser()
    {
        var user = GetNewUser();

        var result = await _store.CreateAsync(user, CancellationToken.None);
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(result, Is.EqualTo(IdentityResult.Success));
            Assert.That((await _store.FindByIdAsync(user.Id.ToString(), CancellationToken.None)).UserName, Is.EqualTo(user.UserName));
        });

        user = await _store.FindByEmailAsync(_testUser.Email, CancellationToken.None);
        Assert.That(user.UserName, Is.EqualTo(_testUser.UserName));
        Assert.ThrowsAsync<ArgumentNullException>(() => _store.FindByEmailAsync(null, CancellationToken.None));

        user = await _store.FindByNameAsync(_testUser.UserName, CancellationToken.None);
        Assert.That(user.Email, Is.EqualTo(_testUser.Email));
        Assert.ThrowsAsync<ArgumentNullException>(() => _store.FindByNameAsync(null, CancellationToken.None));
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(await _store.FindByNameAsync("does-not-exist", CancellationToken.None), Is.Null);
            Assert.That(await _store.FindByIdAsync("0", CancellationToken.None), Is.Null);
            Assert.That(await _store.FindByIdAsync("a", CancellationToken.None), Is.Null);

            Assert.That(await _store.GetUserIdAsync(user, CancellationToken.None), Is.EqualTo(user.Id.ToString()));
            Assert.That(await _store.GetUserNameAsync(user, CancellationToken.None), Is.EqualTo(user.UserName));
            Assert.That(await _store.GetNormalizedUserNameAsync(user, CancellationToken.None), Is.EqualTo(user.UserName?.ToUpperInvariant()));

            Assert.That(await _store.GetNormalizedEmailAsync(user, CancellationToken.None), Is.EqualTo(user.Email?.ToUpperInvariant()));
        });
    }

    [Test]
    public async Task SetUserName()
    {
        var user = GetNewUser();

        var newUserName = "abcdefg";
        var securityStamp = await _store.GetSecurityStampAsync(user, CancellationToken.None);
        await _store.SetUserNameAsync(user, newUserName, CancellationToken.None);
        Assert.That(await _store.GetSecurityStampAsync(user, CancellationToken.None), Is.Not.EqualTo(securityStamp));
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(await _store.GetUserNameAsync(user, CancellationToken.None), Is.EqualTo(newUserName));
            Assert.That(user.UserName, Is.EqualTo(newUserName));
            Assert.That(await _store.GetNormalizedUserNameAsync(user, CancellationToken.None), Is.EqualTo(newUserName.ToUpperInvariant()));
        });
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
        Assert.That(await _store.GetSecurityStampAsync(user, CancellationToken.None), Is.Not.EqualTo(securityStamp));
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(await _store.GetEmailAsync(user, CancellationToken.None), Is.EqualTo(newEmail));
            Assert.That(user.Email, Is.EqualTo(newEmail));
            Assert.That(await _store.GetNormalizedEmailAsync(user, CancellationToken.None), Is.EqualTo(newEmail.ToUpperInvariant()));
        });

        await _store.SetEmailConfirmedAsync(user, true, CancellationToken.None);
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(user.EmailConfirmed, Is.EqualTo(true));
            Assert.That(await _store.GetEmailConfirmedAsync(user, CancellationToken.None), Is.EqualTo(true));
        });
        Assert.That(user.EmailConfirmedOn, Is.Not.Null);
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
        Assert.That(user.PhoneNumber, Is.EqualTo(string.Empty));

        await _store.SetPhoneNumberAsync(user, newPhone, CancellationToken.None);
        Assert.That(await _store.GetSecurityStampAsync(user, CancellationToken.None), Is.Not.EqualTo(securityStamp));
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(await _store.GetPhoneNumberAsync(user, CancellationToken.None), Is.EqualTo(newPhone));

            Assert.That(await _store.GetPhoneNumberConfirmedAsync(user, CancellationToken.None), Is.EqualTo(user.PhoneNumberConfirmed));
        });
        await _store.SetPhoneNumberConfirmedAsync(user, true, CancellationToken.None);
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(user.PhoneNumberConfirmed, Is.EqualTo(true));
            Assert.That(await _store.GetPhoneNumberConfirmedAsync(user, CancellationToken.None), Is.EqualTo(true));
        });
        Assert.That(user.PhoneNumberConfirmedOn, Is.Not.Null);

        // Empty phone number should not be set confirmed
        user.PhoneNumber = string.Empty;
        user.PhoneNumberConfirmed = true;
        await _store.SetPhoneNumberConfirmedAsync(user, true, CancellationToken.None);
        Assert.That(user.PhoneNumberConfirmed, Is.False);
    }

    [Test]
    public async Task UserPasswordStore()
    {
        var user = GetNewUser();
        user.PasswordHash = string.Empty;

        Assert.That(await _store.GetPasswordHashAsync(user, CancellationToken.None), Is.Null);
        var newPwHash = "passwordhash";
        var securityStamp = await _store.GetSecurityStampAsync(user, CancellationToken.None);
        await _store.SetPasswordHashAsync(user, newPwHash, CancellationToken.None);
        Assert.That(await _store.GetSecurityStampAsync(user, CancellationToken.None), Is.Not.EqualTo(securityStamp));
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(await _store.GetPasswordHashAsync(user, CancellationToken.None), Is.EqualTo(newPwHash));
            Assert.That(await _store.HasPasswordAsync(user, CancellationToken.None), Is.True);
        });
    }

    [Test]
    public async Task UserLockoutStore()
    {
        var user = GetNewUser();
        var nonExistentUser = new ApplicationUser {Id = 0};

        Assert.That(await _store.CreateAsync(user, CancellationToken.None), Is.EqualTo(IdentityResult.Success));
        Assert.DoesNotThrowAsync(() => _store.SetLockoutEnabledAsync(user, true, CancellationToken.None));

        await Assert.MultipleAsync(async () =>
        {
            // users for except SystemManagers lock-out should be enabled
            Assert.That(await _store.GetLockoutEnabledAsync(user, CancellationToken.None), Is.True);

            // not set before, so it should be null
            Assert.That(await _store.GetLockoutEndDateAsync(user, CancellationToken.None), Is.Null);
        });
        var lockoutEndDate = new DateTimeOffset?(new DateTime(2020, 05, 30, 14, 15, 16));

        // Test setting lockout end date
        await _store.SetLockoutEndDateAsync(user, lockoutEndDate, CancellationToken.None);
        Assert.That((await _store.GetLockoutEndDateAsync(user, CancellationToken.None)).Value.DateTime, Is.EqualTo(lockoutEndDate.Value.UtcDateTime));

        // Nothing should happen for non-existent users
        Assert.ThrowsAsync<ArgumentException>(() => _store.SetLockoutEndDateAsync(nonExistentUser, lockoutEndDate, CancellationToken.None));
        Assert.DoesNotThrowAsync(() => _store.GetLockoutEndDateAsync(nonExistentUser, CancellationToken.None));

        await Assert.MultipleAsync(async () =>
        {
            // Test access failures
            Assert.That(await _store.GetAccessFailedCountAsync(user, CancellationToken.None), Is.EqualTo(0));
            Assert.That(await _store.IncrementAccessFailedCountAsync(user, CancellationToken.None), Is.EqualTo(1));
        });
        Assert.That(await _store.IncrementAccessFailedCountAsync(user, CancellationToken.None), Is.EqualTo(2));
        Assert.ThrowsAsync<ArgumentException>(() => _store.IncrementAccessFailedCountAsync(nonExistentUser, CancellationToken.None));

        // Test after resetting failed count and lockout date
        await _store.ResetAccessFailedCountAsync(user, CancellationToken.None);
        await _store.SetLockoutEndDateAsync(user, null, CancellationToken.None);
        Assert.That(await _store.GetAccessFailedCountAsync(user, CancellationToken.None), Is.EqualTo(0));
        Assert.DoesNotThrowAsync(() => _store.ResetAccessFailedCountAsync(nonExistentUser, CancellationToken.None));
        Assert.That(await _store.GetAccessFailedCountAsync(nonExistentUser, CancellationToken.None), Is.EqualTo(0));

        // Tests for user as SystemManager
        var role = new ApplicationRole { Name = Constants.RoleName.SystemManager };
        Assert.That(await _roleStore.CreateAsync(role, CancellationToken.None), Is.EqualTo(IdentityResult.Success));
            
        // SystemManagers should not be locked out
        await _store.AddToRoleAsync(user, Constants.RoleName.SystemManager, CancellationToken.None);
        Assert.That(await _store.GetLockoutEnabledAsync(user, CancellationToken.None), Is.False);

        // Test setting lockout end date
        await _store.SetLockoutEndDateAsync(user, lockoutEndDate, CancellationToken.None);
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(await _store.GetLockoutEndDateAsync(user, CancellationToken.None), Is.EqualTo(null));
            Assert.That(await _store.IncrementAccessFailedCountAsync(user, CancellationToken.None), Is.EqualTo(0));
        });
        Assert.That(await _store.IncrementAccessFailedCountAsync(user, CancellationToken.None), Is.EqualTo(0));
    }

    [Test]
    public void DisposeTest()
    {
        Assert.DoesNotThrow(() =>_store.Dispose());
    }
}
