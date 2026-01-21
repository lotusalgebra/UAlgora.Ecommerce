import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Purchase Order Editor
 * Form for editing purchase order details.
 */
export class PurchaseOrderEditor extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .editor-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: var(--uui-size-layout-1);
    }

    uui-box {
      margin-bottom: var(--uui-size-layout-1);
    }

    .form-group {
      margin-bottom: var(--uui-size-space-5);
    }

    .form-group label {
      display: block;
      margin-bottom: var(--uui-size-space-2);
      font-weight: 500;
    }

    uui-input, uui-textarea, uui-select {
      width: 100%;
    }

    .help-text {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      margin-top: var(--uui-size-space-1);
    }

    .totals-section {
      border-top: 1px solid var(--uui-color-border);
      padding-top: var(--uui-size-space-4);
      margin-top: var(--uui-size-space-4);
    }

    .total-row {
      display: flex;
      justify-content: space-between;
      padding: var(--uui-size-space-2) 0;
    }

    .total-row.grand-total {
      font-weight: 600;
      font-size: var(--uui-type-h5-size);
      border-top: 2px solid var(--uui-color-border);
      padding-top: var(--uui-size-space-3);
      margin-top: var(--uui-size-space-2);
    }

    .amount {
      font-variant-numeric: tabular-nums;
    }
  `;

  static properties = {
    _order: { type: Object, state: true },
    _suppliers: { type: Array, state: true },
    _warehouses: { type: Array, state: true }
  };

  constructor() {
    super();
    this._order = {};
    this._suppliers = [];
    this._warehouses = [];
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadOptions();

    const checkWorkspace = () => {
      const workspace = this.closest('ecommerce-purchase-order-workspace');
      if (workspace) {
        this._order = workspace.getOrder();
        this.requestUpdate();
      } else {
        setTimeout(checkWorkspace, 100);
      }
    };
    checkWorkspace();
  }

  async _loadOptions() {
    try {
      const [suppliersRes, warehousesRes] = await Promise.all([
        fetch('/umbraco/management/api/v1/ecommerce/inventory/suppliers', { headers: { 'Accept': 'application/json' } }),
        fetch('/umbraco/management/api/v1/ecommerce/inventory/warehouses', { headers: { 'Accept': 'application/json' } })
      ]);

      if (suppliersRes.ok) this._suppliers = await suppliersRes.json();
      if (warehousesRes.ok) this._warehouses = await warehousesRes.json();
    } catch (error) {
      console.error('Error loading options:', error);
    }
  }

  _updateOrder(field, value) {
    this._order = { ...this._order, [field]: value };
    const workspace = this.closest('ecommerce-purchase-order-workspace');
    if (workspace) {
      workspace.setOrder(this._order);
    }
  }

  _formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: this._order.currencyCode || 'USD'
    }).format(amount || 0);
  }

  render() {
    return html`
      <div class="editor-grid">
        <uui-box headline="Order Details">
          <div class="form-group">
            <label>Supplier *</label>
            <uui-select
              .value=${this._order.supplierId || ''}
              @change=${(e) => this._updateOrder('supplierId', e.target.value)}
            >
              <option value="">Select a supplier...</option>
              ${this._suppliers.map(s => html`
                <option value="${s.id}" ?selected=${s.id === this._order.supplierId}>${s.name}</option>
              `)}
            </uui-select>
          </div>

          <div class="form-group">
            <label>Destination Warehouse *</label>
            <uui-select
              .value=${this._order.warehouseId || ''}
              @change=${(e) => this._updateOrder('warehouseId', e.target.value)}
            >
              <option value="">Select a warehouse...</option>
              ${this._warehouses.map(w => html`
                <option value="${w.id}" ?selected=${w.id === this._order.warehouseId}>${w.name}</option>
              `)}
            </uui-select>
          </div>

          <div class="form-group">
            <label>Order Date</label>
            <uui-input
              type="date"
              .value=${this._order.orderDate?.split('T')[0] || ''}
              @input=${(e) => this._updateOrder('orderDate', e.target.value)}
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Expected Delivery Date</label>
            <uui-input
              type="date"
              .value=${this._order.expectedDeliveryDate?.split('T')[0] || ''}
              @input=${(e) => this._updateOrder('expectedDeliveryDate', e.target.value)}
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Currency</label>
            <uui-select
              .value=${this._order.currencyCode || 'USD'}
              @change=${(e) => this._updateOrder('currencyCode', e.target.value)}
            >
              <option value="USD">USD - US Dollar</option>
              <option value="EUR">EUR - Euro</option>
              <option value="GBP">GBP - British Pound</option>
              <option value="CAD">CAD - Canadian Dollar</option>
              <option value="AUD">AUD - Australian Dollar</option>
            </uui-select>
          </div>
        </uui-box>

        <uui-box headline="Shipping & Reference">
          <div class="form-group">
            <label>Supplier Reference</label>
            <uui-input
              .value=${this._order.supplierReference || ''}
              @input=${(e) => this._updateOrder('supplierReference', e.target.value)}
              placeholder="Supplier's order number"
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Payment Terms</label>
            <uui-input
              .value=${this._order.paymentTerms || ''}
              @input=${(e) => this._updateOrder('paymentTerms', e.target.value)}
              placeholder="e.g., Net 30"
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Shipping Method</label>
            <uui-input
              .value=${this._order.shippingMethod || ''}
              @input=${(e) => this._updateOrder('shippingMethod', e.target.value)}
              placeholder="e.g., FedEx Ground"
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Tracking Number</label>
            <uui-input
              .value=${this._order.trackingNumber || ''}
              @input=${(e) => this._updateOrder('trackingNumber', e.target.value)}
            ></uui-input>
          </div>
        </uui-box>

        <uui-box headline="Notes">
          <div class="form-group">
            <label>Notes to Supplier</label>
            <uui-textarea
              .value=${this._order.notes || ''}
              @input=${(e) => this._updateOrder('notes', e.target.value)}
              placeholder="Notes that will be sent to the supplier"
              rows="3"
            ></uui-textarea>
          </div>

          <div class="form-group">
            <label>Internal Notes</label>
            <uui-textarea
              .value=${this._order.internalNotes || ''}
              @input=${(e) => this._updateOrder('internalNotes', e.target.value)}
              placeholder="Internal notes (not shared with supplier)"
              rows="3"
            ></uui-textarea>
          </div>
        </uui-box>

        <uui-box headline="Order Totals">
          <div class="totals-section">
            <div class="total-row">
              <span>Subtotal</span>
              <span class="amount">${this._formatCurrency(this._order.subtotal)}</span>
            </div>
            <div class="total-row">
              <span>Tax</span>
              <span class="amount">${this._formatCurrency(this._order.taxAmount)}</span>
            </div>
            <div class="total-row">
              <span>Shipping</span>
              <span class="amount">${this._formatCurrency(this._order.shippingCost)}</span>
            </div>
            <div class="total-row">
              <span>Discount</span>
              <span class="amount">-${this._formatCurrency(this._order.discountAmount)}</span>
            </div>
            <div class="total-row grand-total">
              <span>Total</span>
              <span class="amount">${this._formatCurrency(this._order.total)}</span>
            </div>
          </div>
        </uui-box>
      </div>
    `;
  }
}

customElements.define('ecommerce-purchase-order-editor', PurchaseOrderEditor);

export default PurchaseOrderEditor;
