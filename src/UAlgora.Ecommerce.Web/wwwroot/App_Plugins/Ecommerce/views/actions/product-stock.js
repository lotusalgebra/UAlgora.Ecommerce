import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Product Stock Action
 * Quick action to adjust product stock quantity.
 */
export class ProductStockAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .stock-info {
      display: inline-flex;
      align-items: center;
      gap: var(--uui-size-space-2);
    }

    .stock-badge {
      font-size: 12px;
      padding: 2px 6px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-emphasis);
    }

    .stock-badge.low {
      background: var(--uui-color-warning);
      color: white;
    }

    .stock-badge.out {
      background: var(--uui-color-danger);
      color: white;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _stockQuantity: { type: Number, state: true },
    _trackInventory: { type: Boolean, state: true },
    _isLowStock: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._stockQuantity = 0;
    this._trackInventory = true;
    this._isLowStock = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-product-workspace');
    if (workspace) {
      const product = workspace.getProduct();
      this._stockQuantity = product?.stockQuantity ?? 0;
      this._trackInventory = product?.trackInventory ?? true;
      this._isLowStock = product?.isLowStock ?? false;
    }
  }

  async _handleAdjust() {
    const workspace = document.querySelector('ecommerce-product-workspace');
    if (!workspace) return;

    const product = workspace.getProduct();
    if (!product?.id) {
      alert('Please save the product first');
      return;
    }

    const input = prompt(`Current stock: ${this._stockQuantity}\nEnter adjustment (positive to add, negative to remove):`);
    if (input === null) return;

    const adjustment = parseInt(input, 10);
    if (isNaN(adjustment)) {
      alert('Please enter a valid number');
      return;
    }

    const newQuantity = this._stockQuantity + adjustment;
    if (newQuantity < 0) {
      alert(`Cannot reduce stock below 0. Current: ${this._stockQuantity}, Adjustment: ${adjustment}`);
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/products/${product.id}/adjust-stock`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ adjustment })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setProduct(result);
        this._stockQuantity = result.stockQuantity;
        this._isLowStock = result.isLowStock;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Stock adjusted by ${adjustment > 0 ? '+' : ''}${adjustment}. New quantity: ${result.stockQuantity}`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to adjust stock');
      }
    } catch (error) {
      console.error('Error adjusting stock:', error);
      alert('Failed to adjust stock');
    } finally {
      this._processing = false;
    }
  }

  render() {
    if (!this._trackInventory) {
      return html``;
    }

    const stockClass = this._stockQuantity === 0 ? 'out' : this._isLowStock ? 'low' : '';

    return html`
      <uui-button
        look="secondary"
        color="${this._stockQuantity === 0 ? 'danger' : this._isLowStock ? 'warning' : 'default'}"
        ?disabled=${this._processing}
        @click=${this._handleAdjust}
      >
        <span class="stock-info">
          <uui-icon name="icon-layers-alt"></uui-icon>
          Stock: <span class="stock-badge ${stockClass}">${this._stockQuantity}</span>
        </span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-product-stock-action', ProductStockAction);

export default ProductStockAction;
