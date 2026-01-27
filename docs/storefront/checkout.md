# Cart & Checkout

Complete guide to the storefront shopping cart and checkout experience.

## Shopping Cart

### Cart Features

The Algora Commerce cart provides:

- **Persistent Cart**: Cart saved across sessions
- **Guest Support**: Shop without account
- **Real-time Updates**: Instant price calculations
- **Stock Validation**: Check availability before checkout
- **Coupon Codes**: Apply discounts
- **Shipping Estimates**: Preview shipping costs

### Cart Display

![Shopping Cart](../screenshots/cart.png)

#### Mini Cart (Header)

Quick cart access from any page:
- Item count badge
- Dropdown with recent items
- Quick remove items
- Proceed to checkout button

#### Full Cart Page

Accessed at `/cart`:

```
┌─────────────────────────────────────────────────────────┐
│ Shopping Cart (3 items)                                 │
├─────────────────────────────────────────────────────────┤
│ [Image] Classic T-Shirt - Blue/L                        │
│         $29.99 × 2 = $59.98              [−] 2 [+]  [×] │
├─────────────────────────────────────────────────────────┤
│ [Image] Premium Jeans - 32x30                           │
│         $89.99 × 1 = $89.99              [−] 1 [+]  [×] │
├─────────────────────────────────────────────────────────┤
│ Coupon Code: [___________] [Apply]                      │
├─────────────────────────────────────────────────────────┤
│                              Subtotal:         $149.97  │
│                              Discount (SAVE20): -$30.00 │
│                              Shipping:    Calculated... │
│                              ─────────────────────────  │
│                              Total:           $119.97+  │
│                                                         │
│                              [Continue Shopping]        │
│                              [Proceed to Checkout →]    │
└─────────────────────────────────────────────────────────┘
```

### Cart Operations

| Action | Description |
|--------|-------------|
| **Add Item** | Add product with quantity |
| **Update Quantity** | Change item quantity |
| **Remove Item** | Remove from cart |
| **Apply Coupon** | Enter discount code |
| **Remove Coupon** | Remove applied discount |
| **Save for Later** | Move to wishlist |
| **Clear Cart** | Remove all items |

### Cart API Endpoints

```
GET  /api/ecommerce/cart           Get current cart
POST /api/ecommerce/cart/items     Add item to cart
PUT  /api/ecommerce/cart/items/:id Update item quantity
DEL  /api/ecommerce/cart/items/:id Remove item
POST /api/ecommerce/cart/coupon    Apply coupon code
DEL  /api/ecommerce/cart/coupon    Remove coupon
```

---

## Checkout Process

### Checkout Types

Algora Commerce supports multiple checkout flows:

| Type | Description | Best For |
|------|-------------|----------|
| **Multi-Step** | Separate pages for each step | Complex orders |
| **Single-Page** | All steps on one page | Quick checkout |
| **Express** | One-click for returning customers | Repeat buyers |

### Checkout Steps

#### Step 1: Contact Information

![Checkout Contact](../screenshots/checkout-contact.png)

**Guest Checkout:**
- Email address (required)
- Option to create account

**Logged-in Customer:**
- Pre-filled from account
- Option to use different email

#### Step 2: Shipping Address

![Checkout Shipping](../screenshots/checkout-shipping.png)

| Field | Required | Notes |
|-------|----------|-------|
| First Name | Yes | |
| Last Name | Yes | |
| Company | No | B2B orders |
| Address Line 1 | Yes | |
| Address Line 2 | No | Apt, Suite, etc. |
| City | Yes | |
| State/Province | Depends | Based on country |
| Postal Code | Yes | |
| Country | Yes | Dropdown |
| Phone | Optional | For delivery |

**Features:**
- Address autocomplete (Google Places)
- Address validation
- Save address to account
- Select from saved addresses

#### Step 3: Shipping Method

![Checkout Shipping Method](../screenshots/checkout-shipping-method.png)

Available methods based on address:

```
○ Standard Shipping (5-7 business days)        $5.99
● Express Shipping (2-3 business days)        $12.99
○ Next Day Delivery (order by 2pm)            $24.99
○ Free Shipping (7-10 business days)          FREE
  Orders over $50 qualify
```

**Shows:**
- Method name and description
- Estimated delivery date
- Price (or FREE if qualified)

#### Step 4: Payment

![Checkout Payment](../screenshots/checkout-payment.png)

**Supported Payment Methods:**

| Method | Description |
|--------|-------------|
| **Credit Card** | Visa, Mastercard, Amex via Stripe |
| **PayPal** | Redirect to PayPal |
| **Apple Pay** | Safari on Apple devices |
| **Google Pay** | Chrome on Android |
| **Razorpay** | UPI, NetBanking (India) |
| **Store Credit** | Apply account balance |
| **Gift Card** | Enter gift card code |

**Credit Card Form:**
```
Card Number:    [4242 4242 4242 4242]
Expiry:         [12/28]
CVC:            [123]
Name on Card:   [John Smith]

☑ Save card for future purchases
```

**Security Features:**
- PCI-compliant (card data never touches server)
- 3D Secure authentication
- CVV verification
- Address verification (AVS)

#### Step 5: Order Review

![Checkout Review](../screenshots/checkout-review.png)

Final review before placing order:

```
┌─────────────────────────────────────────────────────────┐
│ Order Summary                                           │
├─────────────────────────────────────────────────────────┤
│ Classic T-Shirt - Blue/L × 2                   $59.98  │
│ Premium Jeans - 32x30 × 1                      $89.99  │
├─────────────────────────────────────────────────────────┤
│ Subtotal                                      $149.97  │
│ Discount (SAVE20)                             -$30.00  │
│ Shipping (Express)                             $12.99  │
│ Tax (8.875%)                                   $11.79  │
├─────────────────────────────────────────────────────────┤
│ TOTAL                                         $144.75  │
├─────────────────────────────────────────────────────────┤
│ Ship to: John Smith                                     │
│          123 Main St, New York, NY 10001               │
│                                                         │
│ Pay with: Visa ending in 4242                          │
├─────────────────────────────────────────────────────────┤
│ ☑ I agree to the Terms & Conditions                    │
│                                                         │
│        [← Back]              [Place Order →]           │
└─────────────────────────────────────────────────────────┘
```

---

## Order Confirmation

After successful payment:

![Order Confirmation](../screenshots/order-confirmation.png)

### Confirmation Page Shows:

- Order number
- Order summary
- Shipping address
- Estimated delivery
- Payment confirmation
- "Create Account" prompt (guests)
- "Continue Shopping" button
- Print/email options

### Confirmation Email

Sent automatically with:

- Order details
- Shipping information
- Expected delivery
- Tracking link (when available)
- Return policy
- Support contact

---

## Guest vs. Account Checkout

### Guest Checkout

| Pros | Cons |
|------|------|
| Faster first purchase | Must enter info each time |
| No password required | No order history |
| Lower friction | No saved addresses |
| | No saved payment methods |

### Account Checkout

| Pros | Cons |
|------|------|
| Saved addresses | Requires registration |
| Saved payment methods | Password management |
| Order history | |
| Wishlist access | |
| Loyalty points | |
| Faster repeat orders | |

### Post-Purchase Account Creation

Guests can create account after checkout:

1. Click "Create Account" on confirmation
2. Enter password (email already captured)
3. Order linked to new account
4. Future orders have history

---

## Checkout Customization

### Step Configuration

In backoffice: **Commerce > Settings > Checkout**

| Setting | Options |
|---------|---------|
| **Checkout Type** | Multi-step, Single-page |
| **Guest Checkout** | Enabled, Disabled, Optional |
| **Account Creation** | Required, Optional, Disabled |
| **Address Validation** | Enabled, Disabled |
| **Phone Required** | Yes, No |
| **Order Notes** | Enabled, Disabled |
| **Gift Options** | Enabled, Disabled |

### Custom Checkout Steps

Add custom steps via document types:

1. Create document type with alias `checkoutStep*`
2. Define step properties
3. Step appears in checkout flow

Example: Gift Message Step
```csharp
public class GiftMessageCheckoutStep : ICheckoutStepProvider
{
    public string Alias => "checkoutStepGiftMessage";
    public string Name => "Gift Message";
    public int SortOrder => 25; // After shipping
}
```

---

## Error Handling

### Stock Validation

If item becomes unavailable:

```
⚠ Sorry, "Classic T-Shirt - Blue/L" is now out of stock.

  [Remove from cart] or [Choose different variant]
```

### Payment Failures

```
❌ Payment failed: Card declined

Please check your card details or try a different payment method.

[Try Again] [Use Different Card]
```

### Address Validation

```
⚠ We couldn't validate this address:

  123 Main Stret, New York, NY 10001

  Did you mean:
  ○ 123 Main Street, New York, NY 10001
  ○ Use address as entered
```

---

## Performance Features

### Cart Performance

- Local storage caching
- Debounced quantity updates
- Optimistic UI updates
- Background stock checks

### Checkout Optimization

- Field-level validation (instant feedback)
- Address autocomplete
- Saved payment methods
- One-click checkout for returning customers

---

## Analytics & Tracking

Built-in analytics events:

| Event | Description |
|-------|-------------|
| `add_to_cart` | Item added to cart |
| `remove_from_cart` | Item removed |
| `begin_checkout` | Checkout started |
| `add_shipping_info` | Shipping address entered |
| `add_payment_info` | Payment method selected |
| `purchase` | Order completed |

### Google Analytics 4 Integration

Automatic event tracking with:
- Enhanced ecommerce
- Product impressions
- Checkout funnel
- Purchase conversion

---

## Related Documentation

- [Storefront Overview](./overview.md)
- [Customer Accounts](./customer-accounts.md)
- [Payment Gateways](../user-guide/payments.md)
- [Shipping Configuration](../user-guide/shipping.md)
- [API: Cart Endpoints](../developer/api-reference.md#cart)
- [API: Checkout Endpoints](../developer/api-reference.md#checkout)
