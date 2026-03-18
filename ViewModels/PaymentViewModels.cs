using System.ComponentModel.DataAnnotations;

namespace PMPRacing.ViewModels;

public class PaymentVm
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    
    [Required]
    public string? PaymentMethod { get; set; } // "cash", "card", "transfer"
    
    public string? Notes { get; set; }
}

public class PaymentListVm
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public string? LicensePlate { get; set; }
    public string? CustomerName { get; set; }
    public decimal? Amount { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? Status { get; set; }
}

public class PaymentDetailVm
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public string? LicensePlate { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? BikeModel { get; set; }
    public decimal? Amount { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? Notes { get; set; }
    public string? Status { get; set; }
}
