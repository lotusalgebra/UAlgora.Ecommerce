using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.LicensePortal.Services;

/// <summary>
/// Service interface for sending emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a license purchase confirmation email.
    /// </summary>
    Task SendLicensePurchasedEmailAsync(License license);

    /// <summary>
    /// Sends a license renewal confirmation email.
    /// </summary>
    Task SendLicenseRenewedEmailAsync(License license);

    /// <summary>
    /// Sends a license expiring soon warning email.
    /// </summary>
    Task SendLicenseExpiringSoonEmailAsync(License license, int daysUntilExpiry);

    /// <summary>
    /// Sends a payment failed notification email.
    /// </summary>
    Task SendPaymentFailedEmailAsync(License license, string reason);

    /// <summary>
    /// Sends a subscription cancelled confirmation email.
    /// </summary>
    Task SendSubscriptionCancelledEmailAsync(License license);

    /// <summary>
    /// Sends a login code for passwordless authentication.
    /// </summary>
    Task SendLoginCodeAsync(string email, string code);
}
