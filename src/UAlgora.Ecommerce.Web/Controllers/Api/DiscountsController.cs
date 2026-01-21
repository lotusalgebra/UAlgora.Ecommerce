using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using static UAlgora.Ecommerce.Web.ServiceCollectionExtensions;

namespace UAlgora.Ecommerce.Web.Controllers.Api;

/// <summary>
/// API controller for public discount/coupon operations.
/// </summary>
public class DiscountsController : EcommerceApiController
{
    private readonly IDiscountService _discountService;
    private readonly ICartService _cartService;
    private readonly ICartContextProvider _contextProvider;

    public DiscountsController(
        IDiscountService discountService,
        ICartService cartService,
        ICartContextProvider contextProvider)
    {
        _discountService = discountService;
        _cartService = cartService;
        _contextProvider = contextProvider;
    }

    /// <summary>
    /// Validates a coupon code.
    /// </summary>
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateCoupon(
        [FromBody] ValidateCouponRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.CouponCode))
        {
            return BadRequest(new ApiErrorResponse { Message = "Coupon code is required." });
        }

        var cart = await _cartService.GetCartAsync(ct);
        var result = await _discountService.ValidateCouponAsync(
            request.CouponCode,
            cart,
            _contextProvider.GetCustomerId(),
            ct);

        return ApiSuccess(result);
    }
}

/// <summary>
/// Admin API controller for discount management.
/// </summary>
[Route("api/ecommerce/admin/discounts")]
[Authorize(Policy = EcommerceAdminPolicy)]
public class DiscountsAdminController : EcommerceApiController
{
    private readonly IDiscountService _discountService;

    public DiscountsAdminController(IDiscountService discountService)
    {
        _discountService = discountService;
    }

    /// <summary>
    /// Gets all active discounts.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDiscounts(CancellationToken ct = default)
    {
        var discounts = await _discountService.GetActiveAsync(ct);
        return ApiSuccess(discounts);
    }

    /// <summary>
    /// Gets a discount by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDiscount(Guid id, CancellationToken ct = default)
    {
        var discount = await _discountService.GetByIdAsync(id, ct);
        if (discount == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Discount not found." });
        }

        return ApiSuccess(discount);
    }

    /// <summary>
    /// Gets a discount by code.
    /// </summary>
    [HttpGet("by-code/{code}")]
    public async Task<IActionResult> GetDiscountByCode(string code, CancellationToken ct = default)
    {
        var discount = await _discountService.GetByCodeAsync(code, ct);
        if (discount == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Discount not found." });
        }

        return ApiSuccess(discount);
    }

    /// <summary>
    /// Creates a new discount.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateDiscount(
        [FromBody] CreateDiscountRequest request,
        CancellationToken ct = default)
    {
        var discount = new Discount
        {
            Name = request.Name,
            Description = request.Description,
            Code = request.Code,
            Type = request.Type,
            Scope = request.Scope,
            Value = request.Value,
            MaxDiscountAmount = request.MaxDiscountAmount,
            MinimumOrderAmount = request.MinimumOrderAmount,
            MinimumQuantity = request.MinimumQuantity,
            TotalUsageLimit = request.TotalUsageLimit,
            PerCustomerLimit = request.PerCustomerLimit,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive,
            CanCombine = request.CanCombine,
            Priority = request.Priority,
            ApplicableProductIds = request.ApplicableProductIds ?? [],
            ApplicableCategoryIds = request.ApplicableCategoryIds ?? [],
            ExcludedProductIds = request.ExcludedProductIds ?? [],
            ExcludedCategoryIds = request.ExcludedCategoryIds ?? [],
            BuyQuantity = request.BuyQuantity,
            GetQuantity = request.GetQuantity
        };

        var validation = await _discountService.ValidateAsync(discount, ct);
        if (!validation.IsValid)
        {
            return BadRequest(new ApiErrorResponse
            {
                Message = "Validation failed.",
                Errors = validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            });
        }

        var created = await _discountService.CreateAsync(discount, ct);
        return ApiSuccess(created, "Discount created.");
    }

    /// <summary>
    /// Updates a discount.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateDiscount(
        Guid id,
        [FromBody] UpdateDiscountRequest request,
        CancellationToken ct = default)
    {
        var discount = await _discountService.GetByIdAsync(id, ct);
        if (discount == null)
        {
            return NotFound(new ApiErrorResponse { Message = "Discount not found." });
        }

        discount.Name = request.Name ?? discount.Name;
        discount.Description = request.Description ?? discount.Description;
        discount.Value = request.Value ?? discount.Value;
        discount.MaxDiscountAmount = request.MaxDiscountAmount ?? discount.MaxDiscountAmount;
        discount.MinimumOrderAmount = request.MinimumOrderAmount ?? discount.MinimumOrderAmount;
        discount.TotalUsageLimit = request.TotalUsageLimit ?? discount.TotalUsageLimit;
        discount.PerCustomerLimit = request.PerCustomerLimit ?? discount.PerCustomerLimit;
        discount.StartDate = request.StartDate ?? discount.StartDate;
        discount.EndDate = request.EndDate ?? discount.EndDate;
        discount.IsActive = request.IsActive ?? discount.IsActive;
        discount.CanCombine = request.CanCombine ?? discount.CanCombine;
        discount.Priority = request.Priority ?? discount.Priority;

        if (request.ApplicableProductIds != null)
            discount.ApplicableProductIds = request.ApplicableProductIds;
        if (request.ApplicableCategoryIds != null)
            discount.ApplicableCategoryIds = request.ApplicableCategoryIds;

        var validation = await _discountService.ValidateAsync(discount, ct);
        if (!validation.IsValid)
        {
            return BadRequest(new ApiErrorResponse
            {
                Message = "Validation failed.",
                Errors = validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            });
        }

        var updated = await _discountService.UpdateAsync(discount, ct);
        return ApiSuccess(updated, "Discount updated.");
    }

    /// <summary>
    /// Deletes a discount.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteDiscount(Guid id, CancellationToken ct = default)
    {
        await _discountService.DeleteAsync(id, ct);
        return ApiSuccess(new { }, "Discount deleted.");
    }

    /// <summary>
    /// Generates coupon codes.
    /// </summary>
    [HttpPost("generate-codes")]
    public async Task<IActionResult> GenerateCodes(
        [FromBody] GenerateCodesRequest request,
        CancellationToken ct = default)
    {
        if (request.Count <= 0 || request.Count > 100)
        {
            return BadRequest(new ApiErrorResponse { Message = "Count must be between 1 and 100." });
        }

        var codes = await _discountService.GenerateCodesAsync(request.Count, request.Prefix, ct);
        return ApiSuccess(codes);
    }

    /// <summary>
    /// Deactivates expired discounts.
    /// </summary>
    [HttpPost("deactivate-expired")]
    public async Task<IActionResult> DeactivateExpired(CancellationToken ct = default)
    {
        await _discountService.DeactivateExpiredAsync(ct);
        return ApiSuccess(new { }, "Expired discounts deactivated.");
    }
}

#region Request Models

public class ValidateCouponRequest
{
    public string CouponCode { get; set; } = string.Empty;
}

public class CreateDiscountRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Code { get; set; }
    public DiscountType Type { get; set; } = DiscountType.Percentage;
    public DiscountScope Scope { get; set; } = DiscountScope.Order;
    public decimal Value { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public int? MinimumQuantity { get; set; }
    public int? TotalUsageLimit { get; set; }
    public int? PerCustomerLimit { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool CanCombine { get; set; }
    public int Priority { get; set; }
    public List<Guid>? ApplicableProductIds { get; set; }
    public List<Guid>? ApplicableCategoryIds { get; set; }
    public List<Guid>? ExcludedProductIds { get; set; }
    public List<Guid>? ExcludedCategoryIds { get; set; }
    public int? BuyQuantity { get; set; }
    public int? GetQuantity { get; set; }
}

public class UpdateDiscountRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Value { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public int? TotalUsageLimit { get; set; }
    public int? PerCustomerLimit { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsActive { get; set; }
    public bool? CanCombine { get; set; }
    public int? Priority { get; set; }
    public List<Guid>? ApplicableProductIds { get; set; }
    public List<Guid>? ApplicableCategoryIds { get; set; }
}

public class GenerateCodesRequest
{
    public int Count { get; set; } = 1;
    public string? Prefix { get; set; }
}

#endregion
