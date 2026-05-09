using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PMPRacing.Models;
using PMPRacing.ViewModels;
using Microsoft.AspNetCore.SignalR;
using PMPRacing.Hubs;
using PMPRacing.Services.Email;
using System.Security.Claims;

namespace PMPRacing.Controllers;

[Authorize(Roles = "manager")]
public class CashiersController : Controller
{
    private readonly PmpRacingContext _db;
    private readonly IMemoryCache _cache;
    private readonly EmailApi _emailApi;
    private readonly IHubContext<AdminAccountsHub> _hub;

    public CashiersController(PmpRacingContext db, IMemoryCache cache, EmailApi emailApi, IHubContext<AdminAccountsHub> hub)
    {
        _db = db;
        _cache = cache;
        _emailApi = emailApi;
        _hub = hub;
    }

    public IActionResult Index() => View();

    #region Invoice Management

    [HttpGet]
    public IActionResult Invoices() => View();

    [HttpGet]
    public async Task<IActionResult> InvoiceTable(string search = "", string status = "", int page = 1)
    {
        const int pageSize = 10;
        
        var query = _db.ServiceOrders
            .Include(o => o.Receipt)
            .ThenInclude(r => r!.Bike)
            .ThenInclude(b => b!.Customer)
            .AsNoTracking();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(o => 
                (o.Receipt!.Bike!.LicensePlate != null && o.Receipt.Bike.LicensePlate.Contains(search)) ||
                (o.Receipt.Bike.Customer!.Name != null && o.Receipt.Bike.Customer.Name.Contains(search)) ||
                (o.Receipt.Bike.BikeModel != null && o.Receipt.Bike.BikeModel.Contains(search)));
        }

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(status))
        {
            status = status.Trim().ToLowerInvariant();
            query = query.Where(o => o.Status != null && o.Status == status);
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        
        var invoices = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new InvoiceListVm
            {
                OrderId = o.OrderId,
                LicensePlate = o.Receipt!.Bike!.LicensePlate,
                CustomerName = o.Receipt.Bike.Customer!.Name,
                BikeModel = o.Receipt.Bike.BikeModel,
                Status = o.Status,
                TotalPrice = o.TotalPrice,
                CreatedAt = o.CreatedAt,
                CompletedAt = o.CompletedAt
            })
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalCount = totalCount;

        return PartialView("Partials/_InvoiceTable", invoices);
    }

    [HttpGet]
    public async Task<IActionResult> InvoiceDetails(int id)
    {
        var order = await _db.ServiceOrders
            .Include(o => o.Receipt)
            .ThenInclude(r => r!.Bike)
            .ThenInclude(b => b!.Customer)
            .Include(o => o.ServiceOrderItems).ThenInclude(si => si.Service)
            .Include(o => o.OrderParts)
            .ThenInclude(op => op.Part)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderId == id);

        if (order == null) return NotFound();

        var items = new List<InvoiceItemVm>();
        
        // Add service items
        items.AddRange(order.ServiceOrderItems.Select(si => new InvoiceItemVm
        {
            ItemId = si.ItemId,
            ItemType = "Service",
            Name = si.Service?.ServiceName ?? "Unknown Service",
            Description = null,
            Quantity = 1,
            UnitPrice = si.Price ?? 0
        }));

        // Add parts
        items.AddRange(order.OrderParts.Select(op => new InvoiceItemVm
        {
            ItemId = op.Id,
            ItemType = "Part",
            Name = op.Part?.PartName ?? "Unknown Part",
            Description = null,
            Quantity = op.Quantity ?? 1,
            UnitPrice = op.Price ?? 0
        }));

        var viewModel = new InvoiceDetailVm
        {
            OrderId = order.OrderId,
            LicensePlate = order.Receipt!.Bike!.LicensePlate,
            CustomerName = order.Receipt.Bike.Customer!.Name,
            CustomerEmail = order.Receipt.Bike.Customer.Email,
            CustomerPhone = order.Receipt.Bike.Customer.Phone,
            BikeModel = order.Receipt.Bike.BikeModel,
            Status = order.Status,
            TotalPrice = order.TotalPrice,
            CreatedAt = order.CreatedAt,
            StartedAt = order.StartedAt,
            CompletedAt = order.CompletedAt,
            Items = items
        };

        return PartialView("Partials/_InvoiceDetails", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateInvoice(int id)
    {
        var order = await _db.ServiceOrders.FindAsync(id);
        if (order == null) return NotFound();
        if (order.Status != "completed") return BadRequest("Đơn hàng chưa hoàn thành.");

        order.Status = "awaiting_payment";
        await _db.SaveChangesAsync();
        return Json(new { ok = true });
    }

    #endregion

    #region Vehicle Information

    [HttpGet]
    public IActionResult VehicleInfo() => View();

    [HttpGet]
    public async Task<IActionResult> VehicleTable(string search = "")
    {
        var query = _db.Bikes
            .Include(b => b.Customer)
            .Include(b => b.ServiceReceipts)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(b => 
                (b.LicensePlate != null && b.LicensePlate.Contains(search)) ||
                (b.BikeModel != null && b.BikeModel.Contains(search)) ||
                (b.Customer!.Name != null && b.Customer.Name.Contains(search)));
        }

        var vehicles = await query
            .OrderByDescending(b => b.BikeId)
            .Select(b => new VehicleInfoVm
            {
                BikeId = b.BikeId,
                LicensePlate = b.LicensePlate ?? "",
                BikeModel = b.BikeModel ?? "",
                CustomerId = b.CustomerId,
                CustomerName = b.Customer!.Name,
                CustomerEmail = b.Customer.Email,
                CustomerPhone = b.Customer.Phone,
                Status = b.ServiceReceipts.Any() ? "in_service" : "available"
            })
            .ToListAsync();

        return PartialView("Partials/_VehicleTable", vehicles);
    }

    [HttpGet]
    public IActionResult VehicleCreate() => PartialView("Partials/_VehicleCreate");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VehicleCreate(VehicleInfoVm model)
    {
        if (!ModelState.IsValid) return PartialView("Partials/_VehicleCreate", model);

        // Check if license plate already exists
        var existingBike = await _db.Bikes
            .AnyAsync(b => b.LicensePlate == model.LicensePlate.Trim());

        if (existingBike)
        {
            ModelState.AddModelError(nameof(model.LicensePlate), "License plate already exists.");
            return PartialView("Partials/_VehicleCreate", model);
        }

        // Find or create customer
        Customer? customer = null;
        if (!string.IsNullOrWhiteSpace(model.CustomerName))
        {
            customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.Email == model.CustomerEmail);

            if (customer == null)
            {
                customer = new Customer
                {
                    Name = model.CustomerName.Trim(),
                    Email = string.IsNullOrWhiteSpace(model.CustomerEmail) ? null : model.CustomerEmail.Trim(),
                    Phone = string.IsNullOrWhiteSpace(model.CustomerPhone) ? null : model.CustomerPhone.Trim()
                };
                _db.Customers.Add(customer);
                await _db.SaveChangesAsync();
            }
        }

        var bike = new Bike
        {
            LicensePlate = model.LicensePlate.Trim(),
            BikeModel = model.BikeModel.Trim(),
            CustomerId = customer?.CustomerId
        };

        _db.Bikes.Add(bike);
        await _db.SaveChangesAsync();

        return Json(new { ok = true });
    }

    #endregion

    #region Quotation Management

    [HttpGet]
    public async Task<IActionResult> CreateQuote(int id)
    {
        var order = await _db.ServiceOrders
            .Include(o => o.Receipt)
            .ThenInclude(r => r!.Bike)
            .ThenInclude(b => b!.Customer)
            .Include(o => o.ServiceOrderItems)
            .Include(o => o.OrderParts)
            .ThenInclude(op => op.Part)
            .FirstOrDefaultAsync(o => o.OrderId == id);

        if (order == null) return NotFound();

        var items = new List<QuoteItemVm>();
        
        // Add existing service items
        items.AddRange(order.ServiceOrderItems.Select(si => new QuoteItemVm
        {
            ItemType = "Service",
            Name = si.Service?.ServiceName ?? "Unknown Service",
            Description = null,
            Quantity = 1,
            UnitPrice = si.Price ?? 0
        }));

        // Add existing parts
        items.AddRange(order.OrderParts.Select(op => new QuoteItemVm
        {
            ItemType = "Part",
            Name = op.Part?.PartName ?? "Unknown Part",
            Description = null,
            Quantity = op.Quantity ?? 1,
            UnitPrice = op.Price ?? 0
        }));

        var viewModel = new CreateQuoteVm
        {
            OrderId = order.OrderId,
            LicensePlate = order.Receipt!.Bike!.LicensePlate,
            CustomerName = order.Receipt.Bike.Customer!.Name,
            CustomerEmail = order.Receipt.Bike.Customer.Email,
            BikeModel = order.Receipt.Bike.BikeModel,
            Items = items,
            LaborCost = items.Where(i => i.ItemType == "Service").Sum(i => i.TotalPrice),
            TotalCost = items.Sum(i => i.TotalPrice)
        };

        return PartialView("Partials/_CreateQuote", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendQuote(int orderId, string customerEmail, string notes)
    {
        var order = await _db.ServiceOrders
            .Include(o => o.Receipt)
            .ThenInclude(r => r!.Bike)
            .ThenInclude(b => b!.Customer)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null) return NotFound();

        // Update order status to "quoted"
        order.Status = "quoted";
        await _db.SaveChangesAsync();

        // Here you would implement email sending functionality
        // For now, just return success

        return Json(new { ok = true });
    }

    #endregion

    #region Payment Management

    [HttpGet]
    public IActionResult Payments() => View();

    [HttpGet]
    public async Task<IActionResult> PaymentTable(string search = "", string status = "")
    {
        var query = _db.Payments
            .Include(p => p.Order)
            .ThenInclude(o => o.Receipt)
            .ThenInclude(r => r.Bike)
            .ThenInclude(b => b.Customer)
            .AsNoTracking();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(p => 
                (p.Order.Receipt.Bike.LicensePlate != null && p.Order.Receipt.Bike.LicensePlate.Contains(search)) ||
                (p.Order.Receipt.Bike.Customer.Name != null && p.Order.Receipt.Bike.Customer.Name.Contains(search)));
        }

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(status))
        {
            status = status.Trim().ToLowerInvariant();
            query = query.Where(p => p.Status != null && p.Status == status);
        }

        // Get data first, then project to avoid expression tree issues
        var paymentsData = await query
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync();

        var payments = paymentsData.Select(p => new PaymentListVm
        {
            PaymentId = p.PaymentId,
            OrderId = p.Order?.OrderId ?? 0,
            LicensePlate = p.Order?.Receipt?.Bike?.LicensePlate ?? "N/A",
            CustomerName = p.Order?.Receipt?.Bike?.Customer?.Name ?? "Unknown",
            Amount = p.Amount,
            PaymentMethod = p.Method,
            PaymentDate = p.PaidAt,
            Status = p.Status
        }).ToList();

        return PartialView("Partials/_PaymentTable", payments);
    }

    [HttpGet]
    public async Task<IActionResult> CreatePayment(int orderId)
    {
        var order = await _db.ServiceOrders
            .Include(o => o.Receipt)
            .ThenInclude(r => r!.Bike)
            .ThenInclude(b => b!.Customer)
            .Include(o => o.ServiceOrderItems)
            .Include(o => o.OrderParts)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null) return NotFound();

        var viewModel = new PaymentDetailVm
        {
            OrderId = order.OrderId,
            LicensePlate = order.Receipt!.Bike!.LicensePlate,
            CustomerName = order.Receipt.Bike.Customer!.Name,
            CustomerEmail = order.Receipt.Bike.Customer.Email,
            CustomerPhone = order.Receipt.Bike.Customer.Phone,
            BikeModel = order.Receipt.Bike.BikeModel,
            Amount = order.TotalPrice ?? 0,
            Status = "pending"
        };

        return PartialView("Partials/_CreatePayment", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessPayment(PaymentVm model)
    {
        if (!ModelState.IsValid) return BadRequest("Invalid payment data.");

        var order = await _db.ServiceOrders.FindAsync(model.OrderId);
        if (order == null) return NotFound();

        var payment = new Payment
        {
            OrderId = model.OrderId,
            Amount = model.Amount,
            Method = model.PaymentMethod,
            Status = "completed",
            PaidAt = DateTime.Now
        };

        _db.Payments.Add(payment);

        // Update order status to paid
        order.Status = "paid";
        await _db.SaveChangesAsync();

        return Json(new { ok = true });
    }

    #endregion

    #region Create Invoice (Main Cashier Feature)

    [HttpGet]
    public IActionResult CreateInvoice() => View();

    [HttpGet]
    public async Task<IActionResult> SearchServices(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Json(new List<SearchServiceItemVm>());

        query = query.Trim();

        // Search in Services
        var services = await _db.Services
            .Where(s => s.ServiceName != null && s.ServiceName.Contains(query))
            .Select(s => new SearchServiceItemVm
            {
                Id = s.ServiceId,
                ItemType = "Service",
                Name = s.ServiceName,
                Description = null,
                Price = s.Price,
                Stock = null,
                ImageUrl = null
            })
            .ToListAsync();

        // Search in Parts
        var parts = await _db.Parts
            .Where(p => p.PartName != null && p.PartName.Contains(query) && p.Stock > 0)
            .Select(p => new SearchServiceItemVm
            {
                Id = p.PartId,
                ItemType = "Part",
                Name = p.PartName,
                Description = null,
                Price = p.Price,
                Stock = p.Stock,
                ImageUrl = null
            })
            .ToListAsync();

        var results = services.Concat(parts).ToList();
        return Json(results);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendInvoiceOtp(string email)
    {
        email = (email ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(email)) 
            return BadRequest("Email is required.");

        if (!IsValidEmail(email))
            return BadRequest("Invalid email format.");

        try
        {
            var otp = Random.Shared.Next(100000, 999999).ToString();
            var expires = TimeSpan.FromMinutes(5);
            
            // Store OTP in cache
            _cache.Set($"otp:invoice:{email.ToLowerInvariant()}", otp, expires);

            // Send OTP email
            await _emailApi.SendOtpAsync(email, otp, expires, "Xác thực tạo hóa đơn");
            
            return Json(new { ok = true, message = "OTP đã được gửi đến email của bạn" });
        }
        catch (Exception ex)
        {
            return BadRequest($"Không thể gửi OTP: {ex.Message}");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult VerifyInvoiceOtp(string email, string otpCode)
    {
        email = (email ?? string.Empty).Trim();
        otpCode = (otpCode ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otpCode))
            return BadRequest("Email and OTP are required.");

        var cacheKey = $"otp:invoice:{email.ToLowerInvariant()}";
        if (!_cache.TryGetValue(cacheKey, out string? storedOtp) || string.IsNullOrWhiteSpace(storedOtp))
            return BadRequest("OTP expired or not requested.");

        if (!string.Equals(otpCode, storedOtp, StringComparison.Ordinal))
            return BadRequest("Invalid OTP.");

        // Remove OTP after successful verification
        _cache.Remove(cacheKey);

        return Json(new { ok = true, message = "OTP verified successfully" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceVm model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(string.Join("; ", errors));
        }

        try
        {
            // Create or find customer
            var customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.Email == model.CustomerEmail);

            if (customer == null)
            {
                customer = new Customer
                {
                    Name = model.CustomerName.Trim(),
                    Email = string.IsNullOrWhiteSpace(model.CustomerEmail) ? null : model.CustomerEmail.Trim(),
                    Phone = string.IsNullOrWhiteSpace(model.CustomerPhone) ? null : model.CustomerPhone.Trim()
                };
                _db.Customers.Add(customer);
                await _db.SaveChangesAsync();
            }

            // Create or find bike
            Bike? bike = null;
            if (!string.IsNullOrWhiteSpace(model.LicensePlate))
            {
                bike = await _db.Bikes
                    .FirstOrDefaultAsync(b => b.LicensePlate == model.LicensePlate.Trim());

                if (bike == null)
                {
                    bike = new Bike
                    {
                        LicensePlate = model.LicensePlate.Trim(),
                        BikeModel = string.IsNullOrWhiteSpace(model.BikeModel) ? null : model.BikeModel.Trim(),
                        CustomerId = customer.CustomerId
                    };
                    _db.Bikes.Add(bike);
                    await _db.SaveChangesAsync();
                }
            }

            // Create service receipt
            var receipt = new ServiceReceipt
            {
                BikeId = bike?.BikeId,
                CreatedBy = GetCurrentEmployeeId(),
                Status = "pending",
                CreatedAt = DateTime.Now
            };
            _db.ServiceReceipts.Add(receipt);
            await _db.SaveChangesAsync();

            // Create service order
            var order = new ServiceOrder
            {
                ReceiptId = receipt.ReceiptId,
                Status = "pending",
                TotalPrice = model.TotalPrice,
                CreatedAt = DateTime.Now
            };
            _db.ServiceOrders.Add(order);
            await _db.SaveChangesAsync();

            // Add service order items
            foreach (var item in model.CartItems.Where(i => i.ItemType == "Service"))
            {
                var orderItem = new ServiceOrderItem
                {
                    OrderId = order.OrderId,
                    ServiceId = item.ItemId,
                    Price = item.UnitPrice
                };
                _db.ServiceOrderItems.Add(orderItem);
            }

            // Add order parts
            foreach (var item in model.CartItems.Where(i => i.ItemType == "Part"))
            {
                var orderPart = new OrderPart
                {
                    OrderId = order.OrderId,
                    PartId = item.ItemId,
                    Quantity = item.Quantity,
                    Price = item.UnitPrice
                };
                _db.OrderParts.Add(orderPart);
            }

            await _db.SaveChangesAsync();

            return Json(new { ok = true, orderId = order.OrderId, message = "Invoice created successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to create invoice: {ex.Message}");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePayOSPayment([FromBody] PayOSPaymentRequestVm model)
    {
        try
        {
            // Validate order exists
            var order = await _db.ServiceOrders.FindAsync(model.OrderId);
            if (order == null)
                return BadRequest("Order not found.");

            // Update order status
            order.Status = "awaiting_payment";
            await _db.SaveChangesAsync();

            // Return simulated payment URL
            var paymentUrl = $"/Cashiers/SimulatedPayment?orderId={model.OrderId}&amount={model.Amount}";

            return Json(new PayOSPaymentResponseVm
            {
                Success = true,
                PaymentUrl = paymentUrl,
                OrderId = model.OrderId,
                Message = "Payment link created successfully"
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to create payment: {ex.Message}");
        }
    }

    [HttpGet]
    public IActionResult PaymentSuccess(int orderId)
    {
        ViewBag.Success = true;
        ViewBag.Message = "Thanh toán thành công!";
        ViewBag.OrderId = orderId;
        return View();
    }

    [HttpGet]
    public IActionResult PaymentFailed(int orderId)
    {
        ViewBag.Success = false;
        ViewBag.Message = "Thanh toán thất bại. Vui lòng thử lại.";
        ViewBag.OrderId = orderId;
        return View("PaymentSuccess");
    }

    [HttpGet]
    public async Task<IActionResult> SimulatedPayment(int orderId, decimal amount)
    {
        var order = await _db.ServiceOrders
            .Include(o => o.Receipt)
            .ThenInclude(r => r!.Bike)
            .ThenInclude(b => b!.Customer)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null) return NotFound();

        ViewBag.OrderId = orderId;
        ViewBag.Amount = order.TotalPrice ?? 0;
        ViewBag.CustomerName = order.Receipt?.Bike?.Customer?.Name ?? "Khách hàng";
        ViewBag.Description = $"Thanh toán hóa đơn #{orderId}";

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPaymentSuccess(int orderId)
    {
        try
        {
            var order = await _db.ServiceOrders.FindAsync(orderId);
            if (order == null) return NotFound();

            // Check if payment already exists
            var existingPayment = await _db.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId);
            
            if (existingPayment == null)
            {
                // Create payment record
                var payment = new Payment
                {
                    OrderId = orderId,
                    Amount = order.TotalPrice,
                    Method = "simulated",
                    Status = "completed",
                    PaidAt = DateTime.Now
                };
                _db.Payments.Add(payment);
            }
            else
            {
                existingPayment.Status = "completed";
                existingPayment.PaidAt = DateTime.Now;
            }

            // Always update order status to paid
            order.Status = "paid";
            await _db.SaveChangesAsync();

            return RedirectToAction("PaymentSuccess", new { orderId });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to confirm payment: {ex.Message}");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPaymentFailed(int orderId)
    {
        try
        {
            var order = await _db.ServiceOrders.FindAsync(orderId);
            if (order == null) return NotFound();

            // Update order status to failed
            order.Status = "payment_failed";
            await _db.SaveChangesAsync();

            return RedirectToAction("PaymentFailed", new { orderId });
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to process payment failure: {ex.Message}");
        }
    }

    #region Leave Request

    [HttpGet]
    public async Task<IActionResult> LeaveRequests()
    {
        var employeeIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(employeeIdStr, out int employeeId)) return Unauthorized();

        var requests = await _db.LeaveRequests
            .Where(l => l.EmployeeId == employeeId)
            .OrderByDescending(l => l.LeaveDate)
            .ToListAsync();

        return View(requests);
    }

    [HttpPost]
    public async Task<IActionResult> RequestLeave(DateOnly date, string reason)
    {
        var employeeIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(employeeIdStr, out int employeeId)) return Unauthorized();

        var leave = new LeaveRequest
        {
            EmployeeId = employeeId,
            LeaveDate = date,
            Reason = reason,
            Status = "pending"
        };

        _db.LeaveRequests.Add(leave);
        await _db.SaveChangesAsync();

        await _hub.Clients.All.SendAsync("leaveRequestSubmitted");

        return Json(new { ok = true });
    }

    #endregion

    private int? GetCurrentEmployeeId()
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(idStr, out int id)) return id;
        return null;
    }

    private bool IsValidEmail(string email)
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

    #endregion
}

