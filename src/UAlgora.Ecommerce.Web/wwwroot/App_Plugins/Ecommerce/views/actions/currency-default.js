import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Currency Set Default Action
 * Quick action to set a currency as the default.
 */
export class CurrencyDefaultAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _isDefault: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._isDefault = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-currency-workspace');
    if (workspace) {
      const currency = workspace.getCurrency();
      this._isDefault = currency?.isDefault ?? false;
    }
  }

  async _handleSetDefault() {
    const workspace = document.querySelector('ecommerce-currency-workspace');
    if (!workspace) return;

    const currency = workspace.getCurrency();
    if (!currency?.id) {
      alert('Please save the currency first');
      return;
    }

    if (this._isDefault) {
      alert('This currency is already the default');
      return;
    }

    const confirmed = confirm(`Set "${currency.name}" (${currency.code}) as the default currency? This will be used as the base currency for your store.`);
    if (!confirmed) return;

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/currency/${currency.id}/set-default`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        // Reload the currency to get updated data
        const reloadResponse = await fetch(`/umbraco/backoffice/ecommerce/currency/${currency.id}`, {
          headers: { 'Accept': 'application/json' }
        });

        if (reloadResponse.ok) {
          const result = await reloadResponse.json();
          workspace.setCurrency(result);
          this._isDefault = result.isDefault;
        }

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'Currency set as default',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to set default currency');
      }
    } catch (error) {
      console.error('Error setting default currency:', error);
      alert('Failed to set default currency');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._isDefault ? 'default' : 'positive'}"
        ?disabled=${this._processing || this._isDefault}
        @click=${this._handleSetDefault}
      >
        <uui-icon name="${this._isDefault ? 'icon-check' : 'icon-favorite'}"></uui-icon>
        ${this._processing ? 'Setting...' : this._isDefault ? 'Default Currency' : 'Set as Default'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-currency-default-action', CurrencyDefaultAction);

export default CurrencyDefaultAction;
