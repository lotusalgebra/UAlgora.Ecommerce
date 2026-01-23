using Microsoft.Extensions.Logging;
using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace UAlgora.Ecommerce.Web.DocumentTypes.Services;

/// <summary>
/// Installs and updates document types based on definitions.
/// Follows Single Responsibility Principle - handles only document type installation.
/// Follows Dependency Inversion Principle - depends on abstractions.
/// </summary>
public sealed class DocumentTypeInstaller : IDocumentTypeInstaller
{
    private readonly IContentTypeService _contentTypeService;
    private readonly ITemplateService _templateService;
    private readonly IFileService _fileService;
    private readonly IDataTypeResolver _dataTypeResolver;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IEnumerable<IDocumentTypeDefinitionProvider> _providers;
    private readonly ILogger<DocumentTypeInstaller> _logger;

    public DocumentTypeInstaller(
        IContentTypeService contentTypeService,
        ITemplateService templateService,
        IFileService fileService,
        IDataTypeResolver dataTypeResolver,
        IShortStringHelper shortStringHelper,
        IEnumerable<IDocumentTypeDefinitionProvider> providers,
        ILogger<DocumentTypeInstaller> logger)
    {
        _contentTypeService = contentTypeService;
        _templateService = templateService;
        _fileService = fileService;
        _dataTypeResolver = dataTypeResolver;
        _shortStringHelper = shortStringHelper;
        _providers = providers;
        _logger = logger;
    }

    public IReadOnlyList<DocumentTypeInstallResult> InstallAll()
    {
        _logger.LogInformation("Algora Commerce: Starting document type installation...");

        var results = new List<DocumentTypeInstallResult>();
        var orderedProviders = _providers.OrderBy(p => p.Priority).ToList();

        foreach (var provider in orderedProviders)
        {
            try
            {
                var definition = provider.GetDefinition();
                var result = Install(definition);
                results.Add(result);

                LogResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting definition from provider {ProviderType}", provider.GetType().Name);
            }
        }

        // After all document types are created, update allowed child types
        UpdateAllowedChildTypes(orderedProviders);

        _logger.LogInformation("Algora Commerce: Document type installation complete. " +
            "Created: {Created}, Updated: {Updated}, Skipped: {Skipped}, Failed: {Failed}",
            results.Count(r => r.Action == DocumentTypeInstallAction.Created),
            results.Count(r => r.Action == DocumentTypeInstallAction.Updated),
            results.Count(r => r.Action == DocumentTypeInstallAction.Skipped),
            results.Count(r => r.Action == DocumentTypeInstallAction.Failed));

        return results;
    }

    public DocumentTypeInstallResult Install(DocumentTypeDefinition definition)
    {
        try
        {
            var existing = _contentTypeService.Get(definition.Alias);

            if (existing != null)
            {
                return UpdateExisting(existing, definition);
            }

            return CreateNew(definition);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error installing document type {Alias}", definition.Alias);
            return DocumentTypeInstallResult.Failed(definition.Alias, definition.Name, ex.Message);
        }
    }

    private DocumentTypeInstallResult CreateNew(DocumentTypeDefinition definition)
    {
        _logger.LogInformation("Creating document type: {Name} ({Alias})", definition.Name, definition.Alias);

        var contentType = new ContentType(_shortStringHelper, -1)
        {
            Alias = definition.Alias,
            Name = definition.Name,
            Description = definition.Description,
            Icon = definition.FullIcon,
            AllowedAsRoot = definition.AllowedAsRoot,
            IsElement = definition.IsElement,
            Variations = ContentVariation.Nothing
        };

        // Add property groups and properties
        foreach (var groupDef in definition.PropertyGroups)
        {
            var group = CreatePropertyGroup(groupDef);
            contentType.PropertyGroups.Add(group);
        }

        // Handle default template
        if (!string.IsNullOrEmpty(definition.DefaultTemplate))
        {
            var template = EnsureTemplate(definition.DefaultTemplate, definition.Name);
            if (template != null)
            {
                contentType.AllowedTemplates = new[] { template };
                contentType.SetDefaultTemplate(template);
            }
        }

        _contentTypeService.Save(contentType);

        return DocumentTypeInstallResult.Created(definition.Alias, definition.Name);
    }

    /// <summary>
    /// Ensures a template exists (creates if not) and returns it.
    /// </summary>
    private ITemplate? EnsureTemplate(string alias, string displayName)
    {
        try
        {
            _logger.LogDebug("EnsureTemplate: Looking for template {Alias}", alias);

            // Check if template already exists by alias
            var existing = _fileService.GetTemplate(alias);
            if (existing != null)
            {
                _logger.LogDebug("Template already exists: {Alias} (Id: {Id})", alias, existing.Id);
                return existing;
            }

            _logger.LogInformation("Creating new template: {Alias} ({Name})", alias, displayName);

            // Create new template with proper content
            // The content is a minimal Razor view that inherits from the layout
            var template = new Template(_shortStringHelper, displayName, alias)
            {
                Content = $"@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n@{{\r\n    Layout = \"_Layout.cshtml\";\r\n}}\r\n"
            };

            // Save the template - this creates both the DB entry and view file
            _fileService.SaveTemplate(template);

            // The template object itself has the Id after save, use it directly
            if (template.Id > 0)
            {
                _logger.LogInformation("Successfully created template: {Alias} (Id: {Id})", alias, template.Id);
                return template;
            }

            // Fallback: try to retrieve it
            var created = _fileService.GetTemplate(alias);
            if (created != null)
            {
                _logger.LogInformation("Retrieved template after save: {Alias} (Id: {Id})", alias, created.Id);
                return created;
            }

            _logger.LogWarning("Template {Alias} could not be created or retrieved", alias);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring template {Alias}: {Message}", alias, ex.Message);
            return null;
        }
    }

    private DocumentTypeInstallResult UpdateExisting(IContentType existing, DocumentTypeDefinition definition)
    {
        _logger.LogInformation("Checking document type structure: {Name} ({Alias})", definition.Name, definition.Alias);

        // Check if property groups match (aliases)
        var existingGroupAliases = existing.PropertyGroups.Select(g => g.Alias).OrderBy(a => a).ToList();
        var definitionGroupAliases = definition.PropertyGroups.Select(g => g.Alias).OrderBy(a => a).ToList();

        var groupsMatch = existingGroupAliases.SequenceEqual(definitionGroupAliases);

        // Check if any existing groups are using Group type instead of Tab type
        var hasGroupTypeInsteadOfTab = existing.PropertyGroups.Any(g => g.Type == PropertyGroupType.Group);

        if (!groupsMatch || hasGroupTypeInsteadOfTab)
        {
            var reason = !groupsMatch
                ? $"different property groups. Existing: [{string.Join(", ", existingGroupAliases)}], Expected: [{string.Join(", ", definitionGroupAliases)}]"
                : "property groups using Group type instead of Tab type";

            _logger.LogInformation("Document type {Name} has {Reason}. Deleting and recreating...",
                definition.Name,
                reason);

            // Delete the existing document type
            _contentTypeService.Delete(existing);

            // Create new one
            return CreateNew(definition);
        }

        // Groups match, just update properties if needed
        var updatedProperties = new List<string>();
        var needsSave = false;

        // Update basic properties
        if (existing.Name != definition.Name || existing.Description != definition.Description || existing.Icon != definition.FullIcon)
        {
            existing.Name = definition.Name;
            existing.Description = definition.Description;
            existing.Icon = definition.FullIcon;
            needsSave = true;
        }

        // Update AllowedAsRoot if changed
        if (existing.AllowedAsRoot != definition.AllowedAsRoot)
        {
            existing.AllowedAsRoot = definition.AllowedAsRoot;
            needsSave = true;
            _logger.LogInformation("Updated AllowedAsRoot for {Name} to {Value}", definition.Name, definition.AllowedAsRoot);
        }

        // Update property data types within existing groups
        foreach (var groupDef in definition.PropertyGroups)
        {
            var existingGroup = existing.PropertyGroups.FirstOrDefault(g => g.Alias == groupDef.Alias);
            if (existingGroup == null) continue;

            foreach (var propDef in groupDef.Properties)
            {
                var existingProp = existingGroup.PropertyTypes?.FirstOrDefault(p => p.Alias == propDef.Alias);
                if (existingProp != null)
                {
                    var resolvedDataType = _dataTypeResolver.Resolve(propDef.DataType);
                    if (resolvedDataType != null && existingProp.DataTypeId != resolvedDataType.Id)
                    {
                        existingProp.DataTypeId = resolvedDataType.Id;
                        existingProp.DataTypeKey = resolvedDataType.Key;
                        updatedProperties.Add(propDef.Alias);
                        needsSave = true;
                    }
                }
            }
        }

        // Update default template if needed
        if (!string.IsNullOrEmpty(definition.DefaultTemplate))
        {
            _logger.LogDebug("Checking template {Template} for {Name}", definition.DefaultTemplate, definition.Name);
            var template = EnsureTemplate(definition.DefaultTemplate, definition.Name);

            if (template == null)
            {
                _logger.LogWarning("Template {Template} could not be created/found for {Name}", definition.DefaultTemplate, definition.Name);
            }
            else
            {
                var currentTemplateAlias = existing.DefaultTemplate?.Alias;
                _logger.LogDebug("Current template: {Current}, Desired: {Desired}", currentTemplateAlias ?? "none", template.Alias);

                // Always ensure the template is linked (even if already set - handles edge cases)
                var hasTemplate = existing.AllowedTemplates.Any(t => t.Alias == template.Alias);
                if (!hasTemplate || currentTemplateAlias != template.Alias)
                {
                    if (!hasTemplate)
                    {
                        existing.AllowedTemplates = existing.AllowedTemplates.Concat(new[] { template }).ToArray();
                    }
                    existing.SetDefaultTemplate(template);
                    needsSave = true;
                    _logger.LogInformation("Set default template {Template} for {Name}", template.Alias, definition.Name);
                }
            }
        }

        if (needsSave)
        {
            _contentTypeService.Save(existing);
            return DocumentTypeInstallResult.Updated(definition.Alias, definition.Name, updatedProperties);
        }

        return DocumentTypeInstallResult.Skipped(definition.Alias, definition.Name);
    }

    private PropertyGroup CreatePropertyGroup(PropertyGroupDefinition definition)
    {
        var group = new PropertyGroup(new PropertyTypeCollection(true))
        {
            Alias = definition.Alias,
            Name = definition.Name,
            SortOrder = definition.SortOrder,
            Type = PropertyGroupType.Tab
        };

        foreach (var propDef in definition.Properties)
        {
            var propertyType = CreatePropertyType(propDef);
            if (propertyType != null)
            {
                group.PropertyTypes!.Add(propertyType);
            }
        }

        return group;
    }

    private PropertyType? CreatePropertyType(PropertyDefinition definition)
    {
        var dataType = _dataTypeResolver.Resolve(definition.DataType);
        if (dataType == null)
        {
            _logger.LogWarning("Could not resolve data type for property {Alias}, using default", definition.Alias);
            dataType = _dataTypeResolver.GetDefaultDataType();
        }

        var propertyType = new PropertyType(_shortStringHelper, dataType, definition.Alias)
        {
            Name = definition.Name,
            Description = definition.Description,
            Mandatory = definition.IsMandatory,
            SortOrder = definition.SortOrder,
            Variations = ContentVariation.Nothing
        };

        if (!string.IsNullOrEmpty(definition.ValidationRegex))
        {
            propertyType.ValidationRegExp = definition.ValidationRegex;
            propertyType.ValidationRegExpMessage = definition.ValidationMessage;
        }

        return propertyType;
    }

    private void UpdateAllowedChildTypes(IEnumerable<IDocumentTypeDefinitionProvider> providers)
    {
        foreach (var provider in providers)
        {
            var definition = provider.GetDefinition();
            if (definition.AllowedChildTypes.Count == 0) continue;

            var contentType = _contentTypeService.Get(definition.Alias);
            if (contentType == null) continue;

            var allowedTypes = new List<ContentTypeSort>();
            var sortOrder = 0;

            foreach (var childAlias in definition.AllowedChildTypes)
            {
                var childType = _contentTypeService.Get(childAlias);
                if (childType != null)
                {
                    allowedTypes.Add(new ContentTypeSort(childType.Key, sortOrder++, childAlias));
                }
            }

            if (allowedTypes.Count > 0)
            {
                contentType.AllowedContentTypes = allowedTypes;
                _contentTypeService.Save(contentType);
                _logger.LogDebug("Updated allowed child types for {Alias}", definition.Alias);
            }
        }
    }

    private void LogResult(DocumentTypeInstallResult result)
    {
        var message = result.Action switch
        {
            DocumentTypeInstallAction.Created => $"Created document type: {result.Name}",
            DocumentTypeInstallAction.Updated => $"Updated document type: {result.Name} (Properties: {string.Join(", ", result.UpdatedProperties)})",
            DocumentTypeInstallAction.Skipped => $"Skipped document type (no changes): {result.Name}",
            DocumentTypeInstallAction.Failed => $"Failed to install document type: {result.Name} - {result.ErrorMessage}",
            _ => $"Unknown action for document type: {result.Name}"
        };

        if (result.Success)
        {
            _logger.LogInformation("Algora Commerce: {Message}", message);
        }
        else
        {
            _logger.LogError("Algora Commerce: {Message}", message);
        }
    }
}
