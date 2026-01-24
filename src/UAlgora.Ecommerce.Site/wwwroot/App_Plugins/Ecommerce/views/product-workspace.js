import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { UMB_WORKSPACE_CONTEXT } from "@umbraco-cms/backoffice/workspace";

/**
 * Product Workspace Element
 * Main workspace container for editing products in the Umbraco backoffice.
 */
export class ProductWorkspace extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      width: 100%;
      height: 100%;
    }
  `;

  static properties = {
    _product: { type: Object, state: true },
    _loading: { type: Boolean, state: true },
    _error: { type: String, state: true },
    _isNew: { type: Boolean, state: true }
  };

  #workspaceContext;

  constructor() {
    super();
    this._product = null;
    this._loading = true;
    this._error = null;
    this._isNew = false;
  }

  connectedCallback() {
    super.connectedCallback();

    this.consumeContext(UMB_WORKSPACE_CONTEXT, (context) => {
      this.#workspaceContext = context;
      this._loadProduct();
    });
  }

  async _loadProduct() {
    try {
      this._loading = true;

      // Get product ID from route/context
      const productId = this.#workspaceContext?.getUnique();

      if (!productId || productId === 'create') {
        this._isNew = true;
        this._product = this._getEmptyProduct();
        this._loading = false;
        return;
      }

      const response = await fetch(`/umbraco/management/api/v1/ecommerce/product/${productId}`, {
        headers: {
          'Accept': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error('Failed to load product');
      }

      this._product = await response.json();
      this._error = null;
    } catch (error) {
      this._error = error.message;
      console.error('Error loading product:', error);
    } finally {
      this._loading = false;
    }
  }

  _getEmptyProduct() {
    return {
      id: null,
      name: '',
      sku: '',
      slug: '',
      description: '',
      shortDescription: '',
      basePrice: 0,
      salePrice: null,
      costPrice: null,
      isActive: true,
      isFeatured: false,
      trackInventory: true,
      stockQuantity: 0,
      lowStockThreshold: 10,
      allowBackorders: false,
      weight: null,
      length: null,
      width: null,
      height: null,
      metaTitle: '',
      metaDescription: '',
      metaKeywords: ''
    };
  }

  getProduct() {
    return this._product;
  }

  setProduct(product) {
    this._product = { ...product };
    this.requestUpdate();
  }

  updateProduct(updates) {
    this._product = { ...this._product, ...updates };
    this.requestUpdate();
  }

  isNew() {
    return this._isNew;
  }

  isLoading() {
    return this._loading;
  }

  render() {
    if (this._loading) {
      return html`
        <div style="display: flex; justify-content: center; padding: var(--uui-size-layout-2);">
          <uui-loader></uui-loader>
        </div>
      `;
    }

    if (this._error) {
      return html`
        <uui-box>
          <div slot="headline">Error</div>
          <p>${this._error}</p>
          <uui-button @click=${this._loadProduct}>Retry</uui-button>
        </uui-box>
      `;
    }

    return html`<slot></slot>`;
  }
}

customElements.define('ecommerce-product-workspace', ProductWorkspace);

export default ProductWorkspace;
