import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Order Ship Action
 * Quick action to mark an order as shipped with tracking info.
 */
export class OrderShipAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _canShip: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._canShip = false;
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
      // Can ship if order is Confirmed or Processing
      this._canShip = order?.id && ['confirmed', 'processing'].includes(status);
    }
  }

  async _handleShip() {
    const workspace = document.querySelector('ecommerce-order-workspace');
    if (!workspace) return;

    const order = workspace.getOrder();
    if (!order?.id) {
      alert('Please save the order first');
      return;
    }

    const trackingNumber = prompt('Enter tracking number (optional):');
    if (trackingNumber === null) return;

    let carrier = null;
    if (trackingNumber) {
      carrier = prompt('Enter carrier name (e.g., UPS, FedEx, USPS):');
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/order/${order.id}/ship`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({
          trackingNumber: trackingNumber || null,
          carrier: carrier || null
        })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setOrder(result);
        this._canShip = false;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'Order marked as shipped',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to mark order as shipped');
      }
    } catch (error) {
      console.error('Error shipping order:', error);
      alert('Failed to mark order as shipped');
    } finally {
      this._processing = false;
    }
  }

  render() {
    if (!this._canShip) {
      return html``;
    }

    return html`
      <uui-button
        look="secondary"
        color="positive"
        ?disabled=${this._processing}
        @click=${this._handleShip}
      >
        <uui-icon name="icon-truck"></uui-icon>
        ${this._processing ? 'Shipping...' : 'Mark Shipped'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-order-ship-action', OrderShipAction);

export default OrderShipAction;
