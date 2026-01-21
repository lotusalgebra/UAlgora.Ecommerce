using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Services;

/// <summary>
/// Service interface for discount operations.
/// </summary>
public interface IDiscountService
{
    /// <summary>
    /// Gets a discount by ID.
    /// </summary>
    Task<Discount?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a discount by code.
    /// </summary>
    Task<Discount?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Gets all active discounts.
    /// </summary>
    Task<IReadOnlyList<Discount>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets automatic discounts applicable to a cart.
    /// </summary>
    Task<IReadOnlyList<Discount>> GetAutomaticDiscountsForCartAsync(Cart cart, CancellationToken ct = default);

    /// <summary>
    /// Validates a coupon code.
    /// </summary>
    Task<CouponValidationResult> ValidateCouponAsync(string code, Cart cart, Guid? customerId = null, CancellationToken ct = default);

    /// <summary>
    /// Calculates discount amount for a cart.
    /// </summary>
    Task<DiscountCalculation> CalculateDiscountAsync(Discount discount, Cart cart, CancellationToken ct = default);

    /// <summary>
    /// Calculates all applicable discounts for a cart.
    /// </summary>
    Task<CartDiscountCalculation> CalculateCartDiscountsAsync(Cart cart, string? couponCode = null, CancellationToken ct = default);

    /// <summary>
    /// Creates a new discount.
    /// </summary>
    Task<Discount> CreateAsync(Discount discount, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing discount.
    /// </summary>
    Task<Discount> UpdateAsync(Discount discount, CancellationToken ct = default);

    /// <summary>
    /// Deletes a discount.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Records discount usage.
    /// </summary>
    Task RecordUsageAsync(Guid discountId, Guid orderId, Guid? customerId, decimal amount, CancellationToken ct = default);

    /// <summary>
    /// Generates unique coupon codes.
    /// </summary>
    Task<IReadOnlyList<string>> GenerateCodesAsync(int count, string? prefix = null, CancellationToken ct = default);

    /// <summary>
    /// Validates a discount.
    /// </summary>
    Task<ValidationResult> ValidateAsync(Discount discount, CancellationToken ct = default);

    /// <summary>
    /// Deactivates expired discounts.
    /// </summary>
    Task DeactivateExpiredAsync(CancellationToken ct = default);
}

/// <summary>
/// Result of coupon validation.
/// </summary>
public class CouponValidationResult
{
    public bool IsValid { get; set; }
    public Discount? Discount { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }

    public static CouponValidationResult Success(Discount discount) => new()
    {
        IsValid = true,
        Discount = discount
    };

    public static CouponValidationResult Failure(string errorCode, string message) => new()
    {
        IsValid = false,
        ErrorCode = errorCode,
        ErrorMessage = message
    };
}

/// <summary>
/// Discount calculation result.
/// </summary>
public class DiscountCalculation
{
    public Guid DiscountId { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public DiscountType Type { get; set; }
    public decimal Amount { get; set; }
    public List<LineDiscountAllocation> LineAllocations { get; set; } = [];
}

/// <summary>
/// Discount allocation per line item.
/// </summary>
public class LineDiscountAllocation
{
    public Guid CartItemId { get; set; }
    public decimal Amount { get; set; }
}

/// <summary>
/// Cart discount calculation result.
/// </summary>
public class CartDiscountCalculation
{
    public decimal TotalDiscount { get; set; }
    public List<DiscountCalculation> AppliedDiscounts { get; set; } = [];
    public decimal Subtotal { get; set; }
    public decimal DiscountedSubtotal => Subtotal - TotalDiscount;
}
