import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Supplier Rating Action
 * Quick action to update supplier rating.
 */
export class SupplierRatingAction extends UmbElementMixin(LitElement) {
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

    .good {
      background: var(--uui-color-positive-emphasis);
      color: var(--uui-color-positive);
    }

    .average {
      background: var(--uui-color-warning-emphasis);
      color: var(--uui-color-warning);
    }

    .poor {
      background: var(--uui-color-danger-emphasis);
      color: var(--uui-color-danger);
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _rating: { type: Number, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._rating = null;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-supplier-workspace');
    if (workspace) {
      const supplier = workspace.getSupplier();
      this._rating = supplier?.rating ?? null;
    }
  }

  async _handleRating() {
    const workspace = document.querySelector('ecommerce-supplier-workspace');
    if (!workspace) return;

    const supplier = workspace.getSupplier();
    if (!supplier?.id) {
      alert('Please save the supplier first');
      return;
    }

    const input = prompt(
      `Current rating: ${this._rating ?? 'Not rated'}\n` +
      `Enter new rating (0-5, where 5 is best):`,
      String(this._rating ?? 3)
    );
    if (input === null) return;

    const newRating = parseFloat(input);
    if (isNaN(newRating) || newRating < 0 || newRating > 5) {
      alert('Please enter a valid rating between 0 and 5');
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/inventory/supplier/${supplier.id}/update-rating`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ rating: newRating })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setSupplier(result);
        this._rating = result.rating;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Rating updated to ${newRating}`,
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

  render() {
    const isGood = this._rating !== null && this._rating >= 4;
    const isAverage = this._rating !== null && this._rating >= 2.5 && this._rating < 4;
    const isPoor = this._rating !== null && this._rating < 2.5;
    const displayRating = this._rating !== null ? this._rating.toFixed(1) : 'N/A';

    return html`
      <uui-button
        look="secondary"
        color="default"
        ?disabled=${this._processing}
        @click=${this._handleRating}
      >
        <uui-icon name="icon-rate"></uui-icon>
        Rating
        <span class="rating-badge ${isGood ? 'good' : isAverage ? 'average' : isPoor ? 'poor' : ''}">${displayRating}</span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-supplier-rating-action', SupplierRatingAction);

export default SupplierRatingAction;
