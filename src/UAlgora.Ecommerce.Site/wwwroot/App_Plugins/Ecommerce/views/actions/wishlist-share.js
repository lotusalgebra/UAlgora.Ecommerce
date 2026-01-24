import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Wishlist Share Action
 * Quick action to generate a share token for a wishlist.
 */
export class WishlistShareAction extends UmbElementMixin(LitElement) {
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

  async _handleGenerateShare() {
    const workspace = document.querySelector('ecommerce-wishlist-workspace');
    if (!workspace) return;

    const wishlist = workspace.getWishlist();
    if (!wishlist?.id) {
      alert('Please save the wishlist first');
      return;
    }

    if (this._hasShareToken) {
      // Copy existing share URL to clipboard
      const shareUrl = `${window.location.origin}/wishlist/share/${wishlist.shareToken}`;
      try {
        await navigator.clipboard.writeText(shareUrl);
        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Copied',
            message: 'Share URL copied to clipboard',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } catch (error) {
        prompt('Share URL:', shareUrl);
      }
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/wishlist/${wishlist.id}/generate-share-token`, {
        method: 'POST',
        headers: {
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setWishlist({ ...wishlist, shareToken: result.shareToken });
        this._hasShareToken = true;

        const shareUrl = `${window.location.origin}${result.shareUrl}`;
        try {
          await navigator.clipboard.writeText(shareUrl);
        } catch (err) {
          // Clipboard write may fail
        }

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Share Link Generated',
            message: 'Share URL copied to clipboard',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to generate share link');
      }
    } catch (error) {
      console.error('Error generating share token:', error);
      alert('Failed to generate share link');
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
        @click=${this._handleGenerateShare}
      >
        <uui-icon name="icon-share"></uui-icon>
        ${this._processing ? 'Generating...' : this._hasShareToken ? 'Copy Share Link' : 'Generate Share Link'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-wishlist-share-action', WishlistShareAction);

export default WishlistShareAction;
