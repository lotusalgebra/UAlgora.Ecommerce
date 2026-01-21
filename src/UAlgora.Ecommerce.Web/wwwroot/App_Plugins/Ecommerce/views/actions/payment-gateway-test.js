import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Payment Gateway Test Connection Action
 * Quick action to test the connection to a payment gateway.
 */
export class PaymentGatewayTestAction extends UmbElementMixin(LitElement) {
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

  async _handleTest() {
    const workspace = document.querySelector('ecommerce-payment-gateway-workspace');
    if (!workspace) return;

    const gateway = workspace.getGateway();
    if (!gateway?.id) {
      alert('Please save the payment gateway first');
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/payment/gateway/${gateway.id}/test`, {
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
            headline: result.success ? 'Connection Successful' : 'Connection Failed',
            message: result.message || (result.success ? 'Gateway connection verified' : 'Unable to connect to gateway'),
            color: result.success ? 'positive' : 'danger'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to test connection');
      }
    } catch (error) {
      console.error('Error testing gateway connection:', error);
      alert('Failed to test gateway connection');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        ?disabled=${this._processing}
        @click=${this._handleTest}
      >
        <uui-icon name="icon-connection"></uui-icon>
        ${this._processing ? 'Testing...' : 'Test Connection'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-payment-gateway-test-action', PaymentGatewayTestAction);

export default PaymentGatewayTestAction;
