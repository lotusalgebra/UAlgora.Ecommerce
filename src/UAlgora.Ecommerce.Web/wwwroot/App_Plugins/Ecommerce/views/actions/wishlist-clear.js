import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Wishlist Clear Action
 * Quick action to clear all items from a wishlist.
 */
export class WishlistClearAction extends UmbElementMixin(LitElement) {
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
    const workspace = document.querySelector('ecommerce-wishlist-workspace');
    if (workspace) {
      const wishlist = workspace.getWishlist();
      this._itemCount = wishlist?.items?.length || 0;
    }
  }

  async _handleClear() {
    const workspace = document.querySelector('ecommerce-wishlist-workspace');
    if (!workspace) return;

    const wishlist = workspace.getWishlist();
    if (!wishlist?.id) {
      alert('Please save the wishlist first');
      return;
    }

    if (this._itemCount === 0) {
      alert('This wishlist is already empty');
      return;
    }

    if (!confirm(`Are you sure you want to remove all ${this._itemCount} item(s) from this wishlist? This action cannot be undone.`)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/wishlist/${wishlist.id}/clear`, {
        method: 'POST',
        headers: {
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setWishlist(result);
        this._itemCount = 0;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'All items removed from wishlist',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to clear wishlist');
      }
    } catch (error) {
      console.error('Error clearing wishlist:', error);
      alert('Failed to clear wishlist');
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

customElements.define('ecommerce-wishlist-clear-action', WishlistClearAction);

export default WishlistClearAction;
