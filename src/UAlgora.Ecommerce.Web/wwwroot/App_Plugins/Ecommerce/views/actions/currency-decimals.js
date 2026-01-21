import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Currency Decimal Places Action
 * Quick action to change currency decimal places.
 */
export class CurrencyDecimalsAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .decimal-badge {
      font-size: 11px;
      padding: 2px 6px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-emphasis);
      margin-left: 4px;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _decimalPlaces: { type: Number, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._decimalPlaces = 2;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-currency-workspace');
    if (workspace) {
      const currency = workspace.getCurrency();
      this._decimalPlaces = currency?.decimalPlaces ?? 2;
    }
  }

  async _handleDecimals() {
    const workspace = document.querySelector('ecommerce-currency-workspace');
    if (!workspace) return;

    const currency = workspace.getCurrency();
    if (!currency?.id) {
      alert('Please save the currency first');
      return;
    }

    const input = prompt(`Current decimal places: ${this._decimalPlaces}\nEnter new decimal places (0-4):`, String(this._decimalPlaces));
    if (input === null) return;

    const newDecimals = parseInt(input, 10);
    if (isNaN(newDecimals) || newDecimals < 0 || newDecimals > 4) {
      alert('Please enter a valid number between 0 and 4');
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/currency/${currency.id}/update-decimals`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ decimalPlaces: newDecimals })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setCurrency(result);
        this._decimalPlaces = result.decimalPlaces;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Decimal places updated to ${newDecimals}`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to update decimal places');
      }
    } catch (error) {
      console.error('Error updating decimal places:', error);
      alert('Failed to update decimal places');
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
        @click=${this._handleDecimals}
      >
        <uui-icon name="icon-calculator"></uui-icon>
        Decimals <span class="decimal-badge">${this._decimalPlaces}</span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-currency-decimals-action', CurrencyDecimalsAction);

export default CurrencyDecimalsAction;
