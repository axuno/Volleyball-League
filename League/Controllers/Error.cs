using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using League.Models.Error;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using TournamentManager.MultiTenancy;

namespace League.Controllers
{
    public class Error : AbstractController
    {
        private readonly ILogger _logger;
        private readonly IStringLocalizer<Error> _localizer;
        private readonly IWebHostEnvironment _environment;
        private readonly ITenantContext _tenantContext;

        public Error(ILogger<Error> logger, IStringLocalizer<Error> localizer, IWebHostEnvironment environment, ITenantContext tenantContext)
        {
            _logger = logger;
            _environment = environment;
            _tenantContext = tenantContext;
            _localizer = localizer;
        }

        [Route("error/{id?}")]
        public IActionResult Index(string id)
        {
            ViewBag.TitleTagText = $"{_tenantContext.OrganizationContext.ShortName} - {_localizer["Error"]}";
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
                _logger.LogCritical(viewModel.Exception, "Path: {origPath}", viewModel.OrigPath);
            }
            else
            {
                viewModel.OrigPath = HttpContext.Features
                    .Get<Microsoft.AspNetCore.Diagnostics.IStatusCodeReExecuteFeature>()?.OriginalPath;
                _logger.LogInformation("Path: {origPath}, StatusCode: {id}", viewModel.OrigPath, id);
            }

            viewModel.StatusCode = id;
            viewModel.StatusText = Models.ErrorViewModels.StatusCodes.ResourceManager.GetString("E"+id) ?? _localizer["Error"];
            viewModel.Description = Models.ErrorViewModels.StatusDescriptions.ResourceManager.GetString("E"+id) ?? _localizer["An error has occured"];

            return View(Views.ViewNames.Error.Index, viewModel);
        }

        [Route("{organization:MatchingTenant}/error/access-denied")]
        public IActionResult AccessDenied(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(Views.ViewNames.Error.AccessDenied);
        }
    }
}
