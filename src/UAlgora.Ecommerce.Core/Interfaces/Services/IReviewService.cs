using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Services;

/// <summary>
/// Service interface for review management.
/// </summary>
public interface IReviewService
{
    #region Reviews

    /// <summary>
    /// Gets all reviews with optional filtering.
    /// </summary>
    Task<List<Review>> GetAllReviewsAsync(bool includePending = false, CancellationToken ct = default);

    /// <summary>
    /// Gets paginated reviews with filtering and sorting.
    /// </summary>
    Task<PagedResult<Review>> GetPagedReviewsAsync(
        int page = 1,
        int pageSize = 20,
        string? status = null,
        int? rating = null,
        Guid? productId = null,
        string? sortBy = null,
        bool descending = true,
        CancellationToken ct = default);

    /// <summary>
    /// Gets a review by ID.
    /// </summary>
    Task<Review?> GetReviewByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets reviews by product ID.
    /// </summary>
    Task<List<Review>> GetReviewsByProductIdAsync(Guid productId, bool approvedOnly = true, CancellationToken ct = default);

    /// <summary>
    /// Gets reviews by customer ID.
    /// </summary>
    Task<List<Review>> GetReviewsByCustomerIdAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Gets pending reviews awaiting approval.
    /// </summary>
    Task<List<Review>> GetPendingReviewsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets featured reviews for a product.
    /// </summary>
    Task<List<Review>> GetFeaturedReviewsAsync(Guid productId, int count = 5, CancellationToken ct = default);

    /// <summary>
    /// Creates a new review.
    /// </summary>
    Task<Review> CreateReviewAsync(Review review, CancellationToken ct = default);

    /// <summary>
    /// Updates a review.
    /// </summary>
    Task<Review> UpdateReviewAsync(Review review, CancellationToken ct = default);

    /// <summary>
    /// Deletes a review.
    /// </summary>
    Task DeleteReviewAsync(Guid id, CancellationToken ct = default);

    #endregion

    #region Review Actions

    /// <summary>
    /// Approves a review.
    /// </summary>
    Task<Review> ApproveReviewAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Rejects a review.
    /// </summary>
    Task<Review> RejectReviewAsync(Guid id, string? reason = null, CancellationToken ct = default);

    /// <summary>
    /// Toggles the featured status of a review.
    /// </summary>
    Task<Review> ToggleFeaturedAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Toggles the verified purchase status of a review.
    /// </summary>
    Task<Review> ToggleVerifiedPurchaseAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Adds a merchant response to a review.
    /// </summary>
    Task<Review> AddMerchantResponseAsync(Guid id, string response, CancellationToken ct = default);

    /// <summary>
    /// Removes the merchant response from a review.
    /// </summary>
    Task<Review> RemoveMerchantResponseAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Updates the rating of a review.
    /// </summary>
    Task<Review> UpdateRatingAsync(Guid id, int rating, CancellationToken ct = default);

    /// <summary>
    /// Resets the votes on a review.
    /// </summary>
    Task<Review> ResetVotesAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Marks a review as helpful.
    /// </summary>
    Task<Review> MarkHelpfulAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Marks a review as unhelpful.
    /// </summary>
    Task<Review> MarkUnhelpfulAsync(Guid id, CancellationToken ct = default);

    #endregion

    #region Bulk Actions

    /// <summary>
    /// Approves multiple reviews.
    /// </summary>
    Task<int> BulkApproveAsync(IEnumerable<Guid> ids, CancellationToken ct = default);

    /// <summary>
    /// Rejects multiple reviews.
    /// </summary>
    Task<int> BulkRejectAsync(IEnumerable<Guid> ids, CancellationToken ct = default);

    /// <summary>
    /// Deletes multiple reviews.
    /// </summary>
    Task<int> BulkDeleteAsync(IEnumerable<Guid> ids, CancellationToken ct = default);

    #endregion

    #region Statistics

    /// <summary>
    /// Gets the average rating for a product.
    /// </summary>
    Task<double?> GetAverageRatingAsync(Guid productId, CancellationToken ct = default);

    /// <summary>
    /// Gets the rating distribution for a product.
    /// </summary>
    Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid productId, CancellationToken ct = default);

    /// <summary>
    /// Gets review statistics.
    /// </summary>
    Task<ReviewStatistics> GetStatisticsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets review count for a product.
    /// </summary>
    Task<int> GetReviewCountAsync(Guid productId, bool approvedOnly = true, CancellationToken ct = default);

    /// <summary>
    /// Checks if a customer has reviewed a product.
    /// </summary>
    Task<bool> HasCustomerReviewedAsync(Guid customerId, Guid productId, CancellationToken ct = default);

    #endregion
}

/// <summary>
/// Review statistics summary.
/// </summary>
public class ReviewStatistics
{
    public int TotalReviews { get; set; }
    public int PendingReviews { get; set; }
    public int ApprovedReviews { get; set; }
    public int FeaturedReviews { get; set; }
    public int VerifiedPurchaseReviews { get; set; }
    public double? OverallAverageRating { get; set; }
    public int TodayReviews { get; set; }
    public int ThisWeekReviews { get; set; }
    public int ThisMonthReviews { get; set; }
}
