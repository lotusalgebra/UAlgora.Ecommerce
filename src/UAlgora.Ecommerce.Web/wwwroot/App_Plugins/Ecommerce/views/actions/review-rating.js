import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Review Rating Action
 * Quick action to update review rating.
 */
export class ReviewRatingAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .rating-badge {
      font-size: 11px;
      padding: 2px 6px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-emphasis);
      margin-left: 4px;
    }

    .stars {
      color: #f5a623;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _rating: { type: Number, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._rating = 5;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-review-workspace');
    if (workspace) {
      const review = workspace.getReview();
      this._rating = review?.rating ?? 5;
    }
  }

  async _handleRating() {
    const workspace = document.querySelector('ecommerce-review-workspace');
    if (!workspace) return;

    const review = workspace.getReview();
    if (!review?.id) {
      alert('Please save the review first');
      return;
    }

    const input = prompt(`Current rating: ${this._rating} stars\nEnter new rating (1-5):`, String(this._rating));
    if (input === null) return;

    const newRating = parseInt(input, 10);
    if (isNaN(newRating) || newRating < 1 || newRating > 5) {
      alert('Rating must be between 1 and 5');
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/review/${review.id}/update-rating`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ rating: newRating })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setReview(result);
        this._rating = result.rating;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Rating updated to ${newRating} stars`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to update rating');
      }
    } catch (error) {
      console.error('Error updating rating:', error);
      alert('Failed to update rating');
    } finally {
      this._processing = false;
    }
  }

  _getStars() {
    return '★'.repeat(this._rating) + '☆'.repeat(5 - this._rating);
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="default"
        ?disabled=${this._processing}
        @click=${this._handleRating}
      >
        <uui-icon name="icon-rate"></uui-icon>
        Rating <span class="rating-badge stars">${this._getStars()}</span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-review-rating-action', ReviewRatingAction);

export default ReviewRatingAction;
