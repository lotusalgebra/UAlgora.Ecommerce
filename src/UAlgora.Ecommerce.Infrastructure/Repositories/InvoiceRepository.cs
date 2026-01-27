using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for invoice operations.
/// </summary>
public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(EcommerceDbContext context) : base(context)
    {
    }

    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken ct = default)
    {
        return await DbSet
            .Include(i => i.Order)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber, ct);
    }

    public async Task<IReadOnlyList<Invoice>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(i => i.OrderId == orderId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Invoice?> GetLatestByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(i => i.OrderId == orderId)
            .OrderByDescending(i => i.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Invoice>> GetByStatusAsync(InvoiceStatus status, CancellationToken ct = default)
    {
        return await DbSet
            .Where(i => i.Status == status)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<string> GenerateInvoiceNumberAsync(string prefix = "INV-", bool includeYear = true, int padding = 6, CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var fullPrefix = includeYear ? $"{prefix}{year}-" : prefix;

        var lastInvoice = await DbSet
            .Where(i => i.InvoiceNumber.StartsWith(fullPrefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync(ct);

        int sequence = 1;
        if (lastInvoice != null)
        {
            var lastNumber = lastInvoice.InvoiceNumber.Replace(fullPrefix, "");
            if (int.TryParse(lastNumber, out var parsed))
            {
                sequence = parsed + 1;
            }
        }

        return $"{fullPrefix}{sequence.ToString().PadLeft(padding, '0')}";
    }

    public async Task<IReadOnlyList<InvoiceTemplate>> GetTemplatesAsync(CancellationToken ct = default)
    {
        return await Context.Set<InvoiceTemplate>()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<InvoiceTemplate?> GetDefaultTemplateAsync(InvoiceTemplateType type = InvoiceTemplateType.Invoice, CancellationToken ct = default)
    {
        return await Context.Set<InvoiceTemplate>()
            .Where(t => t.IsActive && t.IsDefault && t.TemplateType == type)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<InvoiceTemplate?> GetTemplateByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await Context.Set<InvoiceTemplate>().FindAsync([id], ct);
    }

    public async Task<InvoiceTemplate?> GetTemplateByCodeAsync(string code, CancellationToken ct = default)
    {
        return await Context.Set<InvoiceTemplate>()
            .FirstOrDefaultAsync(t => t.Code == code, ct);
    }

    public async Task<InvoiceTemplate> AddTemplateAsync(InvoiceTemplate template, CancellationToken ct = default)
    {
        await Context.Set<InvoiceTemplate>().AddAsync(template, ct);
        await Context.SaveChangesAsync(ct);
        return template;
    }

    public async Task<InvoiceTemplate> UpdateTemplateAsync(InvoiceTemplate template, CancellationToken ct = default)
    {
        template.UpdatedAt = DateTime.UtcNow;
        Context.Set<InvoiceTemplate>().Update(template);
        await Context.SaveChangesAsync(ct);
        return template;
    }

    public async Task DeleteTemplateAsync(Guid id, CancellationToken ct = default)
    {
        var template = await Context.Set<InvoiceTemplate>().FindAsync([id], ct);
        if (template != null)
        {
            Context.Set<InvoiceTemplate>().Remove(template);
            await Context.SaveChangesAsync(ct);
        }
    }
}
