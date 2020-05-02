using System;
using System.IO;
using System.Reflection;

namespace League.Test.TestComponents
{
    public class DirectoryLocator
    {
        /// <summary>
        /// Gets the full path to the target project that we wish to test
        /// Assumes that the test project is on the same directory level as the target project
        /// </summary>
        /// <returns>The full path to the target project.</returns>
        public static string GetTargetProjectPath()
        {
            var startupType = typeof(Startup);
            var startupAssembly = startupType.GetTypeInfo().Assembly;

            // Get name of the target project which we want to test
            var projectName = startupAssembly.GetName().Name;

            // Get currently executing test project path
            var applicationBasePath = System.AppContext.BaseDirectory;

            // Find the path to the target project
            var directoryInfo = new DirectoryInfo(applicationBasePath);
            do
            {
                directoryInfo = directoryInfo.Parent;

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
            while (directoryInfo.Parent != null || directoryInfo.Parent.FullName.EndsWith("\\" + projectName));

            throw new Exception($"Project root could not be located using the application root {applicationBasePath}.");
        }

        /// <summary>
        /// Gets the path to the folder that contains configuration files.
        /// </summary>
        /// <returns>Returns the full path to the folder that contains configuration files.</returns>
        public static string GetTargetConfigurationPath()
        {
            return new DirectoryInfo(Path.Combine(GetTargetProjectPath(), Program.ConfigurationFolder)).FullName;
        }
    }
}
