import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Exchange Rate Dates Action
 * Quick action to update exchange rate effective dates.
 */
export class ExchangeRateDatesAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .date-badge {
      font-size: 11px;
      padding: 2px 6px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-emphasis);
      margin-left: 4px;
    }

    .valid {
      background: var(--uui-color-positive-emphasis);
      color: white;
    }

    .expired {
      background: var(--uui-color-warning-emphasis);
      color: white;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _isCurrentlyValid: { type: Boolean, state: true },
    _effectiveTo: { type: String, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._isCurrentlyValid = true;
    this._effectiveTo = null;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-exchange-rate-workspace');
    if (workspace) {
      const rate = workspace.getExchangeRate();
      this._isCurrentlyValid = rate?.isCurrentlyValid ?? true;
      this._effectiveTo = rate?.effectiveTo;
    }
  }

  async _handleClearExpiry() {
    const workspace = document.querySelector('ecommerce-exchange-rate-workspace');
    if (!workspace) return;

    const rate = workspace.getExchangeRate();
    if (!rate?.id) {
      alert('Please save the exchange rate first');
      return;
    }

    const confirmed = confirm('Clear the expiry date? The rate will remain active indefinitely.');
    if (!confirmed) return;

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/currency/rate/${rate.id}/update-dates`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ effectiveFrom: rate.effectiveFrom, effectiveTo: null })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setExchangeRate(result);
        this._isCurrentlyValid = result.isCurrentlyValid;
        this._effectiveTo = result.effectiveTo;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'Expiry date cleared',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to clear expiry date');
      }
    } catch (error) {
      console.error('Error clearing expiry date:', error);
      alert('Failed to clear expiry date');
    } finally {
      this._processing = false;
    }
  }

  render() {
    const statusClass = this._isCurrentlyValid ? 'valid' : 'expired';
    const statusText = this._isCurrentlyValid ? 'Valid' : 'Expired';
    const hasExpiry = this._effectiveTo !== null;

    return html`
      <uui-button
        look="secondary"
        color="${hasExpiry ? 'warning' : 'default'}"
        ?disabled=${this._processing || !hasExpiry}
        @click=${this._handleClearExpiry}
        title="${hasExpiry ? 'Click to clear expiry date' : 'No expiry date set'}"
      >
        <uui-icon name="icon-calendar"></uui-icon>
        ${this._processing ? 'Updating...' : hasExpiry ? 'Clear Expiry' : 'No Expiry'}
        <span class="date-badge ${statusClass}">${statusText}</span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-exchange-rate-dates-action', ExchangeRateDatesAction);

export default ExchangeRateDatesAction;
