using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for customer operations.
/// </summary>
public interface ICustomerRepository : ISoftDeleteRepository<Customer>
{
    /// <summary>
    /// Gets a customer by email.
    /// </summary>
    Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Gets a customer by Umbraco member ID.
    /// </summary>
    Task<Customer?> GetByUmbracoMemberIdAsync(int memberId, CancellationToken ct = default);

    /// <summary>
    /// Gets a customer with addresses loaded.
    /// </summary>
    Task<Customer?> GetWithAddressesAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a customer with orders loaded.
    /// </summary>
    Task<Customer?> GetWithOrdersAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets paginated customers with filtering and sorting.
    /// </summary>
    Task<PagedResult<Customer>> GetPagedAsync(CustomerQueryParameters parameters, CancellationToken ct = default);

    /// <summary>
    /// Gets customers by status.
    /// </summary>
    Task<IReadOnlyList<Customer>> GetByStatusAsync(CustomerStatus status, CancellationToken ct = default);

    /// <summary>
    /// Gets customers by tier.
    /// </summary>
    Task<IReadOnlyList<Customer>> GetByTierAsync(string tier, CancellationToken ct = default);

    /// <summary>
    /// Gets customers by tag.
    /// </summary>
    Task<IReadOnlyList<Customer>> GetByTagAsync(string tag, CancellationToken ct = default);

    /// <summary>
    /// Gets top customers by total spent.
    /// </summary>
    Task<IReadOnlyList<Customer>> GetTopBySpentAsync(int count = 10, CancellationToken ct = default);

    /// <summary>
    /// Gets recent customers.
    /// </summary>
    Task<IReadOnlyList<Customer>> GetRecentAsync(int count = 10, CancellationToken ct = default);

    /// <summary>
    /// Checks if an email exists.
    /// </summary>
    Task<bool> EmailExistsAsync(string email, Guid? excludeId = null, CancellationToken ct = default);

    /// <summary>
    /// Searches customers by term.
    /// </summary>
    Task<IReadOnlyList<Customer>> SearchAsync(string searchTerm, int maxResults = 20, CancellationToken ct = default);

    /// <summary>
    /// Updates customer statistics.
    /// </summary>
    Task UpdateStatisticsAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Updates last login timestamp.
    /// </summary>
    Task UpdateLastLoginAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Gets all unique tiers.
    /// </summary>
    Task<IReadOnlyList<string>> GetAllTiersAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets all unique tags.
    /// </summary>
    Task<IReadOnlyList<string>> GetAllTagsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets customers who accept marketing.
    /// </summary>
    Task<IReadOnlyList<Customer>> GetMarketingSubscribersAsync(CancellationToken ct = default);
}

/// <summary>
/// Query parameters for customer listing.
/// </summary>
public class CustomerQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public CustomerStatus? Status { get; set; }
    public string? Tier { get; set; }
    public string? Tag { get; set; }
    public bool? AcceptsMarketing { get; set; }
    public DateTime? RegisteredAfter { get; set; }
    public DateTime? RegisteredBefore { get; set; }
    public decimal? MinTotalSpent { get; set; }
    public int? MinOrderCount { get; set; }
    public CustomerSortBy SortBy { get; set; } = CustomerSortBy.Newest;
}

/// <summary>
/// Customer sorting options.
/// </summary>
public enum CustomerSortBy
{
    Newest = 0,
    Oldest = 1,
    NameAscending = 2,
    NameDescending = 3,
    TotalSpentHighToLow = 4,
    TotalSpentLowToHigh = 5,
    OrderCountHighToLow = 6
}
