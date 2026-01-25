namespace UAlgora.Ecommerce.Core.Models.Domain;

/// <summary>
/// Represents a configurable step in the checkout flow.
/// </summary>
public class CheckoutStepConfiguration : BaseEntity
{
    /// <summary>
    /// Unique code for this step (e.g., "information", "shipping", "payment", "review").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name for this step.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Title shown to customer during checkout.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Brief description of this step.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Instructions or help text for the customer.
    /// </summary>
    public string? Instructions { get; set; }

    /// <summary>
    /// Icon class for this step (e.g., "icon-user", "icon-truck", "icon-credit-card").
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Order of this step in the checkout flow (1, 2, 3...).
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Whether this step is required to complete checkout.
    /// </summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Whether this step is enabled in the checkout flow.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Whether to show the order summary on this step.
    /// </summary>
    public bool ShowOrderSummary { get; set; } = true;

    /// <summary>
    /// Whether to allow going back to previous steps.
    /// </summary>
    public bool AllowBackNavigation { get; set; } = true;

    /// <summary>
    /// Custom CSS class for styling this step.
    /// </summary>
    public string? CssClass { get; set; }

    /// <summary>
    /// Custom validation rules (JSON format).
    /// </summary>
    public string? ValidationRules { get; set; }

    /// <summary>
    /// Additional configuration as JSON.
    /// </summary>
    public string? Configuration { get; set; }

    /// <summary>
    /// Store ID if checkout steps are store-specific.
    /// </summary>
    public Guid? StoreId { get; set; }
}
