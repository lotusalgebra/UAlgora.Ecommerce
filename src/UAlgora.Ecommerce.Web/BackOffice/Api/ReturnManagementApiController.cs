using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using Umbraco.Cms.Api.Management.Routing;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// Management API controller for return/refund operations in the Umbraco backoffice.
/// </summary>
[VersionedApiBackOfficeRoute($"{EcommerceConstants.ApiRouteBase}/{EcommerceConstants.Routes.Returns}")]
public class ReturnManagementApiController : EcommerceManagementApiControllerBase
{
    private readonly IReturnService _returnService;

    public ReturnManagementApiController(IReturnService returnService)
    {
        _returnService = returnService;
    }

    /// <summary>
    /// Gets pending returns.
    /// </summary>
    [HttpGet("pending")]
    [ProducesResponseType<IReadOnlyList<Return>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPending()
    {
        var returns = await _returnService.GetPendingAsync();
        return Ok(returns);
    }

    /// <summary>
    /// Gets a return by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<Return>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var returnRequest = await _returnService.GetByIdAsync(id);
        if (returnRequest == null)
        {
            return NotFound();
        }
        return Ok(returnRequest);
    }

    /// <summary>
    /// Gets a return by return number.
    /// </summary>
    [HttpGet("by-number/{returnNumber}")]
    [ProducesResponseType<Return>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByReturnNumber(string returnNumber)
    {
        var returnRequest = await _returnService.GetByReturnNumberAsync(returnNumber);
        if (returnRequest == null)
        {
            return NotFound();
        }
        return Ok(returnRequest);
    }

    /// <summary>
    /// Gets returns by order.
    /// </summary>
    [HttpGet("by-order/{orderId:guid}")]
    [ProducesResponseType<IReadOnlyList<Return>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByOrder(Guid orderId)
    {
        var returns = await _returnService.GetByOrderAsync(orderId);
        return Ok(returns);
    }

    /// <summary>
    /// Gets returns by customer.
    /// </summary>
    [HttpGet("by-customer/{customerId:guid}")]
    [ProducesResponseType<IReadOnlyList<Return>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCustomer(Guid customerId)
    {
        var returns = await _returnService.GetByCustomerAsync(customerId);
        return Ok(returns);
    }

    /// <summary>
    /// Gets returns by status.
    /// </summary>
    [HttpGet("by-status/{status}")]
    [ProducesResponseType<IReadOnlyList<Return>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByStatus(ReturnStatus status)
    {
        var returns = await _returnService.GetByStatusAsync(status);
        return Ok(returns);
    }

    /// <summary>
    /// Creates a new return request.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<Return>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateReturnRequest request)
    {
        var returnRequest = new Return
        {
            OrderId = request.OrderId,
            CustomerId = request.CustomerId ?? Guid.Empty,
            StoreId = request.StoreId,
            Reason = request.Reason,
            CustomerNotes = request.CustomerNotes,
            RefundMethod = request.RefundMethod
        };

        // Add items
        foreach (var item in request.Items)
        {
            returnRequest.Items.Add(new ReturnItem
            {
                OrderLineId = item.OrderLineId ?? Guid.Empty,
                ProductId = item.ProductId ?? Guid.Empty,
                Quantity = item.Quantity,
                Reason = item.Reason,
                ReasonDetails = item.ReasonDetails,
                RefundAmount = item.RefundAmount
            });
        }

        var created = await _returnService.CreateAsync(returnRequest);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Approves a return.
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType<ReturnApprovalResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveReturnRequest request)
    {
        var result = await _returnService.ApproveAsync(
            id,
            request.ApprovedAmount,
            request.Notes,
            request.ProcessedBy);
        return Ok(result);
    }

    /// <summary>
    /// Rejects a return.
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectReturnRequest request)
    {
        var result = await _returnService.RejectAsync(id, request.Reason, request.ProcessedBy);
        if (!result)
        {
            return BadRequest(new { error = "Failed to reject return" });
        }
        return Ok(new { success = true });
    }

    /// <summary>
    /// Marks items as received.
    /// </summary>
    [HttpPost("{id:guid}/mark-received")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarkReceived(Guid id, [FromBody] ProcessReturnRequest? request = null)
    {
        var result = await _returnService.MarkReceivedAsync(id, request?.ProcessedBy);
        if (!result)
        {
            return BadRequest(new { error = "Failed to mark as received" });
        }
        return Ok(new { success = true });
    }

    /// <summary>
    /// Processes the refund.
    /// </summary>
    [HttpPost("{id:guid}/process-refund")]
    [ProducesResponseType<RefundResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ProcessRefund(Guid id, [FromBody] ProcessReturnRequest? request = null)
    {
        var result = await _returnService.ProcessRefundAsync(id, request?.ProcessedBy);
        return Ok(result);
    }

    /// <summary>
    /// Completes a return.
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Complete(Guid id, [FromBody] ProcessReturnRequest? request = null)
    {
        var result = await _returnService.CompleteAsync(id, request?.ProcessedBy);
        if (!result)
        {
            return BadRequest(new { error = "Failed to complete return" });
        }
        return Ok(new { success = true });
    }

    /// <summary>
    /// Gets return statistics.
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType<ReturnStatistics>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] Guid? storeId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var stats = await _returnService.GetStatisticsAsync(storeId, start, end);
        return Ok(stats);
    }

    /// <summary>
    /// Validates a return request.
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType<ReturnValidationResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Validate([FromBody] ValidateReturnRequest request)
    {
        var items = request.Items.Select(i => new ReturnItemRequest
        {
            OrderLineId = i.OrderLineId,
            Quantity = i.Quantity,
            Reason = i.Reason,
            ReasonDetails = i.ReasonDetails
        });

        var result = await _returnService.ValidateReturnRequestAsync(request.OrderId, items);
        return Ok(result);
    }
}

#region Request Models

public class CreateReturnRequest
{
    public Guid OrderId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? StoreId { get; set; }
    public ReturnReason Reason { get; set; }
    public string? CustomerNotes { get; set; }
    public RefundMethod RefundMethod { get; set; }
    public List<CreateReturnItemRequest> Items { get; set; } = [];
}

public class CreateReturnItemRequest
{
    public Guid? OrderLineId { get; set; }
    public Guid? ProductId { get; set; }
    public int Quantity { get; set; }
    public ReturnReason Reason { get; set; }
    public string? ReasonDetails { get; set; }
    public decimal RefundAmount { get; set; }
}

public class ApproveReturnRequest
{
    public decimal? ApprovedAmount { get; set; }
    public string? Notes { get; set; }
    public string? ProcessedBy { get; set; }
}

public class RejectReturnRequest
{
    public required string Reason { get; set; }
    public string? ProcessedBy { get; set; }
}

public class ProcessReturnRequest
{
    public string? ProcessedBy { get; set; }
}

public class ValidateReturnRequest
{
    public Guid OrderId { get; set; }
    public List<ValidateReturnItemRequest> Items { get; set; } = [];
}

public class ValidateReturnItemRequest
{
    public Guid OrderLineId { get; set; }
    public int Quantity { get; set; }
    public ReturnReason Reason { get; set; }
    public string? ReasonDetails { get; set; }
}

#endregion
