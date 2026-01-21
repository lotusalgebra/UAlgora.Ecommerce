import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Shipping Method Taxable Action
 * Quick action to toggle whether shipping is taxable.
 */
export class ShippingMethodTaxableAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _isTaxable: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._isTaxable = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-shipping-method-workspace');
    if (workspace) {
      const method = workspace.getShippingMethod();
      this._isTaxable = method?.isTaxable ?? true;
    }
  }

  async _handleToggle() {
    const workspace = document.querySelector('ecommerce-shipping-method-workspace');
    if (!workspace) return;

    const method = workspace.getShippingMethod();
    if (!method?.id) {
      alert('Please save the shipping method first');
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/shipping/method/${method.id}/toggle-taxable`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setShippingMethod(result);
        this._isTaxable = result.isTaxable;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: this._isTaxable ? 'Shipping is now taxable' : 'Shipping is now tax-exempt',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to toggle taxable status');
      }
    } catch (error) {
      console.error('Error toggling taxable status:', error);
      alert('Failed to toggle taxable status');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._isTaxable ? 'default' : 'warning'}"
        ?disabled=${this._processing}
        @click=${this._handleToggle}
      >
        <uui-icon name="${this._isTaxable ? 'icon-coins' : 'icon-coins-dollar'}"></uui-icon>
        ${this._processing ? 'Processing...' : this._isTaxable ? 'Taxable' : 'Tax-Exempt'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-shipping-method-taxable-action', ShippingMethodTaxableAction);

export default ShippingMethodTaxableAction;
