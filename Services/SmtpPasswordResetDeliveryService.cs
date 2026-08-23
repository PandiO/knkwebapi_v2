using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using knkwebapi_v2.Configuration;

namespace knkwebapi_v2.Services
{
    /// <summary>
    /// Sends password reset emails over SMTP (e.g. Gmail with an app password).
    /// </summary>
    public class SmtpPasswordResetDeliveryService : IPasswordResetDeliveryService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<SmtpPasswordResetDeliveryService> _logger;

        public SmtpPasswordResetDeliveryService(IOptions<EmailSettings> settings, ILogger<SmtpPasswordResetDeliveryService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendPasswordResetAsync(string recipientEmail, string? username, string resetUrl)
        {
            if (string.IsNullOrWhiteSpace(_settings.SmtpUsername) || string.IsNullOrWhiteSpace(_settings.SmtpPassword))
            {
                throw new InvalidOperationException(
                    "Email:SmtpUsername/Email:SmtpPassword are not configured. Set Email__SmtpPassword via environment variable or user-secrets.");
            }

            var greetingName = string.IsNullOrWhiteSpace(username) ? "there" : username;
            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromAddress, _settings.FromName),
                Subject = "Reset your Knights & Kings password",
                Body =
                    $"Hi {greetingName},\n\n" +
                    "We received a request to reset your Knights & Kings password. " +
                    $"Click the link below to choose a new password:\n\n{resetUrl}\n\n" +
                    "If you didn't request this, you can safely ignore this email.",
                IsBodyHtml = false
            };
            message.To.Add(recipientEmail);

            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = _settings.UseStartTls,
                Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword)
            };

            await client.SendMailAsync(message);
            _logger.LogInformation("Password reset email sent to {Email} via SMTP", recipientEmail);
        }
    }
}
