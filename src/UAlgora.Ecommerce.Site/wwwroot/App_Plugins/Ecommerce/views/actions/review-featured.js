import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Review Featured Action
 * Quick action to toggle featured status of a review.
 */
export class ReviewFeaturedAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _isFeatured: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._isFeatured = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-review-workspace');
    if (workspace) {
      const review = workspace.getReview();
      this._isFeatured = review?.isFeatured ?? false;
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
      const response = await fetch(`/umbraco/backoffice/ecommerce/review/${review.id}/toggle-featured`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setReview(result);
        this._isFeatured = result.isFeatured;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: result.isFeatured ? 'Review marked as featured' : 'Featured status removed',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to toggle featured status');
      }
    } catch (error) {
      console.error('Error toggling featured status:', error);
      alert('Failed to toggle featured status');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._isFeatured ? 'warning' : 'positive'}"
        ?disabled=${this._processing}
        @click=${this._handleToggle}
      >
        <uui-icon name="icon-favorite"></uui-icon>
        ${this._processing ? 'Processing...' : this._isFeatured ? 'Unfeature' : 'Feature'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-review-featured-action', ReviewFeaturedAction);

export default ReviewFeaturedAction;
