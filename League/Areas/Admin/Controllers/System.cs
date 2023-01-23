using League.Controllers;
using League.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace League.Areas.Admin.Controllers;

[Authorize(Roles = Constants.RoleName.SystemManager)]
[Area("Admin")]
[Route("{organization:MatchingTenant}/[area]/[controller]")]
public class System : AbstractController
{
    [Route("")]
    [HttpGet]
    public IActionResult Index()
    {
        return Content("League.Areas.Admin.Controllers.System.Index");
    }

    [Route("[action]")]
    [HttpGet]
    public IActionResult ShowClaims()
    {
        return View(Views.ViewNames.Area.Admin.System.ShowClaims);
    }

}
