using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Web.DocumentTypes.Abstractions;
using UAlgora.Ecommerce.Web.DocumentTypes.Providers;
using UAlgora.Ecommerce.Web.Services;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Controllers;

namespace UAlgora.Ecommerce.Web.Controllers.Api;

/// <summary>
/// API controller for seeding catalog content.
/// Use for testing and demo purposes.
/// </summary>
[Route("api/algora/[controller]")]
[ApiController]
public class CatalogSeederController : UmbracoApiController
{
    private readonly CatalogContentSeeder _seeder;
    private readonly EnterpriseContentSeeder _enterpriseSeeder;
    private readonly IDocumentTypeInstaller _documentTypeInstaller;
    private readonly IContentTypeService _contentTypeService;
    private readonly IContentService _contentService;

    public CatalogSeederController(
        CatalogContentSeeder seeder,
        EnterpriseContentSeeder enterpriseSeeder,
        IDocumentTypeInstaller documentTypeInstaller,
        IContentTypeService contentTypeService,
        IContentService contentService)
    {
        _seeder = seeder;
        _enterpriseSeeder = enterpriseSeeder;
        _documentTypeInstaller = documentTypeInstaller;
        _contentTypeService = contentTypeService;
        _contentService = contentService;
    }

    /// <summary>
    /// Seeds sample catalog with categories and products
    /// GET /api/algora/catalogseeder/seed
    /// </summary>
    [HttpGet("seed")]
    public IActionResult SeedCatalog()
    {
        var result = _seeder.SeedSampleCatalog();

        if (result.Success)
        {
            return Ok(new
            {
                success = true,
                created = result.Created,
                messages = result.Messages
            });
        }

        return BadRequest(new
        {
            success = false,
            errors = result.Errors
        });
    }

    /// <summary>
    /// Seeds complete enterprise content structure with all categories, products, and CMS pages.
    /// Creates 300+ products across 30+ categories with proper hierarchy.
    /// GET /api/algora/catalogseeder/seed-enterprise
    /// </summary>
    [HttpGet("seed-enterprise")]
    public IActionResult SeedEnterpriseContent([FromQuery] bool clearExisting = false)
    {
        try
        {
            var result = _enterpriseSeeder.SeedEnterpriseContent(clearExisting);

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    created = result.Created,
                    messages = result.Messages,
                    summary = $"Created {result.Created} content items including categories, products, and CMS pages."
                });
            }

            return BadRequest(new
            {
                success = false,
                errors = result.Errors,
                messages = result.Messages
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// Reinstalls/updates document types
    /// GET /api/algora/catalogseeder/install-doctypes
    /// </summary>
    [HttpGet("install-doctypes")]
    public IActionResult InstallDocumentTypes()
    {
        try
        {
            var results = _documentTypeInstaller.InstallAll();
            return Ok(new
            {
                success = true,
                results = results.Select(r => new
                {
                    alias = r.Alias,
                    name = r.Name,
                    action = r.Action.ToString(),
                    updatedProperties = r.UpdatedProperties,
                    success = r.Success,
                    error = r.ErrorMessage
                })
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Force reinstalls document types by deleting them first.
    /// WARNING: This will delete all content using these document types!
    /// GET /api/algora/catalogseeder/force-reinstall
    /// </summary>
    [HttpGet("force-reinstall")]
    public IActionResult ForceReinstallDocumentTypes()
    {
        try
        {
            var messages = new List<string>();

            // Delete content first (required before deleting document types)
            var catalogContent = _contentService.GetRootContent()
                .Where(c => c.ContentType.Alias == AlgoraDocumentTypeConstants.CatalogAlias)
                .ToList();

            foreach (var content in catalogContent)
            {
                _contentService.Delete(content);
                messages.Add($"Deleted content: {content.Name}");
            }

            // Delete document types in reverse order (children first)
            var aliasesToDelete = new[]
            {
                AlgoraDocumentTypeConstants.ProductAlias,
                AlgoraDocumentTypeConstants.CategoryAlias,
                AlgoraDocumentTypeConstants.CatalogAlias,
                AlgoraDocumentTypeConstants.OrderAlias,
                AlgoraDocumentTypeConstants.CheckoutStepAlias
            };

            foreach (var alias in aliasesToDelete)
            {
                var docType = _contentTypeService.Get(alias);
                if (docType != null)
                {
                    _contentTypeService.Delete(docType);
                    messages.Add($"Deleted document type: {alias}");
                }
            }

            // Reinstall document types
            var results = _documentTypeInstaller.InstallAll();

            // Re-seed catalog
            var seedResult = _seeder.SeedSampleCatalog();

            return Ok(new
            {
                success = true,
                messages,
                installResults = results.Select(r => new
                {
                    alias = r.Alias,
                    name = r.Name,
                    action = r.Action.ToString(),
                    success = r.Success
                }),
                seedResult = new
                {
                    created = seedResult.Created,
                    messages = seedResult.Messages,
                    errors = seedResult.Errors
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message, stackTrace = ex.StackTrace });
        }
    }
}
