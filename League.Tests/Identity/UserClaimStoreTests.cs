using System.Data;
using System.Security.Claims;
using League.Identity;
using NUnit.Framework;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.DatabaseSpecific;
using TournamentManager.MultiTenancy;
using Microsoft.AspNetCore.Identity;

namespace League.Tests.Identity;

/// <summary>
/// Integration tests
/// </summary>
[TestFixture]
public class UserClaimStoreTests
{
    private readonly UnitTestHelpers _uth = new();
    private readonly IAppDb _appDb;
    private ApplicationUser _user = new();
    private readonly UserStore _store;
    private TeamEntity _team = new();

    public UserClaimStoreTests()
    {
        _appDb = _uth.GetTenantContext().DbContext.AppDb;
        _store = _uth.GetUserStore();
    }

    [OneTimeTearDown]
    public void DisposeObjects()
    {
        _store.Dispose();
    }

    [Test]
    public void ClaimConstants()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Constants.ClaimType.GetTeamRelatedClaimTypes(), Does.Contain(Constants.ClaimType.ManagesTeam));
            Assert.That(Constants.ClaimType.GetTeamRelatedClaimTypes(), Does.Contain(Constants.ClaimType.PlaysInTeam));
            Assert.That(Constants.ClaimType.GetProgrammaticClaimTypes(), Does.Contain(Constants.ClaimType.ImpersonatedByUser));

            Assert.That(Constants.ClaimType.GetAllClaimTypeValues<string>().Any(cn => cn == Constants.ClaimType.ManagesTeam), Is.True);
            Assert.That(Constants.ClaimType.GetAllClaimTypeValues<string>().Any(cn => cn == Constants.ClaimType.PlaysInTeam), Is.True);
            Assert.That(Constants.ClaimType.GetAllClaimTypeValues<string>().Any(cn => cn == Constants.ClaimType.ImpersonatedByUser), Is.True);

            Assert.That(Constants.ClaimType.GetAllClaimTypeNames(), Does.Contain(nameof(Constants.ClaimType.ManagesTeam)));
            Assert.That(Constants.ClaimType.GetAllClaimTypeNames(), Does.Contain(nameof(Constants.ClaimType.PlaysInTeam)));
            Assert.That(Constants.ClaimType.GetAllClaimTypeNames(), Does.Contain(nameof(Constants.ClaimType.ImpersonatedByUser)));
        });
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
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _store.AddClaimsAsync(_user, new[] { claim }, CancellationToken.None));
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _store.RemoveClaimsAsync(_user, new[] { claim }, CancellationToken.None));
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _store.ReplaceClaimAsync(_user, claim, claim, CancellationToken.None));
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
        await _store.AddClaimsAsync(_user, new[] {claim}, CancellationToken.None);

        // same claim again - should not be added
        await _store.AddClaimsAsync(_user, new[] { claim }, CancellationToken.None);
        var claims = await _store.GetClaimsAsync(_user, CancellationToken.None);
        Assert.That(claims, Has.Count.EqualTo(1));
        Assert.That(claims.FirstOrDefault(c => c.Type == claim.Type && c.Value == claim.Value && c.ValueType == claim.ValueType && c.Issuer == claim.Issuer), Is.Not.EqualTo(null));

        // remove
        await _store.RemoveClaimsAsync(_user, new[] {claim}, CancellationToken.None);
        claims = await _store.GetClaimsAsync(_user, CancellationToken.None);
        Assert.That(claims.FirstOrDefault(c => c.Type == claim.Type && c.Value == claim.Value), Is.EqualTo(null));

        // List of claims to remove is empty
        Assert.DoesNotThrowAsync(() => _store.RemoveClaimsAsync(_user, new List<Claim>(), CancellationToken.None));
    }

    [Test]
    public async Task Add_and_Replace_Regular_Claim()
    {
        // add
        var claim = new Claim("type", "value", "valueType", "issuer");
        var newClaim = new Claim("newType", "newValue", "newValueType", "newIssuer");
        await _store.AddClaimsAsync(_user, new[] { claim }, CancellationToken.None);
        Assert.That((await _store.GetClaimsAsync(_user, CancellationToken.None)), Has.Count.EqualTo(1));

        // replace
        await _store.ReplaceClaimAsync(_user, claim, newClaim, CancellationToken.None);
        Assert.That((await _store.GetClaimsAsync(_user, CancellationToken.None)), Has.Count.EqualTo(1));
            
        // add the original claim again
        await _store.AddClaimsAsync(_user, new[] { claim }, CancellationToken.None);
        Assert.That((await _store.GetClaimsAsync(_user, CancellationToken.None)), Has.Count.EqualTo(2));
        // this time, replacing will not happen, because the claim already exists
        await _store.ReplaceClaimAsync(_user, claim, newClaim, CancellationToken.None);
        // replacing a non-existent claim will do nothing
        await _store.ReplaceClaimAsync(_user, new Claim("non-existent", "non-existent"), newClaim, CancellationToken.None);

        var claims = await _store.GetClaimsAsync(_user, CancellationToken.None);
        Assert.That(claims.Count(c => c.Type == "type" || c.Type == "newType"), Is.EqualTo(2));
    }

    [Test]
    public async Task Get_Users_For_Regular_Claim()
    {
        var claim = new Claim("otherType", "otherValue", "otherValueType", "otherIssuer");
        await _store.AddClaimsAsync(_user, new[] { claim }, CancellationToken.None);

        var users = await _store.GetUsersForClaimAsync(claim, CancellationToken.None);
        Assert.That(users.FirstOrDefault()?.Email, Is.EqualTo(_user.Email));
    }

    #endregion

    #region *** Team related claims ***

    [Test]
    public async Task Add_and_Remove_Manager_Claim()
    {
        // new manager claim
        var claim = new Claim(Constants.ClaimType.ManagesTeam, _team.Id.ToString());
        await _store.AddClaimsAsync(_user, new[] { claim }, CancellationToken.None);
        // non-existent team should throw
        Assert.ThrowsAsync<InvalidOperationException>(() => _store.AddClaimsAsync(_user,
            new [] {new Claim(Constants.ClaimType.ManagesTeam, "0")}, CancellationToken.None));
        // not implemented claim - should throw
        Assert.ThrowsAsync<InvalidOperationException>(() => _store.AddClaimsAsync(_user,
            new [] { new Claim(Constants.ClaimType.NotImplementedClaim, _team.Id.ToString()) }, CancellationToken.None));

        // same manager claim again - should not be added
        await _store.AddClaimsAsync(_user, new[] { claim }, CancellationToken.None);
        var claims = await _store.GetClaimsAsync(_user, CancellationToken.None);
        Assert.That(claims, Has.Count.EqualTo(1));
        Assert.That(claims.FirstOrDefault(c => c.Type == claim.Type && c.Value == claim.Value && c.ValueType == claim.ValueType && c.Issuer == claim.Issuer), Is.Not.EqualTo(null));

        // remove manager
        await _store.RemoveClaimsAsync(_user, new[] { claim }, CancellationToken.None);
        claims = await _store.GetClaimsAsync(_user, CancellationToken.None);
        Assert.That(claims.FirstOrDefault(c => c.Type == claim.Type && c.Value == claim.Value), Is.EqualTo(null));
        // non-existent team should throw
        Assert.ThrowsAsync<InvalidOperationException>(() => _store.RemoveClaimsAsync(_user,
            new [] { new Claim(Constants.ClaimType.ManagesTeam, "0") }, CancellationToken.None));
        // not implemented claim - should throw
        Assert.ThrowsAsync<InvalidOperationException>(() => _store.RemoveClaimsAsync(_user,
            new [] { new Claim(Constants.ClaimType.NotImplementedClaim, _team.Id.ToString()) }, CancellationToken.None));

        // replace manager should fail
        Assert.ThrowsAsync<ArgumentException>(() => _store.ReplaceClaimAsync(_user, claim, claim, CancellationToken.None));
    }

    [Test]
    public async Task Add_and_Remove_Player_Claim()
    {
        // new player claim
        var claim = new Claim(Constants.ClaimType.PlaysInTeam, _team.Id.ToString());
        await _store.AddClaimsAsync(_user, new[] { claim }, CancellationToken.None);

        // same player claim again - should not be added
        await _store.AddClaimsAsync(_user, new[] { claim }, CancellationToken.None);
        var claims = await _store.GetClaimsAsync(_user, CancellationToken.None);
        Assert.That(claims, Has.Count.EqualTo(1));
        Assert.That(claims.FirstOrDefault(c => c.Type == claim.Type && c.Value == claim.Value && c.ValueType == claim.ValueType && c.Issuer == claim.Issuer), Is.Not.EqualTo(null));

        // remove player
        await _store.RemoveClaimsAsync(_user, new[] { claim }, CancellationToken.None);
        claims = await _store.GetClaimsAsync(_user, CancellationToken.None);
        Assert.That(claims.FirstOrDefault(c => c.Type == claim.Type && c.Value == claim.Value), Is.EqualTo(null));
    }

    [Test]
    public async Task Get_Users_For_Manager_Claim()
    {
        var claim = new Claim(Constants.ClaimType.ManagesTeam, _team.Id.ToString());
        await _store.AddClaimsAsync(_user, new[] { claim }, CancellationToken.None);

        // get users for the claim type
        var users = await _store.GetUsersForClaimAsync(claim, CancellationToken.None);
        Assert.That(users.FirstOrDefault()?.Email, Is.EqualTo(_user.Email));

        // get users for claim type which is not implemented
        Assert.ThrowsAsync<NotImplementedException>(() => _store.GetUsersForClaimAsync(new Claim(Constants.ClaimType.NotImplementedClaim, "1", "z"),
            CancellationToken.None));
    }

    [Test]
    public async Task Get_Users_For_Player_Claim()
    {
        var claim = new Claim(Constants.ClaimType.PlaysInTeam, _team.Id.ToString());
        await _store.AddClaimsAsync(_user, new[] { claim }, CancellationToken.None);

        // get users for the claim type
        var users = await _store.GetUsersForClaimAsync(claim, CancellationToken.None);
        Assert.That(users.FirstOrDefault()?.Email, Is.EqualTo(_user.Email));
    }

    #endregion

    #region *** Programmatic claims ***

    [Test]
    public void Add_and_Replace_and_Remove_Programmatic_Claim()
    {
        // Programmatic claims cannot be stored
        var claim = new Claim(Constants.ClaimType.ImpersonatedByUser, "123", "valueType", "issuer");
        Assert.ThrowsAsync<InvalidOperationException>(() => _store.AddClaimsAsync(_user, new []{claim}, CancellationToken.None));

        // Programmatic claims cannot be replaced
        Assert.ThrowsAsync<ArgumentException>(() => _store.ReplaceClaimAsync(_user, claim, new Claim("type", "value"), CancellationToken.None));
        Assert.ThrowsAsync<ArgumentException>(() => _store.ReplaceClaimAsync(_user, new Claim("type", "value"), claim, CancellationToken.None));

        // Programmatic claims cannot be removed
        Assert.ThrowsAsync<InvalidOperationException>(() => _store.RemoveClaimsAsync(_user, new []{ claim }, CancellationToken.None));
    }

    #endregion

    [SetUp]
    public async Task Setup()
    {
        // delete all user rows
        await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<UserEntity>(new PredicateExpression(), CancellationToken.None);

        // delete all team rows
        await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<TeamEntity>(new PredicateExpression(), CancellationToken.None);

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
        await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<UserEntity>(new PredicateExpression(), CancellationToken.None);

        // delete all team rows
        await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<TeamEntity>(new PredicateExpression(), CancellationToken.None);
    }
}
