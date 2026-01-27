using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.LicensePortal.Services;

/// <summary>
/// Service interface for generating and managing licenses.
/// </summary>
public interface ILicenseGenerationService
{
    /// <summary>
    /// Generates and activates a new license.
    /// </summary>
    Task<License> GenerateAndActivateLicenseAsync(
        LicenseType tier,
        string customerEmail,
        string customerName,
        string? companyName,
        string? domain,
        string paymentProvider,
        string? subscriptionId = null);

    /// <summary>
    /// Extends an existing license by one year.
    /// </summary>
    Task<License?> ExtendLicenseAsync(Guid licenseId);

    /// <summary>
    /// Suspends a license (e.g., payment failed).
    /// </summary>
    Task<License?> SuspendLicenseAsync(Guid licenseId, string reason);

    /// <summary>
    /// Reactivates a suspended license.
    /// </summary>
    Task<License?> ReactivateLicenseAsync(Guid licenseId);

    /// <summary>
    /// Gets license by subscription ID.
    /// </summary>
    Task<License?> GetLicenseBySubscriptionIdAsync(string subscriptionId);
}

/// <summary>
/// Result of a license generation operation.
/// </summary>
public class LicenseGenerationResult
{
    public bool Success { get; set; }
    public License? License { get; set; }
    public string? Error { get; set; }
}
