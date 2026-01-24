import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Wishlist Public Action
 * Quick action to toggle public/private status of a wishlist.
 */
export class WishlistPublicAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _isPublic: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._isPublic = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-wishlist-workspace');
    if (workspace) {
      const wishlist = workspace.getWishlist();
      this._isPublic = wishlist?.isPublic || false;
    }
  }

  async _handleTogglePublic() {
    const workspace = document.querySelector('ecommerce-wishlist-workspace');
    if (!workspace) return;

    const wishlist = workspace.getWishlist();
    if (!wishlist?.id) {
      alert('Please save the wishlist first');
      return;
    }

    const action = this._isPublic ? 'private' : 'public';
    if (!confirm(`Make this wishlist ${action}?`)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/wishlist/${wishlist.id}/toggle-public`, {
        method: 'POST',
        headers: {
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setWishlist(result);
        this._isPublic = result.isPublic;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Wishlist is now ${result.isPublic ? 'public' : 'private'}`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to toggle public status');
      }
    } catch (error) {
      console.error('Error toggling public:', error);
      alert('Failed to toggle public status');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._isPublic ? 'warning' : 'default'}"
        ?disabled=${this._processing}
        @click=${this._handleTogglePublic}
      >
        <uui-icon name="${this._isPublic ? 'icon-eye' : 'icon-eye-strikethrough'}"></uui-icon>
        ${this._processing ? 'Processing...' : this._isPublic ? 'Make Private' : 'Make Public'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-wishlist-public-action', WishlistPublicAction);

export default WishlistPublicAction;
