import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Review Reset Votes Action
 * Quick action to reset helpful/unhelpful votes on a review.
 */
export class ReviewResetVotesAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .vote-badge {
      font-size: 11px;
      padding: 2px 6px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-emphasis);
      margin-left: 4px;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _totalVotes: { type: Number, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._totalVotes = 0;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-review-workspace');
    if (workspace) {
      const review = workspace.getReview();
      this._totalVotes = (review?.helpfulVotes ?? 0) + (review?.unhelpfulVotes ?? 0);
    }
  }

  async _handleReset() {
    const workspace = document.querySelector('ecommerce-review-workspace');
    if (!workspace) return;

    const review = workspace.getReview();
    if (!review?.id) {
      alert('Please save the review first');
      return;
    }

    if (this._totalVotes === 0) {
      alert('No votes to reset');
      return;
    }

    const confirmed = confirm(`Reset all ${this._totalVotes} votes on this review? This cannot be undone.`);
    if (!confirmed) return;

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/review/${review.id}/reset-votes`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setReview(result);
        this._totalVotes = result.helpfulVotes + result.unhelpfulVotes;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'Votes reset',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to reset votes');
      }
    } catch (error) {
      console.error('Error resetting votes:', error);
      alert('Failed to reset votes');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="warning"
        ?disabled=${this._processing || this._totalVotes === 0}
        @click=${this._handleReset}
      >
        <uui-icon name="icon-sync"></uui-icon>
        Reset Votes <span class="vote-badge">${this._totalVotes}</span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-review-reset-votes-action', ReviewResetVotesAction);

export default ReviewResetVotesAction;
