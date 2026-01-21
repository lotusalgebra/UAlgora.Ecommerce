import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Payment Gateway Webhooks Action
 * Quick action to toggle webhooks for payment gateways.
 */
export class PaymentGatewayWebhooksAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .webhook-badge {
      font-size: 11px;
      padding: 2px 6px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-emphasis);
      margin-left: 4px;
    }

    .enabled {
      background: var(--uui-color-positive-emphasis);
      color: var(--uui-color-positive);
    }

    .disabled {
      background: var(--uui-color-warning-emphasis);
      color: var(--uui-color-warning);
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _webhooksEnabled: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._webhooksEnabled = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-payment-gateway-workspace');
    if (workspace) {
      const gateway = workspace.getPaymentGateway();
      this._webhooksEnabled = gateway?.webhooksEnabled ?? true;
    }
  }

  async _handleToggle() {
    const workspace = document.querySelector('ecommerce-payment-gateway-workspace');
    if (!workspace) return;

    const gateway = workspace.getPaymentGateway();
    if (!gateway?.id) {
      alert('Please save the payment gateway first');
      return;
    }

    const newState = !this._webhooksEnabled;
    const action = newState ? 'enable' : 'disable';

    if (!confirm(`${action.charAt(0).toUpperCase() + action.slice(1)} webhooks for "${gateway.name}"?\n\n${newState ? 'Payment notifications will be received from the gateway.' : 'Warning: Disabling webhooks may prevent payment status updates from being received.'}`)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/payment/gateway/${gateway.id}/toggle-webhooks`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setPaymentGateway(result);
        this._webhooksEnabled = result.webhooksEnabled;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Webhooks ${result.webhooksEnabled ? 'enabled' : 'disabled'}`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to toggle webhooks');
      }
    } catch (error) {
      console.error('Error toggling webhooks:', error);
      alert('Failed to toggle webhooks');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._webhooksEnabled ? 'positive' : 'warning'}"
        ?disabled=${this._processing}
        @click=${this._handleToggle}
      >
        <uui-icon name="icon-webhook"></uui-icon>
        Webhooks
        <span class="webhook-badge ${this._webhooksEnabled ? 'enabled' : 'disabled'}">
          ${this._webhooksEnabled ? 'On' : 'Off'}
        </span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-payment-gateway-webhooks-action', PaymentGatewayWebhooksAction);

export default PaymentGatewayWebhooksAction;
