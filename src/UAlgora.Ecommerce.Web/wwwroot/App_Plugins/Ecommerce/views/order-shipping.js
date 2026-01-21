import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Order Shipping View
 * Displays shipments and allows creating new shipments.
 */
export class OrderShipping extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .shipments-container {
      display: flex;
      flex-direction: column;
      gap: var(--uui-size-layout-1);
    }

    .shipment-card {
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-1);
    }

    .shipment-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--uui-size-space-4);
      padding-bottom: var(--uui-size-space-4);
      border-bottom: 1px solid var(--uui-color-border);
    }

    .shipment-number {
      font-weight: bold;
      font-size: var(--uui-type-h5-size);
    }

    .status-badge {
      padding: var(--uui-size-space-1) var(--uui-size-space-3);
      border-radius: var(--uui-border-radius);
      font-weight: 500;
      font-size: var(--uui-type-small-size);
      text-transform: uppercase;
    }

    .status-pending { background: #ffc107; color: #000; }
    .status-labelcreated { background: #17a2b8; color: #fff; }
    .status-shipped { background: #6f42c1; color: #fff; }
    .status-intransit { background: #007bff; color: #fff; }
    .status-outfordelivery { background: #20c997; color: #fff; }
    .status-delivered { background: #28a745; color: #fff; }
    .status-failed { background: #dc3545; color: #fff; }

    .shipment-details {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: var(--uui-size-space-4);
      margin-bottom: var(--uui-size-space-4);
    }

    .detail-item {
      display: flex;
      flex-direction: column;
      gap: var(--uui-size-space-1);
    }

    .detail-label {
      color: var(--uui-color-text-alt);
      font-size: var(--uui-type-small-size);
    }

    .detail-value {
      font-weight: 500;
    }

    .tracking-link {
      color: var(--uui-color-interactive);
      text-decoration: none;
    }

    .tracking-link:hover {
      text-decoration: underline;
    }

    .shipment-items {
      margin-top: var(--uui-size-space-4);
    }

    .shipment-items h4 {
      margin: 0 0 var(--uui-size-space-2) 0;
      font-size: var(--uui-type-default-size);
    }

    .items-list {
      display: flex;
      flex-direction: column;
      gap: var(--uui-size-space-2);
    }

    .item-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: var(--uui-size-space-2);
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
    }

    .item-info {
      display: flex;
      flex-direction: column;
    }

    .item-name {
      font-weight: 500;
    }

    .item-sku {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      font-family: monospace;
    }

    .shipment-actions {
      display: flex;
      gap: var(--uui-size-space-2);
      margin-top: var(--uui-size-space-4);
      padding-top: var(--uui-size-space-4);
      border-top: 1px solid var(--uui-color-border);
    }

    .empty-state {
      text-align: center;
      padding: var(--uui-size-layout-3);
      color: var(--uui-color-text-alt);
    }

    .fulfillment-summary {
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-1);
      margin-bottom: var(--uui-size-layout-1);
    }

    .summary-row {
      display: flex;
      justify-content: space-between;
      padding: var(--uui-size-space-2) 0;
    }

    .summary-row:not(:last-child) {
      border-bottom: 1px solid var(--uui-color-border);
    }

    .create-shipment-form {
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-1);
      margin-top: var(--uui-size-layout-1);
    }

    .form-row {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: var(--uui-size-space-4);
      margin-bottom: var(--uui-size-space-4);
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: var(--uui-size-space-2);
    }

    .form-group label {
      font-weight: 500;
    }

    .items-selection {
      margin-top: var(--uui-size-space-4);
    }

    .items-selection h4 {
      margin: 0 0 var(--uui-size-space-2) 0;
    }

    .selectable-item {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-3);
      padding: var(--uui-size-space-3);
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
      margin-bottom: var(--uui-size-space-2);
    }

    .selectable-item.disabled {
      opacity: 0.5;
    }

    .quantity-input {
      width: 80px;
    }

    .form-actions {
      display: flex;
      gap: var(--uui-size-space-2);
      justify-content: flex-end;
      margin-top: var(--uui-size-space-4);
    }
  `;

  static properties = {
    _order: { type: Object, state: true },
    _shipments: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _showCreateForm: { type: Boolean, state: true },
    _newShipment: { type: Object, state: true },
    _processing: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._order = null;
    this._shipments = [];
    this._loading = true;
    this._showCreateForm = false;
    this._newShipment = {
      carrier: '',
      trackingNumber: '',
      items: []
    };
    this._processing = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadData();
  }

  async _loadData() {
    const workspace = this.closest('ecommerce-order-workspace');
    if (workspace) {
      this._order = workspace.getOrder();
      await this._loadShipments();
    }
    this._loading = false;
  }

  async _loadShipments() {
    if (!this._order?.id) return;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/order/${this._order.id}/shipments`, {
        headers: { 'Accept': 'application/json' }
      });

      if (response.ok) {
        this._shipments = await response.json();
      }
    } catch (error) {
      console.error('Error loading shipments:', error);
    }
  }

  _getStatusClass(status) {
    return `status-${status?.toLowerCase().replace(/\s+/g, '') || 'pending'}`;
  }

  _formatDate(date) {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  _getRemainingQuantity(line) {
    return line.quantity - (line.fulfilledQuantity || 0);
  }

  _hasUnfulfilledItems() {
    if (!this._order?.lines) return false;
    return this._order.lines.some(line => this._getRemainingQuantity(line) > 0);
  }

  _toggleCreateForm() {
    this._showCreateForm = !this._showCreateForm;
    if (this._showCreateForm) {
      this._newShipment = {
        carrier: '',
        trackingNumber: '',
        items: this._order.lines
          .filter(line => this._getRemainingQuantity(line) > 0)
          .map(line => ({
            orderLineId: line.id,
            productName: line.productName,
            sku: line.sku,
            maxQuantity: this._getRemainingQuantity(line),
            quantity: this._getRemainingQuantity(line),
            selected: true
          }))
      };
    }
  }

  _handleItemQuantityChange(index, value) {
    const items = [...this._newShipment.items];
    items[index] = { ...items[index], quantity: parseInt(value) || 0 };
    this._newShipment = { ...this._newShipment, items };
  }

  _handleItemToggle(index, checked) {
    const items = [...this._newShipment.items];
    items[index] = { ...items[index], selected: checked };
    this._newShipment = { ...this._newShipment, items };
  }

  async _createShipment() {
    const selectedItems = this._newShipment.items
      .filter(item => item.selected && item.quantity > 0)
      .map(item => ({
        orderLineId: item.orderLineId,
        quantity: item.quantity
      }));

    if (selectedItems.length === 0) {
      alert('Please select at least one item to ship');
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/order/${this._order.id}/shipment`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({
          carrier: this._newShipment.carrier,
          trackingNumber: this._newShipment.trackingNumber,
          items: selectedItems
        })
      });

      if (response.ok) {
        this._showCreateForm = false;
        await this._loadShipments();

        // Refresh the workspace
        const workspace = this.closest('ecommerce-order-workspace');
        if (workspace) {
          await workspace.refreshOrder();
          this._order = workspace.getOrder();
        }
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

  async _updateShipmentStatus(shipmentId, newStatus) {
    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/order/${this._order.id}/shipment/${shipmentId}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ status: newStatus })
      });

      if (response.ok) {
        await this._loadShipments();
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to update shipment');
      }
    } catch (error) {
      console.error('Error updating shipment:', error);
      alert('Failed to update shipment');
    } finally {
      this._processing = false;
    }
  }

  render() {
    if (this._loading) {
      return html`<uui-loader></uui-loader>`;
    }

    if (!this._order) {
      return html`<p>Order not found</p>`;
    }

    const totalOrdered = this._order.lines?.reduce((sum, l) => sum + l.quantity, 0) || 0;
    const totalFulfilled = this._order.lines?.reduce((sum, l) => sum + (l.fulfilledQuantity || 0), 0) || 0;
    const totalRemaining = totalOrdered - totalFulfilled;

    return html`
      <div class="shipments-container">
        <div class="fulfillment-summary">
          <div class="summary-row">
            <span>Total Items Ordered</span>
            <strong>${totalOrdered}</strong>
          </div>
          <div class="summary-row">
            <span>Items Fulfilled</span>
            <strong>${totalFulfilled}</strong>
          </div>
          <div class="summary-row">
            <span>Items Remaining</span>
            <strong style="color: ${totalRemaining > 0 ? 'var(--uui-color-warning)' : 'var(--uui-color-positive)'}">
              ${totalRemaining}
            </strong>
          </div>
          <div class="summary-row">
            <span>Fulfillment Status</span>
            <strong>${this._order.fulfillmentStatus || 'Unfulfilled'}</strong>
          </div>
        </div>

        ${this._hasUnfulfilledItems() && !this._showCreateForm ? html`
          <uui-button look="primary" @click=${this._toggleCreateForm}>
            <uui-icon name="icon-truck"></uui-icon>
            Create Shipment
          </uui-button>
        ` : ''}

        ${this._showCreateForm ? this._renderCreateForm() : ''}

        ${this._shipments.length === 0 && !this._showCreateForm ? html`
          <div class="empty-state">
            <uui-icon name="icon-truck" style="font-size: 48px;"></uui-icon>
            <p>No shipments yet</p>
          </div>
        ` : ''}

        ${this._shipments.map(shipment => this._renderShipment(shipment))}
      </div>
    `;
  }

  _renderCreateForm() {
    return html`
      <div class="create-shipment-form">
        <h3 style="margin: 0 0 var(--uui-size-space-4) 0;">Create New Shipment</h3>

        <div class="form-row">
          <div class="form-group">
            <label>Carrier</label>
            <uui-input
              .value=${this._newShipment.carrier}
              @input=${(e) => this._newShipment = { ...this._newShipment, carrier: e.target.value }}
              placeholder="e.g., FedEx, UPS, USPS"
            ></uui-input>
          </div>
          <div class="form-group">
            <label>Tracking Number</label>
            <uui-input
              .value=${this._newShipment.trackingNumber}
              @input=${(e) => this._newShipment = { ...this._newShipment, trackingNumber: e.target.value }}
              placeholder="Enter tracking number"
            ></uui-input>
          </div>
        </div>

        <div class="items-selection">
          <h4>Select Items to Ship</h4>
          ${this._newShipment.items.map((item, index) => html`
            <div class="selectable-item">
              <uui-checkbox
                ?checked=${item.selected}
                @change=${(e) => this._handleItemToggle(index, e.target.checked)}
              ></uui-checkbox>
              <div class="item-info" style="flex: 1;">
                <span class="item-name">${item.productName}</span>
                <span class="item-sku">SKU: ${item.sku}</span>
              </div>
              <div style="display: flex; align-items: center; gap: var(--uui-size-space-2);">
                <uui-input
                  type="number"
                  class="quantity-input"
                  min="1"
                  max=${item.maxQuantity}
                  .value=${item.quantity}
                  ?disabled=${!item.selected}
                  @input=${(e) => this._handleItemQuantityChange(index, e.target.value)}
                ></uui-input>
                <span>/ ${item.maxQuantity}</span>
              </div>
            </div>
          `)}
        </div>

        <div class="form-actions">
          <uui-button look="secondary" @click=${this._toggleCreateForm}>
            Cancel
          </uui-button>
          <uui-button
            look="primary"
            color="positive"
            ?disabled=${this._processing}
            @click=${this._createShipment}
          >
            ${this._processing ? 'Creating...' : 'Create Shipment'}
          </uui-button>
        </div>
      </div>
    `;
  }

  _renderShipment(shipment) {
    return html`
      <div class="shipment-card">
        <div class="shipment-header">
          <div>
            <span class="shipment-number">${shipment.shipmentNumber || 'Shipment'}</span>
            <span style="color: var(--uui-color-text-alt); margin-left: var(--uui-size-space-2);">
              ${this._formatDate(shipment.createdAt)}
            </span>
          </div>
          <span class="status-badge ${this._getStatusClass(shipment.status)}">
            ${shipment.status}
          </span>
        </div>

        <div class="shipment-details">
          <div class="detail-item">
            <span class="detail-label">Carrier</span>
            <span class="detail-value">${shipment.carrier || 'Not specified'}</span>
          </div>
          <div class="detail-item">
            <span class="detail-label">Tracking Number</span>
            <span class="detail-value">
              ${shipment.trackingUrl
                ? html`<a href="${shipment.trackingUrl}" target="_blank" class="tracking-link">${shipment.trackingNumber}</a>`
                : shipment.trackingNumber || 'Not available'
              }
            </span>
          </div>
          <div class="detail-item">
            <span class="detail-label">Shipped</span>
            <span class="detail-value">${this._formatDate(shipment.shippedAt)}</span>
          </div>
          <div class="detail-item">
            <span class="detail-label">Delivered</span>
            <span class="detail-value">${this._formatDate(shipment.deliveredAt)}</span>
          </div>
        </div>

        ${shipment.items?.length > 0 ? html`
          <div class="shipment-items">
            <h4>Items (${shipment.totalItems})</h4>
            <div class="items-list">
              ${shipment.items.map(item => html`
                <div class="item-row">
                  <div class="item-info">
                    <span class="item-name">${item.productName}</span>
                    <span class="item-sku">SKU: ${item.sku}</span>
                  </div>
                  <span>x${item.quantity}</span>
                </div>
              `)}
            </div>
          </div>
        ` : ''}

        <div class="shipment-actions">
          ${shipment.status === 'Pending' || shipment.status === 'LabelCreated' ? html`
            <uui-button
              look="secondary"
              ?disabled=${this._processing}
              @click=${() => this._updateShipmentStatus(shipment.id, 'InTransit')}
            >
              Mark as Shipped
            </uui-button>
          ` : ''}
          ${shipment.status === 'PickedUp' || shipment.status === 'InTransit' || shipment.status === 'OutForDelivery' ? html`
            <uui-button
              look="secondary"
              color="positive"
              ?disabled=${this._processing}
              @click=${() => this._updateShipmentStatus(shipment.id, 'Delivered')}
            >
              Mark as Delivered
            </uui-button>
          ` : ''}
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-order-shipping', OrderShipping);

export default OrderShipping;
