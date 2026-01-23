using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for Gift Card operations.
/// </summary>
public interface IGiftCardRepository : ISoftDeleteRepository<GiftCard>
{
    /// <summary>
    /// Get a gift card by its code.
    /// </summary>
    Task<GiftCard?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Get gift cards by store.
    /// </summary>
    Task<IReadOnlyList<GiftCard>> GetByStoreAsync(Guid storeId, CancellationToken ct = default);

    /// <summary>
    /// Get gift cards by status.
    /// </summary>
    Task<IReadOnlyList<GiftCard>> GetByStatusAsync(GiftCardStatus status, CancellationToken ct = default);

    /// <summary>
    /// Get active gift cards with available balance.
    /// </summary>
    Task<IReadOnlyList<GiftCard>> GetActiveWithBalanceAsync(CancellationToken ct = default);

    /// <summary>
    /// Get gift cards by customer.
    /// </summary>
    Task<IReadOnlyList<GiftCard>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Get gift cards expiring soon.
    /// </summary>
    Task<IReadOnlyList<GiftCard>> GetExpiringSoonAsync(int daysUntilExpiry, CancellationToken ct = default);

    /// <summary>
    /// Get expired gift cards that are still active.
    /// </summary>
    Task<IReadOnlyList<GiftCard>> GetExpiredActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Check if a gift card code already exists.
    /// </summary>
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default);

    /// <summary>
    /// Get transactions for a gift card.
    /// </summary>
    Task<IReadOnlyList<GiftCardTransaction>> GetTransactionsAsync(Guid giftCardId, CancellationToken ct = default);

    /// <summary>
    /// Add a transaction for a gift card.
    /// </summary>
    Task<GiftCardTransaction> AddTransactionAsync(GiftCardTransaction transaction, CancellationToken ct = default);

    /// <summary>
    /// Deduct balance from a gift card.
    /// </summary>
    Task<bool> DeductBalanceAsync(Guid giftCardId, decimal amount, Guid? orderId, string? performedBy, CancellationToken ct = default);

    /// <summary>
    /// Add balance to a gift card.
    /// </summary>
    Task<bool> AddBalanceAsync(Guid giftCardId, decimal amount, string? performedBy, string? notes, CancellationToken ct = default);
}
