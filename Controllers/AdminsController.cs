using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PMPRacing.Controllers;

[Authorize(Roles = "admin")]
public class AdminsController : Controller
{
    public IActionResult Index() => View();
}

