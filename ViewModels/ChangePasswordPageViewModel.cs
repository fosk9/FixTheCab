namespace PMPRacing.ViewModels;

public class ChangePasswordPageViewModel
{
    public RequestOtpViewModel Request { get; set; } = new();
    public ChangePasswordViewModel Change { get; set; } = new();
}

