using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using Umbraco.Cms.Api.Management.Routing;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// Management API controller for discount/coupon operations in the Umbraco backoffice.
/// </summary>
[VersionedApiBackOfficeRoute($"{EcommerceConstants.ApiRouteBase}/{EcommerceConstants.Routes.Discounts}")]
public class DiscountManagementApiController : EcommerceManagementApiControllerBase
{
    private readonly IDiscountService _discountService;

    public DiscountManagementApiController(IDiscountService discountService)
    {
        _discountService = discountService;
    }

    /// <summary>
    /// Gets the tree structure for discounts organized by status.
    /// </summary>
    [HttpGet("tree")]
    [ProducesResponseType<DiscountTreeResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTree()
    {
        var allDiscounts = await _discountService.GetActiveAsync();
        var discountList = allDiscounts.ToList();
        var now = DateTime.UtcNow;

        var activeCount = discountList.Count(d => d.IsActive &&
            (!d.StartDate.HasValue || d.StartDate <= now) &&
            (!d.EndDate.HasValue || d.EndDate > now));

        var scheduledCount = discountList.Count(d => d.IsActive &&
            d.StartDate.HasValue && d.StartDate > now);

        var expiredCount = discountList.Count(d =>
            d.EndDate.HasValue && d.EndDate <= now);

        var nodes = new List<DiscountTreeNodeModel>
        {
            new()
            {
                Id = "active",
                Name = "Active Discounts",
                Icon = "icon-check color-green",
                Count = activeCount,
                HasChildren = activeCount > 0,
                FilterType = "active"
            },
            new()
            {
                Id = "scheduled",
                Name = "Scheduled",
                Icon = "icon-calendar",
                Count = scheduledCount,
                HasChildren = scheduledCount > 0,
                FilterType = "scheduled"
            },
            new()
            {
                Id = "expired",
                Name = "Expired",
                Icon = "icon-time color-grey",
                Count = expiredCount,
                HasChildren = expiredCount > 0,
                FilterType = "expired"
            },
            new()
            {
                Id = "all-discounts",
                Name = "All Discounts",
                Icon = EcommerceConstants.Icons.Discounts,
                Count = discountList.Count,
                HasChildren = discountList.Count > 0,
                FilterType = "all"
            }
        };

        return Ok(new DiscountTreeResponse { Nodes = nodes });
    }

    /// <summary>
    /// Gets discounts for a specific tree node filter.
    /// </summary>
    [HttpGet("tree/{nodeId}/children")]
    [ProducesResponseType<DiscountListResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTreeChildren(string nodeId)
    {
        var allDiscounts = await _discountService.GetActiveAsync();
        var discountList = allDiscounts.ToList();
        var now = DateTime.UtcNow;

        IEnumerable<Core.Models.Domain.Discount> filteredDiscounts = nodeId switch
        {
            "active" => discountList.Where(d => d.IsActive &&
                (!d.StartDate.HasValue || d.StartDate <= now) &&
                (!d.EndDate.HasValue || d.EndDate > now)),
            "scheduled" => discountList.Where(d => d.IsActive &&
                d.StartDate.HasValue && d.StartDate > now),
            "expired" => discountList.Where(d =>
                d.EndDate.HasValue && d.EndDate <= now),
            "all-discounts" => discountList,
            _ => Enumerable.Empty<Core.Models.Domain.Discount>()
        };

        var items = filteredDiscounts.Select(MapToDiscountItem).ToList();

        return Ok(new DiscountListResponse
        {
            Items = items,
            Total = items.Count
        });
    }

    /// <summary>
    /// Gets all discounts.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<DiscountListResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var discounts = await _discountService.GetActiveAsync();
        var discountList = discounts.ToList();

        if (!includeInactive)
        {
            var now = DateTime.UtcNow;
            discountList = discountList.Where(d => d.IsActive &&
                (!d.EndDate.HasValue || d.EndDate > now)).ToList();
        }

        return Ok(new DiscountListResponse
        {
            Items = discountList.Select(MapToDiscountItem).ToList(),
            Total = discountList.Count
        });
    }

    /// <summary>
    /// Gets a single discount by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<DiscountDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var discount = await _discountService.GetByIdAsync(id);
        if (discount == null)
        {
            return NotFound();
        }

        return Ok(MapToDiscountDetail(discount));
    }

    /// <summary>
    /// Validates a discount code.
    /// </summary>
    [HttpGet("validate/{code}")]
    [ProducesResponseType<DiscountValidationResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateCode(string code)
    {
        var discount = await _discountService.GetByCodeAsync(code);

        if (discount == null)
        {
            return Ok(new DiscountValidationResult
            {
                IsValid = false,
                Message = "Discount code not found"
            });
        }

        var isValid = discount.IsValid;

        var message = isValid ? "Valid" :
            !discount.IsActive ? "Discount is inactive" :
            discount.IsUsageLimitReached ? "Usage limit reached" :
            "Discount is not valid";

        return Ok(new DiscountValidationResult
        {
            IsValid = isValid,
            Message = message,
            Discount = isValid ? MapToDiscountItem(discount) : null
        });
    }

    /// <summary>
    /// Creates a new discount.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<DiscountDetailModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDiscountRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Discount name is required" });
        }

        try
        {
            var discount = new Core.Models.Domain.Discount
            {
                Name = request.Name,
                Description = request.Description,
                Code = request.Code,
                Type = Enum.TryParse<DiscountType>(request.Type, true, out var type) ? type : DiscountType.Percentage,
                Scope = Enum.TryParse<DiscountScope>(request.Scope, true, out var scope) ? scope : DiscountScope.Order,
                Value = request.Value,
                MaxDiscountAmount = request.MaxDiscountAmount,
                MinimumOrderAmount = request.MinimumOrderAmount,
                MinimumQuantity = request.MinimumQuantity,
                MaximumQuantity = request.MaximumQuantity,
                ApplicableProductIds = request.ApplicableProductIds ?? [],
                ApplicableCategoryIds = request.ApplicableCategoryIds ?? [],
                EligibleCustomerIds = request.EligibleCustomerIds ?? [],
                EligibleCustomerTiers = request.EligibleCustomerTiers ?? [],
                ExcludedProductIds = request.ExcludedProductIds ?? [],
                ExcludedCategoryIds = request.ExcludedCategoryIds ?? [],
                ExcludeSaleItems = request.ExcludeSaleItems,
                FirstTimeCustomerOnly = request.FirstTimeCustomerOnly,
                BuyQuantity = request.BuyQuantity,
                GetQuantity = request.GetQuantity,
                GetProductIds = request.GetProductIds ?? [],
                TotalUsageLimit = request.TotalUsageLimit,
                PerCustomerLimit = request.PerCustomerLimit,
                IsActive = request.IsActive,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CanCombine = request.CanCombine,
                Priority = request.Priority
            };

            var created = await _discountService.CreateAsync(discount);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDiscountDetail(created));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing discount.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<DiscountDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDiscountRequest request)
    {
        var existing = await _discountService.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        try
        {
            existing.Name = request.Name ?? existing.Name;
            existing.Description = request.Description;
            existing.Code = request.Code;

            if (!string.IsNullOrEmpty(request.Type) && Enum.TryParse<DiscountType>(request.Type, true, out var type))
            {
                existing.Type = type;
            }

            if (!string.IsNullOrEmpty(request.Scope) && Enum.TryParse<DiscountScope>(request.Scope, true, out var scope))
            {
                existing.Scope = scope;
            }

            existing.Value = request.Value;
            existing.MaxDiscountAmount = request.MaxDiscountAmount;
            existing.MinimumOrderAmount = request.MinimumOrderAmount;
            existing.MinimumQuantity = request.MinimumQuantity;
            existing.MaximumQuantity = request.MaximumQuantity;
            existing.ApplicableProductIds = request.ApplicableProductIds ?? existing.ApplicableProductIds;
            existing.ApplicableCategoryIds = request.ApplicableCategoryIds ?? existing.ApplicableCategoryIds;
            existing.EligibleCustomerIds = request.EligibleCustomerIds ?? existing.EligibleCustomerIds;
            existing.EligibleCustomerTiers = request.EligibleCustomerTiers ?? existing.EligibleCustomerTiers;
            existing.ExcludedProductIds = request.ExcludedProductIds ?? existing.ExcludedProductIds;
            existing.ExcludedCategoryIds = request.ExcludedCategoryIds ?? existing.ExcludedCategoryIds;
            existing.ExcludeSaleItems = request.ExcludeSaleItems;
            existing.FirstTimeCustomerOnly = request.FirstTimeCustomerOnly;
            existing.BuyQuantity = request.BuyQuantity;
            existing.GetQuantity = request.GetQuantity;
            existing.GetProductIds = request.GetProductIds ?? existing.GetProductIds;
            existing.TotalUsageLimit = request.TotalUsageLimit;
            existing.PerCustomerLimit = request.PerCustomerLimit;
            existing.IsActive = request.IsActive;
            existing.StartDate = request.StartDate;
            existing.EndDate = request.EndDate;
            existing.CanCombine = request.CanCombine;
            existing.Priority = request.Priority;

            var updated = await _discountService.UpdateAsync(existing);

            return Ok(MapToDiscountDetail(updated));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a discount.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _discountService.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        await _discountService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Gets usage history for a discount.
    /// </summary>
    [HttpGet("{id:guid}/usage")]
    [ProducesResponseType<DiscountUsageListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUsageHistory(
        Guid id,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10)
    {
        var discount = await _discountService.GetByIdAsync(id);
        if (discount == null)
        {
            return NotFound();
        }

        // For now, return empty list as usage tracking would need a separate repository
        // In a full implementation, this would fetch from DiscountUsage table
        return Ok(new DiscountUsageListResponse
        {
            Items = [],
            Total = 0,
            Skip = skip,
            Take = take
        });
    }

    /// <summary>
    /// Toggles the active status of a discount.
    /// </summary>
    [HttpPost("{id:guid}/toggle-status")]
    [ProducesResponseType<DiscountDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var existing = await _discountService.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.IsActive = !existing.IsActive;
        var updated = await _discountService.UpdateAsync(existing);

        return Ok(MapToDiscountDetail(updated));
    }

    /// <summary>
    /// Duplicates an existing discount.
    /// </summary>
    [HttpPost("{id:guid}/duplicate")]
    [ProducesResponseType<DiscountDetailModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Duplicate(Guid id)
    {
        var existing = await _discountService.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        // Generate unique code if the original has one
        string? newCode = null;
        if (!string.IsNullOrEmpty(existing.Code))
        {
            newCode = $"{existing.Code}-COPY-{DateTime.UtcNow:yyyyMMddHHmmss}";
        }

        // Create duplicate discount
        var duplicate = new Core.Models.Domain.Discount
        {
            Name = $"{existing.Name} (Copy)",
            Description = existing.Description,
            Code = newCode,
            Type = existing.Type,
            Scope = existing.Scope,
            Value = existing.Value,
            MaxDiscountAmount = existing.MaxDiscountAmount,
            MinimumOrderAmount = existing.MinimumOrderAmount,
            MinimumQuantity = existing.MinimumQuantity,
            MaximumQuantity = existing.MaximumQuantity,
            ApplicableProductIds = existing.ApplicableProductIds.ToList(),
            ApplicableCategoryIds = existing.ApplicableCategoryIds.ToList(),
            EligibleCustomerIds = existing.EligibleCustomerIds.ToList(),
            EligibleCustomerTiers = existing.EligibleCustomerTiers.ToList(),
            ExcludedProductIds = existing.ExcludedProductIds.ToList(),
            ExcludedCategoryIds = existing.ExcludedCategoryIds.ToList(),
            ExcludeSaleItems = existing.ExcludeSaleItems,
            FirstTimeCustomerOnly = existing.FirstTimeCustomerOnly,
            BuyQuantity = existing.BuyQuantity,
            GetQuantity = existing.GetQuantity,
            GetProductIds = existing.GetProductIds.ToList(),
            TotalUsageLimit = existing.TotalUsageLimit,
            PerCustomerLimit = existing.PerCustomerLimit,
            UsageCount = 0, // Reset usage count for the copy
            IsActive = false, // Start as inactive for review
            StartDate = null, // Clear dates for review
            EndDate = null,
            CanCombine = existing.CanCombine,
            Priority = existing.Priority
        };

        var created = await _discountService.CreateAsync(duplicate);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDiscountDetail(created));
    }

    /// <summary>
    /// Extends the end date of a discount.
    /// </summary>
    [HttpPost("{id:guid}/extend")]
    [ProducesResponseType<DiscountDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Extend(Guid id, [FromBody] ExtendDiscountRequest request)
    {
        var existing = await _discountService.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.EndDate = request.NewEndDate;
        var updated = await _discountService.UpdateAsync(existing);

        return Ok(MapToDiscountDetail(updated));
    }

    /// <summary>
    /// Resets the usage count of a discount.
    /// </summary>
    [HttpPost("{id:guid}/reset-usage")]
    [ProducesResponseType<DiscountDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetUsage(Guid id)
    {
        var existing = await _discountService.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.UsageCount = 0;
        var updated = await _discountService.UpdateAsync(existing);

        return Ok(MapToDiscountDetail(updated));
    }

    /// <summary>
    /// Updates the priority of a discount.
    /// </summary>
    [HttpPost("{id:guid}/update-priority")]
    [ProducesResponseType<DiscountDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePriority(Guid id, [FromBody] UpdatePriorityRequest request)
    {
        var existing = await _discountService.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.Priority = request.Priority;
        var updated = await _discountService.UpdateAsync(existing);

        return Ok(MapToDiscountDetail(updated));
    }

    /// <summary>
    /// Toggles whether a discount can be combined with others.
    /// </summary>
    [HttpPost("{id:guid}/toggle-combine")]
    [ProducesResponseType<DiscountDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleCombine(Guid id)
    {
        var existing = await _discountService.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.CanCombine = !existing.CanCombine;
        var updated = await _discountService.UpdateAsync(existing);

        return Ok(MapToDiscountDetail(updated));
    }

    /// <summary>
    /// Activates a discount and optionally sets start/end dates.
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType<DiscountDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id, [FromBody] ActivateDiscountRequest? request = null)
    {
        var existing = await _discountService.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.IsActive = true;
        if (request?.StartDate.HasValue == true)
        {
            existing.StartDate = request.StartDate;
        }
        if (request?.EndDate.HasValue == true)
        {
            existing.EndDate = request.EndDate;
        }

        var updated = await _discountService.UpdateAsync(existing);

        return Ok(MapToDiscountDetail(updated));
    }

    /// <summary>
    /// Deactivates a discount.
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType<DiscountDetailModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var existing = await _discountService.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.IsActive = false;
        var updated = await _discountService.UpdateAsync(existing);

        return Ok(MapToDiscountDetail(updated));
    }

    private static DiscountItemModel MapToDiscountItem(Core.Models.Domain.Discount discount)
    {
        var now = DateTime.UtcNow;
        var status = !discount.IsActive ? "Inactive" :
            (discount.StartDate.HasValue && discount.StartDate > now) ? "Scheduled" :
            (discount.EndDate.HasValue && discount.EndDate <= now) ? "Expired" :
            "Active";

        var icon = discount.IsActive ? EcommerceConstants.Icons.Discount : "icon-tag color-grey";

        return new DiscountItemModel
        {
            Id = discount.Id,
            Name = discount.Name,
            Code = discount.Code,
            Type = discount.Type.ToString(),
            Value = discount.Value,
            MinimumOrderAmount = discount.MinimumOrderAmount,
            MaxDiscountAmount = discount.MaxDiscountAmount,
            IsActive = discount.IsActive,
            Status = status,
            Icon = icon,
            StartDate = discount.StartDate,
            EndDate = discount.EndDate,
            TotalUsageLimit = discount.TotalUsageLimit,
            UsageCount = discount.UsageCount,
            DisplayValue = discount.DisplayValue,
            CreatedAt = discount.CreatedAt
        };
    }

    private static DiscountDetailModel MapToDiscountDetail(Core.Models.Domain.Discount discount)
    {
        var item = MapToDiscountItem(discount);

        return new DiscountDetailModel
        {
            Id = item.Id,
            Name = item.Name,
            Code = item.Code,
            Description = discount.Description,
            Type = item.Type,
            Value = item.Value,
            MinimumOrderAmount = item.MinimumOrderAmount,
            MaxDiscountAmount = item.MaxDiscountAmount,
            IsActive = item.IsActive,
            Status = item.Status,
            Icon = item.Icon,
            StartDate = item.StartDate,
            EndDate = item.EndDate,
            TotalUsageLimit = item.TotalUsageLimit,
            UsageCount = item.UsageCount,
            PerCustomerLimit = discount.PerCustomerLimit,
            ApplicableProductIds = discount.ApplicableProductIds,
            ApplicableCategoryIds = discount.ApplicableCategoryIds,
            ExcludedProductIds = discount.ExcludedProductIds,
            EligibleCustomerIds = discount.EligibleCustomerIds,
            CanCombine = discount.CanCombine,
            DisplayValue = item.DisplayValue,
            CreatedAt = item.CreatedAt,
            UpdatedAt = discount.UpdatedAt
        };
    }
}

#region Response Models

public class DiscountTreeResponse
{
    public List<DiscountTreeNodeModel> Nodes { get; set; } = [];
}

public class DiscountTreeNodeModel
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Icon { get; set; }
    public int Count { get; set; }
    public bool HasChildren { get; set; }
    public required string FilterType { get; set; }
}

public class DiscountListResponse
{
    public List<DiscountItemModel> Items { get; set; } = [];
    public int Total { get; set; }
}

public class DiscountItemModel
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Code { get; set; }
    public required string Type { get; set; }
    public decimal Value { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public bool IsActive { get; set; }
    public required string Status { get; set; }
    public required string Icon { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? TotalUsageLimit { get; set; }
    public int UsageCount { get; set; }
    public required string DisplayValue { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DiscountDetailModel : DiscountItemModel
{
    public string? Description { get; set; }
    public int? PerCustomerLimit { get; set; }
    public List<Guid> ApplicableProductIds { get; set; } = [];
    public List<Guid> ApplicableCategoryIds { get; set; } = [];
    public List<Guid> ExcludedProductIds { get; set; } = [];
    public List<Guid> EligibleCustomerIds { get; set; } = [];
    public bool CanCombine { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class DiscountValidationResult
{
    public bool IsValid { get; set; }
    public required string Message { get; set; }
    public DiscountItemModel? Discount { get; set; }
}

public class DiscountUsageListResponse
{
    public List<DiscountUsageItemModel> Items { get; set; } = [];
    public int Total { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}

public class DiscountUsageItemModel
{
    public Guid Id { get; set; }
    public Guid DiscountId { get; set; }
    public Guid OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

#endregion

#region Request Models

public class CreateDiscountRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Code { get; set; }
    public string Type { get; set; } = "Percentage";
    public string Scope { get; set; } = "Order";
    public decimal Value { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public int? MinimumQuantity { get; set; }
    public int? MaximumQuantity { get; set; }
    public List<Guid>? ApplicableProductIds { get; set; }
    public List<Guid>? ApplicableCategoryIds { get; set; }
    public List<Guid>? EligibleCustomerIds { get; set; }
    public List<string>? EligibleCustomerTiers { get; set; }
    public List<Guid>? ExcludedProductIds { get; set; }
    public List<Guid>? ExcludedCategoryIds { get; set; }
    public bool ExcludeSaleItems { get; set; }
    public bool FirstTimeCustomerOnly { get; set; }
    public int? BuyQuantity { get; set; }
    public int? GetQuantity { get; set; }
    public List<Guid>? GetProductIds { get; set; }
    public int? TotalUsageLimit { get; set; }
    public int? PerCustomerLimit { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool CanCombine { get; set; }
    public int Priority { get; set; }
}

public class UpdateDiscountRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Code { get; set; }
    public string? Type { get; set; }
    public string? Scope { get; set; }
    public decimal Value { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public int? MinimumQuantity { get; set; }
    public int? MaximumQuantity { get; set; }
    public List<Guid>? ApplicableProductIds { get; set; }
    public List<Guid>? ApplicableCategoryIds { get; set; }
    public List<Guid>? EligibleCustomerIds { get; set; }
    public List<string>? EligibleCustomerTiers { get; set; }
    public List<Guid>? ExcludedProductIds { get; set; }
    public List<Guid>? ExcludedCategoryIds { get; set; }
    public bool ExcludeSaleItems { get; set; }
    public bool FirstTimeCustomerOnly { get; set; }
    public int? BuyQuantity { get; set; }
    public int? GetQuantity { get; set; }
    public List<Guid>? GetProductIds { get; set; }
    public int? TotalUsageLimit { get; set; }
    public int? PerCustomerLimit { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool CanCombine { get; set; }
    public int Priority { get; set; }
}

public class ExtendDiscountRequest
{
    public DateTime NewEndDate { get; set; }
}

public class UpdatePriorityRequest
{
    public int Priority { get; set; }
}

public class ActivateDiscountRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

#endregion
