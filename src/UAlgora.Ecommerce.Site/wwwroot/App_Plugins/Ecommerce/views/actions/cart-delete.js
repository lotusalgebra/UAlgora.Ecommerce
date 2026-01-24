import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Cart Delete Action
 * Quick action to delete a cart.
 */
export class CartDeleteAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
  }

  async _handleDelete() {
    const workspace = document.querySelector('ecommerce-cart-workspace');
    if (!workspace) return;

    const cart = workspace.getCart();
    if (!cart?.id) {
      alert('Cart not found');
      return;
    }

    const itemCount = cart.items?.length || 0;
    const message = itemCount > 0
      ? `Are you sure you want to delete this cart with ${itemCount} item(s)? This action cannot be undone.`
      : 'Are you sure you want to delete this empty cart? This action cannot be undone.';

    if (!confirm(message)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/cart/${cart.id}`, {
        method: 'DELETE',
        headers: {
          'Accept': 'application/json'
        }
      });

      if (response.ok || response.status === 204) {
        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'Cart deleted',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);

        // Navigate back to cart list
        window.history.back();
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to delete cart');
      }
    } catch (error) {
      console.error('Error deleting cart:', error);
      alert('Failed to delete cart');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="danger"
        ?disabled=${this._processing}
        @click=${this._handleDelete}
      >
        <uui-icon name="icon-delete"></uui-icon>
        ${this._processing ? 'Deleting...' : 'Delete Cart'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-cart-delete-action', CartDeleteAction);

export default CartDeleteAction;
