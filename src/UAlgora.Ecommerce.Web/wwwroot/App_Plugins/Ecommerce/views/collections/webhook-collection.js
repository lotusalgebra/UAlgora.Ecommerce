import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";

export class WebhookCollection extends UmbElementMixin(LitElement) {
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

    /* Status Filter Chips */
    .filter-chips { display: flex; gap: 8px; padding: 12px 16px; border-bottom: 1px solid #e0e0e0; flex-wrap: wrap; background: #fff; }
    .filter-chip { padding: 4px 12px; border-radius: 16px; font-size: 12px; cursor: pointer; border: 1px solid #ddd; background: #fff; transition: all 0.2s; }
    .filter-chip:hover { background: #f0f0f0; }
    .filter-chip.active { background: #1b264f; color: white; border-color: #1b264f; }
    .filter-chip .count { margin-left: 4px; opacity: 0.7; }

    /* Webhook List */
    .webhook-list { flex: 1; overflow-y: auto; }
    .webhook-item { padding: 12px 16px; border-bottom: 1px solid #e9e9e9; cursor: pointer; transition: background 0.2s; }
    .webhook-item:hover { background: #f5f5f5; }
    .webhook-item.selected { background: #e3f2fd; border-left: 3px solid #1976d2; }
    .webhook-item-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 4px; }
    .webhook-name { font-weight: 600; font-size: 14px; }
    .webhook-url { font-family: monospace; font-size: 11px; color: #666; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; margin-bottom: 6px; }
    .webhook-meta { display: flex; gap: 8px; align-items: center; flex-wrap: wrap; }
    .status-dot { width: 8px; height: 8px; border-radius: 50%; display: inline-block; }
    .status-dot.active { background: #4caf50; }
    .status-dot.inactive { background: #9e9e9e; }
    .event-count { font-size: 11px; color: #666; background: #f0f0f0; padding: 2px 6px; border-radius: 3px; }
    .last-triggered { font-size: 11px; color: #999; }

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
    .form-row.single { grid-template-columns: 1fr; }
    .form-group { margin-bottom: 16px; }
    .form-group:last-child { margin-bottom: 0; }
    .form-group label { display: block; margin-bottom: 6px; font-weight: 500; color: #333; font-size: 13px; }
    .form-group input, .form-group select, .form-group textarea { width: 100%; padding: 10px 12px; border: 1px solid #ddd; border-radius: 4px; font-size: 14px; box-sizing: border-box; }
    .form-group input:focus, .form-group select:focus, .form-group textarea:focus { outline: none; border-color: #1976d2; box-shadow: 0 0 0 2px rgba(25, 118, 210, 0.1); }
    .form-group textarea { resize: vertical; min-height: 80px; font-family: monospace; }
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

    /* Event Categories */
    .event-categories { display: flex; flex-direction: column; gap: 16px; }
    .event-category { background: #fff; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden; }
    .event-category-header { padding: 12px 16px; background: #f8f9fa; border-bottom: 1px solid #e0e0e0; display: flex; justify-content: space-between; align-items: center; cursor: pointer; }
    .event-category-header h5 { margin: 0; font-size: 14px; display: flex; align-items: center; gap: 8px; }
    .event-category-header .category-icon { width: 24px; height: 24px; border-radius: 4px; display: flex; align-items: center; justify-content: center; font-size: 12px; }
    .event-category-header .category-icon.order { background: #e3f2fd; color: #1976d2; }
    .event-category-header .category-icon.product { background: #e8f5e9; color: #388e3c; }
    .event-category-header .category-icon.customer { background: #fff3e0; color: #f57c00; }
    .event-category-header .category-icon.return { background: #fce4ec; color: #c2185b; }
    .event-category-header .category-icon.giftcard { background: #f3e5f5; color: #7b1fa2; }
    .event-category-header .category-icon.inventory { background: #e0f2f1; color: #00796b; }
    .select-all-btn { font-size: 12px; color: #1976d2; cursor: pointer; }
    .select-all-btn:hover { text-decoration: underline; }
    .event-list { padding: 12px 16px; display: grid; grid-template-columns: repeat(2, 1fr); gap: 8px; }
    .event-checkbox { display: flex; align-items: center; gap: 8px; padding: 6px 8px; border-radius: 4px; cursor: pointer; transition: background 0.2s; }
    .event-checkbox:hover { background: #f5f5f5; }
    .event-checkbox input { margin: 0; }
    .event-checkbox label { font-size: 13px; cursor: pointer; flex: 1; }
    .event-checkbox .event-desc { font-size: 11px; color: #666; }

    /* Security Section */
    .secret-display { font-family: monospace; background: #f5f5f5; padding: 12px; border-radius: 4px; word-break: break-all; position: relative; }
    .secret-display.hidden { color: transparent; text-shadow: 0 0 8px rgba(0,0,0,0.5); }
    .secret-actions { display: flex; gap: 8px; margin-top: 8px; }
    .signature-example { background: #f8f9fa; border: 1px solid #e0e0e0; border-radius: 4px; padding: 12px; margin-top: 12px; }
    .signature-example h5 { margin: 0 0 8px 0; font-size: 13px; }
    .signature-example pre { margin: 0; font-size: 12px; overflow-x: auto; white-space: pre-wrap; word-break: break-all; }

    /* Delivery Logs */
    .delivery-stats { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; margin-bottom: 20px; }
    .stat-card { background: #fff; border: 1px solid #e0e0e0; border-radius: 8px; padding: 16px; text-align: center; }
    .stat-card .stat-value { font-size: 28px; font-weight: 700; margin-bottom: 4px; }
    .stat-card .stat-label { font-size: 12px; color: #666; }
    .stat-card.success .stat-value { color: #4caf50; }
    .stat-card.failed .stat-value { color: #f44336; }
    .stat-card.pending .stat-value { color: #ff9800; }

    .delivery-table { width: 100%; border-collapse: collapse; background: #fff; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden; }
    .delivery-table th, .delivery-table td { padding: 12px 16px; text-align: left; border-bottom: 1px solid #e0e0e0; }
    .delivery-table th { background: #f8f9fa; font-weight: 600; font-size: 13px; }
    .delivery-table tr:last-child td { border-bottom: none; }
    .delivery-table tr:hover { background: #f9f9f9; }
    .status-badge { padding: 4px 8px; border-radius: 4px; font-size: 11px; font-weight: 600; }
    .status-badge.success { background: #e8f5e9; color: #2e7d32; }
    .status-badge.failed { background: #ffebee; color: #c62828; }
    .status-badge.pending { background: #fff3e0; color: #ef6c00; }
    .status-badge.retrying { background: #e3f2fd; color: #1565c0; }
    .response-code { font-family: monospace; font-size: 12px; }
    .response-code.success { color: #4caf50; }
    .response-code.error { color: #f44336; }

    /* Test Panel */
    .test-panel { background: #fff; border: 1px solid #e0e0e0; border-radius: 8px; padding: 20px; }
    .test-panel h4 { margin: 0 0 16px 0; }
    .payload-preview { background: #1e1e1e; color: #d4d4d4; padding: 16px; border-radius: 4px; font-family: monospace; font-size: 12px; max-height: 300px; overflow: auto; white-space: pre-wrap; }
    .test-result { margin-top: 16px; padding: 16px; border-radius: 4px; }
    .test-result.success { background: #e8f5e9; border: 1px solid #4caf50; }
    .test-result.error { background: #ffebee; border: 1px solid #f44336; }
    .test-result h5 { margin: 0 0 8px 0; }
    .test-result pre { margin: 0; font-size: 12px; overflow-x: auto; }

    /* Empty States */
    .empty-state { display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100%; color: #666; text-align: center; padding: 40px; }
    .empty-state uui-icon { font-size: 64px; margin-bottom: 16px; opacity: 0.3; }
    .empty-state h3 { margin: 0 0 8px 0; }
    .empty-state p { margin: 0 0 20px 0; }

    .loading { display: flex; justify-content: center; align-items: center; height: 200px; }

    /* Retry Settings */
    .retry-settings { display: grid; grid-template-columns: repeat(3, 1fr); gap: 16px; }
    .retry-preview { background: #f8f9fa; border: 1px solid #e0e0e0; border-radius: 4px; padding: 12px; margin-top: 12px; }
    .retry-preview h5 { margin: 0 0 8px 0; font-size: 13px; }
    .retry-timeline { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
    .retry-attempt { padding: 4px 8px; background: #e3f2fd; border-radius: 4px; font-size: 11px; }
  `;

  static properties = {
    _webhooks: { state: true },
    _loading: { state: true },
    _searchTerm: { state: true },
    _statusFilter: { state: true },
    _selectedWebhook: { state: true },
    _activeTab: { state: true },
    _mode: { state: true },
    _formData: { state: true },
    _saving: { state: true },
    _showSecret: { state: true },
    _deliveries: { state: true },
    _loadingDeliveries: { state: true },
    _testResult: { state: true },
    _testing: { state: true }
  };

  constructor() {
    super();
    this._webhooks = [];
    this._loading = true;
    this._searchTerm = '';
    this._statusFilter = 'all';
    this._selectedWebhook = null;
    this._activeTab = 'webhook';
    this._mode = 'view';
    this._formData = this._getEmptyFormData();
    this._saving = false;
    this._showSecret = false;
    this._deliveries = [];
    this._loadingDeliveries = false;
    this._testResult = null;
    this._testing = false;

    this._eventCategories = [
      {
        id: 'order',
        name: 'Order Events',
        icon: 'icon-receipt-dollar',
        events: [
          { code: 'order.created', name: 'Order Created', desc: 'When a new order is placed' },
          { code: 'order.updated', name: 'Order Updated', desc: 'When order details change' },
          { code: 'order.paid', name: 'Order Paid', desc: 'When payment is confirmed' },
          { code: 'order.processing', name: 'Order Processing', desc: 'When order starts processing' },
          { code: 'order.shipped', name: 'Order Shipped', desc: 'When order is shipped' },
          { code: 'order.delivered', name: 'Order Delivered', desc: 'When order is delivered' },
          { code: 'order.cancelled', name: 'Order Cancelled', desc: 'When order is cancelled' },
          { code: 'order.refunded', name: 'Order Refunded', desc: 'When order is refunded' }
        ]
      },
      {
        id: 'product',
        name: 'Product Events',
        icon: 'icon-box',
        events: [
          { code: 'product.created', name: 'Product Created', desc: 'When a new product is added' },
          { code: 'product.updated', name: 'Product Updated', desc: 'When product details change' },
          { code: 'product.deleted', name: 'Product Deleted', desc: 'When a product is removed' },
          { code: 'product.published', name: 'Product Published', desc: 'When product goes live' },
          { code: 'product.unpublished', name: 'Product Unpublished', desc: 'When product is hidden' }
        ]
      },
      {
        id: 'inventory',
        name: 'Inventory Events',
        icon: 'icon-server',
        events: [
          { code: 'inventory.low_stock', name: 'Low Stock Alert', desc: 'When stock falls below threshold' },
          { code: 'inventory.out_of_stock', name: 'Out of Stock', desc: 'When product has no stock' },
          { code: 'inventory.restocked', name: 'Restocked', desc: 'When stock is replenished' },
          { code: 'inventory.adjusted', name: 'Stock Adjusted', desc: 'When stock is manually changed' }
        ]
      },
      {
        id: 'customer',
        name: 'Customer Events',
        icon: 'icon-user',
        events: [
          { code: 'customer.created', name: 'Customer Created', desc: 'When a new customer registers' },
          { code: 'customer.updated', name: 'Customer Updated', desc: 'When customer info changes' },
          { code: 'customer.deleted', name: 'Customer Deleted', desc: 'When customer is removed' }
        ]
      },
      {
        id: 'return',
        name: 'Return Events',
        icon: 'icon-undo',
        events: [
          { code: 'return.requested', name: 'Return Requested', desc: 'When customer requests return' },
          { code: 'return.approved', name: 'Return Approved', desc: 'When return is approved' },
          { code: 'return.rejected', name: 'Return Rejected', desc: 'When return is denied' },
          { code: 'return.received', name: 'Return Received', desc: 'When items are received back' },
          { code: 'return.refunded', name: 'Return Refunded', desc: 'When refund is processed' }
        ]
      },
      {
        id: 'giftcard',
        name: 'Gift Card Events',
        icon: 'icon-gift',
        events: [
          { code: 'giftcard.issued', name: 'Gift Card Issued', desc: 'When a gift card is created' },
          { code: 'giftcard.redeemed', name: 'Gift Card Redeemed', desc: 'When gift card is used' },
          { code: 'giftcard.balance_low', name: 'Balance Low', desc: 'When balance falls below threshold' },
          { code: 'giftcard.expired', name: 'Gift Card Expired', desc: 'When gift card expires' }
        ]
      }
    ];
  }

  _getEmptyFormData() {
    return {
      name: '',
      url: '',
      secret: '',
      events: [],
      isActive: true,
      retryCount: 3,
      retryIntervalSeconds: 60,
      timeoutSeconds: 30,
      headers: ''
    };
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadWebhooks();
  }

  async _loadWebhooks() {
    try {
      this._loading = true;
      const response = await this._authFetch('/umbraco/management/api/v1/ecommerce/webhooks');
      if (response.ok) {
        this._webhooks = await response.json();
      }
    } catch (error) {
      console.error('Error loading webhooks:', error);
    } finally {
      this._loading = false;
    }
  }

  async _loadDeliveries(webhookId) {
    try {
      this._loadingDeliveries = true;
      const response = await this._authFetch(`/umbraco/management/api/v1/ecommerce/webhooks/${webhookId}/deliveries`);
      if (response.ok) {
        this._deliveries = await response.json();
      } else {
        this._deliveries = [];
      }
    } catch (error) {
      console.error('Error loading deliveries:', error);
      this._deliveries = [];
    } finally {
      this._loadingDeliveries = false;
    }
  }

  _getFilteredWebhooks() {
    let filtered = [...this._webhooks];

    if (this._searchTerm) {
      const term = this._searchTerm.toLowerCase();
      filtered = filtered.filter(w =>
        w.name?.toLowerCase().includes(term) ||
        w.url?.toLowerCase().includes(term)
      );
    }

    if (this._statusFilter !== 'all') {
      const isActive = this._statusFilter === 'active';
      filtered = filtered.filter(w => w.isActive === isActive);
    }

    return filtered;
  }

  _getStatusCounts() {
    return {
      all: this._webhooks.length,
      active: this._webhooks.filter(w => w.isActive).length,
      inactive: this._webhooks.filter(w => !w.isActive).length
    };
  }

  _selectWebhook(webhook) {
    this._selectedWebhook = webhook;
    this._mode = 'view';
    this._activeTab = 'webhook';
    this._formData = this._webhookToFormData(webhook);
    this._testResult = null;
    this._loadDeliveries(webhook.id);
  }

  _webhookToFormData(webhook) {
    let events = [];
    try {
      events = webhook.events ? JSON.parse(webhook.events) : [];
    } catch {
      events = [];
    }

    return {
      name: webhook.name || '',
      url: webhook.url || '',
      secret: webhook.secret || '',
      events: events,
      isActive: webhook.isActive ?? true,
      retryCount: webhook.retryCount ?? 3,
      retryIntervalSeconds: webhook.retryIntervalSeconds ?? 60,
      timeoutSeconds: webhook.timeoutSeconds ?? 30,
      headers: webhook.headers || ''
    };
  }

  _startCreate() {
    this._selectedWebhook = null;
    this._mode = 'create';
    this._activeTab = 'webhook';
    this._formData = this._getEmptyFormData();
    this._testResult = null;
  }

  _startEdit() {
    this._mode = 'edit';
  }

  _cancelEdit() {
    if (this._selectedWebhook) {
      this._mode = 'view';
      this._formData = this._webhookToFormData(this._selectedWebhook);
    } else {
      this._selectedWebhook = null;
      this._mode = 'view';
    }
  }

  _handleInputChange(field, value) {
    this._formData = { ...this._formData, [field]: value };
  }

  _handleEventToggle(eventCode, checked) {
    const events = [...(this._formData.events || [])];
    if (checked && !events.includes(eventCode)) {
      events.push(eventCode);
    } else if (!checked) {
      const idx = events.indexOf(eventCode);
      if (idx > -1) events.splice(idx, 1);
    }
    this._formData = { ...this._formData, events };
  }

  _selectAllInCategory(category) {
    const categoryEventCodes = category.events.map(e => e.code);
    const events = [...(this._formData.events || [])];
    const allSelected = categoryEventCodes.every(code => events.includes(code));

    if (allSelected) {
      // Deselect all
      this._formData = {
        ...this._formData,
        events: events.filter(e => !categoryEventCodes.includes(e))
      };
    } else {
      // Select all
      categoryEventCodes.forEach(code => {
        if (!events.includes(code)) events.push(code);
      });
      this._formData = { ...this._formData, events };
    }
  }

  _generateSecret() {
    const array = new Uint8Array(32);
    crypto.getRandomValues(array);
    const secret = Array.from(array, byte => byte.toString(16).padStart(2, '0')).join('');
    this._formData = { ...this._formData, secret };
  }

  _copyToClipboard(text) {
    navigator.clipboard.writeText(text);
  }

  async _saveWebhook() {
    if (!this._formData.name || !this._formData.url) {
      alert('Name and URL are required');
      return;
    }

    this._saving = true;
    try {
      const isNew = this._mode === 'create';
      const payload = {
        ...this._formData,
        events: JSON.stringify(this._formData.events)
      };

      if (!isNew && this._selectedWebhook) {
        payload.id = this._selectedWebhook.id;
      }

      const url = isNew
        ? '/umbraco/management/api/v1/ecommerce/webhooks'
        : `/umbraco/management/api/v1/ecommerce/webhooks/${this._selectedWebhook.id}`;

      const response = await this._authFetch(url, {
        method: isNew ? 'POST' : 'PUT',
        body: JSON.stringify(payload)
      });

      if (!response.ok) throw new Error('Failed to save webhook');

      const savedWebhook = await response.json();
      await this._loadWebhooks();

      // Select the saved webhook
      const webhook = this._webhooks.find(w => w.id === (savedWebhook.id || this._selectedWebhook?.id));
      if (webhook) {
        this._selectWebhook(webhook);
      }
      this._mode = 'view';
    } catch (error) {
      alert('Failed to save webhook: ' + error.message);
    } finally {
      this._saving = false;
    }
  }

  async _deleteWebhook() {
    if (!this._selectedWebhook || !confirm('Are you sure you want to delete this webhook?')) return;

    try {
      const response = await this._authFetch(`/umbraco/management/api/v1/ecommerce/webhooks/${this._selectedWebhook.id}`, {
        method: 'DELETE'
      });

      if (!response.ok) throw new Error('Failed to delete webhook');

      this._selectedWebhook = null;
      this._mode = 'view';
      await this._loadWebhooks();
    } catch (error) {
      alert('Failed to delete webhook: ' + error.message);
    }
  }

  async _testWebhook() {
    if (!this._selectedWebhook) return;

    this._testing = true;
    this._testResult = null;

    try {
      const response = await this._authFetch(`/umbraco/management/api/v1/ecommerce/webhooks/${this._selectedWebhook.id}/test`, {
        method: 'POST'
      });

      const result = await response.json().catch(() => ({}));

      this._testResult = {
        success: response.ok,
        statusCode: result.statusCode || response.status,
        message: result.message || (response.ok ? 'Test webhook sent successfully!' : 'Failed to send test webhook'),
        responseBody: result.responseBody || null,
        timestamp: new Date().toISOString()
      };
    } catch (error) {
      this._testResult = {
        success: false,
        message: error.message,
        timestamp: new Date().toISOString()
      };
    } finally {
      this._testing = false;
    }
  }

  async _retryDelivery(deliveryId) {
    try {
      const response = await this._authFetch(`/umbraco/management/api/v1/ecommerce/webhooks/${this._selectedWebhook.id}/deliveries/${deliveryId}/retry`, {
        method: 'POST'
      });

      if (response.ok) {
        this._loadDeliveries(this._selectedWebhook.id);
      } else {
        alert('Failed to retry delivery');
      }
    } catch (error) {
      alert('Error retrying delivery: ' + error.message);
    }
  }

  _getRetryTimeline() {
    const { retryCount, retryIntervalSeconds } = this._formData;
    const timeline = [];
    let totalTime = 0;

    for (let i = 0; i <= retryCount; i++) {
      if (i === 0) {
        timeline.push({ attempt: 1, delay: 'Immediate' });
      } else {
        const delay = retryIntervalSeconds * Math.pow(2, i - 1);
        totalTime += delay;
        const mins = Math.floor(delay / 60);
        const secs = delay % 60;
        const delayStr = mins > 0 ? `${mins}m ${secs}s` : `${secs}s`;
        timeline.push({ attempt: i + 1, delay: delayStr });
      }
    }

    return timeline;
  }

  _formatDate(dateStr) {
    if (!dateStr) return 'Never';
    const date = new Date(dateStr);
    return date.toLocaleString();
  }

  _parseEvents(eventsJson) {
    try {
      return JSON.parse(eventsJson || '[]');
    } catch {
      return [];
    }
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
    const webhooks = this._getFilteredWebhooks();

    return html`
      <div class="list-panel">
        <div class="list-header">
          <h3>
            Webhooks
            <uui-button look="primary" compact @click=${this._startCreate}>
              <uui-icon name="icon-add"></uui-icon> Add
            </uui-button>
          </h3>
          <div class="search-box">
            <uui-icon name="icon-search"></uui-icon>
            <input
              type="text"
              placeholder="Search webhooks..."
              .value=${this._searchTerm}
              @input=${(e) => this._searchTerm = e.target.value}
            />
          </div>
        </div>

        <div class="filter-chips">
          <span class="filter-chip ${this._statusFilter === 'all' ? 'active' : ''}" @click=${() => this._statusFilter = 'all'}>
            All<span class="count">(${counts.all})</span>
          </span>
          <span class="filter-chip ${this._statusFilter === 'active' ? 'active' : ''}" @click=${() => this._statusFilter = 'active'}>
            Active<span class="count">(${counts.active})</span>
          </span>
          <span class="filter-chip ${this._statusFilter === 'inactive' ? 'active' : ''}" @click=${() => this._statusFilter = 'inactive'}>
            Inactive<span class="count">(${counts.inactive})</span>
          </span>
        </div>

        <div class="webhook-list">
          ${this._loading ? html`<div class="loading"><uui-loader></uui-loader></div>` :
            webhooks.length === 0 ? html`
              <div class="empty-state" style="padding: 40px 20px;">
                <uui-icon name="icon-link"></uui-icon>
                <p>No webhooks found</p>
              </div>
            ` :
            webhooks.map(webhook => this._renderWebhookItem(webhook))
          }
        </div>
      </div>
    `;
  }

  _renderWebhookItem(webhook) {
    const events = this._parseEvents(webhook.events);
    const isSelected = this._selectedWebhook?.id === webhook.id;

    return html`
      <div class="webhook-item ${isSelected ? 'selected' : ''}" @click=${() => this._selectWebhook(webhook)}>
        <div class="webhook-item-header">
          <span class="webhook-name">${webhook.name}</span>
          <span class="status-dot ${webhook.isActive ? 'active' : 'inactive'}"></span>
        </div>
        <div class="webhook-url" title="${webhook.url}">${webhook.url}</div>
        <div class="webhook-meta">
          <span class="event-count">${events.length} event${events.length !== 1 ? 's' : ''}</span>
          ${webhook.lastTriggeredAt ? html`
            <span class="last-triggered">Last: ${this._formatDate(webhook.lastTriggeredAt)}</span>
          ` : ''}
        </div>
      </div>
    `;
  }

  _renderEditorPanel() {
    if (!this._selectedWebhook && this._mode !== 'create') {
      return html`
        <div class="editor-panel">
          <div class="empty-state">
            <uui-icon name="icon-link"></uui-icon>
            <h3>No Webhook Selected</h3>
            <p>Select a webhook from the list or create a new one</p>
            <uui-button look="primary" @click=${this._startCreate}>
              <uui-icon name="icon-add"></uui-icon> Create Webhook
            </uui-button>
          </div>
        </div>
      `;
    }

    const isEditing = this._mode === 'edit' || this._mode === 'create';
    const title = this._mode === 'create' ? 'New Webhook' : this._formData.name || 'Webhook';

    return html`
      <div class="editor-panel">
        <div class="editor-header">
          <h3>${title}</h3>
          <div class="editor-actions">
            ${isEditing ? html`
              <uui-button look="secondary" @click=${this._cancelEdit} ?disabled=${this._saving}>Cancel</uui-button>
              <uui-button look="primary" @click=${this._saveWebhook} ?disabled=${this._saving}>
                ${this._saving ? 'Saving...' : 'Save'}
              </uui-button>
            ` : html`
              <uui-button look="secondary" @click=${this._testWebhook} ?disabled=${this._testing}>
                <uui-icon name="icon-flash"></uui-icon> ${this._testing ? 'Testing...' : 'Test'}
              </uui-button>
              <uui-button look="primary" @click=${this._startEdit}>
                <uui-icon name="icon-edit"></uui-icon> Edit
              </uui-button>
              <uui-button look="secondary" color="danger" @click=${this._deleteWebhook}>
                <uui-icon name="icon-delete"></uui-icon>
              </uui-button>
            `}
          </div>
        </div>

        <div class="tabs">
          <div class="tab ${this._activeTab === 'webhook' ? 'active' : ''}" @click=${() => this._activeTab = 'webhook'}>Webhook</div>
          <div class="tab ${this._activeTab === 'events' ? 'active' : ''}" @click=${() => this._activeTab = 'events'}>Events</div>
          <div class="tab ${this._activeTab === 'security' ? 'active' : ''}" @click=${() => this._activeTab = 'security'}>Security</div>
          ${this._mode !== 'create' ? html`
            <div class="tab ${this._activeTab === 'deliveries' ? 'active' : ''}" @click=${() => this._activeTab = 'deliveries'}>Delivery Logs</div>
          ` : ''}
        </div>

        <div class="tab-content">
          ${this._activeTab === 'webhook' ? this._renderWebhookTab(isEditing) : ''}
          ${this._activeTab === 'events' ? this._renderEventsTab(isEditing) : ''}
          ${this._activeTab === 'security' ? this._renderSecurityTab(isEditing) : ''}
          ${this._activeTab === 'deliveries' ? this._renderDeliveriesTab() : ''}
        </div>
      </div>
    `;
  }

  _renderWebhookTab(isEditing) {
    return html`
      <div class="form-section">
        <h4>Webhook Configuration</h4>

        <div class="form-group">
          <label>Name *</label>
          <input
            type="text"
            .value=${this._formData.name}
            @input=${(e) => this._handleInputChange('name', e.target.value)}
            ?disabled=${!isEditing}
            placeholder="e.g., Order Notification Webhook"
          />
          <small>A friendly name to identify this webhook</small>
        </div>

        <div class="form-group">
          <label>Endpoint URL *</label>
          <input
            type="url"
            .value=${this._formData.url}
            @input=${(e) => this._handleInputChange('url', e.target.value)}
            ?disabled=${!isEditing}
            placeholder="https://your-server.com/webhook"
          />
          <small>The URL that will receive webhook POST requests</small>
        </div>

        <div class="form-group">
          <label>Custom Headers (JSON)</label>
          <textarea
            .value=${this._formData.headers}
            @input=${(e) => this._handleInputChange('headers', e.target.value)}
            ?disabled=${!isEditing}
            placeholder='{"Authorization": "Bearer your-token"}'
            rows="3"
          ></textarea>
          <small>Optional custom headers to include with each request</small>
        </div>

        <div class="form-group">
          <div class="toggle-group">
            <label class="toggle-switch">
              <input
                type="checkbox"
                ?checked=${this._formData.isActive}
                @change=${(e) => this._handleInputChange('isActive', e.target.checked)}
                ?disabled=${!isEditing}
              />
              <span class="toggle-slider"></span>
            </label>
            <span class="toggle-label">Webhook is ${this._formData.isActive ? 'Active' : 'Inactive'}</span>
          </div>
        </div>
      </div>

      <div class="form-section">
        <h4>Retry Settings</h4>

        <div class="retry-settings">
          <div class="form-group">
            <label>Max Retries</label>
            <input
              type="number"
              min="0"
              max="10"
              .value=${this._formData.retryCount}
              @input=${(e) => this._handleInputChange('retryCount', parseInt(e.target.value) || 0)}
              ?disabled=${!isEditing}
            />
          </div>

          <div class="form-group">
            <label>Retry Interval (seconds)</label>
            <input
              type="number"
              min="10"
              max="3600"
              .value=${this._formData.retryIntervalSeconds}
              @input=${(e) => this._handleInputChange('retryIntervalSeconds', parseInt(e.target.value) || 60)}
              ?disabled=${!isEditing}
            />
          </div>

          <div class="form-group">
            <label>Timeout (seconds)</label>
            <input
              type="number"
              min="5"
              max="300"
              .value=${this._formData.timeoutSeconds}
              @input=${(e) => this._handleInputChange('timeoutSeconds', parseInt(e.target.value) || 30)}
              ?disabled=${!isEditing}
            />
          </div>
        </div>

        <div class="retry-preview">
          <h5>Retry Schedule (Exponential Backoff)</h5>
          <div class="retry-timeline">
            ${this._getRetryTimeline().map(r => html`
              <span class="retry-attempt">Attempt ${r.attempt}: ${r.delay}</span>
            `)}
          </div>
        </div>
      </div>

      ${this._testResult ? html`
        <div class="test-result ${this._testResult.success ? 'success' : 'error'}">
          <h5>${this._testResult.success ? '✓ Test Successful' : '✗ Test Failed'}</h5>
          <p>${this._testResult.message}</p>
          ${this._testResult.statusCode ? html`<p>Status Code: ${this._testResult.statusCode}</p>` : ''}
          ${this._testResult.responseBody ? html`
            <pre>${JSON.stringify(this._testResult.responseBody, null, 2)}</pre>
          ` : ''}
        </div>
      ` : ''}
    `;
  }

  _renderEventsTab(isEditing) {
    const selectedEvents = this._formData.events || [];

    return html`
      <div class="form-section">
        <h4>Subscribed Events (${selectedEvents.length} selected)</h4>
        <small style="display: block; margin-bottom: 16px; color: #666;">
          Select which events should trigger this webhook. The webhook will receive a POST request with event details.
        </small>

        <div class="event-categories">
          ${this._eventCategories.map(category => {
            const categoryEventCodes = category.events.map(e => e.code);
            const selectedInCategory = categoryEventCodes.filter(code => selectedEvents.includes(code)).length;
            const allSelected = selectedInCategory === category.events.length;

            return html`
              <div class="event-category">
                <div class="event-category-header" @click=${() => isEditing && this._selectAllInCategory(category)}>
                  <h5>
                    <span class="category-icon ${category.id}">
                      <uui-icon name="${category.icon}"></uui-icon>
                    </span>
                    ${category.name}
                    <span style="font-weight: normal; color: #666;">(${selectedInCategory}/${category.events.length})</span>
                  </h5>
                  ${isEditing ? html`
                    <span class="select-all-btn">${allSelected ? 'Deselect All' : 'Select All'}</span>
                  ` : ''}
                </div>
                <div class="event-list">
                  ${category.events.map(event => html`
                    <div class="event-checkbox">
                      <input
                        type="checkbox"
                        id="event-${event.code}"
                        ?checked=${selectedEvents.includes(event.code)}
                        @change=${(e) => this._handleEventToggle(event.code, e.target.checked)}
                        ?disabled=${!isEditing}
                      />
                      <div>
                        <label for="event-${event.code}">${event.name}</label>
                        <div class="event-desc">${event.desc}</div>
                      </div>
                    </div>
                  `)}
                </div>
              </div>
            `;
          })}
        </div>
      </div>
    `;
  }

  _renderSecurityTab(isEditing) {
    return html`
      <div class="form-section">
        <h4>Webhook Signing Secret</h4>
        <small style="display: block; margin-bottom: 16px; color: #666;">
          Use this secret to verify that webhook requests are coming from Algora Commerce. Each request includes an HMAC-SHA256 signature in the X-Signature header.
        </small>

        <div class="form-group">
          <label>Secret Key</label>
          ${this._formData.secret ? html`
            <div class="secret-display ${this._showSecret ? '' : 'hidden'}">
              ${this._formData.secret}
            </div>
            <div class="secret-actions">
              <uui-button look="secondary" compact @click=${() => this._showSecret = !this._showSecret}>
                <uui-icon name="${this._showSecret ? 'icon-eye-slash' : 'icon-eye'}"></uui-icon>
                ${this._showSecret ? 'Hide' : 'Show'}
              </uui-button>
              <uui-button look="secondary" compact @click=${() => this._copyToClipboard(this._formData.secret)}>
                <uui-icon name="icon-documents"></uui-icon> Copy
              </uui-button>
              ${isEditing ? html`
                <uui-button look="secondary" compact @click=${this._generateSecret}>
                  <uui-icon name="icon-axis-rotation"></uui-icon> Regenerate
                </uui-button>
              ` : ''}
            </div>
          ` : html`
            ${isEditing ? html`
              <div class="input-with-action">
                <input
                  type="text"
                  .value=${this._formData.secret}
                  @input=${(e) => this._handleInputChange('secret', e.target.value)}
                  placeholder="Enter secret or generate one"
                />
                <uui-button look="secondary" @click=${this._generateSecret}>Generate</uui-button>
              </div>
            ` : html`
              <p style="color: #666; font-style: italic;">No secret configured. Edit the webhook to add a signing secret.</p>
            `}
          `}
        </div>

        <div class="signature-example">
          <h5>Signature Verification Example</h5>
          <pre>// Node.js example
const crypto = require('crypto');

function verifySignature(payload, signature, secret) {
  const expectedSignature = crypto
    .createHmac('sha256', secret)
    .update(payload)
    .digest('hex');

  return signature === \`sha256=\${expectedSignature}\`;
}

// Usage
const isValid = verifySignature(
  requestBody,
  request.headers['x-signature'],
  '${this._formData.secret || 'your-secret-key'}'
);</pre>
        </div>
      </div>

      <div class="form-section">
        <h4>Payload Format</h4>
        <div class="signature-example">
          <h5>Example Webhook Payload</h5>
          <pre>{
  "id": "evt_abc123",
  "type": "order.created",
  "timestamp": "2024-01-15T10:30:00Z",
  "data": {
    "orderId": "550e8400-e29b-41d4-a716-446655440000",
    "orderNumber": "ORD-2024-0001",
    "totalAmount": 149.99,
    "currency": "USD",
    "customerId": "cust_xyz789",
    "items": [...]
  }
}</pre>
        </div>
      </div>
    `;
  }

  _renderDeliveriesTab() {
    if (this._loadingDeliveries) {
      return html`<div class="loading"><uui-loader></uui-loader></div>`;
    }

    const deliveries = this._deliveries || [];
    const stats = {
      total: deliveries.length,
      success: deliveries.filter(d => d.status === 'success').length,
      failed: deliveries.filter(d => d.status === 'failed').length,
      pending: deliveries.filter(d => d.status === 'pending' || d.status === 'retrying').length
    };

    return html`
      <div class="delivery-stats">
        <div class="stat-card">
          <div class="stat-value">${stats.total}</div>
          <div class="stat-label">Total Deliveries</div>
        </div>
        <div class="stat-card success">
          <div class="stat-value">${stats.success}</div>
          <div class="stat-label">Successful</div>
        </div>
        <div class="stat-card failed">
          <div class="stat-value">${stats.failed}</div>
          <div class="stat-label">Failed</div>
        </div>
        <div class="stat-card pending">
          <div class="stat-value">${stats.pending}</div>
          <div class="stat-label">Pending</div>
        </div>
      </div>

      ${deliveries.length === 0 ? html`
        <div class="form-section" style="text-align: center; padding: 40px;">
          <uui-icon name="icon-inbox" style="font-size: 48px; opacity: 0.3;"></uui-icon>
          <h4 style="margin: 16px 0 8px;">No Delivery History</h4>
          <p style="color: #666;">Webhook deliveries will appear here once events are triggered.</p>
        </div>
      ` : html`
        <table class="delivery-table">
          <thead>
            <tr>
              <th>Event</th>
              <th>Status</th>
              <th>Response</th>
              <th>Attempts</th>
              <th>Timestamp</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            ${deliveries.map(delivery => html`
              <tr>
                <td>
                  <strong>${delivery.eventType}</strong>
                  <div style="font-size: 11px; color: #666;">${delivery.eventId?.substring(0, 8)}...</div>
                </td>
                <td>
                  <span class="status-badge ${delivery.status}">${delivery.status}</span>
                </td>
                <td>
                  <span class="response-code ${delivery.responseCode >= 200 && delivery.responseCode < 300 ? 'success' : 'error'}">
                    ${delivery.responseCode || '-'}
                  </span>
                </td>
                <td>${delivery.attemptCount || 1}</td>
                <td>${this._formatDate(delivery.createdAt)}</td>
                <td>
                  ${delivery.status === 'failed' ? html`
                    <uui-button look="secondary" compact @click=${() => this._retryDelivery(delivery.id)}>
                      Retry
                    </uui-button>
                  ` : ''}
                </td>
              </tr>
            `)}
          </tbody>
        </table>
      `}
    `;
  }
}

customElements.define('ecommerce-webhook-collection', WebhookCollection);
export default WebhookCollection;
