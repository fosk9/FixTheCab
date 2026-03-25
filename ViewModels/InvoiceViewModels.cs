using System.ComponentModel.DataAnnotations;

namespace PMPRacing.ViewModels;

public class InvoiceListVm
{
    public int OrderId { get; set; }
    public string? LicensePlate { get; set; }
    public string? CustomerName { get; set; }
    public string? BikeModel { get; set; }
    public string? Status { get; set; }
    public decimal? TotalPrice { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? MechanicName { get; set; }
}

public class InvoiceDetailVm
{
    public int OrderId { get; set; }
    public string? LicensePlate { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? BikeModel { get; set; }
    public string? Status { get; set; }
    public decimal? TotalPrice { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? MechanicName { get; set; }
    public List<InvoiceItemVm> Items { get; set; } = new();
}

public class InvoiceItemVm
{
    public int ItemId { get; set; }
    public string? ItemType { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}
