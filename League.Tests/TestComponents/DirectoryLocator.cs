using System;
using System.IO;
using System.Reflection;
#nullable enable
namespace League.Tests.TestComponents;

public class DirectoryLocator
{
    /// <summary>
    /// Gets the full path to the target project that we wish to test
    /// Assumes that the test project is on the same directory level as the target project
    /// </summary>
    /// <returns>The full path to the target project.</returns>
    public static string GetTargetProjectPath(Type classFromTargetAssembly)
    {
        var targetAssembly = classFromTargetAssembly.GetTypeInfo().Assembly;

        // Get name of the target project which we want to test
        var projectName = targetAssembly.GetName().Name ?? throw new Exception("Assembly name is null");

        // Get currently executing test project path
        var applicationBasePath = System.AppContext.BaseDirectory;

        // Find the path to the target project
        var directoryInfo = new DirectoryInfo(applicationBasePath);
        do
        {
            directoryInfo = directoryInfo.Parent;
            if (directoryInfo is null) break;

            var projectDirectoryInfo = new DirectoryInfo(directoryInfo.FullName);
            if (projectDirectoryInfo.Exists)
            {
                var projectFileInfo = new FileInfo(Path.Combine(projectDirectoryInfo.FullName, projectName, $"{projectName}.csproj"));
                if (projectFileInfo.Exists)
                {
                    return Path.Combine(projectDirectoryInfo.FullName, projectName);
                }
            }
        }
        while (directoryInfo.Parent != null);

        throw new Exception($"Project root could not be located using the application root {applicationBasePath}.");
    }

    /// <summary>
    /// Gets the path to the folder that contains configuration files.
    /// </summary>
    /// <returns>Returns the full path to the folder that contains configuration files.</returns>
    public static string GetTargetConfigurationPath()
    {
        return new DirectoryInfo(Path.Combine(GetTargetProjectPath(typeof(League.LeagueStartup)), League.LeagueStartup.ConfigurationFolder)).FullName;
    }
}
