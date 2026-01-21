using Microsoft.EntityFrameworkCore;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Infrastructure.Services;

/// <summary>
/// Service implementation for review management operations.
/// </summary>
public class ReviewService : IReviewService
{
    private readonly EcommerceDbContext _context;

    public ReviewService(EcommerceDbContext context)
    {
        _context = context;
    }

    #region Reviews

    public async Task<List<Review>> GetAllReviewsAsync(bool includePending = false, CancellationToken ct = default)
    {
        var query = _context.Reviews
            .Include(r => r.Product)
            .Include(r => r.Customer)
            .AsQueryable();

        if (!includePending)
        {
            query = query.Where(r => r.IsApproved);
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<PagedResult<Review>> GetPagedReviewsAsync(
        int page = 1,
        int pageSize = 20,
        string? status = null,
        int? rating = null,
        Guid? productId = null,
        string? sortBy = null,
        bool descending = true,
        CancellationToken ct = default)
    {
        var query = _context.Reviews
            .Include(r => r.Product)
            .Include(r => r.Customer)
            .AsQueryable();

        // Apply status filter
        if (!string.IsNullOrEmpty(status))
        {
            query = status.ToLower() switch
            {
                "pending" => query.Where(r => !r.IsApproved),
                "approved" => query.Where(r => r.IsApproved),
                "featured" => query.Where(r => r.IsFeatured),
                "verified" => query.Where(r => r.IsVerifiedPurchase),
                "with-response" => query.Where(r => r.MerchantResponse != null),
                _ => query
            };
        }

        // Apply rating filter
        if (rating.HasValue)
        {
            query = query.Where(r => r.Rating == rating.Value);
        }

        // Apply product filter
        if (productId.HasValue)
        {
            query = query.Where(r => r.ProductId == productId.Value);
        }

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "rating" => descending ? query.OrderByDescending(r => r.Rating) : query.OrderBy(r => r.Rating),
            "helpful" => descending ? query.OrderByDescending(r => r.HelpfulVotes) : query.OrderBy(r => r.HelpfulVotes),
            "votes" => descending ? query.OrderByDescending(r => r.HelpfulVotes + r.UnhelpfulVotes) : query.OrderBy(r => r.HelpfulVotes + r.UnhelpfulVotes),
            "reviewer" => descending ? query.OrderByDescending(r => r.ReviewerName) : query.OrderBy(r => r.ReviewerName),
            _ => descending ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt)
        };

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

    public async Task<Review?> GetReviewByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Reviews
            .Include(r => r.Product)
            .Include(r => r.Customer)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<List<Review>> GetReviewsByProductIdAsync(Guid productId, bool approvedOnly = true, CancellationToken ct = default)
    {
        var query = _context.Reviews
            .Include(r => r.Customer)
            .Where(r => r.ProductId == productId);

        if (approvedOnly)
        {
            query = query.Where(r => r.IsApproved);
        }

        return await query
            .OrderByDescending(r => r.IsFeatured)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Review>> GetReviewsByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _context.Reviews
            .Include(r => r.Product)
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Review>> GetPendingReviewsAsync(CancellationToken ct = default)
    {
        return await _context.Reviews
            .Include(r => r.Product)
            .Include(r => r.Customer)
            .Where(r => !r.IsApproved)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Review>> GetFeaturedReviewsAsync(Guid productId, int count = 5, CancellationToken ct = default)
    {
        return await _context.Reviews
            .Include(r => r.Customer)
            .Where(r => r.ProductId == productId && r.IsApproved && r.IsFeatured)
            .OrderByDescending(r => r.HelpfulVotes)
            .ThenByDescending(r => r.Rating)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<Review> CreateReviewAsync(Review review, CancellationToken ct = default)
    {
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync(ct);
        return review;
    }

    public async Task<Review> UpdateReviewAsync(Review review, CancellationToken ct = default)
    {
        _context.Reviews.Update(review);
        await _context.SaveChangesAsync(ct);
        return review;
    }

    public async Task DeleteReviewAsync(Guid id, CancellationToken ct = default)
    {
        var review = await _context.Reviews.FindAsync([id], ct);
        if (review != null)
        {
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync(ct);
        }
    }

    #endregion

    #region Review Actions

    public async Task<Review> ApproveReviewAsync(Guid id, CancellationToken ct = default)
    {
        var review = await _context.Reviews.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Review {id} not found");

        review.IsApproved = true;
        await _context.SaveChangesAsync(ct);
        return review;
    }

    public async Task<Review> RejectReviewAsync(Guid id, string? reason = null, CancellationToken ct = default)
    {
        var review = await _context.Reviews.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Review {id} not found");

        // Delete the review when rejected (or you could add a RejectionReason field)
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync(ct);
        return review;
    }

    public async Task<Review> ToggleFeaturedAsync(Guid id, CancellationToken ct = default)
    {
        var review = await _context.Reviews.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Review {id} not found");

        review.IsFeatured = !review.IsFeatured;
        await _context.SaveChangesAsync(ct);
        return review;
    }

    public async Task<Review> ToggleVerifiedPurchaseAsync(Guid id, CancellationToken ct = default)
    {
        var review = await _context.Reviews.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Review {id} not found");

        review.IsVerifiedPurchase = !review.IsVerifiedPurchase;
        await _context.SaveChangesAsync(ct);
        return review;
    }

    public async Task<Review> AddMerchantResponseAsync(Guid id, string response, CancellationToken ct = default)
    {
        var review = await _context.Reviews.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Review {id} not found");

        review.MerchantResponse = response;
        review.MerchantRespondedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return review;
    }

    public async Task<Review> RemoveMerchantResponseAsync(Guid id, CancellationToken ct = default)
    {
        var review = await _context.Reviews.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Review {id} not found");

        review.MerchantResponse = null;
        review.MerchantRespondedAt = null;
        await _context.SaveChangesAsync(ct);
        return review;
    }

    public async Task<Review> UpdateRatingAsync(Guid id, int rating, CancellationToken ct = default)
    {
        if (rating < 1 || rating > 5)
        {
            throw new ArgumentException("Rating must be between 1 and 5", nameof(rating));
        }

        var review = await _context.Reviews.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Review {id} not found");

        review.Rating = rating;
        await _context.SaveChangesAsync(ct);
        return review;
    }

    public async Task<Review> ResetVotesAsync(Guid id, CancellationToken ct = default)
    {
        var review = await _context.Reviews.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Review {id} not found");

        review.HelpfulVotes = 0;
        review.UnhelpfulVotes = 0;
        await _context.SaveChangesAsync(ct);
        return review;
    }

    public async Task<Review> MarkHelpfulAsync(Guid id, CancellationToken ct = default)
    {
        var review = await _context.Reviews.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Review {id} not found");

        review.HelpfulVotes++;
        await _context.SaveChangesAsync(ct);
        return review;
    }

    public async Task<Review> MarkUnhelpfulAsync(Guid id, CancellationToken ct = default)
    {
        var review = await _context.Reviews.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Review {id} not found");

        review.UnhelpfulVotes++;
        await _context.SaveChangesAsync(ct);
        return review;
    }

    #endregion

    #region Bulk Actions

    public async Task<int> BulkApproveAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var reviews = await _context.Reviews
            .Where(r => ids.Contains(r.Id) && !r.IsApproved)
            .ToListAsync(ct);

        foreach (var review in reviews)
        {
            review.IsApproved = true;
        }

        await _context.SaveChangesAsync(ct);
        return reviews.Count;
    }

    public async Task<int> BulkRejectAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var reviews = await _context.Reviews
            .Where(r => ids.Contains(r.Id))
            .ToListAsync(ct);

        _context.Reviews.RemoveRange(reviews);
        await _context.SaveChangesAsync(ct);
        return reviews.Count;
    }

    public async Task<int> BulkDeleteAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var reviews = await _context.Reviews
            .Where(r => ids.Contains(r.Id))
            .ToListAsync(ct);

        _context.Reviews.RemoveRange(reviews);
        await _context.SaveChangesAsync(ct);
        return reviews.Count;
    }

    #endregion

    #region Statistics

    public async Task<double?> GetAverageRatingAsync(Guid productId, CancellationToken ct = default)
    {
        var hasReviews = await _context.Reviews
            .Where(r => r.ProductId == productId && r.IsApproved)
            .AnyAsync(ct);

        if (!hasReviews)
            return null;

        return await _context.Reviews
            .Where(r => r.ProductId == productId && r.IsApproved)
            .AverageAsync(r => r.Rating, ct);
    }

    public async Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid productId, CancellationToken ct = default)
    {
        var distribution = await _context.Reviews
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

    public async Task<ReviewStatistics> GetStatisticsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var weekStart = now.Date.AddDays(-(int)now.DayOfWeek);
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var totalReviews = await _context.Reviews.CountAsync(ct);
        var pendingReviews = await _context.Reviews.CountAsync(r => !r.IsApproved, ct);
        var approvedReviews = await _context.Reviews.CountAsync(r => r.IsApproved, ct);
        var featuredReviews = await _context.Reviews.CountAsync(r => r.IsFeatured, ct);
        var verifiedReviews = await _context.Reviews.CountAsync(r => r.IsVerifiedPurchase, ct);
        var todayReviews = await _context.Reviews.CountAsync(r => r.CreatedAt >= todayStart, ct);
        var weekReviews = await _context.Reviews.CountAsync(r => r.CreatedAt >= weekStart, ct);
        var monthReviews = await _context.Reviews.CountAsync(r => r.CreatedAt >= monthStart, ct);

        double? avgRating = null;
        if (approvedReviews > 0)
        {
            avgRating = await _context.Reviews
                .Where(r => r.IsApproved)
                .AverageAsync(r => r.Rating, ct);
        }

        return new ReviewStatistics
        {
            TotalReviews = totalReviews,
            PendingReviews = pendingReviews,
            ApprovedReviews = approvedReviews,
            FeaturedReviews = featuredReviews,
            VerifiedPurchaseReviews = verifiedReviews,
            OverallAverageRating = avgRating,
            TodayReviews = todayReviews,
            ThisWeekReviews = weekReviews,
            ThisMonthReviews = monthReviews
        };
    }

    public async Task<int> GetReviewCountAsync(Guid productId, bool approvedOnly = true, CancellationToken ct = default)
    {
        var query = _context.Reviews.Where(r => r.ProductId == productId);

        if (approvedOnly)
        {
            query = query.Where(r => r.IsApproved);
        }

        return await query.CountAsync(ct);
    }

    public async Task<bool> HasCustomerReviewedAsync(Guid customerId, Guid productId, CancellationToken ct = default)
    {
        return await _context.Reviews
            .AnyAsync(r => r.CustomerId == customerId && r.ProductId == productId, ct);
    }

    #endregion
}
