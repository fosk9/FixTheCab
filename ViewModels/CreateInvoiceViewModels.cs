using System.ComponentModel.DataAnnotations;

namespace PMPRacing.ViewModels;

public class CreateInvoiceVm
{
    public List<CartItemVm> CartItems { get; set; } = new();
    public decimal TotalPrice => CartItems.Sum(i => i.TotalPrice);
    
    [Required]
    [StringLength(100)]
    public string CustomerName { get; set; } = "";
    
    [StringLength(100)]
    public string? BikeModel { get; set; }
    
    [StringLength(20)]
    public string? LicensePlate { get; set; }
    
    [StringLength(20)]
    public string? CustomerPhone { get; set; }
    
    [EmailAddress]
    [StringLength(100)]
    public string? CustomerEmail { get; set; }
    
    public string? OtpCode { get; set; }
    public bool IsOtpVerified { get; set; }
    public string? Notes { get; set; }
}

public class CartItemVm
{
    public int ItemId { get; set; }
    public string? ItemType { get; set; } // "Service" or "Part"
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}

public class SearchServiceItemVm
{
    public int Id { get; set; }
    public string? ItemType { get; set; } // "Service" or "Part"
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? Price { get; set; }
    public int? Stock { get; set; }
}

public class PayOSPaymentRequestVm
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? ReturnUrl { get; set; }
    public string? CancelUrl { get; set; }
}

public class PayOSPaymentResponseVm
{
    public bool Success { get; set; }
    public string? PaymentUrl { get; set; }
    public string? Message { get; set; }
    public int? OrderId { get; set; }
}
