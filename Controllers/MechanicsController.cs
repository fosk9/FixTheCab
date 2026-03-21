using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMPRacing.Models;
using Microsoft.AspNetCore.SignalR;
using PMPRacing.Hubs;
using PMPRacing.ViewModels;
using System.Security.Claims;

namespace PMPRacing.Controllers;

[Authorize(Roles = "mechanic")]
public class MechanicsController : Controller
{
    private readonly PmpRacingContext _db;
    private readonly IHubContext<AdminAccountsHub> _hub;

    public MechanicsController(PmpRacingContext db, IHubContext<AdminAccountsHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    public async Task<IActionResult> Index()
    {
        var mechanicIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(mechanicIdStr, out int mechanicId)) return Unauthorized();

        var model = new MechanicDashboardVm();

        // 1. Xe mới được giao (Pending Start)
        model.IncomingJobs = await _db.MechanicAssignments
            .Include(a => a.Receipt)
            .ThenInclude(r => r.Bike)
            .Where(a => a.MechanicId == mechanicId && a.Receipt.Status == "assigned")
            .Select(a => new AssignedJobVm
            {
                ReceiptId = a.ReceiptId,
                CustomerName = a.Receipt.Bike != null && a.Receipt.Bike.Customer != null ? a.Receipt.Bike.Customer.Name : "Khách lẻ",
                BikeModel = a.Receipt.Bike != null ? a.Receipt.Bike.BikeModel ?? "" : "",
                LicensePlate = a.Receipt.Bike != null ? a.Receipt.Bike.LicensePlate ?? "" : "",
                ReceivedAt = a.Receipt.CreatedAt ?? DateTime.Now,
                Note = a.Note
            })
            .ToListAsync();

        // 2. Xe đang sửa (Processing)
        model.ProcessingJobs = await _db.ServiceOrders
            .Include(o => o.Receipt)
            .ThenInclude(r => r.Bike)
            .Where(o => o.MechanicId == mechanicId && o.Status == "processing")
            .Select(o => new ActiveJobVm
            {
                OrderId = o.OrderId,
                ReceiptId = o.ReceiptId ?? 0,
                CustomerName = o.Receipt != null && o.Receipt.Bike != null && o.Receipt.Bike.Customer != null ? o.Receipt.Bike.Customer.Name : "Khách lẻ",
                LicensePlate = o.Receipt != null && o.Receipt.Bike != null ? o.Receipt.Bike.LicensePlate ?? "" : "",
                Status = o.Status ?? "",
                TotalPrice = o.TotalPrice ?? 0
            })
            .ToListAsync();

        // 3. Lịch làm việc tuần này
        var today = DateOnly.FromDateTime(DateTime.Today);
        model.MySchedule = await _db.WorkSchedules
            .Where(w => w.EmployeeId == mechanicId && w.WorkDate >= today.AddDays(-1))
            .OrderBy(w => w.WorkDate)
            .Take(7)
            .ToListAsync();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> StartJob(int receiptId)
    {
        var mechanicIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(mechanicIdStr, out int mechanicId)) return Unauthorized();

        var receipt = await _db.ServiceReceipts.FirstOrDefaultAsync(r => r.ReceiptId == receiptId);
        if (receipt == null || receipt.Status != "assigned") return BadRequest("Phiếu không hợp lệ hoặc đã được bắt đầu.");

        using var trans = await _db.Database.BeginTransactionAsync();
        try
        {
            receipt.Status = "processing";

            var order = new ServiceOrder
            {
                ReceiptId = receiptId,
                MechanicId = mechanicId,
                Status = "processing",
                CreatedAt = DateTime.Now,
                StartedAt = DateTime.Now
            };

            _db.ServiceOrders.Add(order);
            await _db.SaveChangesAsync();
            await trans.CommitAsync();

            return Json(new { ok = true, orderId = order.OrderId });
        }
        catch (Exception ex)
        {
            await trans.RollbackAsync();
            return BadRequest(ex.Message);
        }
    }

    public async Task<IActionResult> JobDetail(int id)
    {
        var order = await _db.ServiceOrders
            .Include(o => o.Receipt).ThenInclude(r => r.Bike).ThenInclude(b => b.Customer)
            .Include(o => o.OrderParts).ThenInclude(op => op.Part)
            .Include(o => o.ServiceOrderItems).ThenInclude(soi => soi.Service)
            .FirstOrDefaultAsync(o => o.OrderId == id);

        if (order == null) return NotFound();

        var model = new JobDetailVm
        {
            OrderId = order.OrderId,
            ReceiptId = order.ReceiptId ?? 0,
            CustomerName = order.Receipt?.Bike?.Customer?.Name ?? "Khách lẻ",
            BikeModel = order.Receipt?.Bike?.BikeModel ?? "",
            LicensePlate = order.Receipt?.Bike?.LicensePlate ?? "",
            Status = order.Status ?? "",
            SelectedParts = order.OrderParts.ToList(),
            SelectedServices = order.ServiceOrderItems.ToList(),
            PartsTotal = order.OrderParts.Sum(p => (p.Price ?? 0) * (p.Quantity ?? 0)),
            ServicesTotal = order.ServiceOrderItems.Sum(s => s.Price ?? 0)
        };

        ViewBag.AllParts = await _db.Parts.Where(p => p.Stock > 0).ToListAsync();
        ViewBag.AllServices = await _db.Services.ToListAsync();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> AddPart(int orderId, int partId, int quantity)
    {
        var part = await _db.Parts.FindAsync(partId);
        if (part == null || (part.Stock ?? 0) < quantity) return BadRequest("Số lượng tồn kho không đủ.");

        var op = new OrderPart
        {
            OrderId = orderId,
            PartId = partId,
            Quantity = quantity,
            Price = part.Price
        };

        _db.OrderParts.Add(op);
        part.Stock -= quantity; // Trừ tồn kho (UC-03)
        await _db.SaveChangesAsync();

        return Json(new { ok = true });
    }

    [HttpPost]
    public async Task<IActionResult> AddService(int orderId, int serviceId)
    {
        var service = await _db.Services.FindAsync(serviceId);
        if (service == null) return BadRequest("Dịch vụ không tồn tại.");

        var soi = new ServiceOrderItem
        {
            OrderId = orderId,
            ServiceId = serviceId,
            Price = service.Price
        };

        _db.ServiceOrderItems.Add(soi);
        await _db.SaveChangesAsync();

        return Json(new { ok = true });
    }

    [HttpPost]
    public async Task<IActionResult> CompleteJob(int orderId)
    {
        var order = await _db.ServiceOrders.Include(o => o.Receipt).FirstOrDefaultAsync(o => o.OrderId == orderId);
        if (order == null) return NotFound();

        order.Status = "completed";
        order.CompletedAt = DateTime.Now;
        order.TotalPrice = _db.OrderParts.Where(p => p.OrderId == orderId).Sum(p => (p.Price ?? 0) * (p.Quantity ?? 0)) +
                           _db.ServiceOrderItems.Where(s => s.OrderId == orderId).Sum(s => s.Price ?? 0);

        if (order.Receipt != null)
        {
            order.Receipt.Status = "completed";
        }

        await _db.SaveChangesAsync();
        return Json(new { ok = true });
    }

    [HttpGet]
    public async Task<IActionResult> LeaveRequests()
    {
        var mechanicIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(mechanicIdStr, out int mechanicId)) return Unauthorized();

        var requests = await _db.LeaveRequests
            .Where(l => l.EmployeeId == mechanicId)
            .OrderByDescending(l => l.LeaveDate)
            .ToListAsync();

        return View(requests);
    }

    [HttpPost]
    public async Task<IActionResult> RequestLeave(DateOnly date, string reason)
    {
        var mechanicIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(mechanicIdStr, out int mechanicId)) return Unauthorized();

        var leave = new LeaveRequest
        {
            EmployeeId = mechanicId,
            LeaveDate = date,
            Reason = reason,
            Status = "pending"
        };

        _db.LeaveRequests.Add(leave);
        await _db.SaveChangesAsync();

        await _hub.Clients.All.SendAsync("leaveRequestSubmitted");

        return Json(new { ok = true });
    }
}

