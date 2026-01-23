using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Return operations.
/// </summary>
public class ReturnRepository : SoftDeleteRepository<Return>, IReturnRepository
{
    public ReturnRepository(EcommerceDbContext context) : base(context)
    {
    }

    public async Task<Return?> GetByReturnNumberAsync(string returnNumber, CancellationToken ct = default)
    {
        return await DbSet
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.ReturnNumber == returnNumber, ct);
    }

    public async Task<IReadOnlyList<Return>> GetByStoreAsync(Guid storeId, CancellationToken ct = default)
    {
        return await DbSet
            .Include(r => r.Items)
            .Where(r => r.StoreId == storeId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Return>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default)
    {
        return await DbSet
            .Include(r => r.Items)
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Return>> GetByOrderAsync(Guid orderId, CancellationToken ct = default)
    {
        return await DbSet
            .Include(r => r.Items)
            .Where(r => r.OrderId == orderId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Return>> GetByStatusAsync(ReturnStatus status, CancellationToken ct = default)
    {
        return await DbSet
            .Include(r => r.Items)
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Return>> GetPendingAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Include(r => r.Items)
            .Where(r => r.Status == ReturnStatus.Requested)
            .OrderBy(r => r.RequestedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Return>> GetForRestockingAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Include(r => r.Items)
            .Where(r => r.Status == ReturnStatus.ItemsReceived || r.Status == ReturnStatus.Completed)
            .Where(r => r.Items.Any(i => i.ShouldRestock && i.RestockedQuantity < i.Quantity))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ReturnItem>> GetItemsAsync(Guid returnId, CancellationToken ct = default)
    {
        return await Context.ReturnItems
            .Where(i => i.ReturnId == returnId)
            .OrderBy(i => i.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> UpdateStatusAsync(Guid returnId, ReturnStatus newStatus, string? processedBy = null, string? notes = null, CancellationToken ct = default)
    {
        var returnRequest = await GetByIdAsync(returnId, ct);
        if (returnRequest == null)
        {
            return false;
        }

        returnRequest.Status = newStatus;
        returnRequest.ProcessedBy = processedBy ?? returnRequest.ProcessedBy;

        if (!string.IsNullOrEmpty(notes))
        {
            returnRequest.AdminNotes = string.IsNullOrEmpty(returnRequest.AdminNotes)
                ? notes
                : $"{returnRequest.AdminNotes}\n\n{DateTime.UtcNow:u}: {notes}";
        }

        switch (newStatus)
        {
            case ReturnStatus.Approved:
                returnRequest.ApprovedAt = DateTime.UtcNow;
                break;
            case ReturnStatus.Rejected:
                returnRequest.RejectedAt = DateTime.UtcNow;
                break;
            case ReturnStatus.ItemsReceived:
                returnRequest.ReceivedAt = DateTime.UtcNow;
                break;
            case ReturnStatus.Refunded:
                returnRequest.RefundedAt = DateTime.UtcNow;
                break;
            case ReturnStatus.Completed:
                returnRequest.CompletedAt = DateTime.UtcNow;
                break;
        }

        await Context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<Return>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await DbSet
            .Include(r => r.Items)
            .Where(r => r.RequestedAt >= startDate && r.RequestedAt <= endDate)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync(ct);
    }

    public async Task<decimal> GetTotalRefundAmountAsync(Guid? storeId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var query = DbSet
            .Where(r => r.Status == ReturnStatus.Refunded || r.Status == ReturnStatus.Completed)
            .Where(r => r.RefundedAt.HasValue && r.RefundedAt.Value >= startDate && r.RefundedAt.Value <= endDate);

        if (storeId.HasValue)
        {
            query = query.Where(r => r.StoreId == storeId.Value);
        }

        return await query.SumAsync(r => r.ApprovedRefundAmount ?? r.RefundAmount, ct);
    }
}
