namespace UAlgora.Ecommerce.Web.DocumentTypes.Models;

/// <summary>
/// Defines a property group (tab) for a document type.
/// Immutable record for thread-safety and clean design.
/// </summary>
public sealed record PropertyGroupDefinition
{
    /// <summary>
    /// Group alias (must be unique within the document type)
    /// </summary>
    public required string Alias { get; init; }

    /// <summary>
    /// Display name shown as tab header
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Sort order for tab ordering
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Properties within this group
    /// </summary>
    public IReadOnlyList<PropertyDefinition> Properties { get; init; } = [];
}
