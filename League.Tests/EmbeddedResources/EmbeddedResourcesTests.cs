using Microsoft.Extensions.FileProviders;
using NUnit.Framework;

namespace League.Tests.EmbeddedResources
{
    [TestFixture]
    public class EmbeddedLeagueWwwRootResourcesTests
    {
        [TestCase("lib/bootstrap/bootstrap.min.css")]
        [TestCase("lib/bootstrap/bootstrap-all.min.js")]
        [TestCase("lib/fontawesome/fontawesome.css")]
        [TestCase("lib/tempus-dominus/tempus-dominus.all.min.js")]
        public void EmbeddedResourceFileProvider_ReturnsFileInfo_WhenFileExists(string file)
        {
            var assembly = typeof(LeagueStartup).Assembly;
            // Embedded wwwroot files are in "League.wwwroot"
            var provider = new EmbeddedFileProvider(assembly, "League.wwwroot");
            
            var fileInfo = provider.GetFileInfo("lib/bootstrap/bootstrap.min.css");

            Assert.Multiple(() =>
            {
                Assert.That(fileInfo.Exists, Is.True, $"Expected embedded file {file} was not found.");
                Assert.That(fileInfo.PhysicalPath, Is.Null, $"Physical file path was {file} instead of null");
            });
        }
    }
}
