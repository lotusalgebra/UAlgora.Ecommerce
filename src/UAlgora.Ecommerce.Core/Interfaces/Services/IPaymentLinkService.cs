using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Services;

/// <summary>
/// Service interface for Payment Link operations.
/// </summary>
public interface IPaymentLinkService
{
    /// <summary>
    /// Gets a payment link by ID.
    /// </summary>
    Task<PaymentLink?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a payment link by code.
    /// </summary>
    Task<PaymentLink?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Gets all payment links.
    /// </summary>
    Task<IReadOnlyList<PaymentLink>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets payment links by store.
    /// </summary>
    Task<IReadOnlyList<PaymentLink>> GetByStoreAsync(Guid storeId, CancellationToken ct = default);

    /// <summary>
    /// Gets active payment links.
    /// </summary>
    Task<IReadOnlyList<PaymentLink>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Creates a new payment link.
    /// </summary>
    Task<PaymentLink> CreateAsync(PaymentLink paymentLink, CancellationToken ct = default);

    /// <summary>
    /// Updates a payment link.
    /// </summary>
    Task<PaymentLink> UpdateAsync(PaymentLink paymentLink, CancellationToken ct = default);

    /// <summary>
    /// Generates a unique payment link code.
    /// </summary>
    Task<string> GenerateCodeAsync(string? prefix = null, CancellationToken ct = default);

    /// <summary>
    /// Validates a payment link for use.
    /// </summary>
    Task<PaymentLinkValidationResult> ValidateAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Records a payment made through a payment link.
    /// </summary>
    Task<PaymentLinkPaymentResult> RecordPaymentAsync(Guid paymentLinkId, PaymentLinkPayment payment, CancellationToken ct = default);

    /// <summary>
    /// Updates a payment status.
    /// </summary>
    Task<PaymentLinkPayment> UpdatePaymentStatusAsync(Guid paymentId, PaymentLinkPaymentStatus status, CancellationToken ct = default);

    /// <summary>
    /// Gets payments for a payment link.
    /// </summary>
    Task<IReadOnlyList<PaymentLinkPayment>> GetPaymentsAsync(Guid paymentLinkId, CancellationToken ct = default);

    /// <summary>
    /// Gets statistics for a payment link.
    /// </summary>
    Task<PaymentLinkStatistics> GetStatisticsAsync(Guid paymentLinkId, CancellationToken ct = default);

    /// <summary>
    /// Pauses a payment link.
    /// </summary>
    Task<PaymentLink> PauseAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Activates a payment link.
    /// </summary>
    Task<PaymentLink> ActivateAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Archives a payment link.
    /// </summary>
    Task<PaymentLink> ArchiveAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Deletes a payment link (soft delete).
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Deactivates expired payment links.
    /// </summary>
    Task<int> DeactivateExpiredAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets payment links expiring soon.
    /// </summary>
    Task<IReadOnlyList<PaymentLink>> GetExpiringSoonAsync(int days = 30, CancellationToken ct = default);

    /// <summary>
    /// Searches payment links.
    /// </summary>
    Task<PagedResult<PaymentLink>> SearchAsync(
        string? searchTerm = null,
        PaymentLinkStatus? status = null,
        PaymentLinkType? type = null,
        Guid? storeId = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default);

    /// <summary>
    /// Gets the full URL for a payment link.
    /// </summary>
    string GetFullUrl(PaymentLink paymentLink, string baseUrl);

    /// <summary>
    /// Duplicates a payment link.
    /// </summary>
    Task<PaymentLink> DuplicateAsync(Guid id, CancellationToken ct = default);
}

/// <summary>
/// Payment link validation result.
/// </summary>
public class PaymentLinkValidationResult
{
    public bool IsValid { get; set; }
    public PaymentLink? PaymentLink { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }

    public static PaymentLinkValidationResult Success(PaymentLink paymentLink) => new()
    {
        IsValid = true,
        PaymentLink = paymentLink
    };

    public static PaymentLinkValidationResult Failure(string errorCode, string message) => new()
    {
        IsValid = false,
        ErrorCode = errorCode,
        ErrorMessage = message
    };
}

/// <summary>
/// Payment link payment result.
/// </summary>
public class PaymentLinkPaymentResult
{
    public bool Success { get; set; }
    public PaymentLinkPayment? Payment { get; set; }
    public string? ErrorMessage { get; set; }

    public static PaymentLinkPaymentResult Succeeded(PaymentLinkPayment payment) => new()
    {
        Success = true,
        Payment = payment
    };

    public static PaymentLinkPaymentResult Failed(string message) => new()
    {
        Success = false,
        ErrorMessage = message
    };
}
