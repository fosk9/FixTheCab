using Microsoft.AspNetCore.Mvc;
using PMPRacing.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace PMPRacing.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                var role = (User.FindFirstValue(ClaimTypes.Role) ?? string.Empty).Trim().ToLowerInvariant();
                return role switch
                {
                    "admin" => RedirectToAction("Index", "Admins"),
                    "manager" => RedirectToAction("Index", "Managers"),
                    "cashier" => RedirectToAction("Index", "Managers"),
                    "mechanic" => RedirectToAction("Index", "Mechanics"),
                    _ => View()
                };
            }

            return RedirectToAction("Login", "Accounts");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
