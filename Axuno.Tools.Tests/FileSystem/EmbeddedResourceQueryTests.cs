using System.Text.Json;
using NUnit.Framework;

namespace Axuno.Tools.FileSystem.Tests;

[TestFixture]
public class EmbeddedResourceQueryTests
{
    private readonly EmbeddedResourceQuery _sut = new([typeof(EmbeddedResourceQueryTests).Assembly]);

    [Test]
    public void CanReadEmbeddedResource_Without_PreLoading()
    {
        var sut = new EmbeddedResourceQuery();
        var assembly = typeof(EmbeddedResourceQueryTests).Assembly;
        using var stream = sut.Read(assembly, "FileSystem.Data.my-json-file.json");

        var jsonDocument = JsonDocument.Parse(stream!);
        Assert.That(jsonDocument.RootElement.GetProperty("data").GetBoolean(), Is.True);
    }

    [TestCase("FileSystem.Data.my-json-file.json")]
    [TestCase("FileSystem.Data.data.json")]
    public async Task CanReadEmbeddedResource_Generic(string resource)
    {
        await using var stream = _sut.Read<EmbeddedResourceQueryTests>(resource);

        var jsonDocument = await JsonDocument.ParseAsync(stream!);
        Assert.That(jsonDocument.RootElement.GetProperty("data").GetBoolean(), Is.True);
    }

    [TestCase("FileSystem.Data.my-json-file.json")]
    [TestCase("FileSystem.Data.data.json")]
    public async Task CanReadEmbeddedResource_AssemblyAndResource(string resource)
    {
        var assembly = typeof(EmbeddedResourceQueryTests).Assembly;
        await using var stream = _sut.Read(assembly, resource);

        var jsonDocument = await JsonDocument.ParseAsync(stream!);
        Assert.That(jsonDocument.RootElement.GetProperty("data").GetBoolean(), Is.True);
    }

    [TestCase("FileSystem.Data.my-json-file.json")]
    [TestCase("FileSystem.Data.data.json")]
    public async Task CanReadEmbeddedResource_AssemblyNameAndResource(string resource)
    {
        await using var stream = _sut.Read("Axuno.Tools.Tests", resource);

        var jsonDocument = await JsonDocument.ParseAsync(stream!);
        Assert.That(jsonDocument.RootElement.GetProperty("data").GetBoolean(), Is.True);
    }

    [Test]
    public void CanGetResourceNames()
    {
        var assembly = typeof(EmbeddedResourceQueryTests).Assembly;
        var resourceNames = _sut.GetResourceNames(assembly).ToList();

        Assert.That(resourceNames, Has.Count.EqualTo(2));
        Assert.That(resourceNames, Contains.Item("Axuno.Tools.Tests.FileSystem.Data.my-json-file.json"));
    }

    [Test]
    public void CanGetAllResourceNames()
    {
        var resourceNames = _sut.GetAllResourceNames().ToList();

        Assert.That(resourceNames, Has.Count.EqualTo(2));
        Assert.That(resourceNames, Contains.Item("Axuno.Tools.Tests.FileSystem.Data.my-json-file.json"));
    }
}
