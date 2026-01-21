namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a product review.
/// </summary>
public class Review : BaseEntity
{
    /// <summary>
    /// Product ID being reviewed.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Customer ID who wrote the review.
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Order ID (for verified purchase).
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// Reviewer name.
    /// </summary>
    public string ReviewerName { get; set; } = string.Empty;

    /// <summary>
    /// Reviewer email.
    /// </summary>
    public string? ReviewerEmail { get; set; }

    /// <summary>
    /// Rating (1-5).
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// Review title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Review content.
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Pros mentioned.
    /// </summary>
    public string? Pros { get; set; }

    /// <summary>
    /// Cons mentioned.
    /// </summary>
    public string? Cons { get; set; }

    /// <summary>
    /// Whether reviewer recommends the product.
    /// </summary>
    public bool? Recommends { get; set; }

    /// <summary>
    /// Review images.
    /// </summary>
    public List<string> ImageUrls { get; set; } = [];

    /// <summary>
    /// Whether this is a verified purchase.
    /// </summary>
    public bool IsVerifiedPurchase { get; set; }

    /// <summary>
    /// Whether the review is approved.
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    /// Whether the review is featured.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Number of helpful votes.
    /// </summary>
    public int HelpfulVotes { get; set; }

    /// <summary>
    /// Number of unhelpful votes.
    /// </summary>
    public int UnhelpfulVotes { get; set; }

    /// <summary>
    /// Merchant response to the review.
    /// </summary>
    public string? MerchantResponse { get; set; }

    /// <summary>
    /// When merchant responded.
    /// </summary>
    public DateTime? MerchantRespondedAt { get; set; }

    /// <summary>
    /// Navigation property to product.
    /// </summary>
    public Product? Product { get; set; }

    /// <summary>
    /// Navigation property to customer.
    /// </summary>
    public Customer? Customer { get; set; }

    #region Computed Properties

    /// <summary>
    /// Total votes (helpful + unhelpful).
    /// </summary>
    public int TotalVotes => HelpfulVotes + UnhelpfulVotes;

    /// <summary>
    /// Helpfulness percentage.
    /// </summary>
    public decimal? HelpfulnessPercentage =>
        TotalVotes > 0 ? Math.Round((decimal)HelpfulVotes / TotalVotes * 100, 1) : null;

    #endregion
}
