using System.ComponentModel.DataAnnotations;

namespace PMPRacing.ViewModels;

public class AdminAccountRowVm
{
    public int EmployeeId { get; set; }
    public string Name { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Role { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class AdminEditAccountVm
{
    public int EmployeeId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = "";

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    [Required]
    [StringLength(50)]
    public string Role { get; set; } = "cashier";

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "active";

    [StringLength(500)]
    public string? ProfileImagePath { get; set; }

    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6)]
    [Display(Name = "New password (optional)")]
    public string? NewPassword { get; set; }
}

public class AdminCreateAccountStep1Vm
{
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = "";

    [Required]
    [Display(Name = "OTP code")]
    public string OtpCode { get; set; } = "";
}

public class AdminCreateAccountVm
{
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = "";

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = "";

    [StringLength(20)]
    public string? Phone { get; set; }

    [Required]
    [StringLength(50)]
    public string Role { get; set; } = "cashier";

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "active";

    [DataType(DataType.Password)]
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = "";
}

