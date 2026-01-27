# Screenshot Capture Guide

Step-by-step instructions for capturing all documentation screenshots.

## Prerequisites

Ensure these servers are running:

```bash
# Terminal 1: License Portal
cd src/UAlgora.Ecommerce.LicensePortal
dotnet run
# Running at http://localhost:5028

# Terminal 2: Main Site (for backoffice)
cd src/UAlgora.Ecommerce.Site
dotnet run
# Running at http://localhost:5000

# Terminal 3: Documentation
cd docs
npx docsify-cli serve . --port 3000
# Running at http://localhost:3000
```

## Capture Settings

- **Browser**: Chrome (Incognito mode recommended)
- **Resolution**: 1920x1080 or browser window at 1400x900
- **Format**: PNG
- **Tool**: Windows Snipping Tool (`Win+Shift+S`) or browser DevTools

---

## License Portal Screenshots

### 1. license-pricing.png
**URL**: http://localhost:5028/

**Steps**:
1. Open URL in browser
2. Scroll to see all three pricing tiers (Trial, Standard, Enterprise)
3. Capture the full pricing section with cards

**What to capture**: Hero section with "Simple, Transparent Pricing" and all 3 pricing cards

---

### 2. license-checkout.png
**URL**: http://localhost:5028/checkout/standard

**Steps**:
1. Open URL in browser
2. Capture the checkout form showing Stripe and Razorpay options

**What to capture**: Full checkout page with payment options visible

---

### 3. license-login.png
**URL**: http://localhost:5028/Account/Login

**Steps**:
1. Open URL in browser
2. Capture the email login form

**What to capture**: Login form with email input and "Send Login Code" button

---

## Backoffice Screenshots

### 4. backoffice-section.png
**URL**: http://localhost:5000/umbraco

**Steps**:
1. Log in to Umbraco backoffice
2. Look at the left sidebar
3. Capture showing the "Commerce" section icon

**What to capture**: Left sidebar with Commerce section highlighted

---

### 5. dashboard.png
**URL**: http://localhost:5000/umbraco#/commerce/dashboard

**Steps**:
1. Navigate to Commerce > Dashboard
2. Capture the full dashboard with stats

**What to capture**: Sales overview, recent orders, key metrics

---

### 6. product-list.png
**URL**: http://localhost:5000/umbraco#/commerce/products

**Steps**:
1. Navigate to Commerce > Products
2. Ensure some products exist (use seed data)
3. Capture the product list view

**What to capture**: Product grid/list with columns (Image, Name, SKU, Price, Stock, Status)

---

### 7. product-create.png
**URL**: http://localhost:5000/umbraco#/commerce/products/create

**Steps**:
1. Click "Create Product" button
2. Capture the empty product form

**What to capture**: Product editor with Basic Information tab

---

### 8. product-pricing.png
**URL**: Product editor > Pricing tab

**Steps**:
1. Open any product
2. Navigate to Pricing tab
3. Fill in sample prices

**What to capture**: Pricing fields (Base Price, Sale Price, Cost Price)

---

### 9. product-inventory.png
**URL**: Product editor > Inventory tab

**Steps**:
1. Open any product
2. Navigate to Inventory tab

**What to capture**: Stock settings (Track Inventory, Stock Qty, Low Stock Threshold)

---

### 10. product-variants.png
**URL**: Product editor > Variants tab

**Steps**:
1. Open a product with variants (e.g., T-Shirt)
2. Navigate to Variants tab
3. Show variant grid

**What to capture**: Variant list with options (Size, Color combinations)

---

### 11. order-list.png
**URL**: http://localhost:5000/umbraco#/commerce/orders

**Steps**:
1. Navigate to Commerce > Orders
2. Ensure some orders exist

**What to capture**: Order list with columns (Order #, Date, Customer, Total, Status)

---

### 12. order-details.png
**URL**: Click on any order

**Steps**:
1. Click an order from the list
2. Capture the full order details view

**What to capture**: Order info, line items, addresses, payment status, timeline

---

### 13. create-shipment.png
**URL**: Order details > Create Shipment

**Steps**:
1. Open an order in "Processing" status
2. Click "Create Shipment" button
3. Capture the shipment dialog

**What to capture**: Carrier dropdown, tracking number input, item selection

---

### 14. refund-order.png
**URL**: Order details > Refund

**Steps**:
1. Open a paid order
2. Click "Refund" button
3. Capture the refund dialog

**What to capture**: Refund amount input, reason selection, notification checkbox

---

## Storefront Screenshots

### 15. storefront.png
**URL**: http://localhost:5000/products/{any-product-slug}

**Steps**:
1. Navigate to a product page on the storefront
2. Capture the product display

**What to capture**: Product image, title, price, add to cart button, description

---

### 16. cart.png
**URL**: http://localhost:5000/cart

**Steps**:
1. Add some items to cart
2. Navigate to cart page

**What to capture**: Cart items, quantities, subtotal, checkout button

---

### 17. checkout-contact.png
**URL**: http://localhost:5000/checkout (Step 1)

**Steps**:
1. Start checkout process
2. Capture the contact/email step

**What to capture**: Email input, account options

---

### 18. checkout-shipping.png
**URL**: http://localhost:5000/checkout (Step 2)

**Steps**:
1. Proceed to shipping address step
2. Fill in sample address

**What to capture**: Address form fields

---

### 19. checkout-shipping-method.png
**URL**: http://localhost:5000/checkout (Step 3)

**Steps**:
1. Proceed to shipping method step
2. Show available options

**What to capture**: Shipping method options with prices

---

### 20. checkout-payment.png
**URL**: http://localhost:5000/checkout (Step 4)

**Steps**:
1. Proceed to payment step
2. Show Stripe card form

**What to capture**: Credit card input form

---

### 21. checkout-review.png
**URL**: http://localhost:5000/checkout (Step 5)

**Steps**:
1. Proceed to review step
2. Capture order summary

**What to capture**: Order summary, addresses, totals, place order button

---

### 22. order-confirmation.png
**URL**: http://localhost:5000/checkout/success

**Steps**:
1. Complete a test order (use Stripe test card 4242424242424242)
2. Capture confirmation page

**What to capture**: Order number, thank you message, order summary

---

## Quick Reference

| Filename | Location | Priority |
|----------|----------|----------|
| license-pricing.png | License Portal | High |
| license-checkout.png | License Portal | High |
| dashboard.png | Backoffice | High |
| product-list.png | Backoffice | High |
| order-list.png | Backoffice | High |
| cart.png | Storefront | Medium |
| checkout-payment.png | Storefront | Medium |
| product-variants.png | Backoffice | Medium |
| order-details.png | Backoffice | Medium |

---

## After Capturing

1. Save all screenshots to `docs/screenshots/` folder
2. Verify filenames match exactly (lowercase, hyphens)
3. Commit the screenshots:

```bash
git add docs/screenshots/*.png
git commit -m "Add documentation screenshots"
git push
```

The documentation will automatically display the images once they're in place.
