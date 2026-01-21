using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for review operations.
/// </summary>
public interface IReviewRepository : IRepository<Review>
{
    /// <summary>
    /// Gets reviews by product ID.
    /// </summary>
    Task<IReadOnlyList<Review>> GetByProductIdAsync(Guid productId, bool approvedOnly = true, CancellationToken ct = default);

    /// <summary>
    /// Gets paginated reviews for a product.
    /// </summary>
    Task<PagedResult<Review>> GetPagedByProductIdAsync(Guid productId, int page = 1, int pageSize = 10, bool approvedOnly = true, CancellationToken ct = default);

    /// <summary>
    /// Gets reviews by customer ID.
    /// </summary>
    Task<IReadOnlyList<Review>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Gets pending reviews (awaiting approval).
    /// </summary>
    Task<IReadOnlyList<Review>> GetPendingAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets featured reviews for a product.
    /// </summary>
    Task<IReadOnlyList<Review>> GetFeaturedByProductIdAsync(Guid productId, int count = 5, CancellationToken ct = default);

    /// <summary>
    /// Gets average rating for a product.
    /// </summary>
    Task<double?> GetAverageRatingAsync(Guid productId, CancellationToken ct = default);

    /// <summary>
    /// Gets rating distribution for a product.
    /// </summary>
    Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid productId, CancellationToken ct = default);

    /// <summary>
    /// Gets review count for a product.
    /// </summary>
    Task<int> GetCountByProductIdAsync(Guid productId, bool approvedOnly = true, CancellationToken ct = default);

    /// <summary>
    /// Checks if a customer has reviewed a product.
    /// </summary>
    Task<bool> HasCustomerReviewedAsync(Guid customerId, Guid productId, CancellationToken ct = default);

    /// <summary>
    /// Approves a review.
    /// </summary>
    Task ApproveAsync(Guid reviewId, CancellationToken ct = default);

    /// <summary>
    /// Increments helpful votes.
    /// </summary>
    Task IncrementHelpfulVotesAsync(Guid reviewId, CancellationToken ct = default);

    /// <summary>
    /// Increments unhelpful votes.
    /// </summary>
    Task IncrementUnhelpfulVotesAsync(Guid reviewId, CancellationToken ct = default);
}
