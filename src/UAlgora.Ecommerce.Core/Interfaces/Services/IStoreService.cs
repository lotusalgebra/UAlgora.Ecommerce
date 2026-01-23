using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Services;

/// <summary>
/// Service interface for Store operations.
/// </summary>
public interface IStoreService
{
    /// <summary>
    /// Gets a store by ID.
    /// </summary>
    Task<Store?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a store by code.
    /// </summary>
    Task<Store?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Gets a store by domain.
    /// </summary>
    Task<Store?> GetByDomainAsync(string domain, CancellationToken ct = default);

    /// <summary>
    /// Gets all active stores.
    /// </summary>
    Task<IReadOnlyList<Store>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets all stores.
    /// </summary>
    Task<IReadOnlyList<Store>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Creates a new store.
    /// </summary>
    Task<Store> CreateAsync(Store store, CancellationToken ct = default);

    /// <summary>
    /// Updates a store.
    /// </summary>
    Task<Store> UpdateAsync(Store store, CancellationToken ct = default);

    /// <summary>
    /// Deletes a store (soft delete).
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Validates a store's license.
    /// </summary>
    Task<LicenseValidationResult> ValidateLicenseAsync(Guid storeId, CancellationToken ct = default);

    /// <summary>
    /// Gets the current store from request context.
    /// </summary>
    Task<Store?> GetCurrentStoreAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the next order number for a store.
    /// </summary>
    Task<string> GetNextOrderNumberAsync(Guid storeId, CancellationToken ct = default);

    /// <summary>
    /// Checks if a store is in trial.
    /// </summary>
    Task<bool> IsTrialAsync(Guid storeId, CancellationToken ct = default);

    /// <summary>
    /// Gets stores with expiring trials.
    /// </summary>
    Task<IReadOnlyList<Store>> GetExpiringTrialsAsync(int daysUntilExpiry = 7, CancellationToken ct = default);
}

/// <summary>
/// License validation result for a store.
/// </summary>
public class StoreLicenseValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public int? DaysRemaining { get; set; }
    public LicenseType LicenseType { get; set; }
    public bool IsTrial { get; set; }
}
