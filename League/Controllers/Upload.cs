using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using League.Models.UploadViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.MultiTenancy;

namespace League.Controllers
{
    [Route("{organization:MatchingTenant}/[controller]")]
    public class Upload : AbstractController
    {
        private readonly ITenantContext _tenantContext;
        private readonly IWebHostEnvironment _webHostingEnvironment;
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer<Upload> _localizer;
        private readonly ILogger<Upload> _logger;
        private readonly ILoggerFactory _loggerFactory;

        public Upload(ITenantContext tenantContext, IWebHostEnvironment webHostingEnvironment,
            IAuthorizationService authorizationService, IStringLocalizer<Upload> localizer,
            ILoggerFactory loggerFactory)
        {
            _tenantContext = tenantContext;
            _webHostingEnvironment = webHostingEnvironment;
            _authorizationService = authorizationService;
            _localizer = localizer;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<Upload>();
        }


        [HttpGet("team-photo/{id:long}")]
        public async Task<IActionResult> TeamPhoto(long id, CancellationToken cancellationToken)
        {
            if (!(await _authorizationService.AuthorizeAsync(User, new TeamEntity(id),
                Authorization.TeamOperations.ChangePhoto)).Succeeded)
            {
                return Forbid();
            }

            var team = await _tenantContext.DbContext.AppDb.TeamRepository.GetTeamEntityAsync(
                new PredicateExpression(TeamFields.Id == id), cancellationToken);

            if (team == null)
            {
                return NotFound();
            }

            var teamPhoto = new TeamPhotoStaticFile(_webHostingEnvironment, _tenantContext,
                _loggerFactory.CreateLogger<TeamPhotoStaticFile>());

            var model = new TeamPhotoViewModel
            {
                Team = team
            };

            var fi = teamPhoto.GetUriInfo(id);

            if (fi.Uri != null)
            {
                model.PhotoFileUrl = fi.Uri;
                model.PhotoFileDate = fi.Date;
            }
            else
            {
                // shouldn't be the case
                _logger.LogError("Photo file for team id '{0}' not found", id);
                model.PhotoFileUrl = null;
            }

            return View(Views.ViewNames.Upload.TeamPhoto, model);
        }

        [HttpPost("team-photo/{*segments}")]
        public async Task<IActionResult> TeamPhoto([FromForm] IFormFile file, [FromForm] long teamId, CancellationToken cancellationToken)
        {
            if (!(await _authorizationService.AuthorizeAsync(User, new TeamEntity(teamId),
                Authorization.TeamOperations.ChangePhoto)).Succeeded)
            {
                return Forbid();
            }

            var teamInfo =
                await _tenantContext.DbContext.AppDb.TeamRepository.GetTeamEntityAsync(
                    new PredicateExpression(TeamFields.Id == teamId), cancellationToken);
            if (teamInfo == null)
            {
                return NotFound();
            }

            // IIS will return a 404.13 HTTP status code when the uploaded file is bigger than allowed.
            // Set <requestLimits maxAllowedContentLength="1000000000" /> (here: 1GB) in web.config to increase,
            // or set Microsoft.AspNetCore.Http.Features.FormOptions in Startup.
            if (file.Length <= 0)
            {
                Response.StatusCode = 409;
                return Json(new {error = _localizer["Uploaded file is empty"].Value});
            }

            if (file.Length > 5000000)
            {
                Response.StatusCode = 409;
                return Json(new {error = _localizer["Maximum file size is 5 MB"].Value});
            }

            var photoFile = new TeamPhotoStaticFile(_webHostingEnvironment, _tenantContext,
                _loggerFactory.CreateLogger<TeamPhotoStaticFile>());

            var extension =
                Path.GetExtension(ContentDispositionHeaderValue.Parse(file.ContentDisposition)?.FileName.Value
                    ?.ToLowerInvariant());

            if (!new[] {".jpg", ".jpeg", ".png"}.Contains(extension ?? string.Empty))
            {
                Response.StatusCode = 409;
                var msg = _localizer["Uploaded file must be of type JPG, JPEG or PNG"].Value;
                _logger.LogError(msg);
                return Json(new {error = msg});
            }

            try
            {
                var savedFilename =
                    await photoFile.SaveFileAsync(file, extension, teamId, true, cancellationToken);
                if (photoFile.GetFileInfo(teamId).Filename != savedFilename)
                {
                    throw new Exception("Saved filename could not be found");
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "File for team id '{0}' could not be stored", teamId);
                Response.StatusCode = 409;
                return Json(new {error = _localizer["Uploaded file could not be processed"].Value});
            }

            return Json(new
            {
                info = _localizer["Upload completed"].Value, imageUrl = Url.Content(photoFile.GetUriInfo(teamId).Uri)
            });
        }

        [HttpGet("remove-team-photo/{id:long}")]
        public async Task<IActionResult> RemoveTeamPhoto(long id)
        {
            if (!(await _authorizationService.AuthorizeAsync(User, new TeamEntity(id),
                Authorization.TeamOperations.ChangePhoto)).Succeeded)
            {
                return Forbid();
            }

            var photoFile = new TeamPhotoStaticFile(_webHostingEnvironment, _tenantContext,
                _loggerFactory.CreateLogger<TeamPhotoStaticFile>());
            photoFile.DeleteMostRecentFile(id);

            return RedirectToAction(nameof(TeamPhoto), nameof(Upload), new { Organization = _tenantContext.SiteContext.UrlSegmentValue, id});
        }
    }
}