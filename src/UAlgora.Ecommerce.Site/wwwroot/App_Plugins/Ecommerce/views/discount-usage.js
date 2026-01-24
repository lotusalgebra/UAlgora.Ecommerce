import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Discount Usage View
 * Displays usage history and statistics for the discount.
 */
export class DiscountUsage extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .stats-row {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: var(--uui-size-layout-1);
      margin-bottom: var(--uui-size-layout-1);
    }

    .stat-card {
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-1);
      text-align: center;
    }

    .stat-value {
      font-size: var(--uui-type-h2-size);
      font-weight: bold;
      color: var(--uui-color-interactive);
      margin-bottom: var(--uui-size-space-2);
    }

    .stat-value.positive {
      color: var(--uui-color-positive);
    }

    .stat-label {
      color: var(--uui-color-text-alt);
      font-size: var(--uui-type-small-size);
    }

    .section-title {
      font-size: var(--uui-type-h4-size);
      font-weight: bold;
      margin: var(--uui-size-layout-2) 0 var(--uui-size-layout-1) 0;
    }

    .usage-table {
      width: 100%;
      border-collapse: collapse;
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
    }

    .usage-table th,
    .usage-table td {
      padding: var(--uui-size-space-4);
      text-align: left;
      border-bottom: 1px solid var(--uui-color-border);
    }

    .usage-table th {
      background: var(--uui-color-surface-alt);
      font-weight: bold;
    }

    .usage-table tr:last-child td {
      border-bottom: none;
    }

    .usage-table tr:hover {
      background: var(--uui-color-surface-emphasis);
    }

    .order-link {
      color: var(--uui-color-interactive);
      cursor: pointer;
      font-family: monospace;
    }

    .order-link:hover {
      text-decoration: underline;
    }

    .amount-cell {
      font-weight: 500;
      color: var(--uui-color-positive);
    }

    .empty-state {
      text-align: center;
      padding: var(--uui-size-layout-3);
      color: var(--uui-color-text-alt);
    }

    .empty-state uui-icon {
      font-size: 48px;
      margin-bottom: var(--uui-size-space-4);
    }

    .loading {
      display: flex;
      justify-content: center;
      padding: var(--uui-size-layout-2);
    }

    .pagination {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-top: var(--uui-size-layout-1);
      padding: var(--uui-size-space-4);
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
    }

    .pagination-info {
      color: var(--uui-color-text-alt);
    }

    .pagination-buttons {
      display: flex;
      gap: var(--uui-size-space-2);
    }
  `;

  static properties = {
    _discount: { type: Object, state: true },
    _usageHistory: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _page: { type: Number, state: true },
    _pageSize: { type: Number, state: true },
    _totalCount: { type: Number, state: true }
  };

  constructor() {
    super();
    this._discount = null;
    this._usageHistory = [];
    this._loading = true;
    this._page = 1;
    this._pageSize = 10;
    this._totalCount = 0;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = this.closest('ecommerce-discount-workspace');
    if (workspace) {
      this._discount = workspace.getDiscount();
      if (this._discount?.id) {
        this._loadUsageHistory();
      } else {
        this._loading = false;
      }
    }
  }

  async _loadUsageHistory() {
    try {
      this._loading = true;

      const skip = (this._page - 1) * this._pageSize;
      const params = new URLSearchParams({
        skip: skip.toString(),
        take: this._pageSize.toString()
      });

      const response = await fetch(`/umbraco/management/api/v1/ecommerce/discount/${this._discount.id}/usage?${params}`, {
        headers: {
          'Accept': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error('Failed to load usage history');
      }

      const data = await response.json();
      this._usageHistory = data.items || [];
      this._totalCount = data.total || this._usageHistory.length;
    } catch (error) {
      console.error('Error loading usage history:', error);
      this._usageHistory = [];
    } finally {
      this._loading = false;
    }
  }

  _handlePageChange(direction) {
    const totalPages = Math.ceil(this._totalCount / this._pageSize);

    if (direction === 'prev' && this._page > 1) {
      this._page--;
      this._loadUsageHistory();
    } else if (direction === 'next' && this._page < totalPages) {
      this._page++;
      this._loadUsageHistory();
    }
  }

  _handleOrderClick(orderId) {
    window.history.pushState(null, '', `/umbraco/section/ecommerce/workspace/order/edit/${orderId}`);

    const event = new CustomEvent('umb-navigate', {
      bubbles: true,
      composed: true,
      detail: {
        path: `/section/ecommerce/workspace/order/edit/${orderId}`
      }
    });
    this.dispatchEvent(event);
  }

  _formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount || 0);
  }

  _calculateTotalSavings() {
    return this._usageHistory.reduce((total, usage) => total + (usage.discountAmount || 0), 0);
  }

  render() {
    const isNewDiscount = !this._discount?.id;

    if (isNewDiscount) {
      return html`
        <div class="empty-state">
          <uui-icon name="icon-chart-line"></uui-icon>
          <h3>No usage data yet</h3>
          <p>Save the discount first to track usage history.</p>
        </div>
      `;
    }

    return html`
      <div class="stats-row">
        <div class="stat-card">
          <div class="stat-value">${this._discount?.usageCount || 0}</div>
          <div class="stat-label">Total Uses</div>
        </div>
        <div class="stat-card">
          <div class="stat-value positive">${this._formatCurrency(this._calculateTotalSavings())}</div>
          <div class="stat-label">Total Savings Given</div>
        </div>
        <div class="stat-card">
          <div class="stat-value">
            ${this._discount?.totalUsageLimit
              ? `${this._discount.totalUsageLimit - this._discount.usageCount}`
              : 'Unlimited'}
          </div>
          <div class="stat-label">Remaining Uses</div>
        </div>
      </div>

      <uui-box>
        <h3 class="section-title">Usage History</h3>

        ${this._loading
          ? html`<div class="loading"><uui-loader></uui-loader></div>`
          : this._usageHistory.length === 0
            ? this._renderEmptyState()
            : this._renderUsageTable()
        }
      </uui-box>
    `;
  }

  _renderEmptyState() {
    return html`
      <div class="empty-state">
        <uui-icon name="icon-receipt-dollar"></uui-icon>
        <h3>No usage recorded</h3>
        <p>This discount hasn't been used yet.</p>
      </div>
    `;
  }

  _renderUsageTable() {
    const totalPages = Math.ceil(this._totalCount / this._pageSize);
    const startItem = ((this._page - 1) * this._pageSize) + 1;
    const endItem = Math.min(this._page * this._pageSize, this._totalCount);

    return html`
      <table class="usage-table">
        <thead>
          <tr>
            <th>Order</th>
            <th>Customer</th>
            <th>Date</th>
            <th>Discount Amount</th>
          </tr>
        </thead>
        <tbody>
          ${this._usageHistory.map(usage => html`
            <tr>
              <td>
                <span
                  class="order-link"
                  @click=${() => this._handleOrderClick(usage.orderId)}
                >
                  #${usage.orderNumber || usage.orderId}
                </span>
              </td>
              <td>${usage.customerName || usage.customerEmail || 'Guest'}</td>
              <td>${new Date(usage.createdAt).toLocaleDateString()}</td>
              <td class="amount-cell">${this._formatCurrency(usage.discountAmount)}</td>
            </tr>
          `)}
        </tbody>
      </table>

      ${this._totalCount > this._pageSize ? html`
        <div class="pagination">
          <div class="pagination-info">
            Showing ${startItem}-${endItem} of ${this._totalCount} records
          </div>
          <div class="pagination-buttons">
            <uui-button
              look="secondary"
              compact
              ?disabled=${this._page === 1}
              @click=${() => this._handlePageChange('prev')}
            >
              Previous
            </uui-button>
            <uui-button
              look="secondary"
              compact
              ?disabled=${this._page >= totalPages}
              @click=${() => this._handlePageChange('next')}
            >
              Next
            </uui-button>
          </div>
        </div>
      ` : ''}
    `;
  }
}

customElements.define('ecommerce-discount-usage', DiscountUsage);

export default DiscountUsage;
