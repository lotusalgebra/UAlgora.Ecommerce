import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Currency Rounding Action
 * Quick action to change currency rounding mode.
 */
export class CurrencyRoundingAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .rounding-badge {
      font-size: 11px;
      padding: 2px 6px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-emphasis);
      margin-left: 4px;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _rounding: { type: String, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._rounding = 'Standard';
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-currency-workspace');
    if (workspace) {
      const currency = workspace.getCurrency();
      const roundingMap = { 0: 'Standard', 1: 'Up', 2: 'Down', 3: 'ToIncrement' };
      this._rounding = roundingMap[currency?.rounding] ?? 'Standard';
    }
  }

  async _handleRounding() {
    const workspace = document.querySelector('ecommerce-currency-workspace');
    if (!workspace) return;

    const currency = workspace.getCurrency();
    if (!currency?.id) {
      alert('Please save the currency first');
      return;
    }

    const options = ['Standard', 'Up', 'Down', 'ToIncrement'];
    const currentIndex = options.indexOf(this._rounding);
    const newRounding = options[(currentIndex + 1) % options.length];

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/currency/${currency.id}/update-rounding`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ rounding: newRounding, increment: currency.roundingIncrement })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setCurrency(result);
        const roundingMap = { 0: 'Standard', 1: 'Up', 2: 'Down', 3: 'ToIncrement' };
        this._rounding = roundingMap[result.rounding] ?? 'Standard';

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Rounding mode set to ${this._rounding}`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to update rounding mode');
      }
    } catch (error) {
      console.error('Error updating rounding mode:', error);
      alert('Failed to update rounding mode');
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
        @click=${this._handleRounding}
      >
        <uui-icon name="icon-autofill"></uui-icon>
        Round <span class="rounding-badge">${this._rounding}</span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-currency-rounding-action', CurrencyRoundingAction);

export default CurrencyRoundingAction;
