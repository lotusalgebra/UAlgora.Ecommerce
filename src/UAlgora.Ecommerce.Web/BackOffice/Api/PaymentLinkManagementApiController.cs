using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using Umbraco.Cms.Api.Management.Routing;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// Management API controller for payment link operations in the Umbraco backoffice.
/// </summary>
[VersionedApiBackOfficeRoute($"{EcommerceConstants.ApiRouteBase}/{EcommerceConstants.Routes.PaymentLinks}")]
public class PaymentLinkManagementApiController : EcommerceManagementApiControllerBase
{
    private readonly IPaymentLinkService _paymentLinkService;

    public PaymentLinkManagementApiController(IPaymentLinkService paymentLinkService)
    {
        _paymentLinkService = paymentLinkService;
    }

    /// <summary>
    /// Gets all payment links.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PaymentLink>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var paymentLinks = await _paymentLinkService.GetAllAsync();
        return Ok(paymentLinks);
    }

    /// <summary>
    /// Gets payment links by store.
    /// </summary>
    [HttpGet("by-store/{storeId:guid}")]
    [ProducesResponseType<IReadOnlyList<PaymentLink>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByStore(Guid storeId)
    {
        var paymentLinks = await _paymentLinkService.GetByStoreAsync(storeId);
        return Ok(paymentLinks);
    }

    /// <summary>
    /// Gets active payment links.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType<IReadOnlyList<PaymentLink>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive()
    {
        var paymentLinks = await _paymentLinkService.GetActiveAsync();
        return Ok(paymentLinks);
    }

    /// <summary>
    /// Gets a payment link by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<PaymentLink>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var paymentLink = await _paymentLinkService.GetByIdAsync(id);
        if (paymentLink == null)
        {
            return NotFound();
        }
        return Ok(paymentLink);
    }

    /// <summary>
    /// Gets a payment link by code.
    /// </summary>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType<PaymentLink>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code)
    {
        var paymentLink = await _paymentLinkService.GetByCodeAsync(code);
        if (paymentLink == null)
        {
            return NotFound();
        }
        return Ok(paymentLink);
    }

    /// <summary>
    /// Searches payment links.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType<PagedResult<PaymentLink>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? searchTerm = null,
        [FromQuery] PaymentLinkStatus? status = null,
        [FromQuery] PaymentLinkType? type = null,
        [FromQuery] Guid? storeId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _paymentLinkService.SearchAsync(searchTerm, status, type, storeId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new payment link.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<PaymentLink>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePaymentLinkRequest request)
    {
        var paymentLink = new PaymentLink
        {
            StoreId = request.StoreId,
            Code = request.Code ?? string.Empty,
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            Amount = request.Amount,
            CurrencyCode = request.CurrencyCode ?? "USD",
            MinimumAmount = request.MinimumAmount,
            MaximumAmount = request.MaximumAmount,
            SuggestedAmountsJson = request.SuggestedAmountsJson,
            AllowTip = request.AllowTip,
            TipPercentagesJson = request.TipPercentagesJson,
            ProductId = request.ProductId,
            ProductVariantId = request.ProductVariantId,
            AllowQuantity = request.AllowQuantity,
            MaxQuantity = request.MaxQuantity,
            ValidFrom = request.ValidFrom,
            ExpiresAt = request.ExpiresAt,
            MaxUses = request.MaxUses,
            RequireEmail = request.RequireEmail,
            RequirePhone = request.RequirePhone,
            RequireBillingAddress = request.RequireBillingAddress,
            RequireShippingAddress = request.RequireShippingAddress,
            CustomFieldsJson = request.CustomFieldsJson,
            PrefilledEmail = request.PrefilledEmail,
            PrefilledName = request.PrefilledName,
            SuccessMessage = request.SuccessMessage,
            SuccessRedirectUrl = request.SuccessRedirectUrl,
            CancelUrl = request.CancelUrl,
            AllowedPaymentMethodsJson = request.AllowedPaymentMethodsJson,
            BrandColor = request.BrandColor,
            LogoUrl = request.LogoUrl,
            TermsUrl = request.TermsUrl,
            NotificationEmail = request.NotificationEmail,
            SendCustomerReceipt = request.SendCustomerReceipt,
            ReceiptEmailTemplateId = request.ReceiptEmailTemplateId,
            ReferenceNumber = request.ReferenceNumber,
            Notes = request.Notes,
            TagsJson = request.TagsJson
        };

        try
        {
            var created = await _paymentLinkService.CreateAsync(paymentLink);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Updates a payment link.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<PaymentLink>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePaymentLinkRequest request)
    {
        var paymentLink = await _paymentLinkService.GetByIdAsync(id);
        if (paymentLink == null)
        {
            return NotFound();
        }

        // Update properties
        paymentLink.Code = request.Code ?? paymentLink.Code;
        paymentLink.Name = request.Name ?? paymentLink.Name;
        paymentLink.Description = request.Description;
        paymentLink.Type = request.Type ?? paymentLink.Type;
        paymentLink.Amount = request.Amount ?? paymentLink.Amount;
        paymentLink.CurrencyCode = request.CurrencyCode ?? paymentLink.CurrencyCode;
        paymentLink.MinimumAmount = request.MinimumAmount;
        paymentLink.MaximumAmount = request.MaximumAmount;
        paymentLink.SuggestedAmountsJson = request.SuggestedAmountsJson;
        paymentLink.AllowTip = request.AllowTip ?? paymentLink.AllowTip;
        paymentLink.TipPercentagesJson = request.TipPercentagesJson;
        paymentLink.ProductId = request.ProductId;
        paymentLink.ProductVariantId = request.ProductVariantId;
        paymentLink.AllowQuantity = request.AllowQuantity ?? paymentLink.AllowQuantity;
        paymentLink.MaxQuantity = request.MaxQuantity;
        paymentLink.ValidFrom = request.ValidFrom;
        paymentLink.ExpiresAt = request.ExpiresAt;
        paymentLink.MaxUses = request.MaxUses;
        paymentLink.IsActive = request.IsActive ?? paymentLink.IsActive;
        paymentLink.RequireEmail = request.RequireEmail ?? paymentLink.RequireEmail;
        paymentLink.RequirePhone = request.RequirePhone ?? paymentLink.RequirePhone;
        paymentLink.RequireBillingAddress = request.RequireBillingAddress ?? paymentLink.RequireBillingAddress;
        paymentLink.RequireShippingAddress = request.RequireShippingAddress ?? paymentLink.RequireShippingAddress;
        paymentLink.CustomFieldsJson = request.CustomFieldsJson;
        paymentLink.PrefilledEmail = request.PrefilledEmail;
        paymentLink.PrefilledName = request.PrefilledName;
        paymentLink.SuccessMessage = request.SuccessMessage;
        paymentLink.SuccessRedirectUrl = request.SuccessRedirectUrl;
        paymentLink.CancelUrl = request.CancelUrl;
        paymentLink.AllowedPaymentMethodsJson = request.AllowedPaymentMethodsJson;
        paymentLink.BrandColor = request.BrandColor;
        paymentLink.LogoUrl = request.LogoUrl;
        paymentLink.TermsUrl = request.TermsUrl;
        paymentLink.NotificationEmail = request.NotificationEmail;
        paymentLink.SendCustomerReceipt = request.SendCustomerReceipt ?? paymentLink.SendCustomerReceipt;
        paymentLink.ReceiptEmailTemplateId = request.ReceiptEmailTemplateId;
        paymentLink.ReferenceNumber = request.ReferenceNumber;
        paymentLink.Notes = request.Notes;
        paymentLink.TagsJson = request.TagsJson;

        try
        {
            var updated = await _paymentLinkService.UpdateAsync(paymentLink);
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Pauses a payment link.
    /// </summary>
    [HttpPost("{id:guid}/pause")]
    [ProducesResponseType<PaymentLink>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Pause(Guid id)
    {
        try
        {
            var paymentLink = await _paymentLinkService.PauseAsync(id);
            return Ok(paymentLink);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Activates a payment link.
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType<PaymentLink>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id)
    {
        try
        {
            var paymentLink = await _paymentLinkService.ActivateAsync(id);
            return Ok(paymentLink);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Archives a payment link.
    /// </summary>
    [HttpPost("{id:guid}/archive")]
    [ProducesResponseType<PaymentLink>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Archive(Guid id)
    {
        try
        {
            var paymentLink = await _paymentLinkService.ArchiveAsync(id);
            return Ok(paymentLink);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Duplicates a payment link.
    /// </summary>
    [HttpPost("{id:guid}/duplicate")]
    [ProducesResponseType<PaymentLink>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Duplicate(Guid id)
    {
        try
        {
            var duplicate = await _paymentLinkService.DuplicateAsync(id);
            return CreatedAtAction(nameof(GetById), new { id = duplicate.Id }, duplicate);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Deletes a payment link (soft delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _paymentLinkService.DeleteAsync(id);
        return Ok(new { success = true });
    }

    /// <summary>
    /// Gets payments for a payment link.
    /// </summary>
    [HttpGet("{id:guid}/payments")]
    [ProducesResponseType<IReadOnlyList<PaymentLinkPayment>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayments(Guid id)
    {
        var payments = await _paymentLinkService.GetPaymentsAsync(id);
        return Ok(payments);
    }

    /// <summary>
    /// Gets statistics for a payment link.
    /// </summary>
    [HttpGet("{id:guid}/statistics")]
    [ProducesResponseType<PaymentLinkStatistics>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics(Guid id)
    {
        var statistics = await _paymentLinkService.GetStatisticsAsync(id);
        return Ok(statistics);
    }

    /// <summary>
    /// Generates a unique payment link code.
    /// </summary>
    [HttpGet("generate-code")]
    [ProducesResponseType<GenerateCodeResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateCode([FromQuery] string? prefix = null)
    {
        var code = await _paymentLinkService.GenerateCodeAsync(prefix);
        return Ok(new GenerateCodeResult { Code = code });
    }

    /// <summary>
    /// Validates a payment link.
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType<PaymentLinkValidationResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Validate([FromBody] ValidatePaymentLinkRequest request)
    {
        var result = await _paymentLinkService.ValidateAsync(request.Code);
        return Ok(result);
    }

    /// <summary>
    /// Gets expiring payment links.
    /// </summary>
    [HttpGet("expiring-soon")]
    [ProducesResponseType<IReadOnlyList<PaymentLink>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpiringSoon([FromQuery] int days = 30)
    {
        var paymentLinks = await _paymentLinkService.GetExpiringSoonAsync(days);
        return Ok(paymentLinks);
    }
}

#region Request/Response Models

public class CreatePaymentLinkRequest
{
    public Guid? StoreId { get; set; }
    public string? Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public PaymentLinkType Type { get; set; } = PaymentLinkType.FixedAmount;
    public decimal? Amount { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? MinimumAmount { get; set; }
    public decimal? MaximumAmount { get; set; }
    public string? SuggestedAmountsJson { get; set; }
    public bool AllowTip { get; set; }
    public string? TipPercentagesJson { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public bool AllowQuantity { get; set; }
    public int? MaxQuantity { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? MaxUses { get; set; }
    public bool RequireEmail { get; set; } = true;
    public bool RequirePhone { get; set; }
    public bool RequireBillingAddress { get; set; }
    public bool RequireShippingAddress { get; set; }
    public string? CustomFieldsJson { get; set; }
    public string? PrefilledEmail { get; set; }
    public string? PrefilledName { get; set; }
    public string? SuccessMessage { get; set; }
    public string? SuccessRedirectUrl { get; set; }
    public string? CancelUrl { get; set; }
    public string? AllowedPaymentMethodsJson { get; set; }
    public string? BrandColor { get; set; }
    public string? LogoUrl { get; set; }
    public string? TermsUrl { get; set; }
    public string? NotificationEmail { get; set; }
    public bool SendCustomerReceipt { get; set; } = true;
    public Guid? ReceiptEmailTemplateId { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public string? TagsJson { get; set; }
}

public class UpdatePaymentLinkRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public PaymentLinkType? Type { get; set; }
    public decimal? Amount { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? MinimumAmount { get; set; }
    public decimal? MaximumAmount { get; set; }
    public string? SuggestedAmountsJson { get; set; }
    public bool? AllowTip { get; set; }
    public string? TipPercentagesJson { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public bool? AllowQuantity { get; set; }
    public int? MaxQuantity { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? MaxUses { get; set; }
    public bool? IsActive { get; set; }
    public bool? RequireEmail { get; set; }
    public bool? RequirePhone { get; set; }
    public bool? RequireBillingAddress { get; set; }
    public bool? RequireShippingAddress { get; set; }
    public string? CustomFieldsJson { get; set; }
    public string? PrefilledEmail { get; set; }
    public string? PrefilledName { get; set; }
    public string? SuccessMessage { get; set; }
    public string? SuccessRedirectUrl { get; set; }
    public string? CancelUrl { get; set; }
    public string? AllowedPaymentMethodsJson { get; set; }
    public string? BrandColor { get; set; }
    public string? LogoUrl { get; set; }
    public string? TermsUrl { get; set; }
    public string? NotificationEmail { get; set; }
    public bool? SendCustomerReceipt { get; set; }
    public Guid? ReceiptEmailTemplateId { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public string? TagsJson { get; set; }
}

public class ValidatePaymentLinkRequest
{
    public required string Code { get; set; }
}

public class GenerateCodeResult
{
    public required string Code { get; set; }
}

#endregion
