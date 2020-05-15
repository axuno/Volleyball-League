using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Axuno.Tools.GeoSpatial;
using League.Components;
using League.ConfigurationPoco;
using League.DI;
using League.Helpers;
using League.Models.TeamApplicationViewModels;
using League.Models.TeamViewModels;
using League.Models.VenueViewModels;
using League.TagHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.Data;
using TournamentManager.ModelValidators;

namespace League.Controllers
{
    [Route("{organization:ValidOrganizations}/[controller]")]
    public class Venue : AbstractController
    {
        private readonly SiteContext _siteContext;
        private readonly AppDb _appDb;
        private readonly UserManager<Identity.ApplicationUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer<Venue> _localizer;
        private readonly RegionInfo _regionInfo;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Venue> _logger;
        private readonly GoogleConfiguration _googleConfig;
        private readonly string _defaultReturnUrl;

        public Venue(SiteContext siteContext,
            SignInManager<Identity.ApplicationUser> signInManager,
            IAuthorizationService authorizationService, IStringLocalizer<Venue> localizer, RegionInfo regionInfo,
            IConfiguration configuration, ILogger<Venue> logger)
        {
            _defaultReturnUrl = "/";
            _siteContext = siteContext;
            _appDb = siteContext.AppDb;
            _userManager = signInManager.UserManager;
            _authorizationService = authorizationService;
            _localizer = localizer;
            _regionInfo = regionInfo;
            _configuration = configuration;
            _logger = logger;
            _googleConfig = new GoogleConfiguration();
            _configuration.Bind(nameof(ConfigurationPoco.GoogleConfiguration), _googleConfig);
        }

        [HttpGet("[action]/{id:long}")]
        public async Task<IActionResult> Edit(long id, string returnUrl, CancellationToken cancellationToken)
        {
            var venueEntity = (await _appDb.VenueRepository.GetVenuesAsync(
                new PredicateExpression(VenueFields.Id == id),
                cancellationToken)).FirstOrDefault();
            
            if (venueEntity == null)
            {
                return NotFound();
            }

            var venueTeams = await _appDb.VenueRepository.GetVenueTeamRowsAsync(new PredicateExpression(VenueTeamFields.VenueId == venueEntity.Id), cancellationToken);
            if (!(await _authorizationService.AuthorizeAsync(User, venueTeams, Authorization.VenueOperations.EditVenue)).Succeeded)
            {
                return Forbid();
            }

            var model = GetEditModel(false, venueEntity, null,
                venueTeams.Select(vt => vt.TeamName).Distinct().OrderBy(n => n).ToList(), returnUrl);

            model.Venue.MapEntityToFormFields(model.VenueEntity);

            return View(ViewNames.Venue.EditVenue, model);
        }

        [HttpPost("[action]/{*segments}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] VenueEditModel model, CancellationToken cancellationToken)
        {
            var venueEntity = (await _appDb.VenueRepository.GetVenuesAsync(
                new PredicateExpression(VenueFields.Id == model.Venue?.Id),
                cancellationToken)).FirstOrDefault();

            if (venueEntity == null)
            {
                return NotFound();
            }

            var venueTeams = await _appDb.VenueRepository.GetVenueTeamRowsAsync(new PredicateExpression(VenueTeamFields.VenueId == venueEntity.Id), cancellationToken);

            if (!(await _authorizationService.AuthorizeAsync(User, venueTeams, Authorization.VenueOperations.EditVenue)).Succeeded)
            {
                return Forbid();
            }

            model = GetEditModel(false, venueEntity, null,
                venueTeams.Select(vt => vt.TeamName).Distinct().OrderBy(n => n).ToList(), model.ReturnUrl);
            
            model.Venue.MapEntityToFormFields(model.VenueEntity);

            // sync input with new model instance
            if (!await TryUpdateModelAsync(model))
            {
                return View(ViewNames.Venue.EditVenue, model);
            }

            ModelState.Clear();
            model.Venue.MapFormFieldsToEntity(model.VenueEntity);

            var geoResponse = new GoogleGeo.GeoResponse {Success = false};
            if (model.ShouldAutoUpdateLocation())
            {
                geoResponse = await SetupModelForGeoLocation(model, cancellationToken);
            }

            if (!await model.ValidateAsync(
                new VenueValidator(model.VenueEntity, (geoResponse, model.VenuesForDistance)),
                ModelState,
                cancellationToken))
            {
                return View(ViewNames.Venue.EditVenue, model);
            }

            if (await _appDb.GenericRepository.SaveEntityAsync<VenueEntity>(model.VenueEntity, false, false,
                cancellationToken))
            {
                return Redirect(SetAdjustedReturnResult(nameof(Edit), model.ReturnUrl, true));
            }
            
            return View(ViewNames.Venue.EditVenue, model);
        }

        [HttpGet("[action]/{tid:long?}")]
        public async Task<IActionResult> Create(long? tid, string returnUrl, CancellationToken cancellationToken)
        {
            TeamEntity teamEntity = null;
            if (tid.HasValue)
            {
                teamEntity =
                    await _appDb.TeamRepository.GetTeamEntityAsync(new PredicateExpression(TeamFields.Id == tid.Value),
                        cancellationToken);
                
                if (teamEntity == null) return NotFound();
            }

            if (!await CreateIsAuthorized(tid))
            {
                return Forbid();
            }
            
            var model = GetEditModel(true, new VenueEntity(), teamEntity, new string[]{}, returnUrl);

            return View(ViewNames.Venue.EditVenue, model);
        }

        [HttpPost("[action]/{*segments}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] VenueEditModel model, CancellationToken cancellationToken)
        {
            TeamEntity teamEntity = null;
            if (model.TeamId.HasValue)
            {
                teamEntity =
                    await _appDb.TeamRepository.GetTeamEntityAsync(new PredicateExpression(TeamFields.Id == model.TeamId.Value),
                        cancellationToken);
                
                if (teamEntity == null) return NotFound();
            }

            if (!await CreateIsAuthorized(model.TeamId))
            {
                return Forbid();
            }

            model = GetEditModel(true, new VenueEntity(), teamEntity, new string[] { }, model.ReturnUrl);

            model.Venue.MapEntityToFormFields(model.VenueEntity);

            // sync input with new model instance
            if (!await TryUpdateModelAsync(model))
            {
                return View(ViewNames.Venue.EditVenue, model);
            }
            ModelState.Clear();
            model.Venue.MapFormFieldsToEntity(model.VenueEntity);

            var geoResponse = new GoogleGeo.GeoResponse {Success = false};
            if (model.ShouldAutoUpdateLocation())
            {
                geoResponse = await SetupModelForGeoLocation(model, cancellationToken);
            }

            if (!await model.ValidateAsync(
                // with parameter geoResponse == NULL, there is no geo validation
                new VenueValidator(model.VenueEntity, (geoResponse, model.VenuesForDistance)),
                ModelState,
                cancellationToken))
            {
                return View(ViewNames.Venue.EditVenue, model);
            }

            try
            {
                if (teamEntity != null)
                {
                    // Save the new venue for an existing team
                    teamEntity.Venue = model.VenueEntity;

                    return await _appDb.GenericRepository.SaveEntityAsync<TeamEntity>(teamEntity, false, true,
                        cancellationToken)
                        ? Redirect(SetAdjustedReturnResult(nameof(Create), model.ReturnUrl, true))
                        : Redirect(SetAdjustedReturnResult(nameof(Create), model.ReturnUrl, false));
                }
                else
                {
                    // Save the venue (only)
                    return await _appDb.GenericRepository.SaveEntityAsync<VenueEntity>(model.VenueEntity, false, false,
                        cancellationToken)
                        ? Redirect(SetAdjustedReturnResult(nameof(Create), model.ReturnUrl, true))
                        : Redirect(SetAdjustedReturnResult(nameof(Create), model.ReturnUrl, false));
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Failed to save venue.");
                return Redirect(SetAdjustedReturnResult(nameof(Create), model.ReturnUrl, false));
            }
        }
        
        private string SetAdjustedReturnResult(string method, string returnUrl, bool isSuccess)
        {
            if (method.Equals(nameof(Edit)) && returnUrl.Contains(Url.Action(nameof(Team.MyTeam), nameof(Team))))
            {
                TempData.Put<MyTeamMessageModel.MyTeamMessage>(nameof(MyTeamMessageModel.MyTeamMessage),
                    new MyTeamMessageModel.MyTeamMessage
                    {
                        AlertType = isSuccess ? SiteAlertTagHelper.AlertType.Success : SiteAlertTagHelper.AlertType.Danger,
                        MessageId = isSuccess ? MyTeamMessageModel.MessageId.VenueEditSuccess : MyTeamMessageModel.MessageId.VenueEditFailure
                    });

                return returnUrl;
            }

            if (method.Equals(nameof(Create)) && returnUrl.Contains(Url.Action(nameof(Team.MyTeam), nameof(Team))))
            {
                TempData.Put<MyTeamMessageModel.MyTeamMessage>(nameof(MyTeamMessageModel.MyTeamMessage),
                    new MyTeamMessageModel.MyTeamMessage
                    {
                        AlertType = isSuccess ? SiteAlertTagHelper.AlertType.Success : SiteAlertTagHelper.AlertType.Danger,
                        MessageId = isSuccess ? MyTeamMessageModel.MessageId.VenueCreateSuccess : MyTeamMessageModel.MessageId.VenueCreateFailure
                    });

                return returnUrl;
            }

            return _defaultReturnUrl;
        }

        private async Task<GoogleGeo.GeoResponse> SetupModelForGeoLocation(VenueEditModel model, CancellationToken cancellationToken)
        {
            var geoResponse = await model.TrySetGeoLocation(_googleConfig.ServiceApiKey,
                _regionInfo.TwoLetterISORegionName, TimeSpan.FromSeconds(10));
            if (!geoResponse.Success)
            {
                _logger.LogError(geoResponse.Exception,
                    "{0} failed. Response status text: {1}",
                    $"{nameof(VenueEditModel)}.{nameof(VenueEditModel.TrySetGeoLocation)}()",
                    geoResponse.StatusText);
            }
            else
            {
                model.VenuesForDistance = await _appDb.VenueRepository.GetVenuesForDistanceAsync(geoResponse.GeoLocation.LocationType == GoogleGeo.LocationType.RoofTop ? .5 : 2.0,
                    geoResponse.GeoLocation.Longitude.TotalDegrees,
                    geoResponse.GeoLocation.Latitude.TotalDegrees, cancellationToken);
            }

            return geoResponse;
        }

        private async Task<bool> CreateIsAuthorized(long? teamId)
        {
            return (await _authorizationService.AuthorizeAsync(User, new List<VenueTeamRow>(),
                       Authorization.VenueOperations.CreateVenue)).Succeeded
                   && (!teamId.HasValue || (await _authorizationService.AuthorizeAsync(User,
                       new TeamEntity(teamId.Value), Authorization.TeamOperations.EditTeam)).Succeeded);
        }

        private VenueEditModel GetEditModel(bool isNew, VenueEntity venueEntity, TeamEntity teamEntity, IList<string> teamsUsingTheVenue, string returnUrl)
        {
            return new VenueEditModel
            {
                TeamsUsingTheVenue = new string[]{},
                TeamId = teamEntity?.Id,
                ForTeamName = teamEntity?.Name,
                ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : _defaultReturnUrl,
                Venue = new VenueEditorComponentModel {
                    IsNew = isNew,
                    HtmlFieldPrefix = nameof(VenueEditModel.Venue),
                    ShowLatLng = User.IsInRole(League.Identity.Constants.RoleName.SystemManager) ||
                                 User.IsInRole(League.Identity.Constants.RoleName.TournamentManager)
                },
                VenueEntity = venueEntity
            };
        }
    }
}