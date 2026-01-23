using UAlgora.Ecommerce.Web.DocumentTypes.Models;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;

/// <summary>
/// Provides document type definitions.
/// Follows Interface Segregation Principle - each provider handles one document type.
/// Follows Open/Closed Principle - new document types can be added via new providers.
/// </summary>
public interface IDocumentTypeDefinitionProvider
{
    /// <summary>
    /// Gets the document type definition
    /// </summary>
    DocumentTypeDefinition GetDefinition();

    /// <summary>
    /// Gets the priority for installation order (lower = earlier)
    /// </summary>
    int Priority => 100;
}
