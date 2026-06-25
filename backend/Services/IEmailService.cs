namespace Backend.Services;

public interface IEmailService
{
    Task SendCredentialsAsync(string toEmail, string fullName, string password,
        string senderName, string senderEmail);
}
