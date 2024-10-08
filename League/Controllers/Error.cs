﻿using League.Models.Error;
using League.Routing;
using TournamentManager.MultiTenancy;

namespace League.Controllers;

[Route("[controller]")]
public class Error : AbstractController
{
    private readonly ILogger _logger;
    private readonly ILogger _loggerNotFound; //NOSONAR
    private readonly IStringLocalizer<Error> _localizer;
    private readonly ITenantContext _tenantContext;

    public Error(ILogger<Error> logger, IStringLocalizer<Error> localizer, ILoggerFactory loggerFactory, ITenantContext tenantContext)
    {
        _logger = logger;
        _tenantContext = tenantContext;
        _localizer = localizer;
        _loggerNotFound = loggerFactory.CreateLogger(nameof(League) + ".NotFound");
    }

    [Route("{id?}")]
    [HttpGet]
    public IActionResult Index(string? id)
    {
        if (!ModelState.IsValid) return BadRequest();

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
            viewModel.OrigPath = exceptionFeature.Path;
            viewModel.Exception = exceptionFeature.Error;
            _logger.LogError(viewModel.Exception, "Path: {OrigPath}", viewModel.OrigPath);
        }
        else
        {
            viewModel.OrigPath = HttpContext.Features
                .Get<Microsoft.AspNetCore.Diagnostics.IStatusCodeReExecuteFeature>()?.OriginalPath ?? string.Empty;

            if (Response.StatusCode == 404)
                _loggerNotFound.LogInformation("{NotFound}", new {Status = Response.StatusCode, Ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1", Path = viewModel.OrigPath});
            else
                _logger.LogWarning("StatusCode: {StatusCode}, Path: {OrigPath}", Response.StatusCode, viewModel.OrigPath);
        }

        viewModel.StatusCode = id;
        viewModel.StatusText = Models.ErrorViewModels.StatusCodes.ResourceManager.GetString("E"+id) ?? _localizer["Error"];
        viewModel.Description = Models.ErrorViewModels.StatusDescriptions.ResourceManager.GetString("E"+id) ?? _localizer["An error has occured"];

        return View(Views.ViewNames.Error.Index, viewModel);
    }

    [Route("/" + TenantRouteConstraint.Template + "/[controller]/access-denied")]
    [HttpGet]
    public IActionResult AccessDenied(string returnUrl)
    {
        if (!ModelState.IsValid) return View(Views.ViewNames.Error.AccessDenied);

        ViewData["ReturnUrl"] = returnUrl;
        return View(Views.ViewNames.Error.AccessDenied);
    }
}
