using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Services;

/// <summary>
/// Service interface for Gift Card operations.
/// </summary>
public interface IGiftCardService
{
    /// <summary>
    /// Gets a gift card by ID.
    /// </summary>
    Task<GiftCard?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a gift card by code.
    /// </summary>
    Task<GiftCard?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Gets gift cards by store.
    /// </summary>
    Task<IReadOnlyList<GiftCard>> GetByStoreAsync(Guid storeId, CancellationToken ct = default);

    /// <summary>
    /// Gets gift cards by customer.
    /// </summary>
    Task<IReadOnlyList<GiftCard>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Creates a new gift card.
    /// </summary>
    Task<GiftCard> CreateAsync(GiftCard giftCard, CancellationToken ct = default);

    /// <summary>
    /// Generates a unique gift card code.
    /// </summary>
    Task<string> GenerateCodeAsync(string? prefix = null, CancellationToken ct = default);

    /// <summary>
    /// Updates a gift card.
    /// </summary>
    Task<GiftCard> UpdateAsync(GiftCard giftCard, CancellationToken ct = default);

    /// <summary>
    /// Validates a gift card for use.
    /// </summary>
    Task<GiftCardValidationResult> ValidateAsync(string code, decimal orderAmount, CancellationToken ct = default);

    /// <summary>
    /// Redeems a gift card at checkout.
    /// </summary>
    Task<GiftCardRedemptionResult> RedeemAsync(string code, decimal amount, Guid orderId, Guid? customerId, CancellationToken ct = default);

    /// <summary>
    /// Refunds to a gift card.
    /// </summary>
    Task<bool> RefundAsync(Guid giftCardId, decimal amount, Guid orderId, string? performedBy, CancellationToken ct = default);

    /// <summary>
    /// Adjusts gift card balance.
    /// </summary>
    Task<bool> AdjustBalanceAsync(Guid giftCardId, decimal amount, string performedBy, string? notes, CancellationToken ct = default);

    /// <summary>
    /// Gets gift card transactions.
    /// </summary>
    Task<IReadOnlyList<GiftCardTransaction>> GetTransactionsAsync(Guid giftCardId, CancellationToken ct = default);

    /// <summary>
    /// Deactivates expired gift cards.
    /// </summary>
    Task<int> DeactivateExpiredAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets gift cards expiring soon.
    /// </summary>
    Task<IReadOnlyList<GiftCard>> GetExpiringSoonAsync(int days = 30, CancellationToken ct = default);

    /// <summary>
    /// Deletes a gift card (soft delete).
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

/// <summary>
/// Gift card validation result.
/// </summary>
public class GiftCardValidationResult
{
    public bool IsValid { get; set; }
    public GiftCard? GiftCard { get; set; }
    public decimal AvailableBalance { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }

    public static GiftCardValidationResult Success(GiftCard giftCard) => new()
    {
        IsValid = true,
        GiftCard = giftCard,
        AvailableBalance = giftCard.Balance
    };

    public static GiftCardValidationResult Failure(string errorCode, string message) => new()
    {
        IsValid = false,
        ErrorCode = errorCode,
        ErrorMessage = message
    };
}

/// <summary>
/// Gift card redemption result.
/// </summary>
public class GiftCardRedemptionResult
{
    public bool Success { get; set; }
    public decimal AmountRedeemed { get; set; }
    public decimal RemainingBalance { get; set; }
    public string? ErrorMessage { get; set; }
    public GiftCardTransaction? Transaction { get; set; }
}
