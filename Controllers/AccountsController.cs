using System.Security.Claims;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using PMPRacing.Models;
using PMPRacing.Services.Email;
using PMPRacing.ViewModels;

namespace PMPRacing.Controllers;

public class AccountsController : Controller
{
    private readonly PmpRacingContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IMemoryCache _cache;
    private readonly EmailApi _emailApi;

    public AccountsController(PmpRacingContext db, IWebHostEnvironment env, IMemoryCache cache, EmailApi emailApi)
    {
        _db = db;
        _env = env;
        _cache = cache;
        _emailApi = emailApi;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var email = model.Email.Trim();
        var exists = await _db.Employees.AsNoTracking()
            .AnyAsync(e => e.Email != null && e.Email == email);
        if (exists)
        {
            ModelState.AddModelError(nameof(model.Email), "This email is already in use.");
            return View(model);
        }

        var employee = new Employee
        {
            Name = model.Name.Trim(),
            Email = email,
            Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
            Status = "active",
            Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
            CreatedAt = DateTime.Now
        };

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();
        await EnsurePrimaryRoleAsync(employee.EmployeeId, "manager");

        try
        {
            await _emailApi.SendWelcomeAsync(email, employee.Name, model.Password);
        }
        catch
        {
            // Best effort only: registration should still succeed.
        }

        await RefreshSignInAsync(employee);
        return RedirectToRoleHome(await GetPrimaryRoleAsync(employee.EmployeeId));
    }

    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var email = model.Email.Trim();
        var employee = await _db.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Email != null && e.Email == email);

        if (employee == null || string.IsNullOrWhiteSpace(employee.Password))
        {
            ModelState.AddModelError(string.Empty, "Invalid email, role, or password.");
            return View(model);
        }

        var selectedRole = (model.Role ?? string.Empty).Trim().ToLowerInvariant();
        var dbRole = await GetPrimaryRoleAsync(employee.EmployeeId);
        if (string.IsNullOrWhiteSpace(selectedRole) || selectedRole != dbRole)
        {
            ModelState.AddModelError(string.Empty, "Invalid email, role, or password.");
            return View(model);
        }

        var passwordOk = employee.Password == model.Password;
        if (!passwordOk)
        {
            try
            {
                passwordOk = BCrypt.Net.BCrypt.Verify(model.Password, employee.Password);
            }
            catch
            {
                passwordOk = false;
            }
        }

        if (!passwordOk)
        {
            ModelState.AddModelError(string.Empty, "Invalid email, role, or password.");
            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(employee.Status) &&
            !string.Equals(employee.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, "Your account is not active.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, employee.EmployeeId.ToString()),
            new(ClaimTypes.Name, employee.Name ?? employee.Email ?? "User"),
            new(ClaimTypes.Role, dbRole),
        };

        if (!string.IsNullOrWhiteSpace(employee.Email))
            claims.Add(new Claim(ClaimTypes.Email, employee.Email));

        if (!string.IsNullOrWhiteSpace(employee.ProfileImagePath))
            claims.Add(new Claim("profile_image", employee.ProfileImagePath));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                RedirectUri = model.ReturnUrl
            });

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToRoleHome(dbRole);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var email = model.Email.Trim();
        var employee = await _db.Employees.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Email != null && e.Email == email);

        if (employee != null)
        {
            var otp = Random.Shared.Next(100000, 999999).ToString();
            var expires = TimeSpan.FromMinutes(5);
            _cache.Set(GetResetOtpCacheKey(email), otp, expires);

            try
            {
                await _emailApi.SendOtpAsync(email, otp, expires, "Forgot password");
            }
            catch
            {
                // Avoid account/email enumeration via response details.
            }
        }

        TempData["ResetOtpSent"] = "If the email exists, an OTP has been sent.";
        return RedirectToAction(nameof(ResetPassword), new { email });
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ResetPassword(string? email = null)
    {
        return View(new ResetPasswordViewModel
        {
            Email = string.IsNullOrWhiteSpace(email) ? string.Empty : email.Trim()
        });
    }

    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        model.Email = model.Email.Trim();

        if (!ModelState.IsValid)
            return View(model);

        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Email != null && e.Email == model.Email);
        if (employee == null)
        {
            ModelState.AddModelError(nameof(model.Email), "Invalid email.");
            return View(model);
        }

        if (!_cache.TryGetValue(GetResetOtpCacheKey(model.Email), out string? otp) || string.IsNullOrWhiteSpace(otp))
        {
            ModelState.AddModelError(nameof(model.OtpCode), "OTP is expired or not requested.");
            return View(model);
        }

        if (!string.Equals(model.OtpCode.Trim(), otp, StringComparison.Ordinal))
        {
            ModelState.AddModelError(nameof(model.OtpCode), "Invalid OTP code.");
            return View(model);
        }

        employee.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        await _db.SaveChangesAsync();
        _cache.Remove(GetResetOtpCacheKey(model.Email));

        TempData["PasswordResetSuccess"] = "Password reset successfully. Please log in.";
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Accounts");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var id = GetCurrentEmployeeId();
        var employee = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.EmployeeId == id);
        if (employee == null) return RedirectToAction("Login", "Accounts");
        ViewBag.Role = await GetPrimaryRoleAsync(id);
        return View(employee);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var id = GetCurrentEmployeeId();
        var employee = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.EmployeeId == id);
        if (employee == null) return RedirectToAction("Login", "Accounts");

        return View(new EditProfileViewModel
        {
            EmployeeId = employee.EmployeeId,
            Name = employee.Name,
            Email = employee.Email,
            Phone = employee.Phone,
            ProfileImagePath = employee.ProfileImagePath
        });
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var id = GetCurrentEmployeeId();
        if (model.EmployeeId != id)
            return Forbid();

        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.EmployeeId == id);
        if (employee == null) return RedirectToAction("Login", "Accounts");

        employee.Name = model.Name.Trim();
        var email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        var phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();

        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailExists = await _db.Employees.AsNoTracking()
                .AnyAsync(e => e.EmployeeId != id && e.Email != null && e.Email == email);
            if (emailExists)
            {
                ModelState.AddModelError(nameof(model.Email), "Email is already used by another account.");
                return View(model);
            }
        }

        employee.Email = email;
        employee.Phone = phone;

        if (model.ProfileImageFile is { Length: > 0 })
        {
            var ext = Path.GetExtension(model.ProfileImageFile.FileName).ToLowerInvariant();
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".webp", ".gif" };
            if (!allowed.Contains(ext))
            {
                ModelState.AddModelError(nameof(model.ProfileImageFile), "Only image files (.png, .jpg, .jpeg, .webp, .gif) are allowed.");
                return View(model);
            }

            if (model.ProfileImageFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(nameof(model.ProfileImageFile), "Image must be 5MB or smaller.");
                return View(model);
            }

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "avatars");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{id}_{Guid.NewGuid():N}{ext}";
            var savePath = Path.Combine(uploadsDir, fileName);

            await using (var stream = System.IO.File.Create(savePath))
            {
                await model.ProfileImageFile.CopyToAsync(stream);
            }

            employee.ProfileImagePath = $"~/uploads/avatars/{fileName}";
        }
        else
        {
            employee.ProfileImagePath = string.IsNullOrWhiteSpace(model.ProfileImagePath) ? null : model.ProfileImagePath.Trim();
        }

        await _db.SaveChangesAsync();

        await RefreshSignInAsync(employee);
        return RedirectToAction("Profile");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> ChangePassword()
    {
        var id = GetCurrentEmployeeId();
        var employee = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.EmployeeId == id);
        if (employee == null) return RedirectToAction("Login", "Accounts");

        return View(new ChangePasswordPageViewModel
        {
            Request = new RequestOtpViewModel { EmployeeId = id, Email = employee.Email },
            Change = new ChangePasswordViewModel { EmployeeId = id, Email = employee.Email }
        });
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> SendOtp()
    {
        var id = GetCurrentEmployeeId();
        var employee = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.EmployeeId == id);
        if (employee == null) return RedirectToAction("Login", "Accounts");

        if (string.IsNullOrWhiteSpace(employee.Email))
        {
            TempData["OtpError"] = "Your account does not have an email address.";
            return RedirectToAction("ChangePassword");
        }

        var otp = Random.Shared.Next(100000, 999999).ToString();
        var expires = TimeSpan.FromMinutes(5);
        _cache.Set(GetOtpCacheKey(id), otp, expires);

        try
        {
            await _emailApi.SendOtpAsync(employee.Email, otp, expires, "Change password");
            TempData["OtpSent"] = "OTP has been sent to your email.";
        }
        catch (Exception ex)
        {
            TempData["OtpError"] = ex.Message;
        }

        return RedirectToAction("ChangePassword");
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> ChangePassword([Bind(Prefix = "Change")] ChangePasswordViewModel model)
    {
        var id = GetCurrentEmployeeId();
        if (model.EmployeeId != id) return Forbid();

        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.EmployeeId == id);
        if (employee == null) return RedirectToAction("Login", "Accounts");

        model.Email = employee.Email;

        if (!ModelState.IsValid)
        {
            return View("ChangePassword", new ChangePasswordPageViewModel
            {
                Request = new RequestOtpViewModel { EmployeeId = id, Email = employee.Email },
                Change = model
            });
        }

        if (!_cache.TryGetValue(GetOtpCacheKey(id), out string? otp) || string.IsNullOrWhiteSpace(otp))
        {
            TempData["OtpError"] = "OTP is expired or not requested.";
            return RedirectToAction("ChangePassword");
        }

        if (!string.Equals(model.OtpCode.Trim(), otp, StringComparison.Ordinal))
        {
            TempData["OtpError"] = "Invalid OTP code.";
            return RedirectToAction("ChangePassword");
        }

        employee.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        await _db.SaveChangesAsync();
        _cache.Remove(GetOtpCacheKey(id));

        TempData["OtpSent"] = "Password changed successfully.";
        return RedirectToAction("ChangePassword");
    }

    private int GetCurrentEmployeeId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idStr, out var id) ? id : 0;
    }

    private string GetOtpCacheKey(int employeeId) => $"otp:change_password:{employeeId}";
    private static string GetResetOtpCacheKey(string email) => $"otp:forgot_password:{email.ToLowerInvariant()}";

    private async Task<string> GetPrimaryRoleAsync(int employeeId)
    {
        var role = await _db.UserRoles
            .AsNoTracking()
            .Where(ur => ur.EmployeeId == employeeId)
            .Select(ur => ur.Role.RoleName)
            .FirstOrDefaultAsync();

        var normalized = (role ?? "manager").Trim().ToLowerInvariant();
        return normalized == "cashier" ? "manager" : normalized;
    }

    private async Task EnsurePrimaryRoleAsync(int employeeId, string roleName)
    {
        var normalizedRole = roleName.Trim().ToLowerInvariant();
        if (normalizedRole == "cashier")
            normalizedRole = "manager";
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == normalizedRole);
        if (role == null)
            return;

        var exists = await _db.UserRoles.AnyAsync(ur => ur.EmployeeId == employeeId && ur.RoleId == role.RoleId);
        if (exists)
            return;

        _db.UserRoles.Add(new UserRole
        {
            EmployeeId = employeeId,
            RoleId = role.RoleId
        });
        await _db.SaveChangesAsync();
    }

    private async Task RefreshSignInAsync(Employee employee)
    {
        var role = await GetPrimaryRoleAsync(employee.EmployeeId);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, employee.EmployeeId.ToString()),
            new(ClaimTypes.Name, employee.Name ?? employee.Email ?? "User"),
            new(ClaimTypes.Role, role),
        };

        if (!string.IsNullOrWhiteSpace(employee.Email))
            claims.Add(new Claim(ClaimTypes.Email, employee.Email));

        if (!string.IsNullOrWhiteSpace(employee.ProfileImagePath))
            claims.Add(new Claim("profile_image", employee.ProfileImagePath));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }

    private IActionResult RedirectToRoleHome(string role)
    {
        return role switch
        {
            "admin" => RedirectToAction("Index", "Admins"),
            "manager" => RedirectToAction("Index", "Managers"),
            "cashier" => RedirectToAction("Index", "Managers"),
            "mechanic" => RedirectToAction("Index", "Mechanics"),
            _ => RedirectToAction("Index", "Home")
        };
    }
}

