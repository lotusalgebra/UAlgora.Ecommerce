# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Algora Commerce (UAlgora.Ecommerce) is an enterprise e-commerce plugin for Umbraco 15+ CMS. It provides complete e-commerce functionality with both a custom storefront and deep CMS integration through document types and content synchronization.

## Tech Stack

- **Runtime:** .NET 9 / C# 13
- **CMS:** Umbraco 15+ (15.4.4+)
- **Database:** MS SQL Server with Entity Framework Core 9
- **Backoffice UI:** Vanilla JS with Umbraco UI components (no build step)
- **Storefront:** Razor views with Tailwind CSS and Alpine.js

## Architecture

Clean Architecture with four layers:

```
src/
├── UAlgora.Ecommerce.Core/           # Domain models, interfaces (no dependencies)
├── UAlgora.Ecommerce.Infrastructure/ # EF Core, repositories, services
├── UAlgora.Ecommerce.Web/            # Umbraco integration, APIs, backoffice UI
├── UAlgora.Ecommerce.Site/           # Demo site with storefront
└── UAlgora.Ecommerce.LicensePortal/  # Standalone license purchasing portal
```

### Key Patterns

- **Repository Pattern:** All data access through `I*Repository` interfaces
- **Service Layer:** Business logic in `I*Service` implementations
- **Document Types:** CMS content types defined via `IDocumentTypeDefinitionProvider`
- **Content Sync:** Bidirectional sync between database entities and Umbraco content nodes

## Development Commands

```bash
# Build
dotnet build

# Run the site (applies migrations and seeds demo data on startup)
dotnet run --project src/UAlgora.Ecommerce.Site

# Run tests
dotnet test

# Run specific test project
dotnet test tests/UAlgora.Ecommerce.Tests.UI

# Create migration
dotnet ef migrations add <Name> -p src/UAlgora.Ecommerce.Infrastructure -s src/UAlgora.Ecommerce.Site

# Apply migrations manually
dotnet ef database update -p src/UAlgora.Ecommerce.Infrastructure -s src/UAlgora.Ecommerce.Site

# Run License Portal (separate app on different port)
dotnet run --project src/UAlgora.Ecommerce.LicensePortal
```

## Backoffice UI Architecture

The backoffice uses Umbraco's extension system with vanilla JavaScript (no framework):

- **Package manifest:** `wwwroot/App_Plugins/Ecommerce/umbraco-package.json`
- **Collection views:** `views/collections/*-collection.js` - Inline editors with split-pane layout
- **Entry point:** `index.js` - Registers all components

Each collection view (`product-collection.js`, `order-collection.js`, etc.) is a self-contained web component with:
- List panel with search/filter
- Editor panel with tabbed forms
- Direct API calls to Management API

**Important:** App_Plugins exists in both `Web` and `Site` projects. When modifying backoffice UI, update both locations to keep them in sync:
- `src/UAlgora.Ecommerce.Web/wwwroot/App_Plugins/Ecommerce/`
- `src/UAlgora.Ecommerce.Site/wwwroot/App_Plugins/Ecommerce/`

### Management API Pattern

Controllers extend `EcommerceManagementApiControllerBase` and use:
- Route: `/umbraco/management/api/v1/ecommerce/{entity}`
- Attributes: `[VersionedApiBackOfficeRoute]`, `[ApiExplorerSettings(GroupName = "Ecommerce")]`

## API Layers

### Storefront API (`/api/ecommerce/`)
Public endpoints for cart, checkout, products. Controllers in `Web/Controllers/Api/`.

### Management API (`/umbraco/management/api/v1/ecommerce/`)
Backoffice CRUD endpoints. Controllers in `Web/BackOffice/Api/`.

### Customer Auth (`/api/ecommerce/auth/`)
Standalone authentication (separate from Umbraco Members):
- Cookie scheme: `EcommerceCustomer`
- Password hashing: BCrypt
- Account lockout after 5 failed attempts

## Document Types & Content Sync

Document type providers in `Web/DocumentTypes/Providers/` define CMS content types that can be managed in the Umbraco content tree. The system supports bidirectional sync:

1. **DB → Content:** `ProductContentSyncService`, `CategoryContentSyncService`
2. **Content → DB:** `ContentToProductSyncHandler`, `ContentToCategorySyncHandler`

Storefront pages can be either:
- **Hardcoded views:** `Site/Views/Storefront/*.cshtml`
- **CMS-driven:** `Site/Views/algora*.cshtml` (templates for document types)

The `StorefrontController` checks for CMS pages first, falls back to hardcoded views.

## Enterprise Features (License-Gated)

Multi-store, gift cards, returns, email templates, webhooks, audit logging, payment links, GST/tax support, invoicing.

### License Tiers
- **Trial:** 14 days, limited features
- **Standard ($1,500/yr):** Single store, all features
- **Enterprise ($3,000/yr):** Multi-store, priority support

License validation in `Web/Licensing/` with `LicenseContext` singleton.

## Configuration

```json
{
  "ConnectionStrings": {
    "umbracoDbDSN": "Server=.;Database=...;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Algora": {
    "License": {
      "LicenseKey": "ALG-XXX-...",
      "ValidationIntervalHours": 24
    }
  }
}
```

## Service Registration

In `Program.cs`:
```csharp
builder.Services.AddEcommerceInfrastructure(connectionString); // Repos + Services
builder.Services.AddEcommerceWeb();  // Web layer + Content sync
builder.Services.AddAlgoraLicensing(); // Optional: License validation
```

Services are registered via extension methods in each layer's `ServiceCollectionExtensions.cs`.

### Umbraco Composers

Composers auto-register services when the plugin loads:
- `EcommerceComposer` - Main DI registration and notification handlers
- `EcommerceSectionPermissionComposer` - Backoffice section permissions
- `AlgoraDocumentTypeComposer` - Document type provider registration

## Database

- All tables prefixed with `Ecommerce*`
- Soft delete via `IsDeleted` flag
- Audit fields: `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`
- Multi-store foreign keys where applicable

## Testing

```
tests/
├── UAlgora.Ecommerce.Core.Tests/           # Domain model unit tests
├── UAlgora.Ecommerce.Infrastructure.Tests/ # Repository/service tests
├── UAlgora.Ecommerce.Web.Tests/            # API controller tests
└── UAlgora.Ecommerce.Tests.UI/             # Selenium E2E tests
```

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/UAlgora.Ecommerce.Core.Tests

# Run single test by name
dotnet test --filter "FullyQualifiedName~TestMethodName"

# Run tests in specific class
dotnet test --filter "FullyQualifiedName~ClassName"
```

## Supplementary Documentation

- `ecommerce-features.claude.md` - Comprehensive e-commerce domain reference (product, cart, checkout, payments, etc.)
- `umbraco-ecommerce-plugin.claude.md` - Umbraco 15+ integration patterns and code examples
- `docs/` - Product documentation (docsify-based)

## Platform Notes

- **Windows development:** Uses LocalDB for SQL Server (connection string pattern: `Server=(localdb)\MSSQLLocalDB`)
- **Umbraco cache:** Delete `umbraco/Data/TEMP` and `umbraco/Data/NuCache` if backoffice shows stale data after model changes
