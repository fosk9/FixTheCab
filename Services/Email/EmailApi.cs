using System.Globalization;

namespace PMPRacing.Services.Email;

public class EmailApi
{
    private readonly IEmailService _email;

    public EmailApi(IEmailService email)
    {
        _email = email;
    }

    public Task SendOtpAsync(string toEmail, string otpCode, TimeSpan expiresIn, string purpose, CancellationToken cancellationToken = default)
    {
        var minutes = Math.Max(1, (int)Math.Ceiling(expiresIn.TotalMinutes));
        var subject = "PMPRacing - OTP code";
        var html = $@"
<div style=""font-family: Arial, sans-serif; line-height: 1.5"">
  <h2 style=""margin:0 0 12px 0"">Your OTP code</h2>
  <p style=""margin:0 0 12px 0"">Use this code for: <b>{purpose}</b></p>
  <div style=""font-size: 28px; letter-spacing: 6px; font-weight: 700; margin: 0 0 12px 0"">{otpCode}</div>
  <p style=""margin:0"">This code expires in {minutes.ToString(CultureInfo.InvariantCulture)} minute(s).</p>
</div>";

        return _email.SendAsync(toEmail, subject, html, cancellationToken);
    }

    public Task SendWelcomeAsync(string toEmail, string displayName, string plainPassword, CancellationToken cancellationToken = default)
    {
        var subject = "Welcome to PMPRacing";
        var html = $@"
<div style=""font-family: Arial, sans-serif; line-height: 1.5"">
  <h2 style=""margin:0 0 12px 0"">Welcome, {displayName}!</h2>
  <p style=""margin:0 0 12px 0"">Your account has been created.</p>
  <p style=""margin:0 0 6px 0""><b>Login email:</b> {toEmail}</p>
  <p style=""margin:0 0 12px 0""><b>Temporary password:</b> {plainPassword}</p>
  <p style=""margin:0"">Please change your password after signing in.</p>
</div>";

        return _email.SendAsync(toEmail, subject, html, cancellationToken);
    }
}

