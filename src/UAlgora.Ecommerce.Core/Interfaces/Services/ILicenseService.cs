using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Services;

/// <summary>
/// Service interface for License operations.
/// </summary>
public interface ILicenseService
{
    /// <summary>
    /// Gets a license by ID.
    /// </summary>
    Task<License?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a license by key.
    /// </summary>
    Task<License?> GetByKeyAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Gets licenses by customer email.
    /// </summary>
    Task<IReadOnlyList<License>> GetByCustomerEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Gets all active licenses.
    /// </summary>
    Task<IReadOnlyList<License>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Creates a new license.
    /// </summary>
    Task<License> CreateAsync(License license, CancellationToken ct = default);

    /// <summary>
    /// Generates a license key.
    /// </summary>
    Task<string> GenerateKeyAsync(LicenseType type, CancellationToken ct = default);

    /// <summary>
    /// Validates a license key.
    /// </summary>
    Task<LicenseValidationResponse> ValidateAsync(string key, string? domain = null, CancellationToken ct = default);

    /// <summary>
    /// Activates a license.
    /// </summary>
    Task<LicenseActivationResult> ActivateAsync(string key, string domain, string? machineFingerprint = null, CancellationToken ct = default);

    /// <summary>
    /// Deactivates a license.
    /// </summary>
    Task<bool> DeactivateAsync(Guid licenseId, CancellationToken ct = default);

    /// <summary>
    /// Renews a license.
    /// </summary>
    Task<License> RenewAsync(Guid licenseId, DateTime newValidUntil, CancellationToken ct = default);

    /// <summary>
    /// Upgrades a license type.
    /// </summary>
    Task<License> UpgradeAsync(Guid licenseId, LicenseType newType, CancellationToken ct = default);

    /// <summary>
    /// Checks if a feature is enabled.
    /// </summary>
    Task<bool> IsFeatureEnabledAsync(Guid licenseId, string feature, CancellationToken ct = default);

    /// <summary>
    /// Gets enabled features for a license.
    /// </summary>
    Task<IReadOnlyList<string>> GetEnabledFeaturesAsync(Guid licenseId, CancellationToken ct = default);

    /// <summary>
    /// Gets licenses expiring soon.
    /// </summary>
    Task<IReadOnlyList<License>> GetExpiringSoonAsync(int days = 30, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing license.
    /// </summary>
    Task<License> UpdateAsync(License license, CancellationToken ct = default);

    /// <summary>
    /// Creates a trial license for a store.
    /// </summary>
    Task<License> CreateTrialAsync(string customerName, string customerEmail, int trialDays = 14, CancellationToken ct = default);
}

/// <summary>
/// License validation response.
/// </summary>
public class LicenseValidationResponse
{
    public bool IsValid { get; set; }
    public LicenseValidationResult Result { get; set; }
    public License? License { get; set; }
    public string? ErrorMessage { get; set; }
    public int? DaysRemaining { get; set; }
    public IReadOnlyList<string> EnabledFeatures { get; set; } = [];
    public IReadOnlyList<string> Warnings { get; set; } = [];
}

/// <summary>
/// License activation result.
/// </summary>
public class LicenseActivationResult
{
    public bool Success { get; set; }
    public License? License { get; set; }
    public string? ErrorMessage { get; set; }
    public int RemainingActivations { get; set; }
}
