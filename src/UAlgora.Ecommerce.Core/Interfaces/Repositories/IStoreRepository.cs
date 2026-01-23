using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for Store operations.
/// </summary>
public interface IStoreRepository : ISoftDeleteRepository<Store>
{
    /// <summary>
    /// Get a store by its unique code.
    /// </summary>
    Task<Store?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Get a store by domain name.
    /// </summary>
    Task<Store?> GetByDomainAsync(string domain, CancellationToken ct = default);

    /// <summary>
    /// Get a store by its Umbraco node ID.
    /// </summary>
    Task<Store?> GetByUmbracoNodeIdAsync(int nodeId, CancellationToken ct = default);

    /// <summary>
    /// Get all active stores.
    /// </summary>
    Task<IReadOnlyList<Store>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Get stores by status.
    /// </summary>
    Task<IReadOnlyList<Store>> GetByStatusAsync(StoreStatus status, CancellationToken ct = default);

    /// <summary>
    /// Get stores with expiring trials.
    /// </summary>
    Task<IReadOnlyList<Store>> GetExpiringTrialsAsync(int daysUntilExpiry, CancellationToken ct = default);

    /// <summary>
    /// Check if a store code already exists.
    /// </summary>
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default);

    /// <summary>
    /// Check if a domain is already used.
    /// </summary>
    Task<bool> DomainExistsAsync(string domain, Guid? excludeId = null, CancellationToken ct = default);

    /// <summary>
    /// Get the next order number for a store and increment the sequence.
    /// </summary>
    Task<string> GetNextOrderNumberAsync(Guid storeId, CancellationToken ct = default);
}
