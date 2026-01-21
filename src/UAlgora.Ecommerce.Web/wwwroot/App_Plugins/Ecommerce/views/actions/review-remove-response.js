import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Review Remove Response Action
 * Quick action to remove merchant response from a review.
 */
export class ReviewRemoveResponseAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _hasResponse: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._hasResponse = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-review-workspace');
    if (workspace) {
      const review = workspace.getReview();
      this._hasResponse = !!review?.merchantResponse;
    }
  }

  async _handleRemove() {
    const workspace = document.querySelector('ecommerce-review-workspace');
    if (!workspace) return;

    const review = workspace.getReview();
    if (!review?.id) {
      alert('Please save the review first');
      return;
    }

    if (!this._hasResponse) {
      alert('No response to remove');
      return;
    }

    const confirmed = confirm('Remove your response from this review?');
    if (!confirmed) return;

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/review/${review.id}/remove-response`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setReview(result);
        this._hasResponse = !!result.merchantResponse;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'Response removed',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to remove response');
      }
    } catch (error) {
      console.error('Error removing response:', error);
      alert('Failed to remove response');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="warning"
        ?disabled=${this._processing || !this._hasResponse}
        @click=${this._handleRemove}
      >
        <uui-icon name="icon-message"></uui-icon>
        ${this._processing ? 'Removing...' : 'Remove Response'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-review-remove-response-action', ReviewRemoveResponseAction);

export default ReviewRemoveResponseAction;
