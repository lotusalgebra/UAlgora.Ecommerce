using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Services;

/// <summary>
/// Service interface for Return/Refund operations.
/// </summary>
public interface IReturnService
{
    /// <summary>
    /// Gets a return by ID.
    /// </summary>
    Task<Return?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a return by return number.
    /// </summary>
    Task<Return?> GetByReturnNumberAsync(string returnNumber, CancellationToken ct = default);

    /// <summary>
    /// Gets returns by order.
    /// </summary>
    Task<IReadOnlyList<Return>> GetByOrderAsync(Guid orderId, CancellationToken ct = default);

    /// <summary>
    /// Gets returns by customer.
    /// </summary>
    Task<IReadOnlyList<Return>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// Gets returns by status.
    /// </summary>
    Task<IReadOnlyList<Return>> GetByStatusAsync(ReturnStatus status, CancellationToken ct = default);

    /// <summary>
    /// Gets pending returns awaiting approval.
    /// </summary>
    Task<IReadOnlyList<Return>> GetPendingAsync(CancellationToken ct = default);

    /// <summary>
    /// Creates a new return request.
    /// </summary>
    Task<Return> CreateAsync(Return returnRequest, CancellationToken ct = default);

    /// <summary>
    /// Generates a unique return number.
    /// </summary>
    Task<string> GenerateReturnNumberAsync(Guid storeId, CancellationToken ct = default);

    /// <summary>
    /// Approves a return request.
    /// </summary>
    Task<ReturnApprovalResult> ApproveAsync(Guid returnId, decimal? approvedAmount = null, string? notes = null, string? processedBy = null, CancellationToken ct = default);

    /// <summary>
    /// Rejects a return request.
    /// </summary>
    Task<bool> RejectAsync(Guid returnId, string reason, string? processedBy = null, CancellationToken ct = default);

    /// <summary>
    /// Marks items as received.
    /// </summary>
    Task<bool> MarkReceivedAsync(Guid returnId, string? processedBy = null, CancellationToken ct = default);

    /// <summary>
    /// Processes the refund.
    /// </summary>
    Task<RefundResult> ProcessRefundAsync(Guid returnId, string? processedBy = null, CancellationToken ct = default);

    /// <summary>
    /// Completes a return.
    /// </summary>
    Task<bool> CompleteAsync(Guid returnId, string? processedBy = null, CancellationToken ct = default);

    /// <summary>
    /// Updates return status.
    /// </summary>
    Task<bool> UpdateStatusAsync(Guid returnId, ReturnStatus newStatus, string? processedBy = null, string? notes = null, CancellationToken ct = default);

    /// <summary>
    /// Validates if items can be returned.
    /// </summary>
    Task<ReturnValidationResult> ValidateReturnRequestAsync(Guid orderId, IEnumerable<ReturnItemRequest> items, CancellationToken ct = default);

    /// <summary>
    /// Gets return statistics.
    /// </summary>
    Task<ReturnStatistics> GetStatisticsAsync(Guid? storeId, DateTime startDate, DateTime endDate, CancellationToken ct = default);

    /// <summary>
    /// Deletes a return (soft delete).
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

/// <summary>
/// Return item request for validation.
/// </summary>
public class ReturnItemRequest
{
    public Guid OrderLineId { get; set; }
    public int Quantity { get; set; }
    public ReturnReason Reason { get; set; }
    public string? ReasonDetails { get; set; }
}

/// <summary>
/// Return validation result.
/// </summary>
public class ReturnValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = [];
    public decimal EstimatedRefund { get; set; }
}

/// <summary>
/// Return approval result.
/// </summary>
public class ReturnApprovalResult
{
    public bool Success { get; set; }
    public decimal ApprovedAmount { get; set; }
    public string? ReturnLabelUrl { get; set; }
    public string? TrackingNumber { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Refund result.
/// </summary>
public class RefundResult
{
    public bool Success { get; set; }
    public decimal RefundedAmount { get; set; }
    public RefundMethod RefundMethod { get; set; }
    public string? TransactionId { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Return statistics.
/// </summary>
public class ReturnStatistics
{
    public int TotalReturns { get; set; }
    public int PendingReturns { get; set; }
    public int ApprovedReturns { get; set; }
    public int RejectedReturns { get; set; }
    public int CompletedReturns { get; set; }
    public decimal TotalRefundAmount { get; set; }
    public decimal AverageRefundAmount { get; set; }
    public Dictionary<ReturnReason, int> ReasonBreakdown { get; set; } = [];
}
