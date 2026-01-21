import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Purchase Order Workspace
 * Main container for purchase order editing.
 */
export class PurchaseOrderWorkspace extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      height: 100%;
    }

    .workspace-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: var(--uui-size-layout-1);
      border-bottom: 1px solid var(--uui-color-border);
      background: var(--uui-color-surface);
    }

    .header-info {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-4);
    }

    .header-info uui-icon {
      font-size: 32px;
      color: var(--uui-color-interactive);
    }

    .header-title h1 {
      margin: 0;
      font-size: var(--uui-type-h4-size);
    }

    .header-meta {
      display: flex;
      gap: var(--uui-size-space-3);
      margin-top: var(--uui-size-space-1);
    }

    .status-badge {
      display: inline-block;
      padding: var(--uui-size-space-1) var(--uui-size-space-3);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
      font-weight: 500;
    }

    .status-draft { background: var(--uui-color-surface-alt); color: var(--uui-color-text-alt); }
    .status-pending { background: var(--uui-color-warning-emphasis); color: var(--uui-color-warning); }
    .status-approved { background: var(--uui-color-default-emphasis); color: var(--uui-color-default); }
    .status-sent { background: var(--uui-color-default-emphasis); color: var(--uui-color-default); }
    .status-partiallyreceived { background: var(--uui-color-warning-emphasis); color: var(--uui-color-warning); }
    .status-received { background: var(--uui-color-positive-emphasis); color: var(--uui-color-positive); }
    .status-completed { background: var(--uui-color-positive-emphasis); color: var(--uui-color-positive); }
    .status-cancelled { background: var(--uui-color-danger-emphasis); color: var(--uui-color-danger); }

    .badge-supplier {
      background: var(--uui-color-default-emphasis);
      color: var(--uui-color-default);
    }

    .total-badge {
      font-weight: 600;
    }
  `;

  static properties = {
    _order: { type: Object, state: true },
    _isNew: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._order = {
      orderNumber: '',
      supplierId: null,
      supplierName: '',
      warehouseId: null,
      warehouseName: '',
      status: 'Draft',
      orderDate: new Date().toISOString().split('T')[0],
      expectedDeliveryDate: null,
      currencyCode: 'USD',
      subtotal: 0,
      taxAmount: 0,
      shippingCost: 0,
      discountAmount: 0,
      total: 0,
      supplierReference: '',
      paymentTerms: '',
      shippingMethod: '',
      trackingNumber: '',
      notes: '',
      internalNotes: '',
      items: []
    };
    this._isNew = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadOrder();
  }

  async _loadOrder() {
    const path = window.location.pathname;
    const editMatch = path.match(/\/purchase-order\/edit\/([^/]+)/);

    if (editMatch) {
      this._isNew = false;
      const orderId = editMatch[1];

      try {
        const response = await fetch(`/umbraco/management/api/v1/ecommerce/inventory/purchase-order/${orderId}`, {
          headers: { 'Accept': 'application/json' }
        });

        if (response.ok) {
          this._order = await response.json();
        }
      } catch (error) {
        console.error('Error loading purchase order:', error);
      }
    }
  }

  getOrder() {
    return this._order;
  }

  setOrder(order) {
    this._order = { ...order };
    this.requestUpdate();
  }

  isNewOrder() {
    return this._isNew;
  }

  _getStatusLabel(status) {
    const labels = {
      'Draft': 'Draft',
      'Pending': 'Pending Approval',
      'Approved': 'Approved',
      'Sent': 'Sent to Supplier',
      'PartiallyReceived': 'Partially Received',
      'Received': 'Received',
      'Completed': 'Completed',
      'Cancelled': 'Cancelled'
    };
    return labels[status] || status;
  }

  _formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: this._order.currencyCode || 'USD'
    }).format(amount || 0);
  }

  render() {
    return html`
      <div class="workspace-header">
        <div class="header-info">
          <uui-icon name="icon-receipt-dollar"></uui-icon>
          <div class="header-title">
            <h1>${this._isNew ? 'New Purchase Order' : `PO #${this._order.orderNumber}`}</h1>
            <div class="header-meta">
              <span class="status-badge status-${this._order.status?.toLowerCase()}">
                ${this._getStatusLabel(this._order.status)}
              </span>
              ${this._order.supplierName ? html`
                <span class="status-badge badge-supplier">${this._order.supplierName}</span>
              ` : ''}
              <span class="status-badge total-badge">${this._formatCurrency(this._order.total)}</span>
            </div>
          </div>
        </div>
        <slot name="action-menu"></slot>
      </div>
      <slot></slot>
    `;
  }
}

customElements.define('ecommerce-purchase-order-workspace', PurchaseOrderWorkspace);

export default PurchaseOrderWorkspace;
