using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PMPRacing.Controllers;

[Authorize(Roles = "cashier")]
public class CashiersController : Controller
{
    public IActionResult Index() => View();
}

