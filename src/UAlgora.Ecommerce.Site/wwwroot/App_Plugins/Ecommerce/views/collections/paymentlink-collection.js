import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

export class PaymentLinkCollection extends UmbElementMixin(LitElement) {
  static styles = css`
    :host { display: block; height: 100%; }

    /* Split Pane Layout */
    .split-container { display: flex; height: calc(100vh - 60px); }
    .list-panel { width: 380px; min-width: 380px; border-right: 1px solid #e0e0e0; display: flex; flex-direction: column; background: #fafafa; }
    .editor-panel { flex: 1; display: flex; flex-direction: column; overflow: hidden; }

    /* List Panel Header */
    .list-header { padding: 16px; border-bottom: 1px solid #e0e0e0; background: #fff; }
    .list-header h3 { margin: 0 0 12px 0; font-size: 18px; display: flex; align-items: center; justify-content: space-between; }
    .search-box { position: relative; }
    .search-box input { width: 100%; padding: 8px 12px 8px 32px; border: 1px solid #ddd; border-radius: 4px; box-sizing: border-box; }
    .search-box uui-icon { position: absolute; left: 10px; top: 50%; transform: translateY(-50%); color: #999; }

    /* Filter Chips */
    .filter-chips { display: flex; gap: 8px; padding: 12px 16px; border-bottom: 1px solid #e0e0e0; flex-wrap: wrap; background: #fff; }
    .filter-chip { padding: 4px 12px; border-radius: 16px; font-size: 12px; cursor: pointer; border: 1px solid #ddd; background: #fff; transition: all 0.2s; }
    .filter-chip:hover { background: #f0f0f0; }
    .filter-chip.active { background: #1b264f; color: white; border-color: #1b264f; }
    .filter-chip .count { margin-left: 4px; opacity: 0.7; }

    /* Payment Link List */
    .link-list { flex: 1; overflow-y: auto; }
    .link-item { padding: 14px 16px; border-bottom: 1px solid #e9e9e9; cursor: pointer; transition: background 0.2s; }
    .link-item:hover { background: #f5f5f5; }
    .link-item.selected { background: #e3f2fd; border-left: 3px solid #1976d2; }
    .link-item-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 6px; }
    .link-name { font-weight: 600; font-size: 14px; }
    .link-amount { font-weight: 600; color: #2e7d32; }
    .link-code { font-family: monospace; font-size: 11px; color: #666; background: #f0f0f0; padding: 2px 6px; border-radius: 3px; margin-bottom: 6px; display: inline-block; }
    .link-meta { display: flex; gap: 8px; align-items: center; flex-wrap: wrap; }
    .status-badge { padding: 3px 8px; border-radius: 4px; font-size: 11px; font-weight: 600; }
    .status-badge.active { background: #e8f5e9; color: #2e7d32; }
    .status-badge.paused { background: #fff3e0; color: #ef6c00; }
    .status-badge.expired { background: #ffebee; color: #c62828; }
    .status-badge.completed { background: #e3f2fd; color: #1565c0; }
    .status-badge.archived { background: #e0e0e0; color: #616161; }
    .type-badge { font-size: 11px; color: #666; background: #f5f5f5; padding: 2px 6px; border-radius: 3px; }
    .usage-info { font-size: 11px; color: #999; }

    /* Editor Panel */
    .editor-header { padding: 16px 20px; border-bottom: 1px solid #e0e0e0; background: #fff; display: flex; justify-content: space-between; align-items: center; }
    .editor-header h3 { margin: 0; font-size: 18px; }
    .editor-actions { display: flex; gap: 8px; }

    /* Tabs */
    .tabs { display: flex; border-bottom: 1px solid #e0e0e0; background: #fff; padding: 0 20px; }
    .tab { padding: 12px 20px; cursor: pointer; border-bottom: 2px solid transparent; font-weight: 500; color: #666; transition: all 0.2s; }
    .tab:hover { color: #333; background: #f9f9f9; }
    .tab.active { color: #1976d2; border-bottom-color: #1976d2; }

    /* Tab Content */
    .tab-content { flex: 1; overflow-y: auto; padding: 20px; background: #fafafa; }

    /* Form Styles */
    .form-section { background: #fff; border: 1px solid #e0e0e0; border-radius: 8px; padding: 20px; margin-bottom: 20px; }
    .form-section h4 { margin: 0 0 16px 0; padding-bottom: 12px; border-bottom: 1px solid #eee; color: #333; }
    .form-row { display: grid; grid-template-columns: repeat(2, 1fr); gap: 16px; margin-bottom: 16px; }
    .form-row.triple { grid-template-columns: repeat(3, 1fr); }
    .form-row.single { grid-template-columns: 1fr; }
    .form-group { margin-bottom: 16px; }
    .form-group:last-child { margin-bottom: 0; }
    .form-group label { display: block; margin-bottom: 6px; font-weight: 500; color: #333; font-size: 13px; }
    .form-group input, .form-group select, .form-group textarea { width: 100%; padding: 10px 12px; border: 1px solid #ddd; border-radius: 4px; font-size: 14px; box-sizing: border-box; }
    .form-group input:focus, .form-group select:focus, .form-group textarea:focus { outline: none; border-color: #1976d2; box-shadow: 0 0 0 2px rgba(25, 118, 210, 0.1); }
    .form-group textarea { resize: vertical; min-height: 80px; }
    .form-group small { display: block; margin-top: 4px; color: #666; font-size: 12px; }
    .form-group .input-with-action { display: flex; gap: 8px; }
    .form-group .input-with-action input { flex: 1; }

    /* Toggle Switch */
    .toggle-group { display: flex; align-items: center; gap: 12px; }
    .toggle-switch { position: relative; width: 44px; height: 24px; }
    .toggle-switch input { opacity: 0; width: 0; height: 0; }
    .toggle-slider { position: absolute; cursor: pointer; top: 0; left: 0; right: 0; bottom: 0; background: #ccc; border-radius: 24px; transition: 0.3s; }
    .toggle-slider:before { position: absolute; content: ""; height: 18px; width: 18px; left: 3px; bottom: 3px; background: white; border-radius: 50%; transition: 0.3s; }
    .toggle-switch input:checked + .toggle-slider { background: #4caf50; }
    .toggle-switch input:checked + .toggle-slider:before { transform: translateX(20px); }
    .toggle-label { font-size: 14px; }

    /* Link Preview */
    .link-preview { background: #f8f9fa; border: 1px solid #e0e0e0; border-radius: 8px; padding: 16px; margin-bottom: 20px; }
    .link-preview h5 { margin: 0 0 12px 0; font-size: 13px; color: #666; }
    .link-url { display: flex; align-items: center; gap: 8px; background: #fff; padding: 10px 12px; border-radius: 4px; border: 1px solid #ddd; }
    .link-url code { flex: 1; font-family: monospace; font-size: 13px; word-break: break-all; }
    .link-url button { flex-shrink: 0; }

    /* Stats Cards */
    .stats-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; margin-bottom: 20px; }
    .stat-card { background: #fff; border: 1px solid #e0e0e0; border-radius: 8px; padding: 16px; text-align: center; }
    .stat-card .stat-value { font-size: 28px; font-weight: 700; margin-bottom: 4px; }
    .stat-card .stat-label { font-size: 12px; color: #666; }
    .stat-card.success .stat-value { color: #4caf50; }
    .stat-card.primary .stat-value { color: #1976d2; }
    .stat-card.warning .stat-value { color: #ff9800; }

    /* Payment History Table */
    .payment-table { width: 100%; border-collapse: collapse; background: #fff; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden; }
    .payment-table th, .payment-table td { padding: 12px 16px; text-align: left; border-bottom: 1px solid #e0e0e0; }
    .payment-table th { background: #f8f9fa; font-weight: 600; font-size: 13px; }
    .payment-table tr:last-child td { border-bottom: none; }
    .payment-table tr:hover { background: #f9f9f9; }
    .payment-status { padding: 4px 8px; border-radius: 4px; font-size: 11px; font-weight: 600; }
    .payment-status.completed { background: #e8f5e9; color: #2e7d32; }
    .payment-status.pending { background: #fff3e0; color: #ef6c00; }
    .payment-status.failed { background: #ffebee; color: #c62828; }

    /* Empty States */
    .empty-state { display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100%; color: #666; text-align: center; padding: 40px; }
    .empty-state uui-icon { font-size: 64px; margin-bottom: 16px; opacity: 0.3; }
    .empty-state h3 { margin: 0 0 8px 0; }
    .empty-state p { margin: 0 0 20px 0; }

    .loading { display: flex; justify-content: center; align-items: center; height: 200px; }

    /* Type Selector Cards */
    .type-cards { display: grid; grid-template-columns: repeat(3, 1fr); gap: 12px; margin-bottom: 20px; }
    .type-card { border: 2px solid #e0e0e0; border-radius: 8px; padding: 16px; cursor: pointer; transition: all 0.2s; text-align: center; }
    .type-card:hover { border-color: #1976d2; background: #f5f9ff; }
    .type-card.selected { border-color: #1976d2; background: #e3f2fd; }
    .type-card uui-icon { font-size: 24px; margin-bottom: 8px; color: #1976d2; }
    .type-card h5 { margin: 0 0 4px 0; font-size: 14px; }
    .type-card p { margin: 0; font-size: 11px; color: #666; }

    /* Quick Actions */
    .quick-actions { display: flex; gap: 8px; flex-wrap: wrap; margin-bottom: 20px; }
    .quick-action { padding: 8px 16px; border: 1px solid #ddd; border-radius: 4px; background: #fff; cursor: pointer; display: flex; align-items: center; gap: 6px; font-size: 13px; transition: all 0.2s; }
    .quick-action:hover { background: #f5f5f5; border-color: #ccc; }
    .quick-action.primary { background: #1976d2; color: white; border-color: #1976d2; }
    .quick-action.primary:hover { background: #1565c0; }
    .quick-action.danger { color: #c62828; }
    .quick-action.danger:hover { background: #ffebee; }
  `;

  static properties = {
    _paymentLinks: { state: true },
    _loading: { state: true },
    _searchTerm: { state: true },
    _statusFilter: { state: true },
    _selectedLink: { state: true },
    _activeTab: { state: true },
    _mode: { state: true },
    _formData: { state: true },
    _saving: { state: true },
    _payments: { state: true },
    _loadingPayments: { state: true },
    _statistics: { state: true },
    _currencies: { state: true }
  };

  constructor() {
    super();
    this._paymentLinks = [];
    this._loading = true;
    this._searchTerm = '';
    this._statusFilter = 'all';
    this._selectedLink = null;
    this._activeTab = 'details';
    this._mode = 'view';
    this._formData = this._getEmptyFormData();
    this._saving = false;
    this._payments = [];
    this._loadingPayments = false;
    this._statistics = null;
    this._currencies = [];

    this._types = [
      { value: 0, label: 'Fixed Amount', icon: 'icon-coins-dollar-alt', desc: 'Charge a specific amount' },
      { value: 1, label: 'Customer Choice', icon: 'icon-edit', desc: 'Let customer enter amount' },
      { value: 2, label: 'Product', icon: 'icon-box', desc: 'Link to a specific product' },
      { value: 3, label: 'Subscription', icon: 'icon-axis-rotation', desc: 'Recurring payments' },
      { value: 4, label: 'Invoice', icon: 'icon-document', desc: 'Invoice payment' },
      { value: 5, label: 'Donation', icon: 'icon-hearts', desc: 'Accept donations' }
    ];

    this._statusLabels = {
      0: 'Active',
      1: 'Paused',
      2: 'Expired',
      3: 'Completed',
      4: 'Archived'
    };
  }

  _getEmptyFormData() {
    return {
      name: '',
      code: '',
      description: '',
      type: 0,
      amount: '',
      currencyCode: 'USD',
      minimumAmount: '',
      maximumAmount: '',
      suggestedAmounts: '',
      allowTip: false,
      tipPercentages: '10,15,20',
      validFrom: '',
      expiresAt: '',
      maxUses: '',
      requireEmail: true,
      requirePhone: false,
      requireBillingAddress: false,
      requireShippingAddress: false,
      successMessage: '',
      successRedirectUrl: '',
      brandColor: '#1976d2',
      notificationEmail: '',
      sendCustomerReceipt: true,
      referenceNumber: '',
      notes: ''
    };
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadPaymentLinks();
    this._loadCurrencies();
  }

  async _loadCurrencies() {
    try {
      const res = await fetch('/umbraco/management/api/v1/ecommerce/currency', { credentials: 'include', headers: { 'Accept': 'application/json' } });
      if (res.ok) {
        const data = await res.json();
        this._currencies = data.items || data || [];
      }
    } catch (e) { console.error('Error loading currencies:', e); }
  }

  async _loadPaymentLinks() {
    try {
      this._loading = true;
      const response = await fetch('/umbraco/management/api/v1/ecommerce/payment-link', {
        credentials: 'include',
        headers: { 'Accept': 'application/json' }
      });
      if (response.ok) {
        this._paymentLinks = await response.json();
      }
    } catch (error) {
      console.error('Error loading payment links:', error);
    } finally {
      this._loading = false;
    }
  }

  async _loadPayments(linkId) {
    try {
      this._loadingPayments = true;
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/payment-link/${linkId}/payments`, {
        credentials: 'include',
        headers: { 'Accept': 'application/json' }
      });
      if (response.ok) {
        this._payments = await response.json();
      }
    } catch (error) {
      console.error('Error loading payments:', error);
      this._payments = [];
    } finally {
      this._loadingPayments = false;
    }
  }

  async _loadStatistics(linkId) {
    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/payment-link/${linkId}/statistics`, {
        credentials: 'include',
        headers: { 'Accept': 'application/json' }
      });
      if (response.ok) {
        this._statistics = await response.json();
      }
    } catch (error) {
      console.error('Error loading statistics:', error);
      this._statistics = null;
    }
  }

  _getFilteredLinks() {
    let filtered = [...this._paymentLinks];

    if (this._searchTerm) {
      const term = this._searchTerm.toLowerCase();
      filtered = filtered.filter(l =>
        l.name?.toLowerCase().includes(term) ||
        l.code?.toLowerCase().includes(term) ||
        l.description?.toLowerCase().includes(term)
      );
    }

    if (this._statusFilter !== 'all') {
      const statusValue = parseInt(this._statusFilter);
      filtered = filtered.filter(l => l.status === statusValue);
    }

    return filtered;
  }

  _getStatusCounts() {
    return {
      all: this._paymentLinks.length,
      0: this._paymentLinks.filter(l => l.status === 0).length,
      1: this._paymentLinks.filter(l => l.status === 1).length,
      2: this._paymentLinks.filter(l => l.status === 2).length,
      3: this._paymentLinks.filter(l => l.status === 3).length
    };
  }

  _selectLink(link) {
    this._selectedLink = link;
    this._mode = 'view';
    this._activeTab = 'details';
    this._formData = this._linkToFormData(link);
    this._loadPayments(link.id);
    this._loadStatistics(link.id);
  }

  _linkToFormData(link) {
    return {
      name: link.name || '',
      code: link.code || '',
      description: link.description || '',
      type: link.type || 0,
      amount: link.amount?.toString() || '',
      currencyCode: link.currencyCode || 'USD',
      minimumAmount: link.minimumAmount?.toString() || '',
      maximumAmount: link.maximumAmount?.toString() || '',
      suggestedAmounts: link.suggestedAmountsJson || '',
      allowTip: link.allowTip || false,
      tipPercentages: link.tipPercentagesJson || '10,15,20',
      validFrom: link.validFrom ? link.validFrom.split('T')[0] : '',
      expiresAt: link.expiresAt ? link.expiresAt.split('T')[0] : '',
      maxUses: link.maxUses?.toString() || '',
      requireEmail: link.requireEmail ?? true,
      requirePhone: link.requirePhone || false,
      requireBillingAddress: link.requireBillingAddress || false,
      requireShippingAddress: link.requireShippingAddress || false,
      successMessage: link.successMessage || '',
      successRedirectUrl: link.successRedirectUrl || '',
      brandColor: link.brandColor || '#1976d2',
      notificationEmail: link.notificationEmail || '',
      sendCustomerReceipt: link.sendCustomerReceipt ?? true,
      referenceNumber: link.referenceNumber || '',
      notes: link.notes || ''
    };
  }

  _startCreate() {
    this._selectedLink = null;
    this._mode = 'create';
    this._activeTab = 'details';
    this._formData = this._getEmptyFormData();
    this._payments = [];
    this._statistics = null;
  }

  _startEdit() {
    this._mode = 'edit';
  }

  _cancelEdit() {
    if (this._selectedLink) {
      this._mode = 'view';
      this._formData = this._linkToFormData(this._selectedLink);
    } else {
      this._selectedLink = null;
      this._mode = 'view';
    }
  }

  _handleInputChange(field, value) {
    this._formData = { ...this._formData, [field]: value };
  }

  async _generateCode() {
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/payment-link/generate-code', {
        credentials: 'include',
        headers: { 'Accept': 'application/json' }
      });
      if (response.ok) {
        const result = await response.json();
        this._formData = { ...this._formData, code: result.code };
      }
    } catch (error) {
      console.error('Error generating code:', error);
    }
  }

  async _saveLink() {
    if (!this._formData.name) {
      alert('Name is required');
      return;
    }

    this._saving = true;
    try {
      const isNew = this._mode === 'create';
      const payload = {
        name: this._formData.name,
        code: this._formData.code || null,
        description: this._formData.description || null,
        type: parseInt(this._formData.type),
        amount: this._formData.amount ? parseFloat(this._formData.amount) : null,
        currencyCode: this._formData.currencyCode,
        minimumAmount: this._formData.minimumAmount ? parseFloat(this._formData.minimumAmount) : null,
        maximumAmount: this._formData.maximumAmount ? parseFloat(this._formData.maximumAmount) : null,
        suggestedAmountsJson: this._formData.suggestedAmounts || null,
        allowTip: this._formData.allowTip,
        tipPercentagesJson: this._formData.tipPercentages || null,
        validFrom: this._formData.validFrom || null,
        expiresAt: this._formData.expiresAt || null,
        maxUses: this._formData.maxUses ? parseInt(this._formData.maxUses) : null,
        requireEmail: this._formData.requireEmail,
        requirePhone: this._formData.requirePhone,
        requireBillingAddress: this._formData.requireBillingAddress,
        requireShippingAddress: this._formData.requireShippingAddress,
        successMessage: this._formData.successMessage || null,
        successRedirectUrl: this._formData.successRedirectUrl || null,
        brandColor: this._formData.brandColor || null,
        notificationEmail: this._formData.notificationEmail || null,
        sendCustomerReceipt: this._formData.sendCustomerReceipt,
        referenceNumber: this._formData.referenceNumber || null,
        notes: this._formData.notes || null
      };

      const url = isNew
        ? '/umbraco/management/api/v1/ecommerce/payment-link'
        : `/umbraco/management/api/v1/ecommerce/payment-link/${this._selectedLink.id}`;

      const response = await fetch(url, {
        method: isNew ? 'POST' : 'PUT',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });

      if (!response.ok) throw new Error('Failed to save payment link');

      const savedLink = await response.json();
      await this._loadPaymentLinks();

      const link = this._paymentLinks.find(l => l.id === (savedLink.id || this._selectedLink?.id));
      if (link) {
        this._selectLink(link);
      }
      this._mode = 'view';
    } catch (error) {
      alert('Failed to save payment link: ' + error.message);
    } finally {
      this._saving = false;
    }
  }

  async _deleteLink() {
    if (!this._selectedLink || !confirm('Are you sure you want to delete this payment link?')) return;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/payment-link/${this._selectedLink.id}`, {
        method: 'DELETE',
        credentials: 'include'
      });

      if (!response.ok) throw new Error('Failed to delete');

      this._selectedLink = null;
      this._mode = 'view';
      await this._loadPaymentLinks();
    } catch (error) {
      alert('Failed to delete: ' + error.message);
    }
  }

  async _pauseLink() {
    if (!this._selectedLink) return;
    try {
      await fetch(`/umbraco/management/api/v1/ecommerce/payment-link/${this._selectedLink.id}/pause`, {
        method: 'POST',
        credentials: 'include'
      });
      await this._loadPaymentLinks();
      const link = this._paymentLinks.find(l => l.id === this._selectedLink.id);
      if (link) this._selectLink(link);
    } catch (error) {
      alert('Failed to pause: ' + error.message);
    }
  }

  async _activateLink() {
    if (!this._selectedLink) return;
    try {
      await fetch(`/umbraco/management/api/v1/ecommerce/payment-link/${this._selectedLink.id}/activate`, {
        method: 'POST',
        credentials: 'include'
      });
      await this._loadPaymentLinks();
      const link = this._paymentLinks.find(l => l.id === this._selectedLink.id);
      if (link) this._selectLink(link);
    } catch (error) {
      alert('Failed to activate: ' + error.message);
    }
  }

  async _duplicateLink() {
    if (!this._selectedLink) return;
    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/payment-link/${this._selectedLink.id}/duplicate`, {
        method: 'POST',
        credentials: 'include'
      });
      if (response.ok) {
        const duplicate = await response.json();
        await this._loadPaymentLinks();
        const link = this._paymentLinks.find(l => l.id === duplicate.id);
        if (link) this._selectLink(link);
      }
    } catch (error) {
      alert('Failed to duplicate: ' + error.message);
    }
  }

  _copyToClipboard(text) {
    navigator.clipboard.writeText(text);
  }

  _formatCurrency(amount, currency = 'USD') {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency }).format(amount || 0);
  }

  _formatDate(dateStr) {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleDateString();
  }

  _getTypeLabel(type) {
    const t = this._types.find(t => t.value === type);
    return t ? t.label : 'Unknown';
  }

  _getStatusClass(status) {
    const classes = { 0: 'active', 1: 'paused', 2: 'expired', 3: 'completed', 4: 'archived' };
    return classes[status] || 'active';
  }

  render() {
    return html`
      <div class="split-container">
        ${this._renderListPanel()}
        ${this._renderEditorPanel()}
      </div>
    `;
  }

  _renderListPanel() {
    const counts = this._getStatusCounts();
    const links = this._getFilteredLinks();

    return html`
      <div class="list-panel">
        <div class="list-header">
          <h3>
            Payment Links
            <uui-button look="primary" compact @click=${this._startCreate}>
              <uui-icon name="icon-add"></uui-icon> Create
            </uui-button>
          </h3>
          <div class="search-box">
            <uui-icon name="icon-search"></uui-icon>
            <input
              type="text"
              placeholder="Search links..."
              .value=${this._searchTerm}
              @input=${(e) => this._searchTerm = e.target.value}
            />
          </div>
        </div>

        <div class="filter-chips">
          <span class="filter-chip ${this._statusFilter === 'all' ? 'active' : ''}" @click=${() => this._statusFilter = 'all'}>
            All<span class="count">(${counts.all})</span>
          </span>
          <span class="filter-chip ${this._statusFilter === '0' ? 'active' : ''}" @click=${() => this._statusFilter = '0'}>
            Active<span class="count">(${counts[0]})</span>
          </span>
          <span class="filter-chip ${this._statusFilter === '1' ? 'active' : ''}" @click=${() => this._statusFilter = '1'}>
            Paused<span class="count">(${counts[1]})</span>
          </span>
        </div>

        <div class="link-list">
          ${this._loading ? html`<div class="loading"><uui-loader></uui-loader></div>` :
            links.length === 0 ? html`
              <div class="empty-state" style="padding: 40px 20px;">
                <uui-icon name="icon-link"></uui-icon>
                <p>No payment links found</p>
              </div>
            ` :
            links.map(link => this._renderLinkItem(link))
          }
        </div>
      </div>
    `;
  }

  _renderLinkItem(link) {
    const isSelected = this._selectedLink?.id === link.id;

    return html`
      <div class="link-item ${isSelected ? 'selected' : ''}" @click=${() => this._selectLink(link)}>
        <div class="link-item-header">
          <span class="link-name">${link.name}</span>
          ${link.amount ? html`<span class="link-amount">${this._formatCurrency(link.amount, link.currencyCode)}</span>` : ''}
        </div>
        <div class="link-code">${link.code}</div>
        <div class="link-meta">
          <span class="status-badge ${this._getStatusClass(link.status)}">${this._statusLabels[link.status]}</span>
          <span class="type-badge">${this._getTypeLabel(link.type)}</span>
          <span class="usage-info">${link.usageCount || 0} uses${link.maxUses ? ` / ${link.maxUses}` : ''}</span>
        </div>
      </div>
    `;
  }

  _renderEditorPanel() {
    if (!this._selectedLink && this._mode !== 'create') {
      return html`
        <div class="editor-panel">
          <div class="empty-state">
            <uui-icon name="icon-link"></uui-icon>
            <h3>No Payment Link Selected</h3>
            <p>Select a payment link from the list or create a new one</p>
            <uui-button look="primary" @click=${this._startCreate}>
              <uui-icon name="icon-add"></uui-icon> Create Payment Link
            </uui-button>
          </div>
        </div>
      `;
    }

    const isEditing = this._mode === 'edit' || this._mode === 'create';
    const title = this._mode === 'create' ? 'New Payment Link' : this._formData.name || 'Payment Link';

    return html`
      <div class="editor-panel">
        <div class="editor-header">
          <h3>${title}</h3>
          <div class="editor-actions">
            ${isEditing ? html`
              <uui-button look="secondary" @click=${this._cancelEdit} ?disabled=${this._saving}>Cancel</uui-button>
              <uui-button look="primary" @click=${this._saveLink} ?disabled=${this._saving}>
                ${this._saving ? 'Saving...' : 'Save'}
              </uui-button>
            ` : html`
              <uui-button look="primary" @click=${this._startEdit}>
                <uui-icon name="icon-edit"></uui-icon> Edit
              </uui-button>
            `}
          </div>
        </div>

        <div class="tabs">
          <div class="tab ${this._activeTab === 'details' ? 'active' : ''}" @click=${() => this._activeTab = 'details'}>Details</div>
          <div class="tab ${this._activeTab === 'settings' ? 'active' : ''}" @click=${() => this._activeTab = 'settings'}>Settings</div>
          ${this._mode !== 'create' ? html`
            <div class="tab ${this._activeTab === 'payments' ? 'active' : ''}" @click=${() => this._activeTab = 'payments'}>Payments</div>
          ` : ''}
        </div>

        <div class="tab-content">
          ${this._activeTab === 'details' ? this._renderDetailsTab(isEditing) : ''}
          ${this._activeTab === 'settings' ? this._renderSettingsTab(isEditing) : ''}
          ${this._activeTab === 'payments' ? this._renderPaymentsTab() : ''}
        </div>
      </div>
    `;
  }

  _renderDetailsTab(isEditing) {
    const baseUrl = window.location.origin;
    const linkUrl = this._formData.code ? `${baseUrl}/pay/${this._formData.code}` : '';

    return html`
      ${this._selectedLink && !isEditing ? html`
        <div class="link-preview">
          <h5>Payment Link URL</h5>
          <div class="link-url">
            <code>${linkUrl}</code>
            <uui-button look="secondary" compact @click=${() => this._copyToClipboard(linkUrl)}>
              <uui-icon name="icon-documents"></uui-icon> Copy
            </uui-button>
            <uui-button look="secondary" compact @click=${() => window.open(linkUrl, '_blank')}>
              <uui-icon name="icon-out"></uui-icon> Open
            </uui-button>
          </div>
        </div>

        <div class="quick-actions">
          ${this._selectedLink.status === 0 ? html`
            <button class="quick-action" @click=${this._pauseLink}>
              <uui-icon name="icon-pause"></uui-icon> Pause
            </button>
          ` : html`
            <button class="quick-action primary" @click=${this._activateLink}>
              <uui-icon name="icon-play"></uui-icon> Activate
            </button>
          `}
          <button class="quick-action" @click=${this._duplicateLink}>
            <uui-icon name="icon-documents"></uui-icon> Duplicate
          </button>
          <button class="quick-action danger" @click=${this._deleteLink}>
            <uui-icon name="icon-delete"></uui-icon> Delete
          </button>
        </div>
      ` : ''}

      <div class="form-section">
        <h4>Basic Information</h4>

        <div class="form-group">
          <label>Name *</label>
          <input
            type="text"
            .value=${this._formData.name}
            @input=${(e) => this._handleInputChange('name', e.target.value)}
            ?disabled=${!isEditing}
            placeholder="e.g., Product Payment, Donation, Invoice #123"
          />
        </div>

        <div class="form-group">
          <label>Code (URL Slug)</label>
          <div class="input-with-action">
            <input
              type="text"
              .value=${this._formData.code}
              @input=${(e) => this._handleInputChange('code', e.target.value)}
              ?disabled=${!isEditing}
              placeholder="Auto-generated if empty"
            />
            ${isEditing ? html`
              <uui-button look="secondary" @click=${this._generateCode}>Generate</uui-button>
            ` : ''}
          </div>
          <small>Used in the payment URL: /pay/{code}</small>
        </div>

        <div class="form-group">
          <label>Description</label>
          <textarea
            .value=${this._formData.description}
            @input=${(e) => this._handleInputChange('description', e.target.value)}
            ?disabled=${!isEditing}
            placeholder="Describe what this payment is for"
            rows="3"
          ></textarea>
        </div>
      </div>

      <div class="form-section">
        <h4>Payment Type</h4>

        ${isEditing ? html`
          <div class="type-cards">
            ${this._types.slice(0, 3).map(type => html`
              <div
                class="type-card ${this._formData.type === type.value ? 'selected' : ''}"
                @click=${() => this._handleInputChange('type', type.value)}
              >
                <uui-icon name="${type.icon}"></uui-icon>
                <h5>${type.label}</h5>
                <p>${type.desc}</p>
              </div>
            `)}
          </div>
          <div class="type-cards">
            ${this._types.slice(3).map(type => html`
              <div
                class="type-card ${this._formData.type === type.value ? 'selected' : ''}"
                @click=${() => this._handleInputChange('type', type.value)}
              >
                <uui-icon name="${type.icon}"></uui-icon>
                <h5>${type.label}</h5>
                <p>${type.desc}</p>
              </div>
            `)}
          </div>
        ` : html`
          <p>${this._getTypeLabel(this._formData.type)}</p>
        `}
      </div>

      <div class="form-section">
        <h4>Amount</h4>

        <div class="form-row">
          <div class="form-group">
            <label>Amount ${this._formData.type === 0 ? '*' : ''}</label>
            <input
              type="number"
              step="0.01"
              min="0"
              .value=${this._formData.amount}
              @input=${(e) => this._handleInputChange('amount', e.target.value)}
              ?disabled=${!isEditing}
              placeholder="0.00"
            />
          </div>

          <div class="form-group">
            <label>Currency</label>
            <select
              .value=${this._formData.currencyCode}
              @change=${(e) => this._handleInputChange('currencyCode', e.target.value)}
              ?disabled=${!isEditing}
            >
              ${this._currencies.length === 0 ? html`<option value="USD">USD - US Dollar (Loading...)</option>` : ''}
              ${this._currencies.map(c => html`<option value="${c.code}" ?selected=${this._formData.currencyCode === c.code}>${c.code} - ${c.name}</option>`)}
            </select>
          </div>
        </div>

        ${this._formData.type === 1 || this._formData.type === 5 ? html`
          <div class="form-row">
            <div class="form-group">
              <label>Minimum Amount</label>
              <input
                type="number"
                step="0.01"
                min="0"
                .value=${this._formData.minimumAmount}
                @input=${(e) => this._handleInputChange('minimumAmount', e.target.value)}
                ?disabled=${!isEditing}
                placeholder="No minimum"
              />
            </div>

            <div class="form-group">
              <label>Maximum Amount</label>
              <input
                type="number"
                step="0.01"
                min="0"
                .value=${this._formData.maximumAmount}
                @input=${(e) => this._handleInputChange('maximumAmount', e.target.value)}
                ?disabled=${!isEditing}
                placeholder="No maximum"
              />
            </div>
          </div>
        ` : ''}

        <div class="form-group">
          <div class="toggle-group">
            <label class="toggle-switch">
              <input
                type="checkbox"
                ?checked=${this._formData.allowTip}
                @change=${(e) => this._handleInputChange('allowTip', e.target.checked)}
                ?disabled=${!isEditing}
              />
              <span class="toggle-slider"></span>
            </label>
            <span class="toggle-label">Allow customers to add a tip</span>
          </div>
        </div>
      </div>
    `;
  }

  _renderSettingsTab(isEditing) {
    return html`
      <div class="form-section">
        <h4>Validity Period</h4>

        <div class="form-row">
          <div class="form-group">
            <label>Valid From</label>
            <input
              type="date"
              .value=${this._formData.validFrom}
              @input=${(e) => this._handleInputChange('validFrom', e.target.value)}
              ?disabled=${!isEditing}
            />
            <small>Leave empty to activate immediately</small>
          </div>

          <div class="form-group">
            <label>Expires At</label>
            <input
              type="date"
              .value=${this._formData.expiresAt}
              @input=${(e) => this._handleInputChange('expiresAt', e.target.value)}
              ?disabled=${!isEditing}
            />
            <small>Leave empty for no expiration</small>
          </div>
        </div>

        <div class="form-group">
          <label>Maximum Uses</label>
          <input
            type="number"
            min="1"
            .value=${this._formData.maxUses}
            @input=${(e) => this._handleInputChange('maxUses', e.target.value)}
            ?disabled=${!isEditing}
            placeholder="Unlimited"
          />
          <small>Limit how many times this link can be used</small>
        </div>
      </div>

      <div class="form-section">
        <h4>Customer Information</h4>

        <div class="form-group">
          <div class="toggle-group">
            <label class="toggle-switch">
              <input
                type="checkbox"
                ?checked=${this._formData.requireEmail}
                @change=${(e) => this._handleInputChange('requireEmail', e.target.checked)}
                ?disabled=${!isEditing}
              />
              <span class="toggle-slider"></span>
            </label>
            <span class="toggle-label">Require email address</span>
          </div>
        </div>

        <div class="form-group">
          <div class="toggle-group">
            <label class="toggle-switch">
              <input
                type="checkbox"
                ?checked=${this._formData.requirePhone}
                @change=${(e) => this._handleInputChange('requirePhone', e.target.checked)}
                ?disabled=${!isEditing}
              />
              <span class="toggle-slider"></span>
            </label>
            <span class="toggle-label">Require phone number</span>
          </div>
        </div>

        <div class="form-group">
          <div class="toggle-group">
            <label class="toggle-switch">
              <input
                type="checkbox"
                ?checked=${this._formData.requireBillingAddress}
                @change=${(e) => this._handleInputChange('requireBillingAddress', e.target.checked)}
                ?disabled=${!isEditing}
              />
              <span class="toggle-slider"></span>
            </label>
            <span class="toggle-label">Require billing address</span>
          </div>
        </div>
      </div>

      <div class="form-section">
        <h4>After Payment</h4>

        <div class="form-group">
          <label>Success Message</label>
          <textarea
            .value=${this._formData.successMessage}
            @input=${(e) => this._handleInputChange('successMessage', e.target.value)}
            ?disabled=${!isEditing}
            placeholder="Thank you for your payment!"
            rows="2"
          ></textarea>
        </div>

        <div class="form-group">
          <label>Redirect URL</label>
          <input
            type="url"
            .value=${this._formData.successRedirectUrl}
            @input=${(e) => this._handleInputChange('successRedirectUrl', e.target.value)}
            ?disabled=${!isEditing}
            placeholder="https://yoursite.com/thank-you"
          />
          <small>Redirect customer after successful payment</small>
        </div>

        <div class="form-group">
          <div class="toggle-group">
            <label class="toggle-switch">
              <input
                type="checkbox"
                ?checked=${this._formData.sendCustomerReceipt}
                @change=${(e) => this._handleInputChange('sendCustomerReceipt', e.target.checked)}
                ?disabled=${!isEditing}
              />
              <span class="toggle-slider"></span>
            </label>
            <span class="toggle-label">Send email receipt to customer</span>
          </div>
        </div>
      </div>

      <div class="form-section">
        <h4>Notifications</h4>

        <div class="form-group">
          <label>Notification Email</label>
          <input
            type="email"
            .value=${this._formData.notificationEmail}
            @input=${(e) => this._handleInputChange('notificationEmail', e.target.value)}
            ?disabled=${!isEditing}
            placeholder="admin@yourstore.com"
          />
          <small>Receive notifications when payments are made</small>
        </div>
      </div>

      <div class="form-section">
        <h4>Internal Notes</h4>

        <div class="form-group">
          <label>Reference Number</label>
          <input
            type="text"
            .value=${this._formData.referenceNumber}
            @input=${(e) => this._handleInputChange('referenceNumber', e.target.value)}
            ?disabled=${!isEditing}
            placeholder="e.g., INV-2024-001"
          />
        </div>

        <div class="form-group">
          <label>Notes</label>
          <textarea
            .value=${this._formData.notes}
            @input=${(e) => this._handleInputChange('notes', e.target.value)}
            ?disabled=${!isEditing}
            placeholder="Internal notes (not shown to customers)"
            rows="3"
          ></textarea>
        </div>
      </div>
    `;
  }

  _renderPaymentsTab() {
    if (this._loadingPayments) {
      return html`<div class="loading"><uui-loader></uui-loader></div>`;
    }

    const stats = this._statistics || {};

    return html`
      <div class="stats-grid">
        <div class="stat-card primary">
          <div class="stat-value">${stats.totalPayments || 0}</div>
          <div class="stat-label">Total Payments</div>
        </div>
        <div class="stat-card success">
          <div class="stat-value">${this._formatCurrency(stats.totalCollected || 0, this._formData.currencyCode)}</div>
          <div class="stat-label">Total Collected</div>
        </div>
        <div class="stat-card">
          <div class="stat-value">${(stats.conversionRate || 0).toFixed(1)}%</div>
          <div class="stat-label">Success Rate</div>
        </div>
        <div class="stat-card warning">
          <div class="stat-value">${this._formatCurrency(stats.averageAmount || 0, this._formData.currencyCode)}</div>
          <div class="stat-label">Avg. Amount</div>
        </div>
      </div>

      ${this._payments.length === 0 ? html`
        <div class="form-section" style="text-align: center; padding: 40px;">
          <uui-icon name="icon-inbox" style="font-size: 48px; opacity: 0.3;"></uui-icon>
          <h4 style="margin: 16px 0 8px;">No Payments Yet</h4>
          <p style="color: #666;">Payments will appear here once customers use this link.</p>
        </div>
      ` : html`
        <table class="payment-table">
          <thead>
            <tr>
              <th>Customer</th>
              <th>Amount</th>
              <th>Status</th>
              <th>Date</th>
            </tr>
          </thead>
          <tbody>
            ${this._payments.map(payment => html`
              <tr>
                <td>
                  <strong>${payment.customerName || 'Guest'}</strong>
                  ${payment.customerEmail ? html`<div style="font-size: 11px; color: #666;">${payment.customerEmail}</div>` : ''}
                </td>
                <td>
                  <strong>${this._formatCurrency(payment.amount, payment.currencyCode)}</strong>
                  ${payment.tipAmount > 0 ? html`<div style="font-size: 11px; color: #666;">+${this._formatCurrency(payment.tipAmount)} tip</div>` : ''}
                </td>
                <td>
                  <span class="payment-status ${payment.status === 2 ? 'completed' : payment.status === 0 ? 'pending' : 'failed'}">
                    ${payment.status === 2 ? 'Completed' : payment.status === 0 ? 'Pending' : 'Failed'}
                  </span>
                </td>
                <td>${this._formatDate(payment.createdAt)}</td>
              </tr>
            `)}
          </tbody>
        </table>
      `}
    `;
  }
}

customElements.define('ecommerce-paymentlink-collection', PaymentLinkCollection);
export default PaymentLinkCollection;
