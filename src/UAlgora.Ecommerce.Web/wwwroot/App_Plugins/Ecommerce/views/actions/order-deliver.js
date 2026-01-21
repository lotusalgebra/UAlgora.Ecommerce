import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Order Deliver Action
 * Quick action to mark an order as delivered.
 */
export class OrderDeliverAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _canDeliver: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._canDeliver = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-order-workspace');
    if (workspace) {
      const order = workspace.getOrder();
      const status = order?.status?.toLowerCase();
      // Can mark delivered if order is shipped
      this._canDeliver = order?.id && status === 'shipped';
    }
  }

  async _handleDeliver() {
    const workspace = document.querySelector('ecommerce-order-workspace');
    if (!workspace) return;

    const order = workspace.getOrder();
    if (!order?.id) {
      alert('Please save the order first');
      return;
    }

    if (!confirm(`Mark order ${order.orderNumber} as delivered?`)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/order/${order.id}/deliver`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setOrder(result);
        this._canDeliver = false;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'Order marked as delivered',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to mark order as delivered');
      }
    } catch (error) {
      console.error('Error marking order as delivered:', error);
      alert('Failed to mark order as delivered');
    } finally {
      this._processing = false;
    }
  }

  render() {
    if (!this._canDeliver) {
      return html``;
    }

    return html`
      <uui-button
        look="secondary"
        color="positive"
        ?disabled=${this._processing}
        @click=${this._handleDeliver}
      >
        <uui-icon name="icon-home"></uui-icon>
        ${this._processing ? 'Processing...' : 'Mark Delivered'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-order-deliver-action', OrderDeliverAction);

export default OrderDeliverAction;
