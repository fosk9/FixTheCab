using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMPRacing.Models;
using PMPRacing.ViewModels;

namespace PMPRacing.Controllers;

[Authorize(Roles = "manager")]
public class ManagersController : Controller
{
    private readonly PmpRacingContext _db;

    public ManagersController(PmpRacingContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index() 
    {
        ViewBag.TotalOrders = await _db.ServiceOrders.CountAsync();
        ViewBag.TotalRevenue = await _db.ServiceOrders.Where(o => o.Status == "completed" || o.Status == "paid").SumAsync(o => o.TotalPrice) ?? 0;
        ViewBag.PendingJobs = await _db.ServiceReceipts.CountAsync(r => r.Status == "pending");
        return View();
    }

    // ==========================================
    // UC-03: Quản lý kho phụ tùng (Parts)
    // ==========================================
    [HttpGet]
    public IActionResult Parts() => View();

    [HttpGet]
    public async Task<IActionResult> PartsTable(string search = "")
    {
        var query = _db.Parts.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(p => p.PartName != null && p.PartName.Contains(search));
        }

        var rows = await query
            .OrderBy(p => p.PartId)
            .Select(p => new PartListRowVm
            {
                PartId = p.PartId,
                PartName = p.PartName,
                Price = p.Price ?? 0,
                Stock = p.Stock ?? 0,
                WarningLevel = p.WarningLevel ?? 0
            })
            .ToListAsync();

        return PartialView("Partials/_PartsTable", rows);
    }

    [HttpGet]
    public IActionResult PartCreate()
    {
        return PartialView("Partials/_PartCreate", new PartCreateVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PartCreate(PartCreateVm model)
    {
        if (!ModelState.IsValid)
            return PartialView("Partials/_PartCreate", model);

        var newPart = new Part
        {
            PartName = model.PartName.Trim(),
            Price = model.Price,
            Stock = model.Stock,
            WarningLevel = model.WarningLevel
        };

        _db.Parts.Add(newPart);
        await _db.SaveChangesAsync();

        return Json(new { ok = true });
    }

    [HttpGet]
    public async Task<IActionResult> PartEdit(int id)
    {
        var p = await _db.Parts.AsNoTracking().FirstOrDefaultAsync(x => x.PartId == id);
        if (p == null) return NotFound();

        return PartialView("Partials/_PartEdit", new PartEditVm
        {
            PartId = p.PartId,
            PartName = p.PartName,
            Price = p.Price ?? 0,
            Stock = p.Stock ?? 0,
            WarningLevel = p.WarningLevel ?? 0
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PartEdit(PartEditVm model)
    {
        if (!ModelState.IsValid)
            return PartialView("Partials/_PartEdit", model);

        var p = await _db.Parts.FirstOrDefaultAsync(x => x.PartId == model.PartId);
        if (p == null) return NotFound();

        p.PartName = model.PartName.Trim();
        p.Price = model.Price;
        p.Stock = model.Stock;
        p.WarningLevel = model.WarningLevel;

        await _db.SaveChangesAsync();
        return Json(new { ok = true });
    }

    // ==========================================
    // UC-06: Phân Công (Assignments)
    // ==========================================
    [HttpGet]
    public IActionResult Assignments() => View();

    [HttpGet]
    public async Task<IActionResult> PendingTable()
    {
        var rows = await _db.Set<PendingAssignmentQueue>()
                            .OrderBy(x => x.ReceivedAt)
                            .ToListAsync();
        return PartialView("Partials/_PendingAssignments", rows);
    }

    [HttpGet]
    public async Task<IActionResult> WorkloadTable()
    {
        var rows = await _db.Set<MechanicWorkload>()
                            .OrderByDescending(x => x.ActiveJobCount)
                            .ToListAsync();
        return PartialView("Partials/_MechanicWorkload", rows);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignMechanic(int receiptId, int mechanicId, string note)
    {
        try
        {
            int managerId = 2;
            var claimStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(claimStr, out int parsedId)) managerId = parsedId;

            var paramReceipt = new Microsoft.Data.SqlClient.SqlParameter("@ReceiptId", receiptId);
            var paramMechanic = new Microsoft.Data.SqlClient.SqlParameter("@MechanicId", mechanicId);
            var paramManager = new Microsoft.Data.SqlClient.SqlParameter("@ManagerId", managerId);
            var paramNote = new Microsoft.Data.SqlClient.SqlParameter("@Note", (object)note ?? DBNull.Value);

            await _db.Database.ExecuteSqlRawAsync(
                "EXEC AssignMechanicToReceipt @ReceiptId, @MechanicId, @ManagerId, @Note",
                paramReceipt, paramMechanic, paramManager, paramNote);

            return Json(new { ok = true });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }
    }

    // ==========================================
    // UC-09: Duyệt Nghỉ Phép (LeaveRequests)
    // ==========================================
    [HttpGet]
    public IActionResult LeaveRequests() => View();

    [HttpGet]
    public async Task<IActionResult> LeaveRequestsTable(string status = "pending")
    {
        var query = _db.LeaveRequests.Include(l => l.Employee).Include(l => l.ApprovedByNavigation).AsNoTracking();
        
        status = status.Trim().ToLower();
        query = query.Where(l => l.Status == status);

        var rows = await query.OrderBy(l => l.LeaveDate).ToListAsync();
        return PartialView("Partials/_LeaveRequestsTable", rows);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActOnLeaveRequest(int id, string action)
    {
        try
        {
            var req = await _db.LeaveRequests.FirstOrDefaultAsync(l => l.RequestId == id);
            if (req == null) return NotFound("Không tìm thấy đơn.");

            if (req.Status != "pending") return BadRequest("Đơn này đã được xử lý trước đó.");

            int managerId = 2;
            var claimStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(claimStr, out int parsedId)) managerId = parsedId;

            req.Status = action == "approve" ? "approved" : "rejected";
            req.ApprovedBy = managerId;

            await _db.SaveChangesAsync();
            return Json(new { ok = true });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // ==========================================
    // UC-04: Lịch Làm Việc (Schedules)
    // ==========================================
    [HttpGet]
    public IActionResult Schedules() => View();

    [HttpGet]
    public async Task<IActionResult> SchedulesTable(DateTime? date)
    {
        var targetDate = date ?? DateTime.Today;
        var weekStart = DateOnly.FromDateTime(targetDate.AddDays(-(int)targetDate.DayOfWeek + (int)DayOfWeek.Monday));
        var weekEnd = weekStart.AddDays(6);

        var rows = await _db.WorkSchedules
                            .Include(w => w.Employee)
                            .Where(w => w.WorkDate >= weekStart && w.WorkDate <= weekEnd)
                            .OrderBy(w => w.WorkDate).ThenBy(w => w.Employee.Name)
                            .AsNoTracking()
                            .ToListAsync();

        ViewBag.WeekStart = weekStart;
        ViewBag.WeekEnd = weekEnd;
        return PartialView("Partials/_SchedulesTable", rows);
    }
}
