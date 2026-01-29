import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";

export class CustomerCollection extends UmbElementMixin(LitElement) {
  #authContext;

  async _getAuthHeaders() {
    if (!this.#authContext) {
      this.#authContext = await this.getContext(UMB_AUTH_CONTEXT);
    }
    const token = await this.#authContext?.getLatestToken();
    return {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    };
  }

  async _authFetch(url, options = {}) {
    const headers = await this._getAuthHeaders();
    return fetch(url, {
      ...options,
      headers: { ...headers, ...options.headers }
    });
  }

  static styles = css`
    :host {
      display: block;
      padding: 20px;
    }

    .collection-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
      gap: 16px;
      flex-wrap: wrap;
    }

    .search-group {
      display: flex;
      gap: 8px;
      align-items: center;
      flex: 1;
      max-width: 400px;
    }

    .search-group uui-input { flex: 1; }

    .collection-table {
      width: 100%;
      border-collapse: collapse;
      background: #fff;
      border: 1px solid #e0e0e0;
      border-radius: 8px;
    }

    .collection-table th, .collection-table td {
      padding: 12px 16px;
      text-align: left;
      border-bottom: 1px solid #e0e0e0;
    }

    .collection-table th {
      background: #f5f5f5;
      font-weight: 600;
    }

    .collection-table tr:hover { background: #f9f9f9; cursor: pointer; }
    .collection-table tr:last-child td { border-bottom: none; }

    .customer-cell {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .customer-avatar {
      width: 40px; height: 40px;
      border-radius: 50%;
      background: #e0e0e0;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: bold;
      color: #666;
      flex-shrink: 0;
    }

    .customer-info { min-width: 0; }
    .customer-name { font-weight: 500; }
    .customer-email { font-size: 12px; color: #666; }

    .stats-cell { text-align: right; }
    .stats-value { font-weight: 500; }

    .empty-state {
      text-align: center;
      padding: 60px 20px;
      color: #666;
    }

    .loading {
      display: flex;
      justify-content: center;
      padding: 40px;
    }

    .pagination {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-top: 16px;
      padding: 12px;
      background: #f5f5f5;
      border-radius: 8px;
    }

    /* Modal Styles */
    .modal-overlay {
      position: fixed;
      top: 0; left: 0; right: 0; bottom: 0;
      background: rgba(0,0,0,0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 10000;
    }

    .modal {
      background: white;
      border-radius: 8px;
      width: 90%;
      max-width: 600px;
      max-height: 90vh;
      overflow-y: auto;
      box-shadow: 0 4px 20px rgba(0,0,0,0.3);
    }

    .modal-header {
      padding: 20px;
      border-bottom: 1px solid #e0e0e0;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .modal-header h2 { margin: 0; font-size: 20px; }

    .modal-body { padding: 20px; }

    .modal-footer {
      padding: 20px;
      border-top: 1px solid #e0e0e0;
      display: flex;
      justify-content: flex-end;
      gap: 10px;
    }

    .form-group {
      margin-bottom: 16px;
    }

    .form-group label {
      display: block;
      margin-bottom: 6px;
      font-weight: 500;
      color: #333;
    }

    .form-group input, .form-group textarea, .form-group select {
      width: 100%;
      padding: 10px;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 14px;
      box-sizing: border-box;
    }

    .form-group textarea { min-height: 80px; resize: vertical; }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
    }

    .section-title {
      font-size: 16px;
      font-weight: 600;
      margin: 24px 0 12px 0;
      padding-bottom: 8px;
      border-bottom: 1px solid #eee;
    }

    .section-title:first-child { margin-top: 0; }
  `;

  static properties = {
    _customers: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _searchTerm: { type: String, state: true },
    _page: { type: Number, state: true },
    _pageSize: { type: Number, state: true },
    _totalCount: { type: Number, state: true },
    _showModal: { type: Boolean, state: true },
    _editingCustomer: { type: Object, state: true },
    _saving: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._customers = [];
    this._loading = true;
    this._searchTerm = '';
    this._page = 1;
    this._pageSize = 25;
    this._totalCount = 0;
    this._showModal = false;
    this._editingCustomer = null;
    this._saving = false;
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

      if (this._searchTerm) params.append('search', this._searchTerm);

      const response = await this._authFetch(`/umbraco/management/api/v1/ecommerce/customer?${params}`);

      if (!response.ok) throw new Error('Failed to load customers');

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
    clearTimeout(this._searchTimeout);
    this._searchTimeout = setTimeout(() => this._loadCustomers(), 300);
  }

  _handlePageChange(direction) {
    const totalPages = Math.ceil(this._totalCount / this._pageSize);
    if (direction === 'prev' && this._page > 1) { this._page--; this._loadCustomers(); }
    else if (direction === 'next' && this._page < totalPages) { this._page++; this._loadCustomers(); }
  }

  _openCreateModal() {
    this._editingCustomer = {
      firstName: '', lastName: '', email: '', phone: '',
      companyName: '', taxId: '',
      defaultShippingAddress: {
        firstName: '', lastName: '',
        address1: '', address2: '',
        city: '', stateProvince: '', postalCode: '', country: ''
      }
    };
    this._showModal = true;
  }

  _openEditModal(customer) {
    this._editingCustomer = {
      ...customer,
      defaultShippingAddress: customer.defaultShippingAddress || {
        firstName: '', lastName: '',
        address1: '', address2: '',
        city: '', stateProvince: '', postalCode: '', country: ''
      }
    };
    this._showModal = true;
  }

  _closeModal() {
    this._showModal = false;
    this._editingCustomer = null;
  }

  _handleInputChange(field, value) {
    this._editingCustomer = { ...this._editingCustomer, [field]: value };
  }

  _handleAddressChange(field, value) {
    this._editingCustomer = {
      ...this._editingCustomer,
      defaultShippingAddress: {
        ...this._editingCustomer.defaultShippingAddress,
        [field]: value
      }
    };
  }

  async _saveCustomer() {
    if (!this._editingCustomer.email) {
      alert('Email is required');
      return;
    }

    this._saving = true;
    try {
      const isNew = !this._editingCustomer.id;
      const url = isNew
        ? '/umbraco/management/api/v1/ecommerce/customer'
        : `/umbraco/management/api/v1/ecommerce/customer/${this._editingCustomer.id}`;

      const response = await this._authFetch(url, {
        method: isNew ? 'POST' : 'PUT',
        body: JSON.stringify(this._editingCustomer)
      });

      if (!response.ok) {
        const error = await response.text();
        throw new Error(error || 'Failed to save customer');
      }

      this._closeModal();
      this._loadCustomers();
    } catch (error) {
      console.error('Error saving customer:', error);
      alert('Failed to save customer: ' + error.message);
    } finally {
      this._saving = false;
    }
  }

  _formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount || 0);
  }

  _getInitials(customer) {
    const first = customer.firstName?.charAt(0) || '';
    const last = customer.lastName?.charAt(0) || '';
    return (first + last).toUpperCase() || customer.email?.charAt(0)?.toUpperCase() || '?';
  }

  render() {
    return html`
      <div class="collection-header">
        <div class="search-group">
          <uui-input label="Search customers" placeholder="Search customers..." .value=${this._searchTerm} @input=${this._handleSearch}>
            <uui-icon slot="prepend" name="icon-search"></uui-icon>
          </uui-input>
        </div>
        <uui-button look="primary" label="Add Customer" @click=${this._openCreateModal}>
          <uui-icon name="icon-add"></uui-icon> Add Customer
        </uui-button>
      </div>

      ${this._loading
        ? html`<div class="loading"><uui-loader></uui-loader></div>`
        : this._customers.length === 0
          ? this._renderEmptyState()
          : this._renderCustomersTable()
      }

      ${this._showModal ? this._renderModal() : ''}
    `;
  }

  _renderEmptyState() {
    return html`
      <div class="empty-state">
        <uui-icon name="icon-users" style="font-size: 48px;"></uui-icon>
        <h3>No customers found</h3>
        <p>${this._searchTerm ? 'Try a different search term' : 'No customers yet'}</p>
        ${!this._searchTerm ? html`
          <uui-button look="primary" label="Add Customer" @click=${this._openCreateModal}>Add Your First Customer</uui-button>
        ` : ''}
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
            <th style="width: 80px;">Actions</th>
          </tr>
        </thead>
        <tbody>
          ${this._customers.map(customer => html`
            <tr>
              <td>
                <div class="customer-cell">
                  <div class="customer-avatar">${this._getInitials(customer)}</div>
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
              <td>${customer.lastOrderAt ? new Date(customer.lastOrderAt).toLocaleDateString() : 'Never'}</td>
              <td>
                <uui-button look="secondary" compact label="Edit" @click=${() => this._openEditModal(customer)}>Edit</uui-button>
              </td>
            </tr>
          `)}
        </tbody>
      </table>

      <div class="pagination">
        <div>Showing ${startItem}-${endItem} of ${this._totalCount} customers</div>
        <div style="display: flex; gap: 8px;">
          <uui-button look="secondary" compact label="Previous" ?disabled=${this._page === 1} @click=${() => this._handlePageChange('prev')}>Previous</uui-button>
          <uui-button look="secondary" compact label="Next" ?disabled=${this._page >= totalPages} @click=${() => this._handlePageChange('next')}>Next</uui-button>
        </div>
      </div>
    `;
  }

  _renderModal() {
    const isNew = !this._editingCustomer?.id;
    const addr = this._editingCustomer?.defaultShippingAddress || {};

    return html`
      <div class="modal-overlay" @click=${(e) => e.target === e.currentTarget && this._closeModal()}>
        <div class="modal">
          <div class="modal-header">
            <h2>${isNew ? 'Add New Customer' : 'Edit Customer'}</h2>
            <uui-button look="secondary" compact label="Close" @click=${this._closeModal}>&times;</uui-button>
          </div>
          <div class="modal-body">
            <div class="section-title">Basic Information</div>
            <div class="form-row">
              <div class="form-group">
                <label>First Name</label>
                <input type="text" .value=${this._editingCustomer?.firstName || ''} @input=${(e) => this._handleInputChange('firstName', e.target.value)} />
              </div>
              <div class="form-group">
                <label>Last Name</label>
                <input type="text" .value=${this._editingCustomer?.lastName || ''} @input=${(e) => this._handleInputChange('lastName', e.target.value)} />
              </div>
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Email *</label>
                <input type="email" .value=${this._editingCustomer?.email || ''} @input=${(e) => this._handleInputChange('email', e.target.value)} />
              </div>
              <div class="form-group">
                <label>Phone</label>
                <input type="tel" .value=${this._editingCustomer?.phone || ''} @input=${(e) => this._handleInputChange('phone', e.target.value)} />
              </div>
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Company</label>
                <input type="text" .value=${this._editingCustomer?.companyName || ''} @input=${(e) => this._handleInputChange('companyName', e.target.value)} />
              </div>
              <div class="form-group">
                <label>Tax ID</label>
                <input type="text" .value=${this._editingCustomer?.taxId || ''} @input=${(e) => this._handleInputChange('taxId', e.target.value)} />
              </div>
            </div>

            <div class="section-title">Default Shipping Address</div>
            <div class="form-row">
              <div class="form-group">
                <label>First Name</label>
                <input type="text" .value=${addr.firstName || ''} @input=${(e) => this._handleAddressChange('firstName', e.target.value)} />
              </div>
              <div class="form-group">
                <label>Last Name</label>
                <input type="text" .value=${addr.lastName || ''} @input=${(e) => this._handleAddressChange('lastName', e.target.value)} />
              </div>
            </div>
            <div class="form-group">
              <label>Address Line 1</label>
              <input type="text" .value=${addr.address1 || ''} @input=${(e) => this._handleAddressChange('address1', e.target.value)} />
            </div>
            <div class="form-group">
              <label>Address Line 2</label>
              <input type="text" .value=${addr.address2 || ''} @input=${(e) => this._handleAddressChange('address2', e.target.value)} />
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>City</label>
                <input type="text" .value=${addr.city || ''} @input=${(e) => this._handleAddressChange('city', e.target.value)} />
              </div>
              <div class="form-group">
                <label>State/Province</label>
                <input type="text" .value=${addr.stateProvince || ''} @input=${(e) => this._handleAddressChange('stateProvince', e.target.value)} />
              </div>
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Postal Code</label>
                <input type="text" .value=${addr.postalCode || ''} @input=${(e) => this._handleAddressChange('postalCode', e.target.value)} />
              </div>
              <div class="form-group">
                <label>Country</label>
                <input type="text" .value=${addr.country || ''} @input=${(e) => this._handleAddressChange('country', e.target.value)} />
              </div>
            </div>
          </div>
          <div class="modal-footer">
            <uui-button look="secondary" label="Cancel" @click=${this._closeModal} ?disabled=${this._saving}>Cancel</uui-button>
            <uui-button look="primary" label="Save" @click=${this._saveCustomer} ?disabled=${this._saving}>
              ${this._saving ? 'Saving...' : 'Save Customer'}
            </uui-button>
          </div>
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-customer-collection', CustomerCollection);
export default CustomerCollection;
