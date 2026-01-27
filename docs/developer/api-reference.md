# API Reference

Complete API reference for Algora Commerce.

## Overview

Algora Commerce provides two API layers:

| API | Base URL | Authentication | Purpose |
|-----|----------|----------------|---------|
| **Storefront API** | `/api/ecommerce/` | Session/JWT | Customer-facing operations |
| **Management API** | `/umbraco/management/api/v1/ecommerce/` | Backoffice Auth | Admin operations |

## Authentication

### Storefront API

#### Session-Based (Default)

Cart operations use session cookies automatically.

```javascript
// Cart operations work without authentication
fetch('/api/ecommerce/cart')
  .then(res => res.json())
  .then(cart => console.log(cart));
```

#### Customer Authentication

For customer-specific operations:

```http
POST /api/ecommerce/auth/login
Content-Type: application/json

{
  "email": "customer@example.com",
  "password": "securepassword"
}
```

Response:
```json
{
  "success": true,
  "customer": {
    "id": "guid",
    "email": "customer@example.com",
    "firstName": "John",
    "lastName": "Smith"
  },
  "token": "eyJhbGciOiJIUzI1NiIs..."
}
```

### Management API

Requires Umbraco backoffice authentication.

```javascript
// Include Umbraco auth token
fetch('/umbraco/management/api/v1/ecommerce/products', {
  headers: {
    'Authorization': 'Bearer ' + umbracoToken
  }
});
```

---

## Storefront API

### Products

#### List Products

```http
GET /api/ecommerce/products
```

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `page` | int | Page number (default: 1) |
| `pageSize` | int | Items per page (default: 20, max: 100) |
| `search` | string | Search term |
| `categoryId` | guid | Filter by category |
| `brand` | string | Filter by brand |
| `minPrice` | decimal | Minimum price |
| `maxPrice` | decimal | Maximum price |
| `inStock` | bool | Only in-stock items |
| `onSale` | bool | Only sale items |
| `sortBy` | string | `newest`, `price_asc`, `price_desc`, `name` |

**Response:**
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Classic T-Shirt",
      "slug": "classic-t-shirt",
      "sku": "TSH-001",
      "shortDescription": "Premium cotton t-shirt",
      "basePrice": 29.99,
      "salePrice": 24.99,
      "currentPrice": 24.99,
      "isOnSale": true,
      "isInStock": true,
      "stockQuantity": 150,
      "primaryImageUrl": "/media/products/tshirt-blue.jpg",
      "hasVariants": true,
      "variantCount": 12,
      "rating": 4.5,
      "reviewCount": 23
    }
  ],
  "totalCount": 156,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8
}
```

#### Get Product

```http
GET /api/ecommerce/products/{id}
```

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Classic T-Shirt",
  "slug": "classic-t-shirt",
  "sku": "TSH-001",
  "shortDescription": "Premium cotton t-shirt",
  "description": "<p>Full HTML description...</p>",
  "basePrice": 29.99,
  "salePrice": 24.99,
  "costPrice": 12.00,
  "currentPrice": 24.99,
  "currencyCode": "USD",
  "taxIncluded": false,
  "isOnSale": true,
  "isInStock": true,
  "stockQuantity": 150,
  "lowStockThreshold": 10,
  "allowBackorders": false,
  "weight": 0.3,
  "length": 30,
  "width": 25,
  "height": 2,
  "images": [
    {
      "id": "guid",
      "url": "/media/products/tshirt-blue.jpg",
      "altText": "Blue t-shirt front view",
      "isPrimary": true
    }
  ],
  "categories": [
    { "id": "guid", "name": "Apparel", "slug": "apparel" }
  ],
  "tags": ["cotton", "casual", "summer"],
  "brand": "MyBrand",
  "hasVariants": true,
  "variants": [
    {
      "id": "guid",
      "sku": "TSH-001-BLU-S",
      "name": "Blue / Small",
      "options": { "Color": "Blue", "Size": "Small" },
      "price": 29.99,
      "salePrice": 24.99,
      "stockQuantity": 25,
      "imageUrl": "/media/products/tshirt-blue.jpg",
      "isDefault": false
    }
  ],
  "attributes": [
    { "name": "Material", "value": "100% Cotton" },
    { "name": "Care", "value": "Machine wash cold" }
  ],
  "rating": 4.5,
  "reviewCount": 23,
  "isFeatured": true,
  "status": "Published",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-27T14:22:00Z"
}
```

#### Get Product by Slug

```http
GET /api/ecommerce/products/slug/{slug}
```

---

### Categories

#### List Categories

```http
GET /api/ecommerce/categories
```

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `parentId` | guid | Get children of parent |
| `flat` | bool | Return flat list vs tree |
| `includeCount` | bool | Include product counts |

**Response:**
```json
{
  "items": [
    {
      "id": "guid",
      "name": "Apparel",
      "slug": "apparel",
      "description": "Clothing and accessories",
      "imageUrl": "/media/categories/apparel.jpg",
      "parentId": null,
      "productCount": 45,
      "children": [
        {
          "id": "guid",
          "name": "T-Shirts",
          "slug": "t-shirts",
          "parentId": "parent-guid",
          "productCount": 23
        }
      ]
    }
  ]
}
```

---

### Cart

#### Get Cart

```http
GET /api/ecommerce/cart
```

**Response:**
```json
{
  "id": "guid",
  "customerId": null,
  "sessionId": "abc123",
  "currencyCode": "USD",
  "items": [
    {
      "id": "guid",
      "productId": "guid",
      "variantId": "guid",
      "productName": "Classic T-Shirt",
      "sku": "TSH-001-BLU-S",
      "variantName": "Blue / Small",
      "variantOptions": { "Color": "Blue", "Size": "Small" },
      "imageUrl": "/media/products/tshirt-blue.jpg",
      "quantity": 2,
      "unitPrice": 24.99,
      "lineTotal": 49.98,
      "discountAmount": 0
    }
  ],
  "subtotal": 49.98,
  "discountTotal": 0,
  "shippingTotal": 0,
  "taxTotal": 0,
  "grandTotal": 49.98,
  "appliedDiscounts": [],
  "couponCode": null,
  "itemCount": 2,
  "isEmpty": false
}
```

#### Add Item to Cart

```http
POST /api/ecommerce/cart/items
Content-Type: application/json

{
  "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "variantId": "variant-guid",
  "quantity": 2
}
```

**Response:** Updated cart object

#### Update Item Quantity

```http
PUT /api/ecommerce/cart/items/{itemId}
Content-Type: application/json

{
  "quantity": 3
}
```

#### Remove Item

```http
DELETE /api/ecommerce/cart/items/{itemId}
```

#### Apply Coupon

```http
POST /api/ecommerce/cart/coupon
Content-Type: application/json

{
  "code": "SAVE20"
}
```

**Success Response:**
```json
{
  "success": true,
  "cart": { /* updated cart */ },
  "discount": {
    "code": "SAVE20",
    "name": "20% Off Everything",
    "type": "Percentage",
    "value": 20,
    "discountAmount": 10.00
  }
}
```

**Error Response:**
```json
{
  "success": false,
  "error": "INVALID_CODE",
  "message": "This coupon code is invalid or expired"
}
```

#### Get Shipping Options

```http
GET /api/ecommerce/cart/shipping-options
```

Requires shipping address to be set.

**Response:**
```json
{
  "options": [
    {
      "id": "standard",
      "name": "Standard Shipping",
      "description": "5-7 business days",
      "rate": 5.99,
      "estimatedDays": { "min": 5, "max": 7 }
    },
    {
      "id": "express",
      "name": "Express Shipping",
      "description": "2-3 business days",
      "rate": 12.99,
      "estimatedDays": { "min": 2, "max": 3 }
    }
  ]
}
```

---

### Checkout

#### Initialize Checkout

```http
POST /api/ecommerce/checkout/initialize
```

Creates a checkout session from the current cart.

**Response:**
```json
{
  "sessionId": "chk_abc123",
  "cart": { /* cart object */ },
  "expiresAt": "2024-01-27T16:00:00Z"
}
```

#### Update Shipping Address

```http
PUT /api/ecommerce/checkout/{sessionId}/shipping-address
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Smith",
  "company": null,
  "addressLine1": "123 Main Street",
  "addressLine2": "Apt 4B",
  "city": "New York",
  "stateProvince": "NY",
  "postalCode": "10001",
  "countryCode": "US",
  "phone": "+15551234567"
}
```

#### Set Shipping Method

```http
PUT /api/ecommerce/checkout/{sessionId}/shipping-method
Content-Type: application/json

{
  "shippingMethodId": "express"
}
```

#### Create Payment Intent

```http
POST /api/ecommerce/checkout/{sessionId}/payment-intent
```

Creates a Stripe Payment Intent.

**Response:**
```json
{
  "clientSecret": "pi_xxx_secret_xxx",
  "paymentIntentId": "pi_xxx",
  "amount": 14475,
  "currency": "usd"
}
```

#### Complete Checkout

```http
POST /api/ecommerce/checkout/{sessionId}/complete
Content-Type: application/json

{
  "paymentIntentId": "pi_xxx",
  "billingAddress": {
    "sameAsShipping": true
  },
  "customerNote": "Please leave at door",
  "acceptTerms": true
}
```

**Response:**
```json
{
  "success": true,
  "order": {
    "id": "guid",
    "orderNumber": "ORD-2024-0142",
    "status": "Confirmed",
    "grandTotal": 144.75,
    "createdAt": "2024-01-27T14:35:00Z"
  }
}
```

---

### Orders (Customer)

#### List My Orders

```http
GET /api/ecommerce/orders
Authorization: Bearer {token}
```

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `page` | int | Page number |
| `pageSize` | int | Items per page |
| `status` | string | Filter by status |

**Response:**
```json
{
  "items": [
    {
      "id": "guid",
      "orderNumber": "ORD-2024-0142",
      "status": "Shipped",
      "paymentStatus": "Captured",
      "fulfillmentStatus": "Fulfilled",
      "itemCount": 3,
      "grandTotal": 144.75,
      "createdAt": "2024-01-27T14:35:00Z",
      "trackingNumber": "789456123",
      "trackingUrl": "https://fedex.com/track/789456123"
    }
  ],
  "totalCount": 12,
  "page": 1,
  "pageSize": 10
}
```

#### Get Order Details

```http
GET /api/ecommerce/orders/{id}
Authorization: Bearer {token}
```

---

### Customer Authentication

#### Register

```http
POST /api/ecommerce/auth/register
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Smith",
  "acceptMarketing": true
}
```

#### Login

```http
POST /api/ecommerce/auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "SecurePass123!"
}
```

#### Logout

```http
POST /api/ecommerce/auth/logout
```

#### Forgot Password

```http
POST /api/ecommerce/auth/forgot-password
Content-Type: application/json

{
  "email": "john@example.com"
}
```

#### Reset Password

```http
POST /api/ecommerce/auth/reset-password
Content-Type: application/json

{
  "token": "reset-token-from-email",
  "password": "NewSecurePass123!"
}
```

---

## Management API

### Products Management

#### List Products

```http
GET /umbraco/management/api/v1/ecommerce/products
```

#### Create Product

```http
POST /umbraco/management/api/v1/ecommerce/products
Content-Type: application/json

{
  "name": "New Product",
  "sku": "NEW-001",
  "basePrice": 49.99,
  "description": "Product description",
  "categoryIds": ["guid"],
  "status": "Draft"
}
```

#### Update Product

```http
PUT /umbraco/management/api/v1/ecommerce/products/{id}
```

#### Delete Product

```http
DELETE /umbraco/management/api/v1/ecommerce/products/{id}
```

### Orders Management

#### List Orders

```http
GET /umbraco/management/api/v1/ecommerce/orders
```

#### Update Order Status

```http
PUT /umbraco/management/api/v1/ecommerce/orders/{id}/status
Content-Type: application/json

{
  "status": "Processing"
}
```

#### Create Shipment

```http
POST /umbraco/management/api/v1/ecommerce/orders/{id}/shipments
Content-Type: application/json

{
  "carrier": "FedEx",
  "trackingNumber": "789456123",
  "items": [
    { "orderLineId": "guid", "quantity": 2 }
  ],
  "notifyCustomer": true
}
```

#### Process Refund

```http
POST /umbraco/management/api/v1/ecommerce/orders/{id}/refund
Content-Type: application/json

{
  "amount": 50.00,
  "reason": "Customer requested",
  "notifyCustomer": true
}
```

---

## Error Responses

All errors follow this format:

```json
{
  "error": "ERROR_CODE",
  "message": "Human readable message",
  "details": { /* optional additional info */ }
}
```

### Common Error Codes

| Code | HTTP Status | Description |
|------|-------------|-------------|
| `NOT_FOUND` | 404 | Resource not found |
| `VALIDATION_ERROR` | 400 | Invalid request data |
| `UNAUTHORIZED` | 401 | Authentication required |
| `FORBIDDEN` | 403 | Permission denied |
| `INSUFFICIENT_STOCK` | 400 | Not enough inventory |
| `INVALID_COUPON` | 400 | Coupon code invalid |
| `PAYMENT_FAILED` | 400 | Payment processing failed |
| `CART_EMPTY` | 400 | Cart has no items |

---

## Rate Limiting

API requests are rate limited:

| API | Limit | Window |
|-----|-------|--------|
| Storefront API | 100 requests | per minute |
| Management API | 300 requests | per minute |

Rate limit headers:
```http
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1706367600
```

---

## Webhooks

Subscribe to events via Settings > Webhooks:

| Event | Description |
|-------|-------------|
| `order.created` | New order placed |
| `order.updated` | Order status changed |
| `order.completed` | Order completed |
| `order.cancelled` | Order cancelled |
| `payment.captured` | Payment successful |
| `payment.failed` | Payment failed |
| `payment.refunded` | Refund processed |
| `product.created` | Product created |
| `product.updated` | Product updated |
| `customer.created` | New customer registered |

Webhook payload:
```json
{
  "event": "order.created",
  "timestamp": "2024-01-27T14:35:00Z",
  "data": {
    "orderId": "guid",
    "orderNumber": "ORD-2024-0142",
    /* event-specific data */
  }
}
```

---

## SDK Examples

### JavaScript

```javascript
// Using the built-in cart helper
const cart = window.ecommerceCart;

// Add to cart
await cart.addItem('product-id', 'variant-id', 2);

// Update quantity
await cart.updateQuantity('item-id', 3);

// Apply coupon
await cart.applyCoupon('SAVE20');

// Subscribe to cart updates
cart.subscribe(updatedCart => {
  console.log('Cart updated:', updatedCart.itemCount);
});
```

### C# (.NET)

```csharp
// Inject services
public class MyController : Controller
{
    private readonly ICartService _cartService;
    private readonly IProductService _productService;

    public async Task<IActionResult> AddToCart(Guid productId, int qty)
    {
        var cart = await _cartService.AddItemAsync(new AddToCartRequest
        {
            ProductId = productId,
            Quantity = qty
        });

        return Ok(cart);
    }
}
```

---

## Related Documentation

- [Architecture Overview](./architecture.md)
- [Extending the Plugin](./extending.md)
- [Webhooks & Events](./webhooks.md)
