# UAlgora.Ecommerce

A comprehensive e-commerce plugin for Umbraco 15+ built with clean architecture principles.

## Features

### Core E-commerce

- **Products** - Full product management with variants, SKUs, pricing, inventory tracking, and SEO metadata
- **Categories** - Hierarchical category organization with unlimited nesting
- **Customers** - Customer accounts with addresses, loyalty points, and store credit
- **Orders** - Complete order lifecycle with status workflow, shipments, and refunds
- **Carts** - Shopping cart with guest support, expiration, and abandoned cart recovery
- **Wishlists** - Multiple wishlists per customer with sharing and merging capabilities
- **Discounts** - Flexible discount system with codes, conditions, and usage limits

### Additional Modules

- **Reviews** - Product reviews with ratings, moderation, and verified purchase badges
- **Inventory** - Multi-warehouse stock management with transfers and adjustments
- **Shipping** - Configurable shipping zones, methods, and rate calculations
- **Tax** - Tax zones, categories, and automatic tax calculations
- **Payments** - Multiple payment gateway integrations
- **Currency** - Multi-currency support with exchange rates

### Umbraco Backoffice Integration

- Custom section with tree navigation
- Workspace editors for all entities
- Quick actions for common operations
- Dashboard with sales statistics
- Full Management API

## Requirements

- .NET 9.0+
- Umbraco 15.0+
- SQL Server 2019+ (or SQL Server Express)

## Installation

### Via NuGet (Recommended)

```bash
dotnet add package UAlgora.Ecommerce.Web
```

### Manual Installation

1. Clone the repository:
```bash
git clone https://github.com/lotusalgebra/UAlgora.Ecommerce.git
```

2. Add project references to your Umbraco solution:
```bash
dotnet add reference path/to/UAlgora.Ecommerce.Web/UAlgora.Ecommerce.Web.csproj
```

3. Run database migrations:
```bash
dotnet ef database update --project src/UAlgora.Ecommerce.Infrastructure
```

## Configuration

Add the following to your `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "EcommerceConnection": "Server=.;Database=UAlgoraEcommerce;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Ecommerce": {
    "DefaultCurrency": "USD",
    "EnableGuestCheckout": true,
    "CartExpirationDays": 30,
    "AbandonedCartDays": 7
  }
}
```

## Project Structure

```
UAlgora.Ecommerce/
├── src/
│   ├── UAlgora.Ecommerce.Core/           # Domain models, interfaces, enums
│   ├── UAlgora.Ecommerce.Infrastructure/ # EF Core, repositories, services
│   ├── UAlgora.Ecommerce.Web/            # API controllers, backoffice UI
│   └── UAlgora.Ecommerce.Site/           # Demo Umbraco site
├── tests/
│   ├── UAlgora.Ecommerce.Core.Tests/
│   ├── UAlgora.Ecommerce.Infrastructure.Tests/
│   └── UAlgora.Ecommerce.Web.Tests/
└── README.md
```

## Architecture

This project follows **Clean Architecture** principles:

- **Core Layer** - Domain entities, interfaces, and business rules (no external dependencies)
- **Infrastructure Layer** - Data access, external services, and implementations
- **Web Layer** - API controllers, Umbraco integration, and UI components

### Key Patterns

- Repository Pattern for data access
- Service Layer for business logic
- CQRS-lite with query parameters
- Soft delete for data retention
- Audit trails with CreatedAt/UpdatedAt

## API Endpoints

### Storefront API

| Endpoint | Description |
|----------|-------------|
| `GET /api/ecommerce/products` | List products with filtering |
| `GET /api/ecommerce/products/{id}` | Get product details |
| `GET /api/ecommerce/categories` | List categories |
| `POST /api/ecommerce/cart/items` | Add item to cart |
| `POST /api/ecommerce/checkout` | Process checkout |
| `GET /api/ecommerce/orders` | List customer orders |

### Management API (Backoffice)

| Endpoint | Description |
|----------|-------------|
| `GET /umbraco/backoffice/ecommerce/products` | Manage products |
| `GET /umbraco/backoffice/ecommerce/orders` | Manage orders |
| `GET /umbraco/backoffice/ecommerce/customers` | Manage customers |
| `GET /umbraco/backoffice/ecommerce/discounts` | Manage discounts |
| `GET /umbraco/backoffice/ecommerce/dashboard/statistics` | Sales statistics |

## Backoffice Workspaces

The plugin adds a dedicated **Ecommerce** section to the Umbraco backoffice with the following workspaces:

| Workspace | Actions |
|-----------|---------|
| Products | Save, Duplicate, Publish, Archive, Featured, Visibility, Stock, Sale |
| Categories | Save, Toggle, Move, Duplicate, Featured, Root, Sort |
| Customers | Save, Add Credit, Add Points, Status, Marketing, Verify |
| Orders | Status, Ship, Deliver, Complete, Cancel, Hold, Refund, Note, Print, Create Shipment |
| Carts | Clear, Notes, Expiration, Assign, Delete |
| Wishlists | Default, Public, Share, Remove Share, Rename, Duplicate, Clear |
| Discounts | Save, Toggle, Duplicate, Copy Code, Extend, Priority, Combine, Reset Usage |
| Reviews | Approve, Reject, Featured, Verified, Rating, Respond, Remove Response, Reset Votes |
| Shipping | Zones, Methods, Rates |
| Tax | Zones, Categories, Rates |
| Payments | Gateways, Methods |
| Inventory | Warehouses, Adjustments, Transfers, Suppliers |
| Currency | Currencies, Exchange Rates |

## Development

### Build

```bash
dotnet build
```

### Run Tests

```bash
dotnet test
```

### Run Development Server

```bash
cd src/UAlgora.Ecommerce.Site
dotnet run
```

### Add Migration

```bash
dotnet ef migrations add MigrationName --project src/UAlgora.Ecommerce.Infrastructure --startup-project src/UAlgora.Ecommerce.Site
```

## Extensibility

The plugin is designed for extensibility:

### Custom Payment Provider

```csharp
public class MyPaymentProvider : IPaymentProvider
{
    public string Name => "MyPayment";

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        // Your implementation
    }
}
```

### Custom Shipping Provider

```csharp
public class MyShippingProvider : IShippingProvider
{
    public string Name => "MyShipping";

    public async Task<ShippingRate> CalculateRateAsync(ShippingRequest request)
    {
        // Your implementation
    }
}
```

### Register Custom Providers

```csharp
services.AddScoped<IPaymentProvider, MyPaymentProvider>();
services.AddScoped<IShippingProvider, MyShippingProvider>();
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- **Issues**: [GitHub Issues](https://github.com/lotusalgebra/UAlgora.Ecommerce/issues)
- **Documentation**: [Wiki](https://github.com/lotusalgebra/UAlgora.Ecommerce/wiki)

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## Acknowledgments

- Built for [Umbraco CMS](https://umbraco.com/)
- UI components powered by [Lit](https://lit.dev/)
- Icons from [Umbraco UI](https://uui.umbraco.com/)
