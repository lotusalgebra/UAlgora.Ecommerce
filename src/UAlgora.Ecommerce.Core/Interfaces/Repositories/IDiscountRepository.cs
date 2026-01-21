using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for discount operations.
/// </summary>
public interface IDiscountRepository : ISoftDeleteRepository<Discount>
{
    /// <summary>
    /// Gets a discount by code.
    /// </summary>
    Task<Discount?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Gets all active discounts.
    /// </summary>
    Task<IReadOnlyList<Discount>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets active automatic discounts (non-coupon).
    /// </summary>
    Task<IReadOnlyList<Discount>> GetActiveAutomaticAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets discounts by type.
    /// </summary>
    Task<IReadOnlyList<Discount>> GetByTypeAsync(DiscountType type, CancellationToken ct = default);

    /// <summary>
    /// Gets discounts applicable to a product.
    /// </summary>
    Task<IReadOnlyList<Discount>> GetApplicableToProductAsync(Guid productId, CancellationToken ct = default);

    /// <summary>
    /// Gets discounts applicable to a category.
    /// </summary>
    Task<IReadOnlyList<Discount>> GetApplicableToCategoryAsync(Guid categoryId, CancellationToken ct = default);

    /// <summary>
    /// Gets discounts valid for a date range.
    /// </summary>
    Task<IReadOnlyList<Discount>> GetValidForDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);

    /// <summary>
    /// Checks if a code exists.
    /// </summary>
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default);

    /// <summary>
    /// Increments usage count.
    /// </summary>
    Task IncrementUsageCountAsync(Guid discountId, CancellationToken ct = default);

    /// <summary>
    /// Gets usage count for a customer.
    /// </summary>
    Task<int> GetCustomerUsageCountAsync(Guid discountId, Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Records discount usage.
    /// </summary>
    Task RecordUsageAsync(DiscountUsage usage, CancellationToken ct = default);

    /// <summary>
    /// Gets expired discounts.
    /// </summary>
    Task<IReadOnlyList<Discount>> GetExpiredAsync(CancellationToken ct = default);

    /// <summary>
    /// Deactivates expired discounts.
    /// </summary>
    Task DeactivateExpiredAsync(CancellationToken ct = default);
}
