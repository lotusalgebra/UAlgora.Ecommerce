using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using Umbraco.Cms.Api.Management.Routing;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// Management API controller for audit log operations in the Umbraco backoffice.
/// </summary>
[VersionedApiBackOfficeRoute($"{EcommerceConstants.ApiRouteBase}/{EcommerceConstants.Routes.Audit}")]
public class AuditManagementApiController : EcommerceManagementApiControllerBase
{
    private readonly IAuditService _auditService;

    public AuditManagementApiController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    /// <summary>
    /// Gets audit logs by store.
    /// </summary>
    [HttpGet("by-store")]
    [ProducesResponseType<IReadOnlyList<AuditLog>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByStore(
        [FromQuery] Guid? storeId = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100)
    {
        var logs = await _auditService.GetByStoreAsync(storeId, skip, take);
        return Ok(logs);
    }

    /// <summary>
    /// Gets audit logs for an entity.
    /// </summary>
    [HttpGet("by-entity/{entityType}/{entityId:guid}")]
    [ProducesResponseType<IReadOnlyList<AuditLog>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEntity(string entityType, Guid entityId)
    {
        var logs = await _auditService.GetByEntityAsync(entityType, entityId);
        return Ok(logs);
    }

    /// <summary>
    /// Gets audit logs by user.
    /// </summary>
    [HttpGet("by-user/{userId}")]
    [ProducesResponseType<IReadOnlyList<AuditLog>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByUser(
        string userId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100)
    {
        var logs = await _auditService.GetByUserAsync(userId, skip, take);
        return Ok(logs);
    }

    /// <summary>
    /// Gets audit logs by date range.
    /// </summary>
    [HttpGet("by-date-range")]
    [ProducesResponseType<IReadOnlyList<AuditLog>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100)
    {
        var logs = await _auditService.GetByDateRangeAsync(startDate, endDate, skip, take);
        return Ok(logs);
    }

    /// <summary>
    /// Searches audit logs.
    /// </summary>
    [HttpPost("search")]
    [ProducesResponseType<AuditSearchResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromBody] AuditSearchRequest request)
    {
        var criteria = new AuditSearchCriteria
        {
            StoreId = request.StoreId,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            UserId = request.UserId,
            Action = request.Action,
            Category = request.Category,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsSuccess = request.IsSuccess,
            Skip = request.Skip ?? 0,
            Take = request.Take ?? 100
        };

        var result = await _auditService.SearchAsync(criteria);
        return Ok(result);
    }

    /// <summary>
    /// Logs an audit entry.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Log([FromBody] LogAuditRequest request)
    {
        await _auditService.LogAsync(
            request.Action,
            request.Category,
            request.EntityType,
            request.EntityId,
            request.Description,
            request.OldValues,
            request.NewValues,
            request.StoreId);

        return StatusCode(StatusCodes.Status201Created, new { success = true });
    }

    /// <summary>
    /// Exports audit logs.
    /// </summary>
    [HttpPost("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Export([FromBody] ExportAuditRequest request)
    {
        var criteria = new AuditSearchCriteria
        {
            StoreId = request.StoreId,
            EntityType = request.EntityType,
            UserId = request.UserId,
            Action = request.Action,
            Category = request.Category,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        var format = request.Format ?? ExportFormat.Csv;
        var bytes = await _auditService.ExportAsync(criteria, format);

        var contentType = format switch
        {
            ExportFormat.Csv => "text/csv",
            ExportFormat.Json => "application/json",
            ExportFormat.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "application/octet-stream"
        };

        var extension = format switch
        {
            ExportFormat.Csv => "csv",
            ExportFormat.Json => "json",
            ExportFormat.Excel => "xlsx",
            _ => "bin"
        };

        return File(bytes, contentType, $"audit-logs-{DateTime.UtcNow:yyyyMMdd-HHmmss}.{extension}");
    }

    /// <summary>
    /// Purges old audit logs.
    /// </summary>
    [HttpPost("purge")]
    [ProducesResponseType<PurgeResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Purge([FromBody] PurgeAuditRequest request)
    {
        var cutoffDate = request.OlderThan ?? DateTime.UtcNow.AddDays(-90);
        var count = await _auditService.PurgeAsync(cutoffDate);
        return Ok(new PurgeResult { DeletedCount = count });
    }
}

#region Request/Response Models

public class AuditSearchRequest
{
    public Guid? StoreId { get; set; }
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? UserId { get; set; }
    public AuditAction? Action { get; set; }
    public AuditCategory? Category { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsSuccess { get; set; }
    public int? Skip { get; set; }
    public int? Take { get; set; }
}

public class LogAuditRequest
{
    public AuditAction Action { get; set; }
    public AuditCategory Category { get; set; }
    public required string EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? Description { get; set; }
    public object? OldValues { get; set; }
    public object? NewValues { get; set; }
    public Guid? StoreId { get; set; }
}

public class ExportAuditRequest
{
    public Guid? StoreId { get; set; }
    public string? EntityType { get; set; }
    public string? UserId { get; set; }
    public AuditAction? Action { get; set; }
    public AuditCategory? Category { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ExportFormat? Format { get; set; }
}

public class PurgeAuditRequest
{
    public DateTime? OlderThan { get; set; }
}

public class PurgeResult
{
    public int DeletedCount { get; set; }
}

#endregion
