using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using Umbraco.Cms.Api.Management.Routing;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// Management API controller for gift card operations in the Umbraco backoffice.
/// </summary>
[VersionedApiBackOfficeRoute($"{EcommerceConstants.ApiRouteBase}/{EcommerceConstants.Routes.GiftCards}")]
public class GiftCardManagementApiController : EcommerceManagementApiControllerBase
{
    private readonly IGiftCardService _giftCardService;

    public GiftCardManagementApiController(IGiftCardService giftCardService)
    {
        _giftCardService = giftCardService;
    }

    /// <summary>
    /// Gets gift cards by store.
    /// </summary>
    [HttpGet("by-store/{storeId:guid}")]
    [ProducesResponseType<IReadOnlyList<GiftCard>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByStore(Guid storeId)
    {
        var giftCards = await _giftCardService.GetByStoreAsync(storeId);
        return Ok(giftCards);
    }

    /// <summary>
    /// Gets a gift card by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<GiftCard>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var giftCard = await _giftCardService.GetByIdAsync(id);
        if (giftCard == null)
        {
            return NotFound();
        }
        return Ok(giftCard);
    }

    /// <summary>
    /// Gets a gift card by code.
    /// </summary>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType<GiftCard>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code)
    {
        var giftCard = await _giftCardService.GetByCodeAsync(code);
        if (giftCard == null)
        {
            return NotFound();
        }
        return Ok(giftCard);
    }

    /// <summary>
    /// Creates a new gift card.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<GiftCard>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateGiftCardRequest request)
    {
        var giftCard = new GiftCard
        {
            InitialValue = request.InitialBalance,
            Balance = request.InitialBalance,
            CurrencyCode = request.CurrencyCode ?? "USD",
            ExpiresAt = request.ExpiresAt,
            RecipientEmail = request.RecipientEmail,
            RecipientName = request.RecipientName,
            Message = request.PersonalMessage,
            Status = GiftCardStatus.Active
        };

        var created = await _giftCardService.CreateAsync(giftCard);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Checks the balance of a gift card.
    /// </summary>
    [HttpGet("by-code/{code}/balance")]
    [ProducesResponseType<GiftCardBalanceResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckBalance(string code)
    {
        var giftCard = await _giftCardService.GetByCodeAsync(code);
        if (giftCard == null)
        {
            return NotFound(new { error = "Gift card not found" });
        }
        return Ok(new GiftCardBalanceResult { Code = code, Balance = giftCard.Balance });
    }

    /// <summary>
    /// Redeems a gift card.
    /// </summary>
    [HttpPost("redeem")]
    [ProducesResponseType<GiftCardRedemptionResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Redeem([FromBody] RedeemGiftCardRequest request)
    {
        if (!request.OrderId.HasValue)
        {
            return BadRequest(new { error = "OrderId is required" });
        }

        var result = await _giftCardService.RedeemAsync(
            request.Code,
            request.Amount,
            request.OrderId.Value,
            request.CustomerId);
        return Ok(result);
    }

    /// <summary>
    /// Validates a gift card for use in checkout.
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType<GiftCardValidationResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Validate([FromBody] ValidateGiftCardRequest request)
    {
        var result = await _giftCardService.ValidateAsync(request.Code, request.Amount);
        return Ok(result);
    }

    /// <summary>
    /// Gets gift card transactions.
    /// </summary>
    [HttpGet("{id:guid}/transactions")]
    [ProducesResponseType<IReadOnlyList<GiftCardTransaction>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactions(Guid id)
    {
        var transactions = await _giftCardService.GetTransactionsAsync(id);
        return Ok(transactions);
    }

    /// <summary>
    /// Adjusts a gift card balance.
    /// </summary>
    [HttpPost("{id:guid}/adjust")]
    [ProducesResponseType<GiftCard>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AdjustBalance(Guid id, [FromBody] AdjustBalanceRequest request)
    {
        var result = await _giftCardService.AdjustBalanceAsync(id, request.Amount, request.PerformedBy ?? "System", request.Reason);
        if (!result)
        {
            return BadRequest(new { error = "Failed to adjust balance" });
        }
        var giftCard = await _giftCardService.GetByIdAsync(id);
        return Ok(giftCard);
    }

    /// <summary>
    /// Deletes a gift card (soft delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _giftCardService.DeleteAsync(id);
        return Ok(new { success = true });
    }

    /// <summary>
    /// Gets expiring gift cards.
    /// </summary>
    [HttpGet("expiring-soon")]
    [ProducesResponseType<IReadOnlyList<GiftCard>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpiringSoon([FromQuery] int days = 30)
    {
        var giftCards = await _giftCardService.GetExpiringSoonAsync(days);
        return Ok(giftCards);
    }
}

#region Request/Response Models

public class CreateGiftCardRequest
{
    public decimal InitialBalance { get; set; }
    public string? CurrencyCode { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? RecipientEmail { get; set; }
    public string? RecipientName { get; set; }
    public string? PersonalMessage { get; set; }
}

public class RedeemGiftCardRequest
{
    public required string Code { get; set; }
    public decimal Amount { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? CustomerId { get; set; }
}

public class ValidateGiftCardRequest
{
    public required string Code { get; set; }
    public decimal Amount { get; set; }
}

public class AdjustBalanceRequest
{
    public decimal Amount { get; set; }
    public string? PerformedBy { get; set; }
    public string? Reason { get; set; }
}

public class GiftCardBalanceResult
{
    public required string Code { get; set; }
    public decimal Balance { get; set; }
}

#endregion
