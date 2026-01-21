import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Discount Collection View
 * Displays a list of discounts with filtering by status.
 */
export class DiscountCollection extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .collection-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--uui-size-layout-1);
      gap: var(--uui-size-layout-1);
      flex-wrap: wrap;
    }

    .filter-group {
      display: flex;
      gap: var(--uui-size-space-4);
      align-items: center;
    }

    .status-filters {
      display: flex;
      gap: var(--uui-size-space-2);
      flex-wrap: wrap;
    }

    .status-filter {
      padding: var(--uui-size-space-2) var(--uui-size-space-4);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      cursor: pointer;
      font-size: var(--uui-type-small-size);
      background: var(--uui-color-surface);
    }

    .status-filter:hover {
      background: var(--uui-color-surface-emphasis);
    }

    .status-filter.active {
      background: var(--uui-color-selected);
      border-color: var(--uui-color-selected);
    }

    .collection-table {
      width: 100%;
      border-collapse: collapse;
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
    }

    .collection-table th,
    .collection-table td {
      padding: var(--uui-size-space-4);
      text-align: left;
      border-bottom: 1px solid var(--uui-color-border);
    }

    .collection-table th {
      background: var(--uui-color-surface-alt);
      font-weight: bold;
    }

    .collection-table tr:hover {
      background: var(--uui-color-surface-emphasis);
      cursor: pointer;
    }

    .collection-table tr:last-child td {
      border-bottom: none;
    }

    .discount-name {
      font-weight: 500;
    }

    .discount-code {
      font-family: monospace;
      background: var(--uui-color-surface-alt);
      padding: var(--uui-size-space-1) var(--uui-size-space-2);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
    }

    .discount-value {
      font-weight: bold;
      color: var(--uui-color-positive);
    }

    .status-badge {
      display: inline-block;
      padding: var(--uui-size-space-1) var(--uui-size-space-3);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
      font-weight: 500;
    }

    .status-active {
      background: var(--uui-color-positive-emphasis);
      color: var(--uui-color-positive);
    }

    .status-scheduled {
      background: var(--uui-color-warning-emphasis);
      color: var(--uui-color-warning);
    }

    .status-expired {
      background: var(--uui-color-danger-emphasis);
      color: var(--uui-color-danger);
    }

    .status-inactive {
      background: var(--uui-color-surface-alt);
      color: var(--uui-color-text-alt);
    }

    .usage-cell {
      white-space: nowrap;
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

    .date-cell {
      white-space: nowrap;
      color: var(--uui-color-text-alt);
      font-size: var(--uui-type-small-size);
    }
  `;

  static properties = {
    _discounts: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _statusFilter: { type: String, state: true }
  };

  constructor() {
    super();
    this._discounts = [];
    this._loading = true;
    this._statusFilter = null;
  }

  connectedCallback() {
    super.connectedCallback();
    this._parseUrlParams();
    this._loadDiscounts();
  }

  _parseUrlParams() {
    const urlParams = new URLSearchParams(window.location.search);
    const filter = urlParams.get('filter');
    if (filter) {
      this._statusFilter = filter;
    }
  }

  async _loadDiscounts() {
    try {
      this._loading = true;

      const url = this._statusFilter && this._statusFilter !== 'all'
        ? `/umbraco/management/api/v1/ecommerce/discount/tree/${this._statusFilter}/children`
        : '/umbraco/management/api/v1/ecommerce/discount?includeInactive=true';

      const response = await fetch(url, {
        headers: {
          'Accept': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error('Failed to load discounts');
      }

      const data = await response.json();
      this._discounts = data.items || [];
    } catch (error) {
      console.error('Error loading discounts:', error);
      this._discounts = [];
    } finally {
      this._loading = false;
    }
  }

  _handleStatusFilter(status) {
    this._statusFilter = status === this._statusFilter ? null : status;
    this._loadDiscounts();
  }

  _handleRowClick(discount) {
    window.history.pushState(null, '', `/umbraco/section/ecommerce/workspace/discount/edit/${discount.id}`);

    const event = new CustomEvent('umb-navigate', {
      bubbles: true,
      composed: true,
      detail: {
        path: `/section/ecommerce/workspace/discount/edit/${discount.id}`
      }
    });
    this.dispatchEvent(event);
  }

  _handleCreateDiscount() {
    window.history.pushState(null, '', '/umbraco/section/ecommerce/workspace/discount/create');

    const event = new CustomEvent('umb-navigate', {
      bubbles: true,
      composed: true,
      detail: {
        path: '/section/ecommerce/workspace/discount/create'
      }
    });
    this.dispatchEvent(event);
  }

  _getStatusClass(status) {
    switch (status?.toLowerCase()) {
      case 'active': return 'status-active';
      case 'scheduled': return 'status-scheduled';
      case 'expired': return 'status-expired';
      case 'inactive': return 'status-inactive';
      default: return 'status-inactive';
    }
  }

  _formatUsage(discount) {
    if (discount.totalUsageLimit) {
      return `${discount.usageCount} / ${discount.totalUsageLimit}`;
    }
    return `${discount.usageCount} uses`;
  }

  _formatDateRange(discount) {
    const parts = [];
    if (discount.startDate) {
      parts.push(`From ${new Date(discount.startDate).toLocaleDateString()}`);
    }
    if (discount.endDate) {
      parts.push(`Until ${new Date(discount.endDate).toLocaleDateString()}`);
    }
    return parts.length > 0 ? parts.join(' - ') : 'No date limit';
  }

  render() {
    const statuses = ['Active', 'Scheduled', 'Expired'];

    return html`
      <div class="collection-header">
        <div class="filter-group">
          <div class="status-filters">
            ${statuses.map(status => html`
              <button
                class="status-filter ${this._statusFilter === status.toLowerCase() ? 'active' : ''}"
                @click=${() => this._handleStatusFilter(status.toLowerCase())}
              >
                ${status}
              </button>
            `)}
          </div>
        </div>
        <uui-button
          look="primary"
          @click=${this._handleCreateDiscount}
        >
          <uui-icon name="icon-add"></uui-icon>
          Create Discount
        </uui-button>
      </div>

      ${this._loading
        ? html`<div class="loading"><uui-loader></uui-loader></div>`
        : this._discounts.length === 0
          ? this._renderEmptyState()
          : this._renderDiscountsTable()
      }
    `;
  }

  _renderEmptyState() {
    return html`
      <div class="empty-state">
        <uui-icon name="icon-tag"></uui-icon>
        <h3>No discounts found</h3>
        <p>${this._statusFilter ? `No ${this._statusFilter} discounts` : 'Create your first discount to get started'}</p>
      </div>
    `;
  }

  _renderDiscountsTable() {
    return html`
      <table class="collection-table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Code</th>
            <th>Value</th>
            <th>Status</th>
            <th>Usage</th>
            <th>Validity</th>
          </tr>
        </thead>
        <tbody>
          ${this._discounts.map(discount => html`
            <tr @click=${() => this._handleRowClick(discount)}>
              <td>
                <span class="discount-name">${discount.name}</span>
              </td>
              <td>
                ${discount.code
                  ? html`<span class="discount-code">${discount.code}</span>`
                  : html`<span style="color: var(--uui-color-text-alt);">Auto</span>`}
              </td>
              <td>
                <span class="discount-value">${discount.displayValue}</span>
              </td>
              <td>
                <span class="status-badge ${this._getStatusClass(discount.status)}">
                  ${discount.status}
                </span>
              </td>
              <td class="usage-cell">
                ${this._formatUsage(discount)}
              </td>
              <td class="date-cell">
                ${this._formatDateRange(discount)}
              </td>
            </tr>
          `)}
        </tbody>
      </table>
    `;
  }
}

customElements.define('ecommerce-discount-collection', DiscountCollection);

export default DiscountCollection;
