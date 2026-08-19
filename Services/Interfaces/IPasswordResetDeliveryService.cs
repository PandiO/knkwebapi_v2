using System.Threading.Tasks;

namespace knkwebapi_v2.Services
{
    /// <summary>
    /// Delivers password reset links to users.
    /// </summary>
    public interface IPasswordResetDeliveryService
    {
        /// <summary>
        /// Sends a password reset link to a recipient email.
        /// </summary>
        Task SendPasswordResetAsync(string recipientEmail, string? username, string resetUrl);
    }
}
