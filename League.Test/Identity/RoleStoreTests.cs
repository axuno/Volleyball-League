using Microsoft.AspNetCore.Identity;
using System;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Threading;
using League.Identity;
using League.Test.TestComponents;
using NUnit.Framework;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.DatabaseSpecific;
using TournamentManager.MultiTenancy;

namespace League.Test.Identity
{
    /// <summary>
    /// Integration tests
    /// </summary>
    [TestFixture]
    public class RoleStoreTests
    {
        private readonly UnitTestHelpers _uth = new UnitTestHelpers();
        private readonly AppDb _appDb;
        private readonly RoleStore _roleStore;
        private readonly UserStore _userStore;

        public RoleStoreTests()
        {
            _appDb = _uth.GetTenantContext().DbContext.AppDb;
            _userStore = _uth.GetUserStore();
            _roleStore = _uth.GetRoleStore();
        }

        private readonly ApplicationUser _testUser = new ApplicationUser
        {
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

        [Test]
        public void RoleConstants()
        {
            Assert.IsTrue(Constants.RoleName.GetTeamRelatedRoles().Contains(Constants.RoleName.TeamManager));
            Assert.IsTrue(Constants.RoleName.GetTeamRelatedRoles().Contains(Constants.RoleName.Player));

            Assert.IsTrue(Constants.RoleName.GetAllValues<string>().Any(rn => rn == Constants.RoleName.TournamentManager));
            Assert.IsTrue(Constants.RoleName.GetAllValues<string>().Any(rn => rn == Constants.RoleName.SystemManager));
            Assert.IsTrue(Constants.RoleName.GetAllNames().Contains(nameof(Constants.RoleName.TournamentManager)));
        }

        [Test]
        public void ArgumentNullExceptions()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _roleStore.FindByNameAsync(null, CancellationToken.None));
            Assert.ThrowsAsync<ArgumentNullException>(() => _roleStore.GetClaimsAsync(null, CancellationToken.None));
            Assert.ThrowsAsync<ArgumentNullException>(() => _roleStore.GetRoleNameAsync(null, CancellationToken.None));
            Assert.ThrowsAsync<ArgumentNullException>(() => _roleStore.SetRoleNameAsync(null, "x", CancellationToken.None));
            Assert.ThrowsAsync<ArgumentNullException>(() => _roleStore.SetRoleNameAsync(new ApplicationRole(), null, CancellationToken.None));
            Assert.ThrowsAsync<ArgumentNullException>(() => _roleStore.GetRoleIdAsync(null, CancellationToken.None));
            Assert.ThrowsAsync<ArgumentNullException>(() => _roleStore.GetNormalizedRoleNameAsync(null, CancellationToken.None));
            Assert.ThrowsAsync<ArgumentNullException>(() => _roleStore.CreateAsync(null, CancellationToken.None));
            Assert.ThrowsAsync<ArgumentNullException>(() => _roleStore.UpdateAsync(null, CancellationToken.None));
            Assert.ThrowsAsync<ArgumentNullException>(() => _roleStore.DeleteAsync(null, CancellationToken.None));
            Assert.ThrowsAsync<ArgumentNullException>(() => _roleStore.AddClaimAsync(null, new Claim("any", "thing"),  CancellationToken.None));
            Assert.ThrowsAsync<ArgumentNullException>(() => _roleStore.AddClaimAsync(new ApplicationRole(), null, CancellationToken.None));
            Assert.ThrowsAsync<ArgumentNullException>(() => _roleStore.RemoveClaimAsync(null, new Claim("any", "thing"),  CancellationToken.None));
            Assert.ThrowsAsync<ArgumentNullException>(() => _roleStore.RemoveClaimAsync(new ApplicationRole(), null, CancellationToken.None));
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
                Assert.AreNotEqual(IdentityResult.Success, await _roleStore.CreateAsync(role, CancellationToken.None));
                Assert.AreNotEqual(IdentityResult.Success, await _roleStore.DeleteAsync(role, CancellationToken.None));
                Assert.AreNotEqual(IdentityResult.Success, await _roleStore.UpdateAsync(role, CancellationToken.None));
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
                Assert.AreEqual(IdentityResult.Success, await _roleStore.CreateAsync(role, CancellationToken.None));

                await da.StartTransactionAsync(IsolationLevel.Serializable, "transaction1");
                da.SaveTransaction("transaction1");
                // lock the table for completely
                await da.ExecuteSQLAsync(CancellationToken.None,
                    $"SELECT 1 FROM [{_appDb.DbContext.Schema}].[{da.GetPersistentTableName(new IdentityRoleClaimEntity())}] WITH (TABLOCKX)");
                // Trying to update will fail because the table is locked
                _appDb.DbContext.CommandTimeOut = 2;
                // new claim
                var claim = new Claim(Constants.ClaimType.ManagesTeam, "y", "z");
                Assert.ThrowsAsync<ORMQueryExecutionException>(() => _roleStore.AddClaimAsync(role, claim, CancellationToken.None));
                Assert.ThrowsAsync<ORMQueryExecutionException>(() => _roleStore.RemoveClaimAsync(role, claim, CancellationToken.None));
                da.Rollback("transaction1");
            }
            _appDb.DbContext.CommandTimeOut = currentTimeOut;
        }


        [Test]
        public async Task Create_Role()
        {
            // trying to create a role with an unknown name fails
            var role = new ApplicationRole { Name = "some-illegal-role-name" };
            Assert.AreNotEqual(IdentityResult.Success, await _roleStore.CreateAsync(role, CancellationToken.None));

            role = new ApplicationRole {Name = Constants.RoleName.SystemManager};
            Assert.AreEqual(IdentityResult.Success, await _roleStore.CreateAsync(role, CancellationToken.None));

            var createdRole = await _roleStore.FindByNameAsync(role.Name, CancellationToken.None);
            Assert.AreEqual(role.Name, createdRole.Name);

            // trying to create the same role again fails
            Assert.AreNotEqual(IdentityResult.Success, await _roleStore.CreateAsync(role, CancellationToken.None));
        }

        [Test]
        public async Task Update_Role()
        {
            // create a new role
            var newRole = new ApplicationRole { Name = Constants.RoleName.SystemManager };
            Assert.AreEqual(IdentityResult.Success, await _roleStore.CreateAsync(newRole, CancellationToken.None));
            var createdRole = await _roleStore.FindByNameAsync(Constants.RoleName.SystemManager, CancellationToken.None);
            Assert.IsNotNull(createdRole);

            // trying to update a role with an unknown name should fail
            createdRole.Name = "some-illegal-role-name";
            Assert.AreNotEqual(IdentityResult.Success, await _roleStore.UpdateAsync(createdRole, CancellationToken.None));

            // trying to update a role with non-existent ID should fail
            Assert.AreNotEqual(IdentityResult.Success, await _roleStore.UpdateAsync(new ApplicationRole {Id = -98765, Name = Constants.RoleName.TournamentManager}, CancellationToken.None));
            
            // update with allowed role name should succeed
            createdRole.Name = Constants.RoleName.Player;
            Assert.AreEqual(IdentityResult.Success, await _roleStore.UpdateAsync(createdRole, CancellationToken.None));
            Assert.IsNotNull(await _roleStore.FindByNameAsync(Constants.RoleName.Player, CancellationToken.None));

            // create a new role
            newRole = new ApplicationRole { Name = Constants.RoleName.SystemManager };
            Assert.AreEqual(IdentityResult.Success, await _roleStore.CreateAsync(newRole, CancellationToken.None));
            createdRole = await _roleStore.FindByNameAsync(Constants.RoleName.SystemManager, CancellationToken.None);
            Assert.IsNotNull(createdRole);
            // try to update with existing role name should fail
            createdRole.Name = Constants.RoleName.Player;
            Assert.AreNotEqual(IdentityResult.Success, await _roleStore.UpdateAsync(createdRole, CancellationToken.None));
        }

        [Test]
        public async Task Delete_Role()
        {
            // create a new role
            var newRole = new ApplicationRole { Name = Constants.RoleName.SystemManager };
            Assert.AreEqual(IdentityResult.Success, await _roleStore.CreateAsync(newRole, CancellationToken.None));
            var createdRole = await _roleStore.FindByNameAsync(Constants.RoleName.SystemManager, CancellationToken.None);
            Assert.IsNotNull(createdRole);

            // delete the created role
            Assert.AreEqual(IdentityResult.Success, await _roleStore.DeleteAsync(createdRole, CancellationToken.None));
            var role = await _roleStore.FindByNameAsync(Constants.RoleName.SystemManager, CancellationToken.None);
            Assert.IsNull(role);
        }

        [Test]
        public async Task Find_Role()
        {
            // create a new role
            var newRole = new ApplicationRole { Name = Constants.RoleName.SystemManager };
            Assert.AreEqual(IdentityResult.Success, await _roleStore.CreateAsync(newRole, CancellationToken.None));
            var createdRole = await _roleStore.FindByNameAsync(Constants.RoleName.SystemManager, CancellationToken.None);
            Assert.IsNotNull(createdRole);
            Assert.AreEqual(createdRole.Name, (await _roleStore.FindByIdAsync(createdRole.Id.ToString(), CancellationToken.None)).Name);
            Assert.IsNull(await _roleStore.FindByIdAsync("0", CancellationToken.None));
            Assert.IsNull(await _roleStore.FindByIdAsync("abc", CancellationToken.None));
        }

        [Test]
        public async Task Normalized_Role_Name()
        {
            var role = new ApplicationRole { Name = Constants.RoleName.SystemManager };
            Assert.AreEqual(role.Name.Length, (await _roleStore.GetNormalizedRoleNameAsync(role, CancellationToken.None)).Length);

            Assert.DoesNotThrowAsync(async () => await _roleStore.SetNormalizedRoleNameAsync(role, await _roleStore.GetNormalizedRoleNameAsync(role, CancellationToken.None), CancellationToken.None));
        }

        [Test]
        public void Get_Role_Id_and_Name()
        {
            var role = new ApplicationRole { Id = 1234, Name = Constants.RoleName.SystemManager };
            Assert.DoesNotThrowAsync(async () => await _roleStore.SetRoleNameAsync(role, Constants.RoleName.TournamentManager, CancellationToken.None));
            Assert.AreEqual(role.Id.ToString(), _roleStore.GetRoleIdAsync(role, CancellationToken.None).Result);
            Assert.AreEqual(Constants.RoleName.TournamentManager, _roleStore.GetRoleNameAsync(role, CancellationToken.None).Result);
        }

        [Test]
        public async Task Add_and_Get_and_Remove_Role_Claim()
        {
            // create role
            var role = new ApplicationRole { Name = Constants.RoleName.SystemManager };
            Assert.AreEqual(IdentityResult.Success, await _roleStore.CreateAsync(role, CancellationToken.None));

            // add claim to non existing role should throw
            Assert.ThrowsAsync<Exception>(() => _roleStore.AddClaimAsync(new ApplicationRole(), new Claim(Constants.ClaimType.ManagesTeam, "y", "z"), CancellationToken.None));

            // add claim to role
            var guid = Guid.NewGuid().ToString("N");
            var claim = new Claim(guid, guid, "Guid", "League");
            await _roleStore.AddClaimAsync(role, claim, CancellationToken.None);
            // each claim will be stored only once, so this one will not be added
            await _roleStore.AddClaimAsync(role, claim, CancellationToken.None);

            // get created claim
            var claims = await _roleStore.GetClaimsAsync(role, CancellationToken.None);
            Assert.AreEqual(1, claims.Count);
            var createdClaim = claims.FirstOrDefault();
            Assert.IsNotNull(createdClaim);
            Assert.IsTrue(claim.Type == createdClaim.Type && claim.Value == createdClaim.Value && claim.ValueType == createdClaim.ValueType && claim.Issuer == createdClaim.Issuer);

            // remove claim
            await _roleStore.RemoveClaimAsync(role, createdClaim, CancellationToken.None);
            Assert.AreEqual(0, (await _roleStore.GetClaimsAsync(role, CancellationToken.None)).Count);
            // remove non-existing claim should throw
            Assert.ThrowsAsync<Exception>(() => _roleStore.RemoveClaimAsync(role, new Claim(Constants.ClaimType.ManagesTeam, "x"), CancellationToken.None));
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
            await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<UserEntity>(null, CancellationToken.None);
            // remove all roles
            await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<IdentityRoleEntity>(null, CancellationToken.None);
            // remove all role claims
            await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<IdentityRoleClaimEntity>(null, CancellationToken.None);

            // create a new test user
            Assert.AreEqual(IdentityResult.Success ,await _userStore.CreateAsync(GetNewUser(), CancellationToken.None));
        }

        [TearDown]
        public async Task Cleanup()
        {
            // remove all users
            await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<UserEntity>(null, CancellationToken.None);
            // remove all roles
            await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<IdentityRoleEntity>(null, CancellationToken.None);
            // remove all role claims
            await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<IdentityRoleClaimEntity>(null, CancellationToken.None);
        }
    }
}
