using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PMPRacing.Controllers;

[Authorize(Roles = "manager")]
public class ManagersController : Controller
{
    public IActionResult Index() => View();
}

