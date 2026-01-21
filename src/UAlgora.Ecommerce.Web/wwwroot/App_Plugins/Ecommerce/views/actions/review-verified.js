import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Review Verified Purchase Action
 * Quick action to toggle verified purchase status of a review.
 */
export class ReviewVerifiedAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _isVerified: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._isVerified = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-review-workspace');
    if (workspace) {
      const review = workspace.getReview();
      this._isVerified = review?.isVerifiedPurchase ?? false;
    }
  }

  async _handleToggle() {
    const workspace = document.querySelector('ecommerce-review-workspace');
    if (!workspace) return;

    const review = workspace.getReview();
    if (!review?.id) {
      alert('Please save the review first');
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/review/${review.id}/toggle-verified`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setReview(result);
        this._isVerified = result.isVerifiedPurchase;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: result.isVerifiedPurchase ? 'Marked as verified purchase' : 'Verified status removed',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to toggle verified status');
      }
    } catch (error) {
      console.error('Error toggling verified status:', error);
      alert('Failed to toggle verified status');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._isVerified ? 'default' : 'positive'}"
        ?disabled=${this._processing}
        @click=${this._handleToggle}
      >
        <uui-icon name="icon-certificate"></uui-icon>
        ${this._processing ? 'Processing...' : this._isVerified ? 'Unverify' : 'Verify Purchase'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-review-verified-action', ReviewVerifiedAction);

export default ReviewVerifiedAction;
