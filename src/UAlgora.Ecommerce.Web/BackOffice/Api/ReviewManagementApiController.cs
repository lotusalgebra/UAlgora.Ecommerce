using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Web.Common.Routing;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// API controller for review management in the backoffice.
/// </summary>
[ApiController]
[BackOfficeRoute("ecommerce/review")]
[MapToApi("ecommerce-management-api")]
public class ReviewManagementApiController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewManagementApiController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    #region Reviews

    /// <summary>
    /// Gets all reviews with optional filtering.
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> GetReviews(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] int? rating = null,
        [FromQuery] Guid? productId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool descending = true,
        CancellationToken ct = default)
    {
        var result = await _reviewService.GetPagedReviewsAsync(page, pageSize, status, rating, productId, sortBy, descending, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets a review by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetReview(Guid id, CancellationToken ct = default)
    {
        var review = await _reviewService.GetReviewByIdAsync(id, ct);
        if (review == null)
        {
            return NotFound();
        }
        return Ok(review);
    }

    /// <summary>
    /// Gets pending reviews awaiting approval.
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingReviews(CancellationToken ct = default)
    {
        var reviews = await _reviewService.GetPendingReviewsAsync(ct);
        return Ok(new ReviewListResponse { Items = reviews });
    }

    /// <summary>
    /// Gets reviews by product ID.
    /// </summary>
    [HttpGet("product/{productId:guid}")]
    public async Task<IActionResult> GetProductReviews(Guid productId, [FromQuery] bool approvedOnly = false, CancellationToken ct = default)
    {
        var reviews = await _reviewService.GetReviewsByProductIdAsync(productId, approvedOnly, ct);
        return Ok(new ReviewListResponse { Items = reviews });
    }

    /// <summary>
    /// Gets reviews by customer ID.
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    public async Task<IActionResult> GetCustomerReviews(Guid customerId, CancellationToken ct = default)
    {
        var reviews = await _reviewService.GetReviewsByCustomerIdAsync(customerId, ct);
        return Ok(new ReviewListResponse { Items = reviews });
    }

    /// <summary>
    /// Gets featured reviews for a product.
    /// </summary>
    [HttpGet("product/{productId:guid}/featured")]
    public async Task<IActionResult> GetFeaturedReviews(Guid productId, [FromQuery] int count = 5, CancellationToken ct = default)
    {
        var reviews = await _reviewService.GetFeaturedReviewsAsync(productId, count, ct);
        return Ok(new ReviewListResponse { Items = reviews });
    }

    /// <summary>
    /// Creates a new review.
    /// </summary>
    [HttpPost("")]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request, CancellationToken ct = default)
    {
        if (request.Rating < 1 || request.Rating > 5)
        {
            return BadRequest(new { message = "Rating must be between 1 and 5" });
        }

        var review = new Review
        {
            ProductId = request.ProductId,
            CustomerId = request.CustomerId,
            OrderId = request.OrderId,
            ReviewerName = request.ReviewerName,
            ReviewerEmail = request.ReviewerEmail,
            Rating = request.Rating,
            Title = request.Title,
            Content = request.Content,
            Pros = request.Pros,
            Cons = request.Cons,
            Recommends = request.Recommends,
            ImageUrls = request.ImageUrls ?? [],
            IsVerifiedPurchase = request.IsVerifiedPurchase,
            IsApproved = request.IsApproved,
            IsFeatured = request.IsFeatured
        };

        var created = await _reviewService.CreateReviewAsync(review, ct);
        return CreatedAtAction(nameof(GetReview), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates a review.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateReview(Guid id, [FromBody] UpdateReviewRequest request, CancellationToken ct = default)
    {
        var review = await _reviewService.GetReviewByIdAsync(id, ct);
        if (review == null)
        {
            return NotFound();
        }

        if (request.Rating < 1 || request.Rating > 5)
        {
            return BadRequest(new { message = "Rating must be between 1 and 5" });
        }

        review.ReviewerName = request.ReviewerName;
        review.ReviewerEmail = request.ReviewerEmail;
        review.Rating = request.Rating;
        review.Title = request.Title;
        review.Content = request.Content;
        review.Pros = request.Pros;
        review.Cons = request.Cons;
        review.Recommends = request.Recommends;
        review.ImageUrls = request.ImageUrls ?? [];
        review.IsVerifiedPurchase = request.IsVerifiedPurchase;
        review.IsApproved = request.IsApproved;
        review.IsFeatured = request.IsFeatured;

        var updated = await _reviewService.UpdateReviewAsync(review, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a review.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteReview(Guid id, CancellationToken ct = default)
    {
        var review = await _reviewService.GetReviewByIdAsync(id, ct);
        if (review == null)
        {
            return NotFound();
        }

        await _reviewService.DeleteReviewAsync(id, ct);
        return NoContent();
    }

    #endregion

    #region Review Actions

    /// <summary>
    /// Approves a review.
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> ApproveReview(Guid id, CancellationToken ct = default)
    {
        var review = await _reviewService.GetReviewByIdAsync(id, ct);
        if (review == null)
        {
            return NotFound();
        }

        var updated = await _reviewService.ApproveReviewAsync(id, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Rejects a review.
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> RejectReview(Guid id, [FromBody] RejectReviewRequest? request, CancellationToken ct = default)
    {
        var review = await _reviewService.GetReviewByIdAsync(id, ct);
        if (review == null)
        {
            return NotFound();
        }

        await _reviewService.RejectReviewAsync(id, request?.Reason, ct);
        return NoContent();
    }

    /// <summary>
    /// Toggles the featured status of a review.
    /// </summary>
    [HttpPost("{id:guid}/toggle-featured")]
    public async Task<IActionResult> ToggleFeatured(Guid id, CancellationToken ct = default)
    {
        var review = await _reviewService.GetReviewByIdAsync(id, ct);
        if (review == null)
        {
            return NotFound();
        }

        var updated = await _reviewService.ToggleFeaturedAsync(id, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Toggles the verified purchase status of a review.
    /// </summary>
    [HttpPost("{id:guid}/toggle-verified")]
    public async Task<IActionResult> ToggleVerified(Guid id, CancellationToken ct = default)
    {
        var review = await _reviewService.GetReviewByIdAsync(id, ct);
        if (review == null)
        {
            return NotFound();
        }

        var updated = await _reviewService.ToggleVerifiedPurchaseAsync(id, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Adds a merchant response to a review.
    /// </summary>
    [HttpPost("{id:guid}/respond")]
    public async Task<IActionResult> AddMerchantResponse(Guid id, [FromBody] MerchantResponseRequest request, CancellationToken ct = default)
    {
        var review = await _reviewService.GetReviewByIdAsync(id, ct);
        if (review == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Response))
        {
            return BadRequest(new { message = "Response is required" });
        }

        var updated = await _reviewService.AddMerchantResponseAsync(id, request.Response, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Removes the merchant response from a review.
    /// </summary>
    [HttpPost("{id:guid}/remove-response")]
    public async Task<IActionResult> RemoveMerchantResponse(Guid id, CancellationToken ct = default)
    {
        var review = await _reviewService.GetReviewByIdAsync(id, ct);
        if (review == null)
        {
            return NotFound();
        }

        var updated = await _reviewService.RemoveMerchantResponseAsync(id, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Updates the rating of a review.
    /// </summary>
    [HttpPost("{id:guid}/update-rating")]
    public async Task<IActionResult> UpdateRating(Guid id, [FromBody] UpdateRatingRequest request, CancellationToken ct = default)
    {
        var review = await _reviewService.GetReviewByIdAsync(id, ct);
        if (review == null)
        {
            return NotFound();
        }

        if (request.Rating < 1 || request.Rating > 5)
        {
            return BadRequest(new { message = "Rating must be between 1 and 5" });
        }

        var updated = await _reviewService.UpdateRatingAsync(id, request.Rating, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Resets the votes on a review.
    /// </summary>
    [HttpPost("{id:guid}/reset-votes")]
    public async Task<IActionResult> ResetVotes(Guid id, CancellationToken ct = default)
    {
        var review = await _reviewService.GetReviewByIdAsync(id, ct);
        if (review == null)
        {
            return NotFound();
        }

        var updated = await _reviewService.ResetVotesAsync(id, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Marks a review as helpful.
    /// </summary>
    [HttpPost("{id:guid}/helpful")]
    public async Task<IActionResult> MarkHelpful(Guid id, CancellationToken ct = default)
    {
        var review = await _reviewService.GetReviewByIdAsync(id, ct);
        if (review == null)
        {
            return NotFound();
        }

        var updated = await _reviewService.MarkHelpfulAsync(id, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Marks a review as unhelpful.
    /// </summary>
    [HttpPost("{id:guid}/unhelpful")]
    public async Task<IActionResult> MarkUnhelpful(Guid id, CancellationToken ct = default)
    {
        var review = await _reviewService.GetReviewByIdAsync(id, ct);
        if (review == null)
        {
            return NotFound();
        }

        var updated = await _reviewService.MarkUnhelpfulAsync(id, ct);
        return Ok(updated);
    }

    #endregion

    #region Bulk Actions

    /// <summary>
    /// Approves multiple reviews.
    /// </summary>
    [HttpPost("bulk/approve")]
    public async Task<IActionResult> BulkApprove([FromBody] BulkReviewRequest request, CancellationToken ct = default)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return BadRequest(new { message = "No review IDs provided" });
        }

        var count = await _reviewService.BulkApproveAsync(request.Ids, ct);
        return Ok(new { approvedCount = count });
    }

    /// <summary>
    /// Rejects multiple reviews.
    /// </summary>
    [HttpPost("bulk/reject")]
    public async Task<IActionResult> BulkReject([FromBody] BulkReviewRequest request, CancellationToken ct = default)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return BadRequest(new { message = "No review IDs provided" });
        }

        var count = await _reviewService.BulkRejectAsync(request.Ids, ct);
        return Ok(new { rejectedCount = count });
    }

    /// <summary>
    /// Deletes multiple reviews.
    /// </summary>
    [HttpPost("bulk/delete")]
    public async Task<IActionResult> BulkDelete([FromBody] BulkReviewRequest request, CancellationToken ct = default)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return BadRequest(new { message = "No review IDs provided" });
        }

        var count = await _reviewService.BulkDeleteAsync(request.Ids, ct);
        return Ok(new { deletedCount = count });
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Gets review statistics.
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(CancellationToken ct = default)
    {
        var stats = await _reviewService.GetStatisticsAsync(ct);
        return Ok(stats);
    }

    /// <summary>
    /// Gets the average rating for a product.
    /// </summary>
    [HttpGet("product/{productId:guid}/average-rating")]
    public async Task<IActionResult> GetAverageRating(Guid productId, CancellationToken ct = default)
    {
        var rating = await _reviewService.GetAverageRatingAsync(productId, ct);
        var count = await _reviewService.GetReviewCountAsync(productId, true, ct);
        return Ok(new { averageRating = rating, reviewCount = count });
    }

    /// <summary>
    /// Gets the rating distribution for a product.
    /// </summary>
    [HttpGet("product/{productId:guid}/rating-distribution")]
    public async Task<IActionResult> GetRatingDistribution(Guid productId, CancellationToken ct = default)
    {
        var distribution = await _reviewService.GetRatingDistributionAsync(productId, ct);
        return Ok(distribution);
    }

    #endregion
}

#region Request/Response Models

public class ReviewListResponse
{
    public List<Review> Items { get; set; } = [];
}

public class CreateReviewRequest
{
    public Guid ProductId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? OrderId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string? ReviewerEmail { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Pros { get; set; }
    public string? Cons { get; set; }
    public bool? Recommends { get; set; }
    public List<string>? ImageUrls { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public bool IsApproved { get; set; }
    public bool IsFeatured { get; set; }
}

public class UpdateReviewRequest
{
    public string ReviewerName { get; set; } = string.Empty;
    public string? ReviewerEmail { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Pros { get; set; }
    public string? Cons { get; set; }
    public bool? Recommends { get; set; }
    public List<string>? ImageUrls { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public bool IsApproved { get; set; }
    public bool IsFeatured { get; set; }
}

public class RejectReviewRequest
{
    public string? Reason { get; set; }
}

public class MerchantResponseRequest
{
    public string Response { get; set; } = string.Empty;
}

public class UpdateRatingRequest
{
    public int Rating { get; set; }
}

public class BulkReviewRequest
{
    public List<Guid>? Ids { get; set; }
}

#endregion
