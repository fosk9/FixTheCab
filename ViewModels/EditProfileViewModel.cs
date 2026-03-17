using System.ComponentModel.DataAnnotations;

namespace PMPRacing.ViewModels;

public class EditProfileViewModel
{
    public int EmployeeId { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(100)]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [StringLength(20)]
    [Display(Name = "Phone")]
    public string? Phone { get; set; }

    [StringLength(500)]
    [Display(Name = "Profile image path")]
    public string? ProfileImagePath { get; set; }

    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6)]
    [Display(Name = "New password")]
    public string? NewPassword { get; set; }
}

