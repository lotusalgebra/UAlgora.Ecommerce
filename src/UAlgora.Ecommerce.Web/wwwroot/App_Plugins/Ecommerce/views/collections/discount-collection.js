import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";

export class DiscountCollection extends UmbElementMixin(LitElement) {
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

    .filter-group {
      display: flex;
      gap: 8px;
      align-items: center;
    }

    .status-filters {
      display: flex;
      gap: 4px;
      flex-wrap: wrap;
    }

    .status-filter {
      padding: 6px 12px;
      border: 1px solid #ddd;
      border-radius: 4px;
      cursor: pointer;
      font-size: 13px;
      background: #fff;
    }

    .status-filter:hover { background: #f5f5f5; }
    .status-filter.active { background: #1b264f; color: white; border-color: #1b264f; }

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

    .discount-name { font-weight: 500; }

    .discount-code {
      font-family: monospace;
      background: #f0f0f0;
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 12px;
    }

    .discount-value {
      font-weight: bold;
      color: #27ae60;
    }

    .status-badge {
      display: inline-block;
      padding: 4px 10px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: 500;
    }

    .status-active { background: #d4edda; color: #155724; }
    .status-scheduled { background: #fff3cd; color: #856404; }
    .status-expired { background: #f8d7da; color: #721c24; }
    .status-inactive { background: #e9ecef; color: #6c757d; }

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

    .checkbox-group {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .checkbox-group input { width: auto; }

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
    _discounts: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _statusFilter: { type: String, state: true },
    _showModal: { type: Boolean, state: true },
    _editingDiscount: { type: Object, state: true },
    _saving: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._discounts = [];
    this._loading = true;
    this._statusFilter = null;
    this._showModal = false;
    this._editingDiscount = null;
    this._saving = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._parseUrlParams();
    this._loadDiscounts();
  }

  _parseUrlParams() {
    const urlParams = new URLSearchParams(window.location.search);
    const filter = urlParams.get('filter');
    if (filter) this._statusFilter = filter;
  }

  async _loadDiscounts() {
    try {
      this._loading = true;

      const url = this._statusFilter && this._statusFilter !== 'all'
        ? `/umbraco/management/api/v1/ecommerce/discount/tree/${this._statusFilter}/children`
        : '/umbraco/management/api/v1/ecommerce/discount?includeInactive=true';

      const response = await this._authFetch(url);

      if (!response.ok) throw new Error('Failed to load discounts');

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

  _openCreateModal() {
    this._editingDiscount = {
      name: '', code: '', description: '',
      discountType: 'Percentage',
      discountValue: 0,
      minimumOrderAmount: null,
      maximumDiscountAmount: null,
      totalUsageLimit: null,
      perCustomerUsageLimit: null,
      startDate: '',
      endDate: '',
      isActive: true,
      isCombinableWithOtherDiscounts: true
    };
    this._showModal = true;
  }

  _openEditModal(discount) {
    this._editingDiscount = {
      ...discount,
      startDate: discount.startDate ? discount.startDate.split('T')[0] : '',
      endDate: discount.endDate ? discount.endDate.split('T')[0] : ''
    };
    this._showModal = true;
  }

  _closeModal() {
    this._showModal = false;
    this._editingDiscount = null;
  }

  _handleInputChange(field, value) {
    this._editingDiscount = { ...this._editingDiscount, [field]: value };
  }

  async _saveDiscount() {
    if (!this._editingDiscount.name) {
      alert('Name is required');
      return;
    }

    this._saving = true;
    try {
      const isNew = !this._editingDiscount.id;
      const url = isNew
        ? '/umbraco/management/api/v1/ecommerce/discount'
        : `/umbraco/management/api/v1/ecommerce/discount/${this._editingDiscount.id}`;

      // Prepare data
      const data = { ...this._editingDiscount };
      if (data.startDate) data.startDate = new Date(data.startDate).toISOString();
      if (data.endDate) data.endDate = new Date(data.endDate).toISOString();

      const response = await this._authFetch(url, {
        method: isNew ? 'POST' : 'PUT',
        body: JSON.stringify(data)
      });

      if (!response.ok) {
        const error = await response.text();
        throw new Error(error || 'Failed to save discount');
      }

      this._closeModal();
      this._loadDiscounts();
    } catch (error) {
      console.error('Error saving discount:', error);
      alert('Failed to save discount: ' + error.message);
    } finally {
      this._saving = false;
    }
  }

  async _deleteDiscount(discount) {
    if (!confirm(`Are you sure you want to delete "${discount.name}"?`)) return;

    try {
      const response = await this._authFetch(`/umbraco/management/api/v1/ecommerce/discount/${discount.id}`, {
        method: 'DELETE'
      });

      if (!response.ok) throw new Error('Failed to delete discount');
      this._loadDiscounts();
    } catch (error) {
      console.error('Error deleting discount:', error);
      alert('Failed to delete discount');
    }
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
      return `${discount.usageCount || 0} / ${discount.totalUsageLimit}`;
    }
    return `${discount.usageCount || 0} uses`;
  }

  _formatDateRange(discount) {
    const parts = [];
    if (discount.startDate) parts.push(`From ${new Date(discount.startDate).toLocaleDateString()}`);
    if (discount.endDate) parts.push(`Until ${new Date(discount.endDate).toLocaleDateString()}`);
    return parts.length > 0 ? parts.join(' - ') : 'No date limit';
  }

  _renderTypeSpecificFields() {
    const type = this._editingDiscount?.type || this._editingDiscount?.discountType || 'Percentage';

    switch (type) {
      case 'BuyXGetY':
        return html`
          <div class="section-title">Buy X Get Y Settings</div>
          <div class="form-row">
            <div class="form-group">
              <label>Buy Quantity</label>
              <input type="number" min="1" .value=${this._editingDiscount?.buyQuantity || 1} @input=${(e) => this._handleInputChange('buyQuantity', parseInt(e.target.value) || 1)} />
            </div>
            <div class="form-group">
              <label>Get Quantity (Free)</label>
              <input type="number" min="1" .value=${this._editingDiscount?.getQuantity || 1} @input=${(e) => this._handleInputChange('getQuantity', parseInt(e.target.value) || 1)} />
            </div>
          </div>
        `;

      case 'EarlyPayment':
        return html`
          <div class="section-title">Early Payment Settings</div>
          <div class="form-row">
            <div class="form-group">
              <label>Early Payment Days *</label>
              <input type="number" min="1" placeholder="e.g. 10" .value=${this._editingDiscount?.earlyPaymentDays || ''} @input=${(e) => this._handleInputChange('earlyPaymentDays', e.target.value ? parseInt(e.target.value) : null)} />
              <small style="color:#666;">Days within which payment qualifies for discount</small>
            </div>
            <div class="form-group">
              <label>Standard Payment Days</label>
              <input type="number" min="1" placeholder="e.g. 30" .value=${this._editingDiscount?.standardPaymentDays || ''} @input=${(e) => this._handleInputChange('standardPaymentDays', e.target.value ? parseInt(e.target.value) : null)} />
              <small style="color:#666;">Normal payment term (e.g. Net 30)</small>
            </div>
          </div>
        `;

      case 'Overstock':
        return html`
          <div class="section-title">Overstock / Clearance Settings</div>
          <div class="form-group checkbox-group">
            <input type="checkbox" id="isOverstockClearance" .checked=${this._editingDiscount?.isOverstockClearance !== false} @change=${(e) => this._handleInputChange('isOverstockClearance', e.target.checked)} />
            <label for="isOverstockClearance">Mark as clearance sale</label>
          </div>
          <small style="color:#666;">Apply to specific products or categories using the Conditions section below. Great for moving excess inventory.</small>
        `;

      case 'Bundle':
        return html`
          <div class="section-title">Bundle Settings</div>
          <div class="form-group">
            <label>Bundle Discount Percentage</label>
            <input type="number" step="0.01" min="0" max="100" .value=${this._editingDiscount?.bundleDiscountValue || ''} @input=${(e) => this._handleInputChange('bundleDiscountValue', e.target.value ? parseFloat(e.target.value) : null)} />
            <small style="color:#666;">Percentage off when all bundle products are in the cart. Specify bundle products in the Conditions section.</small>
          </div>
        `;

      case 'BulkVolume':
        return html`
          <div class="section-title">Volume Tier Settings</div>
          <small style="color:#666;">Define quantity-based discount tiers. Higher quantities get bigger discounts.</small>
          ${(this._editingDiscount?.volumeTiers || []).map((tier, i) => html`
            <div class="form-row" style="margin-top:8px;">
              <div class="form-group">
                <label>Min Quantity</label>
                <input type="number" min="1" .value=${tier.minQuantity || ''} @input=${(e) => this._updateVolumeTier(i, 'minQuantity', parseInt(e.target.value) || 0)} />
              </div>
              <div class="form-group">
                <label>Discount %</label>
                <div style="display:flex;gap:8px;align-items:center;">
                  <input type="number" step="0.01" min="0" max="100" .value=${tier.discountPercent || ''} @input=${(e) => this._updateVolumeTier(i, 'discountPercent', parseFloat(e.target.value) || 0)} />
                  <uui-button look="secondary" compact label="Remove" @click=${() => this._removeVolumeTier(i)}>&times;</uui-button>
                </div>
              </div>
            </div>
          `)}
          <uui-button look="secondary" style="margin-top:8px;" label="Add Tier" @click=${this._addVolumeTier}>+ Add Tier</uui-button>
        `;

      case 'Seasonal':
        return html`
          <div class="section-title">Seasonal Settings</div>
          <div class="form-group">
            <label>Season Label</label>
            <input type="text" placeholder="e.g. Summer Sale, Post-Holiday Clearance" .value=${this._editingDiscount?.seasonLabel || ''} @input=${(e) => this._handleInputChange('seasonLabel', e.target.value)} />
          </div>
        `;

      case 'Referral':
        return html`
          <div class="section-title">Referral Settings</div>
          <div class="form-group checkbox-group">
            <input type="checkbox" id="referralTwoWay" .checked=${this._editingDiscount?.referralTwoWay === true} @change=${(e) => this._handleInputChange('referralTwoWay', e.target.checked)} />
            <label for="referralTwoWay">Two-way referral (both referrer and new customer get discount)</label>
          </div>
          ${this._editingDiscount?.referralTwoWay ? html`
            <div class="form-group">
              <label>New Customer Discount Value</label>
              <input type="number" step="0.01" .value=${this._editingDiscount?.referralNewCustomerValue || ''} @input=${(e) => this._handleInputChange('referralNewCustomerValue', e.target.value ? parseFloat(e.target.value) : null)} />
              <small style="color:#666;">The "Value" field above applies to the referrer.</small>
            </div>
          ` : ''}
        `;

      case 'LoyaltyProgram':
        return html`
          <div class="section-title">Loyalty Program Settings</div>
          <div class="form-row">
            <div class="form-group">
              <label>Points Threshold</label>
              <input type="number" min="0" .value=${this._editingDiscount?.loyaltyPointsThreshold || ''} @input=${(e) => this._handleInputChange('loyaltyPointsThreshold', e.target.value ? parseInt(e.target.value) : null)} />
              <small style="color:#666;">Points needed to unlock this discount</small>
            </div>
            <div class="form-group">
              <label>Required Tier</label>
              <input type="text" placeholder="e.g. Gold, Platinum" .value=${this._editingDiscount?.loyaltyTierRequired || ''} @input=${(e) => this._handleInputChange('loyaltyTierRequired', e.target.value)} />
            </div>
          </div>
        `;

      case 'EmailSubscription':
        return html`
          <div class="section-title">Email Subscription Settings</div>
          <div class="form-group checkbox-group">
            <input type="checkbox" id="requiresEmailSubscription" .checked=${this._editingDiscount?.requiresEmailSubscription !== false} @change=${(e) => this._handleInputChange('requiresEmailSubscription', e.target.checked)} />
            <label for="requiresEmailSubscription">Requires new email subscription sign-up</label>
          </div>
          <div class="form-group checkbox-group">
            <input type="checkbox" id="isCartAbandonmentRecovery" .checked=${this._editingDiscount?.isCartAbandonmentRecovery === true} @change=${(e) => this._handleInputChange('isCartAbandonmentRecovery', e.target.checked)} />
            <label for="isCartAbandonmentRecovery">Cart abandonment recovery discount</label>
          </div>
        `;

      case 'TradeInCredit':
        return html`
          <div class="section-title">Trade-In Credit Settings</div>
          <div class="form-group">
            <label>Credit Per Item Traded In</label>
            <input type="number" step="0.01" min="0" .value=${this._editingDiscount?.tradeInCreditPerItem || ''} @input=${(e) => this._handleInputChange('tradeInCreditPerItem', e.target.value ? parseFloat(e.target.value) : null)} />
            <small style="color:#666;">Credit amount given per traded-in item. Use the Conditions section to specify eligible trade-in and target products.</small>
          </div>
        `;

      default:
        return '';
    }
  }

  _addVolumeTier() {
    const tiers = [...(this._editingDiscount?.volumeTiers || []), { minQuantity: 0, discountPercent: 0 }];
    this._editingDiscount = { ...this._editingDiscount, volumeTiers: tiers };
  }

  _removeVolumeTier(index) {
    const tiers = [...(this._editingDiscount?.volumeTiers || [])];
    tiers.splice(index, 1);
    this._editingDiscount = { ...this._editingDiscount, volumeTiers: tiers };
  }

  _updateVolumeTier(index, field, value) {
    const tiers = [...(this._editingDiscount?.volumeTiers || [])];
    tiers[index] = { ...tiers[index], [field]: value };
    this._editingDiscount = { ...this._editingDiscount, volumeTiers: tiers };
  }

  render() {
    const statuses = ['Active', 'Scheduled', 'Expired'];

    return html`
      <div class="collection-header">
        <div class="filter-group">
          <div class="status-filters">
            ${statuses.map(status => html`
              <button class="status-filter ${this._statusFilter === status.toLowerCase() ? 'active' : ''}" @click=${() => this._handleStatusFilter(status.toLowerCase())}>
                ${status}
              </button>
            `)}
          </div>
        </div>
        <uui-button look="primary" label="Create Discount" @click=${this._openCreateModal}>
          <uui-icon name="icon-add"></uui-icon> Create Discount
        </uui-button>
      </div>

      ${this._loading
        ? html`<div class="loading"><uui-loader></uui-loader></div>`
        : this._discounts.length === 0
          ? this._renderEmptyState()
          : this._renderDiscountsTable()
      }

      ${this._showModal ? this._renderModal() : ''}
    `;
  }

  _renderEmptyState() {
    return html`
      <div class="empty-state">
        <uui-icon name="icon-tag" style="font-size: 48px;"></uui-icon>
        <h3>No discounts found</h3>
        <p>${this._statusFilter ? `No ${this._statusFilter} discounts` : 'Create your first discount to get started'}</p>
        ${!this._statusFilter ? html`
          <uui-button look="primary" label="Create Discount" @click=${this._openCreateModal}>Create Your First Discount</uui-button>
        ` : ''}
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
            <th style="width: 80px;">Actions</th>
          </tr>
        </thead>
        <tbody>
          ${this._discounts.map(discount => html`
            <tr>
              <td><span class="discount-name">${discount.name}</span></td>
              <td>
                ${discount.code
                  ? html`<span class="discount-code">${discount.code}</span>`
                  : html`<span style="color: #999;">Auto</span>`}
              </td>
              <td><span class="discount-value">${discount.displayValue || `${discount.discountValue}${discount.discountType === 'Percentage' ? '%' : ''}`}</span></td>
              <td><span class="status-badge ${this._getStatusClass(discount.status)}">${discount.status || 'Inactive'}</span></td>
              <td>${this._formatUsage(discount)}</td>
              <td style="font-size: 12px; color: #666;">${this._formatDateRange(discount)}</td>
              <td>
                <uui-button look="secondary" compact label="Edit" @click=${() => this._openEditModal(discount)}>Edit</uui-button>
              </td>
            </tr>
          `)}
        </tbody>
      </table>
    `;
  }

  _renderModal() {
    const isNew = !this._editingDiscount?.id;

    return html`
      <div class="modal-overlay" @click=${(e) => e.target === e.currentTarget && this._closeModal()}>
        <div class="modal">
          <div class="modal-header">
            <h2>${isNew ? 'Create New Discount' : 'Edit Discount'}</h2>
            <uui-button look="secondary" compact label="Close" @click=${this._closeModal}>&times;</uui-button>
          </div>
          <div class="modal-body">
            <div class="section-title">Basic Information</div>
            <div class="form-group">
              <label>Discount Name *</label>
              <input type="text" .value=${this._editingDiscount?.name || ''} @input=${(e) => this._handleInputChange('name', e.target.value)} />
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Coupon Code</label>
                <input type="text" placeholder="Leave empty for auto discount" .value=${this._editingDiscount?.code || ''} @input=${(e) => this._handleInputChange('code', e.target.value)} />
              </div>
              <div class="form-group">
                <label>Discount Type</label>
                <select .value=${this._editingDiscount?.type || this._editingDiscount?.discountType || 'Percentage'} @change=${(e) => this._handleInputChange('type', e.target.value)}>
                  <option value="Percentage">Percentage</option>
                  <option value="FixedAmount">Fixed Amount</option>
                  <option value="FreeShipping">Free Shipping</option>
                  <option value="BuyXGetY">Buy X Get Y (BOGO)</option>
                  <option value="EarlyPayment">Early Payment</option>
                  <option value="Overstock">Overstock / Clearance</option>
                  <option value="Bundle">Price Bundle</option>
                  <option value="BulkVolume">Bulk / Volume</option>
                  <option value="Seasonal">Seasonal</option>
                  <option value="Referral">Referral</option>
                  <option value="LoyaltyProgram">Loyalty Program</option>
                  <option value="EmailSubscription">Email Subscription</option>
                  <option value="TradeInCredit">Trade-In Credit</option>
                </select>
              </div>
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Discount Value</label>
                <input type="number" step="0.01" .value=${this._editingDiscount?.discountValue || 0} @input=${(e) => this._handleInputChange('discountValue', parseFloat(e.target.value) || 0)} />
              </div>
              <div class="form-group">
                <label>Max Discount Amount</label>
                <input type="number" step="0.01" placeholder="No limit" .value=${this._editingDiscount?.maximumDiscountAmount || ''} @input=${(e) => this._handleInputChange('maximumDiscountAmount', e.target.value ? parseFloat(e.target.value) : null)} />
              </div>
            </div>
            <div class="form-group">
              <label>Description</label>
              <textarea .value=${this._editingDiscount?.description || ''} @input=${(e) => this._handleInputChange('description', e.target.value)}></textarea>
            </div>

            ${this._renderTypeSpecificFields()}

            <div class="section-title">Conditions</div>
            <div class="form-row">
              <div class="form-group">
                <label>Minimum Order Amount</label>
                <input type="number" step="0.01" placeholder="No minimum" .value=${this._editingDiscount?.minimumOrderAmount || ''} @input=${(e) => this._handleInputChange('minimumOrderAmount', e.target.value ? parseFloat(e.target.value) : null)} />
              </div>
              <div class="form-group">
                <label>Total Usage Limit</label>
                <input type="number" placeholder="Unlimited" .value=${this._editingDiscount?.totalUsageLimit || ''} @input=${(e) => this._handleInputChange('totalUsageLimit', e.target.value ? parseInt(e.target.value) : null)} />
              </div>
            </div>
            <div class="form-group">
              <label>Per Customer Limit</label>
              <input type="number" placeholder="Unlimited" .value=${this._editingDiscount?.perCustomerUsageLimit || ''} @input=${(e) => this._handleInputChange('perCustomerUsageLimit', e.target.value ? parseInt(e.target.value) : null)} />
            </div>

            <div class="section-title">Validity Period</div>
            <div class="form-row">
              <div class="form-group">
                <label>Start Date</label>
                <input type="date" .value=${this._editingDiscount?.startDate || ''} @input=${(e) => this._handleInputChange('startDate', e.target.value)} />
              </div>
              <div class="form-group">
                <label>End Date</label>
                <input type="date" .value=${this._editingDiscount?.endDate || ''} @input=${(e) => this._handleInputChange('endDate', e.target.value)} />
              </div>
            </div>

            <div class="section-title">Settings</div>
            <div class="form-row">
              <div class="form-group checkbox-group">
                <input type="checkbox" id="isActive" .checked=${this._editingDiscount?.isActive !== false} @change=${(e) => this._handleInputChange('isActive', e.target.checked)} />
                <label for="isActive">Active</label>
              </div>
              <div class="form-group checkbox-group">
                <input type="checkbox" id="isCombinable" .checked=${this._editingDiscount?.isCombinableWithOtherDiscounts !== false} @change=${(e) => this._handleInputChange('isCombinableWithOtherDiscounts', e.target.checked)} />
                <label for="isCombinable">Combinable with other discounts</label>
              </div>
            </div>
          </div>
          <div class="modal-footer">
            <uui-button look="secondary" label="Cancel" @click=${this._closeModal} ?disabled=${this._saving}>Cancel</uui-button>
            <uui-button look="primary" label="Save" @click=${this._saveDiscount} ?disabled=${this._saving}>
              ${this._saving ? 'Saving...' : 'Save Discount'}
            </uui-button>
          </div>
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-discount-collection', DiscountCollection);
export default DiscountCollection;
