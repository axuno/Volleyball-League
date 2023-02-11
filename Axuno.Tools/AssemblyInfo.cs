using System;
using System.Diagnostics;
using System.Reflection;

namespace Axuno.Tools;

/// <summary>
/// Retrieve <see cref="Assembly"/> information.
/// </summary>
public class AssemblyInfo
{
    private readonly Assembly _assembly;
    private readonly FileVersionInfo _fileVersionInfo;

    /// <summary>
    /// Creates a new instance for assembly infos about the given <paramref name="forType"/> <see cref="Type"/>.
    /// </summary>
    /// <param name="forType">Any <see cref="Type"/> that is part of the <see cref="Assembly"/></param>
    /// <exception cref="InvalidOperationException">If no assembly can be found.</exception>
    public AssemblyInfo(Type forType)
    {
        var assembly = Assembly.GetAssembly(forType);
        if (assembly == null) throw new InvalidOperationException("No assembly found for the type");
        _assembly = assembly;
        _fileVersionInfo = FileVersionInfo.GetVersionInfo(_assembly.Location);
    }

    /// <summary>
    /// Gets the simple name.
    /// </summary>
    public string? Name => _assembly.GetName().Name;

    /// <summary>
    /// Gets the <see cref="System.Version"/>.
    /// (/Project/PropertyGroup/AssemblyVersion)
    /// </summary>
    public Version? AssemblyVersion => _assembly.GetName().Version;

    /// <summary>
    /// Gets the <see cref="FileVersionInfo.ProductVersion"/>.
    /// (/Project/PropertyGroup/Version). It will be overwritten if (/Project/PropertyGroup/InformationalVersion)
    /// </summary>
    public string? Version => _fileVersionInfo.ProductVersion;

    /// <summary>
    /// Gets the <see cref="FileVersionInfo.FileVersion"/>.
    /// (/Project/PropertyGroup/FileVersion)
    /// </summary>
    public string? FileVersion => _fileVersionInfo.FileVersion;

    /// <summary>
    /// Gets the full name (aka display name).
    /// </summary>
    public string FullName => _assembly.GetName().FullName;

    /// <summary>
    /// Gets the <see cref="AssemblyProductAttribute.Product"/>.
    /// (/Project/PropertyGroup/Product)
    /// </summary>
    public string? Product => _assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;

    /// <summary>
    /// Gets the <see cref="AssemblyInformationalVersionAttribute.InformationalVersion"/>.
    /// (/Project/PropertyGroup/InformationalVersion) or (/Project/PropertyGroup/Version) if not set.
    /// </summary>
    public string? InformationalVersion => _assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
}
