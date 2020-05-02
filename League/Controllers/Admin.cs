using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using League.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace League.Controllers
{
    [Authorize(Roles = Constants.RoleName.SystemManager)]
    [Route("{organization:ValidOrganizations}/[controller]")]
    public class Admin : AbstractController
    {


        [Route("[action]")]
        public IActionResult ShowClaims()
        {
            return View(ViewNames.Admin.ShowClaims);
        }

    }
}
