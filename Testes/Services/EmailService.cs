using System.Net;
using System.Net.Mail;

namespace Testes.Services;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlContent)
    {
        var smtpHost = _config["EmailSettings:SmtpHost"];
        var smtpPortString = _config["EmailSettings:SmtpPort"];
        var fromEmail = _config["EmailSettings:FromEmail"];
        var password = _config["EmailSettings:Password"];

        if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpPortString) || string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(password))
            throw new InvalidOperationException("Configurações de email inválidas ou ausentes no appsettings.json.");

        var smtpPort = int.Parse(smtpPortString);

        var message = new MailMessage();
        message.From = new MailAddress(fromEmail, "Feed the Future BR");
        message.To.Add(new MailAddress(to));
        message.Subject = subject;
        message.Body = htmlContent;
        message.IsBodyHtml = true;

        using (var smtpClient = new SmtpClient(smtpHost, smtpPort))
        {
            smtpClient.Credentials = new NetworkCredential(fromEmail, password);
            smtpClient.EnableSsl = true;
            await smtpClient.SendMailAsync(message);
        }
    }
}