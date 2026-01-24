import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Currency Toggle Status Action
 * Quick action to toggle currency active/inactive status.
 */
export class CurrencyToggleAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _isActive: { type: Boolean, state: true },
    _isDefault: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._isActive = true;
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
      this._isActive = currency?.isActive ?? true;
      this._isDefault = currency?.isDefault ?? false;
    }
  }

  async _handleToggle() {
    const workspace = document.querySelector('ecommerce-currency-workspace');
    if (!workspace) return;

    const currency = workspace.getCurrency();
    if (!currency?.id) {
      alert('Please save the currency first');
      return;
    }

    if (this._isDefault && this._isActive) {
      alert('Cannot deactivate the default currency');
      return;
    }

    const action = this._isActive ? 'deactivate' : 'activate';
    const confirmed = confirm(`Are you sure you want to ${action} "${currency.name}"?`);
    if (!confirmed) return;

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/currency/${currency.id}/toggle-status`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setCurrency(result);
        this._isActive = result.isActive;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Currency ${result.isActive ? 'activated' : 'deactivated'}`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to toggle currency status');
      }
    } catch (error) {
      console.error('Error toggling currency status:', error);
      alert('Failed to toggle currency status');
    } finally {
      this._processing = false;
    }
  }

  render() {
    const disabled = this._processing || (this._isDefault && this._isActive);

    return html`
      <uui-button
        look="secondary"
        color="${this._isActive ? 'warning' : 'positive'}"
        ?disabled=${disabled}
        @click=${this._handleToggle}
      >
        <uui-icon name="${this._isActive ? 'icon-block' : 'icon-check'}"></uui-icon>
        ${this._processing ? 'Processing...' : this._isActive ? 'Deactivate' : 'Activate'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-currency-toggle-action', CurrencyToggleAction);

export default CurrencyToggleAction;
