import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";

/**
 * Email Template Collection with Inline Editor
 * Umbraco Commerce-style email template management with rich editing.
 */
export class EmailTemplateCollection extends UmbElementMixin(LitElement) {
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

    /* Category Filter Chips */
    .category-filters {
      display: flex;
      gap: 6px;
      padding: 12px 20px;
      border-bottom: 1px solid #e0e0e0;
      flex-wrap: wrap;
    }

    .category-chip {
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

    .category-chip:hover { background: #f5f5f5; }
    .category-chip.active { border-color: transparent; }
    .category-chip.all.active { background: #1b264f; color: #fff; }
    .category-chip.order.active { background: #3b82f6; color: #fff; }
    .category-chip.customer.active { background: #8b5cf6; color: #fff; }
    .category-chip.cart.active { background: #f59e0b; color: #fff; }
    .category-chip.giftcard.active { background: #22c55e; color: #fff; }
    .category-chip.return.active { background: #ef4444; color: #fff; }

    /* Template List */
    .template-list {
      flex: 1;
      overflow-y: auto;
      padding: 12px;
    }

    .template-item {
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

    .template-item:hover {
      border-color: #667eea;
      box-shadow: 0 2px 8px rgba(102, 126, 234, 0.15);
    }

    .template-item.selected {
      border-color: #667eea;
      background: #f8f9ff;
    }

    .template-icon {
      width: 44px;
      height: 44px;
      border-radius: 10px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 18px;
    }

    .template-icon.order { background: linear-gradient(135deg, #3b82f6, #2563eb); color: #fff; }
    .template-icon.customer { background: linear-gradient(135deg, #8b5cf6, #7c3aed); color: #fff; }
    .template-icon.cart { background: linear-gradient(135deg, #f59e0b, #d97706); color: #fff; }
    .template-icon.giftcard { background: linear-gradient(135deg, #22c55e, #16a34a); color: #fff; }
    .template-icon.return { background: linear-gradient(135deg, #ef4444, #dc2626); color: #fff; }
    .template-icon.other { background: linear-gradient(135deg, #6b7280, #4b5563); color: #fff; }

    .template-info {
      flex: 1;
      min-width: 0;
    }

    .template-name {
      font-weight: 600;
      color: #1b264f;
      font-size: 14px;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .template-meta {
      font-size: 12px;
      color: #888;
      margin-top: 4px;
    }

    .template-status {
      text-align: right;
    }

    .status-dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      display: inline-block;
    }

    .status-dot.active { background: #22c55e; }
    .status-dot.inactive { background: #e0e0e0; }

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

    .tab:hover { color: #1b264f; }
    .tab.active { color: #667eea; border-bottom-color: #667eea; }

    /* Tab Content */
    .tab-content {
      flex: 1;
      overflow-y: auto;
      padding: 24px;
    }

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

    .form-group textarea {
      font-family: 'Monaco', 'Consolas', monospace;
      min-height: 300px;
      resize: vertical;
    }

    .form-group .hint {
      font-size: 12px;
      color: #888;
      margin-top: 4px;
    }

    /* Status Toggle */
    .status-toggle {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 12px 16px;
      background: #f9f9f9;
      border-radius: 8px;
    }

    .status-toggle-label {
      flex: 1;
    }

    .status-toggle-label strong {
      display: block;
      color: #1b264f;
    }

    .status-toggle-label span {
      font-size: 12px;
      color: #888;
    }

    /* Variable Tags */
    .variable-list {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
    }

    .variable-tag {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 8px 12px;
      background: #f5f5f5;
      border-radius: 6px;
      font-family: monospace;
      font-size: 13px;
      cursor: pointer;
      transition: all 0.2s;
      border: 1px solid transparent;
    }

    .variable-tag:hover {
      background: #e8e8ff;
      border-color: #667eea;
    }

    .variable-tag uui-icon {
      font-size: 12px;
      color: #888;
    }

    .variable-category {
      margin-bottom: 20px;
    }

    .variable-category-title {
      font-size: 12px;
      font-weight: 600;
      color: #888;
      text-transform: uppercase;
      margin-bottom: 10px;
    }

    /* Preview Panel */
    .preview-container {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 20px;
    }

    .preview-section {
      background: #fff;
      border-radius: 12px;
      border: 1px solid #e0e0e0;
      overflow: hidden;
    }

    .preview-header {
      padding: 12px 16px;
      background: #f9f9f9;
      border-bottom: 1px solid #e0e0e0;
      font-weight: 600;
      font-size: 13px;
      color: #555;
    }

    .preview-body {
      padding: 16px;
    }

    .preview-subject {
      font-size: 16px;
      font-weight: 600;
      color: #1b264f;
      margin-bottom: 16px;
      padding-bottom: 12px;
      border-bottom: 1px solid #f0f0f0;
    }

    .preview-html {
      min-height: 300px;
      max-height: 500px;
      overflow: auto;
    }

    .preview-html iframe {
      width: 100%;
      height: 400px;
      border: none;
    }

    /* Test Email */
    .test-email-section {
      background: linear-gradient(135deg, #667eea, #764ba2);
      border-radius: 12px;
      padding: 20px;
      color: #fff;
    }

    .test-email-section h4 {
      margin: 0 0 12px 0;
      font-size: 16px;
    }

    .test-email-section p {
      margin: 0 0 16px 0;
      opacity: 0.9;
      font-size: 14px;
    }

    .test-email-input {
      display: flex;
      gap: 8px;
    }

    .test-email-input input {
      flex: 1;
      padding: 10px 12px;
      border: none;
      border-radius: 8px;
      font-size: 14px;
    }

    /* Quick Actions */
    .quick-actions {
      display: flex;
      gap: 8px;
      margin-bottom: 20px;
    }

    .action-btn {
      display: flex;
      align-items: center;
      gap: 6px;
      padding: 10px 16px;
      border-radius: 8px;
      font-size: 13px;
      font-weight: 500;
      cursor: pointer;
      border: none;
      transition: all 0.2s;
    }

    .action-btn.primary {
      background: #667eea;
      color: #fff;
    }

    .action-btn.primary:hover {
      background: #5a67d8;
    }

    .action-btn.secondary {
      background: #f5f5f5;
      color: #555;
      border: 1px solid #e0e0e0;
    }

    .action-btn.secondary:hover {
      background: #e0e0e0;
    }

    .action-btn:disabled {
      opacity: 0.5;
      cursor: not-allowed;
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

    /* Category Badge */
    .category-badge {
      display: inline-flex;
      align-items: center;
      gap: 4px;
      padding: 4px 10px;
      border-radius: 4px;
      font-size: 11px;
      font-weight: 600;
      text-transform: uppercase;
    }

    .category-badge.order { background: #dbeafe; color: #2563eb; }
    .category-badge.customer { background: #ede9fe; color: #7c3aed; }
    .category-badge.cart { background: #fef3c7; color: #d97706; }
    .category-badge.giftcard { background: #d1fae5; color: #059669; }
    .category-badge.return { background: #fee2e2; color: #dc2626; }
    .category-badge.other { background: #e5e7eb; color: #4b5563; }
  `;

  static properties = {
    _templates: { state: true },
    _loading: { state: true },
    _searchTerm: { state: true },
    _categoryFilter: { state: true },
    _selectedTemplate: { state: true },
    _loadingTemplate: { state: true },
    _activeTab: { state: true },
    _saving: { state: true },
    _mode: { state: true },
    _formData: { state: true },
    _eventTypes: { state: true },
    _testEmail: { state: true },
    _sendingTest: { state: true }
  };

  constructor() {
    super();
    this._templates = [];
    this._loading = true;
    this._searchTerm = '';
    this._categoryFilter = 'all';
    this._selectedTemplate = null;
    this._loadingTemplate = false;
    this._activeTab = 'template';
    this._saving = false;
    this._mode = 'list';
    this._formData = this._getEmptyForm();
    this._eventTypes = this._getDefaultEventTypes();
    this._testEmail = '';
    this._sendingTest = false;
  }

  _getEmptyForm() {
    return {
      code: '',
      name: '',
      subject: '',
      bodyHtml: this._getDefaultTemplate(),
      eventType: 100,
      language: 'en-US',
      isActive: true
    };
  }

  _getDefaultTemplate() {
    return `<!DOCTYPE html>
<html>
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <style>
    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; line-height: 1.6; color: #333; }
    .container { max-width: 600px; margin: 0 auto; padding: 20px; }
    .header { text-align: center; padding: 20px 0; border-bottom: 1px solid #eee; }
    .content { padding: 30px 0; }
    .footer { text-align: center; padding: 20px 0; color: #888; font-size: 12px; border-top: 1px solid #eee; }
    .button { display: inline-block; padding: 12px 24px; background: #667eea; color: #fff; text-decoration: none; border-radius: 6px; }
  </style>
</head>
<body>
  <div class="container">
    <div class="header">
      <h1>{{StoreName}}</h1>
    </div>
    <div class="content">
      <p>Hello {{CustomerName}},</p>
      <p>Your email content goes here...</p>
    </div>
    <div class="footer">
      <p>&copy; {{Year}} {{StoreName}}. All rights reserved.</p>
    </div>
  </div>
</body>
</html>`;
  }

  _getDefaultEventTypes() {
    return [
      { value: 100, name: 'Order Confirmation', category: 'Order' },
      { value: 101, name: 'Order Processing', category: 'Order' },
      { value: 102, name: 'Order Shipped', category: 'Order' },
      { value: 103, name: 'Order Delivered', category: 'Order' },
      { value: 104, name: 'Order Cancelled', category: 'Order' },
      { value: 105, name: 'Order Refunded', category: 'Order' },
      { value: 200, name: 'Welcome', category: 'Customer' },
      { value: 201, name: 'Password Reset', category: 'Customer' },
      { value: 202, name: 'Review Request', category: 'Customer' },
      { value: 203, name: 'Account Updated', category: 'Customer' },
      { value: 300, name: 'Cart Abandoned (1hr)', category: 'Cart' },
      { value: 301, name: 'Cart Abandoned (24hr)', category: 'Cart' },
      { value: 302, name: 'Cart Abandoned (72hr)', category: 'Cart' },
      { value: 400, name: 'Gift Card Issued', category: 'Gift Card' },
      { value: 401, name: 'Gift Card Balance Low', category: 'Gift Card' },
      { value: 500, name: 'Return Requested', category: 'Return' },
      { value: 501, name: 'Return Approved', category: 'Return' },
      { value: 502, name: 'Return Rejected', category: 'Return' },
      { value: 503, name: 'Refund Processed', category: 'Return' }
    ];
  }

  _getVariables() {
    return {
      'General': [
        { name: 'StoreName', description: 'Store name' },
        { name: 'StoreUrl', description: 'Store website URL' },
        { name: 'Year', description: 'Current year' },
        { name: 'Date', description: 'Current date' }
      ],
      'Customer': [
        { name: 'CustomerName', description: 'Customer full name' },
        { name: 'CustomerFirstName', description: 'Customer first name' },
        { name: 'CustomerEmail', description: 'Customer email' }
      ],
      'Order': [
        { name: 'OrderNumber', description: 'Order number' },
        { name: 'OrderDate', description: 'Order date' },
        { name: 'OrderTotal', description: 'Order total amount' },
        { name: 'OrderItems', description: 'Order items list' },
        { name: 'TrackingNumber', description: 'Shipping tracking number' },
        { name: 'TrackingUrl', description: 'Tracking URL' }
      ],
      'Gift Card': [
        { name: 'GiftCardCode', description: 'Gift card code' },
        { name: 'GiftCardBalance', description: 'Current balance' },
        { name: 'GiftCardExpiry', description: 'Expiration date' }
      ],
      'Return': [
        { name: 'ReturnNumber', description: 'Return number' },
        { name: 'RefundAmount', description: 'Refund amount' }
      ]
    };
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadTemplates();
    this._loadEventTypes();
  }

  async _loadTemplates() {
    try {
      this._loading = true;
      const response = await this._authFetch('/umbraco/management/api/v1/ecommerce/emailtemplates');
      if (response.ok) {
        this._templates = await response.json();
      }
    } catch (error) {
      console.error('Error loading templates:', error);
      // Demo data
      this._templates = [
        { id: '1', code: 'order-confirmation', name: 'Order Confirmation', subject: 'Your Order #{{OrderNumber}} is Confirmed', eventType: 100, language: 'en-US', isActive: true },
        { id: '2', code: 'order-shipped', name: 'Order Shipped', subject: 'Your Order #{{OrderNumber}} Has Shipped!', eventType: 102, language: 'en-US', isActive: true },
        { id: '3', code: 'welcome', name: 'Welcome Email', subject: 'Welcome to {{StoreName}}!', eventType: 200, language: 'en-US', isActive: true },
        { id: '4', code: 'cart-abandoned-1h', name: 'Cart Abandoned (1 Hour)', subject: 'Did you forget something?', eventType: 300, language: 'en-US', isActive: false },
        { id: '5', code: 'gift-card-issued', name: 'Gift Card Issued', subject: 'You received a gift card!', eventType: 400, language: 'en-US', isActive: true }
      ];
    } finally {
      this._loading = false;
    }
  }

  async _loadEventTypes() {
    try {
      const response = await this._authFetch('/umbraco/management/api/v1/ecommerce/emailtemplates/event-types');
      if (response.ok) {
        this._eventTypes = await response.json();
      }
    } catch (error) {
      console.error('Error loading event types:', error);
    }
  }

  async _loadTemplateDetails(templateId) {
    try {
      this._loadingTemplate = true;
      const response = await this._authFetch(`/umbraco/management/api/v1/ecommerce/emailtemplates/${templateId}`);
      if (response.ok) {
        this._selectedTemplate = await response.json();
        this._formData = { ...this._selectedTemplate };
      }
    } catch (error) {
      console.error('Error loading template:', error);
      const template = this._templates.find(t => t.id === templateId);
      this._selectedTemplate = { ...template, bodyHtml: this._getDefaultTemplate() };
      this._formData = { ...this._selectedTemplate };
    } finally {
      this._loadingTemplate = false;
    }
  }

  _handleTemplateSelect(template) {
    this._mode = 'edit';
    this._activeTab = 'template';
    this._loadTemplateDetails(template.id);
  }

  _handleCategoryFilterChange(category) {
    this._categoryFilter = category;
  }

  _handleCreateNew() {
    this._mode = 'create';
    this._selectedTemplate = null;
    this._formData = this._getEmptyForm();
    this._activeTab = 'template';
  }

  _handleFormChange(field, value) {
    this._formData = { ...this._formData, [field]: value };
  }

  async _handleSave() {
    if (!this._formData.code || !this._formData.name || !this._formData.subject) {
      alert('Code, Name, and Subject are required');
      return;
    }

    this._saving = true;
    try {
      const isNew = this._mode === 'create';
      const url = isNew
        ? '/umbraco/management/api/v1/ecommerce/emailtemplates'
        : `/umbraco/management/api/v1/ecommerce/emailtemplates/${this._selectedTemplate.id}`;

      const response = await this._authFetch(url, {
        method: isNew ? 'POST' : 'PUT',
        body: JSON.stringify(this._formData)
      });

      if (!response.ok) throw new Error('Failed to save template');

      if (isNew) {
        this._mode = 'list';
        this._selectedTemplate = null;
      } else {
        this._selectedTemplate = { ...this._formData };
      }
      this._loadTemplates();
    } catch (error) {
      alert('Failed to save template: ' + error.message);
    } finally {
      this._saving = false;
    }
  }

  async _handleSendTestEmail() {
    if (!this._testEmail) {
      alert('Please enter an email address');
      return;
    }

    this._sendingTest = true;
    try {
      const response = await this._authFetch(`/umbraco/management/api/v1/ecommerce/emailtemplates/${this._selectedTemplate.id}/send-test`, {
        method: 'POST',
        body: JSON.stringify({ toEmail: this._testEmail })
      });

      if (response.ok) {
        alert('Test email sent successfully!');
      } else {
        throw new Error('Failed to send test email');
      }
    } catch (error) {
      alert('Failed to send test email: ' + error.message);
    } finally {
      this._sendingTest = false;
    }
  }

  _handleCopyVariable(varName) {
    navigator.clipboard.writeText(`{{${varName}}}`);
  }

  _getEventCategory(eventType) {
    if (eventType >= 100 && eventType < 200) return 'order';
    if (eventType >= 200 && eventType < 300) return 'customer';
    if (eventType >= 300 && eventType < 400) return 'cart';
    if (eventType >= 400 && eventType < 500) return 'giftcard';
    if (eventType >= 500 && eventType < 600) return 'return';
    return 'other';
  }

  _getEventName(eventType) {
    const event = this._eventTypes.find(e => e.value === eventType);
    return event ? event.name : 'Unknown';
  }

  _getFilteredTemplates() {
    let filtered = this._templates;

    if (this._categoryFilter !== 'all') {
      filtered = filtered.filter(t => this._getEventCategory(t.eventType) === this._categoryFilter);
    }

    if (this._searchTerm) {
      const term = this._searchTerm.toLowerCase();
      filtered = filtered.filter(t =>
        t.name?.toLowerCase().includes(term) ||
        t.code?.toLowerCase().includes(term) ||
        t.subject?.toLowerCase().includes(term)
      );
    }

    return filtered;
  }

  render() {
    return html`
      <div class="list-panel">
        ${this._renderListPanel()}
      </div>
      <div class="editor-panel">
        ${this._mode === 'list' ? this._renderEmptyState() :
          this._mode === 'create' ? this._renderEditor(true) :
          this._renderEditor(false)}
      </div>
    `;
  }

  _renderListPanel() {
    const filteredTemplates = this._getFilteredTemplates();

    return html`
      <div class="list-header">
        <h2>Email Templates</h2>
        <div class="header-actions">
          <div class="search-box">
            <uui-icon name="icon-search"></uui-icon>
            <input type="text" placeholder="Search templates..." .value=${this._searchTerm} @input=${(e) => this._searchTerm = e.target.value} />
          </div>
          <uui-button look="primary" compact @click=${this._handleCreateNew}>
            <uui-icon name="icon-add"></uui-icon>
          </uui-button>
        </div>
      </div>

      <div class="category-filters">
        <span class="category-chip all ${this._categoryFilter === 'all' ? 'active' : ''}" @click=${() => this._handleCategoryFilterChange('all')}>All</span>
        <span class="category-chip order ${this._categoryFilter === 'order' ? 'active' : ''}" @click=${() => this._handleCategoryFilterChange('order')}>Order</span>
        <span class="category-chip customer ${this._categoryFilter === 'customer' ? 'active' : ''}" @click=${() => this._handleCategoryFilterChange('customer')}>Customer</span>
        <span class="category-chip cart ${this._categoryFilter === 'cart' ? 'active' : ''}" @click=${() => this._handleCategoryFilterChange('cart')}>Cart</span>
        <span class="category-chip giftcard ${this._categoryFilter === 'giftcard' ? 'active' : ''}" @click=${() => this._handleCategoryFilterChange('giftcard')}>Gift Card</span>
        <span class="category-chip return ${this._categoryFilter === 'return' ? 'active' : ''}" @click=${() => this._handleCategoryFilterChange('return')}>Return</span>
      </div>

      <div class="template-list">
        ${this._loading ? html`<div class="loading"><uui-loader></uui-loader></div>` :
          filteredTemplates.length === 0 ? html`
            <div class="empty-state">
              <uui-icon name="icon-message"></uui-icon>
              <p>No templates found</p>
            </div>
          ` : filteredTemplates.map(template => html`
            <div class="template-item ${this._selectedTemplate?.id === template.id ? 'selected' : ''}" @click=${() => this._handleTemplateSelect(template)}>
              <div class="template-icon ${this._getEventCategory(template.eventType)}">
                <uui-icon name="icon-message"></uui-icon>
              </div>
              <div class="template-info">
                <div class="template-name">${template.name}</div>
                <div class="template-meta">${this._getEventName(template.eventType)} • ${template.language}</div>
              </div>
              <div class="template-status">
                <span class="status-dot ${template.isActive ? 'active' : 'inactive'}"></span>
              </div>
            </div>
          `)}
      </div>
    `;
  }

  _renderEmptyState() {
    return html`
      <div class="empty-state">
        <uui-icon name="icon-message"></uui-icon>
        <h3>Select an Email Template</h3>
        <p>Select a template from the list to edit, or create a new one.</p>
        <uui-button look="primary" @click=${this._handleCreateNew}>Create Template</uui-button>
      </div>
    `;
  }

  _renderEditor(isNew) {
    if (this._loadingTemplate) {
      return html`<div class="loading"><uui-loader></uui-loader></div>`;
    }

    const category = this._getEventCategory(this._formData.eventType);

    return html`
      <div class="editor-header">
        <div>
          <h2>${isNew ? 'New Email Template' : this._formData.name}</h2>
          ${!isNew ? html`<span class="category-badge ${category}">${category}</span>` : ''}
        </div>
        <div class="editor-actions">
          <uui-button look="secondary" @click=${() => { this._mode = 'list'; this._selectedTemplate = null; }} ?disabled=${this._saving}>Cancel</uui-button>
          <uui-button look="primary" @click=${this._handleSave} ?disabled=${this._saving}>
            ${this._saving ? 'Saving...' : 'Save Template'}
          </uui-button>
        </div>
      </div>

      <div class="tabs">
        <div class="tab ${this._activeTab === 'template' ? 'active' : ''}" @click=${() => this._activeTab = 'template'}>Template</div>
        <div class="tab ${this._activeTab === 'content' ? 'active' : ''}" @click=${() => this._activeTab = 'content'}>Content</div>
        <div class="tab ${this._activeTab === 'variables' ? 'active' : ''}" @click=${() => this._activeTab = 'variables'}>Variables</div>
        <div class="tab ${this._activeTab === 'preview' ? 'active' : ''}" @click=${() => this._activeTab = 'preview'}>Preview & Test</div>
      </div>

      <div class="tab-content">
        ${this._activeTab === 'template' ? this._renderTemplateTab(isNew) :
          this._activeTab === 'content' ? this._renderContentTab() :
          this._activeTab === 'variables' ? this._renderVariablesTab() :
          this._renderPreviewTab()}
      </div>
    `;
  }

  _renderTemplateTab(isNew) {
    return html`
      <div class="form-section">
        <div class="form-section-title">Template Information</div>
        <div class="form-grid">
          <div class="form-group">
            <label>Code *</label>
            <input type="text" .value=${this._formData.code} @input=${(e) => this._handleFormChange('code', e.target.value)} ?disabled=${!isNew} placeholder="e.g., order-confirmation" />
            <span class="hint">Unique identifier for this template</span>
          </div>
          <div class="form-group">
            <label>Name *</label>
            <input type="text" .value=${this._formData.name} @input=${(e) => this._handleFormChange('name', e.target.value)} placeholder="e.g., Order Confirmation" />
          </div>
        </div>
      </div>

      <div class="form-section">
        <div class="form-section-title">Event & Language</div>
        <div class="form-grid">
          <div class="form-group">
            <label>Event Type</label>
            <select .value=${this._formData.eventType} @change=${(e) => this._handleFormChange('eventType', parseInt(e.target.value))}>
              ${this._eventTypes.map(et => html`
                <option value=${et.value}>${et.category} - ${et.name}</option>
              `)}
            </select>
            <span class="hint">When this email will be triggered</span>
          </div>
          <div class="form-group">
            <label>Language</label>
            <select .value=${this._formData.language} @change=${(e) => this._handleFormChange('language', e.target.value)}>
              <option value="en-US">English (US)</option>
              <option value="en-GB">English (UK)</option>
              <option value="es-ES">Spanish</option>
              <option value="fr-FR">French</option>
              <option value="de-DE">German</option>
              <option value="it-IT">Italian</option>
              <option value="pt-BR">Portuguese (Brazil)</option>
              <option value="zh-CN">Chinese (Simplified)</option>
              <option value="ja-JP">Japanese</option>
            </select>
          </div>
        </div>
      </div>

      <div class="form-section">
        <div class="form-section-title">Status</div>
        <div class="status-toggle">
          <div class="status-toggle-label">
            <strong>Template Active</strong>
            <span>When enabled, this template will be used to send emails</span>
          </div>
          <uui-toggle ?checked=${this._formData.isActive} @change=${(e) => this._handleFormChange('isActive', e.target.checked)}></uui-toggle>
        </div>
      </div>
    `;
  }

  _renderContentTab() {
    return html`
      <div class="form-section">
        <div class="form-section-title">Email Subject</div>
        <div class="form-group">
          <label>Subject Line *</label>
          <input type="text" .value=${this._formData.subject} @input=${(e) => this._handleFormChange('subject', e.target.value)} placeholder="e.g., Your Order #{{OrderNumber}} is Confirmed" />
          <span class="hint">You can use variables like {{OrderNumber}}, {{CustomerName}}, etc.</span>
        </div>
      </div>

      <div class="form-section">
        <div class="form-section-title">Email Body (HTML)</div>
        <div class="form-group">
          <label>HTML Content</label>
          <textarea .value=${this._formData.bodyHtml} @input=${(e) => this._handleFormChange('bodyHtml', e.target.value)} placeholder="Enter HTML content..."></textarea>
          <span class="hint">Use HTML to format your email. Variables are supported: {{VariableName}}</span>
        </div>
      </div>
    `;
  }

  _renderVariablesTab() {
    const variables = this._getVariables();

    return html`
      <div class="form-section">
        <div class="form-section-title">Available Variables</div>
        <p style="color:#666;font-size:14px;margin-bottom:20px;">
          Click on a variable to copy it to your clipboard. Then paste it into your subject or body.
        </p>

        ${Object.entries(variables).map(([category, vars]) => html`
          <div class="variable-category">
            <div class="variable-category-title">${category}</div>
            <div class="variable-list">
              ${vars.map(v => html`
                <div class="variable-tag" @click=${() => this._handleCopyVariable(v.name)} title="${v.description}">
                  <uui-icon name="icon-paste"></uui-icon>
                  {{${v.name}}}
                </div>
              `)}
            </div>
          </div>
        `)}
      </div>
    `;
  }

  _renderPreviewTab() {
    return html`
      <div class="preview-container">
        <div class="preview-section">
          <div class="preview-header">Email Preview</div>
          <div class="preview-body">
            <div class="preview-subject">${this._formData.subject?.replace(/\{\{(\w+)\}\}/g, '<strong>[$1]</strong>') || 'No subject'}</div>
            <div class="preview-html">
              <iframe srcdoc="${this._formData.bodyHtml || '<p>No content</p>'}"></iframe>
            </div>
          </div>
        </div>

        <div>
          ${this._selectedTemplate?.id ? html`
            <div class="test-email-section">
              <h4>Send Test Email</h4>
              <p>Send a test email to verify how it looks in an email client.</p>
              <div class="test-email-input">
                <input type="email" placeholder="Enter email address..." .value=${this._testEmail} @input=${(e) => this._testEmail = e.target.value} />
                <uui-button look="primary" @click=${this._handleSendTestEmail} ?disabled=${this._sendingTest}>
                  ${this._sendingTest ? 'Sending...' : 'Send Test'}
                </uui-button>
              </div>
            </div>
          ` : html`
            <div class="form-section">
              <div class="form-section-title">Test Email</div>
              <p style="color:#666;font-size:14px;">Save the template first to send test emails.</p>
            </div>
          `}

          <div class="form-section" style="margin-top:20px;">
            <div class="form-section-title">Tips</div>
            <ul style="color:#666;font-size:14px;padding-left:20px;margin:0;">
              <li>Use inline CSS for best email client compatibility</li>
              <li>Keep images hosted externally</li>
              <li>Test in multiple email clients</li>
              <li>Include a plain text fallback</li>
            </ul>
          </div>
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-emailtemplate-collection', EmailTemplateCollection);
export default EmailTemplateCollection;
