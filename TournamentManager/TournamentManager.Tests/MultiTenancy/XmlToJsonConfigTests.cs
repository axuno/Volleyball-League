using System.Text;
using FluentAssertions;
using NUnit.Framework;
using TournamentManager.JsonCSerializer;
using TournamentManager.MultiTenancy;

namespace TournamentManager.Tests.MultiTenancy;

[TestFixture]
public class XmlToJsonConfigTests
{
    [Test]
    public async Task XmlToJsonConfigTest()
    {
        var tenantContextSerializer = new YAXLib.YAXSerializer<TenantContext?>();
        var xmlConfigPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "SampleTenant.config");
        var xmlDeserialized = tenantContextSerializer.DeserializeFromFile(xmlConfigPath)!;

        var jsonConfigPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "SampleTenant.json");
        var jsonC = JsonCSerializer<TenantContext>.Serialize(xmlDeserialized);
        await File.WriteAllTextAsync(jsonConfigPath, jsonC, Encoding.UTF8);

        var jsonFromFile = await File.ReadAllTextAsync(jsonConfigPath, Encoding.UTF8);
        var jsonDeserialized = JsonCSerializer<TenantContext>.Deserialize(jsonFromFile);
        
        xmlDeserialized.Should().BeEquivalentTo(jsonDeserialized, o => o
            // exclude explicitly unserialized properties
            // that would cause a circular reference for TenantContext or DbContext
            .Excluding(ctx => ctx.SiteContext.Tenant)
            .Excluding(ctx => ctx.OrganizationContext.Tenant)
            .Excluding(ctx => ctx.TournamentContext.Tenant)
            .Excluding(ctx => ctx.DbContext.Tenant)
            .Excluding(ctx => ctx.DbContext.AppDb));
    }
}
