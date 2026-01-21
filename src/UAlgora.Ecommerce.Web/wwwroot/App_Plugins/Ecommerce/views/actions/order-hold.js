import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Order Hold Action
 * Quick action to put an order on hold or release it.
 */
export class OrderHoldAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _isOnHold: { type: Boolean, state: true },
    _canHold: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._isOnHold = false;
    this._canHold = false;
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
      this._isOnHold = status === 'onhold';
      // Can hold/release if order is not completed, cancelled, or refunded
      this._canHold = order?.id && !['completed', 'cancelled', 'refunded', 'delivered'].includes(status);
    }
  }

  async _handleToggleHold() {
    const workspace = document.querySelector('ecommerce-order-workspace');
    if (!workspace) return;

    const order = workspace.getOrder();
    if (!order?.id) {
      alert('Please save the order first');
      return;
    }

    const action = this._isOnHold ? 'release' : 'hold';
    const confirmMessage = this._isOnHold
      ? `Release order ${order.orderNumber} from hold?`
      : `Put order ${order.orderNumber} on hold?`;

    if (!confirm(confirmMessage)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/order/${order.id}/${action}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setOrder(result);
        this._isOnHold = result.status?.toLowerCase() === 'onhold';

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: this._isOnHold ? 'Order placed on hold' : 'Order released from hold',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || `Failed to ${action} order`);
      }
    } catch (error) {
      console.error(`Error ${action}ing order:`, error);
      alert(`Failed to ${action} order`);
    } finally {
      this._processing = false;
    }
  }

  render() {
    if (!this._canHold && !this._isOnHold) {
      return html``;
    }

    return html`
      <uui-button
        look="secondary"
        color="${this._isOnHold ? 'positive' : 'warning'}"
        ?disabled=${this._processing}
        @click=${this._handleToggleHold}
      >
        <uui-icon name="${this._isOnHold ? 'icon-play' : 'icon-pause'}"></uui-icon>
        ${this._processing ? 'Processing...' : this._isOnHold ? 'Release Hold' : 'Put On Hold'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-order-hold-action', OrderHoldAction);

export default OrderHoldAction;
