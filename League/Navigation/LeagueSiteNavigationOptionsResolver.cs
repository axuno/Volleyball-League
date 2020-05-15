using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using cloudscribe.Web.Navigation;
using League.DI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace League.Navigation
{
    public class LeagueSiteNavigationOptionsResolver : IOptions<NavigationOptions>
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly string _organizationKey;

        public LeagueSiteNavigationOptionsResolver(IWebHostEnvironment hostingEnvironment, SiteContext siteContext)
        {
            _hostingEnvironment = hostingEnvironment;
            _organizationKey = siteContext.OrganizationKey;
        }

        public NavigationOptions Value
        {
            get
            {
                var options = new NavigationOptions();
                // file name must be relative to ContentRootPath!
                var siteXmlFileName = Path.Combine(Program.ConfigurationFolder, "LeagueNavigation" + (string.IsNullOrEmpty(_organizationKey) ? string.Empty : $".{_organizationKey}") + ".config");
                var siteJsonFileName = Path.Combine(Program.ConfigurationFolder, "LeagueNavigation" + (string.IsNullOrEmpty(_organizationKey) ? string.Empty : $".{_organizationKey}") + ".json");

                options.NavigationMapXmlFileName = File.Exists(Path.Combine(_hostingEnvironment.ContentRootPath, siteXmlFileName)) ? siteXmlFileName : null;
                options.NavigationMapJsonFileName = File.Exists(Path.Combine(_hostingEnvironment.ContentRootPath, siteJsonFileName)) ? siteJsonFileName : null;

                return options;
            }
        }
    }
}
