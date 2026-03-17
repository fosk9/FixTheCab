using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PMPRacing.Controllers;

[Authorize(Roles = "mechanic")]
public class MechanicsController : Controller
{
    public IActionResult Index() => View();
}

