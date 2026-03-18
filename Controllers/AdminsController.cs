using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.SignalR;
using PMPRacing.Hubs;
using PMPRacing.Models;
using PMPRacing.Services.Email;
using PMPRacing.ViewModels;

namespace PMPRacing.Controllers;

[Authorize(Roles = "admin")]
public class AdminsController : Controller
{
    private readonly PmpRacingContext _db;
    private readonly IMemoryCache _cache;
    private readonly EmailApi _emailApi;
    private readonly IHubContext<AdminAccountsHub> _hub;

    public AdminsController(PmpRacingContext db, IMemoryCache cache, EmailApi emailApi, IHubContext<AdminAccountsHub> hub)
    {
        _db = db;
        _cache = cache;
        _emailApi = emailApi;
        _hub = hub;
    }

    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult Accounts() => View();

    [HttpGet]
    public async Task<IActionResult> AccountsTable(string search = "", string role = "", string status = "")
    {
        var query = _db.Employees.AsNoTracking();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(e => 
                e.Name.Contains(search) ||
                (e.Email != null && e.Email.Contains(search)) ||
                (e.Phone != null && e.Phone.Contains(search)));
        }

        // Apply role filter
        if (!string.IsNullOrWhiteSpace(role))
        {
            role = role.Trim().ToLowerInvariant();
            query = query.Where(e => e.Role != null && e.Role == role);
        }

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(status))
        {
            status = status.Trim().ToLowerInvariant();
            query = query.Where(e => e.Status != null && e.Status == status);
        }

        var rows = await query
            .OrderBy(e => e.EmployeeId)
            .Select(e => new AdminAccountRowVm
            {
                EmployeeId = e.EmployeeId,
                Name = e.Name,
                Email = e.Email,
                Phone = e.Phone,
                Role = e.Role,
                Status = e.Status,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();

        return PartialView("Partials/_AccountsTable", rows);
    }

    [HttpGet]
    public async Task<IActionResult> AccountDetails(int id)
    {
        var employee = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.EmployeeId == id);
        if (employee == null) return NotFound();
        return PartialView("Partials/_AccountDetails", employee);
    }

    [HttpGet]
    public async Task<IActionResult> AccountEdit(int id)
    {
        var e = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.EmployeeId == id);
        if (e == null) return NotFound();
        return PartialView("Partials/_AccountEdit", new AdminEditAccountVm
        {
            EmployeeId = e.EmployeeId,
            Name = e.Name,
            Email = e.Email,
            Phone = e.Phone,
            Role = e.Role ?? "cashier",
            Status = e.Status ?? "active",
            ProfileImagePath = e.ProfileImagePath
        });
    }

    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> AccountEdit(AdminEditAccountVm model)
    {
        if (!ModelState.IsValid) return PartialView("Partials/_AccountEdit", model);

        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.EmployeeId == model.EmployeeId);
        if (employee == null) return NotFound();

        employee.Name = model.Name.Trim();
        employee.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        employee.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
        employee.Role = model.Role.Trim().ToLowerInvariant();
        employee.Status = model.Status.Trim().ToLowerInvariant();
        employee.ProfileImagePath = string.IsNullOrWhiteSpace(model.ProfileImagePath) ? null : model.ProfileImagePath.Trim();

        if (!string.IsNullOrWhiteSpace(model.NewPassword))
            employee.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

        await _db.SaveChangesAsync();
        await _hub.Clients.All.SendAsync("accountsChanged");
        return Json(new { ok = true });
    }

    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> AccountDisable(int id)
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.EmployeeId == id);
        if (employee == null) return NotFound();
        employee.Status = "inactive";
        await _db.SaveChangesAsync();
        await _hub.Clients.All.SendAsync("accountsChanged");
        return Json(new { ok = true });
    }

    [HttpGet]
    public IActionResult AccountCreate() => PartialView("Partials/_AccountCreate");

    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> SendCreateOtp(string email)
    {
        try
        {
            email = (email ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(email)) 
                return BadRequest("Email is required.");

            if (!IsValidEmail(email))
                return BadRequest("Invalid email format.");

            var exists = await _db.Employees.AsNoTracking().AnyAsync(e => e.Email != null && e.Email == email);
            if (exists) 
                return BadRequest("Email already exists.");

            var otp = Random.Shared.Next(100000, 999999).ToString();
            var expires = TimeSpan.FromMinutes(5);
            _cache.Set(GetCreateOtpKey(email), otp, expires);

            await _emailApi.SendOtpAsync(email, otp, expires, "Create account");
            return Json(new { ok = true });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to send OTP: {ex.Message}");
        }
    }

    [ValidateAntiForgeryToken]
    [HttpPost]
    public IActionResult VerifyCreateOtp(AdminCreateAccountStep1Vm model)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest("Invalid data.");

            if (!_cache.TryGetValue(GetCreateOtpKey(model.Email.Trim()), out string? otp) || string.IsNullOrWhiteSpace(otp))
                return BadRequest("OTP expired or not requested.");

            if (!string.Equals(model.OtpCode.Trim(), otp, StringComparison.Ordinal))
                return BadRequest("Invalid OTP.");

            return Json(new { ok = true });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to verify OTP: {ex.Message}");
        }
    }

    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> AccountCreate(AdminCreateAccountVm model)
    {
        try
        {
            if (!ModelState.IsValid) 
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(string.Join("; ", errors));
            }

            var email = model.Email.Trim();
            var exists = await _db.Employees.AsNoTracking().AnyAsync(e => e.Email != null && e.Email == email);
            if (exists)
            {
                return BadRequest("Email already exists.");
            }

            // Require OTP previously verified (still present in cache)
            if (!_cache.TryGetValue(GetCreateOtpKey(email), out string? otp) || string.IsNullOrWhiteSpace(otp))
            {
                return BadRequest("OTP expired. Please request OTP again.");
            }

            var employee = new Employee
            {
                Name = model.Name.Trim(),
                Email = email,
                Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
                Role = model.Role.Trim().ToLowerInvariant(),
                Status = model.Status.Trim().ToLowerInvariant(),
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                CreatedAt = DateTime.Now
            };

            _db.Employees.Add(employee);
            await _db.SaveChangesAsync();
            _cache.Remove(GetCreateOtpKey(email));

            try
            {
                await _emailApi.SendWelcomeAsync(email, employee.Name, model.Password);
            }
            catch
            {
                // best-effort: do not block account creation if email fails
            }

            await _hub.Clients.All.SendAsync("accountsChanged");
            return Json(new { ok = true });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to create account: {ex.Message}");
        }
    }

    private static string GetCreateOtpKey(string email) => $"otp:create_account:{email.ToLowerInvariant()}";

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

