import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Wishlist Remove Share Action
 * Quick action to remove the share token from a wishlist.
 */
export class WishlistRemoveShareAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _hasShareToken: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._hasShareToken = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-wishlist-workspace');
    if (workspace) {
      const wishlist = workspace.getWishlist();
      this._hasShareToken = !!wishlist?.shareToken;
    }
  }

  async _handleRemoveShare() {
    const workspace = document.querySelector('ecommerce-wishlist-workspace');
    if (!workspace) return;

    const wishlist = workspace.getWishlist();
    if (!wishlist?.id) {
      alert('Please save the wishlist first');
      return;
    }

    if (!wishlist.shareToken) {
      alert('This wishlist does not have a share link');
      return;
    }

    if (!confirm('Remove the share link? Existing links will stop working.')) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/wishlist/${wishlist.id}/remove-share-token`, {
        method: 'POST',
        headers: {
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setWishlist(result);
        this._hasShareToken = false;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'Share link removed',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to remove share link');
      }
    } catch (error) {
      console.error('Error removing share token:', error);
      alert('Failed to remove share link');
    } finally {
      this._processing = false;
    }
  }

  render() {
    if (!this._hasShareToken) {
      return html``;
    }

    return html`
      <uui-button
        look="secondary"
        color="danger"
        ?disabled=${this._processing}
        @click=${this._handleRemoveShare}
      >
        <uui-icon name="icon-link-broken"></uui-icon>
        ${this._processing ? 'Removing...' : 'Remove Share Link'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-wishlist-remove-share-action', WishlistRemoveShareAction);

export default WishlistRemoveShareAction;
