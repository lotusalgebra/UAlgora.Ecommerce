import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Product Sale Action
 * Quick action to put a product on sale or remove from sale.
 */
export class ProductSaleAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .sale-badge {
      font-size: 10px;
      padding: 2px 4px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-danger);
      color: white;
      margin-left: 4px;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _isOnSale: { type: Boolean, state: true },
    _basePrice: { type: Number, state: true },
    _salePrice: { type: Number, state: true },
    _discountPercentage: { type: Number, state: true },
    _currencyCode: { type: String, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._isOnSale = false;
    this._basePrice = 0;
    this._salePrice = null;
    this._discountPercentage = null;
    this._currencyCode = 'USD';
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-product-workspace');
    if (workspace) {
      const product = workspace.getProduct();
      this._isOnSale = product?.isOnSale ?? false;
      this._basePrice = product?.basePrice ?? 0;
      this._salePrice = product?.salePrice;
      this._discountPercentage = product?.discountPercentage;
      this._currencyCode = product?.currencyCode || 'USD';
    }
  }

  _formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: this._currencyCode
    }).format(amount || 0);
  }

  async _handleToggleSale() {
    const workspace = document.querySelector('ecommerce-product-workspace');
    if (!workspace) return;

    const product = workspace.getProduct();
    if (!product?.id) {
      alert('Please save the product first');
      return;
    }

    if (this._isOnSale) {
      // Remove from sale
      if (!confirm('Remove this product from sale?')) {
        return;
      }

      this._processing = true;

      try {
        const response = await fetch(`/umbraco/management/api/v1/ecommerce/products/${product.id}/remove-sale`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
          }
        });

        if (response.ok) {
          const result = await response.json();
          workspace.setProduct(result);
          this._isOnSale = false;
          this._salePrice = null;
          this._discountPercentage = null;

          const event = new CustomEvent('umb-notification', {
            bubbles: true,
            composed: true,
            detail: {
              headline: 'Success',
              message: 'Product removed from sale',
              color: 'positive'
            }
          });
          this.dispatchEvent(event);
        } else {
          const error = await response.json();
          alert(error.message || 'Failed to remove sale');
        }
      } catch (error) {
        console.error('Error removing sale:', error);
        alert('Failed to remove sale');
      } finally {
        this._processing = false;
      }
    } else {
      // Put on sale
      const input = prompt(`Base price: ${this._formatCurrency(this._basePrice)}\nEnter sale price:`);
      if (input === null) return;

      const salePrice = parseFloat(input);
      if (isNaN(salePrice) || salePrice <= 0) {
        alert('Please enter a valid price');
        return;
      }

      if (salePrice >= this._basePrice) {
        alert('Sale price must be less than base price');
        return;
      }

      this._processing = true;

      try {
        const response = await fetch(`/umbraco/management/api/v1/ecommerce/products/${product.id}/set-sale`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
          },
          body: JSON.stringify({ salePrice })
        });

        if (response.ok) {
          const result = await response.json();
          workspace.setProduct(result);
          this._isOnSale = result.isOnSale;
          this._salePrice = result.salePrice;
          this._discountPercentage = result.discountPercentage;

          const event = new CustomEvent('umb-notification', {
            bubbles: true,
            composed: true,
            detail: {
              headline: 'Success',
              message: `Product on sale at ${this._formatCurrency(salePrice)} (${result.discountPercentage}% off)`,
              color: 'positive'
            }
          });
          this.dispatchEvent(event);
        } else {
          const error = await response.json();
          alert(error.message || 'Failed to set sale price');
        }
      } catch (error) {
        console.error('Error setting sale:', error);
        alert('Failed to set sale price');
      } finally {
        this._processing = false;
      }
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._isOnSale ? 'danger' : 'default'}"
        ?disabled=${this._processing}
        @click=${this._handleToggleSale}
      >
        <uui-icon name="${this._isOnSale ? 'icon-coins' : 'icon-tag'}"></uui-icon>
        ${this._processing ? 'Processing...' : this._isOnSale ? html`On Sale <span class="sale-badge">${this._discountPercentage}% OFF</span>` : 'Put on Sale'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-product-sale-action', ProductSaleAction);

export default ProductSaleAction;
