import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Cart Expiration Action
 * Quick action to set or remove cart expiration.
 */
export class CartExpirationAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _hasExpiration: { type: Boolean, state: true },
    _isExpired: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._hasExpiration = false;
    this._isExpired = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-cart-workspace');
    if (workspace) {
      const cart = workspace.getCart();
      this._hasExpiration = !!cart?.expiresAt;
      if (cart?.expiresAt) {
        this._isExpired = new Date(cart.expiresAt) < new Date();
      }
    }
  }

  async _handleExpiration() {
    const workspace = document.querySelector('ecommerce-cart-workspace');
    if (!workspace) return;

    const cart = workspace.getCart();
    if (!cart?.id) {
      alert('Cart not found');
      return;
    }

    if (this._hasExpiration) {
      // Remove expiration
      if (!confirm('Remove the expiration date from this cart?')) {
        return;
      }

      this._processing = true;

      try {
        const response = await fetch(`/umbraco/backoffice/ecommerce/cart/${cart.id}/remove-expiration`, {
          method: 'POST',
          headers: {
            'Accept': 'application/json'
          }
        });

        if (response.ok) {
          const result = await response.json();
          workspace.setCart(result);
          this._hasExpiration = false;
          this._isExpired = false;

          const event = new CustomEvent('umb-notification', {
            bubbles: true,
            composed: true,
            detail: {
              headline: 'Success',
              message: 'Cart expiration removed',
              color: 'positive'
            }
          });
          this.dispatchEvent(event);
        } else {
          const error = await response.json();
          alert(error.message || 'Failed to remove expiration');
        }
      } catch (error) {
        console.error('Error removing expiration:', error);
        alert('Failed to remove expiration');
      } finally {
        this._processing = false;
      }
    } else {
      // Set expiration
      const daysInput = prompt('Set cart to expire in how many days?', '7');
      if (daysInput === null) return;

      const days = parseInt(daysInput, 10);
      if (isNaN(days) || days < 1) {
        alert('Please enter a valid number of days');
        return;
      }

      const expiresAt = new Date();
      expiresAt.setDate(expiresAt.getDate() + days);

      this._processing = true;

      try {
        const response = await fetch(`/umbraco/backoffice/ecommerce/cart/${cart.id}/set-expiration`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
          },
          body: JSON.stringify({ expiresAt: expiresAt.toISOString() })
        });

        if (response.ok) {
          const result = await response.json();
          workspace.setCart(result);
          this._hasExpiration = true;
          this._isExpired = false;

          const event = new CustomEvent('umb-notification', {
            bubbles: true,
            composed: true,
            detail: {
              headline: 'Success',
              message: `Cart set to expire in ${days} day(s)`,
              color: 'positive'
            }
          });
          this.dispatchEvent(event);
        } else {
          const error = await response.json();
          alert(error.message || 'Failed to set expiration');
        }
      } catch (error) {
        console.error('Error setting expiration:', error);
        alert('Failed to set expiration');
      } finally {
        this._processing = false;
      }
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._isExpired ? 'danger' : this._hasExpiration ? 'warning' : 'default'}"
        ?disabled=${this._processing}
        @click=${this._handleExpiration}
      >
        <uui-icon name="icon-time"></uui-icon>
        ${this._processing ? 'Processing...' : this._isExpired ? 'Expired' : this._hasExpiration ? 'Remove Expiration' : 'Set Expiration'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-cart-expiration-action', CartExpirationAction);

export default CartExpirationAction;
