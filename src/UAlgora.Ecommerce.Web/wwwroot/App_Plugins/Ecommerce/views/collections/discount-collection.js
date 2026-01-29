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
    :host { display: block; height: 100%; }
    .container { display: flex; height: 100%; }
    .list-panel { width: 380px; border-right: 1px solid #e0e0e0; display: flex; flex-direction: column; background: #fafafa; }
    .editor-panel { flex: 1; overflow-y: auto; background: #fff; }
    .list-header { padding: 16px; border-bottom: 1px solid #e0e0e0; background: #fff; }
    .list-header h2 { margin: 0 0 12px 0; font-size: 18px; }
    .search-row { display: flex; gap: 8px; margin-bottom: 10px; }
    .search-row input { flex: 1; padding: 8px 12px; border: 1px solid #ddd; border-radius: 4px; font-size: 13px; }
    .status-filters { display: flex; gap: 4px; flex-wrap: wrap; }
    .status-filter { padding: 5px 10px; border: 1px solid #ddd; border-radius: 4px; cursor: pointer; font-size: 12px; background: #fff; }
    .status-filter:hover { background: #f5f5f5; }
    .status-filter.active { background: #1b264f; color: white; border-color: #1b264f; }
    .list-content { flex: 1; overflow-y: auto; }
    .list-item { padding: 12px 16px; border-bottom: 1px solid #e9e9e9; cursor: pointer; display: flex; align-items: center; gap: 12px; }
    .list-item:hover { background: #f0f0f0; }
    .list-item.active { background: #e3f2fd; border-left: 3px solid #1976d2; }
    .list-item-icon { width: 36px; height: 36px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 16px; flex-shrink: 0; }
    .list-item-info { flex: 1; min-width: 0; }
    .list-item-name { font-weight: 600; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; font-size: 13px; }
    .list-item-meta { font-size: 11px; color: #666; display: flex; gap: 6px; flex-wrap: wrap; margin-top: 2px; }
    .badge { padding: 2px 6px; border-radius: 3px; font-size: 10px; font-weight: 500; }
    .badge-active { background: #d4edda; color: #155724; }
    .badge-scheduled { background: #fff3cd; color: #856404; }
    .badge-expired { background: #f8d7da; color: #721c24; }
    .badge-inactive { background: #e9ecef; color: #6c757d; }
    .discount-value-badge { font-weight: 700; color: #27ae60; font-size: 12px; }
    .editor-header { padding: 20px 24px; border-bottom: 1px solid #e0e0e0; display: flex; justify-content: space-between; align-items: center; background: #fff; position: sticky; top: 0; z-index: 10; }
    .editor-header h2 { margin: 0; font-size: 20px; }
    .editor-actions { display: flex; gap: 8px; }
    .tabs { display: flex; gap: 4px; border-bottom: 1px solid #e0e0e0; padding: 0 24px; background: #fafafa; }
    .tab { padding: 12px 20px; cursor: pointer; border-bottom: 2px solid transparent; color: #666; font-weight: 500; font-size: 13px; }
    .tab:hover { color: #333; }
    .tab.active { color: #1976d2; border-bottom-color: #1976d2; }
    .tab-content { display: none; }
    .tab-content.active { display: block; }
    .editor-body { padding: 24px; }
    .form-section { margin-bottom: 24px; }
    .form-section-title { font-size: 14px; font-weight: 600; color: #333; margin-bottom: 16px; padding-bottom: 8px; border-bottom: 1px solid #e0e0e0; }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin-bottom: 16px; }
    .form-group { margin-bottom: 16px; }
    .form-group label { display: block; margin-bottom: 6px; font-weight: 500; font-size: 13px; color: #333; }
    .form-group input, .form-group select, .form-group textarea { width: 100%; padding: 10px 12px; border: 1px solid #ddd; border-radius: 4px; font-size: 14px; box-sizing: border-box; }
    .form-group input:focus, .form-group select:focus, .form-group textarea:focus { outline: none; border-color: #1976d2; box-shadow: 0 0 0 2px rgba(25,118,210,0.1); }
    .form-group textarea { min-height: 80px; resize: vertical; }
    .form-group small { display: block; margin-top: 4px; color: #666; font-size: 12px; }
    .checkbox-row { display: flex; gap: 24px; margin-bottom: 16px; }
    .checkbox-item { display: flex; align-items: center; gap: 8px; }
    .checkbox-item input { width: 18px; height: 18px; }
    .empty-state { display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100%; color: #666; }
    .empty-state uui-icon { font-size: 64px; margin-bottom: 16px; opacity: 0.5; }
    .loading { display: flex; justify-content: center; align-items: center; height: 100%; }
    .stats-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 12px; margin-bottom: 24px; }
    .stat-card { background: #f8f9fa; border-radius: 8px; padding: 16px; text-align: center; }
    .stat-value { font-size: 24px; font-weight: 700; color: #1976d2; }
    .stat-label { font-size: 11px; color: #666; margin-top: 4px; }
    .volume-tier-row { display: flex; gap: 8px; align-items: center; margin-bottom: 8px; }
    .volume-tier-row input { flex: 1; }
    .discount-code { font-family: monospace; background: #f0f0f0; padding: 2px 6px; border-radius: 3px; font-size: 11px; }
  `;

  static properties = {
    _discounts: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _searchTerm: { type: String, state: true },
    _statusFilter: { type: String, state: true },
    _editingDiscount: { type: Object, state: true },
    _activeTab: { type: String, state: true },
    _saving: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._discounts = [];
    this._loading = true;
    this._searchTerm = '';
    this._statusFilter = null;
    this._editingDiscount = null;
    this._activeTab = 'general';
    this._saving = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadDiscounts();
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

  _selectDiscount(discount) {
    this._editingDiscount = {
      ...discount,
      startDate: discount.startDate ? discount.startDate.split('T')[0] : '',
      endDate: discount.endDate ? discount.endDate.split('T')[0] : ''
    };
    this._activeTab = 'general';
  }

  _createNew() {
    this._editingDiscount = {
      name: '', code: '', description: '',
      type: 'Percentage',
      discountValue: 0,
      minimumOrderAmount: null,
      maximumDiscountAmount: null,
      totalUsageLimit: null,
      perCustomerUsageLimit: null,
      startDate: '',
      endDate: '',
      isActive: true,
      isCombinableWithOtherDiscounts: true,
      volumeTiers: []
    };
    this._activeTab = 'general';
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

      const data = { ...this._editingDiscount };
      if (!data.discountType) data.discountType = data.type;
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

      await this._loadDiscounts();
      if (isNew) {
        const saved = this._discounts.find(d => d.name === data.name);
        if (saved) this._selectDiscount(saved);
      }
    } catch (error) {
      console.error('Error saving discount:', error);
      alert('Failed to save discount: ' + error.message);
    } finally {
      this._saving = false;
    }
  }

  async _deleteDiscount() {
    if (!this._editingDiscount?.id) return;
    if (!confirm(`Are you sure you want to delete "${this._editingDiscount.name}"?`)) return;
    try {
      const response = await this._authFetch(`/umbraco/management/api/v1/ecommerce/discount/${this._editingDiscount.id}`, {
        method: 'DELETE'
      });
      if (!response.ok) throw new Error('Failed to delete discount');
      this._editingDiscount = null;
      this._loadDiscounts();
    } catch (error) {
      console.error('Error deleting discount:', error);
      alert('Failed to delete discount');
    }
  }

  _getStatusBadge(discount) {
    const s = (discount.status || 'inactive').toLowerCase();
    return s;
  }

  _getTypeIcon(type) {
    switch (type) {
      case 'Percentage': return { bg: '#e3f2fd', icon: '%' };
      case 'FixedAmount': return { bg: '#e8f5e9', icon: '$' };
      case 'FreeShipping': return { bg: '#fff3e0', icon: '🚚' };
      case 'BuyXGetY': return { bg: '#fce4ec', icon: '🎁' };
      case 'EarlyPayment': return { bg: '#e0f7fa', icon: '⏰' };
      case 'Overstock': return { bg: '#fff8e1', icon: '📦' };
      case 'Bundle': return { bg: '#f3e5f5', icon: '🔗' };
      case 'BulkVolume': return { bg: '#e8eaf6', icon: '📊' };
      case 'Seasonal': return { bg: '#e8f5e9', icon: '🌸' };
      case 'Referral': return { bg: '#fce4ec', icon: '👥' };
      case 'LoyaltyProgram': return { bg: '#fff3e0', icon: '⭐' };
      case 'EmailSubscription': return { bg: '#e3f2fd', icon: '✉' };
      case 'TradeInCredit': return { bg: '#e0f2f1', icon: '♻' };
      default: return { bg: '#f5f5f5', icon: '🏷' };
    }
  }

  _formatUsage(discount) {
    if (discount.totalUsageLimit) {
      return `${discount.usageCount || 0}/${discount.totalUsageLimit}`;
    }
    return `${discount.usageCount || 0} uses`;
  }

  _filteredDiscounts() {
    let list = this._discounts;
    if (this._searchTerm) {
      const q = this._searchTerm.toLowerCase();
      list = list.filter(d => d.name?.toLowerCase().includes(q) || d.code?.toLowerCase().includes(q));
    }
    return list;
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
    return html`
      <div class="container">
        ${this._renderListPanel()}
        ${this._renderEditorPanel()}
      </div>
    `;
  }

  _renderListPanel() {
    const statuses = ['Active', 'Scheduled', 'Expired'];
    const filtered = this._filteredDiscounts();

    return html`
      <div class="list-panel">
        <div class="list-header">
          <h2>Discounts</h2>
          <div class="search-row">
            <input type="text" placeholder="Search discounts..." .value=${this._searchTerm} @input=${(e) => { this._searchTerm = e.target.value; }} />
            <uui-button look="primary" compact label="New" @click=${this._createNew}>+ New</uui-button>
          </div>
          <div class="status-filters">
            ${statuses.map(s => html`
              <button class="status-filter ${this._statusFilter === s.toLowerCase() ? 'active' : ''}" @click=${() => this._handleStatusFilter(s.toLowerCase())}>${s}</button>
            `)}
          </div>
        </div>
        <div class="list-content">
          ${this._loading
            ? html`<div class="loading"><uui-loader></uui-loader></div>`
            : filtered.length === 0
              ? html`<div style="padding:40px;text-align:center;color:#999;">No discounts found</div>`
              : filtered.map(d => this._renderListItem(d))
          }
        </div>
      </div>
    `;
  }

  _renderListItem(discount) {
    const isActive = this._editingDiscount?.id === discount.id;
    const typeInfo = this._getTypeIcon(discount.type || discount.discountType);
    const status = this._getStatusBadge(discount);

    return html`
      <div class="list-item ${isActive ? 'active' : ''}" @click=${() => this._selectDiscount(discount)}>
        <div class="list-item-icon" style="background:${typeInfo.bg};">${typeInfo.icon}</div>
        <div class="list-item-info">
          <div class="list-item-name">${discount.name}</div>
          <div class="list-item-meta">
            ${discount.code ? html`<span class="discount-code">${discount.code}</span>` : ''}
            <span class="discount-value-badge">${discount.displayValue || `${discount.discountValue}%`}</span>
            <span class="badge badge-${status}">${discount.status || 'Inactive'}</span>
          </div>
        </div>
      </div>
    `;
  }

  _renderEditorPanel() {
    if (!this._editingDiscount) {
      return html`
        <div class="editor-panel">
          <div class="empty-state">
            <uui-icon name="icon-tag"></uui-icon>
            <h3>Select a discount</h3>
            <p>Choose a discount from the list or create a new one</p>
            <uui-button look="primary" label="Create Discount" @click=${this._createNew}>Create Discount</uui-button>
          </div>
        </div>
      `;
    }

    const isNew = !this._editingDiscount.id;
    const tabs = ['General', 'Type Settings', 'Conditions', 'Settings'];

    return html`
      <div class="editor-panel">
        <div class="editor-header">
          <h2>${isNew ? 'New Discount' : this._editingDiscount.name}</h2>
          <div class="editor-actions">
            ${!isNew ? html`<uui-button look="secondary" color="danger" label="Delete" @click=${this._deleteDiscount}>Delete</uui-button>` : ''}
            <uui-button look="primary" label="Save" @click=${this._saveDiscount} ?disabled=${this._saving}>
              ${this._saving ? 'Saving...' : 'Save'}
            </uui-button>
          </div>
        </div>

        <div class="tabs">
          ${tabs.map(t => html`
            <div class="tab ${this._activeTab === t.toLowerCase().replace(' ', '-') ? 'active' : ''}" @click=${() => { this._activeTab = t.toLowerCase().replace(' ', '-'); }}>
              ${t}
            </div>
          `)}
        </div>

        <div class="editor-body">
          <div class="tab-content ${this._activeTab === 'general' ? 'active' : ''}">
            ${this._renderGeneralTab()}
          </div>
          <div class="tab-content ${this._activeTab === 'type-settings' ? 'active' : ''}">
            ${this._renderTypeSettingsTab()}
          </div>
          <div class="tab-content ${this._activeTab === 'conditions' ? 'active' : ''}">
            ${this._renderConditionsTab()}
          </div>
          <div class="tab-content ${this._activeTab === 'settings' ? 'active' : ''}">
            ${this._renderSettingsTab()}
          </div>
        </div>
      </div>
    `;
  }

  _renderGeneralTab() {
    const d = this._editingDiscount;
    return html`
      <div class="form-section">
        <div class="form-section-title">Basic Information</div>
        <div class="form-group">
          <label>Discount Name *</label>
          <input type="text" .value=${d.name || ''} @input=${(e) => this._handleInputChange('name', e.target.value)} />
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Coupon Code</label>
            <input type="text" placeholder="Leave empty for auto discount" .value=${d.code || ''} @input=${(e) => this._handleInputChange('code', e.target.value)} />
          </div>
          <div class="form-group">
            <label>Discount Type</label>
            <select .value=${d.type || d.discountType || 'Percentage'} @change=${(e) => this._handleInputChange('type', e.target.value)}>
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
            <input type="number" step="0.01" .value=${d.discountValue || 0} @input=${(e) => this._handleInputChange('discountValue', parseFloat(e.target.value) || 0)} />
          </div>
          <div class="form-group">
            <label>Max Discount Amount</label>
            <input type="number" step="0.01" placeholder="No limit" .value=${d.maximumDiscountAmount || ''} @input=${(e) => this._handleInputChange('maximumDiscountAmount', e.target.value ? parseFloat(e.target.value) : null)} />
          </div>
        </div>
        <div class="form-group">
          <label>Description</label>
          <textarea .value=${d.description || ''} @input=${(e) => this._handleInputChange('description', e.target.value)}></textarea>
        </div>
      </div>

      <div class="form-section">
        <div class="form-section-title">Validity Period</div>
        <div class="form-row">
          <div class="form-group">
            <label>Start Date</label>
            <input type="date" .value=${d.startDate || ''} @input=${(e) => this._handleInputChange('startDate', e.target.value)} />
          </div>
          <div class="form-group">
            <label>End Date</label>
            <input type="date" .value=${d.endDate || ''} @input=${(e) => this._handleInputChange('endDate', e.target.value)} />
          </div>
        </div>
      </div>

      ${d.id ? html`
        <div class="form-section">
          <div class="form-section-title">Usage Statistics</div>
          <div class="stats-grid">
            <div class="stat-card">
              <div class="stat-value">${d.usageCount || 0}</div>
              <div class="stat-label">Times Used</div>
            </div>
            <div class="stat-card">
              <div class="stat-value">${d.totalUsageLimit || '∞'}</div>
              <div class="stat-label">Total Limit</div>
            </div>
            <div class="stat-card">
              <div class="stat-value">${d.perCustomerUsageLimit || '∞'}</div>
              <div class="stat-label">Per Customer</div>
            </div>
            <div class="stat-card">
              <div class="stat-value">${d.status || 'N/A'}</div>
              <div class="stat-label">Status</div>
            </div>
          </div>
        </div>
      ` : ''}
    `;
  }

  _renderTypeSettingsTab() {
    const type = this._editingDiscount?.type || this._editingDiscount?.discountType || 'Percentage';

    switch (type) {
      case 'BuyXGetY':
        return html`
          <div class="form-section">
            <div class="form-section-title">Buy X Get Y Settings</div>
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
          </div>
        `;

      case 'EarlyPayment':
        return html`
          <div class="form-section">
            <div class="form-section-title">Early Payment Settings</div>
            <div class="form-row">
              <div class="form-group">
                <label>Early Payment Days *</label>
                <input type="number" min="1" placeholder="e.g. 10" .value=${this._editingDiscount?.earlyPaymentDays || ''} @input=${(e) => this._handleInputChange('earlyPaymentDays', e.target.value ? parseInt(e.target.value) : null)} />
                <small>Days within which payment qualifies for discount</small>
              </div>
              <div class="form-group">
                <label>Standard Payment Days</label>
                <input type="number" min="1" placeholder="e.g. 30" .value=${this._editingDiscount?.standardPaymentDays || ''} @input=${(e) => this._handleInputChange('standardPaymentDays', e.target.value ? parseInt(e.target.value) : null)} />
                <small>Normal payment term (e.g. Net 30)</small>
              </div>
            </div>
          </div>
        `;

      case 'Overstock':
        return html`
          <div class="form-section">
            <div class="form-section-title">Overstock / Clearance Settings</div>
            <div class="checkbox-row">
              <div class="checkbox-item">
                <input type="checkbox" id="isOverstockClearance" .checked=${this._editingDiscount?.isOverstockClearance !== false} @change=${(e) => this._handleInputChange('isOverstockClearance', e.target.checked)} />
                <label for="isOverstockClearance">Mark as clearance sale</label>
              </div>
            </div>
            <small style="color:#666;">Apply to specific products or categories using the Conditions tab. Great for moving excess inventory.</small>
          </div>
        `;

      case 'Bundle':
        return html`
          <div class="form-section">
            <div class="form-section-title">Bundle Settings</div>
            <div class="form-group">
              <label>Bundle Discount Percentage</label>
              <input type="number" step="0.01" min="0" max="100" .value=${this._editingDiscount?.bundleDiscountValue || ''} @input=${(e) => this._handleInputChange('bundleDiscountValue', e.target.value ? parseFloat(e.target.value) : null)} />
              <small>Percentage off when all bundle products are in the cart. Specify bundle products in the Conditions tab.</small>
            </div>
          </div>
        `;

      case 'BulkVolume':
        return html`
          <div class="form-section">
            <div class="form-section-title">Volume Tier Settings</div>
            <small style="color:#666;display:block;margin-bottom:12px;">Define quantity-based discount tiers. Higher quantities get bigger discounts.</small>
            ${(this._editingDiscount?.volumeTiers || []).map((tier, i) => html`
              <div class="volume-tier-row">
                <div class="form-group" style="margin-bottom:0;flex:1;">
                  <label>Min Qty</label>
                  <input type="number" min="1" .value=${tier.minQuantity || ''} @input=${(e) => this._updateVolumeTier(i, 'minQuantity', parseInt(e.target.value) || 0)} />
                </div>
                <div class="form-group" style="margin-bottom:0;flex:1;">
                  <label>Discount %</label>
                  <input type="number" step="0.01" min="0" max="100" .value=${tier.discountPercent || ''} @input=${(e) => this._updateVolumeTier(i, 'discountPercent', parseFloat(e.target.value) || 0)} />
                </div>
                <uui-button look="secondary" compact label="Remove" style="margin-top:22px;" @click=${() => this._removeVolumeTier(i)}>&times;</uui-button>
              </div>
            `)}
            <uui-button look="secondary" style="margin-top:8px;" label="Add Tier" @click=${this._addVolumeTier}>+ Add Tier</uui-button>
          </div>
        `;

      case 'Seasonal':
        return html`
          <div class="form-section">
            <div class="form-section-title">Seasonal Settings</div>
            <div class="form-group">
              <label>Season Label</label>
              <input type="text" placeholder="e.g. Summer Sale, Post-Holiday Clearance" .value=${this._editingDiscount?.seasonLabel || ''} @input=${(e) => this._handleInputChange('seasonLabel', e.target.value)} />
            </div>
          </div>
        `;

      case 'Referral':
        return html`
          <div class="form-section">
            <div class="form-section-title">Referral Settings</div>
            <div class="checkbox-row">
              <div class="checkbox-item">
                <input type="checkbox" id="referralTwoWay" .checked=${this._editingDiscount?.referralTwoWay === true} @change=${(e) => this._handleInputChange('referralTwoWay', e.target.checked)} />
                <label for="referralTwoWay">Two-way referral (both referrer and new customer get discount)</label>
              </div>
            </div>
            ${this._editingDiscount?.referralTwoWay ? html`
              <div class="form-group">
                <label>New Customer Discount Value</label>
                <input type="number" step="0.01" .value=${this._editingDiscount?.referralNewCustomerValue || ''} @input=${(e) => this._handleInputChange('referralNewCustomerValue', e.target.value ? parseFloat(e.target.value) : null)} />
                <small>The "Value" field on the General tab applies to the referrer.</small>
              </div>
            ` : ''}
          </div>
        `;

      case 'LoyaltyProgram':
        return html`
          <div class="form-section">
            <div class="form-section-title">Loyalty Program Settings</div>
            <div class="form-row">
              <div class="form-group">
                <label>Points Threshold</label>
                <input type="number" min="0" .value=${this._editingDiscount?.loyaltyPointsThreshold || ''} @input=${(e) => this._handleInputChange('loyaltyPointsThreshold', e.target.value ? parseInt(e.target.value) : null)} />
                <small>Points needed to unlock this discount</small>
              </div>
              <div class="form-group">
                <label>Required Tier</label>
                <input type="text" placeholder="e.g. Gold, Platinum" .value=${this._editingDiscount?.loyaltyTierRequired || ''} @input=${(e) => this._handleInputChange('loyaltyTierRequired', e.target.value)} />
              </div>
            </div>
          </div>
        `;

      case 'EmailSubscription':
        return html`
          <div class="form-section">
            <div class="form-section-title">Email Subscription Settings</div>
            <div class="checkbox-row">
              <div class="checkbox-item">
                <input type="checkbox" id="requiresEmailSubscription" .checked=${this._editingDiscount?.requiresEmailSubscription !== false} @change=${(e) => this._handleInputChange('requiresEmailSubscription', e.target.checked)} />
                <label for="requiresEmailSubscription">Requires new email subscription sign-up</label>
              </div>
            </div>
            <div class="checkbox-row">
              <div class="checkbox-item">
                <input type="checkbox" id="isCartAbandonmentRecovery" .checked=${this._editingDiscount?.isCartAbandonmentRecovery === true} @change=${(e) => this._handleInputChange('isCartAbandonmentRecovery', e.target.checked)} />
                <label for="isCartAbandonmentRecovery">Cart abandonment recovery discount</label>
              </div>
            </div>
          </div>
        `;

      case 'TradeInCredit':
        return html`
          <div class="form-section">
            <div class="form-section-title">Trade-In Credit Settings</div>
            <div class="form-group">
              <label>Credit Per Item Traded In</label>
              <input type="number" step="0.01" min="0" .value=${this._editingDiscount?.tradeInCreditPerItem || ''} @input=${(e) => this._handleInputChange('tradeInCreditPerItem', e.target.value ? parseFloat(e.target.value) : null)} />
              <small>Credit amount given per traded-in item. Use the Conditions tab to specify eligible trade-in and target products.</small>
            </div>
          </div>
        `;

      default:
        return html`
          <div class="form-section">
            <div style="padding:40px;text-align:center;color:#999;">
              <p>No additional settings for this discount type.</p>
              <small>Configure value and conditions on the other tabs.</small>
            </div>
          </div>
        `;
    }
  }

  _renderConditionsTab() {
    const d = this._editingDiscount;
    return html`
      <div class="form-section">
        <div class="form-section-title">Order Requirements</div>
        <div class="form-row">
          <div class="form-group">
            <label>Minimum Order Amount</label>
            <input type="number" step="0.01" placeholder="No minimum" .value=${d.minimumOrderAmount || ''} @input=${(e) => this._handleInputChange('minimumOrderAmount', e.target.value ? parseFloat(e.target.value) : null)} />
          </div>
          <div class="form-group">
            <label>Scope</label>
            <select .value=${d.scope || 'Order'} @change=${(e) => this._handleInputChange('scope', e.target.value)}>
              <option value="Order">Entire Order</option>
              <option value="Product">Specific Products</option>
              <option value="Category">Specific Categories</option>
              <option value="Shipping">Shipping</option>
            </select>
          </div>
        </div>
      </div>

      <div class="form-section">
        <div class="form-section-title">Usage Limits</div>
        <div class="form-row">
          <div class="form-group">
            <label>Total Usage Limit</label>
            <input type="number" placeholder="Unlimited" .value=${d.totalUsageLimit || ''} @input=${(e) => this._handleInputChange('totalUsageLimit', e.target.value ? parseInt(e.target.value) : null)} />
          </div>
          <div class="form-group">
            <label>Per Customer Limit</label>
            <input type="number" placeholder="Unlimited" .value=${d.perCustomerUsageLimit || ''} @input=${(e) => this._handleInputChange('perCustomerUsageLimit', e.target.value ? parseInt(e.target.value) : null)} />
          </div>
        </div>
      </div>

      <div class="form-section">
        <div class="form-section-title">First-Time Purchase</div>
        <div class="checkbox-row">
          <div class="checkbox-item">
            <input type="checkbox" id="firstTimeOnly" .checked=${d.isFirstOrderOnly === true} @change=${(e) => this._handleInputChange('isFirstOrderOnly', e.target.checked)} />
            <label for="firstTimeOnly">Only valid for first-time customers</label>
          </div>
        </div>
      </div>
    `;
  }

  _renderSettingsTab() {
    const d = this._editingDiscount;
    return html`
      <div class="form-section">
        <div class="form-section-title">Status</div>
        <div class="checkbox-row">
          <div class="checkbox-item">
            <input type="checkbox" id="isActive" .checked=${d.isActive !== false} @change=${(e) => this._handleInputChange('isActive', e.target.checked)} />
            <label for="isActive">Active</label>
          </div>
        </div>
      </div>

      <div class="form-section">
        <div class="form-section-title">Stacking</div>
        <div class="checkbox-row">
          <div class="checkbox-item">
            <input type="checkbox" id="isCombinable" .checked=${d.isCombinableWithOtherDiscounts !== false} @change=${(e) => this._handleInputChange('isCombinableWithOtherDiscounts', e.target.checked)} />
            <label for="isCombinable">Combinable with other discounts</label>
          </div>
        </div>
        <div class="checkbox-row">
          <div class="checkbox-item">
            <input type="checkbox" id="autoApply" .checked=${d.isAutoApplied === true} @change=${(e) => this._handleInputChange('isAutoApplied', e.target.checked)} />
            <label for="autoApply">Auto-apply (no coupon code required)</label>
          </div>
        </div>
      </div>

      <div class="form-section">
        <div class="form-section-title">Priority</div>
        <div class="form-group">
          <label>Sort Order</label>
          <input type="number" placeholder="0" .value=${d.sortOrder || ''} @input=${(e) => this._handleInputChange('sortOrder', e.target.value ? parseInt(e.target.value) : null)} />
          <small>Lower numbers are applied first when multiple discounts are active.</small>
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-discount-collection', DiscountCollection);
export default DiscountCollection;
