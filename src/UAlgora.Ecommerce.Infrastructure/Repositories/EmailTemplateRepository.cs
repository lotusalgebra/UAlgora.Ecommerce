using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Email Template operations.
/// </summary>
public class EmailTemplateRepository : SoftDeleteRepository<EmailTemplate>, IEmailTemplateRepository
{
    public EmailTemplateRepository(EcommerceDbContext context) : base(context)
    {
    }

    public async Task<EmailTemplate?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(e => e.Code == code, ct);
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetByStoreAsync(Guid? storeId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(e => e.StoreId == storeId)
            .OrderBy(e => e.EventType)
            .ThenBy(e => e.Language)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetByEventTypeAsync(EmailTemplateEventType eventType, CancellationToken ct = default)
    {
        return await DbSet
            .Where(e => e.EventType == eventType)
            .OrderBy(e => e.Language)
            .ToListAsync(ct);
    }

    public async Task<EmailTemplate?> GetActiveTemplateAsync(EmailTemplateEventType eventType, string language, Guid? storeId = null, CancellationToken ct = default)
    {
        // First try to find a store-specific template
        if (storeId.HasValue)
        {
            var storeTemplate = await DbSet
                .Where(e => e.EventType == eventType)
                .Where(e => e.Language == language)
                .Where(e => e.StoreId == storeId.Value)
                .Where(e => e.IsActive)
                .OrderByDescending(e => e.Priority)
                .FirstOrDefaultAsync(ct);

            if (storeTemplate != null)
            {
                return storeTemplate;
            }
        }

        // Fall back to default template (no store)
        return await DbSet
            .Where(e => e.EventType == eventType)
            .Where(e => e.Language == language)
            .Where(e => e.StoreId == null)
            .Where(e => e.IsActive)
            .Where(e => e.IsDefault)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetActiveAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Where(e => e.IsActive)
            .OrderBy(e => e.EventType)
            .ThenBy(e => e.Language)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetByLanguageAsync(string language, CancellationToken ct = default)
    {
        return await DbSet
            .Where(e => e.Language == language)
            .OrderBy(e => e.EventType)
            .ToListAsync(ct);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = DbSet.Where(e => e.Code == code);
        if (excludeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeId.Value);
        }
        return await query.AnyAsync(ct);
    }

    public async Task IncrementSendCountAsync(Guid templateId, CancellationToken ct = default)
    {
        var template = await GetByIdAsync(templateId, ct);
        if (template != null)
        {
            template.SendCount++;
            template.LastSentAt = DateTime.UtcNow;
            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetDefaultTemplatesAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Where(e => e.StoreId == null)
            .Where(e => e.IsDefault)
            .OrderBy(e => e.EventType)
            .ThenBy(e => e.Language)
            .ToListAsync(ct);
    }
}
