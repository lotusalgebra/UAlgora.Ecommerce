import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Order Details View
 * Shows order summary, totals, and payment information.
 */
export class OrderDetails extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .details-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: var(--uui-size-layout-1);
    }

    .detail-card {
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-1);
    }

    .detail-card h3 {
      margin: 0 0 var(--uui-size-space-4) 0;
      font-size: var(--uui-type-default-size);
      font-weight: bold;
      color: var(--uui-color-text);
      border-bottom: 1px solid var(--uui-color-border);
      padding-bottom: var(--uui-size-space-2);
    }

    .detail-row {
      display: flex;
      justify-content: space-between;
      padding: var(--uui-size-space-2) 0;
      border-bottom: 1px solid var(--uui-color-border-standalone);
    }

    .detail-row:last-child {
      border-bottom: none;
    }

    .detail-label {
      color: var(--uui-color-text-alt);
    }

    .detail-value {
      font-weight: 500;
      text-align: right;
    }

    .totals-section .detail-row.grand-total {
      font-size: var(--uui-type-h4-size);
      font-weight: bold;
      border-top: 2px solid var(--uui-color-border);
      margin-top: var(--uui-size-space-2);
      padding-top: var(--uui-size-space-4);
    }

    .status-badge {
      display: inline-block;
      padding: var(--uui-size-space-1) var(--uui-size-space-3);
      border-radius: var(--uui-border-radius);
      font-weight: 500;
      font-size: var(--uui-type-small-size);
    }

    .status-paid { background: #28a745; color: #fff; }
    .status-pending { background: #ffc107; color: #000; }
    .status-failed { background: #dc3545; color: #fff; }
    .status-refunded { background: #fd7e14; color: #fff; }

    .notes-section {
      margin-top: var(--uui-size-layout-1);
    }

    .note-box {
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-space-4);
      margin-top: var(--uui-size-space-2);
      font-style: italic;
      color: var(--uui-color-text-alt);
    }

    .form-group {
      margin-top: var(--uui-size-layout-1);
    }

    .form-group label {
      display: block;
      margin-bottom: var(--uui-size-space-2);
      font-weight: bold;
    }

    uui-textarea {
      width: 100%;
    }
  `;

  static properties = {
    _order: { type: Object, state: true },
    _internalNote: { type: String, state: true },
    _savingNote: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._order = null;
    this._internalNote = '';
    this._savingNote = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = this.closest('ecommerce-order-workspace');
    if (workspace) {
      this._order = workspace.getOrder();
      this._internalNote = this._order?.internalNote || '';
    }
  }

  _formatCurrency(amount, currencyCode = 'USD') {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currencyCode
    }).format(amount || 0);
  }

  _getPaymentStatusClass(status) {
    const statusLower = status?.toLowerCase() || 'pending';
    if (statusLower === 'paid' || statusLower === 'captured') return 'status-paid';
    if (statusLower === 'refunded') return 'status-refunded';
    if (statusLower === 'failed') return 'status-failed';
    return 'status-pending';
  }

  async _handleSaveNote() {
    if (!this._order?.id) return;

    try {
      this._savingNote = true;

      const response = await fetch(`/umbraco/management/api/v1/ecommerce/order/${this._order.id}/note`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ note: this._internalNote, isInternal: true })
      });

      if (response.ok) {
        this._showNotification('positive', 'Note saved successfully');
      } else {
        throw new Error('Failed to save note');
      }
    } catch (error) {
      console.error('Error saving note:', error);
      this._showNotification('danger', 'Failed to save note');
    } finally {
      this._savingNote = false;
    }
  }

  _showNotification(color, message) {
    const event = new CustomEvent('umb-notification', {
      bubbles: true,
      composed: true,
      detail: { headline: color === 'positive' ? 'Success' : 'Error', message, color }
    });
    this.dispatchEvent(event);
  }

  render() {
    if (!this._order) {
      return html`<uui-loader></uui-loader>`;
    }

    const currency = this._order.currencyCode || 'USD';

    return html`
      <div class="details-grid">
        <div class="detail-card">
          <h3>Order Information</h3>
          <div class="detail-row">
            <span class="detail-label">Order Number</span>
            <span class="detail-value">#${this._order.orderNumber}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">Date</span>
            <span class="detail-value">${new Date(this._order.createdAt).toLocaleString()}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">Status</span>
            <span class="detail-value">${this._order.status}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">Items</span>
            <span class="detail-value">${this._order.itemCount} item(s)</span>
          </div>
        </div>

        <div class="detail-card">
          <h3>Payment Information</h3>
          <div class="detail-row">
            <span class="detail-label">Payment Status</span>
            <span class="detail-value">
              <span class="status-badge ${this._getPaymentStatusClass(this._order.paymentStatus)}">
                ${this._order.paymentStatus}
              </span>
            </span>
          </div>
          <div class="detail-row">
            <span class="detail-label">Payment Method</span>
            <span class="detail-value">${this._order.paymentMethod || 'N/A'}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">Currency</span>
            <span class="detail-value">${currency}</span>
          </div>
        </div>
      </div>

      <div class="details-grid" style="margin-top: var(--uui-size-layout-1);">
        <div class="detail-card totals-section">
          <h3>Order Totals</h3>
          <div class="detail-row">
            <span class="detail-label">Subtotal</span>
            <span class="detail-value">${this._formatCurrency(this._order.subtotal, currency)}</span>
          </div>
          ${this._order.discountTotal > 0 ? html`
            <div class="detail-row">
              <span class="detail-label">Discount</span>
              <span class="detail-value" style="color: var(--uui-color-positive);">
                -${this._formatCurrency(this._order.discountTotal, currency)}
              </span>
            </div>
          ` : ''}
          <div class="detail-row">
            <span class="detail-label">Shipping</span>
            <span class="detail-value">${this._formatCurrency(this._order.shippingTotal, currency)}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">Tax</span>
            <span class="detail-value">${this._formatCurrency(this._order.taxTotal, currency)}</span>
          </div>
          <div class="detail-row grand-total">
            <span class="detail-label">Grand Total</span>
            <span class="detail-value">${this._formatCurrency(this._order.grandTotal, currency)}</span>
          </div>
        </div>

        <div class="detail-card">
          <h3>Notes</h3>
          ${this._order.customerNote ? html`
            <div>
              <strong>Customer Note:</strong>
              <div class="note-box">${this._order.customerNote}</div>
            </div>
          ` : html`
            <p style="color: var(--uui-color-text-alt);">No customer note</p>
          `}

          <div class="form-group">
            <label for="internalNote">Internal Note</label>
            <uui-textarea
              id="internalNote"
              .value=${this._internalNote}
              @input=${(e) => this._internalNote = e.target.value}
              placeholder="Add internal notes about this order..."
              rows="3"
            ></uui-textarea>
            <uui-button
              style="margin-top: var(--uui-size-space-2);"
              look="secondary"
              ?disabled=${this._savingNote}
              @click=${this._handleSaveNote}
            >
              ${this._savingNote ? 'Saving...' : 'Save Note'}
            </uui-button>
          </div>
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-order-details', OrderDetails);

export default OrderDetails;
