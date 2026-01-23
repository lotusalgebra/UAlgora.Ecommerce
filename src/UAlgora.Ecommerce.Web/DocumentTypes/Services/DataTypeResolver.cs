using Microsoft.Extensions.Logging;
using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Services;

/// <summary>
/// Resolves data type references to actual Umbraco data types.
/// Implements caching for performance optimization.
/// </summary>
public sealed class DataTypeResolver : IDataTypeResolver
{
    private readonly IDataTypeService _dataTypeService;
    private readonly ILogger<DataTypeResolver> _logger;

    // Cache for resolved data types
    private readonly Dictionary<WellKnownDataType, IDataType?> _wellKnownCache = new();
    private readonly Dictionary<string, IDataType?> _customCache = new(StringComparer.OrdinalIgnoreCase);
    private IDataType? _defaultDataType;
    private IEnumerable<IDataType>? _allDataTypes;

    // Well-known data type definitions with search patterns
    // Note: Some GUIDs may not be available in all Umbraco versions, so we use name-based fallbacks
    private static readonly Dictionary<WellKnownDataType, DataTypeSearchPattern> WellKnownPatterns = new()
    {
        [WellKnownDataType.Textstring] = new("Textstring", Constants.DataTypes.Guids.TextstringGuid),
        [WellKnownDataType.Textarea] = new("Textarea", Constants.DataTypes.Guids.TextareaGuid),
        [WellKnownDataType.Numeric] = new("Numeric", null, ["Numeric", "Integer", "Number"]),
        [WellKnownDataType.Decimal] = new("Decimal", null, ["Decimal", "Numeric"]),
        [WellKnownDataType.TrueFalse] = new("True/False", Constants.DataTypes.Guids.CheckboxGuid, ["True/False", "Checkbox", "Boolean"]),
        [WellKnownDataType.DatePicker] = new("Date Picker", Constants.DataTypes.Guids.DatePickerGuid),
        [WellKnownDataType.DatePickerWithTime] = new("Date Picker with time", Constants.DataTypes.Guids.DatePickerWithTimeGuid),
        [WellKnownDataType.MediaPicker] = new("Media Picker", null, ["Media Picker", "Image Media Picker", "Image Cropper"]),
        [WellKnownDataType.ContentPicker] = new("Content Picker", Constants.DataTypes.Guids.ContentPickerGuid),
        [WellKnownDataType.MultipleMediaPicker] = new("Multiple Media Picker", null, ["Multiple Media Picker", "Media Picker (multiple)"]),
        [WellKnownDataType.Tags] = new("Tags", Constants.DataTypes.Guids.TagsGuid),
        [WellKnownDataType.Dropdown] = new("Dropdown", Constants.DataTypes.Guids.DropdownGuid),
        [WellKnownDataType.RadioButtonList] = new("Radiobox", Constants.DataTypes.Guids.RadioboxGuid),
        [WellKnownDataType.RichText] = new("Richtext editor", Constants.DataTypes.Guids.RichtextEditorGuid, ["Richtext", "Rich Text", "TinyMCE"]),
        [WellKnownDataType.Label] = new("Label (string)", Constants.DataTypes.Guids.LabelStringGuid, ["Label"]),
        [WellKnownDataType.ColorPicker] = new("Color Picker", null, ["Color Picker", "Colour Picker"]),
        [WellKnownDataType.EmailAddress] = new("Email address", null, ["Email"]),
        [WellKnownDataType.UploadField] = new("Upload File", Constants.DataTypes.Guids.UploadGuid, ["Upload", "File Upload"])
    };

    public DataTypeResolver(IDataTypeService dataTypeService, ILogger<DataTypeResolver> logger)
    {
        _dataTypeService = dataTypeService;
        _logger = logger;
    }

    public IDataType? Resolve(DataTypeReference reference)
    {
        // Try well-known type first
        if (reference.WellKnownType.HasValue)
        {
            var dataType = ResolveWellKnown(reference.WellKnownType.Value);
            if (dataType != null) return dataType;
        }

        // Try custom type name
        if (!string.IsNullOrEmpty(reference.CustomTypeName))
        {
            var dataType = ResolveCustom(reference.CustomTypeName);
            if (dataType != null) return dataType;
        }

        // Try fallback
        if (reference.Fallback != null)
        {
            return Resolve(reference.Fallback);
        }

        _logger.LogWarning("Could not resolve data type reference: {Reference}", reference);
        return null;
    }

    public int? ResolveId(DataTypeReference reference)
    {
        return Resolve(reference)?.Id;
    }

    public IDataType GetDefaultDataType()
    {
        if (_defaultDataType != null) return _defaultDataType;

        _defaultDataType = ResolveWellKnown(WellKnownDataType.Textstring);

        if (_defaultDataType == null)
        {
            // Last resort - get any data type
            _defaultDataType = GetAllDataTypes().FirstOrDefault();
            if (_defaultDataType == null)
            {
                throw new InvalidOperationException("No data types found in Umbraco. Cannot install document types.");
            }
        }

        return _defaultDataType;
    }

    private IDataType? ResolveWellKnown(WellKnownDataType wellKnownType)
    {
        if (_wellKnownCache.TryGetValue(wellKnownType, out var cached))
        {
            return cached;
        }

        if (!WellKnownPatterns.TryGetValue(wellKnownType, out var pattern))
        {
            _logger.LogWarning("No search pattern defined for well-known data type: {Type}", wellKnownType);
            _wellKnownCache[wellKnownType] = null;
            return null;
        }

        IDataType? dataType = null;

        // Try by GUID first (most reliable)
        if (pattern.Guid.HasValue)
        {
            dataType = GetAllDataTypes().FirstOrDefault(dt => dt.Key == pattern.Guid.Value);
        }

        // Try by exact name
        if (dataType == null)
        {
            dataType = GetAllDataTypes().FirstOrDefault(dt =>
                dt.Name?.Equals(pattern.PrimaryName, StringComparison.OrdinalIgnoreCase) == true);
        }

        // Try alternative names
        if (dataType == null && pattern.AlternativeNames.Count > 0)
        {
            foreach (var altName in pattern.AlternativeNames)
            {
                dataType = GetAllDataTypes().FirstOrDefault(dt =>
                    dt.Name?.Contains(altName, StringComparison.OrdinalIgnoreCase) == true);

                if (dataType != null) break;
            }
        }

        _wellKnownCache[wellKnownType] = dataType;

        if (dataType != null)
        {
            _logger.LogDebug("Resolved well-known data type {Type} to '{Name}' (ID: {Id})",
                wellKnownType, dataType.Name, dataType.Id);
        }
        else
        {
            _logger.LogWarning("Could not resolve well-known data type: {Type}", wellKnownType);
        }

        return dataType;
    }

    private IDataType? ResolveCustom(string typeName)
    {
        if (_customCache.TryGetValue(typeName, out var cached))
        {
            return cached;
        }

        var dataType = GetAllDataTypes().FirstOrDefault(dt =>
            dt.Name?.Equals(typeName, StringComparison.OrdinalIgnoreCase) == true);

        if (dataType == null)
        {
            dataType = GetAllDataTypes().FirstOrDefault(dt =>
                dt.Name?.Contains(typeName, StringComparison.OrdinalIgnoreCase) == true);
        }

        _customCache[typeName] = dataType;

        if (dataType != null)
        {
            _logger.LogDebug("Resolved custom data type '{TypeName}' to '{Name}' (ID: {Id})",
                typeName, dataType.Name, dataType.Id);
        }
        else
        {
            _logger.LogWarning("Could not resolve custom data type: {TypeName}", typeName);
        }

        return dataType;
    }

    private IEnumerable<IDataType> GetAllDataTypes()
    {
        return _allDataTypes ??= _dataTypeService.GetAll().ToList();
    }

    /// <summary>
    /// Search pattern for finding data types
    /// </summary>
    private sealed record DataTypeSearchPattern(
        string PrimaryName,
        Guid? Guid,
        IReadOnlyList<string>? AlternativeNames = null)
    {
        public IReadOnlyList<string> AlternativeNames { get; } = AlternativeNames ?? [];
    }
}
