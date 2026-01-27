# Configuration Guide

Complete reference for all Algora Commerce configuration options.

## Configuration File

All settings go in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "umbracoDbDSN": "..."
  },
  "Ecommerce": {
    "Store": { },
    "Stripe": { },
    "Razorpay": { },
    "Shipping": { },
    "Tax": { },
    "Email": { },
    "Inventory": { },
    "Cart": { },
    "Checkout": { }
  },
  "Algora": {
    "License": { }
  }
}
```

---

## Store Settings

```json
{
  "Ecommerce": {
    "Store": {
      "Name": "My Store",
      "DefaultCurrency": "USD",
      "SupportedCurrencies": ["USD", "EUR", "GBP"],
      "PricesIncludeTax": false,
      "WeightUnit": "kg",
      "DimensionUnit": "cm",
      "Timezone": "America/New_York",
      "ContactEmail": "support@mystore.com",
      "ContactPhone": "+1-555-123-4567"
    }
  }
}
```

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `Name` | string | "My Store" | Store name displayed to customers |
| `DefaultCurrency` | string | "USD" | Default currency code (ISO 4217) |
| `SupportedCurrencies` | array | ["USD"] | Multi-currency support (Enterprise) |
| `PricesIncludeTax` | bool | false | Whether prices include tax |
| `WeightUnit` | string | "kg" | Weight unit: "kg" or "lb" |
| `DimensionUnit` | string | "cm" | Dimension unit: "cm" or "in" |
| `Timezone` | string | "UTC" | Store timezone |
| `ContactEmail` | string | | Support email |
| `ContactPhone` | string | | Support phone |

---

## Payment Gateways

### Stripe

```json
{
  "Ecommerce": {
    "Stripe": {
      "PublishableKey": "pk_live_...",
      "SecretKey": "sk_live_...",
      "WebhookSecret": "whsec_...",
      "CaptureMethod": "automatic",
      "StatementDescriptor": "MYSTORE",
      "EnableApplePay": true,
      "EnableGooglePay": true
    }
  }
}
```

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `PublishableKey` | string | required | Stripe publishable API key |
| `SecretKey` | string | required | Stripe secret API key |
| `WebhookSecret` | string | required | Webhook signing secret |
| `CaptureMethod` | string | "automatic" | "automatic" or "manual" |
| `StatementDescriptor` | string | | Bank statement description (max 22 chars) |
| `EnableApplePay` | bool | true | Enable Apple Pay |
| `EnableGooglePay` | bool | true | Enable Google Pay |

**Getting Stripe Keys:**
1. Log in to [Stripe Dashboard](https://dashboard.stripe.com)
2. Go to Developers > API Keys
3. Copy Publishable and Secret keys
4. For webhooks: Developers > Webhooks > Add endpoint

### Razorpay

```json
{
  "Ecommerce": {
    "Razorpay": {
      "KeyId": "rzp_live_...",
      "KeySecret": "...",
      "WebhookSecret": "...",
      "DisplayName": "My Store",
      "Description": "Payment for order",
      "Currency": "INR",
      "EnableUPI": true,
      "EnableNetBanking": true,
      "EnableCards": true
    }
  }
}
```

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `KeyId` | string | required | Razorpay Key ID |
| `KeySecret` | string | required | Razorpay Key Secret |
| `WebhookSecret` | string | required | Webhook secret |
| `DisplayName` | string | Store name | Checkout modal title |
| `Description` | string | | Payment description |
| `Currency` | string | "INR" | Currency code |
| `EnableUPI` | bool | true | Enable UPI payments |
| `EnableNetBanking` | bool | true | Enable net banking |
| `EnableCards` | bool | true | Enable card payments |

---

## Shipping

```json
{
  "Ecommerce": {
    "Shipping": {
      "DefaultProvider": "flat_rate",
      "FreeShippingThreshold": 50.00,
      "FreeShippingCountries": ["US", "CA"],
      "Methods": [
        {
          "Id": "standard",
          "Name": "Standard Shipping",
          "Description": "5-7 business days",
          "Rate": 5.99,
          "MinDays": 5,
          "MaxDays": 7,
          "Countries": ["US"],
          "MinOrderAmount": 0
        },
        {
          "Id": "express",
          "Name": "Express Shipping",
          "Description": "2-3 business days",
          "Rate": 12.99,
          "MinDays": 2,
          "MaxDays": 3,
          "Countries": ["US"]
        },
        {
          "Id": "international",
          "Name": "International",
          "Description": "7-14 business days",
          "Rate": 24.99,
          "MinDays": 7,
          "MaxDays": 14,
          "Countries": ["*"],
          "ExcludeCountries": ["US", "CA"]
        }
      ]
    }
  }
}
```

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `DefaultProvider` | string | "flat_rate" | Shipping calculation method |
| `FreeShippingThreshold` | decimal | 0 | Order total for free shipping (0 = disabled) |
| `FreeShippingCountries` | array | [] | Countries eligible for free shipping |
| `Methods` | array | [] | Available shipping methods |

### Shipping Method Options

| Setting | Type | Description |
|---------|------|-------------|
| `Id` | string | Unique identifier |
| `Name` | string | Display name |
| `Description` | string | Customer-visible description |
| `Rate` | decimal | Shipping cost |
| `MinDays` | int | Minimum delivery days |
| `MaxDays` | int | Maximum delivery days |
| `Countries` | array | Applicable countries ("*" for all) |
| `ExcludeCountries` | array | Countries to exclude |
| `MinOrderAmount` | decimal | Minimum order for this method |
| `MaxOrderAmount` | decimal | Maximum order for this method |
| `MinWeight` | decimal | Minimum order weight |
| `MaxWeight` | decimal | Maximum order weight |

---

## Tax

```json
{
  "Ecommerce": {
    "Tax": {
      "DefaultRate": 0.0,
      "TaxShipping": false,
      "DisplayPricesWithTax": false,
      "CalculationMethod": "line_item",
      "TaxRates": [
        {
          "CountryCode": "US",
          "StateCode": "NY",
          "Rate": 0.08875,
          "Name": "NY Sales Tax"
        },
        {
          "CountryCode": "US",
          "StateCode": "CA",
          "Rate": 0.0725,
          "Name": "CA Sales Tax"
        },
        {
          "CountryCode": "GB",
          "Rate": 0.20,
          "Name": "UK VAT"
        }
      ],
      "TaxExemptCustomerTags": ["wholesale", "reseller"]
    }
  }
}
```

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `DefaultRate` | decimal | 0.0 | Default tax rate (fallback) |
| `TaxShipping` | bool | false | Whether to tax shipping |
| `DisplayPricesWithTax` | bool | false | Show prices including tax |
| `CalculationMethod` | string | "line_item" | "line_item" or "order_total" |
| `TaxRates` | array | [] | Tax rates by region |
| `TaxExemptCustomerTags` | array | [] | Customer tags for tax exemption |

---

## Email

```json
{
  "Ecommerce": {
    "Email": {
      "Provider": "sendgrid",
      "SendGridApiKey": "SG...",
      "FromEmail": "orders@mystore.com",
      "FromName": "My Store",
      "ReplyToEmail": "support@mystore.com",
      "BccEmail": "orders-archive@mystore.com",
      "Templates": {
        "OrderConfirmation": "d-abc123...",
        "ShippingNotification": "d-def456...",
        "RefundNotification": "d-ghi789..."
      }
    }
  }
}
```

| Setting | Type | Description |
|---------|------|-------------|
| `Provider` | string | Email provider: "sendgrid", "smtp", "mailgun" |
| `SendGridApiKey` | string | SendGrid API key |
| `FromEmail` | string | Sender email address |
| `FromName` | string | Sender name |
| `ReplyToEmail` | string | Reply-to address |
| `BccEmail` | string | BCC for all order emails |
| `Templates` | object | SendGrid dynamic template IDs |

### SMTP Configuration

```json
{
  "Ecommerce": {
    "Email": {
      "Provider": "smtp",
      "SmtpHost": "smtp.example.com",
      "SmtpPort": 587,
      "SmtpUsername": "user@example.com",
      "SmtpPassword": "password",
      "SmtpEnableSsl": true,
      "FromEmail": "orders@example.com",
      "FromName": "My Store"
    }
  }
}
```

---

## Inventory

```json
{
  "Ecommerce": {
    "Inventory": {
      "TrackInventory": true,
      "LowStockThreshold": 5,
      "OutOfStockThreshold": 0,
      "AllowBackorders": false,
      "ReserveStockOnAddToCart": false,
      "ReserveStockDuration": 15,
      "ShowStockLevel": true,
      "ShowLowStockWarning": true
    }
  }
}
```

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `TrackInventory` | bool | true | Enable inventory tracking |
| `LowStockThreshold` | int | 5 | Low stock warning level |
| `OutOfStockThreshold` | int | 0 | Out of stock level |
| `AllowBackorders` | bool | false | Allow orders when out of stock |
| `ReserveStockOnAddToCart` | bool | false | Reserve stock in cart |
| `ReserveStockDuration` | int | 15 | Minutes to hold reservation |
| `ShowStockLevel` | bool | true | Display stock count on product page |
| `ShowLowStockWarning` | bool | true | Show "Only X left" warning |

---

## Cart

```json
{
  "Ecommerce": {
    "Cart": {
      "ExpirationDays": 30,
      "MergeOnLogin": true,
      "MaxItemQuantity": 99,
      "MaxUniqueItems": 100,
      "AllowNotes": true,
      "AllowGiftMessage": true,
      "EnableWishlist": true
    }
  }
}
```

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `ExpirationDays` | int | 30 | Days before cart expires |
| `MergeOnLogin` | bool | true | Merge guest cart on login |
| `MaxItemQuantity` | int | 99 | Max quantity per line item |
| `MaxUniqueItems` | int | 100 | Max different products in cart |
| `AllowNotes` | bool | true | Allow cart/order notes |
| `AllowGiftMessage` | bool | true | Enable gift messages |
| `EnableWishlist` | bool | true | Enable wishlist feature |

---

## Checkout

```json
{
  "Ecommerce": {
    "Checkout": {
      "Type": "multi-step",
      "GuestCheckout": true,
      "RequirePhone": false,
      "RequireCompany": false,
      "DefaultCountry": "US",
      "EnableAddressAutocomplete": true,
      "TermsAndConditionsUrl": "/terms",
      "PrivacyPolicyUrl": "/privacy",
      "MinimumOrderAmount": 0,
      "MaximumOrderAmount": 10000
    }
  }
}
```

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `Type` | string | "multi-step" | "multi-step" or "single-page" |
| `GuestCheckout` | bool | true | Allow guest checkout |
| `RequirePhone` | bool | false | Phone number required |
| `RequireCompany` | bool | false | Company name required |
| `DefaultCountry` | string | "US" | Default country selection |
| `EnableAddressAutocomplete` | bool | true | Google Places autocomplete |
| `TermsAndConditionsUrl` | string | "/terms" | Terms page URL |
| `PrivacyPolicyUrl` | string | "/privacy" | Privacy page URL |
| `MinimumOrderAmount` | decimal | 0 | Minimum order total |
| `MaximumOrderAmount` | decimal | 10000 | Maximum order total |

---

## License

```json
{
  "Algora": {
    "License": {
      "LicenseKey": "ALG-STD-XXXX-XXXX-XXXX",
      "ValidationIntervalHours": 24,
      "AllowOfflineGraceDays": 7
    }
  }
}
```

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `LicenseKey` | string | required | Your license key |
| `ValidationIntervalHours` | int | 24 | Hours between license checks |
| `AllowOfflineGraceDays` | int | 7 | Grace period if validation fails |

**Getting a License:**
1. Visit [https://licenses.algoracommerce.com](https://licenses.algoracommerce.com)
2. Choose your plan (Standard or Enterprise)
3. Complete payment
4. License key emailed immediately

---

## Environment Variables

For sensitive settings, use environment variables:

```bash
# Windows
set Ecommerce__Stripe__SecretKey=sk_live_xxx
set Algora__License__LicenseKey=ALG-STD-xxx

# Linux/Mac
export Ecommerce__Stripe__SecretKey=sk_live_xxx
export Algora__License__LicenseKey=ALG-STD-xxx
```

Or in `appsettings.json`:
```json
{
  "Ecommerce": {
    "Stripe": {
      "SecretKey": "${STRIPE_SECRET_KEY}"
    }
  }
}
```

---

## Per-Environment Configuration

Use environment-specific files:

- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production overrides

```json
// appsettings.Development.json
{
  "Ecommerce": {
    "Stripe": {
      "PublishableKey": "pk_test_...",
      "SecretKey": "sk_test_..."
    }
  }
}
```

```json
// appsettings.Production.json
{
  "Ecommerce": {
    "Stripe": {
      "PublishableKey": "pk_live_...",
      "SecretKey": "sk_live_..."
    }
  }
}
```

---

## Related Documentation

- [Installation Guide](./installation.md)
- [Quick Start Tutorial](./quick-start.md)
- [Payment Gateways](../user-guide/payments.md)
