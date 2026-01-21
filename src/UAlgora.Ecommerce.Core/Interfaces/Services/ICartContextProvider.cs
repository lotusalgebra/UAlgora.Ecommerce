namespace UAlgora.Ecommerce.Core.Interfaces.Services;

/// <summary>
/// Provides context for cart operations (session ID, customer ID).
/// </summary>
public interface ICartContextProvider
{
    /// <summary>
    /// Gets the current session ID.
    /// </summary>
    string? GetSessionId();

    /// <summary>
    /// Gets the current customer ID.
    /// </summary>
    Guid? GetCustomerId();

    /// <summary>
    /// Whether the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }
}
