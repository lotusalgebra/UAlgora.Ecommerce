import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Product Archive Action
 * Quick action to archive or restore a product.
 */
export class ProductArchiveAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _isArchived: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._isArchived = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-product-workspace');
    if (workspace) {
      const product = workspace.getProduct();
      this._isArchived = product?.status?.toLowerCase() === 'archived';
    }
  }

  async _handleToggle() {
    const workspace = document.querySelector('ecommerce-product-workspace');
    if (!workspace) return;

    const product = workspace.getProduct();
    if (!product?.id) {
      alert('Please save the product first');
      return;
    }

    const action = this._isArchived ? 'restore' : 'archive';
    const confirmMessage = this._isArchived
      ? `Restore "${product.name}" from archive?`
      : `Archive "${product.name}"? It will be hidden from customers.`;

    if (!confirm(confirmMessage)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/products/${product.id}/${action}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setProduct(result);
        this._isArchived = result.status?.toLowerCase() === 'archived';

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: this._isArchived ? 'Product archived' : 'Product restored',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || `Failed to ${action} product`);
      }
    } catch (error) {
      console.error(`Error ${action}ing product:`, error);
      alert(`Failed to ${action} product`);
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._isArchived ? 'positive' : 'default'}"
        ?disabled=${this._processing}
        @click=${this._handleToggle}
      >
        <uui-icon name="${this._isArchived ? 'icon-undo' : 'icon-box'}"></uui-icon>
        ${this._processing ? 'Processing...' : this._isArchived ? 'Restore' : 'Archive'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-product-archive-action', ProductArchiveAction);

export default ProductArchiveAction;
