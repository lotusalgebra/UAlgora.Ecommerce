# Order Management

Complete guide to managing customer orders in Algora Commerce.

## Accessing Orders

1. Log in to Umbraco backoffice
2. Click **Commerce** in the sidebar
3. Click **Orders** in the tree

![Order List](../screenshots/order-list.png)

## Order List View

The order list displays all orders with:

| Column | Description |
|--------|-------------|
| Order # | Unique order number (e.g., ORD-2024-0001) |
| Date | Order creation date/time |
| Customer | Customer name and email |
| Items | Number of line items |
| Total | Order grand total |
| Payment | Payment status indicator |
| Fulfillment | Shipping/fulfillment status |
| Status | Overall order status |

### Filtering Orders

Use filters to find specific orders:

- **Search**: Order number, customer name, email
- **Date Range**: Filter by order date
- **Status**: Pending, Confirmed, Processing, Shipped, Completed, Cancelled
- **Payment Status**: Pending, Authorized, Captured, Refunded
- **Fulfillment**: Unfulfilled, Partially Fulfilled, Fulfilled

### Quick Stats

The order dashboard shows:

```
Today's Orders: 15        Revenue: $2,450.00
Pending: 3                Processing: 5
Ready to Ship: 7          Completed: 0
```

---

## Order Statuses

### Overall Order Status

| Status | Description | Next Steps |
|--------|-------------|------------|
| **Pending** | Order placed, awaiting payment | Wait for payment confirmation |
| **Confirmed** | Payment received | Begin processing |
| **Processing** | Order being prepared | Pick, pack, create shipment |
| **Shipped** | Handed to carrier | Awaiting delivery |
| **Delivered** | Confirmed delivered | Mark complete or await return window |
| **Completed** | Order finalized | Archived |
| **Cancelled** | Order cancelled | Process refund if needed |
| **On Hold** | Temporarily paused | Review issue, then continue |
| **Refunded** | Full refund issued | Closed |

### Payment Status

| Status | Description |
|--------|-------------|
| **Pending** | Awaiting payment |
| **Authorized** | Payment authorized, not captured |
| **Captured** | Payment collected |
| **Partially Refunded** | Partial refund issued |
| **Refunded** | Full refund issued |
| **Failed** | Payment failed |
| **Voided** | Authorization cancelled |

### Fulfillment Status

| Status | Description |
|--------|-------------|
| **Unfulfilled** | Nothing shipped yet |
| **Partially Fulfilled** | Some items shipped |
| **Fulfilled** | All items shipped |
| **Returned** | Items returned |

---

## Order Details View

Click any order to view full details.

![Order Details](../screenshots/order-details.png)

### Order Information Panel

```
Order: ORD-2024-0142
Date: January 27, 2024 at 2:35 PM
Status: Processing
Payment: Captured (Stripe)
```

### Customer Information

- Name and email
- Phone number
- Account link (if registered)
- Previous order count

### Addresses

**Shipping Address:**
```
John Smith
123 Main Street, Apt 4B
New York, NY 10001
United States
+1 (555) 123-4567
```

**Billing Address:**
```
Same as shipping
- or -
Different billing address details
```

### Line Items

| Product | SKU | Variant | Qty | Price | Total |
|---------|-----|---------|-----|-------|-------|
| Classic T-Shirt | TSH-001 | Blue/Large | 2 | $29.99 | $59.98 |
| Premium Jeans | JNS-005 | 32x30 | 1 | $89.99 | $89.99 |

### Order Summary

```
Subtotal:                 $149.97
Discount (SAVE20):        -$30.00
Shipping (Standard):       $5.99
Tax (8.875%):             $10.65
─────────────────────────────────
Grand Total:             $136.61
```

### Timeline / Activity Log

```
Jan 27, 2:35 PM   Order placed
Jan 27, 2:35 PM   Payment authorized (Stripe pi_xxx)
Jan 27, 2:36 PM   Payment captured ($136.61)
Jan 27, 3:00 PM   Status changed to Processing by Admin
Jan 27, 4:15 PM   Shipment created: FedEx 789456123
```

---

## Order Actions

### Toolbar Actions

| Action | Description |
|--------|-------------|
| **Print Order** | Generate printable order summary |
| **Print Packing Slip** | Generate packing slip for warehouse |
| **Print Invoice** | Generate customer invoice |
| **Email Customer** | Send email to customer |
| **Duplicate** | Create new order with same items |

### Status Actions

| Action | When Available | Effect |
|--------|----------------|--------|
| **Confirm Order** | Pending orders | Marks payment confirmed |
| **Mark Processing** | Confirmed orders | Begins fulfillment |
| **Create Shipment** | Processing orders | Opens shipment dialog |
| **Mark Shipped** | Has shipment | Updates to Shipped |
| **Mark Delivered** | Shipped orders | Updates to Delivered |
| **Complete Order** | Delivered orders | Finalizes order |
| **Cancel Order** | Before shipped | Cancels and optionally refunds |
| **Hold Order** | Any active order | Pauses processing |

---

## Creating Shipments

When an order is ready to ship:

### Step 1: Click "Create Shipment"

![Create Shipment](../screenshots/create-shipment.png)

### Step 2: Enter Shipment Details

| Field | Description |
|-------|-------------|
| **Carrier** | Select shipping carrier (FedEx, UPS, etc.) |
| **Tracking Number** | Enter carrier tracking number |
| **Shipped Date** | Date shipped (default today) |
| **Items** | Select which items are in this shipment |
| **Notes** | Internal notes about shipment |

### Step 3: Notify Customer

- Check **Send Notification** to email customer
- Tracking link included automatically

### Partial Shipments

For orders with multiple items:

1. Create first shipment with available items
2. Order status becomes "Partially Fulfilled"
3. Create additional shipments as items become available
4. Order becomes "Fulfilled" when all items shipped

---

## Processing Refunds

### Full Refund

1. Open order details
2. Click **Refund** in toolbar
3. Confirm refund amount
4. Select refund reason
5. Check **Notify Customer** if desired
6. Click **Process Refund**

![Process Refund](../screenshots/refund-order.png)

### Partial Refund

1. Open order details
2. Click **Refund**
3. Enter specific refund amount
4. Add note explaining partial refund
5. Process refund

### Refund to Store Credit

Instead of payment refund:

1. Click **Refund to Store Credit**
2. Amount added to customer's store credit balance
3. Customer can use on future purchases

---

## Order Notes

### Internal Notes

Add notes visible only to staff:

1. Click **Add Note** in the Notes section
2. Type your note
3. Click Save

Notes are timestamped with author.

### Customer-Visible Notes

For notes the customer should see:

1. Click **Add Customer Note**
2. Type message
3. Optionally email to customer

---

## Editing Orders

### What Can Be Edited

| Field | Editable When |
|-------|---------------|
| Shipping Address | Before shipped |
| Billing Address | Before shipped |
| Internal Notes | Always |
| Customer Notes | Always |
| Line Items | Before shipped |
| Discounts | Before shipped |

### Adding/Removing Items

Before an order ships, you can:

1. Click **Edit Order**
2. Add new line items
3. Remove existing items
4. Adjust quantities
5. Recalculate totals
6. Save changes

Note: Price adjustments may require additional payment or refund.

---

## Bulk Operations

### Bulk Status Update

1. Select multiple orders using checkboxes
2. Click **Bulk Actions**
3. Choose new status
4. Confirm action

### Bulk Print

1. Select orders
2. Click **Print**
3. Choose: Orders, Packing Slips, or Invoices
4. Print all selected

### Export Orders

1. Filter orders as needed
2. Click **Export**
3. Choose format (CSV, Excel, JSON)
4. Download file

---

## Order Automation

Configure automatic actions in Settings > Order Automation:

### Auto-Capture

```
When: Payment authorized
Then: Automatically capture payment
Delay: Immediate / 1 hour / Custom
```

### Auto-Fulfill (Digital Products)

```
When: Order contains only digital products
Then: Mark as fulfilled automatically
Send: Download links via email
```

### Auto-Archive

```
When: Order completed
After: 30 days
Then: Move to archive
```

### Low Stock Alert

```
When: Product stock falls below threshold
Then: Email notification to admin
```

---

## Fraud Prevention

### Risk Indicators

Orders are analyzed for fraud risk:

| Indicator | Risk Level | Description |
|-----------|------------|-------------|
| Address Mismatch | Medium | Billing/shipping in different countries |
| Multiple Orders | Low | Same customer, multiple orders today |
| High Value | Medium | Order exceeds typical value |
| Failed Attempts | High | Previous failed payment attempts |
| Proxy/VPN | Medium | Order placed via proxy |

### Manual Review

High-risk orders are automatically held:

1. Review order details
2. Verify customer information
3. Approve or Cancel order
4. Add notes about review

---

## Reports

Access order reports from **Commerce > Reports**:

| Report | Description |
|--------|-------------|
| **Sales Summary** | Revenue by period |
| **Orders by Status** | Order status breakdown |
| **Top Products** | Best-selling items |
| **Customer Orders** | Orders by customer |
| **Refunds** | Refund summary and reasons |
| **Shipping** | Shipments and carriers |

---

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+P` | Print order |
| `Ctrl+S` | Save changes |
| `N` | Add note |
| `E` | Edit order |
| `R` | Refund dialog |
| `?` | Show shortcuts |

---

## Related Documentation

- [Shipping Configuration](./shipping.md)
- [Payment Gateways](./payments.md)
- [Returns Management](./returns.md)
- [API: Orders Endpoint](../developer/api-reference.md#orders)
