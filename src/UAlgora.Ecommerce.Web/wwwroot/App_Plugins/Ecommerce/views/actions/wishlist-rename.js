import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Wishlist Rename Action
 * Quick action to rename a wishlist.
 */
export class WishlistRenameAction extends UmbElementMixin(LitElement) {
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

  async _handleRename() {
    const workspace = document.querySelector('ecommerce-wishlist-workspace');
    if (!workspace) return;

    const wishlist = workspace.getWishlist();
    if (!wishlist?.id) {
      alert('Please save the wishlist first');
      return;
    }

    const newName = prompt('Enter a new name for the wishlist:', wishlist.name);
    if (newName === null) return; // User cancelled

    if (!newName.trim()) {
      alert('Name cannot be empty');
      return;
    }

    if (newName.trim() === wishlist.name) {
      return; // No change
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/wishlist/${wishlist.id}/rename`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ name: newName.trim() })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setWishlist(result);

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Wishlist renamed to "${result.name}"`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to rename wishlist');
      }
    } catch (error) {
      console.error('Error renaming wishlist:', error);
      alert('Failed to rename wishlist');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="default"
        ?disabled=${this._processing}
        @click=${this._handleRename}
      >
        <uui-icon name="icon-edit"></uui-icon>
        ${this._processing ? 'Renaming...' : 'Rename'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-wishlist-rename-action', WishlistRenameAction);

export default WishlistRenameAction;
