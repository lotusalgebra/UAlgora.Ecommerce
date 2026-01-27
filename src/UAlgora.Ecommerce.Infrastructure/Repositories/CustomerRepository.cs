using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;
using RepoSortBy = UAlgora.Ecommerce.Core.Interfaces.Repositories.CustomerSortBy;

namespace UAlgora.Ecommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for customer operations.
/// </summary>
public class CustomerRepository : SoftDeleteRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(EcommerceDbContext context) : base(context)
    {
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(c => c.Email == email, ct);
    }

    public async Task<Customer?> GetByUmbracoMemberIdAsync(int memberId, CancellationToken ct = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(c => c.UmbracoMemberId == memberId, ct);
    }

    public async Task<Customer?> GetWithAddressesAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Customer?> GetWithOrdersAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<PagedResult<Customer>> GetPagedAsync(
        CustomerQueryParameters parameters,
        CancellationToken ct = default)
    {
        var query = DbSet.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var term = parameters.SearchTerm.ToLower();
            query = query.Where(c =>
                c.Email.ToLower().Contains(term) ||
                c.FirstName.ToLower().Contains(term) ||
                c.LastName.ToLower().Contains(term) ||
                (c.Phone != null && c.Phone.Contains(term)) ||
                (c.Company != null && c.Company.ToLower().Contains(term)));
        }

        if (parameters.Status.HasValue)
        {
            query = query.Where(c => c.Status == parameters.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Tier))
        {
            query = query.Where(c => c.CustomerTier == parameters.Tier);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Tag))
        {
            query = query.Where(c => c.Tags.Contains(parameters.Tag));
        }

        if (parameters.AcceptsMarketing.HasValue)
        {
            query = query.Where(c => c.AcceptsMarketing == parameters.AcceptsMarketing.Value);
        }

        if (parameters.RegisteredAfter.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= parameters.RegisteredAfter.Value);
        }

        if (parameters.RegisteredBefore.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= parameters.RegisteredBefore.Value);
        }

        if (parameters.MinTotalSpent.HasValue)
        {
            query = query.Where(c => c.TotalSpent >= parameters.MinTotalSpent.Value);
        }

        if (parameters.MinOrderCount.HasValue)
        {
            query = query.Where(c => c.TotalOrders >= parameters.MinOrderCount.Value);
        }

        // Apply sorting
        query = parameters.SortBy switch
        {
            RepoSortBy.Newest => query.OrderByDescending(c => c.CreatedAt),
            RepoSortBy.Oldest => query.OrderBy(c => c.CreatedAt),
            RepoSortBy.NameAscending => query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName),
            RepoSortBy.NameDescending => query.OrderByDescending(c => c.LastName).ThenByDescending(c => c.FirstName),
            RepoSortBy.TotalSpentHighToLow => query.OrderByDescending(c => c.TotalSpent),
            RepoSortBy.TotalSpentLowToHigh => query.OrderBy(c => c.TotalSpent),
            RepoSortBy.OrderCountHighToLow => query.OrderByDescending(c => c.TotalOrders),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };

        // Get total count
        var totalCount = await query.CountAsync(ct);

        // Apply pagination
        var items = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Customer>
        {
            Items = items,
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize
        };
    }

    public async Task<IReadOnlyList<Customer>> GetByStatusAsync(CustomerStatus status, CancellationToken ct = default)
    {
        return await DbSet
            .Where(c => c.Status == status)
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Customer>> GetByTierAsync(string tier, CancellationToken ct = default)
    {
        return await DbSet
            .Where(c => c.CustomerTier == tier)
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Customer>> GetByTagAsync(string tag, CancellationToken ct = default)
    {
        return await DbSet
            .Where(c => c.Tags.Contains(tag))
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Customer>> GetTopBySpentAsync(int count = 10, CancellationToken ct = default)
    {
        return await DbSet
            .Where(c => c.Status == CustomerStatus.Active)
            .OrderByDescending(c => c.TotalSpent)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Customer>> GetRecentAsync(int count = 10, CancellationToken ct = default)
    {
        return await DbSet
            .OrderByDescending(c => c.CreatedAt)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = DbSet.Where(c => c.Email == email);
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }
        return await query.AnyAsync(ct);
    }

    public async Task<IReadOnlyList<Customer>> SearchAsync(
        string searchTerm,
        int maxResults = 20,
        CancellationToken ct = default)
    {
        var term = searchTerm.ToLower();
        return await DbSet
            .Where(c => c.Status == CustomerStatus.Active)
            .Where(c =>
                c.Email.ToLower().Contains(term) ||
                c.FirstName.ToLower().Contains(term) ||
                c.LastName.ToLower().Contains(term) ||
                (c.Phone != null && c.Phone.Contains(term)))
            .OrderByDescending(c => c.Email.ToLower().StartsWith(term))
            .ThenBy(c => c.LastName)
            .Take(maxResults)
            .ToListAsync(ct);
    }

    public async Task UpdateStatisticsAsync(Guid customerId, CancellationToken ct = default)
    {
        var customer = await GetByIdAsync(customerId, ct);
        if (customer == null) return;

        var orders = await Context.Orders
            .Where(o => o.CustomerId == customerId)
            .Where(o => o.PaymentStatus == PaymentStatus.Captured)
            .ToListAsync(ct);

        customer.TotalOrders = orders.Count;
        customer.TotalSpent = orders.Sum(o => o.GrandTotal - o.RefundedAmount);
        customer.AverageOrderValue = orders.Count > 0 ? customer.TotalSpent / orders.Count : 0;
        customer.LastOrderAt = orders.OrderByDescending(o => o.CreatedAt).FirstOrDefault()?.CreatedAt;

        await Context.SaveChangesAsync(ct);
    }

    public async Task UpdateLastLoginAsync(Guid customerId, CancellationToken ct = default)
    {
        var customer = await GetByIdAsync(customerId, ct);
        if (customer != null)
        {
            customer.LastLoginAt = DateTime.UtcNow;
            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task<IReadOnlyList<string>> GetAllTiersAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Where(c => c.CustomerTier != null)
            .Select(c => c.CustomerTier!)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<string>> GetAllTagsAsync(CancellationToken ct = default)
    {
        var customers = await DbSet
            .Where(c => c.Tags.Any())
            .Select(c => c.Tags)
            .ToListAsync(ct);

        return customers
            .SelectMany(t => t)
            .Distinct()
            .OrderBy(t => t)
            .ToList();
    }

    public async Task<IReadOnlyList<Customer>> GetMarketingSubscribersAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Where(c => c.AcceptsMarketing && c.Status == CustomerStatus.Active)
            .OrderBy(c => c.Email)
            .ToListAsync(ct);
    }
}
