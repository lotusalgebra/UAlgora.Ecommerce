using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for License operations.
/// </summary>
public interface ILicenseRepository : IRepository<License>
{
    /// <summary>
    /// Get a license by its key.
    /// </summary>
    Task<License?> GetByKeyAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Get licenses by status.
    /// </summary>
    Task<IReadOnlyList<License>> GetByStatusAsync(LicenseStatus status, CancellationToken ct = default);

    /// <summary>
    /// Get licenses by type.
    /// </summary>
    Task<IReadOnlyList<License>> GetByTypeAsync(LicenseType type, CancellationToken ct = default);

    /// <summary>
    /// Get licenses by customer email.
    /// </summary>
    Task<IReadOnlyList<License>> GetByCustomerEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Get active licenses.
    /// </summary>
    Task<IReadOnlyList<License>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Get licenses expiring soon.
    /// </summary>
    Task<IReadOnlyList<License>> GetExpiringSoonAsync(int daysUntilExpiry, CancellationToken ct = default);

    /// <summary>
    /// Get expired licenses that are still active.
    /// </summary>
    Task<IReadOnlyList<License>> GetExpiredActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Get licenses requiring validation (validation interval exceeded).
    /// </summary>
    Task<IReadOnlyList<License>> GetRequiringValidationAsync(CancellationToken ct = default);

    /// <summary>
    /// Update license validation status.
    /// </summary>
    Task UpdateValidationStatusAsync(Guid licenseId, LicenseValidationResult result, string? error = null, CancellationToken ct = default);

    /// <summary>
    /// Increment activation count.
    /// </summary>
    Task<bool> IncrementActivationCountAsync(Guid licenseId, CancellationToken ct = default);

    /// <summary>
    /// Check if a license key already exists.
    /// </summary>
    Task<bool> KeyExistsAsync(string key, Guid? excludeId = null, CancellationToken ct = default);

    /// <summary>
    /// Get license by domain.
    /// </summary>
    Task<License?> GetByDomainAsync(string domain, CancellationToken ct = default);
}
