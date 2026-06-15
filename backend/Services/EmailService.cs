using System.Net;
using System.Net.Mail;

namespace Backend.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendCredentialsAsync(string toEmail, string fullName, string password)
    {
        var host = _config["Email:SmtpHost"];

        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogWarning(
                "SMTP yapılandırılmamış. E-posta atlandı. >> Kullanıcı: {Email} | Şifre: {Password}",
                toEmail, password);
            return;
        }

        var port = int.TryParse(_config["Email:SmtpPort"], out var p) ? p : 587;
        var username = _config["Email:Username"] ?? "";
        var smtpPassword = _config["Email:Password"] ?? "";
        var fromEmail = _config["Email:From"] ?? username;
        var fromName = _config["Email:FromName"] ?? "MÜDEK Sistemi";

        using var message = new MailMessage();
        message.From = new MailAddress(fromEmail, fromName);
        message.To.Add(new MailAddress(toEmail, fullName));
        message.Subject = "MÜDEK Sistemi — Giriş Bilgileriniz";
        message.IsBodyHtml = true;
        message.Body = $"""
            <div style="font-family:Arial,sans-serif;max-width:480px;margin:0 auto;padding:32px;">
              <h2 style="color:#16213e;margin-bottom:4px;">MÜDEK Ders Kalite Kontrol Sistemi</h2>
              <p>Merhaba <strong>{fullName}</strong>,</p>
              <p>Sisteme tanımlandınız. Aşağıdaki bilgilerle giriş yapabilirsiniz:</p>
              <div style="background:#f5f5f5;border-radius:8px;padding:20px;margin:20px 0;">
                <p style="margin:0 0 8px;"><strong>E-posta:</strong> {toEmail}</p>
                <p style="margin:0;"><strong>Şifre:</strong> {password}</p>
              </div>
              <p style="color:#888;font-size:13px;">Güvenliğiniz için ilk girişten sonra şifrenizi değiştirmenizi öneririz.</p>
            </div>
            """;

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(username, smtpPassword)
        };

        try
        {
            await client.SendMailAsync(message);
            _logger.LogInformation("E-posta gönderildi: {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "E-posta gönderilemedi: {Email}", toEmail);
        }
    }
}
