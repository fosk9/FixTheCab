using System.ComponentModel.DataAnnotations;

namespace PMPRacing.ViewModels;

public class PartListRowVm
{
    public int PartId { get; set; }
    public string PartName { get; set; } = null!;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int WarningLevel { get; set; }
}

public class PartCreateVm
{
    [Required(ErrorMessage = "Tên phụ tùng không được để trống")]
    [MaxLength(100, ErrorMessage = "Tên quá dài")]
    public string PartName { get; set; } = null!;

    [Required]
    [Range(0, 100000000, ErrorMessage = "Giá không hợp lệ")]
    public decimal Price { get; set; }

    [Required]
    [Range(0, 10000, ErrorMessage = "Số lượng tồn kho không hợp lệ")]
    public int Stock { get; set; }

    [Required]
    [Range(0, 1000, ErrorMessage = "Mức cảnh báo tồn kho không hợp lệ")]
    public int WarningLevel { get; set; }
}

public class PartEditVm : PartCreateVm
{
    [Required]
    public int PartId { get; set; }
}

public class ServiceListRowVm
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = null!;
    public decimal? Price { get; set; }
}

public class ServiceCreateVm
{
    [Required(ErrorMessage = "Tên dịch vụ không được để trống")]
    [MaxLength(100, ErrorMessage = "Tên quá dài")]
    public string ServiceName { get; set; } = null!;

    [Required]
    [Range(0, 100000000, ErrorMessage = "Giá không hợp lệ")]
    public decimal Price { get; set; }
}

public class ServiceEditVm : ServiceCreateVm
{
    [Required]
    public int ServiceId { get; set; }
}

public class WorkHistoryRowVm
{
    public int OrderId { get; set; }
    public string LicensePlate { get; set; } = null!;
    public string BikeModel { get; set; } = null!;
    public string CustomerName { get; set; } = null!;
    public DateTime? CompletedAt { get; set; }
}
