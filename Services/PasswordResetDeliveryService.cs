using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace knkwebapi_v2.Services
{
    /// <summary>
    /// Development-safe password reset delivery service.
    /// Replace with SMTP/provider integration for production email delivery.
    /// </summary>
    public class PasswordResetDeliveryService : IPasswordResetDeliveryService
    {
        private readonly ILogger<PasswordResetDeliveryService> _logger;

        public PasswordResetDeliveryService(ILogger<PasswordResetDeliveryService> logger)
        {
            _logger = logger;
        }

        public Task SendPasswordResetAsync(string recipientEmail, string? username, string resetUrl)
        {
            // Intentionally logs only in backend for now; delivery provider can be swapped in later.
            _logger.LogInformation(
                "Password reset requested for {Email} ({Username}). Reset URL: {ResetUrl}",
                recipientEmail,
                string.IsNullOrWhiteSpace(username) ? "unknown" : username,
                resetUrl);

            return Task.CompletedTask;
        }
    }
}
