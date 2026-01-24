import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Payment Gateway Sandbox Toggle Action
 * Quick action to toggle sandbox/live mode for a payment gateway.
 */
export class PaymentGatewaySandboxAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _isSandbox: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._isSandbox = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-payment-gateway-workspace');
    if (workspace) {
      const gateway = workspace.getGateway();
      this._isSandbox = gateway?.isSandbox ?? true;
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

    const newMode = this._isSandbox ? 'LIVE' : 'Sandbox';
    const confirmed = confirm(`Switch to ${newMode} mode? ${!this._isSandbox ? 'This will use sandbox/test credentials.' : 'This will process REAL payments!'}`);
    if (!confirmed) return;

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/payment/gateway/${gateway.id}/toggle-sandbox`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setGateway(result);
        this._isSandbox = result.isSandbox;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: result.isSandbox ? 'Switched to Sandbox mode' : 'Switched to LIVE mode',
            color: result.isSandbox ? 'positive' : 'warning'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to toggle sandbox mode');
      }
    } catch (error) {
      console.error('Error toggling sandbox mode:', error);
      alert('Failed to toggle sandbox mode');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._isSandbox ? 'positive' : 'danger'}"
        ?disabled=${this._processing}
        @click=${this._handleToggle}
      >
        <uui-icon name="${this._isSandbox ? 'icon-lab' : 'icon-alert'}"></uui-icon>
        ${this._processing ? 'Switching...' : this._isSandbox ? 'Sandbox Mode' : 'LIVE Mode'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-payment-gateway-sandbox-action', PaymentGatewaySandboxAction);

export default PaymentGatewaySandboxAction;
