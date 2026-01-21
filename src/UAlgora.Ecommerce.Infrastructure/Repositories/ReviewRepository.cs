using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for review operations.
/// </summary>
public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(EcommerceDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Review>> GetByProductIdAsync(
        Guid productId,
        bool approvedOnly = true,
        CancellationToken ct = default)
    {
        var query = DbSet.Where(r => r.ProductId == productId);

        if (approvedOnly)
        {
            query = query.Where(r => r.IsApproved);
        }

        return await query
            .OrderByDescending(r => r.IsFeatured)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<PagedResult<Review>> GetPagedByProductIdAsync(
        Guid productId,
        int page = 1,
        int pageSize = 10,
        bool approvedOnly = true,
        CancellationToken ct = default)
    {
        var query = DbSet.Where(r => r.ProductId == productId);

        if (approvedOnly)
        {
            query = query.Where(r => r.IsApproved);
        }

        query = query
            .OrderByDescending(r => r.IsFeatured)
            .ThenByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Review>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IReadOnlyList<Review>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Review>> GetPendingAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Where(r => !r.IsApproved)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Review>> GetFeaturedByProductIdAsync(
        Guid productId,
        int count = 5,
        CancellationToken ct = default)
    {
        return await DbSet
            .Where(r => r.ProductId == productId && r.IsApproved && r.IsFeatured)
            .OrderByDescending(r => r.HelpfulVotes)
            .ThenByDescending(r => r.Rating)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<double?> GetAverageRatingAsync(Guid productId, CancellationToken ct = default)
    {
        var hasReviews = await DbSet
            .Where(r => r.ProductId == productId && r.IsApproved)
            .AnyAsync(ct);

        if (!hasReviews)
            return null;

        return await DbSet
            .Where(r => r.ProductId == productId && r.IsApproved)
            .AverageAsync(r => r.Rating, ct);
    }

    public async Task<Dictionary<int, int>> GetRatingDistributionAsync(
        Guid productId,
        CancellationToken ct = default)
    {
        var distribution = await DbSet
            .Where(r => r.ProductId == productId && r.IsApproved)
            .GroupBy(r => r.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Rating, x => x.Count, ct);

        // Ensure all ratings 1-5 are present
        for (int i = 1; i <= 5; i++)
        {
            if (!distribution.ContainsKey(i))
            {
                distribution[i] = 0;
            }
        }

        return distribution;
    }

    public async Task<int> GetCountByProductIdAsync(
        Guid productId,
        bool approvedOnly = true,
        CancellationToken ct = default)
    {
        var query = DbSet.Where(r => r.ProductId == productId);

        if (approvedOnly)
        {
            query = query.Where(r => r.IsApproved);
        }

        return await query.CountAsync(ct);
    }

    public async Task<bool> HasCustomerReviewedAsync(
        Guid customerId,
        Guid productId,
        CancellationToken ct = default)
    {
        return await DbSet
            .AnyAsync(r => r.CustomerId == customerId && r.ProductId == productId, ct);
    }

    public async Task ApproveAsync(Guid reviewId, CancellationToken ct = default)
    {
        var review = await GetByIdAsync(reviewId, ct);
        if (review != null)
        {
            review.IsApproved = true;
            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task IncrementHelpfulVotesAsync(Guid reviewId, CancellationToken ct = default)
    {
        var review = await GetByIdAsync(reviewId, ct);
        if (review != null)
        {
            review.HelpfulVotes++;
            await Context.SaveChangesAsync(ct);
        }
    }

    public async Task IncrementUnhelpfulVotesAsync(Guid reviewId, CancellationToken ct = default)
    {
        var review = await GetByIdAsync(reviewId, ct);
        if (review != null)
        {
            review.UnhelpfulVotes++;
            await Context.SaveChangesAsync(ct);
        }
    }
}
