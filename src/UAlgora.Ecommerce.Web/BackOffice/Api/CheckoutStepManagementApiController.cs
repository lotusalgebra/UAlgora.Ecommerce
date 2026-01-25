using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using Umbraco.Cms.Api.Management.Routing;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// Management API controller for checkout step configuration.
/// </summary>
[VersionedApiBackOfficeRoute($"{EcommerceConstants.ApiRouteBase}/checkoutstep")]
public class CheckoutStepManagementApiController : EcommerceManagementApiControllerBase
{
    private readonly ICheckoutStepRepository _repository;

    public CheckoutStepManagementApiController(ICheckoutStepRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Gets all checkout steps.
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> GetAll([FromQuery] Guid? storeId = null, CancellationToken ct = default)
    {
        var steps = await _repository.GetAllAsync(storeId, ct);
        return Ok(new { items = steps });
    }

    /// <summary>
    /// Gets enabled checkout steps only.
    /// </summary>
    [HttpGet("enabled")]
    public async Task<IActionResult> GetEnabled([FromQuery] Guid? storeId = null, CancellationToken ct = default)
    {
        var steps = await _repository.GetEnabledAsync(storeId, ct);
        return Ok(new { items = steps });
    }

    /// <summary>
    /// Gets a checkout step by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var step = await _repository.GetByIdAsync(id, ct);
        if (step == null)
            return NotFound();

        return Ok(step);
    }

    /// <summary>
    /// Gets a checkout step by code.
    /// </summary>
    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCode(string code, [FromQuery] Guid? storeId = null, CancellationToken ct = default)
    {
        var step = await _repository.GetByCodeAsync(code, storeId, ct);
        if (step == null)
            return NotFound();

        return Ok(step);
    }

    /// <summary>
    /// Creates a new checkout step.
    /// </summary>
    [HttpPost("")]
    public async Task<IActionResult> Create([FromBody] CreateCheckoutStepRequest request, CancellationToken ct = default)
    {
        // Check for duplicate code
        var existing = await _repository.GetByCodeAsync(request.Code, request.StoreId, ct);
        if (existing != null)
            return BadRequest(new { message = $"Checkout step with code '{request.Code}' already exists" });

        var step = new CheckoutStepConfiguration
        {
            Code = request.Code,
            Name = request.Name,
            Title = request.Title,
            Description = request.Description,
            Instructions = request.Instructions,
            Icon = request.Icon,
            SortOrder = request.SortOrder,
            IsRequired = request.IsRequired,
            IsEnabled = request.IsEnabled,
            ShowOrderSummary = request.ShowOrderSummary,
            AllowBackNavigation = request.AllowBackNavigation,
            CssClass = request.CssClass,
            ValidationRules = request.ValidationRules,
            Configuration = request.Configuration,
            StoreId = request.StoreId
        };

        var created = await _repository.CreateAsync(step, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates a checkout step.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCheckoutStepRequest request, CancellationToken ct = default)
    {
        var step = await _repository.GetByIdAsync(id, ct);
        if (step == null)
            return NotFound();

        // Check for duplicate code if code is changing
        if (step.Code != request.Code)
        {
            var existing = await _repository.GetByCodeAsync(request.Code, step.StoreId, ct);
            if (existing != null)
                return BadRequest(new { message = $"Checkout step with code '{request.Code}' already exists" });
        }

        step.Code = request.Code;
        step.Name = request.Name;
        step.Title = request.Title;
        step.Description = request.Description;
        step.Instructions = request.Instructions;
        step.Icon = request.Icon;
        step.SortOrder = request.SortOrder;
        step.IsRequired = request.IsRequired;
        step.IsEnabled = request.IsEnabled;
        step.ShowOrderSummary = request.ShowOrderSummary;
        step.AllowBackNavigation = request.AllowBackNavigation;
        step.CssClass = request.CssClass;
        step.ValidationRules = request.ValidationRules;
        step.Configuration = request.Configuration;

        var updated = await _repository.UpdateAsync(step, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a checkout step.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var step = await _repository.GetByIdAsync(id, ct);
        if (step == null)
            return NotFound();

        await _repository.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Toggles the enabled status of a checkout step.
    /// </summary>
    [HttpPost("{id:guid}/toggle")]
    public async Task<IActionResult> Toggle(Guid id, CancellationToken ct = default)
    {
        var step = await _repository.GetByIdAsync(id, ct);
        if (step == null)
            return NotFound();

        step.IsEnabled = !step.IsEnabled;
        var updated = await _repository.UpdateAsync(step, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Reorders checkout steps.
    /// </summary>
    [HttpPost("reorder")]
    public async Task<IActionResult> Reorder([FromBody] ReorderCheckoutStepsRequest request, CancellationToken ct = default)
    {
        var orders = request.Items.Select(i => (i.Id, i.SortOrder));
        await _repository.ReorderAsync(orders, ct);
        return NoContent();
    }

    /// <summary>
    /// Seeds default checkout steps if none exist.
    /// </summary>
    [HttpPost("seed-defaults")]
    public async Task<IActionResult> SeedDefaults([FromQuery] Guid? storeId = null, CancellationToken ct = default)
    {
        var existing = await _repository.GetAllAsync(storeId, ct);
        if (existing.Any())
            return BadRequest(new { message = "Checkout steps already exist. Delete them first to seed defaults." });

        var defaults = new[]
        {
            new CheckoutStepConfiguration
            {
                Code = "information",
                Name = "Information",
                Title = "Contact Information",
                Description = "Enter your contact details",
                Icon = "icon-user",
                SortOrder = 1,
                IsRequired = true,
                IsEnabled = true,
                ShowOrderSummary = true,
                AllowBackNavigation = false,
                StoreId = storeId
            },
            new CheckoutStepConfiguration
            {
                Code = "shipping",
                Name = "Shipping",
                Title = "Shipping Address",
                Description = "Where should we deliver your order?",
                Icon = "icon-truck",
                SortOrder = 2,
                IsRequired = true,
                IsEnabled = true,
                ShowOrderSummary = true,
                AllowBackNavigation = true,
                StoreId = storeId
            },
            new CheckoutStepConfiguration
            {
                Code = "payment",
                Name = "Payment",
                Title = "Payment Method",
                Description = "Select your payment method",
                Icon = "icon-credit-card",
                SortOrder = 3,
                IsRequired = true,
                IsEnabled = true,
                ShowOrderSummary = true,
                AllowBackNavigation = true,
                StoreId = storeId
            },
            new CheckoutStepConfiguration
            {
                Code = "review",
                Name = "Review",
                Title = "Review Order",
                Description = "Review your order before placing it",
                Icon = "icon-check",
                SortOrder = 4,
                IsRequired = true,
                IsEnabled = true,
                ShowOrderSummary = true,
                AllowBackNavigation = true,
                StoreId = storeId
            }
        };

        foreach (var step in defaults)
        {
            await _repository.CreateAsync(step, ct);
        }

        return Ok(new { message = "Default checkout steps created", count = defaults.Length });
    }
}

#region Request Models

public class CreateCheckoutStepRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Instructions { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsRequired { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
    public bool ShowOrderSummary { get; set; } = true;
    public bool AllowBackNavigation { get; set; } = true;
    public string? CssClass { get; set; }
    public string? ValidationRules { get; set; }
    public string? Configuration { get; set; }
    public Guid? StoreId { get; set; }
}

public class UpdateCheckoutStepRequest : CreateCheckoutStepRequest { }

public class ReorderCheckoutStepsRequest
{
    public List<ReorderItem> Items { get; set; } = new();
}

public class ReorderItem
{
    public Guid Id { get; set; }
    public int SortOrder { get; set; }
}

#endregion
