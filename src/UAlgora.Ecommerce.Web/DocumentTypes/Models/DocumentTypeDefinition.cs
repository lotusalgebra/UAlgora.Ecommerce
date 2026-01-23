namespace UAlgora.Ecommerce.Web.DocumentTypes.Models;

/// <summary>
/// Defines a complete document type configuration.
/// Immutable record for thread-safety and clean design.
/// </summary>
public sealed record DocumentTypeDefinition
{
    /// <summary>
    /// Document type alias (must be unique)
    /// </summary>
    public required string Alias { get; init; }

    /// <summary>
    /// Display name
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Description for administrators
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Icon class (e.g., "icon-box")
    /// </summary>
    public string Icon { get; init; } = "icon-document";

    /// <summary>
    /// Icon color suffix (e.g., "color-green")
    /// </summary>
    public string? IconColor { get; init; }

    /// <summary>
    /// Whether this document type can be created at the root
    /// </summary>
    public bool AllowedAsRoot { get; init; }

    /// <summary>
    /// Whether this document type is an element type (for block editors)
    /// </summary>
    public bool IsElement { get; init; }

    /// <summary>
    /// Property groups (tabs) with their properties
    /// </summary>
    public IReadOnlyList<PropertyGroupDefinition> PropertyGroups { get; init; } = [];

    /// <summary>
    /// Aliases of document types allowed as children
    /// </summary>
    public IReadOnlyList<string> AllowedChildTypes { get; init; } = [];

    /// <summary>
    /// Alias of composition document types to inherit from
    /// </summary>
    public IReadOnlyList<string> Compositions { get; init; } = [];

    /// <summary>
    /// Default template alias for this document type (if any).
    /// The template must exist in Views folder with matching name.
    /// </summary>
    public string? DefaultTemplate { get; init; }

    /// <summary>
    /// Gets the full icon string including color
    /// </summary>
    public string FullIcon => string.IsNullOrEmpty(IconColor) ? Icon : $"{Icon} {IconColor}";
}
