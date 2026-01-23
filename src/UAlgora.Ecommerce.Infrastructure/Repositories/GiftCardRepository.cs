using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Gift Card operations.
/// </summary>
public class GiftCardRepository : SoftDeleteRepository<GiftCard>, IGiftCardRepository
{
    public GiftCardRepository(EcommerceDbContext context) : base(context)
    {
    }

    public async Task<GiftCard?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(g => g.Code == code, ct);
    }

    public async Task<IReadOnlyList<GiftCard>> GetByStoreAsync(Guid storeId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(g => g.StoreId == storeId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<GiftCard>> GetByStatusAsync(GiftCardStatus status, CancellationToken ct = default)
    {
        return await DbSet
            .Where(g => g.Status == status)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<GiftCard>> GetActiveWithBalanceAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Where(g => g.Status == GiftCardStatus.Active)
            .Where(g => g.IsActive)
            .Where(g => g.Balance > 0)
            .Where(g => !g.ExpiresAt.HasValue || g.ExpiresAt.Value > now)
            .Where(g => !g.ValidFrom.HasValue || g.ValidFrom.Value <= now)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<GiftCard>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(g => g.IssuedToCustomerId == customerId || g.PurchasedByCustomerId == customerId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<GiftCard>> GetExpiringSoonAsync(int daysUntilExpiry, CancellationToken ct = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysUntilExpiry);
        return await DbSet
            .Where(g => g.Status == GiftCardStatus.Active)
            .Where(g => g.Balance > 0)
            .Where(g => g.ExpiresAt.HasValue && g.ExpiresAt.Value <= cutoffDate && g.ExpiresAt.Value > DateTime.UtcNow)
            .OrderBy(g => g.ExpiresAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<GiftCard>> GetExpiredActiveAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Where(g => g.Status == GiftCardStatus.Active)
            .Where(g => g.ExpiresAt.HasValue && g.ExpiresAt.Value <= now)
            .ToListAsync(ct);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = DbSet.Where(g => g.Code == code);
        if (excludeId.HasValue)
        {
            query = query.Where(g => g.Id != excludeId.Value);
        }
        return await query.AnyAsync(ct);
    }

    public async Task<IReadOnlyList<GiftCardTransaction>> GetTransactionsAsync(Guid giftCardId, CancellationToken ct = default)
    {
        return await Context.GiftCardTransactions
            .Where(t => t.GiftCardId == giftCardId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<GiftCardTransaction> AddTransactionAsync(GiftCardTransaction transaction, CancellationToken ct = default)
    {
        await Context.GiftCardTransactions.AddAsync(transaction, ct);
        await Context.SaveChangesAsync(ct);
        return transaction;
    }

    public async Task<bool> DeductBalanceAsync(Guid giftCardId, decimal amount, Guid? orderId, string? performedBy, CancellationToken ct = default)
    {
        var giftCard = await GetByIdAsync(giftCardId, ct);
        if (giftCard == null || giftCard.Balance < amount)
        {
            return false;
        }

        var balanceBefore = giftCard.Balance;
        giftCard.Balance -= amount;
        giftCard.UsageCount++;
        giftCard.LastUsedAt = DateTime.UtcNow;

        var transaction = new GiftCardTransaction
        {
            GiftCardId = giftCardId,
            OrderId = orderId,
            Type = GiftCardTransactionType.Redeem,
            Amount = amount,
            BalanceBefore = balanceBefore,
            BalanceAfter = giftCard.Balance,
            CurrencyCode = giftCard.CurrencyCode,
            PerformedBy = performedBy
        };

        await Context.GiftCardTransactions.AddAsync(transaction, ct);
        await Context.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> AddBalanceAsync(Guid giftCardId, decimal amount, string? performedBy, string? notes, CancellationToken ct = default)
    {
        var giftCard = await GetByIdAsync(giftCardId, ct);
        if (giftCard == null)
        {
            return false;
        }

        var balanceBefore = giftCard.Balance;
        giftCard.Balance += amount;

        var transaction = new GiftCardTransaction
        {
            GiftCardId = giftCardId,
            Type = GiftCardTransactionType.Adjustment,
            Amount = amount,
            BalanceBefore = balanceBefore,
            BalanceAfter = giftCard.Balance,
            CurrencyCode = giftCard.CurrencyCode,
            PerformedBy = performedBy,
            Notes = notes
        };

        await Context.GiftCardTransactions.AddAsync(transaction, ct);
        await Context.SaveChangesAsync(ct);

        return true;
    }
}
