import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Order Cancel Action
 * Quick action to cancel an order.
 */
export class OrderCancelAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _canCancel: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._canCancel = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-order-workspace');
    if (workspace) {
      const order = workspace.getOrder();
      // Can cancel if status is Pending, Confirmed, or Processing and not fulfilled
      const status = order?.status?.toLowerCase();
      const fulfillmentStatus = order?.fulfillmentStatus?.toLowerCase();
      this._canCancel = order?.id &&
        ['pending', 'confirmed', 'processing'].includes(status) &&
        fulfillmentStatus !== 'fulfilled';
    }
  }

  async _handleCancel() {
    const workspace = document.querySelector('ecommerce-order-workspace');
    if (!workspace) return;

    const order = workspace.getOrder();
    if (!order?.id) {
      alert('Please save the order first');
      return;
    }

    const reason = prompt('Enter cancellation reason (optional):');
    if (reason === null) return; // User clicked Cancel

    if (!confirm(`Are you sure you want to cancel order ${order.orderNumber}?`)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/order/${order.id}/cancel`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ reason })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setOrder(result);
        this._canCancel = false;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'Order cancelled successfully',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to cancel order');
      }
    } catch (error) {
      console.error('Error cancelling order:', error);
      alert('Failed to cancel order');
    } finally {
      this._processing = false;
    }
  }

  render() {
    if (!this._canCancel) {
      return html``;
    }

    return html`
      <uui-button
        look="secondary"
        color="danger"
        ?disabled=${this._processing}
        @click=${this._handleCancel}
      >
        <uui-icon name="icon-delete"></uui-icon>
        ${this._processing ? 'Cancelling...' : 'Cancel Order'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-order-cancel-action', OrderCancelAction);

export default OrderCancelAction;
