using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using Umbraco.Cms.Core.Models;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;

/// <summary>
/// Resolves data types from references.
/// Follows Single Responsibility Principle - only handles data type resolution.
/// </summary>
public interface IDataTypeResolver
{
    /// <summary>
    /// Resolves a data type reference to an actual data type
    /// </summary>
    /// <param name="reference">The data type reference</param>
    /// <returns>The resolved data type, or null if not found</returns>
    IDataType? Resolve(DataTypeReference reference);

    /// <summary>
    /// Resolves a data type reference to its ID
    /// </summary>
    /// <param name="reference">The data type reference</param>
    /// <returns>The data type ID, or null if not found</returns>
    int? ResolveId(DataTypeReference reference);

    /// <summary>
    /// Gets the default fallback data type (typically Textstring)
    /// </summary>
    IDataType GetDefaultDataType();
}
