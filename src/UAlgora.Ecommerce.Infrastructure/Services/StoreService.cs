using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Services;

/// <summary>
/// Service implementation for Store operations.
/// </summary>
public class StoreService : IStoreService
{
    private readonly IStoreRepository _storeRepository;

    public StoreService(IStoreRepository storeRepository)
    {
        _storeRepository = storeRepository;
    }

    public async Task<Store?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _storeRepository.GetByIdAsync(id, ct);
    }

    public async Task<Store?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _storeRepository.GetByCodeAsync(code, ct);
    }

    public async Task<Store?> GetByDomainAsync(string domain, CancellationToken ct = default)
    {
        return await _storeRepository.GetByDomainAsync(domain, ct);
    }

    public async Task<IReadOnlyList<Store>> GetActiveAsync(CancellationToken ct = default)
    {
        return await _storeRepository.GetActiveAsync(ct);
    }

    public async Task<IReadOnlyList<Store>> GetAllAsync(CancellationToken ct = default)
    {
        return await _storeRepository.GetAllAsync(ct);
    }

    public async Task<Store> CreateAsync(Store store, CancellationToken ct = default)
    {
        // Validate unique code
        if (await _storeRepository.CodeExistsAsync(store.Code, ct: ct))
        {
            throw new InvalidOperationException($"Store with code '{store.Code}' already exists.");
        }

        // Validate unique domain
        if (!string.IsNullOrEmpty(store.Domain) &&
            await _storeRepository.DomainExistsAsync(store.Domain, ct: ct))
        {
            throw new InvalidOperationException($"Store with domain '{store.Domain}' already exists.");
        }

        // Set defaults for new store
        if (store.LicenseType == LicenseType.Trial && !store.TrialExpiresAt.HasValue)
        {
            store.TrialExpiresAt = DateTime.UtcNow.AddDays(14);
        }

        return await _storeRepository.AddAsync(store, ct);
    }

    public async Task<Store> UpdateAsync(Store store, CancellationToken ct = default)
    {
        // Validate unique code
        if (await _storeRepository.CodeExistsAsync(store.Code, store.Id, ct))
        {
            throw new InvalidOperationException($"Store with code '{store.Code}' already exists.");
        }

        // Validate unique domain
        if (!string.IsNullOrEmpty(store.Domain) &&
            await _storeRepository.DomainExistsAsync(store.Domain, store.Id, ct))
        {
            throw new InvalidOperationException($"Store with domain '{store.Domain}' already exists.");
        }

        return await _storeRepository.UpdateAsync(store, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _storeRepository.SoftDeleteAsync(id, ct);
    }

    public async Task<LicenseValidationResult> ValidateLicenseAsync(Guid storeId, CancellationToken ct = default)
    {
        var store = await _storeRepository.GetByIdAsync(storeId, ct);
        if (store == null)
        {
            return LicenseValidationResult.InvalidKey;
        }

        // Check trial expiration
        if (store.IsTrial)
        {
            if (store.IsTrialExpired)
            {
                return LicenseValidationResult.Expired;
            }
            return LicenseValidationResult.Valid;
        }

        // Check license
        if (string.IsNullOrEmpty(store.LicenseKey))
        {
            return LicenseValidationResult.InvalidKey;
        }

        if (store.LicenseExpiresAt.HasValue && store.LicenseExpiresAt < DateTime.UtcNow)
        {
            return LicenseValidationResult.Expired;
        }

        return LicenseValidationResult.Valid;
    }

    public async Task<Store?> GetCurrentStoreAsync(CancellationToken ct = default)
    {
        // In a multi-store scenario, this should be resolved from the request context
        // For now, return the first active store or null
        var stores = await _storeRepository.GetActiveAsync(ct);
        return stores.FirstOrDefault();
    }

    public async Task<string> GetNextOrderNumberAsync(Guid storeId, CancellationToken ct = default)
    {
        return await _storeRepository.GetNextOrderNumberAsync(storeId, ct);
    }

    public async Task<bool> IsTrialAsync(Guid storeId, CancellationToken ct = default)
    {
        var store = await _storeRepository.GetByIdAsync(storeId, ct);
        return store?.IsTrial ?? false;
    }

    public async Task<IReadOnlyList<Store>> GetExpiringTrialsAsync(int daysUntilExpiry = 7, CancellationToken ct = default)
    {
        return await _storeRepository.GetExpiringTrialsAsync(daysUntilExpiry, ct);
    }
}
