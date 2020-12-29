using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Threading;
using League.DI;
using League.Identity;
using NUnit.Framework;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.Data;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.DatabaseSpecific;
using TournamentManager.MultiTenancy;

namespace League.Test.Identity
{
    /// <summary>
    /// Integration tests
    /// </summary>
    [TestFixture]
    public class UserClaimStoreTests
    {
        private readonly UnitTestHelpers _uth = new UnitTestHelpers();
        private readonly League.DI.SiteContext _orgCtx;
        private readonly AppDb _appDb; private ApplicationUser _user = null;
        private readonly UserStore _store;
        private TeamEntity _team = null;

        public UserClaimStoreTests()
        {
            _orgCtx = _uth.GetsiteContext();
            _appDb = _orgCtx.AppDb;
            _store = _uth.GetUserStore();
        }

        [Test]
        public void ClaimConstants()
        {
            Assert.IsTrue(Constants.ClaimType.GetTeamRelatedClaimTypes().Contains(Constants.ClaimType.ManagesTeam));
            Assert.IsTrue(Constants.ClaimType.GetTeamRelatedClaimTypes().Contains(Constants.ClaimType.PlaysInTeam));
            Assert.IsTrue(Constants.ClaimType.GetProgrammaticClaimTypes().Contains(Constants.ClaimType.ImpersonatedByUser));

            Assert.IsTrue(Constants.ClaimType.GetAllValues<string>().Any(cn => cn == Constants.ClaimType.ManagesTeam));
            Assert.IsTrue(Constants.ClaimType.GetAllValues<string>().Any(cn => cn == Constants.ClaimType.PlaysInTeam));
            Assert.IsTrue(Constants.ClaimType.GetAllValues<string>().Any(cn => cn == Constants.ClaimType.ImpersonatedByUser));
            
            Assert.IsTrue(Constants.ClaimType.GetAllNames().Contains(nameof(Constants.ClaimType.ManagesTeam)));
            Assert.IsTrue(Constants.ClaimType.GetAllNames().Contains(nameof(Constants.ClaimType.PlaysInTeam)));
            Assert.IsTrue(Constants.ClaimType.GetAllNames().Contains(nameof(Constants.ClaimType.ImpersonatedByUser)));
        }

        [Test]
        public void ArgumentNullExceptions()
        {
            // Should throw ArgumentNullException
            Assert.ThrowsAsync<ArgumentNullException>(() => _store.GetClaimsAsync(null, CancellationToken.None));
            Assert.ThrowsAsync<ArgumentNullException>(() => _store.AddClaimsAsync(null, new Claim[] { new Claim("x", "y") }, CancellationToken.None));
            Assert.ThrowsAsync<ArgumentNullException>(() => _store.AddClaimsAsync(_user, null, CancellationToken.None));
            Assert.ThrowsAsync<ArgumentNullException>(() => _store.RemoveClaimsAsync(null, new Claim[] { new Claim("x", "y") }, CancellationToken.None));
            Assert.ThrowsAsync<ArgumentNullException>(() => _store.RemoveClaimsAsync(_user, null, CancellationToken.None));
            Assert.ThrowsAsync<ArgumentNullException>(() => _store.ReplaceClaimAsync(null, new Claim("x", "y"), new Claim("a", "b"), CancellationToken.None));
            Assert.ThrowsAsync<ArgumentNullException>(() => _store.ReplaceClaimAsync(_user, null, new Claim("a", "b"), CancellationToken.None));
            Assert.ThrowsAsync<ArgumentNullException>(() => _store.ReplaceClaimAsync(_user, new Claim("x", "y"), null, CancellationToken.None));
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
                    $"SELECT 1 FROM [{_appDb.DbContext.Schema}].[{da.GetPersistentTableName(new IdentityUserClaimEntity())}] WITH (TABLOCKX)");
                // Trying to update will fail because the table is locked
                _appDb.DbContext.CommandTimeOut = 2;
                // new claim
                var claim = new Claim("type", "value", "valueType", "issuer");
                Assert.ThrowsAsync<ORMQueryExecutionException>(async () => await _store.AddClaimsAsync(_user, new Claim[] { claim }, CancellationToken.None));
                Assert.ThrowsAsync<ORMQueryExecutionException>(async () => await _store.RemoveClaimsAsync(_user, new Claim[] { claim }, CancellationToken.None));
                Assert.ThrowsAsync<ORMQueryExecutionException>(async () => await _store.ReplaceClaimAsync(_user, claim, claim, CancellationToken.None));
                da.Rollback("transaction1");
            }
            _appDb.DbContext.CommandTimeOut = currentTimeOut;
        }

        #region *** Regular claims ***

        [Test]
        public async Task Add_and_Remove_Regular_Claim()
        {
 
            // new claim
            var claim = new Claim("type", "value", "valueType", "issuer");
            await _store.AddClaimsAsync(_user, new Claim[] {claim}, CancellationToken.None);

            // same claim again - should not be added
            await _store.AddClaimsAsync(_user, new Claim[] { claim }, CancellationToken.None);
            var claims = await _store.GetClaimsAsync(_user, CancellationToken.None);
            Assert.AreEqual(1, claims.Count);
            Assert.IsTrue(claims.FirstOrDefault(c => c.Type == claim.Type && c.Value == claim.Value && c.ValueType == claim.ValueType && c.Issuer == claim.Issuer) != null);

            // remove
            await _store.RemoveClaimsAsync(_user, new Claim[] {claim}, CancellationToken.None);
            claims = await _store.GetClaimsAsync(_user, CancellationToken.None);
            Assert.IsTrue(claims.FirstOrDefault(c => c.Type == claim.Type && c.Value == claim.Value) == null);

            // List of claims to remove is empty
            Assert.DoesNotThrowAsync(() => _store.RemoveClaimsAsync(_user, new List<Claim>(), CancellationToken.None));
        }

        [Test]
        public async Task Add_and_Replace_Regular_Claim()
        {
            // add
            var claim = new Claim("type", "value", "valueType", "issuer");
            var newClaim = new Claim("newType", "newValue", "newValueType", "newIssuer");
            await _store.AddClaimsAsync(_user, new Claim[] { claim }, CancellationToken.None);
            Assert.AreEqual(1, (await _store.GetClaimsAsync(_user, CancellationToken.None)).Count);

            // replace
            await _store.ReplaceClaimAsync(_user, claim, newClaim, CancellationToken.None);
            Assert.AreEqual(1, (await _store.GetClaimsAsync(_user, CancellationToken.None)).Count);
            
            // add the original claim again
            await _store.AddClaimsAsync(_user, new Claim[] { claim }, CancellationToken.None);
            Assert.AreEqual(2, (await _store.GetClaimsAsync(_user, CancellationToken.None)).Count);
            // this time, replacing will not happen, because the claim already exists
            await _store.ReplaceClaimAsync(_user, claim, newClaim, CancellationToken.None);
            // replacing a non-existent claim will do nothing
            await _store.ReplaceClaimAsync(_user, new Claim("non-existent", "non-existent"), newClaim, CancellationToken.None);

            var claims = await _store.GetClaimsAsync(_user, CancellationToken.None);
            Assert.IsTrue(claims.Count(c => c.Type == "type" || c.Type == "newType") == 2);
        }

        [Test]
        public async Task Get_Users_For_Regular_Claim()
        {
            var claim = new Claim("otherType", "otherValue", "otherValueType", "otherIssuer");
            await _store.AddClaimsAsync(_user, new Claim[] { claim }, CancellationToken.None);

            var users = await _store.GetUsersForClaimAsync(claim, CancellationToken.None);
            Assert.AreEqual(_user.Email, users.FirstOrDefault()?.Email);

            Assert.ThrowsAsync<ArgumentNullException>(() => _store.GetUsersForClaimAsync(null, CancellationToken.None));
        }

        #endregion

        #region *** Team related claims ***

        [Test]
        public async Task Add_and_Remove_Manager_Claim()
        {
            // new manager claim
            var claim = new Claim(Constants.ClaimType.ManagesTeam, _team.Id.ToString());
            await _store.AddClaimsAsync(_user, new Claim[] { claim }, CancellationToken.None);
            // non-existent team should throw
            Assert.ThrowsAsync<ArgumentException>(() => _store.AddClaimsAsync(_user,
                new [] {new Claim(Constants.ClaimType.ManagesTeam, "0")}, CancellationToken.None));
            // not implemented claim - should throw
            Assert.ThrowsAsync<NotImplementedException>(() => _store.AddClaimsAsync(_user,
                new [] { new Claim(Constants.ClaimType.NotImplementedClaim, _team.Id.ToString()) }, CancellationToken.None));

            // same manager claim again - should not be added
            await _store.AddClaimsAsync(_user, new Claim[] { claim }, CancellationToken.None);
            var claims = await _store.GetClaimsAsync(_user, CancellationToken.None);
            Assert.AreEqual(1, claims.Count);
            Assert.IsTrue(claims.FirstOrDefault(c => c.Type == claim.Type && c.Value == claim.Value && c.ValueType == claim.ValueType && c.Issuer == claim.Issuer) != null);

            // remove manager
            await _store.RemoveClaimsAsync(_user, new Claim[] { claim }, CancellationToken.None);
            claims = await _store.GetClaimsAsync(_user, CancellationToken.None);
            Assert.IsTrue(claims.FirstOrDefault(c => c.Type == claim.Type && c.Value == claim.Value) == null);
            // non-existent team should throw
            Assert.ThrowsAsync<ArgumentException>(() => _store.RemoveClaimsAsync(_user,
                new [] { new Claim(Constants.ClaimType.ManagesTeam, "0") }, CancellationToken.None));
            // not implemented claim - should throw
            Assert.ThrowsAsync<NotImplementedException>(() => _store.RemoveClaimsAsync(_user,
                new [] { new Claim(Constants.ClaimType.NotImplementedClaim, _team.Id.ToString()) }, CancellationToken.None));

            // replace manager should fail
            Assert.ThrowsAsync<ArgumentException>(() => _store.ReplaceClaimAsync(_user, claim, claim, CancellationToken.None));
        }

        [Test]
        public async Task Add_and_Remove_Player_Claim()
        {
            // new player claim
            var claim = new Claim(Constants.ClaimType.PlaysInTeam, _team.Id.ToString());
            await _store.AddClaimsAsync(_user, new Claim[] { claim }, CancellationToken.None);

            // same player claim again - should not be added
            await _store.AddClaimsAsync(_user, new Claim[] { claim }, CancellationToken.None);
            var claims = await _store.GetClaimsAsync(_user, CancellationToken.None);
            Assert.AreEqual(1, claims.Count);
            Assert.IsTrue(claims.FirstOrDefault(c => c.Type == claim.Type && c.Value == claim.Value && c.ValueType == claim.ValueType && c.Issuer == claim.Issuer) != null);

            // remove player
            await _store.RemoveClaimsAsync(_user, new Claim[] { claim }, CancellationToken.None);
            claims = await _store.GetClaimsAsync(_user, CancellationToken.None);
            Assert.IsTrue(claims.FirstOrDefault(c => c.Type == claim.Type && c.Value == claim.Value) == null);
        }

        [Test]
        public async Task Get_Users_For_Manager_Claim()
        {
            var claim = new Claim(Constants.ClaimType.ManagesTeam, _team.Id.ToString());
            await _store.AddClaimsAsync(_user, new Claim[] { claim }, CancellationToken.None);

            // get users for the claim type
            var users = await _store.GetUsersForClaimAsync(claim, CancellationToken.None);
            Assert.AreEqual(_user.Email, users.FirstOrDefault()?.Email);

            // get users for claim type which is not implemented
            Assert.ThrowsAsync<NotImplementedException>(() => _store.GetUsersForClaimAsync(new Claim(Constants.ClaimType.NotImplementedClaim, "1", "z"),
                    CancellationToken.None));
        }

        [Test]
        public async Task Get_Users_For_Player_Claim()
        {
            var claim = new Claim(Constants.ClaimType.PlaysInTeam, _team.Id.ToString());
            await _store.AddClaimsAsync(_user, new Claim[] { claim }, CancellationToken.None);

            // get users for the claim type
            var users = await _store.GetUsersForClaimAsync(claim, CancellationToken.None);
            Assert.AreEqual(_user.Email, users.FirstOrDefault()?.Email);
        }

        #endregion

        #region *** Programmatic claims ***

        [Test]
        public void Add_and_Replace_and_Remove_Programmatic_Claim()
        {
            // Programmatic claims cannot be stored
            var claim = new Claim(Constants.ClaimType.ImpersonatedByUser, "123", "valueType", "issuer");
            Assert.ThrowsAsync<ArgumentException>(() => _store.AddClaimsAsync(_user, new []{claim}, CancellationToken.None));

            // Programmatic claims cannot be replaced
            Assert.ThrowsAsync<ArgumentException>(() => _store.ReplaceClaimAsync(_user, claim, new Claim("type", "value"), CancellationToken.None));
            Assert.ThrowsAsync<ArgumentException>(() => _store.ReplaceClaimAsync(_user, new Claim("type", "value"), claim, CancellationToken.None));

            // Programmatic claims cannot be removed
            Assert.ThrowsAsync<ArgumentException>(() => _store.RemoveClaimsAsync(_user, new []{ claim }, CancellationToken.None));
        }

        #endregion

        [SetUp]
        public async Task Setup()
        {
            // delete all user rows
            await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<UserEntity>(null, CancellationToken.None);

            // delete all team rows
            await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<TeamEntity>(null, CancellationToken.None);

            // create user
            _user = new ApplicationUser {UserName = "UserName", Email = "userclaim@store.test"};
            await _store.CreateAsync(_user, CancellationToken.None);
            _user = await _store.FindByEmailAsync(_user.Email, CancellationToken.None);

            // create team
            _team = new TeamEntity { Name = "Test Team"};
            await _appDb.GenericRepository.SaveEntityAsync(_team, true, false, CancellationToken.None);
        }

        [TearDown]
        public async Task Cleanup()
        {
            // delete all user rows
            await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<UserEntity>(null, CancellationToken.None);

            // delete all team rows
            await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<TeamEntity>(null, CancellationToken.None);
        }
    }
}
