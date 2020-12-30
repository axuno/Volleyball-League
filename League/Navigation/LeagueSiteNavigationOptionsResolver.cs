using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using cloudscribe.Web.Navigation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using TournamentManager.MultiTenancy;

namespace League.Navigation
{
    public class LeagueSiteNavigationOptionsResolver : IOptions<NavigationOptions>
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ITenantContext _tenantContext;

        public LeagueSiteNavigationOptionsResolver(IWebHostEnvironment hostingEnvironment, ITenantContext tenantContext)
        {
            _hostingEnvironment = hostingEnvironment;
            _tenantContext = tenantContext;
        }

        public NavigationOptions Value
        {
            get
            {
                var options = new NavigationOptions();
                // file name must be relative to ContentRootPath!
                var siteXmlFileName = Path.Combine(Program.ConfigurationFolder, "LeagueNavigation" + (_tenantContext.IsDefault ? string.Empty : $".{_tenantContext.Identifier}") + ".config");
                var siteJsonFileName = Path.Combine(Program.ConfigurationFolder, "LeagueNavigation" + (_tenantContext.IsDefault ? string.Empty : $".{_tenantContext.Identifier}") + ".json");

                options.NavigationMapXmlFileName = File.Exists(Path.Combine(_hostingEnvironment.ContentRootPath, siteXmlFileName)) ? siteXmlFileName : null;
                options.NavigationMapJsonFileName = File.Exists(Path.Combine(_hostingEnvironment.ContentRootPath, siteJsonFileName)) ? siteJsonFileName : null;

                return options;
            }
        }
    }
}
