import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Exchange Rate Toggle Status Action
 * Quick action to toggle exchange rate active/inactive status.
 */
export class ExchangeRateToggleAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _isActive: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._isActive = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-exchange-rate-workspace');
    if (workspace) {
      const rate = workspace.getExchangeRate();
      this._isActive = rate?.isActive ?? true;
    }
  }

  async _handleToggle() {
    const workspace = document.querySelector('ecommerce-exchange-rate-workspace');
    if (!workspace) return;

    const rate = workspace.getExchangeRate();
    if (!rate?.id) {
      alert('Please save the exchange rate first');
      return;
    }

    const action = this._isActive ? 'deactivate' : 'activate';
    const confirmed = confirm(`Are you sure you want to ${action} this exchange rate?`);
    if (!confirmed) return;

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/currency/rate/${rate.id}/toggle-status`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setExchangeRate(result);
        this._isActive = result.isActive;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Exchange rate ${result.isActive ? 'activated' : 'deactivated'}`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to toggle exchange rate status');
      }
    } catch (error) {
      console.error('Error toggling exchange rate status:', error);
      alert('Failed to toggle exchange rate status');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._isActive ? 'warning' : 'positive'}"
        ?disabled=${this._processing}
        @click=${this._handleToggle}
      >
        <uui-icon name="${this._isActive ? 'icon-block' : 'icon-check'}"></uui-icon>
        ${this._processing ? 'Processing...' : this._isActive ? 'Deactivate' : 'Activate'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-exchange-rate-toggle-action', ExchangeRateToggleAction);

export default ExchangeRateToggleAction;
