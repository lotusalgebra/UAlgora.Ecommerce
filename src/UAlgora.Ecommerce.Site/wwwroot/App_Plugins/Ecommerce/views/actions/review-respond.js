import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Review Respond Action
 * Quick action to add/edit merchant response to a review.
 */
export class ReviewRespondAction extends UmbElementMixin(LitElement) {
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

  async _handleRespond() {
    const workspace = document.querySelector('ecommerce-review-workspace');
    if (!workspace) return;

    const review = workspace.getReview();
    if (!review?.id) {
      alert('Please save the review first');
      return;
    }

    const currentResponse = review.merchantResponse || '';
    const response = prompt('Enter your response to this review:', currentResponse);
    if (response === null) return; // User cancelled

    if (!response.trim()) {
      alert('Response cannot be empty. Use "Remove Response" to delete existing response.');
      return;
    }

    this._processing = true;

    try {
      const apiResponse = await fetch(`/umbraco/backoffice/ecommerce/review/${review.id}/respond`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ response: response.trim() })
      });

      if (apiResponse.ok) {
        const result = await apiResponse.json();
        workspace.setReview(result);
        this._hasResponse = !!result.merchantResponse;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'Response added to review',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await apiResponse.json();
        alert(error.message || 'Failed to add response');
      }
    } catch (error) {
      console.error('Error adding response:', error);
      alert('Failed to add response');
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
        @click=${this._handleRespond}
      >
        <uui-icon name="icon-message"></uui-icon>
        ${this._processing ? 'Saving...' : this._hasResponse ? 'Edit Response' : 'Respond'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-review-respond-action', ReviewRespondAction);

export default ReviewRespondAction;
