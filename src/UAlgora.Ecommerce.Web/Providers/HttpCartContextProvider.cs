using Microsoft.AspNetCore.Http;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace UAlgora.Ecommerce.Web.Providers;

/// <summary>
/// HTTP-based implementation of ICartContextProvider.
/// Provides session and customer context from HTTP context and Umbraco membership.
/// </summary>
public class HttpCartContextProvider : ICartContextProvider
{
    private const string SessionIdCookieName = "UAlgora.CartSession";
    private const string CustomerIdClaimType = "ecommerce_customer_id";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMemberManager _memberManager;
    private readonly IMemberService _memberService;

    public HttpCartContextProvider(
        IHttpContextAccessor httpContextAccessor,
        IMemberManager memberManager,
        IMemberService memberService)
    {
        _httpContextAccessor = httpContextAccessor;
        _memberManager = memberManager;
        _memberService = memberService;
    }

    /// <summary>
    /// Gets the current session ID from cookie or creates a new one.
    /// </summary>
    public string? GetSessionId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return null;
        }

        // Try to get existing session ID from cookie
        if (httpContext.Request.Cookies.TryGetValue(SessionIdCookieName, out var sessionId)
            && !string.IsNullOrEmpty(sessionId))
        {
            return sessionId;
        }

        // Generate new session ID and set cookie
        sessionId = Guid.NewGuid().ToString("N");

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = httpContext.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            IsEssential = true // Required for GDPR - cart functionality is essential
        };

        httpContext.Response.Cookies.Append(SessionIdCookieName, sessionId, cookieOptions);

        return sessionId;
    }

    /// <summary>
    /// Gets the current customer ID from the authenticated Umbraco member.
    /// </summary>
    public Guid? GetCustomerId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        // First, try to get customer ID from claims (if we've cached it)
        var customerIdClaim = httpContext.User.FindFirst(CustomerIdClaimType);
        if (customerIdClaim != null && Guid.TryParse(customerIdClaim.Value, out var cachedCustomerId))
        {
            return cachedCustomerId;
        }

        // Get the current member
        var member = _memberManager.GetCurrentMemberAsync().GetAwaiter().GetResult();
        if (member == null)
        {
            return null;
        }

        // Try to get ecommerce customer ID from member properties
        var umbracoMember = _memberService.GetByKey(member.Key);
        if (umbracoMember == null)
        {
            return null;
        }

        // Check for ecommerceCustomerId property on member
        var customerIdProperty = umbracoMember.GetValue<string>("ecommerceCustomerId");
        if (!string.IsNullOrEmpty(customerIdProperty) && Guid.TryParse(customerIdProperty, out var customerId))
        {
            return customerId;
        }

        // Fallback: Use member key as customer ID
        // This allows immediate cart association for new members
        return umbracoMember.Key;
    }

    /// <summary>
    /// Whether the current user is authenticated.
    /// </summary>
    public bool IsAuthenticated
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.User?.Identity?.IsAuthenticated == true;
        }
    }
}
