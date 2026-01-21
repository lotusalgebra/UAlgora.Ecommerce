using Microsoft.AspNetCore.Mvc;

namespace UAlgora.Ecommerce.Web.Controllers.Api;

/// <summary>
/// Base controller for all e-commerce API endpoints.
/// Uses standard ASP.NET Core ControllerBase (recommended for Umbraco 15+).
/// </summary>
[ApiController]
[Route("api/ecommerce/[controller]")]
[Produces("application/json")]
public abstract class EcommerceApiController : ControllerBase
{
    /// <summary>
    /// Returns a standardized error response.
    /// </summary>
    protected IActionResult ApiError(string message, int statusCode = 400)
    {
        return StatusCode(statusCode, new ApiErrorResponse
        {
            Success = false,
            Message = message
        });
    }

    /// <summary>
    /// Returns a standardized success response.
    /// </summary>
    protected IActionResult ApiSuccess<T>(T data, string? message = null)
    {
        return Ok(new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        });
    }

    /// <summary>
    /// Returns a standardized paged response.
    /// </summary>
    protected IActionResult ApiPaged<T>(IEnumerable<T> items, int totalCount, int page, int pageSize)
    {
        return Ok(new ApiPagedResponse<T>
        {
            Success = true,
            Data = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }
}

/// <summary>
/// Standard API response wrapper.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}

/// <summary>
/// Standard API error response.
/// </summary>
public class ApiErrorResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}

/// <summary>
/// Standard API paged response.
/// </summary>
public class ApiPagedResponse<T>
{
    public bool Success { get; set; }
    public IEnumerable<T>? Data { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
