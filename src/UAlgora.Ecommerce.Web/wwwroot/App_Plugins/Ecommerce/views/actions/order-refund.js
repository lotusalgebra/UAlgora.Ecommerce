import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Order Refund Action
 * Quick action to process a refund for an order.
 */
export class OrderRefundAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _canRefund: { type: Boolean, state: true },
    _processing: { type: Boolean, state: true },
    _refundableAmount: { type: Number, state: true },
    _currencyCode: { type: String, state: true }
  };

  constructor() {
    super();
    this._canRefund = false;
    this._processing = false;
    this._refundableAmount = 0;
    this._currencyCode = 'USD';
  }

  connectedCallback() {
    super.connectedCallback();
    this._checkRefundEligibility();
  }

  async _checkRefundEligibility() {
    const workspace = document.querySelector('ecommerce-order-workspace');
    if (!workspace) return;

    const order = workspace.getOrder();
    if (!order?.id) return;

    this._currencyCode = order.currencyCode || 'USD';

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/order/${order.id}/can-refund`, {
        headers: { 'Accept': 'application/json' }
      });

      if (response.ok) {
        const result = await response.json();
        this._canRefund = result.canRefund;
        this._refundableAmount = result.refundableAmount;
      }
    } catch (error) {
      console.error('Error checking refund eligibility:', error);
    }
  }

  _formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: this._currencyCode
    }).format(amount || 0);
  }

  async _processRefund() {
    const workspace = document.querySelector('ecommerce-order-workspace');
    if (!workspace) return;

    const order = workspace.getOrder();
    if (!order?.id) return;

    const amount = prompt(`Enter refund amount (max ${this._formatCurrency(this._refundableAmount)}):`);
    if (!amount) return;

    const refundAmount = parseFloat(amount);
    if (isNaN(refundAmount) || refundAmount <= 0 || refundAmount > this._refundableAmount) {
      alert(`Invalid amount. Please enter a value between 0 and ${this._formatCurrency(this._refundableAmount)}`);
      return;
    }

    const reason = prompt('Enter refund reason (optional):');

    if (!confirm(`Are you sure you want to refund ${this._formatCurrency(refundAmount)}?`)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/order/${order.id}/refund`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({
          amount: refundAmount,
          reason: reason
        })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setOrder(result);
        await this._checkRefundEligibility();
        alert('Refund processed successfully');
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to process refund');
      }
    } catch (error) {
      console.error('Error processing refund:', error);
      alert('Failed to process refund');
    } finally {
      this._processing = false;
    }
  }

  render() {
    if (!this._canRefund) {
      return html``;
    }

    return html`
      <uui-button
        look="secondary"
        color="warning"
        ?disabled=${this._processing}
        @click=${this._processRefund}
      >
        ${this._processing ? 'Processing...' : 'Refund'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-order-refund-action', OrderRefundAction);

export default OrderRefundAction;
