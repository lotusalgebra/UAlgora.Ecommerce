import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Payment Method Set Default Action
 * Quick action to set a payment method as the default.
 */
export class PaymentMethodDefaultAction extends UmbElementMixin(LitElement) {
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
    const workspace = document.querySelector('ecommerce-payment-method-workspace');
    if (workspace) {
      const method = workspace.getMethod();
      this._isDefault = method?.isDefault ?? false;
    }
  }

  async _handleSetDefault() {
    const workspace = document.querySelector('ecommerce-payment-method-workspace');
    if (!workspace) return;

    const method = workspace.getMethod();
    if (!method?.id) {
      alert('Please save the payment method first');
      return;
    }

    if (this._isDefault) {
      alert('This method is already the default');
      return;
    }

    const confirmed = confirm(`Set "${method.name}" as the default payment method?`);
    if (!confirmed) return;

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/payment/method/${method.id}/set-default`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        // Reload the method to get updated data
        const reloadResponse = await fetch(`/umbraco/management/api/v1/ecommerce/payment/method/${method.id}`, {
          headers: { 'Accept': 'application/json' }
        });

        if (reloadResponse.ok) {
          const result = await reloadResponse.json();
          workspace.setMethod(result);
          this._isDefault = result.isDefault;
        }

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'Payment method set as default',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to set default method');
      }
    } catch (error) {
      console.error('Error setting default method:', error);
      alert('Failed to set default method');
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
        ${this._processing ? 'Setting...' : this._isDefault ? 'Default Method' : 'Set as Default'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-payment-method-default-action', PaymentMethodDefaultAction);

export default PaymentMethodDefaultAction;
