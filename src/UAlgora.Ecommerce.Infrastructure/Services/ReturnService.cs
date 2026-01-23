using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Services;

/// <summary>
/// Service implementation for Return/Refund operations.
/// </summary>
public class ReturnService : IReturnService
{
    private readonly IReturnRepository _returnRepository;
    private readonly IOrderRepository _orderRepository;

    public ReturnService(
        IReturnRepository returnRepository,
        IOrderRepository orderRepository)
    {
        _returnRepository = returnRepository;
        _orderRepository = orderRepository;
    }

    public async Task<Return?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _returnRepository.GetByIdAsync(id, ct);
    }

    public async Task<Return?> GetByReturnNumberAsync(string returnNumber, CancellationToken ct = default)
    {
        return await _returnRepository.GetByReturnNumberAsync(returnNumber, ct);
    }

    public async Task<IReadOnlyList<Return>> GetByOrderAsync(Guid orderId, CancellationToken ct = default)
    {
        return await _returnRepository.GetByOrderAsync(orderId, ct);
    }

    public async Task<IReadOnlyList<Return>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _returnRepository.GetByCustomerAsync(customerId, ct);
    }

    public async Task<IReadOnlyList<Return>> GetByStatusAsync(ReturnStatus status, CancellationToken ct = default)
    {
        return await _returnRepository.GetByStatusAsync(status, ct);
    }

    public async Task<IReadOnlyList<Return>> GetPendingAsync(CancellationToken ct = default)
    {
        return await _returnRepository.GetPendingAsync(ct);
    }

    public async Task<Return> CreateAsync(Return returnRequest, CancellationToken ct = default)
    {
        // Generate return number if not provided
        if (string.IsNullOrEmpty(returnRequest.ReturnNumber))
        {
            returnRequest.ReturnNumber = await GenerateReturnNumberAsync(returnRequest.StoreId ?? Guid.Empty, ct);
        }

        // Set defaults
        returnRequest.RequestedAt = DateTime.UtcNow;
        returnRequest.Status = ReturnStatus.Requested;

        // Calculate total refund amount from items if not set
        if (returnRequest.RefundAmount == 0 && returnRequest.Items.Count > 0)
        {
            returnRequest.RefundAmount = returnRequest.Items.Sum(i => i.RefundAmount);
        }

        return await _returnRepository.AddAsync(returnRequest, ct);
    }

    public async Task<string> GenerateReturnNumberAsync(Guid storeId, CancellationToken ct = default)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random().Next(1000, 9999);
        var returnNumber = $"RMA-{timestamp}-{random}";

        // Check if it already exists
        var existing = await _returnRepository.GetByReturnNumberAsync(returnNumber, ct);
        if (existing != null)
        {
            // Recursively generate a new one
            return await GenerateReturnNumberAsync(storeId, ct);
        }

        return returnNumber;
    }

    public async Task<ReturnApprovalResult> ApproveAsync(Guid returnId, decimal? approvedAmount = null, string? notes = null, string? processedBy = null, CancellationToken ct = default)
    {
        var returnRequest = await _returnRepository.GetByIdAsync(returnId, ct);
        if (returnRequest == null)
        {
            return new ReturnApprovalResult
            {
                Success = false,
                ErrorMessage = "Return request not found."
            };
        }

        if (!returnRequest.CanApprove)
        {
            return new ReturnApprovalResult
            {
                Success = false,
                ErrorMessage = $"Return cannot be approved. Current status: {returnRequest.Status}"
            };
        }

        returnRequest.Status = ReturnStatus.Approved;
        returnRequest.ApprovedAt = DateTime.UtcNow;
        returnRequest.ApprovedRefundAmount = approvedAmount ?? returnRequest.RefundAmount;
        returnRequest.ProcessedBy = processedBy;
        if (!string.IsNullOrEmpty(notes))
        {
            returnRequest.AdminNotes = notes;
        }

        await _returnRepository.UpdateAsync(returnRequest, ct);

        return new ReturnApprovalResult
        {
            Success = true,
            ApprovedAmount = returnRequest.ApprovedRefundAmount ?? 0,
            ReturnLabelUrl = returnRequest.ReturnLabelUrl,
            TrackingNumber = returnRequest.ReturnTrackingNumber
        };
    }

    public async Task<bool> RejectAsync(Guid returnId, string reason, string? processedBy = null, CancellationToken ct = default)
    {
        var returnRequest = await _returnRepository.GetByIdAsync(returnId, ct);
        if (returnRequest == null || !returnRequest.CanReject)
        {
            return false;
        }

        returnRequest.Status = ReturnStatus.Rejected;
        returnRequest.RejectedAt = DateTime.UtcNow;
        returnRequest.RejectionReason = reason;
        returnRequest.ProcessedBy = processedBy;

        await _returnRepository.UpdateAsync(returnRequest, ct);
        return true;
    }

    public async Task<bool> MarkReceivedAsync(Guid returnId, string? processedBy = null, CancellationToken ct = default)
    {
        var returnRequest = await _returnRepository.GetByIdAsync(returnId, ct);
        if (returnRequest == null)
        {
            return false;
        }

        returnRequest.Status = ReturnStatus.ItemsReceived;
        returnRequest.ReceivedAt = DateTime.UtcNow;
        returnRequest.ProcessedBy = processedBy;

        await _returnRepository.UpdateAsync(returnRequest, ct);
        return true;
    }

    public async Task<RefundResult> ProcessRefundAsync(Guid returnId, string? processedBy = null, CancellationToken ct = default)
    {
        var returnRequest = await _returnRepository.GetByIdAsync(returnId, ct);
        if (returnRequest == null)
        {
            return new RefundResult
            {
                Success = false,
                ErrorMessage = "Return request not found."
            };
        }

        if (returnRequest.Status != ReturnStatus.ItemsReceived &&
            returnRequest.Status != ReturnStatus.InspectionInProgress)
        {
            return new RefundResult
            {
                Success = false,
                ErrorMessage = $"Return must have items received before processing refund. Current status: {returnRequest.Status}"
            };
        }

        returnRequest.Status = ReturnStatus.RefundProcessing;
        await _returnRepository.UpdateAsync(returnRequest, ct);

        // Calculate net refund
        var netRefund = returnRequest.NetRefundAmount;

        // In a real implementation, this would integrate with payment provider
        // For now, we'll simulate success
        returnRequest.Status = ReturnStatus.Refunded;
        returnRequest.RefundedAt = DateTime.UtcNow;
        returnRequest.ProcessedBy = processedBy;

        await _returnRepository.UpdateAsync(returnRequest, ct);

        return new RefundResult
        {
            Success = true,
            RefundedAmount = netRefund,
            RefundMethod = returnRequest.RefundMethod,
            TransactionId = $"REF-{Guid.NewGuid():N}"
        };
    }

    public async Task<bool> CompleteAsync(Guid returnId, string? processedBy = null, CancellationToken ct = default)
    {
        var returnRequest = await _returnRepository.GetByIdAsync(returnId, ct);
        if (returnRequest == null)
        {
            return false;
        }

        returnRequest.Status = ReturnStatus.Completed;
        returnRequest.CompletedAt = DateTime.UtcNow;
        returnRequest.ProcessedBy = processedBy;

        await _returnRepository.UpdateAsync(returnRequest, ct);
        return true;
    }

    public async Task<bool> UpdateStatusAsync(Guid returnId, ReturnStatus newStatus, string? processedBy = null, string? notes = null, CancellationToken ct = default)
    {
        var returnRequest = await _returnRepository.GetByIdAsync(returnId, ct);
        if (returnRequest == null)
        {
            return false;
        }

        returnRequest.Status = newStatus;
        returnRequest.ProcessedBy = processedBy;
        if (!string.IsNullOrEmpty(notes))
        {
            returnRequest.AdminNotes = (returnRequest.AdminNotes ?? "") + $"\n[{DateTime.UtcNow:g}] {notes}";
        }

        // Set timestamps based on status
        switch (newStatus)
        {
            case ReturnStatus.Approved:
                returnRequest.ApprovedAt = DateTime.UtcNow;
                break;
            case ReturnStatus.Rejected:
                returnRequest.RejectedAt = DateTime.UtcNow;
                break;
            case ReturnStatus.ItemsReceived:
                returnRequest.ReceivedAt = DateTime.UtcNow;
                break;
            case ReturnStatus.Refunded:
                returnRequest.RefundedAt = DateTime.UtcNow;
                break;
            case ReturnStatus.Completed:
                returnRequest.CompletedAt = DateTime.UtcNow;
                break;
        }

        await _returnRepository.UpdateAsync(returnRequest, ct);
        return true;
    }

    public async Task<ReturnValidationResult> ValidateReturnRequestAsync(Guid orderId, IEnumerable<ReturnItemRequest> items, CancellationToken ct = default)
    {
        var result = new ReturnValidationResult { IsValid = true };
        var order = await _orderRepository.GetByIdAsync(orderId, ct);

        if (order == null)
        {
            result.IsValid = false;
            result.Errors.Add("Order not found.");
            return result;
        }

        // Check if order is eligible for return (typically within 30 days)
        var returnWindowDays = 30;
        if (order.CreatedAt.AddDays(returnWindowDays) < DateTime.UtcNow)
        {
            result.IsValid = false;
            result.Errors.Add($"Return window has expired. Orders must be returned within {returnWindowDays} days.");
            return result;
        }

        // Calculate estimated refund
        decimal totalRefund = 0;
        foreach (var item in items)
        {
            var orderLine = order.Lines.FirstOrDefault(l => l.Id == item.OrderLineId);
            if (orderLine == null)
            {
                result.IsValid = false;
                result.Errors.Add($"Order line {item.OrderLineId} not found in order.");
                continue;
            }

            if (item.Quantity > orderLine.Quantity)
            {
                result.IsValid = false;
                result.Errors.Add($"Return quantity ({item.Quantity}) exceeds ordered quantity ({orderLine.Quantity}) for order line {item.OrderLineId}.");
                continue;
            }

            // Calculate refund for this item
            var unitPrice = orderLine.UnitPrice;
            totalRefund += unitPrice * item.Quantity;
        }

        result.EstimatedRefund = totalRefund;
        return result;
    }

    public async Task<ReturnStatistics> GetStatisticsAsync(Guid? storeId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        // Get all returns in the date range
        var returns = await _returnRepository.GetByDateRangeAsync(startDate, endDate, ct);

        // Filter by store if specified
        if (storeId.HasValue)
        {
            returns = returns.Where(r => r.StoreId == storeId.Value).ToList();
        }

        var stats = new ReturnStatistics
        {
            TotalReturns = returns.Count,
            PendingReturns = returns.Count(r => r.Status == ReturnStatus.Requested || r.Status == ReturnStatus.UnderReview),
            ApprovedReturns = returns.Count(r => r.Status == ReturnStatus.Approved),
            RejectedReturns = returns.Count(r => r.Status == ReturnStatus.Rejected),
            CompletedReturns = returns.Count(r => r.Status == ReturnStatus.Completed || r.Status == ReturnStatus.Refunded),
            TotalRefundAmount = returns.Where(r => r.RefundedAt.HasValue).Sum(r => r.ApprovedRefundAmount ?? r.RefundAmount),
            ReasonBreakdown = new Dictionary<ReturnReason, int>()
        };

        stats.AverageRefundAmount = stats.CompletedReturns > 0
            ? stats.TotalRefundAmount / stats.CompletedReturns
            : 0;

        // Calculate reason breakdown from return items
        var allItems = returns.SelectMany(r => r.Items);
        foreach (var item in allItems)
        {
            if (!stats.ReasonBreakdown.ContainsKey(item.Reason))
            {
                stats.ReasonBreakdown[item.Reason] = 0;
            }
            stats.ReasonBreakdown[item.Reason]++;
        }

        return stats;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _returnRepository.SoftDeleteAsync(id, ct);
    }
}
