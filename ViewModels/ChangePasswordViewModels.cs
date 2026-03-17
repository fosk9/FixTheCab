using System.ComponentModel.DataAnnotations;

namespace PMPRacing.ViewModels;

public class RequestOtpViewModel
{
    public int EmployeeId { get; set; }

    [Display(Name = "Email")]
    public string? Email { get; set; }
}

public class ChangePasswordViewModel
{
    public int EmployeeId { get; set; }

    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required]
    [Display(Name = "OTP code")]
    public string OtpCode { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6)]
    [Display(Name = "New password")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[^a-zA-Z0-9]).{6,}$",
        ErrorMessage = "New password must be at least 6 characters and contain at least 1 uppercase letter and 1 special character.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword))]
    [Display(Name = "Confirm new password")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

