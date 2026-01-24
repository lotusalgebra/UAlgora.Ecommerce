import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Order Create Shipment Action
 * Quick action to create a shipment for unfulfilled items.
 */
export class OrderCreateShipmentAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _hasUnfulfilledItems: { type: Boolean, state: true },
    _processing: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._hasUnfulfilledItems = false;
    this._processing = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._checkFulfillmentStatus();
  }

  _checkFulfillmentStatus() {
    const workspace = document.querySelector('ecommerce-order-workspace');
    if (!workspace) return;

    const order = workspace.getOrder();
    if (!order?.lines) return;

    this._hasUnfulfilledItems = order.lines.some(line =>
      (line.quantity || 0) > (line.fulfilledQuantity || 0)
    );
  }

  async _createQuickShipment() {
    const workspace = document.querySelector('ecommerce-order-workspace');
    if (!workspace) return;

    const order = workspace.getOrder();
    if (!order?.id) return;

    // Get unfulfilled items
    const unfulfilledItems = order.lines.filter(line =>
      (line.quantity || 0) > (line.fulfilledQuantity || 0)
    );

    if (unfulfilledItems.length === 0) {
      alert('All items have already been fulfilled');
      return;
    }

    const trackingNumber = prompt('Enter tracking number (optional):');
    const carrier = prompt('Enter carrier (e.g., FedEx, UPS, USPS):');

    if (!confirm('Create shipment for all unfulfilled items?')) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/order/${order.id}/shipment`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({
          carrier: carrier,
          trackingNumber: trackingNumber,
          items: unfulfilledItems.map(line => ({
            orderLineId: line.id,
            quantity: (line.quantity || 0) - (line.fulfilledQuantity || 0)
          }))
        })
      });

      if (response.ok) {
        await workspace.refreshOrder();
        this._checkFulfillmentStatus();
        alert('Shipment created successfully');
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to create shipment');
      }
    } catch (error) {
      console.error('Error creating shipment:', error);
      alert('Failed to create shipment');
    } finally {
      this._processing = false;
    }
  }

  render() {
    if (!this._hasUnfulfilledItems) {
      return html``;
    }

    return html`
      <uui-button
        look="secondary"
        ?disabled=${this._processing}
        @click=${this._createQuickShipment}
      >
        <uui-icon name="icon-truck"></uui-icon>
        ${this._processing ? 'Creating...' : 'Ship'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-order-create-shipment-action', OrderCreateShipmentAction);

export default OrderCreateShipmentAction;
