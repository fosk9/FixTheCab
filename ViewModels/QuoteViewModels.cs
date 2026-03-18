using System.ComponentModel.DataAnnotations;

namespace PMPRacing.ViewModels;

public class CreateQuoteVm
{
    public int OrderId { get; set; }
    public string? LicensePlate { get; set; }
    public string? CustomerName { get; set; }
    public string? BikeModel { get; set; }
    public List<QuoteItemVm> Items { get; set; } = new();
    public decimal LaborCost { get; set; }
    public decimal TotalCost { get; set; }
    public string? CustomerEmail { get; set; }
    public string? Notes { get; set; }
}

public class QuoteItemVm
{
    public string? ItemType { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}
