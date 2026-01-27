# Product Management

This guide covers everything you need to know about managing products in Algora Commerce.

## Accessing Product Management

1. Log in to the Umbraco backoffice
2. Click **Commerce** in the left sidebar
3. Click **Products** in the tree

![Product List](../screenshots/product-list.png)

## Product List View

The product list provides a quick overview of all your products with:

| Column | Description |
|--------|-------------|
| Image | Product thumbnail |
| Name | Product name (click to edit) |
| SKU | Stock Keeping Unit |
| Price | Current selling price |
| Stock | Available inventory |
| Status | Published, Draft, or Archived |

### Filtering Products

Use the filter bar to narrow down products:

- **Search**: Type product name, SKU, or description
- **Category**: Filter by category
- **Status**: Published, Draft, Archived
- **Stock**: In Stock, Low Stock, Out of Stock
- **Price Range**: Min and max price

### Bulk Actions

Select multiple products using checkboxes, then:

- **Publish**: Make products visible on storefront
- **Archive**: Hide products without deleting
- **Delete**: Permanently remove (requires confirmation)
- **Update Price**: Bulk price adjustment
- **Update Stock**: Bulk inventory update

---

## Creating a Product

### Step 1: Basic Information

Click **Create Product** to open the product editor.

![Create Product](../screenshots/product-create.png)

| Field | Required | Description |
|-------|----------|-------------|
| **Name** | Yes | Product title shown to customers |
| **SKU** | Yes | Unique identifier for inventory |
| **Slug** | Auto | URL-friendly name (auto-generated) |
| **Short Description** | No | Brief summary (shown in listings) |
| **Description** | No | Full product description (rich text) |

### Step 2: Pricing

![Product Pricing](../screenshots/product-pricing.png)

| Field | Description |
|-------|-------------|
| **Base Price** | Regular selling price |
| **Sale Price** | Discounted price (optional) |
| **Cost Price** | Your cost (for profit tracking) |
| **Currency** | Price currency (default from settings) |
| **Tax Included** | Whether price includes tax |
| **Tax Class** | Tax category for calculations |

**Example Pricing Setup:**
```
Base Price: $99.99
Sale Price: $79.99 (20% off)
Cost Price: $45.00
Profit: $34.99 (44% margin)
```

### Step 3: Inventory

![Product Inventory](../screenshots/product-inventory.png)

| Field | Description |
|-------|-------------|
| **Track Inventory** | Enable stock tracking |
| **Stock Quantity** | Current available stock |
| **Low Stock Threshold** | Alert when stock falls below |
| **Allow Backorders** | Accept orders when out of stock |
| **Stock Status** | In Stock, Out of Stock, On Backorder, Pre-order |

### Step 4: Images

Upload multiple product images:

1. Click **Add Image**
2. Select from Media Library or upload new
3. Drag to reorder
4. Click star icon to set primary image

**Best Practices:**
- Use high-resolution images (min 1000x1000px)
- Maintain consistent aspect ratio
- Include multiple angles
- Show product in use

### Step 5: Organization

![Product Organization](../screenshots/product-organization.png)

| Field | Description |
|-------|-------------|
| **Categories** | Select one or more categories |
| **Tags** | Keywords for search and filtering |
| **Brand** | Product brand/manufacturer |
| **Featured** | Show on homepage featured section |

### Step 6: Physical Properties (for Shipping)

| Field | Description |
|-------|-------------|
| **Weight** | Product weight (kg or lb) |
| **Length** | Package length |
| **Width** | Package width |
| **Height** | Package height |

---

## Product Variants

Variants allow you to sell different versions of a product (e.g., sizes, colors).

### Creating Variants

1. Enable **Has Variants** toggle
2. Define **Attributes** (e.g., Size, Color)
3. Add **Options** for each attribute
4. Generate variant combinations

![Product Variants](../screenshots/product-variants.png)

**Example Variant Setup:**

```
Product: Classic T-Shirt
Attributes:
  - Size: S, M, L, XL
  - Color: Black, White, Navy

Generated Variants (12 total):
  - Classic T-Shirt - S/Black
  - Classic T-Shirt - S/White
  - Classic T-Shirt - S/Navy
  - Classic T-Shirt - M/Black
  ... and so on
```

### Variant-Specific Settings

Each variant can have its own:

| Setting | Description |
|---------|-------------|
| **SKU** | Unique SKU per variant |
| **Price** | Override base price |
| **Sale Price** | Variant-specific sale |
| **Stock** | Individual stock tracking |
| **Image** | Variant-specific image |
| **Weight** | Different shipping weight |

### Managing Variants

- **Enable/Disable**: Toggle variant availability
- **Set Default**: Choose default variant shown
- **Reorder**: Drag to change display order
- **Bulk Edit**: Update multiple variants at once

---

## Product Status

| Status | Description | Visible on Storefront |
|--------|-------------|----------------------|
| **Draft** | Work in progress | No |
| **Published** | Live and available | Yes |
| **Archived** | Discontinued but retained | No |

### Status Workflow

```
Draft → Published → Archived
         ↓
      Published (can revert)
```

---

## SEO Settings

Optimize products for search engines:

![Product SEO](../screenshots/product-seo.png)

| Field | Description | Best Practice |
|-------|-------------|---------------|
| **Meta Title** | Search result title | Include product name + key feature |
| **Meta Description** | Search result snippet | 150-160 characters, include keywords |
| **URL Slug** | Page URL | Use hyphens, lowercase, descriptive |

**Example:**
```
Meta Title: Classic Cotton T-Shirt - Soft & Comfortable | MyStore
Meta Description: Premium 100% cotton t-shirt available in 4 colors.
                  Machine washable, pre-shrunk fabric. Free shipping over $50.
URL Slug: classic-cotton-t-shirt
```

---

## Quick Actions

The product editor toolbar provides quick actions:

| Action | Keyboard | Description |
|--------|----------|-------------|
| **Save** | Ctrl+S | Save changes |
| **Publish** | Ctrl+P | Publish product |
| **Duplicate** | - | Create a copy |
| **Archive** | - | Archive product |
| **Delete** | - | Delete permanently |
| **View on Site** | - | Open storefront page |

---

## Import/Export

### Exporting Products

1. Go to **Products** list
2. Click **Export** button
3. Choose format: CSV or JSON
4. Select fields to include
5. Download file

### Importing Products

1. Click **Import** button
2. Upload CSV or JSON file
3. Map columns to fields
4. Preview import
5. Confirm to import

**CSV Format Example:**
```csv
sku,name,price,stock,category,status
TSHIRT-001,"Classic T-Shirt",29.99,100,Apparel,Published
TSHIRT-002,"Premium T-Shirt",49.99,50,Apparel,Published
```

---

## Content Sync (Umbraco Integration)

Algora Commerce can sync products with Umbraco content nodes for CMS-driven product pages.

### How It Works

1. **Database → Content**: Product data syncs to Umbraco document type
2. **Content → Database**: Edits in Content tree sync back to e-commerce

### Enabling Sync

1. Create document type with alias `product`
2. Map properties in Settings > Content Sync
3. Products appear in Content tree

![Content Sync](../screenshots/content-sync.png)

---

## Tips & Best Practices

### Product Naming
- Use clear, descriptive names
- Include key attributes (e.g., "Blue Cotton T-Shirt - Unisex")
- Keep names under 60 characters for SEO

### Pricing Strategy
- Set competitive base prices
- Use sale prices for promotions
- Track cost prices for margin analysis

### Inventory Management
- Set low stock thresholds (typically 5-10 units)
- Enable backorders for high-demand items
- Regular stock audits

### Images
- Primary image should be the "hero" shot
- Include detail images for texture/quality
- Add lifestyle images showing product in use

---

## Related Documentation

- [Category Management](./categories.md)
- [Inventory Management](./inventory.md)
- [Discounts & Pricing](./discounts.md)
- [API: Products Endpoint](../developer/api-reference.md#products)
