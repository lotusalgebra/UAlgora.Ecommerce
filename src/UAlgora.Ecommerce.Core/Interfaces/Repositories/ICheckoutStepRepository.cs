using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for checkout step configuration operations.
/// </summary>
public interface ICheckoutStepRepository
{
    Task<IEnumerable<CheckoutStepConfiguration>> GetAllAsync(Guid? storeId = null, CancellationToken ct = default);
    Task<IEnumerable<CheckoutStepConfiguration>> GetEnabledAsync(Guid? storeId = null, CancellationToken ct = default);
    Task<CheckoutStepConfiguration?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CheckoutStepConfiguration?> GetByCodeAsync(string code, Guid? storeId = null, CancellationToken ct = default);
    Task<CheckoutStepConfiguration> CreateAsync(CheckoutStepConfiguration step, CancellationToken ct = default);
    Task<CheckoutStepConfiguration> UpdateAsync(CheckoutStepConfiguration step, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task ReorderAsync(IEnumerable<(Guid Id, int SortOrder)> orders, CancellationToken ct = default);
}
