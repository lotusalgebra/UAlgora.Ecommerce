# Installation Guide

This guide walks you through installing Algora Commerce on your Umbraco 15+ site.

## Prerequisites

Before installing, ensure you have:

- **Umbraco 15.0+** installed and running
- **.NET 9.0 SDK** installed
- **SQL Server 2019+** (or SQL Server Express)
- **Visual Studio 2022** or **VS Code** with C# extension

## Installation Methods

### Method 1: NuGet Package (Recommended)

The easiest way to install Algora Commerce is via NuGet:

```bash
# Navigate to your Umbraco project
cd YourUmbracoSite

# Install the package
dotnet add package UAlgora.Ecommerce.Web
```

### Method 2: Project Reference

For development or customization, you can reference the projects directly:

```bash
# Clone the repository
git clone https://github.com/lotusalgebra/UAlgora.Ecommerce.git

# Add project references
dotnet add reference path/to/UAlgora.Ecommerce.Web/UAlgora.Ecommerce.Web.csproj
```

## Configuration

### 1. Database Connection

Algora Commerce uses Entity Framework Core and shares the Umbraco database by default. Ensure your `appsettings.json` has the connection string:

```json
{
  "ConnectionStrings": {
    "umbracoDbDSN": "Server=.;Database=YourUmbracoDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### 2. Run Database Migrations

Apply the e-commerce database tables:

```bash
dotnet ef database update -p src/UAlgora.Ecommerce.Infrastructure -s YourUmbracoSite
```

Or, migrations are applied automatically on first startup.

### 3. Add E-Commerce Configuration

Add the following to your `appsettings.json`:

```json
{
  "Ecommerce": {
    "Store": {
      "Name": "My Store",
      "DefaultCurrency": "USD",
      "PricesIncludeTax": false
    },
    "Stripe": {
      "PublishableKey": "pk_test_...",
      "SecretKey": "sk_test_...",
      "WebhookSecret": "whsec_..."
    },
    "Shipping": {
      "StandardRate": 5.99,
      "ExpressRate": 12.99,
      "FreeShippingThreshold": 50.00
    },
    "Tax": {
      "DefaultRate": 0.0,
      "TaxShipping": false
    }
  },
  "Algora": {
    "License": {
      "LicenseKey": "YOUR-LICENSE-KEY",
      "ValidationIntervalHours": 24
    }
  }
}
```

### 4. Register Services

In your `Program.cs`, add the e-commerce services:

```csharp
// After builder.CreateUmbracoBuilder()
builder.Services.AddEcommerceInfrastructure(connectionString);
builder.Services.AddEcommerceWeb();
```

### 5. Start the Application

```bash
dotnet run
```

## Verification

After installation, verify everything is working:

1. **Backoffice Access**: Log in to Umbraco backoffice
2. **Commerce Section**: You should see a new "Commerce" section in the left sidebar
3. **Dashboard**: The Commerce dashboard shows sales overview

![Commerce Section](../screenshots/backoffice-section.png)

## Troubleshooting

### Issue: Commerce section not appearing

**Solution**: Ensure your user has access to the Commerce section:
1. Go to Users > Groups
2. Edit your user group
3. Enable "Commerce" section access

### Issue: Database migration errors

**Solution**: Manually run migrations:
```bash
dotnet ef migrations add InitialEcommerce -p src/UAlgora.Ecommerce.Infrastructure -s YourUmbracoSite
dotnet ef database update -p src/UAlgora.Ecommerce.Infrastructure -s YourUmbracoSite
```

### Issue: License validation failed

**Solution**:
1. Check your license key in `appsettings.json`
2. Ensure your domain matches the licensed domain
3. Verify the license hasn't expired at https://licenses.algoracommerce.com

## Next Steps

- [Configuration Guide](./configuration.md) - Detailed configuration options
- [Quick Start Tutorial](./quick-start.md) - Create your first product
- [Product Management](../user-guide/products.md) - Learn about product features
