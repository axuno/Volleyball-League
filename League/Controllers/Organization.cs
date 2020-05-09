using System.Net;
using League.DI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.FileProviders;

namespace League.Controllers
{
    /// <summary>
    /// This controller is able to return any organization-specific views from folder ~/Organization/
    /// </summary>
    [Route("{organization:ValidOrganizations}")]
    public class Organization : AbstractController
    {
        // Note:
        // We cannot search a view by 'File.Exists' because this won't work with precompiled views.
        // ViewEngine.FindView will get
        // -> precompiled views
        // -> view files (.cshtml) in case razor runtime compilation is on
        // -> localized view files (e.g.: .de.cshtml) depending on request culture
        // -> view files could also come from other providers, not only PhysicalFileProvider
        private readonly ICompositeViewEngine _viewEngine;
        private readonly ActionContext _actionContext;
        private readonly OrganizationSiteContext _siteContext;

        public Organization(ICompositeViewEngine viewEngine, IActionContextAccessor actionContextAccessor, OrganizationSiteContext siteContext)
        {
            _viewEngine = viewEngine;
            _actionContext = actionContextAccessor.ActionContext;
            _siteContext = siteContext;
        }

        [Route("{section=home}/{content=index}")]
        public IActionResult Index(string section = "home", string content = "index")
        {
            // Note: Indicate the current controller-specific directory ("Organization") with the "./" prefix
            var view = $"./{_siteContext.FolderName}/{section}/{content}";
            var result = _viewEngine.FindView(_actionContext, view, false);
            if(!result.Success) return NotFound();
            
            // give the full absolute view path as argument, so searching the view won't run a second time
            return View(result.View.Path);
        }
    }
}
