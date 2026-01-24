import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Cart Clear Action
 * Quick action to clear all items from a cart.
 */
export class CartClearAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _itemCount: { type: Number, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._itemCount = 0;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-cart-workspace');
    if (workspace) {
      const cart = workspace.getCart();
      this._itemCount = cart?.items?.length || 0;
    }
  }

  async _handleClear() {
    const workspace = document.querySelector('ecommerce-cart-workspace');
    if (!workspace) return;

    const cart = workspace.getCart();
    if (!cart?.id) {
      alert('Cart not found');
      return;
    }

    if (this._itemCount === 0) {
      alert('This cart is already empty');
      return;
    }

    if (!confirm(`Are you sure you want to remove all ${this._itemCount} item(s) from this cart? This action cannot be undone.`)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/cart/${cart.id}/clear`, {
        method: 'POST',
        headers: {
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setCart(result);
        this._itemCount = 0;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'All items removed from cart',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to clear cart');
      }
    } catch (error) {
      console.error('Error clearing cart:', error);
      alert('Failed to clear cart');
    } finally {
      this._processing = false;
    }
  }

  render() {
    if (this._itemCount === 0) {
      return html``;
    }

    return html`
      <uui-button
        look="secondary"
        color="danger"
        ?disabled=${this._processing}
        @click=${this._handleClear}
      >
        <uui-icon name="icon-trash"></uui-icon>
        ${this._processing ? 'Clearing...' : `Clear (${this._itemCount})`}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-cart-clear-action', CartClearAction);

export default CartClearAction;
