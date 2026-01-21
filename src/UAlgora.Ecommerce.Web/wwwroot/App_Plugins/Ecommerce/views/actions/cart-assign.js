import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Cart Assign Action
 * Quick action to assign a cart to a customer.
 */
export class CartAssignAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _isGuest: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._isGuest = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-cart-workspace');
    if (workspace) {
      const cart = workspace.getCart();
      this._isGuest = !cart?.customerId;
    }
  }

  async _handleAssign() {
    const workspace = document.querySelector('ecommerce-cart-workspace');
    if (!workspace) return;

    const cart = workspace.getCart();
    if (!cart?.id) {
      alert('Cart not found');
      return;
    }

    if (!this._isGuest) {
      alert('This cart is already assigned to a customer');
      return;
    }

    const customerIdInput = prompt('Enter Customer ID (GUID) to assign this cart to:');
    if (customerIdInput === null) return;

    // Basic GUID validation
    const guidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
    if (!guidRegex.test(customerIdInput.trim())) {
      alert('Please enter a valid Customer ID (GUID format)');
      return;
    }

    if (!confirm('Assign this cart to the specified customer?')) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/cart/${cart.id}/assign-customer`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ customerId: customerIdInput.trim() })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setCart(result);
        this._isGuest = false;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'Cart assigned to customer',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to assign cart');
      }
    } catch (error) {
      console.error('Error assigning cart:', error);
      alert('Failed to assign cart');
    } finally {
      this._processing = false;
    }
  }

  render() {
    if (!this._isGuest) {
      return html``;
    }

    return html`
      <uui-button
        look="secondary"
        color="default"
        ?disabled=${this._processing}
        @click=${this._handleAssign}
      >
        <uui-icon name="icon-user"></uui-icon>
        ${this._processing ? 'Assigning...' : 'Assign to Customer'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-cart-assign-action', CartAssignAction);

export default CartAssignAction;
