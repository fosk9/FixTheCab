using System.ComponentModel.DataAnnotations;

namespace PMPRacing.ViewModels;

public class VehicleInfoVm
{
    public int BikeId { get; set; }
    
    [Required]
    [StringLength(20)]
    public string LicensePlate { get; set; } = "";
    
    [Required]
    [StringLength(100)]
    public string BikeModel { get; set; } = "";
    
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    
    public string? Notes { get; set; }
    public string? Status { get; set; } = "pending";
}
