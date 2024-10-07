using System.Reflection;

namespace Axuno.Tools.FileSystem;

/// <summary>
/// A class to read embedded resources from assemblies without using an <c>IFileProvider</c>.
/// </summary>
public class EmbeddedResourceQuery : IEmbeddedResourceQuery
{
    private readonly Dictionary<Assembly, string> _assemblyNames;

    /// <summary>
    /// Creates a new instance of <see cref="EmbeddedResourceQuery"/>.
    /// </summary>
    public EmbeddedResourceQuery() : this(Array.Empty<Assembly>())
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="EmbeddedResourceQuery"/> with the assemblies to pre-load.
    /// </summary>
    /// <param name="assembliesToPreLoad"></param>
    public EmbeddedResourceQuery(IEnumerable<Assembly> assembliesToPreLoad)
    {
        _assemblyNames = new Dictionary<Assembly, string>();
        foreach (var assembly in assembliesToPreLoad)
        {
            TryAddAssemblySimpleName(assembly);
        }
    }

    /// <inheritdoc />
    public Stream? Read<T>(string resource)
    {
        var assembly = typeof(T).Assembly;
        return ReadInternal(assembly, resource);
    }

    /// <inheritdoc />
    public Stream? Read(Assembly assembly, string resource)
    {
        return ReadInternal(assembly, resource);
    }

    /// <inheritdoc />
    public Stream? Read(string assemblyName, string resource)
    {
        var assembly = Assembly.Load(assemblyName);
        return ReadInternal(assembly, resource);
    }

    internal Stream? ReadInternal(Assembly assembly, string resource)
    {
        TryAddAssemblySimpleName(assembly);
        return assembly.GetManifestResourceStream($"{_assemblyNames[assembly]}.{resource}");
    }

    private void TryAddAssemblySimpleName(Assembly assembly)
    {
        var simpleName = assembly.GetName().Name;
        if (simpleName != null) _assemblyNames.TryAdd(assembly, simpleName);
    }

    /// <inheritdoc />
    public string[] GetResourceNames(Assembly assembly)
    {
        TryAddAssemblySimpleName(assembly);
        return assembly.GetManifestResourceNames();
    }

    /// <inheritdoc />
    public IEnumerable<string> GetAllResourceNames()
    {
        foreach (var assembly in _assemblyNames.Keys)
        {
            var resourceNames = GetResourceNames(assembly);
            foreach (var resourceName in resourceNames)
            {
                yield return resourceName;
            }
        }
    }
}

