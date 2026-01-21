import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Review Approve Action
 * Quick action to approve a review.
 */
export class ReviewApproveAction extends UmbElementMixin(LitElement) {
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

  async _handleApprove() {
    const workspace = document.querySelector('ecommerce-review-workspace');
    if (!workspace) return;

    const review = workspace.getReview();
    if (!review?.id) {
      alert('Please save the review first');
      return;
    }

    if (this._isApproved) {
      alert('This review is already approved');
      return;
    }

    const confirmed = confirm('Approve this review? It will be visible to customers.');
    if (!confirmed) return;

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/review/${review.id}/approve`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setReview(result);
        this._isApproved = result.isApproved;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'Review approved',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to approve review');
      }
    } catch (error) {
      console.error('Error approving review:', error);
      alert('Failed to approve review');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="primary"
        color="positive"
        ?disabled=${this._processing || this._isApproved}
        @click=${this._handleApprove}
      >
        <uui-icon name="icon-check"></uui-icon>
        ${this._processing ? 'Approving...' : this._isApproved ? 'Approved' : 'Approve'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-review-approve-action', ReviewApproveAction);

export default ReviewApproveAction;
