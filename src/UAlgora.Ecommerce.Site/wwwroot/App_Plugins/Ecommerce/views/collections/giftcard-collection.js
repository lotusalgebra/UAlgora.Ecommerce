import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Gift Card Collection with Inline Editor
 * Umbraco Commerce-style gift card management with split-pane layout.
 */
export class GiftCardCollection extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: flex;
      height: 100%;
      background: #f5f5f5;
    }

    /* List Panel */
    .list-panel {
      width: 380px;
      min-width: 380px;
      background: #fff;
      border-right: 1px solid #e0e0e0;
      display: flex;
      flex-direction: column;
      height: 100%;
    }

    .list-header {
      padding: 20px;
      border-bottom: 1px solid #e0e0e0;
    }

    .list-header h2 {
      margin: 0 0 16px 0;
      font-size: 20px;
      font-weight: 600;
      color: #1b264f;
    }

    .header-actions {
      display: flex;
      gap: 8px;
    }

    .search-box {
      flex: 1;
      display: flex;
      align-items: center;
      background: #f5f5f5;
      border-radius: 8px;
      padding: 8px 12px;
    }

    .search-box input {
      flex: 1;
      border: none;
      background: transparent;
      outline: none;
      font-size: 14px;
    }

    /* Status Filter Chips */
    .status-filters {
      display: flex;
      gap: 6px;
      padding: 12px 20px;
      border-bottom: 1px solid #e0e0e0;
      flex-wrap: wrap;
    }

    .status-chip {
      padding: 6px 12px;
      border-radius: 20px;
      font-size: 12px;
      font-weight: 500;
      cursor: pointer;
      border: 1px solid #e0e0e0;
      background: #fff;
      color: #666;
      transition: all 0.2s;
    }

    .status-chip:hover {
      background: #f5f5f5;
    }

    .status-chip.active {
      border-color: transparent;
    }

    .status-chip.all.active { background: #1b264f; color: #fff; }
    .status-chip.active-status.active { background: #22c55e; color: #fff; }
    .status-chip.redeemed.active { background: #3b82f6; color: #fff; }
    .status-chip.expired.active { background: #ef4444; color: #fff; }
    .status-chip.disabled.active { background: #6b7280; color: #fff; }

    /* Gift Card List */
    .card-list {
      flex: 1;
      overflow-y: auto;
      padding: 12px;
    }

    .card-item {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 14px;
      margin-bottom: 8px;
      background: #fff;
      border: 1px solid #e0e0e0;
      border-radius: 10px;
      cursor: pointer;
      transition: all 0.2s;
    }

    .card-item:hover {
      border-color: #667eea;
      box-shadow: 0 2px 8px rgba(102, 126, 234, 0.15);
    }

    .card-item.selected {
      border-color: #667eea;
      background: #f8f9ff;
    }

    .card-icon {
      width: 44px;
      height: 44px;
      border-radius: 10px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 20px;
    }

    .card-icon.active { background: linear-gradient(135deg, #22c55e, #16a34a); color: #fff; }
    .card-icon.redeemed { background: linear-gradient(135deg, #3b82f6, #2563eb); color: #fff; }
    .card-icon.expired { background: linear-gradient(135deg, #ef4444, #dc2626); color: #fff; }
    .card-icon.disabled { background: linear-gradient(135deg, #6b7280, #4b5563); color: #fff; }

    .card-info {
      flex: 1;
      min-width: 0;
    }

    .card-code {
      font-family: 'Courier New', monospace;
      font-weight: 600;
      color: #1b264f;
      font-size: 14px;
    }

    .card-meta {
      font-size: 12px;
      color: #888;
      margin-top: 4px;
    }

    .card-balance {
      text-align: right;
    }

    .balance-amount {
      font-weight: 600;
      font-size: 16px;
      color: #22c55e;
    }

    .balance-label {
      font-size: 11px;
      color: #888;
    }

    /* Editor Panel */
    .editor-panel {
      flex: 1;
      display: flex;
      flex-direction: column;
      height: 100%;
      overflow: hidden;
    }

    .editor-header {
      padding: 20px 24px;
      background: #fff;
      border-bottom: 1px solid #e0e0e0;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .editor-header h2 {
      margin: 0;
      font-size: 20px;
      font-weight: 600;
      color: #1b264f;
    }

    .editor-actions {
      display: flex;
      gap: 8px;
    }

    /* Tabs */
    .tabs {
      display: flex;
      background: #fff;
      border-bottom: 1px solid #e0e0e0;
      padding: 0 24px;
    }

    .tab {
      padding: 14px 20px;
      font-size: 14px;
      font-weight: 500;
      color: #666;
      cursor: pointer;
      border-bottom: 2px solid transparent;
      transition: all 0.2s;
    }

    .tab:hover {
      color: #1b264f;
    }

    .tab.active {
      color: #667eea;
      border-bottom-color: #667eea;
    }

    /* Tab Content */
    .tab-content {
      flex: 1;
      overflow-y: auto;
      padding: 24px;
    }

    /* Info Cards */
    .info-cards {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: 16px;
      margin-bottom: 24px;
    }

    .info-card {
      background: #fff;
      border-radius: 12px;
      padding: 16px;
      border: 1px solid #e0e0e0;
    }

    .info-card-label {
      font-size: 12px;
      color: #888;
      margin-bottom: 8px;
    }

    .info-card-value {
      font-size: 24px;
      font-weight: 700;
      color: #1b264f;
    }

    .info-card-value.balance { color: #22c55e; }
    .info-card-value.initial { color: #3b82f6; }
    .info-card-value.used { color: #f59e0b; }

    /* Form Sections */
    .form-section {
      background: #fff;
      border-radius: 12px;
      padding: 20px;
      margin-bottom: 20px;
      border: 1px solid #e0e0e0;
    }

    .form-section-title {
      font-size: 14px;
      font-weight: 600;
      color: #1b264f;
      margin-bottom: 16px;
      padding-bottom: 12px;
      border-bottom: 1px solid #f0f0f0;
    }

    .form-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
    }

    .form-group {
      margin-bottom: 16px;
    }

    .form-group.full-width {
      grid-column: 1 / -1;
    }

    .form-group label {
      display: block;
      font-size: 13px;
      font-weight: 500;
      color: #555;
      margin-bottom: 6px;
    }

    .form-group input,
    .form-group select,
    .form-group textarea {
      width: 100%;
      padding: 10px 12px;
      border: 1px solid #ddd;
      border-radius: 8px;
      font-size: 14px;
      transition: border-color 0.2s;
      box-sizing: border-box;
    }

    .form-group input:focus,
    .form-group select:focus,
    .form-group textarea:focus {
      outline: none;
      border-color: #667eea;
    }

    .form-group input[readonly] {
      background: #f5f5f5;
      cursor: not-allowed;
    }

    .form-group .hint {
      font-size: 12px;
      color: #888;
      margin-top: 4px;
    }

    /* Status Badge */
    .status-badge {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 6px 12px;
      border-radius: 20px;
      font-size: 13px;
      font-weight: 500;
    }

    .status-badge.active { background: #d1fae5; color: #059669; }
    .status-badge.redeemed { background: #dbeafe; color: #2563eb; }
    .status-badge.expired { background: #fee2e2; color: #dc2626; }
    .status-badge.disabled { background: #e5e7eb; color: #4b5563; }

    /* Transaction List */
    .transaction-list {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .transaction-item {
      display: flex;
      align-items: center;
      gap: 16px;
      padding: 16px;
      background: #f9f9f9;
      border-radius: 10px;
    }

    .transaction-icon {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 16px;
    }

    .transaction-icon.issue { background: #d1fae5; color: #059669; }
    .transaction-icon.redeem { background: #fee2e2; color: #dc2626; }
    .transaction-icon.refund { background: #dbeafe; color: #2563eb; }
    .transaction-icon.adjust { background: #fef3c7; color: #d97706; }

    .transaction-info {
      flex: 1;
    }

    .transaction-type {
      font-weight: 500;
      color: #1b264f;
    }

    .transaction-meta {
      font-size: 12px;
      color: #888;
      margin-top: 2px;
    }

    .transaction-amount {
      font-weight: 600;
      font-size: 16px;
    }

    .transaction-amount.positive { color: #22c55e; }
    .transaction-amount.negative { color: #ef4444; }

    .transaction-balance {
      font-size: 12px;
      color: #888;
      text-align: right;
    }

    /* Quick Actions */
    .quick-actions {
      display: flex;
      gap: 8px;
      margin-bottom: 20px;
    }

    .quick-action-btn {
      display: flex;
      align-items: center;
      gap: 6px;
      padding: 10px 16px;
      border: 1px solid #e0e0e0;
      border-radius: 8px;
      background: #fff;
      cursor: pointer;
      font-size: 13px;
      font-weight: 500;
      color: #555;
      transition: all 0.2s;
    }

    .quick-action-btn:hover {
      border-color: #667eea;
      color: #667eea;
    }

    .quick-action-btn.danger:hover {
      border-color: #ef4444;
      color: #ef4444;
    }

    /* Empty State */
    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      height: 100%;
      color: #888;
      text-align: center;
      padding: 40px;
    }

    .empty-state uui-icon {
      font-size: 64px;
      margin-bottom: 16px;
      opacity: 0.3;
    }

    .empty-state h3 {
      margin: 0 0 8px 0;
      color: #1b264f;
    }

    .empty-state p {
      margin: 0 0 20px 0;
    }

    /* Loading */
    .loading {
      display: flex;
      align-items: center;
      justify-content: center;
      height: 100%;
    }

    /* Adjustment Modal */
    .modal-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 10000;
    }

    .modal {
      background: #fff;
      border-radius: 12px;
      width: 90%;
      max-width: 450px;
      overflow: hidden;
    }

    .modal-header {
      padding: 20px;
      border-bottom: 1px solid #e0e0e0;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .modal-header h3 {
      margin: 0;
      font-size: 18px;
      font-weight: 600;
    }

    .modal-body {
      padding: 20px;
    }

    .modal-footer {
      padding: 16px 20px;
      background: #f9f9f9;
      display: flex;
      justify-content: flex-end;
      gap: 12px;
    }

    /* Pagination */
    .pagination {
      display: flex;
      justify-content: center;
      align-items: center;
      gap: 8px;
      padding: 16px;
      border-top: 1px solid #e0e0e0;
    }

    .page-info {
      font-size: 13px;
      color: #888;
    }
  `;

  static properties = {
    _giftCards: { state: true },
    _loading: { state: true },
    _searchTerm: { state: true },
    _statusFilter: { state: true },
    _selectedCard: { state: true },
    _loadingCard: { state: true },
    _activeTab: { state: true },
    _saving: { state: true },
    _mode: { state: true },
    _formData: { state: true },
    _transactions: { state: true },
    _showAdjustModal: { state: true },
    _adjustmentData: { state: true }
  };

  constructor() {
    super();
    this._giftCards = [];
    this._loading = true;
    this._searchTerm = '';
    this._statusFilter = 'all';
    this._selectedCard = null;
    this._loadingCard = false;
    this._activeTab = 'details';
    this._saving = false;
    this._mode = 'list';
    this._formData = this._getEmptyForm();
    this._transactions = [];
    this._showAdjustModal = false;
    this._adjustmentData = { type: 'adjust', amount: 0, notes: '' };
  }

  _getEmptyForm() {
    return {
      initialValue: 50,
      currencyCode: 'USD',
      expiresAt: '',
      activationMethod: 0,
      codeFormat: 'GIFT-{0}',
      codeLength: 8,
      notes: '',
      recipientEmail: '',
      recipientName: '',
      senderName: '',
      message: ''
    };
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadGiftCards();
  }

  async _loadGiftCards() {
    try {
      this._loading = true;
      let endpoint = '/umbraco/management/api/v1/ecommerce/giftcards/active';
      if (this._statusFilter !== 'all') {
        endpoint = `/umbraco/management/api/v1/ecommerce/giftcards/by-status/${this._statusFilter}`;
      }
      const response = await fetch(endpoint, {
        credentials: 'include',
        headers: { 'Accept': 'application/json' }
      });
      if (response.ok) {
        this._giftCards = await response.json();
      }
    } catch (error) {
      console.error('Error loading gift cards:', error);
      // Demo data
      this._giftCards = [
        { id: '1', code: 'GIFT-ABC12345', initialValue: 100, balance: 75.50, status: 0, expiresAt: '2026-12-31', usageCount: 2, currencyCode: 'USD' },
        { id: '2', code: 'GIFT-XYZ98765', initialValue: 50, balance: 50, status: 0, expiresAt: '2026-06-30', usageCount: 0, currencyCode: 'USD' },
        { id: '3', code: 'GIFT-QWE45678', initialValue: 200, balance: 0, status: 1, expiresAt: '2025-12-31', usageCount: 5, currencyCode: 'USD' },
        { id: '4', code: 'GIFT-RTY11111', initialValue: 75, balance: 75, status: 2, expiresAt: '2025-01-15', usageCount: 0, currencyCode: 'USD' }
      ];
    } finally {
      this._loading = false;
    }
  }

  async _loadCardDetails(cardId) {
    try {
      this._loadingCard = true;
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/giftcards/${cardId}`, {
        credentials: 'include',
        headers: { 'Accept': 'application/json' }
      });
      if (response.ok) {
        this._selectedCard = await response.json();
        this._loadTransactions(cardId);
      }
    } catch (error) {
      console.error('Error loading card details:', error);
      this._selectedCard = this._giftCards.find(c => c.id === cardId);
      this._transactions = [
        { id: '1', type: 0, amount: 100, balanceBefore: 0, balanceAfter: 100, createdAt: '2025-01-01T10:00:00', notes: 'Initial issue' },
        { id: '2', type: 1, amount: -24.50, balanceBefore: 100, balanceAfter: 75.50, orderId: 'ORD-001', createdAt: '2025-01-15T14:30:00' }
      ];
    } finally {
      this._loadingCard = false;
    }
  }

  async _loadTransactions(cardId) {
    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/giftcards/${cardId}/transactions`, {
        credentials: 'include',
        headers: { 'Accept': 'application/json' }
      });
      if (response.ok) {
        this._transactions = await response.json();
      }
    } catch (error) {
      console.error('Error loading transactions:', error);
    }
  }

  _handleCardSelect(card) {
    this._mode = 'edit';
    this._loadCardDetails(card.id);
  }

  _handleStatusFilterChange(status) {
    this._statusFilter = status;
    this._loadGiftCards();
  }

  _handleSearchInput(e) {
    this._searchTerm = e.target.value;
  }

  _handleCreateNew() {
    this._mode = 'create';
    this._selectedCard = null;
    this._formData = this._getEmptyForm();
    this._activeTab = 'details';
  }

  _handleTabChange(tab) {
    this._activeTab = tab;
  }

  _handleFormChange(field, value) {
    this._formData = { ...this._formData, [field]: value };
  }

  async _handleSave() {
    if (this._formData.initialValue <= 0) {
      alert('Initial value must be greater than 0');
      return;
    }

    this._saving = true;
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/giftcards/generate', {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(this._formData)
      });
      if (!response.ok) throw new Error('Failed to create gift card');
      const newCard = await response.json();
      alert(`Gift card created! Code: ${newCard.code}`);
      this._mode = 'list';
      this._loadGiftCards();
    } catch (error) {
      alert('Failed to create gift card: ' + error.message);
    } finally {
      this._saving = false;
    }
  }

  _openAdjustModal() {
    this._adjustmentData = { type: 'adjust', amount: 0, notes: '' };
    this._showAdjustModal = true;
  }

  _closeAdjustModal() {
    this._showAdjustModal = false;
  }

  async _handleAdjustBalance() {
    if (this._adjustmentData.amount === 0) {
      alert('Amount cannot be zero');
      return;
    }

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/giftcards/${this._selectedCard.id}/adjust`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(this._adjustmentData)
      });
      if (!response.ok) throw new Error('Failed to adjust balance');
      this._closeAdjustModal();
      this._loadCardDetails(this._selectedCard.id);
      this._loadGiftCards();
    } catch (error) {
      alert('Failed to adjust balance: ' + error.message);
    }
  }

  async _handleDisableCard() {
    if (!confirm('Are you sure you want to disable this gift card?')) return;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/giftcards/${this._selectedCard.id}/disable`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' }
      });
      if (!response.ok) throw new Error('Failed to disable card');
      this._loadCardDetails(this._selectedCard.id);
      this._loadGiftCards();
    } catch (error) {
      alert('Failed to disable card: ' + error.message);
    }
  }

  _getFilteredCards() {
    if (!this._searchTerm) return this._giftCards;
    const term = this._searchTerm.toLowerCase();
    return this._giftCards.filter(c =>
      c.code?.toLowerCase().includes(term) ||
      c.recipientEmail?.toLowerCase().includes(term)
    );
  }

  _formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount || 0);
  }

  _getStatusClass(status) {
    switch (status) {
      case 0: return 'active';
      case 1: return 'redeemed';
      case 2: return 'expired';
      default: return 'disabled';
    }
  }

  _getStatusText(status) {
    switch (status) {
      case 0: return 'Active';
      case 1: return 'Redeemed';
      case 2: return 'Expired';
      default: return 'Disabled';
    }
  }

  _getTransactionIcon(type) {
    switch (type) {
      case 0: return 'issue';
      case 1: return 'redeem';
      case 2: return 'refund';
      default: return 'adjust';
    }
  }

  _getTransactionType(type) {
    switch (type) {
      case 0: return 'Issued';
      case 1: return 'Redeemed';
      case 2: return 'Refunded';
      default: return 'Adjusted';
    }
  }

  _getActivationText(method) {
    switch (method) {
      case 0: return 'Manual';
      case 1: return 'Automatic';
      case 2: return 'On Order Status';
      default: return 'Unknown';
    }
  }

  render() {
    return html`
      <div class="list-panel">
        ${this._renderListPanel()}
      </div>
      <div class="editor-panel">
        ${this._mode === 'list' ? this._renderEmptyState() :
          this._mode === 'create' ? this._renderCreateForm() :
          this._renderEditor()}
      </div>
      ${this._showAdjustModal ? this._renderAdjustModal() : ''}
    `;
  }

  _renderListPanel() {
    const filteredCards = this._getFilteredCards();

    return html`
      <div class="list-header">
        <h2>Gift Cards</h2>
        <div class="header-actions">
          <div class="search-box">
            <uui-icon name="icon-search"></uui-icon>
            <input type="text" placeholder="Search by code..." .value=${this._searchTerm} @input=${this._handleSearchInput} />
          </div>
          <uui-button look="primary" compact @click=${this._handleCreateNew}>
            <uui-icon name="icon-add"></uui-icon>
          </uui-button>
        </div>
      </div>

      <div class="status-filters">
        <span class="status-chip all ${this._statusFilter === 'all' ? 'active' : ''}" @click=${() => this._handleStatusFilterChange('all')}>All</span>
        <span class="status-chip active-status ${this._statusFilter === '0' ? 'active' : ''}" @click=${() => this._handleStatusFilterChange('0')}>Active</span>
        <span class="status-chip redeemed ${this._statusFilter === '1' ? 'active' : ''}" @click=${() => this._handleStatusFilterChange('1')}>Redeemed</span>
        <span class="status-chip expired ${this._statusFilter === '2' ? 'active' : ''}" @click=${() => this._handleStatusFilterChange('2')}>Expired</span>
        <span class="status-chip disabled ${this._statusFilter === '3' ? 'active' : ''}" @click=${() => this._handleStatusFilterChange('3')}>Disabled</span>
      </div>

      <div class="card-list">
        ${this._loading ? html`<div class="loading"><uui-loader></uui-loader></div>` :
          filteredCards.length === 0 ? html`
            <div class="empty-state">
              <uui-icon name="icon-gift"></uui-icon>
              <p>No gift cards found</p>
            </div>
          ` : filteredCards.map(card => html`
            <div class="card-item ${this._selectedCard?.id === card.id ? 'selected' : ''}" @click=${() => this._handleCardSelect(card)}>
              <div class="card-icon ${this._getStatusClass(card.status)}">
                <uui-icon name="icon-gift"></uui-icon>
              </div>
              <div class="card-info">
                <div class="card-code">${card.code}</div>
                <div class="card-meta">${this._getStatusText(card.status)} • ${card.usageCount || 0} uses</div>
              </div>
              <div class="card-balance">
                <div class="balance-amount">${this._formatCurrency(card.balance)}</div>
                <div class="balance-label">of ${this._formatCurrency(card.initialValue)}</div>
              </div>
            </div>
          `)}
      </div>
    `;
  }

  _renderEmptyState() {
    return html`
      <div class="empty-state">
        <uui-icon name="icon-gift"></uui-icon>
        <h3>Select a Gift Card</h3>
        <p>Select a gift card from the list to view details, or create a new one.</p>
        <uui-button look="primary" @click=${this._handleCreateNew}>Generate Gift Card</uui-button>
      </div>
    `;
  }

  _renderCreateForm() {
    return html`
      <div class="editor-header">
        <h2>Generate Gift Card</h2>
        <div class="editor-actions">
          <uui-button look="secondary" @click=${() => this._mode = 'list'} ?disabled=${this._saving}>Cancel</uui-button>
          <uui-button look="primary" @click=${this._handleSave} ?disabled=${this._saving}>
            ${this._saving ? 'Generating...' : 'Generate'}
          </uui-button>
        </div>
      </div>

      <div class="tabs">
        <div class="tab ${this._activeTab === 'details' ? 'active' : ''}" @click=${() => this._handleTabChange('details')}>Card Details</div>
        <div class="tab ${this._activeTab === 'recipient' ? 'active' : ''}" @click=${() => this._handleTabChange('recipient')}>Recipient</div>
        <div class="tab ${this._activeTab === 'settings' ? 'active' : ''}" @click=${() => this._handleTabChange('settings')}>Settings</div>
      </div>

      <div class="tab-content">
        ${this._activeTab === 'details' ? this._renderDetailsTab() :
          this._activeTab === 'recipient' ? this._renderRecipientTab() :
          this._renderSettingsTab()}
      </div>
    `;
  }

  _renderDetailsTab() {
    return html`
      <div class="form-section">
        <div class="form-section-title">Value & Currency</div>
        <div class="form-grid">
          <div class="form-group">
            <label>Initial Value *</label>
            <input type="number" min="1" step="0.01" .value=${this._formData.initialValue} @input=${(e) => this._handleFormChange('initialValue', parseFloat(e.target.value))} />
          </div>
          <div class="form-group">
            <label>Currency</label>
            <select .value=${this._formData.currencyCode} @change=${(e) => this._handleFormChange('currencyCode', e.target.value)}>
              <option value="USD">USD - US Dollar</option>
              <option value="EUR">EUR - Euro</option>
              <option value="GBP">GBP - British Pound</option>
              <option value="INR">INR - Indian Rupee</option>
            </select>
          </div>
        </div>
      </div>

      <div class="form-section">
        <div class="form-section-title">Expiration</div>
        <div class="form-group">
          <label>Expires At (optional)</label>
          <input type="date" .value=${this._formData.expiresAt} @input=${(e) => this._handleFormChange('expiresAt', e.target.value)} />
          <span class="hint">Leave blank for no expiration</span>
        </div>
      </div>

      <div class="form-section">
        <div class="form-section-title">Notes</div>
        <div class="form-group">
          <label>Internal Notes</label>
          <textarea rows="3" .value=${this._formData.notes} @input=${(e) => this._handleFormChange('notes', e.target.value)} placeholder="Add any internal notes..."></textarea>
        </div>
      </div>
    `;
  }

  _renderRecipientTab() {
    return html`
      <div class="form-section">
        <div class="form-section-title">Recipient Information</div>
        <div class="form-grid">
          <div class="form-group">
            <label>Recipient Name</label>
            <input type="text" .value=${this._formData.recipientName} @input=${(e) => this._handleFormChange('recipientName', e.target.value)} placeholder="John Doe" />
          </div>
          <div class="form-group">
            <label>Recipient Email</label>
            <input type="email" .value=${this._formData.recipientEmail} @input=${(e) => this._handleFormChange('recipientEmail', e.target.value)} placeholder="john@example.com" />
          </div>
        </div>
      </div>

      <div class="form-section">
        <div class="form-section-title">Sender & Message</div>
        <div class="form-group">
          <label>Sender Name</label>
          <input type="text" .value=${this._formData.senderName} @input=${(e) => this._handleFormChange('senderName', e.target.value)} placeholder="Jane Doe" />
        </div>
        <div class="form-group">
          <label>Gift Message</label>
          <textarea rows="4" .value=${this._formData.message} @input=${(e) => this._handleFormChange('message', e.target.value)} placeholder="Happy Birthday! Enjoy your gift..."></textarea>
        </div>
      </div>
    `;
  }

  _renderSettingsTab() {
    return html`
      <div class="form-section">
        <div class="form-section-title">Activation Method</div>
        <div class="form-group">
          <label>When should this card become active?</label>
          <select .value=${this._formData.activationMethod} @change=${(e) => this._handleFormChange('activationMethod', parseInt(e.target.value))}>
            <option value="0">Manual - Requires manual activation</option>
            <option value="1">Automatic - Active immediately after creation</option>
            <option value="2">On Order Status - Active when order is completed</option>
          </select>
          <span class="hint">Automatic activation is recommended for most use cases</span>
        </div>
      </div>

      <div class="form-section">
        <div class="form-section-title">Code Generation</div>
        <div class="form-grid">
          <div class="form-group">
            <label>Code Format</label>
            <input type="text" .value=${this._formData.codeFormat} @input=${(e) => this._handleFormChange('codeFormat', e.target.value)} />
            <span class="hint">Use {0} as placeholder for random characters</span>
          </div>
          <div class="form-group">
            <label>Random Characters Length</label>
            <input type="number" min="4" max="16" .value=${this._formData.codeLength} @input=${(e) => this._handleFormChange('codeLength', parseInt(e.target.value))} />
            <span class="hint">4-16 characters recommended</span>
          </div>
        </div>
      </div>
    `;
  }

  _renderEditor() {
    if (this._loadingCard) {
      return html`<div class="loading"><uui-loader></uui-loader></div>`;
    }

    if (!this._selectedCard) {
      return this._renderEmptyState();
    }

    const card = this._selectedCard;
    const usedAmount = card.initialValue - card.balance;

    return html`
      <div class="editor-header">
        <div>
          <h2 style="font-family: monospace;">${card.code}</h2>
          <span class="status-badge ${this._getStatusClass(card.status)}">${this._getStatusText(card.status)}</span>
        </div>
        <div class="editor-actions">
          <uui-button look="secondary" @click=${() => { this._mode = 'list'; this._selectedCard = null; }}>Close</uui-button>
        </div>
      </div>

      <div class="tabs">
        <div class="tab ${this._activeTab === 'details' ? 'active' : ''}" @click=${() => this._handleTabChange('details')}>Details</div>
        <div class="tab ${this._activeTab === 'transactions' ? 'active' : ''}" @click=${() => this._handleTabChange('transactions')}>Transactions</div>
        <div class="tab ${this._activeTab === 'actions' ? 'active' : ''}" @click=${() => this._handleTabChange('actions')}>Actions</div>
      </div>

      <div class="tab-content">
        ${this._activeTab === 'details' ? html`
          <div class="info-cards">
            <div class="info-card">
              <div class="info-card-label">Current Balance</div>
              <div class="info-card-value balance">${this._formatCurrency(card.balance)}</div>
            </div>
            <div class="info-card">
              <div class="info-card-label">Initial Value</div>
              <div class="info-card-value initial">${this._formatCurrency(card.initialValue)}</div>
            </div>
            <div class="info-card">
              <div class="info-card-label">Used Amount</div>
              <div class="info-card-value used">${this._formatCurrency(usedAmount)}</div>
            </div>
            <div class="info-card">
              <div class="info-card-label">Usage Count</div>
              <div class="info-card-value">${card.usageCount || 0}</div>
            </div>
          </div>

          <div class="form-section">
            <div class="form-section-title">Card Information</div>
            <div class="form-grid">
              <div class="form-group">
                <label>Code</label>
                <input type="text" readonly .value=${card.code} />
              </div>
              <div class="form-group">
                <label>Status</label>
                <input type="text" readonly .value=${this._getStatusText(card.status)} />
              </div>
              <div class="form-group">
                <label>Currency</label>
                <input type="text" readonly .value=${card.currencyCode || 'USD'} />
              </div>
              <div class="form-group">
                <label>Expires At</label>
                <input type="text" readonly .value=${card.expiresAt ? new Date(card.expiresAt).toLocaleDateString() : 'Never'} />
              </div>
              <div class="form-group">
                <label>Created At</label>
                <input type="text" readonly .value=${card.createdAt ? new Date(card.createdAt).toLocaleString() : '-'} />
              </div>
              <div class="form-group">
                <label>Last Used</label>
                <input type="text" readonly .value=${card.lastUsedAt ? new Date(card.lastUsedAt).toLocaleString() : 'Never'} />
              </div>
            </div>
          </div>

          ${card.notes ? html`
            <div class="form-section">
              <div class="form-section-title">Notes</div>
              <p>${card.notes}</p>
            </div>
          ` : ''}
        ` : this._activeTab === 'transactions' ? html`
          <div class="form-section">
            <div class="form-section-title">Transaction History</div>
            ${this._transactions.length === 0 ? html`
              <div class="empty-state">
                <uui-icon name="icon-list"></uui-icon>
                <p>No transactions yet</p>
              </div>
            ` : html`
              <div class="transaction-list">
                ${this._transactions.map(tx => html`
                  <div class="transaction-item">
                    <div class="transaction-icon ${this._getTransactionIcon(tx.type)}">
                      <uui-icon name="${tx.type === 0 ? 'icon-add' : tx.type === 1 ? 'icon-arrow-right' : 'icon-undo'}"></uui-icon>
                    </div>
                    <div class="transaction-info">
                      <div class="transaction-type">${this._getTransactionType(tx.type)}</div>
                      <div class="transaction-meta">
                        ${tx.createdAt ? new Date(tx.createdAt).toLocaleString() : '-'}
                        ${tx.orderId ? ` • Order ${tx.orderId}` : ''}
                      </div>
                    </div>
                    <div>
                      <div class="transaction-amount ${tx.amount >= 0 ? 'positive' : 'negative'}">
                        ${tx.amount >= 0 ? '+' : ''}${this._formatCurrency(tx.amount)}
                      </div>
                      <div class="transaction-balance">Balance: ${this._formatCurrency(tx.balanceAfter)}</div>
                    </div>
                  </div>
                `)}
              </div>
            `}
          </div>
        ` : html`
          <div class="quick-actions">
            <button class="quick-action-btn" @click=${this._openAdjustModal}>
              <uui-icon name="icon-edit"></uui-icon> Adjust Balance
            </button>
            ${card.status === 0 ? html`
              <button class="quick-action-btn danger" @click=${this._handleDisableCard}>
                <uui-icon name="icon-block"></uui-icon> Disable Card
              </button>
            ` : ''}
          </div>

          <div class="form-section">
            <div class="form-section-title">Available Actions</div>
            <p style="color: #666; font-size: 14px;">
              Use the buttons above to manage this gift card. Balance adjustments will be logged in the transaction history.
            </p>
          </div>
        `}
      </div>
    `;
  }

  _renderAdjustModal() {
    return html`
      <div class="modal-overlay" @click=${(e) => e.target === e.currentTarget && this._closeAdjustModal()}>
        <div class="modal">
          <div class="modal-header">
            <h3>Adjust Balance</h3>
            <uui-button look="secondary" compact @click=${this._closeAdjustModal}>&times;</uui-button>
          </div>
          <div class="modal-body">
            <div class="form-group">
              <label>Adjustment Type</label>
              <select .value=${this._adjustmentData.type} @change=${(e) => this._adjustmentData = { ...this._adjustmentData, type: e.target.value }}>
                <option value="add">Add Balance</option>
                <option value="subtract">Subtract Balance</option>
              </select>
            </div>
            <div class="form-group">
              <label>Amount</label>
              <input type="number" min="0.01" step="0.01" .value=${this._adjustmentData.amount} @input=${(e) => this._adjustmentData = { ...this._adjustmentData, amount: parseFloat(e.target.value) }} />
            </div>
            <div class="form-group">
              <label>Notes</label>
              <textarea rows="2" .value=${this._adjustmentData.notes} @input=${(e) => this._adjustmentData = { ...this._adjustmentData, notes: e.target.value }} placeholder="Reason for adjustment..."></textarea>
            </div>
          </div>
          <div class="modal-footer">
            <uui-button look="secondary" @click=${this._closeAdjustModal}>Cancel</uui-button>
            <uui-button look="primary" @click=${this._handleAdjustBalance}>Apply Adjustment</uui-button>
          </div>
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-giftcard-collection', GiftCardCollection);
export default GiftCardCollection;
