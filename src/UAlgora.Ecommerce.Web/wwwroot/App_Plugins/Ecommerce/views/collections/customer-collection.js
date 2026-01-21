import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Customer Collection View
 * Displays a list of customers with search and filtering.
 */
export class CustomerCollection extends UmbElementMixin(LitElement) {
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

    .search-group {
      display: flex;
      gap: var(--uui-size-space-3);
      align-items: center;
      flex: 1;
      max-width: 400px;
    }

    .search-group uui-input {
      flex: 1;
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
      color: var(--uui-color-text);
    }

    .collection-table tr:hover {
      background: var(--uui-color-surface-emphasis);
      cursor: pointer;
    }

    .collection-table tr:last-child td {
      border-bottom: none;
    }

    .customer-cell {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-3);
    }

    .customer-avatar {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      background: var(--uui-color-surface-alt);
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: bold;
      color: var(--uui-color-text-alt);
      flex-shrink: 0;
    }

    .customer-info {
      min-width: 0;
    }

    .customer-name {
      font-weight: 500;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .customer-email {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .stats-cell {
      text-align: right;
    }

    .stats-value {
      font-weight: 500;
    }

    .stats-label {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
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

    .date-cell {
      white-space: nowrap;
      color: var(--uui-color-text-alt);
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

    .status-inactive {
      background: var(--uui-color-danger-emphasis);
      color: var(--uui-color-danger);
    }

    .status-suspended {
      background: var(--uui-color-warning-emphasis);
      color: var(--uui-color-warning);
    }
  `;

  static properties = {
    _customers: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _searchTerm: { type: String, state: true },
    _page: { type: Number, state: true },
    _pageSize: { type: Number, state: true },
    _totalCount: { type: Number, state: true }
  };

  constructor() {
    super();
    this._customers = [];
    this._loading = true;
    this._searchTerm = '';
    this._page = 1;
    this._pageSize = 25;
    this._totalCount = 0;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadCustomers();
  }

  async _loadCustomers() {
    try {
      this._loading = true;

      const skip = (this._page - 1) * this._pageSize;
      const params = new URLSearchParams({
        skip: skip.toString(),
        take: this._pageSize.toString()
      });

      if (this._searchTerm) {
        params.append('search', this._searchTerm);
      }

      const response = await fetch(`/umbraco/management/api/v1/ecommerce/customer?${params}`, {
        headers: {
          'Accept': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error('Failed to load customers');
      }

      const data = await response.json();
      this._customers = data.items || [];
      this._totalCount = data.total || this._customers.length;
    } catch (error) {
      console.error('Error loading customers:', error);
      this._customers = [];
    } finally {
      this._loading = false;
    }
  }

  _handleSearch(e) {
    this._searchTerm = e.target.value;
    this._page = 1;

    // Debounce search
    clearTimeout(this._searchTimeout);
    this._searchTimeout = setTimeout(() => {
      this._loadCustomers();
    }, 300);
  }

  _handleRowClick(customer) {
    window.history.pushState(null, '', `/umbraco/section/ecommerce/workspace/customer/edit/${customer.id}`);

    const event = new CustomEvent('umb-navigate', {
      bubbles: true,
      composed: true,
      detail: {
        path: `/section/ecommerce/workspace/customer/edit/${customer.id}`
      }
    });
    this.dispatchEvent(event);
  }

  _handlePageChange(direction) {
    const totalPages = Math.ceil(this._totalCount / this._pageSize);

    if (direction === 'prev' && this._page > 1) {
      this._page--;
      this._loadCustomers();
    } else if (direction === 'next' && this._page < totalPages) {
      this._page++;
      this._loadCustomers();
    }
  }

  _handleCreateCustomer() {
    window.history.pushState(null, '', '/umbraco/section/ecommerce/workspace/customer/create');

    const event = new CustomEvent('umb-navigate', {
      bubbles: true,
      composed: true,
      detail: {
        path: '/section/ecommerce/workspace/customer/create'
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

  _getInitials(customer) {
    const first = customer.firstName?.charAt(0) || '';
    const last = customer.lastName?.charAt(0) || '';
    return (first + last).toUpperCase() || customer.email?.charAt(0)?.toUpperCase() || '?';
  }

  _getStatusClass(status) {
    const s = status?.toLowerCase() || 'active';
    if (s === 'inactive') return 'status-inactive';
    if (s === 'suspended') return 'status-suspended';
    return 'status-active';
  }

  render() {
    return html`
      <div class="collection-header">
        <div class="search-group">
          <uui-input
            placeholder="Search customers..."
            .value=${this._searchTerm}
            @input=${this._handleSearch}
          >
            <uui-icon slot="prepend" name="icon-search"></uui-icon>
          </uui-input>
        </div>
        <uui-button
          look="primary"
          @click=${this._handleCreateCustomer}
        >
          <uui-icon name="icon-add"></uui-icon>
          Add Customer
        </uui-button>
      </div>

      ${this._loading
        ? html`<div class="loading"><uui-loader></uui-loader></div>`
        : this._customers.length === 0
          ? this._renderEmptyState()
          : this._renderCustomersTable()
      }
    `;
  }

  _renderEmptyState() {
    return html`
      <div class="empty-state">
        <uui-icon name="icon-users"></uui-icon>
        <h3>No customers found</h3>
        <p>${this._searchTerm ? 'Try a different search term' : 'No customers yet'}</p>
      </div>
    `;
  }

  _renderCustomersTable() {
    const totalPages = Math.ceil(this._totalCount / this._pageSize);
    const startItem = ((this._page - 1) * this._pageSize) + 1;
    const endItem = Math.min(this._page * this._pageSize, this._totalCount);

    return html`
      <table class="collection-table">
        <thead>
          <tr>
            <th>Customer</th>
            <th>Phone</th>
            <th>Orders</th>
            <th class="stats-cell">Total Spent</th>
            <th>Last Order</th>
          </tr>
        </thead>
        <tbody>
          ${this._customers.map(customer => html`
            <tr @click=${() => this._handleRowClick(customer)}>
              <td>
                <div class="customer-cell">
                  <div class="customer-avatar">
                    ${this._getInitials(customer)}
                  </div>
                  <div class="customer-info">
                    <div class="customer-name">${customer.fullName || 'Unknown'}</div>
                    <div class="customer-email">${customer.email}</div>
                  </div>
                </div>
              </td>
              <td>${customer.phone || '-'}</td>
              <td>${customer.totalOrders || 0}</td>
              <td class="stats-cell">
                <span class="stats-value">${this._formatCurrency(customer.totalSpent)}</span>
              </td>
              <td class="date-cell">
                ${customer.lastOrderAt
                  ? new Date(customer.lastOrderAt).toLocaleDateString()
                  : 'Never'}
              </td>
            </tr>
          `)}
        </tbody>
      </table>

      <div class="pagination">
        <div class="pagination-info">
          Showing ${startItem}-${endItem} of ${this._totalCount} customers
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
    `;
  }
}

customElements.define('ecommerce-customer-collection', CustomerCollection);

export default CustomerCollection;
