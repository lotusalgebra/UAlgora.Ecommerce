import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Payment Gateway Duplicate Action
 * Quick action to create a copy of the current payment gateway.
 */
export class PaymentGatewayDuplicateAction extends UmbElementMixin(LitElement) {
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
    const workspace = document.querySelector('ecommerce-payment-gateway-workspace');
    if (!workspace) return;

    const gateway = workspace.getGateway();
    if (!gateway?.id) {
      alert('Please save the payment gateway first before duplicating');
      return;
    }

    const confirmed = confirm(`Are you sure you want to duplicate "${gateway.name}"? Note: Live credentials will not be copied for security reasons.`);
    if (!confirmed) return;

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/payment/gateway/${gateway.id}/duplicate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const duplicatedGateway = await response.json();

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Payment gateway duplicated: "${duplicatedGateway.name}"`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);

        // Navigate to the duplicated gateway
        window.location.href = `/umbraco/section/ecommerce/workspace/payment-gateway/edit/${duplicatedGateway.id}`;
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to duplicate payment gateway');
      }
    } catch (error) {
      console.error('Error duplicating payment gateway:', error);
      alert('Failed to duplicate payment gateway');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        ?disabled=${this._processing}
        @click=${this._handleDuplicate}
      >
        <uui-icon name="icon-documents"></uui-icon>
        ${this._processing ? 'Duplicating...' : 'Duplicate'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-payment-gateway-duplicate-action', PaymentGatewayDuplicateAction);

export default PaymentGatewayDuplicateAction;
