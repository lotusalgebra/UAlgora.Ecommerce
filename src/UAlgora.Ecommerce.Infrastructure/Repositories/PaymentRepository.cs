using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for payment operations.
/// </summary>
public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(EcommerceDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Payment>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Payment?> GetByTransactionIdAsync(string transactionId, CancellationToken ct = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId, ct);
    }

    public async Task<Payment?> GetByPaymentIntentIdAsync(string paymentIntentId, CancellationToken ct = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(p => p.PaymentIntentId == paymentIntentId, ct);
    }

    public async Task<IReadOnlyList<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken ct = default)
    {
        return await DbSet
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Payment>> GetByProviderAsync(string provider, CancellationToken ct = default)
    {
        return await DbSet
            .Where(p => p.Provider == provider)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Payment>> GetRefundsAsync(Guid paymentId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(p => p.ParentPaymentId == paymentId)
            .Where(p => p.IsRefund)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Payment>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default)
    {
        return await DbSet
            .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<decimal> GetTotalCapturedAsync(Guid orderId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(p => p.OrderId == orderId)
            .Where(p => p.Status == PaymentStatus.Captured || p.Status == PaymentStatus.PartiallyRefunded)
            .Where(p => !p.IsRefund)
            .SumAsync(p => p.Amount, ct);
    }

    public async Task<decimal> GetTotalRefundedAsync(Guid orderId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(p => p.OrderId == orderId)
            .Where(p => p.IsRefund)
            .Where(p => p.Status == PaymentStatus.Refunded)
            .SumAsync(p => p.Amount, ct);
    }
}

/// <summary>
/// Repository implementation for stored payment method operations.
/// </summary>
public class StoredPaymentMethodRepository : Repository<StoredPaymentMethod>, IStoredPaymentMethodRepository
{
    public StoredPaymentMethodRepository(EcommerceDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<StoredPaymentMethod>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken ct = default)
    {
        return await DbSet
            .Where(pm => pm.CustomerId == customerId)
            .OrderByDescending(pm => pm.IsDefault)
            .ThenByDescending(pm => pm.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<StoredPaymentMethod?> GetDefaultAsync(Guid customerId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(pm => pm.CustomerId == customerId && pm.IsDefault)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<StoredPaymentMethod?> GetByProviderMethodIdAsync(
        string providerMethodId,
        CancellationToken ct = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(pm => pm.ProviderMethodId == providerMethodId, ct);
    }

    public async Task SetDefaultAsync(Guid customerId, Guid paymentMethodId, CancellationToken ct = default)
    {
        // Clear existing defaults
        var existingDefaults = await DbSet
            .Where(pm => pm.CustomerId == customerId && pm.IsDefault)
            .ToListAsync(ct);

        foreach (var pm in existingDefaults)
        {
            pm.IsDefault = false;
        }

        // Set new default
        var paymentMethod = await DbSet
            .FirstOrDefaultAsync(pm => pm.Id == paymentMethodId && pm.CustomerId == customerId, ct);

        if (paymentMethod != null)
        {
            paymentMethod.IsDefault = true;
        }

        await Context.SaveChangesAsync(ct);
    }

    public async Task DeleteByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        var paymentMethods = await DbSet
            .Where(pm => pm.CustomerId == customerId)
            .ToListAsync(ct);

        DbSet.RemoveRange(paymentMethods);
        await Context.SaveChangesAsync(ct);
    }
}
