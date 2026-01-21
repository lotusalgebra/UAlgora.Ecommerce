# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

UAlgora.Ecommerce is a full-featured e-commerce plugin for Umbraco 15+ CMS. It provides complete e-commerce functionality including product management, cart, checkout, payments, orders, shipping, and backoffice administration via tree views.

## Tech Stack

- **Runtime:** .NET 9 / C# 13
- **CMS:** Umbraco 15+
- **Database:** MS SQL Server (Entity Framework Core 9)
- **Backoffice UI:** Lit + Web Components
- **Frontend:** Angular / Razor / Blazor

## Architecture

This project follows **Clean Architecture** with loosely coupled, fully object-oriented code:

```
src/
├── UAlgora.Ecommerce.Core/           # Domain models, interfaces, events
│   ├── Models/Domain/                # Product, Cart, Order, Customer, etc.
│   ├── Interfaces/                   # Repository & service contracts
│   └── Events/                       # Domain events
├── UAlgora.Ecommerce.Infrastructure/ # Data access, external services
│   ├── Data/                         # EF Core DbContext, configurations
│   ├── Repositories/                 # Repository implementations
│   ├── Services/                     # Business logic implementations
│   └── Providers/                    # Payment, Shipping, Tax providers
├── UAlgora.Ecommerce.Web/            # Umbraco integration layer
│   ├── Composers/                    # DI registration
│   ├── Controllers/Api/              # REST API endpoints
│   ├── Controllers/Backoffice/       # Admin tree controllers
│   ├── NotificationHandlers/         # Umbraco event handlers
│   └── wwwroot/App_Plugins/          # Backoffice UI components
└── tests/                            # Unit and integration tests
```

## Development Commands

```bash
# Build solution
dotnet build

# Run the Umbraco site
dotnet run --project src/UAlgora.Ecommerce.Site

# Run tests
dotnet test

# Create EF Core migration
dotnet ef migrations add <MigrationName> -p src/UAlgora.Ecommerce.Infrastructure -s src/UAlgora.Ecommerce.Site

# Apply migrations
dotnet ef database update -p src/UAlgora.Ecommerce.Infrastructure -s src/UAlgora.Ecommerce.Site

# Build NuGet package
dotnet pack -c Release
```

## Key Umbraco Integration Points

### Composers
Register all services in `EcommerceComposer.cs` using `IUmbracoBuilder`:
- DbContext, repositories, services via `builder.Services`
- Notification handlers via `builder.AddNotificationHandler<T, THandler>()`
- Configuration via `builder.Services.Configure<T>(builder.Config.GetSection("..."))`

### Backoffice Extensions
- **Sections:** Custom navigation area (Commerce section)
- **Trees:** Hierarchical navigation (Products, Orders, Customers)
- **Dashboards:** Overview widgets for the Commerce section
- **Content Apps:** Contextual panels on product content nodes

### Package Manifest
Define backoffice UI extensions in `wwwroot/App_Plugins/UAlgora.Ecommerce/umbraco-package.json`

## Core Domain Models

- **Product/ProductVariant:** SKU, pricing, inventory, variants with options
- **Cart/CartItem:** Session-based or customer-linked shopping cart
- **Order/OrderLine:** Order lifecycle, payment status, fulfillment status
- **Customer:** Account, addresses, order history
- **Discount:** Coupon codes, automatic discounts, rules

## API Endpoints

Base path: `/api/ecommerce/`

| Endpoint | Description |
|----------|-------------|
| `GET /cart` | Get current cart |
| `POST /cart/items` | Add item to cart |
| `PUT /cart/items/{id}` | Update item quantity |
| `DELETE /cart/items/{id}` | Remove item |
| `POST /cart/coupon` | Apply coupon code |
| `POST /checkout/initialize` | Start checkout session |
| `POST /checkout/{id}/complete` | Complete order |
| `GET /products` | List products (paginated) |
| `GET /orders/{id}` | Get order details |

## Configuration

Settings in `appsettings.json` under `"Ecommerce"` section:
- `Store`: Name, currency, tax settings
- `Stripe`: Payment gateway credentials
- `Shipping`: Rates and free shipping threshold
- `Tax`: Default rate and regional rates
- `Inventory`: Stock thresholds, backorder settings

## Design Principles

1. **Extensibility:** All services use interfaces; users can swap implementations
2. **Tree View Management:** Products, categories, orders managed via Umbraco backoffice trees
3. **NuGet Distribution:** Packaged for easy installation
4. **Licensing:** 14-day trial with encrypted license key validation

## Database Tables

All tables prefixed with `Ecommerce`:
- `EcommerceProducts`, `EcommerceProductVariants`
- `EcommerceCarts`, `EcommerceCartItems`
- `EcommerceOrders`, `EcommerceOrderLines`
- `EcommerceCustomers`, `EcommerceDiscounts`
