using System.Reflection;

namespace Axuno.Tools.FileSystem;

/// <summary>
/// Provides methods to read embedded resources from assemblies without using an <c>IFileProvider</c>.
/// </summary>
public interface IEmbeddedResourceQuery
{
    /// <summary>
    /// Reads an embedded resource from the assembly of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resource"></param>
    /// <returns></returns>
    Stream? Read<T>(string resource);

    /// <summary>
    /// Reads an embedded resource from the assembly of <paramref name="assembly"/>.
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="resource"></param>
    /// <returns></returns>
    Stream? Read(Assembly assembly, string resource);

    /// <summary>
    /// Reads an embedded resource from the assembly of <paramref name="assemblyName"/>.
    /// </summary>
    /// <param name="assemblyName"></param>
    /// <param name="resource"></param>
    /// <returns></returns>
    Stream? Read(string assemblyName, string resource);

    /// <summary>
    /// Gets all resource names from the assembly of <paramref name="assembly"/>.
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    string[] GetResourceNames(Assembly assembly);

    /// <summary>
    /// Gets all resource names from all pre-loaded assemblies.
    /// </summary>
    /// <returns></returns>
    IEnumerable<string> GetAllResourceNames();
}
