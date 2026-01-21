import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Wishlist Duplicate Action
 * Quick action to duplicate a wishlist.
 */
export class WishlistDuplicateAction extends UmbElementMixin(LitElement) {
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

  async _handleDuplicate() {
    const workspace = document.querySelector('ecommerce-wishlist-workspace');
    if (!workspace) return;

    const wishlist = workspace.getWishlist();
    if (!wishlist?.id) {
      alert('Please save the wishlist first');
      return;
    }

    const defaultName = `${wishlist.name} (Copy)`;
    const newName = prompt('Enter a name for the duplicate wishlist:', defaultName);
    if (newName === null) return; // User cancelled

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/wishlist/${wishlist.id}/duplicate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ newName: newName.trim() || null })
      });

      if (response.ok) {
        const result = await response.json();

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Wishlist duplicated as "${result.name}"`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);

        // Optionally navigate to the new wishlist
        if (confirm('Would you like to open the duplicated wishlist?')) {
          window.location.href = `/umbraco#/ecommerce/wishlist/${result.id}`;
        }
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to duplicate wishlist');
      }
    } catch (error) {
      console.error('Error duplicating wishlist:', error);
      alert('Failed to duplicate wishlist');
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
        @click=${this._handleDuplicate}
      >
        <uui-icon name="icon-documents"></uui-icon>
        ${this._processing ? 'Duplicating...' : 'Duplicate'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-wishlist-duplicate-action', WishlistDuplicateAction);

export default WishlistDuplicateAction;
