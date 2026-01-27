# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

> **Note:** This is a subproject of UAlgora.Ecommerce. See the root [CLAUDE.md](../../CLAUDE.md) for overall solution architecture.

## Project Overview

The License Portal is a standalone ASP.NET Core MVC application that handles license purchasing, customer account management, and subscription renewals for Algora Commerce. It runs independently from the main Umbraco site.

## Tech Stack

- **Runtime:** .NET 9 / C# 13
- **Framework:** ASP.NET Core MVC
- **Database:** SQL Server (shared EcommerceDbContext from Infrastructure)
- **Payments:** Stripe and Razorpay
- **Email:** SendGrid
- **Frontend:** Razor views with Bootstrap 5

## Development Commands

```bash
# Run the portal (default: https://localhost:5001)
dotnet run

# Run with specific environment
dotnet run --environment Development

# Run from solution root
dotnet run --project src/UAlgora.Ecommerce.LicensePortal
```

## Project Structure

```
LicensePortal/
├── Controllers/
│   ├── HomeController.cs      # Landing page, pricing display
│   ├── CheckoutController.cs  # License purchase flow
│   ├── AccountController.cs   # Customer dashboard, license management
│   ├── WebhookController.cs   # Stripe/Razorpay webhooks
│   └── DevController.cs       # Development-only test endpoints
├── Services/
│   ├── IStripePaymentService.cs / StripePaymentService.cs
│   ├── IRazorpayPaymentService.cs / RazorpayPaymentService.cs
│   ├── ILicenseGenerationService.cs / LicenseGenerationService.cs
│   └── IEmailService.cs / EmailService.cs
├── Models/
│   ├── LicensePortalOptions.cs  # Configuration model
│   ├── CheckoutViewModel.cs     # Checkout page model
│   └── AccountViewModels.cs     # Account management models
└── Views/
```

## Dependencies

Uses repositories from Infrastructure layer:
- `ILicenseRepository` - License records
- `ILicenseSubscriptionRepository` - Active subscriptions
- `ILicensePaymentRepository` - Payment history

## Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=UAlgoraEcommerce;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "LicensePortal": {
    "BaseUrl": "https://licenses.algoracommerce.com",
    "Pricing": {
      "Standard": { "AnnualPrice": 1500, "Currency": "USD" },
      "Enterprise": { "AnnualPrice": 3000, "Currency": "USD" }
    },
    "Stripe": {
      "PublishableKey": "pk_...",
      "SecretKey": "sk_...",
      "WebhookSecret": "whsec_..."
    },
    "Razorpay": {
      "KeyId": "rzp_...",
      "KeySecret": "...",
      "WebhookSecret": "..."
    },
    "Email": {
      "SendGridApiKey": "SG...",
      "FromEmail": "noreply@algoracommerce.com",
      "FromName": "Algora Commerce"
    }
  }
}
```

## Key Flows

### License Purchase
1. User selects tier on `/Home/Pricing`
2. Checkout form at `/Checkout` collects info
3. Stripe/Razorpay payment processing
4. `LicenseGenerationService` creates encrypted license key
5. `EmailService` sends license via SendGrid

### Webhook Handling
- **Stripe:** `POST /Webhook/Stripe` - handles `checkout.session.completed`, `invoice.paid`
- **Razorpay:** `POST /Webhook/Razorpay` - handles `payment.captured`, `subscription.charged`

### Customer Account
- Login/register at `/Account`
- View licenses, download keys, manage subscriptions
- Renewal and upgrade workflows

## License Tiers

| Tier | Annual Price | Key Features |
|------|-------------|--------------|
| Standard | $1,500 | Single store, all core features |
| Enterprise | $3,000 | Multi-store, B2B, white-labeling |

## DevController (Development Only)

Available only in Development environment for testing:
- `/Dev/TestEmail` - Test email delivery
- `/Dev/GenerateTestLicense` - Create test license
