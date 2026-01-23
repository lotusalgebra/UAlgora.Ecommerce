using UAlgora.Ecommerce.Web.DocumentTypes.Models;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;

/// <summary>
/// Installs document types in Umbraco.
/// Follows Single Responsibility Principle - only handles document type installation.
/// Follows Dependency Inversion Principle - depends on abstractions (IDataTypeResolver).
/// </summary>
public interface IDocumentTypeInstaller
{
    /// <summary>
    /// Installs or updates a document type from its definition
    /// </summary>
    /// <param name="definition">The document type definition</param>
    /// <returns>Result of the installation</returns>
    DocumentTypeInstallResult Install(DocumentTypeDefinition definition);

    /// <summary>
    /// Installs all document types from registered providers
    /// </summary>
    /// <returns>Results of all installations</returns>
    IReadOnlyList<DocumentTypeInstallResult> InstallAll();
}

/// <summary>
/// Result of a document type installation operation
/// </summary>
public sealed record DocumentTypeInstallResult
{
    public required string Alias { get; init; }
    public required string Name { get; init; }
    public required DocumentTypeInstallAction Action { get; init; }
    public bool Success { get; init; } = true;
    public string? ErrorMessage { get; init; }
    public IReadOnlyList<string> UpdatedProperties { get; init; } = [];

    public static DocumentTypeInstallResult Created(string alias, string name)
        => new() { Alias = alias, Name = name, Action = DocumentTypeInstallAction.Created };

    public static DocumentTypeInstallResult Updated(string alias, string name, IReadOnlyList<string>? updatedProperties = null)
        => new() { Alias = alias, Name = name, Action = DocumentTypeInstallAction.Updated, UpdatedProperties = updatedProperties ?? [] };

    public static DocumentTypeInstallResult Skipped(string alias, string name)
        => new() { Alias = alias, Name = name, Action = DocumentTypeInstallAction.Skipped };

    public static DocumentTypeInstallResult Failed(string alias, string name, string error)
        => new() { Alias = alias, Name = name, Action = DocumentTypeInstallAction.Failed, Success = false, ErrorMessage = error };
}

/// <summary>
/// Action taken during document type installation
/// </summary>
public enum DocumentTypeInstallAction
{
    Created,
    Updated,
    Skipped,
    Failed
}
