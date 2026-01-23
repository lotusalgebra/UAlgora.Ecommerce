namespace UAlgora.Ecommerce.Web.DocumentTypes.Models;

/// <summary>
/// Defines a property for a document type.
/// Immutable record for thread-safety and clean design.
/// </summary>
public sealed record PropertyDefinition
{
    /// <summary>
    /// Property alias (must be unique within the document type)
    /// </summary>
    public required string Alias { get; init; }

    /// <summary>
    /// Display name shown in the backoffice
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Description/help text for editors
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Data type to use for this property
    /// </summary>
    public required DataTypeReference DataType { get; init; }

    /// <summary>
    /// Whether this property is required
    /// </summary>
    public bool IsMandatory { get; init; }

    /// <summary>
    /// Sort order within the property group
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Validation regex pattern (optional)
    /// </summary>
    public string? ValidationRegex { get; init; }

    /// <summary>
    /// Validation error message (optional)
    /// </summary>
    public string? ValidationMessage { get; init; }
}

/// <summary>
/// Reference to a data type by well-known type or name
/// </summary>
public sealed record DataTypeReference
{
    private DataTypeReference() { }

    /// <summary>
    /// Well-known data type identifier
    /// </summary>
    public WellKnownDataType? WellKnownType { get; private init; }

    /// <summary>
    /// Custom data type name to search for
    /// </summary>
    public string? CustomTypeName { get; private init; }

    /// <summary>
    /// Fallback data type if primary is not found
    /// </summary>
    public DataTypeReference? Fallback { get; private init; }

    /// <summary>
    /// Creates a reference to a well-known Umbraco data type
    /// </summary>
    public static DataTypeReference WellKnown(WellKnownDataType type, DataTypeReference? fallback = null)
        => new() { WellKnownType = type, Fallback = fallback };

    /// <summary>
    /// Creates a reference to a custom data type by name
    /// </summary>
    public static DataTypeReference Custom(string name, DataTypeReference? fallback = null)
        => new() { CustomTypeName = name, Fallback = fallback };
}

/// <summary>
/// Well-known Umbraco data types
/// </summary>
public enum WellKnownDataType
{
    Textstring,
    Textarea,
    Numeric,
    Decimal,
    TrueFalse,
    DatePicker,
    DatePickerWithTime,
    MediaPicker,
    ContentPicker,
    MultipleMediaPicker,
    Tags,
    Dropdown,
    RadioButtonList,
    RichText,
    Label,
    ColorPicker,
    EmailAddress,
    UploadField
}
