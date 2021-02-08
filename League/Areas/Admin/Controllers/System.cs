using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using League.Controllers;
using League.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace League.Areas.Admin.Controllers
{
    [Authorize(Roles = Constants.RoleName.SystemManager)]
    [Area("Admin")]
    [Route("{organization:MatchingTenant}/[area]/[controller]")]
    public class System : AbstractController
    {
        [Route("")]
        public IActionResult Index()
        {
            return Content("League.Areas.Admin.Controllers.System.Index");
        }

        [Route("[action]")]
        public IActionResult ShowClaims()
        {
            return View(Views.ViewNames.Area.Admin.System.ShowClaims);
        }

    }
}
