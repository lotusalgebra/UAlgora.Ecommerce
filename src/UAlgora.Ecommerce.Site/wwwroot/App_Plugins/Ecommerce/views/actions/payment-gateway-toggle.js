import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Payment Gateway Toggle Action
 * Quick action to activate/deactivate a payment gateway.
 */
export class PaymentGatewayToggleAction extends UmbElementMixin(LitElement) {
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
    const workspace = document.querySelector('ecommerce-payment-gateway-workspace');
    if (workspace) {
      const gateway = workspace.getGateway();
      this._isActive = gateway?.isActive ?? true;
    }
  }

  async _handleToggle() {
    const workspace = document.querySelector('ecommerce-payment-gateway-workspace');
    if (!workspace) return;

    const gateway = workspace.getGateway();
    if (!gateway?.id) {
      alert('Please save the payment gateway first');
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/payment/gateway/${gateway.id}/toggle-status`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setGateway(result);
        this._isActive = result.isActive;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: result.isActive ? 'Payment gateway activated' : 'Payment gateway deactivated',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to toggle payment gateway');
      }
    } catch (error) {
      console.error('Error toggling payment gateway:', error);
      alert('Failed to toggle payment gateway');
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

customElements.define('ecommerce-payment-gateway-toggle-action', PaymentGatewayToggleAction);

export default PaymentGatewayToggleAction;
