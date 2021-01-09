﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using League.Identity;
using League.Test.TestComponents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using TournamentManager.DAL.DatabaseSpecific;
using TournamentManager.Data;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.MultiTenancy;

namespace League.Test.Identity
{
    /// <summary>
    /// Integration tests
    /// </summary>
    [TestFixture]
    public class UserRoleStoreTests
    {
        private readonly UnitTestHelpers _uth = new UnitTestHelpers();
        private readonly AppDb _appDb;
        private readonly UserStore _userStore;
        private ApplicationUser _user = null;

        public UserRoleStoreTests()
        {
            _appDb = _uth.GetTenantContext().DbContext.AppDb;
            _userStore = _uth.GetUserStore();
        }

        [Test]
        public async Task ShouldThrow_WritingTo_Database()
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
                Assert.ThrowsAsync<Exception>(async () => await _userStore.RemoveFromRoleAsync(_user, Constants.RoleName.TournamentManager, CancellationToken.None));
                Assert.ThrowsAsync<Exception>(async () => await _userStore.AddToRoleAsync(_user, Constants.RoleName.TournamentManager, CancellationToken.None));
                da.Rollback("transaction1");
            }
            _appDb.DbContext.CommandTimeOut = currentTimeOut;
        }

        [Test]
        public async Task AddToRole_GetRoles_GetUsersInRole()
        {
            await _userStore.AddToRoleAsync(_user, Constants.RoleName.TournamentManager, CancellationToken.None);
            Assert.IsTrue(await _userStore.IsInRoleAsync(_user, Constants.RoleName.TournamentManager, CancellationToken.None));

            // adding the same role again should throw
            Assert.ThrowsAsync<Exception>(() => _userStore.AddToRoleAsync(_user, Constants.RoleName.TournamentManager, CancellationToken.None));

            Assert.IsTrue((await _userStore.GetRolesAsync(_user, CancellationToken.None)).Contains(Constants.RoleName.TournamentManager));

            Assert.IsTrue((await _userStore.GetUsersInRoleAsync(Constants.RoleName.TournamentManager, CancellationToken.None)).FirstOrDefault(u => u.Email == _user.Email) != null);

            Assert.DoesNotThrowAsync(async () => await _userStore.RemoveFromRoleAsync(_user, Constants.RoleName.TournamentManager, CancellationToken.None));
        }

        [Test]
        public async Task Add_Remove_To_Implicit_Roles()
        {
            const string email = "player-and-manager.userrole@store.tests";
            var team = new TeamEntity { Name = "Fancy Team" };
            var userEntity = new UserEntity { Email = email, UserName = "PlayerManagerUserRoleStoreTester" };
            userEntity.PlayerInTeams.Add(new PlayerInTeamEntity { User = userEntity, Team = team });
            userEntity.ManagerOfTeams.Add(new ManagerOfTeamEntity { User = userEntity, Team = team });
            await _appDb.GenericRepository.SaveEntityAsync(userEntity, true, true, CancellationToken.None);

            var user = await _userStore.FindByEmailAsync(email, CancellationToken.None);
            Assert.IsTrue(await _userStore.IsInRoleAsync(user, Constants.RoleName.Player, CancellationToken.None));
            Assert.IsTrue(await _userStore.IsInRoleAsync(user, Constants.RoleName.TeamManager, CancellationToken.None));
            Assert.IsTrue((await _userStore.GetUsersInRoleAsync(Constants.RoleName.Player, CancellationToken.None)).Count == 1);
            Assert.IsTrue((await _userStore.GetUsersInRoleAsync(Constants.RoleName.TeamManager, CancellationToken.None)).Count == 1);


            await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<PlayerInTeamEntity>(null, CancellationToken.None);
            await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<ManagerOfTeamEntity>(null, CancellationToken.None);

            Assert.IsFalse(await _userStore.IsInRoleAsync(user, Constants.RoleName.Player, CancellationToken.None));
            Assert.IsFalse(await _userStore.IsInRoleAsync(user, Constants.RoleName.TeamManager, CancellationToken.None));
        }

        [Test]
        public void TryToAdd_Forbidden_Roles()
        {
            foreach (var role in Constants.RoleName.GetTeamRelatedRoles())
            {
                Assert.ThrowsAsync<Exception>(async () => await _userStore.AddToRoleAsync(_user, role, CancellationToken.None));
            }
        }

        [Test]
        public void TryToRemove_Forbidden_Roles()
        {
            foreach (var role in Constants.RoleName.GetTeamRelatedRoles())
            {
                Assert.ThrowsAsync<Exception>( () =>  _userStore.RemoveFromRoleAsync(_user, role, CancellationToken.None));
            }
        }

        [Test]
        public void ArgumentNullExceptions()
        {
            // Should throw ArgumentNullException
            Assert.ThrowsAsync<ArgumentNullException>(() => _userStore.IsInRoleAsync(null, "x", CancellationToken.None));
            Assert.ThrowsAsync<ArgumentNullException>(() => _userStore.IsInRoleAsync(_user, null, CancellationToken.None));

            Assert.ThrowsAsync<ArgumentNullException>( () =>  _userStore.RemoveFromRoleAsync(_user, null, CancellationToken.None));
            Assert.ThrowsAsync<ArgumentNullException>(() => _userStore.RemoveFromRoleAsync(null, "xyz", CancellationToken.None));

            Assert.ThrowsAsync<ArgumentNullException>(() => _userStore.AddToRoleAsync(_user, null, CancellationToken.None));
            Assert.ThrowsAsync<ArgumentNullException>(() => _userStore.AddToRoleAsync(null, "xyz", CancellationToken.None));

            Assert.ThrowsAsync<ArgumentNullException>(() => _userStore.GetRolesAsync(null, CancellationToken.None));

            Assert.ThrowsAsync<ArgumentNullException>(() => _userStore.GetUsersInRoleAsync(null, CancellationToken.None));
        }

        [SetUp]
        public async Task Setup()
        {
            // delete all user rows (will also remove rows in other tables connected with foreign keys)
            await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<UserEntity>(null, CancellationToken.None);

            _user = new ApplicationUser { Email = "userrole@store.tests", UserName = "UserRoleStoreTester"};
            Assert.AreEqual(IdentityResult.Success, await _userStore.CreateAsync(_user, CancellationToken.None));
        }

        [TearDown]
        public async Task Cleanup()
        {
            // delete all user rows (will also remove rows in other tables connected with foreign keys)
            await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<UserEntity>(null, CancellationToken.None);
        }

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<IdentityRoleEntity>(null, CancellationToken.None);
            await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<TeamEntity>(null, CancellationToken.None);

            var roles = new EntityCollection<IdentityRoleEntity>();
            foreach (var roleName in Constants.RoleName.GetAllValues<string>())
            {
                roles.Add(new IdentityRoleEntity { Name = roleName });
            }

            await _appDb.GenericRepository.SaveEntitiesAsync(roles, false, false, CancellationToken.None);
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<IdentityRoleEntity>(null, CancellationToken.None);
            await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<TeamEntity>(null, CancellationToken.None);
        }
    }
}
