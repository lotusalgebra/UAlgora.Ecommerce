import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Review Reject Action
 * Quick action to reject a review.
 */
export class ReviewRejectAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _isApproved: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._isApproved = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-review-workspace');
    if (workspace) {
      const review = workspace.getReview();
      this._isApproved = review?.isApproved ?? false;
    }
  }

  async _handleReject() {
    const workspace = document.querySelector('ecommerce-review-workspace');
    if (!workspace) return;

    const review = workspace.getReview();
    if (!review?.id) {
      alert('Please save the review first');
      return;
    }

    const reason = prompt('Enter rejection reason (optional):');
    if (reason === null) return; // User cancelled

    const confirmed = confirm('Reject and delete this review? This action cannot be undone.');
    if (!confirmed) return;

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/review/${review.id}/reject`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ reason: reason || null })
      });

      if (response.ok) {
        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'Review rejected and deleted',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);

        // Navigate back to list
        window.history.back();
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to reject review');
      }
    } catch (error) {
      console.error('Error rejecting review:', error);
      alert('Failed to reject review');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="danger"
        ?disabled=${this._processing}
        @click=${this._handleReject}
      >
        <uui-icon name="icon-delete"></uui-icon>
        ${this._processing ? 'Rejecting...' : 'Reject'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-review-reject-action', ReviewRejectAction);

export default ReviewRejectAction;
