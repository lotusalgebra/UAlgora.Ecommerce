using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Services;

/// <summary>
/// Service implementation for discount operations.
/// </summary>
public class DiscountService : IDiscountService
{
    private readonly IDiscountRepository _discountRepository;
    private readonly ICustomerRepository _customerRepository;

    public DiscountService(
        IDiscountRepository discountRepository,
        ICustomerRepository customerRepository)
    {
        _discountRepository = discountRepository;
        _customerRepository = customerRepository;
    }

    public async Task<Discount?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _discountRepository.GetByIdAsync(id, ct);
    }

    public async Task<Discount?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _discountRepository.GetByCodeAsync(code, ct);
    }

    public async Task<IReadOnlyList<Discount>> GetActiveAsync(CancellationToken ct = default)
    {
        return await _discountRepository.GetActiveAsync(ct);
    }

    public async Task<IReadOnlyList<Discount>> GetAutomaticDiscountsForCartAsync(
        Cart cart,
        CancellationToken ct = default)
    {
        var automaticDiscounts = await _discountRepository.GetActiveAutomaticAsync(ct);
        var applicableDiscounts = new List<Discount>();

        foreach (var discount in automaticDiscounts)
        {
            if (IsApplicableToCart(discount, cart))
            {
                applicableDiscounts.Add(discount);
            }
        }

        return applicableDiscounts;
    }

    public async Task<CouponValidationResult> ValidateCouponAsync(
        string code,
        Cart cart,
        Guid? customerId = null,
        CancellationToken ct = default)
    {
        var discount = await _discountRepository.GetByCodeAsync(code, ct);

        if (discount == null)
        {
            return CouponValidationResult.Failure("INVALID_CODE", "Coupon code not found.");
        }

        // Check if discount is active
        if (!discount.IsActive)
        {
            return CouponValidationResult.Failure("INACTIVE", "This coupon is no longer active.");
        }

        // Check date validity
        var now = DateTime.UtcNow;
        if (discount.StartDate.HasValue && discount.StartDate.Value > now)
        {
            return CouponValidationResult.Failure("NOT_STARTED", "This coupon is not yet valid.");
        }

        if (discount.EndDate.HasValue && discount.EndDate.Value < now)
        {
            return CouponValidationResult.Failure("EXPIRED", "This coupon has expired.");
        }

        // Check usage limits
        if (discount.TotalUsageLimit.HasValue && discount.UsageCount >= discount.TotalUsageLimit.Value)
        {
            return CouponValidationResult.Failure("USAGE_LIMIT_REACHED", "This coupon has reached its usage limit.");
        }

        // Check per-customer usage limit
        if (customerId.HasValue && discount.PerCustomerLimit.HasValue)
        {
            var customerUsage = await _discountRepository.GetCustomerUsageCountAsync(discount.Id, customerId.Value, ct);
            if (customerUsage >= discount.PerCustomerLimit.Value)
            {
                return CouponValidationResult.Failure("CUSTOMER_USAGE_LIMIT", "You have already used this coupon the maximum number of times.");
            }
        }

        // Check minimum order amount
        if (discount.MinimumOrderAmount.HasValue && cart.Subtotal < discount.MinimumOrderAmount.Value)
        {
            return CouponValidationResult.Failure("MINIMUM_NOT_MET",
                $"Minimum order amount of {discount.MinimumOrderAmount:C} required.");
        }

        // Check if applicable to cart
        if (!IsApplicableToCart(discount, cart))
        {
            return CouponValidationResult.Failure("NOT_APPLICABLE", "This coupon is not applicable to your cart.");
        }

        return CouponValidationResult.Success(discount);
    }

    public async Task<DiscountCalculation> CalculateDiscountAsync(
        Discount discount,
        Cart cart,
        CancellationToken ct = default)
    {
        var calculation = new DiscountCalculation
        {
            DiscountId = discount.Id,
            Code = discount.Code,
            Name = discount.Name,
            Type = discount.Type
        };

        decimal totalDiscount = 0;

        switch (discount.Type)
        {
            case DiscountType.Percentage:
                totalDiscount = CalculatePercentageDiscount(discount, cart, calculation);
                break;

            case DiscountType.FixedAmount:
                totalDiscount = CalculateFixedDiscount(discount, cart, calculation);
                break;

            case DiscountType.FreeShipping:
                totalDiscount = cart.ShippingTotal;
                break;

            case DiscountType.BuyXGetY:
                totalDiscount = CalculateBuyXGetYDiscount(discount, cart, calculation);
                break;
        }

        // Apply maximum discount if set
        if (discount.MaxDiscountAmount.HasValue && totalDiscount > discount.MaxDiscountAmount.Value)
        {
            totalDiscount = discount.MaxDiscountAmount.Value;
        }

        calculation.Amount = totalDiscount;
        return await Task.FromResult(calculation);
    }

    public async Task<CartDiscountCalculation> CalculateCartDiscountsAsync(
        Cart cart,
        string? couponCode = null,
        CancellationToken ct = default)
    {
        var calculation = new CartDiscountCalculation
        {
            Subtotal = cart.Subtotal
        };

        // Get automatic discounts
        var automaticDiscounts = await GetAutomaticDiscountsForCartAsync(cart, ct);
        foreach (var discount in automaticDiscounts)
        {
            var discountCalc = await CalculateDiscountAsync(discount, cart, ct);
            if (discountCalc.Amount > 0)
            {
                calculation.AppliedDiscounts.Add(discountCalc);
                calculation.TotalDiscount += discountCalc.Amount;
            }
        }

        // Apply coupon discount
        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            var couponDiscount = await _discountRepository.GetByCodeAsync(couponCode, ct);
            if (couponDiscount != null)
            {
                var couponCalc = await CalculateDiscountAsync(couponDiscount, cart, ct);
                if (couponCalc.Amount > 0)
                {
                    calculation.AppliedDiscounts.Add(couponCalc);
                    calculation.TotalDiscount += couponCalc.Amount;
                }
            }
        }

        // Ensure discount doesn't exceed subtotal
        if (calculation.TotalDiscount > cart.Subtotal)
        {
            calculation.TotalDiscount = cart.Subtotal;
        }

        return calculation;
    }

    public async Task<Discount> CreateAsync(Discount discount, CancellationToken ct = default)
    {
        return await _discountRepository.AddAsync(discount, ct);
    }

    public async Task<Discount> UpdateAsync(Discount discount, CancellationToken ct = default)
    {
        return await _discountRepository.UpdateAsync(discount, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _discountRepository.SoftDeleteAsync(id, ct);
    }

    public async Task RecordUsageAsync(
        Guid discountId,
        Guid orderId,
        Guid? customerId,
        decimal amount,
        CancellationToken ct = default)
    {
        var usage = new DiscountUsage
        {
            DiscountId = discountId,
            OrderId = orderId,
            CustomerId = customerId,
            DiscountAmount = amount
        };

        await _discountRepository.RecordUsageAsync(usage, ct);
        await _discountRepository.IncrementUsageCountAsync(discountId, ct);
    }

    public async Task<IReadOnlyList<string>> GenerateCodesAsync(
        int count,
        string? prefix = null,
        CancellationToken ct = default)
    {
        var codes = new List<string>();
        var prefixStr = string.IsNullOrWhiteSpace(prefix) ? "" : prefix.ToUpperInvariant() + "-";

        for (int i = 0; i < count; i++)
        {
            string code;
            do
            {
                code = prefixStr + GenerateRandomCode(8);
            }
            while (await _discountRepository.CodeExistsAsync(code, null, ct) || codes.Contains(code));

            codes.Add(code);
        }

        return codes;
    }

    public async Task<ValidationResult> ValidateAsync(Discount discount, CancellationToken ct = default)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(discount.Name))
        {
            errors.Add(new ValidationError { PropertyName = "Name", ErrorMessage = "Discount name is required." });
        }

        if (!string.IsNullOrWhiteSpace(discount.Code))
        {
            var excludeId = discount.Id == Guid.Empty ? null : (Guid?)discount.Id;
            if (await _discountRepository.CodeExistsAsync(discount.Code, excludeId, ct))
            {
                errors.Add(new ValidationError { PropertyName = "Code", ErrorMessage = "Coupon code already exists." });
            }
        }

        if (discount.Type == DiscountType.Percentage && (discount.Value <= 0 || discount.Value > 100))
        {
            errors.Add(new ValidationError { PropertyName = "Value", ErrorMessage = "Percentage must be between 0 and 100." });
        }

        if (discount.Type == DiscountType.FixedAmount && discount.Value <= 0)
        {
            errors.Add(new ValidationError { PropertyName = "Value", ErrorMessage = "Discount amount must be greater than 0." });
        }

        if (discount.StartDate.HasValue && discount.EndDate.HasValue && discount.StartDate > discount.EndDate)
        {
            errors.Add(new ValidationError { PropertyName = "EndDate", ErrorMessage = "End date must be after start date." });
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }

    public async Task DeactivateExpiredAsync(CancellationToken ct = default)
    {
        await _discountRepository.DeactivateExpiredAsync(ct);
    }

    private bool IsApplicableToCart(Discount discount, Cart cart)
    {
        // If no restrictions, applicable to all
        if (!discount.ApplicableProductIds.Any() && !discount.ApplicableCategoryIds.Any())
        {
            return true;
        }

        // Check if any cart item matches
        foreach (var item in cart.Items)
        {
            if (discount.ApplicableProductIds.Contains(item.ProductId))
            {
                return true;
            }
        }

        return false;
    }

    private decimal CalculatePercentageDiscount(Discount discount, Cart cart, DiscountCalculation calculation)
    {
        decimal totalDiscount = 0;

        if (!discount.ApplicableProductIds.Any() && !discount.ApplicableCategoryIds.Any())
        {
            // Apply to entire cart
            totalDiscount = cart.Subtotal * (discount.Value / 100m);
            foreach (var item in cart.Items)
            {
                calculation.LineAllocations.Add(new LineDiscountAllocation
                {
                    CartItemId = item.Id,
                    Amount = item.LineTotal * (discount.Value / 100m)
                });
            }
        }
        else
        {
            // Apply only to applicable items
            foreach (var item in cart.Items)
            {
                if (discount.ApplicableProductIds.Contains(item.ProductId))
                {
                    var itemDiscount = item.LineTotal * (discount.Value / 100m);
                    totalDiscount += itemDiscount;
                    calculation.LineAllocations.Add(new LineDiscountAllocation
                    {
                        CartItemId = item.Id,
                        Amount = itemDiscount
                    });
                }
            }
        }

        return totalDiscount;
    }

    private decimal CalculateFixedDiscount(Discount discount, Cart cart, DiscountCalculation calculation)
    {
        if (!discount.ApplicableProductIds.Any() && !discount.ApplicableCategoryIds.Any())
        {
            // Apply to entire cart, allocate proportionally
            var discountAmount = Math.Min(discount.Value, cart.Subtotal);
            foreach (var item in cart.Items)
            {
                var proportion = item.LineTotal / cart.Subtotal;
                calculation.LineAllocations.Add(new LineDiscountAllocation
                {
                    CartItemId = item.Id,
                    Amount = discountAmount * proportion
                });
            }
            return discountAmount;
        }
        else
        {
            // Apply only to applicable items
            decimal applicableTotal = 0;
            var applicableItems = new List<CartItem>();

            foreach (var item in cart.Items)
            {
                if (discount.ApplicableProductIds.Contains(item.ProductId))
                {
                    applicableTotal += item.LineTotal;
                    applicableItems.Add(item);
                }
            }

            var discountAmount = Math.Min(discount.Value, applicableTotal);
            foreach (var item in applicableItems)
            {
                var proportion = item.LineTotal / applicableTotal;
                calculation.LineAllocations.Add(new LineDiscountAllocation
                {
                    CartItemId = item.Id,
                    Amount = discountAmount * proportion
                });
            }

            return discountAmount;
        }
    }

    private decimal CalculateBuyXGetYDiscount(Discount discount, Cart cart, DiscountCalculation calculation)
    {
        // Simple BOGO implementation
        // BuyQuantity is stored in MinimumQuantity, GetQuantity in MaximumQuantity
        // The free items would be the lowest priced items
        var buyQty = discount.MinimumQuantity ?? 1;
        var getQty = discount.MaximumQuantity ?? 1;

        var applicableItems = cart.Items
            .Where(i => !discount.ApplicableProductIds.Any() || discount.ApplicableProductIds.Contains(i.ProductId))
            .OrderBy(i => i.UnitPrice)
            .ToList();

        int totalQuantity = applicableItems.Sum(i => i.Quantity);
        int setsEligible = totalQuantity / (buyQty + getQty);
        int freeItems = setsEligible * getQty;

        decimal totalDiscount = 0;
        int freeAllocated = 0;

        foreach (var item in applicableItems)
        {
            if (freeAllocated >= freeItems) break;

            var freeFromThisItem = Math.Min(item.Quantity, freeItems - freeAllocated);
            var itemDiscount = freeFromThisItem * item.UnitPrice;
            totalDiscount += itemDiscount;
            freeAllocated += freeFromThisItem;

            calculation.LineAllocations.Add(new LineDiscountAllocation
            {
                CartItemId = item.Id,
                Amount = itemDiscount
            });
        }

        return totalDiscount;
    }

    private static string GenerateRandomCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = Random.Shared;
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
