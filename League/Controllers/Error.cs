using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using League.DI;
using League.Models.Error;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

namespace League.Controllers
{
    public class Error : AbstractController
    {
        private readonly ILogger _logger;
        private readonly IStringLocalizer<Error> _localizer;
        private readonly IWebHostEnvironment _environment;
        private readonly OrganizationSiteContext _siteContext;

        public Error(ILogger<Error> logger, IStringLocalizer<Error> localizer, IWebHostEnvironment environment, OrganizationSiteContext siteContext)
        {
            _logger = logger;
            _environment = environment;
            _siteContext = siteContext;
            _localizer = localizer;
        }

        [Route("error/{id?}")]
        public IActionResult Index(string id)
        {
            ViewBag.TitleTagText = $"{_siteContext.ShortName} - {_localizer["Error"]}";
            id ??= string.Empty;
            id = id.Trim();

            var viewModel = new ErrorViewModel();

            // The StatusCodePagesMiddleware stores a request-feature with
            // the original path on the HttpContext, that can be accessed from the Features property.
            // Note: IExceptionHandlerFeature does not contain the path
            var exceptionFeature = HttpContext.Features
                .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();

            if (exceptionFeature?.Error != null)
            {
                viewModel.OrigPath = exceptionFeature?.Path;
                viewModel.Exception = exceptionFeature?.Error;
                _logger.LogCritical(viewModel.Exception, "Path: {0}", viewModel.OrigPath);
            }
            else
            {
                viewModel.OrigPath = HttpContext.Features
                    .Get<Microsoft.AspNetCore.Diagnostics.IStatusCodeReExecuteFeature>()?.OriginalPath;
                _logger.LogInformation("Path: {0}, StatusCode: {1}", viewModel.OrigPath, id);
            }

            viewModel.StatusCode = id;
            viewModel.StatusText = Models.ErrorViewModels.StatusCodes.ResourceManager.GetString("E"+id) ?? _localizer["Error"];
            viewModel.Description = Models.ErrorViewModels.StatusDescriptions.ResourceManager.GetString("E"+id) ?? _localizer["An error has occured"];

            return View(ViewNames.Error.Index, viewModel);
        }

        [Route("{organization:ValidOrganizations}/error/access-denied")]
        public IActionResult AccessDenied(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(ViewNames.Error.AccessDenied);
        }
    }
}