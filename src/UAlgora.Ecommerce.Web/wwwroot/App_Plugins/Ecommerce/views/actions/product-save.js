import { LitElement, html } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Product Save Action
 * Handles saving product data to the backend.
 */
export class ProductSaveAction extends UmbElementMixin(LitElement) {
  static properties = {
    _saving: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._saving = false;
  }

  async _handleSave() {
    const workspace = this.closest('ecommerce-product-workspace');
    if (!workspace) {
      console.error('Could not find product workspace');
      return;
    }

    const product = workspace.getProduct();
    if (!product) {
      console.error('No product data to save');
      return;
    }

    // Validate required fields
    if (!product.name?.trim()) {
      this._showNotification('error', 'Product name is required');
      return;
    }

    if (!product.sku?.trim()) {
      this._showNotification('error', 'Product SKU is required');
      return;
    }

    try {
      this._saving = true;

      const isNew = workspace.isNew();
      const url = isNew
        ? '/umbraco/management/api/v1/ecommerce/product'
        : `/umbraco/management/api/v1/ecommerce/product/${product.id}`;

      const method = isNew ? 'POST' : 'PUT';

      const response = await fetch(url, {
        method,
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(this._prepareProductData(product))
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || 'Failed to save product');
      }

      const savedProduct = await response.json();
      workspace.setProduct(savedProduct);

      this._showNotification('positive', isNew ? 'Product created successfully' : 'Product saved successfully');

      // If it was a new product, update the URL
      if (isNew && savedProduct.id) {
        // Navigate to the saved product
        window.history.replaceState(null, '', `/umbraco/section/ecommerce/workspace/product/edit/${savedProduct.id}`);
      }
    } catch (error) {
      console.error('Error saving product:', error);
      this._showNotification('danger', error.message || 'Failed to save product');
    } finally {
      this._saving = false;
    }
  }

  _prepareProductData(product) {
    return {
      name: product.name?.trim(),
      sku: product.sku?.trim(),
      slug: product.slug?.trim() || null,
      description: product.description || null,
      shortDescription: product.shortDescription || null,
      basePrice: product.basePrice || 0,
      salePrice: product.salePrice || null,
      costPrice: product.costPrice || null,
      isActive: product.isActive ?? true,
      isFeatured: product.isFeatured ?? false,
      trackInventory: product.trackInventory ?? true,
      stockQuantity: product.stockQuantity || 0,
      lowStockThreshold: product.lowStockThreshold || 10,
      allowBackorders: product.allowBackorders ?? false,
      weight: product.weight || null,
      length: product.length || null,
      width: product.width || null,
      height: product.height || null,
      metaTitle: product.metaTitle || null,
      metaDescription: product.metaDescription || null,
      metaKeywords: product.metaKeywords || null,
      variants: product.variants || [],
      categoryIds: product.categoryIds || [],
      imageUrls: product.imageUrls || []
    };
  }

  _showNotification(color, message) {
    // Use Umbraco's notification system
    const event = new CustomEvent('umb-notification', {
      bubbles: true,
      composed: true,
      detail: {
        headline: color === 'positive' ? 'Success' : 'Error',
        message,
        color
      }
    });
    this.dispatchEvent(event);
  }

  render() {
    return html`
      <uui-button
        look="primary"
        color="positive"
        ?disabled=${this._saving}
        @click=${this._handleSave}
      >
        ${this._saving
          ? html`<uui-loader-circle></uui-loader-circle>`
          : html`<uui-icon name="icon-save"></uui-icon>`
        }
        ${this._saving ? 'Saving...' : 'Save'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-product-save-action', ProductSaveAction);

export default ProductSaveAction;
