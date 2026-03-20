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

        // Last 30 days revenue
        var past30Days = DateTime.Today.AddDays(-29);
        
        var revenueDataRaw = await _db.ServiceOrders
            .Where(o => (o.Status == "completed" || o.Status == "paid") && o.CreatedAt >= past30Days)
            .Select(o => new { o.CreatedAt, o.TotalPrice })
            .ToListAsync();

        var revenueData = revenueDataRaw
            .Where(o => o.CreatedAt.HasValue)
            .GroupBy(o => o.CreatedAt.Value.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(x => x.TotalPrice) })
            .ToList();

        var labels = new List<string>();
        var data = new List<decimal>();

        for(int i = 0; i < 30; i++)
        {
            var d = past30Days.AddDays(i);
            labels.Add(d.ToString("dd/MM"));
            var dayTotal = revenueData.FirstOrDefault(x => x.Date == d)?.Total ?? 0;
            data.Add(dayTotal);
        }

        ViewBag.ChartLabels = labels;
        ViewBag.ChartData = data;

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
        var activeStatuses = new[] { "assigned", "in_progress", "processing" };
        
        var mechanics = await _db.Employees
                            .Where(e => e.Role == "mechanic" && e.Status == "active")
                            .AsNoTracking()
                            .ToListAsync();

        var activeJobs = await _db.MechanicAssignments
                            .Include(ma => ma.Receipt)
                            .Where(ma => ma.Receipt != null && activeStatuses.Contains(ma.Receipt.Status))
                            .GroupBy(ma => ma.MechanicId)
                            .Select(g => new { MechanicId = g.Key, Count = g.Count() })
                            .ToListAsync();

        var rows = mechanics.Select(m => new MechanicWorkload {
            EmployeeId = m.EmployeeId,
            MechanicName = m.Name,
            ActiveJobCount = activeJobs.FirstOrDefault(j => j.MechanicId == m.EmployeeId)?.Count ?? 0
        }).OrderByDescending(x => x.ActiveJobCount).ToList();

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
    [HttpGet]
    public async Task<IActionResult> SeedSchedules()
    {
        var today = DateTime.Today;
        var weekStart = DateOnly.FromDateTime(today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday));
        
        var employees = await _db.Employees.Where(e => e.Role == "mechanic" || e.Role == "manager").Take(5).ToListAsync();
        if (!employees.Any()) return Content("Không tìm thấy nhân viên nào");

        int count = 0;
        foreach(var emp in employees)
        {
            for(int i = 0; i < 5; i++)
            {
                var targetDate = weekStart.AddDays(i);
                bool exists = await _db.WorkSchedules.AnyAsync(w => w.EmployeeId == emp.EmployeeId && w.WorkDate == targetDate);
                if (!exists) 
                {
                    _db.WorkSchedules.Add(new WorkSchedule {
                        EmployeeId = emp.EmployeeId,
                        WorkDate = targetDate,
                        Shift = i % 2 == 0 ? "morning" : "afternoon",
                        CheckIn = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, 8, 0, 0),
                        CheckOut = i < 3 ? new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, 17, 30, 0) : null
                    });
                    count++;
                }
            }
        }
        await _db.SaveChangesAsync();
        return Content($"Đã tạo thành công {count} bản ghi lịch làm việc mẫu cho tuần này!");
    }
    [HttpGet]
    public async Task<IActionResult> SeedLeaves()
    {
        var employees = await _db.Employees.Where(e => e.Role == "mechanic" || e.Role == "cashier").Take(5).ToListAsync();
        if (!employees.Any()) return Content("Không tìm thấy nhân viên.");

        int count = 0;
        var statuses = new[] { "pending", "approved", "rejected" };
        var reasons = new[] { "Nghỉ ốm", "Việc gia đình", "Du lịch", "Khám sức khỏe", "Nghỉ phép năm" };
        var rand = new Random();

        foreach(var emp in employees)
        {
            for(int i = 0; i < 4; i++) // 4 per employee = 20 total
            {
                var targetDate = DateOnly.FromDateTime(DateTime.Today.AddDays(rand.Next(-10, 10)));
                _db.LeaveRequests.Add(new LeaveRequest {
                    EmployeeId = emp.EmployeeId,
                    LeaveDate = targetDate,
                    Reason = reasons[rand.Next(reasons.Length)],
                    Status = statuses[rand.Next(statuses.Length)]
                });
                count++;
            }
        }
        await _db.SaveChangesAsync();
        return Content($"Đã tạo thành công {count} đơn nghỉ phép mẫu!");
    }

    [HttpGet]
    public async Task<IActionResult> SeedAssignments()
    {
        var customer = await _db.Customers.FirstOrDefaultAsync();
        var bike = await _db.Bikes.FirstOrDefaultAsync();
        var cashier = await _db.Employees.FirstOrDefaultAsync(e => e.Role == "cashier" || e.Role == "manager");
        
        if (customer == null || bike == null || cashier == null) return Content("Thiếu Master Data (Khách, Xe, Thu ngân). Bắt buộc phải có sẵn trong database mới tạo được.");

        int count = 0;
        for(int i = 0; i < 15; i++)
        {
            _db.ServiceReceipts.Add(new ServiceReceipt {
                BikeId = bike.BikeId,
                CreatedBy = cashier.EmployeeId,
                Status = "pending",
                CreatedAt = DateTime.Now.AddHours(-i)
            });
            count++;
        }
        await _db.SaveChangesAsync();
        return Content($"Đã tạo thành công {count} xe chờ phân công!");
    }

    [HttpGet]
    public async Task<IActionResult> SeedRevenue()
    {
        var mechanic = await _db.Employees.FirstOrDefaultAsync(e => e.Role == "mechanic");
        var receipt = await _db.ServiceReceipts.FirstOrDefaultAsync();
        
        if (mechanic == null || receipt == null) 
            return Content("Thiếu Master Data (Thợ hoặc Phiếu tiếp nhận) để tạo doanh thu mẫu.");

        int count = 0;
        var rand = new Random();
        for (int i = 0; i < 30; i++)
        {
            // Create 1-4 random orders per day
            int ordersPerDay = rand.Next(1, 5); 
            for (int j = 0; j < ordersPerDay; j++)
            {
                var date = DateTime.Today.AddDays(-i).AddHours(rand.Next(8, 18));
                _db.ServiceOrders.Add(new ServiceOrder
                {
                    ReceiptId = receipt.ReceiptId,
                    MechanicId = mechanic.EmployeeId,
                    Status = "paid",
                    TotalPrice = rand.Next(150, 1500) * 1000, // 150k to 1.5M
                    CreatedAt = date,
                    StartedAt = date.AddMinutes(-30),
                    CompletedAt = date.AddMinutes(rand.Next(20, 60))
                });
                count++;
            }
        }
        await _db.SaveChangesAsync();
        return Content($"Đã tạo thành công {count} đơn hàng mẫu trong 30 ngày qua!");
    }
}
