import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Exchange Rate Markup Action
 * Quick action to update exchange rate markup percentage.
 */
export class ExchangeRateMarkupAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .markup-badge {
      font-size: 11px;
      padding: 2px 6px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-emphasis);
      margin-left: 4px;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _markup: { type: Number, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._markup = null;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-exchange-rate-workspace');
    if (workspace) {
      const rate = workspace.getExchangeRate();
      this._markup = rate?.markupPercent ?? null;
    }
  }

  async _handleUpdateMarkup() {
    const workspace = document.querySelector('ecommerce-exchange-rate-workspace');
    if (!workspace) return;

    const rate = workspace.getExchangeRate();
    if (!rate?.id) {
      alert('Please save the exchange rate first');
      return;
    }

    const currentValue = this._markup !== null ? String(this._markup) : '0';
    const input = prompt(`Current markup: ${currentValue}%\nEnter new markup percentage (0 for none):`, currentValue);
    if (input === null) return;

    const newMarkup = parseFloat(input);
    if (isNaN(newMarkup)) {
      alert('Please enter a valid number');
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/currency/rate/${rate.id}/update-markup`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ markupPercent: newMarkup === 0 ? null : newMarkup })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setExchangeRate(result);
        this._markup = result.markupPercent;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: newMarkup === 0 ? 'Markup removed' : `Markup updated to ${newMarkup}%`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to update markup');
      }
    } catch (error) {
      console.error('Error updating markup:', error);
      alert('Failed to update markup');
    } finally {
      this._processing = false;
    }
  }

  render() {
    const displayMarkup = this._markup !== null ? `${this._markup}%` : 'None';

    return html`
      <uui-button
        look="secondary"
        color="default"
        ?disabled=${this._processing}
        @click=${this._handleUpdateMarkup}
      >
        <uui-icon name="icon-rate-up"></uui-icon>
        Markup <span class="markup-badge">${displayMarkup}</span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-exchange-rate-markup-action', ExchangeRateMarkupAction);

export default ExchangeRateMarkupAction;
