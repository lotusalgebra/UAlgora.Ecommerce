using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Webhook operations.
/// </summary>
public class WebhookRepository : SoftDeleteRepository<Webhook>, IWebhookRepository
{
    public WebhookRepository(EcommerceDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Webhook>> GetByStoreAsync(Guid? storeId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(w => w.StoreId == storeId)
            .OrderBy(w => w.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Webhook>> GetActiveAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Where(w => w.IsActive && !w.IsAutoDisabled)
            .OrderBy(w => w.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Webhook>> GetByEventAsync(string eventType, Guid? storeId = null, CancellationToken ct = default)
    {
        var query = DbSet
            .Where(w => w.IsActive && !w.IsAutoDisabled)
            .Where(w => w.SubscribeToAll || w.EventsJson.Contains(eventType));

        if (storeId.HasValue)
        {
            query = query.Where(w => w.StoreId == storeId.Value || w.StoreId == null);
        }

        return await query.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Webhook>> GetAutoDisabledAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Where(w => w.IsAutoDisabled)
            .OrderByDescending(w => w.AutoDisabledAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<WebhookDelivery>> GetDeliveriesAsync(Guid webhookId, int skip = 0, int take = 50, CancellationToken ct = default)
    {
        return await Context.WebhookDeliveries
            .Where(d => d.WebhookId == webhookId)
            .OrderByDescending(d => d.StartedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<WebhookDelivery>> GetFailedDeliveriesAsync(Guid webhookId, int take = 10, CancellationToken ct = default)
    {
        return await Context.WebhookDeliveries
            .Where(d => d.WebhookId == webhookId && !d.IsSuccess)
            .OrderByDescending(d => d.StartedAt)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<WebhookDelivery> AddDeliveryAsync(WebhookDelivery delivery, CancellationToken ct = default)
    {
        await Context.WebhookDeliveries.AddAsync(delivery, ct);
        await Context.SaveChangesAsync(ct);
        return delivery;
    }

    public async Task UpdateStatisticsAsync(Guid webhookId, bool isSuccess, int? statusCode, long durationMs, string? error = null, CancellationToken ct = default)
    {
        var webhook = await GetByIdAsync(webhookId, ct);
        if (webhook == null) return;

        webhook.TotalDeliveries++;
        webhook.LastTriggeredAt = DateTime.UtcNow;
        webhook.LastStatusCode = statusCode;

        if (isSuccess)
        {
            webhook.SuccessfulDeliveries++;
            webhook.LastSuccessAt = DateTime.UtcNow;
            webhook.ConsecutiveFailures = 0;

            // Update average response time
            if (webhook.AverageResponseTimeMs.HasValue)
            {
                webhook.AverageResponseTimeMs = (webhook.AverageResponseTimeMs.Value * (webhook.SuccessfulDeliveries - 1) + durationMs) / webhook.SuccessfulDeliveries;
            }
            else
            {
                webhook.AverageResponseTimeMs = durationMs;
            }
        }
        else
        {
            webhook.FailedDeliveries++;
            webhook.LastFailureAt = DateTime.UtcNow;
            webhook.LastError = error;
            webhook.ConsecutiveFailures++;
        }

        await Context.SaveChangesAsync(ct);
    }

    public async Task<bool> AutoDisableAsync(Guid webhookId, string reason, CancellationToken ct = default)
    {
        var webhook = await GetByIdAsync(webhookId, ct);
        if (webhook == null) return false;

        webhook.IsAutoDisabled = true;
        webhook.AutoDisabledAt = DateTime.UtcNow;
        webhook.AutoDisableReason = reason;

        await Context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ReEnableAsync(Guid webhookId, CancellationToken ct = default)
    {
        var webhook = await GetByIdAsync(webhookId, ct);
        if (webhook == null) return false;

        webhook.IsAutoDisabled = false;
        webhook.AutoDisabledAt = null;
        webhook.AutoDisableReason = null;
        webhook.ConsecutiveFailures = 0;

        await Context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<WebhookDelivery>> GetPendingRetriesAsync(int maxAttempts = 5, CancellationToken ct = default)
    {
        return await Context.WebhookDeliveries
            .Include(d => d.Webhook)
            .Where(d => !d.IsSuccess)
            .Where(d => d.AttemptNumber < maxAttempts)
            .Where(d => d.Webhook.RetryEnabled)
            .Where(d => d.Webhook.IsActive && !d.Webhook.IsAutoDisabled)
            .Where(d => d.ScheduledAt.HasValue && d.ScheduledAt.Value <= DateTime.UtcNow)
            .OrderBy(d => d.ScheduledAt)
            .ToListAsync(ct);
    }

    public async Task<int> DeleteOldDeliveriesAsync(DateTime cutoffDate, CancellationToken ct = default)
    {
        var deliveriesToDelete = await Context.WebhookDeliveries
            .Where(d => d.StartedAt < cutoffDate)
            .ToListAsync(ct);

        if (deliveriesToDelete.Count > 0)
        {
            Context.WebhookDeliveries.RemoveRange(deliveriesToDelete);
            await Context.SaveChangesAsync(ct);
        }

        return deliveriesToDelete.Count;
    }
}
