import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Exchange Rate Value Action
 * Quick action to update exchange rate value.
 */
export class ExchangeRateValueAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .rate-badge {
      font-size: 11px;
      padding: 2px 6px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-emphasis);
      margin-left: 4px;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _rate: { type: Number, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._rate = 1.0;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-exchange-rate-workspace');
    if (workspace) {
      const rate = workspace.getExchangeRate();
      this._rate = rate?.rate ?? 1.0;
    }
  }

  async _handleUpdateRate() {
    const workspace = document.querySelector('ecommerce-exchange-rate-workspace');
    if (!workspace) return;

    const rate = workspace.getExchangeRate();
    if (!rate?.id) {
      alert('Please save the exchange rate first');
      return;
    }

    const input = prompt(`Current rate: ${this._rate}\nEnter new exchange rate:`, String(this._rate));
    if (input === null) return;

    const newRate = parseFloat(input);
    if (isNaN(newRate) || newRate <= 0) {
      alert('Please enter a valid positive number');
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/currency/rate/${rate.id}/update-rate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ rate: newRate })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setExchangeRate(result);
        this._rate = result.rate;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Exchange rate updated to ${newRate}`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to update exchange rate');
      }
    } catch (error) {
      console.error('Error updating exchange rate:', error);
      alert('Failed to update exchange rate');
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
        @click=${this._handleUpdateRate}
      >
        <uui-icon name="icon-coin"></uui-icon>
        Rate <span class="rate-badge">${this._rate.toFixed(4)}</span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-exchange-rate-value-action', ExchangeRateValueAction);

export default ExchangeRateValueAction;
