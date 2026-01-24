import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Wishlist Default Action
 * Quick action to set a wishlist as default for a customer.
 */
export class WishlistDefaultAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _isDefault: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._isDefault = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-wishlist-workspace');
    if (workspace) {
      const wishlist = workspace.getWishlist();
      this._isDefault = wishlist?.isDefault || false;
    }
  }

  async _handleSetDefault() {
    const workspace = document.querySelector('ecommerce-wishlist-workspace');
    if (!workspace) return;

    const wishlist = workspace.getWishlist();
    if (!wishlist?.id) {
      alert('Please save the wishlist first');
      return;
    }

    if (wishlist.isDefault) {
      alert('This wishlist is already the default');
      return;
    }

    if (!confirm('Set this wishlist as the default for this customer?')) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/wishlist/${wishlist.id}/set-default`, {
        method: 'POST',
        headers: {
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setWishlist(result);
        this._isDefault = result.isDefault;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'Wishlist set as default',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to set as default');
      }
    } catch (error) {
      console.error('Error setting default:', error);
      alert('Failed to set as default');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="default"
        ?disabled=${this._processing || this._isDefault}
        @click=${this._handleSetDefault}
      >
        <uui-icon name="icon-favorite"></uui-icon>
        ${this._processing ? 'Processing...' : this._isDefault ? 'Default' : 'Set Default'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-wishlist-default-action', WishlistDefaultAction);

export default WishlistDefaultAction;
