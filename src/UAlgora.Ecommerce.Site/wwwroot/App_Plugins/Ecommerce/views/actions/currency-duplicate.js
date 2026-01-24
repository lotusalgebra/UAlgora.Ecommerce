import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Currency Duplicate Action
 * Quick action to duplicate a currency configuration.
 */
export class CurrencyDuplicateAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
  }

  async _handleDuplicate() {
    const workspace = document.querySelector('ecommerce-currency-workspace');
    if (!workspace) return;

    const currency = workspace.getCurrency();
    if (!currency?.id) {
      alert('Please save the currency first');
      return;
    }

    const confirmed = confirm(`Create a copy of "${currency.name}" (${currency.code})? The copy will be created as inactive with a modified code.`);
    if (!confirmed) return;

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/currency/${currency.id}/duplicate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Currency duplicated as "${result.code}"`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);

        // Navigate to the new currency
        if (result.id) {
          window.location.href = `/umbraco/backoffice/ecommerce/currency/edit/${result.id}`;
        }
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to duplicate currency');
      }
    } catch (error) {
      console.error('Error duplicating currency:', error);
      alert('Failed to duplicate currency');
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
        @click=${this._handleDuplicate}
      >
        <uui-icon name="icon-documents"></uui-icon>
        ${this._processing ? 'Duplicating...' : 'Duplicate'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-currency-duplicate-action', CurrencyDuplicateAction);

export default CurrencyDuplicateAction;
