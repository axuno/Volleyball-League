using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Security.Claims;
using League.Identity;
using NUnit.Framework;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.DatabaseSpecific;
using TournamentManager.MultiTenancy;

namespace League.Tests.Identity;

/// <summary>
/// Integration tests
/// </summary>
[TestFixture]
public class RoleStoreTests
{
    private readonly UnitTestHelpers _uth = new();
    private readonly IAppDb _appDb;
    private readonly RoleStore _roleStore;
    private readonly UserStore _userStore;

    public RoleStoreTests()
    {
        _appDb = _uth.GetTenantContext().DbContext.AppDb;
        _userStore = _uth.GetUserStore();
        _roleStore = _uth.GetRoleStore();
    }

    [OneTimeTearDown]
    public void DisposeObjects()
    {
        _roleStore.Dispose();
        _userStore.Dispose();
    }

    private readonly ApplicationUser _testUser = new() {
        Email = "role@store.test",
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
        return new()
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

    [Test]
    public void RoleConstants()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Constants.RoleName.GetTeamRelatedRoles(), Does.Contain(Constants.RoleName.TeamManager));
            Assert.That(Constants.RoleName.GetTeamRelatedRoles(), Does.Contain(Constants.RoleName.Player));

            Assert.That(Constants.RoleName.GetAllRoleValues<string>().Any(rn => rn == Constants.RoleName.TournamentManager), Is.True);
            Assert.That(Constants.RoleName.GetAllRoleValues<string>().Any(rn => rn == Constants.RoleName.SystemManager), Is.True);
            Assert.That(Constants.RoleName.GetAllRoleNames(), Does.Contain(nameof(Constants.RoleName.TournamentManager)));
        }
    }

    [Test]
    public async Task ShouldThrow_Writing_Role_To_Database()
    {
        // Deleting the role should fail because of a table lock
        var currentTimeOut = _appDb.DbContext.CommandTimeOut;
        using (var da = (DataAccessAdapter)_appDb.DbContext.GetNewAdapter())
        {
            await da.StartTransactionAsync(IsolationLevel.Serializable, "transaction1");
            da.SaveTransaction("transaction1");
            // lock the table for completely
            await da.ExecuteSQLAsync(CancellationToken.None,
                $"SELECT 1 FROM [{_appDb.DbContext.Schema}].[{da.GetPersistentTableName(new IdentityRoleEntity())}] WITH (TABLOCKX)");
            // Trying to update will fail because the table is locked
            _appDb.DbContext.CommandTimeOut = 2;
            // new claim
            var role = new ApplicationRole() {Name = Constants.RoleName.TournamentManager};
            using (Assert.EnterMultipleScope())
            {
                Assert.That(await _roleStore.CreateAsync(role, CancellationToken.None), Is.Not.EqualTo(IdentityResult.Success));
                Assert.That(await _roleStore.DeleteAsync(role, CancellationToken.None), Is.Not.EqualTo(IdentityResult.Success));
                Assert.That(await _roleStore.UpdateAsync(role, CancellationToken.None), Is.Not.EqualTo(IdentityResult.Success));
            }
            da.Rollback("transaction1");
        }
        _appDb.DbContext.CommandTimeOut = currentTimeOut;
    }

    [Test]
    public async Task ShouldThrow_Writing_Claim_To_Database()
    {
        // Deleting the role should fail because of a table lock
        var currentTimeOut = _appDb.DbContext.CommandTimeOut;
        using (var da = (DataAccessAdapter)_appDb.DbContext.GetNewAdapter())
        {
            var role = new ApplicationRole { Name = Constants.RoleName.TeamManager };
            Assert.That(await _roleStore.CreateAsync(role, CancellationToken.None), Is.EqualTo(IdentityResult.Success));

            await da.StartTransactionAsync(IsolationLevel.Serializable, "transaction1");
            da.SaveTransaction("transaction1");
            // lock the table for completely
            await da.ExecuteSQLAsync(CancellationToken.None,
                $"SELECT 1 FROM [{_appDb.DbContext.Schema}].[{da.GetPersistentTableName(new IdentityRoleClaimEntity())}] WITH (TABLOCKX)");
            // Trying to update will fail because the table is locked
            _appDb.DbContext.CommandTimeOut = 2;
            // new claim
            var claim = new Claim(Constants.ClaimType.ManagesTeam, "y", "z");
            Assert.ThrowsAsync<InvalidOperationException>(() => _roleStore.AddClaimAsync(role, claim, CancellationToken.None));
            Assert.ThrowsAsync<InvalidOperationException>(() => _roleStore.RemoveClaimAsync(role, claim, CancellationToken.None));
            da.Rollback("transaction1");
        }
        _appDb.DbContext.CommandTimeOut = currentTimeOut;
    }


    [Test]
    public async Task Create_Role()
    {
        // trying to create a role with an unknown name fails
        var role = new ApplicationRole { Name = "some-illegal-role-name" };
        Assert.That(await _roleStore.CreateAsync(role, CancellationToken.None), Is.Not.EqualTo(IdentityResult.Success));

        role = new() {Name = Constants.RoleName.SystemManager};
        Assert.That(await _roleStore.CreateAsync(role, CancellationToken.None), Is.EqualTo(IdentityResult.Success));

        var createdRole = await _roleStore.FindByNameAsync(role.Name, CancellationToken.None);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(createdRole.Name, Is.EqualTo(role.Name));

            // trying to create the same role again fails
            Assert.That(await _roleStore.CreateAsync(role, CancellationToken.None), Is.Not.EqualTo(IdentityResult.Success));
        }
    }

    [Test]
    public async Task Update_Role()
    {
        // create a new role
        var newRole = new ApplicationRole { Name = Constants.RoleName.SystemManager };
        Assert.That(await _roleStore.CreateAsync(newRole, CancellationToken.None), Is.EqualTo(IdentityResult.Success));
        var createdRole = await _roleStore.FindByNameAsync(Constants.RoleName.SystemManager, CancellationToken.None);
        Assert.That(createdRole, Is.Not.Null);

        // trying to update a role with an unknown name should fail
        createdRole.Name = "some-illegal-role-name";
        using (Assert.EnterMultipleScope())
        {
            Assert.That(await _roleStore.UpdateAsync(createdRole, CancellationToken.None), Is.Not.EqualTo(IdentityResult.Success));

            // trying to update a role with non-existent ID should fail
            Assert.That(await _roleStore.UpdateAsync(new() { Id = -98765, Name = Constants.RoleName.TournamentManager }, CancellationToken.None), Is.Not.EqualTo(IdentityResult.Success));
        }

        // update with allowed role name should succeed
        createdRole.Name = Constants.RoleName.Player;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(await _roleStore.UpdateAsync(createdRole, CancellationToken.None), Is.EqualTo(IdentityResult.Success));
            Assert.That(await _roleStore.FindByNameAsync(Constants.RoleName.Player, CancellationToken.None), Is.Not.Null);
        }

        // create a new role
        newRole = new() { Name = Constants.RoleName.SystemManager };
        Assert.That(await _roleStore.CreateAsync(newRole, CancellationToken.None), Is.EqualTo(IdentityResult.Success));
        createdRole = await _roleStore.FindByNameAsync(Constants.RoleName.SystemManager, CancellationToken.None);
        Assert.That(createdRole, Is.Not.Null);
        // try to update with existing role name should fail
        createdRole.Name = Constants.RoleName.Player;
        Assert.That(await _roleStore.UpdateAsync(createdRole, CancellationToken.None), Is.Not.EqualTo(IdentityResult.Success));
    }

    [Test]
    public async Task Delete_Role()
    {
        // create a new role
        var newRole = new ApplicationRole { Name = Constants.RoleName.SystemManager };
        Assert.That(await _roleStore.CreateAsync(newRole, CancellationToken.None), Is.EqualTo(IdentityResult.Success));
        var createdRole = await _roleStore.FindByNameAsync(Constants.RoleName.SystemManager, CancellationToken.None);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(createdRole, Is.Not.Null);

            // delete the created role
            Assert.That(await _roleStore.DeleteAsync(createdRole, CancellationToken.None), Is.EqualTo(IdentityResult.Success));
        }
        var role = await _roleStore.FindByNameAsync(Constants.RoleName.SystemManager, CancellationToken.None);
        Assert.That(role, Is.Null);
    }

    [Test]
    public async Task Find_Role()
    {
        // create a new role
        var newRole = new ApplicationRole { Name = Constants.RoleName.SystemManager };
        Assert.That(await _roleStore.CreateAsync(newRole, CancellationToken.None), Is.EqualTo(IdentityResult.Success));
        var createdRole = await _roleStore.FindByNameAsync(Constants.RoleName.SystemManager, CancellationToken.None);
        Assert.That(createdRole, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That((await _roleStore.FindByIdAsync(createdRole.Id.ToString(), CancellationToken.None)).Name, Is.EqualTo(createdRole.Name));
            Assert.That(await _roleStore.FindByIdAsync("0", CancellationToken.None), Is.Null);
            Assert.That(await _roleStore.FindByIdAsync("abc", CancellationToken.None), Is.Null);
        }
    }

    [Test]
    public async Task Normalized_Role_Name()
    {
        var role = new ApplicationRole { Name = Constants.RoleName.SystemManager };
        Assert.That((await _roleStore.GetNormalizedRoleNameAsync(role, CancellationToken.None)), Has.Length.EqualTo(role.Name.Length));

        Assert.DoesNotThrowAsync(async () => await _roleStore.SetNormalizedRoleNameAsync(role, await _roleStore.GetNormalizedRoleNameAsync(role, CancellationToken.None), CancellationToken.None));
    }

    [Test]
    public void Get_Role_Id_and_Name()
    {
        var role = new ApplicationRole { Id = 1234, Name = Constants.RoleName.SystemManager };
        Assert.DoesNotThrowAsync(async () => await _roleStore.SetRoleNameAsync(role, Constants.RoleName.TournamentManager, CancellationToken.None));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_roleStore.GetRoleIdAsync(role, CancellationToken.None).Result, Is.EqualTo(role.Id.ToString()));
            Assert.That(_roleStore.GetRoleNameAsync(role, CancellationToken.None).Result, Is.EqualTo(Constants.RoleName.TournamentManager));
        }
    }

    [Test]
    public async Task Add_and_Get_and_Remove_Role_Claim()
    {
        // create role
        var role = new ApplicationRole { Name = Constants.RoleName.SystemManager };
        Assert.That(await _roleStore.CreateAsync(role, CancellationToken.None), Is.EqualTo(IdentityResult.Success));

        // add claim to non existing role should throw
        Assert.ThrowsAsync<InvalidOperationException>(() => _roleStore.AddClaimAsync(new(), new(Constants.ClaimType.ManagesTeam, "y", "z"), CancellationToken.None));

        // add claim to role
        var guid = Guid.NewGuid().ToString("N");
        var claim = new Claim(guid, guid, "Guid", "League");
        await _roleStore.AddClaimAsync(role, claim, CancellationToken.None);
        // each claim will be stored only once, so this one will not be added
        await _roleStore.AddClaimAsync(role, claim, CancellationToken.None);

        // get created claim
        var claims = await _roleStore.GetClaimsAsync(role, CancellationToken.None);
        Assert.That(claims, Has.Count.EqualTo(1));
        var createdClaim = claims.First();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(createdClaim, Is.Not.Null);
            Assert.That(claim.Type == createdClaim.Type && claim.Value == createdClaim.Value && claim.ValueType == createdClaim.ValueType && claim.Issuer == createdClaim.Issuer, Is.True);
        }

        // remove claim
        await _roleStore.RemoveClaimAsync(role, createdClaim, CancellationToken.None);
        Assert.That((await _roleStore.GetClaimsAsync(role, CancellationToken.None)), Is.Empty);
        // remove non-existing claim should throw
        Assert.ThrowsAsync<InvalidOperationException>(() => _roleStore.RemoveClaimAsync(role, new(Constants.ClaimType.ManagesTeam, "x"), CancellationToken.None));
    }
        
    [Test]
    public void DisposeTest()
    {
        Assert.DoesNotThrow(() => _roleStore.Dispose());
    }


    [SetUp]
    public async Task Setup()
    {
        // remove all users
        await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<UserEntity>(new PredicateExpression(), CancellationToken.None);
        // remove all roles
        await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<IdentityRoleEntity>(new PredicateExpression(), CancellationToken.None);
        // remove all role claims
        await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<IdentityRoleClaimEntity>(new PredicateExpression(), CancellationToken.None);

        // create a new test user
        Assert.That(await _userStore.CreateAsync(GetNewUser(), CancellationToken.None), Is.EqualTo(IdentityResult.Success));
    }

    [TearDown]
    public async Task Cleanup()
    {
        // remove all users
        await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<UserEntity>(new PredicateExpression(), CancellationToken.None);
        // remove all roles
        await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<IdentityRoleEntity>(new PredicateExpression(), CancellationToken.None);
        // remove all role claims
        await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<IdentityRoleClaimEntity>(new PredicateExpression(), CancellationToken.None);
    }
}
