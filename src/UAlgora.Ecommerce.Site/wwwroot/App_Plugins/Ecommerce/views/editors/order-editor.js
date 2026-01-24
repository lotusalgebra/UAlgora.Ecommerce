import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Algora Order Editor Component
 * Enterprise-grade editor for orders with proper tab-based document type management.
 *
 * Architecture:
 * - Tab-based property groups matching Umbraco document type patterns
 * - Centralized state management with validation pipeline
 * - Reusable field rendering methods
 * - Configuration-driven design
 */
export class OrderEditor extends UmbElementMixin(LitElement) {
  // ============================================
  // BRANDING CONFIGURATION
  // ============================================
  static DOCUMENT_TYPE = {
    name: 'Order',
    prefix: 'Algora',
    icon: 'icon-receipt-dollar',
    color: '#f59e0b', // Amber/Orange theme
    colorLight: '#fef3c7'
  };

  // ============================================
  // CONFIGURATION
  // ============================================

  static TABS = [
    { id: 'overview', label: 'Overview', icon: 'document' },
    { id: 'customer', label: 'Customer', icon: 'user' },
    { id: 'items', label: 'Line Items', icon: 'box' },
    { id: 'addresses', label: 'Addresses', icon: 'map' },
    { id: 'payment', label: 'Payment', icon: 'credit-card' },
    { id: 'shipping', label: 'Shipping', icon: 'truck' },
    { id: 'notes', label: 'Notes', icon: 'message' },
    { id: 'timeline', label: 'Timeline', icon: 'time' }
  ];

  static VALIDATION_RULES = {
    customerEmail: { required: true, pattern: /^[^\s@]+@[^\s@]+\.[^\s@]+$/ },
    customerName: { required: true, minLength: 2 },
    'shippingAddress.addressLine1': { required: true },
    'shippingAddress.city': { required: true },
    'shippingAddress.postalCode': { required: true },
    'shippingAddress.country': { required: true }
  };

  static STATUS_OPTIONS = {
    order: ['Pending', 'Confirmed', 'Processing', 'Shipped', 'Delivered', 'Completed', 'Cancelled', 'Refunded'],
    payment: ['Pending', 'Paid', 'PartiallyPaid', 'Failed', 'Refunded'],
    fulfillment: ['Unfulfilled', 'PartiallyFulfilled', 'Fulfilled', 'Shipped', 'Delivered']
  };

  static PAYMENT_METHODS = [
    { value: 'CreditCard', label: 'Credit Card' },
    { value: 'DebitCard', label: 'Debit Card' },
    { value: 'PayPal', label: 'PayPal' },
    { value: 'BankTransfer', label: 'Bank Transfer' },
    { value: 'CashOnDelivery', label: 'Cash on Delivery' },
    { value: 'Crypto', label: 'Cryptocurrency' }
  ];

  static SHIPPING_METHODS = [
    { value: 'Standard', label: 'Standard Shipping' },
    { value: 'Express', label: 'Express Shipping' },
    { value: 'Overnight', label: 'Overnight Shipping' },
    { value: 'SameDay', label: 'Same Day Delivery' },
    { value: 'Pickup', label: 'Store Pickup' },
    { value: 'Free', label: 'Free Shipping' }
  ];

  // ============================================
  // STYLES
  // ============================================

  static styles = css`
    :host {
      display: block;
      height: 100%;
      background: var(--uui-color-background);
    }

    * { box-sizing: border-box; }

    /* Layout */
    .editor-layout {
      display: flex;
      flex-direction: column;
      height: 100%;
    }

    /* Header with Algora Branding */
    .editor-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 16px 24px;
      background: linear-gradient(135deg, #fffbeb 0%, #fef3c7 100%);
      border-bottom: 3px solid #f59e0b;
      flex-shrink: 0;
    }

    .header-title {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .algora-badge {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 6px 12px;
      background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
      border-radius: 6px;
      color: white;
      font-weight: 600;
      font-size: 12px;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      box-shadow: 0 2px 4px rgba(245, 158, 11, 0.3);
    }

    .algora-badge uui-icon {
      font-size: 16px;
    }

    .header-title h1 {
      margin: 0;
      font-size: 20px;
      font-weight: 600;
      color: #92400e;
    }

    .status-badge {
      padding: 4px 10px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: 500;
      text-transform: uppercase;
    }

    .badge-pending { background: #fef3c7; color: #92400e; }
    .badge-confirmed { background: #dbeafe; color: #1e40af; }
    .badge-processing { background: #e0e7ff; color: #3730a3; }
    .badge-shipped, .badge-delivered, .badge-completed, .badge-paid { background: #d1fae5; color: #065f46; }
    .badge-cancelled, .badge-failed { background: #fee2e2; color: #991b1b; }
    .badge-refunded { background: #f3f4f6; color: #374151; }

    .header-actions {
      display: flex;
      gap: 8px;
    }

    .btn {
      padding: 8px 16px;
      border: none;
      border-radius: 4px;
      font-size: 14px;
      font-weight: 500;
      cursor: pointer;
      transition: all 0.15s ease;
    }

    .btn-secondary {
      background: var(--uui-color-surface);
      color: var(--uui-color-text);
      border: 1px solid var(--uui-color-border);
    }

    .btn-secondary:hover { background: var(--uui-color-surface-emphasis); }

    .btn-primary {
      background: var(--uui-color-positive);
      color: white;
    }

    .btn-primary:hover { background: var(--uui-color-positive-emphasis); }
    .btn-danger { background: var(--uui-color-danger); color: white; }
    .btn:disabled { opacity: 0.6; cursor: not-allowed; }

    /* Tabs */
    .tabs-nav {
      display: flex;
      background: var(--uui-color-surface);
      border-bottom: 1px solid var(--uui-color-border);
      padding: 0 24px;
      flex-shrink: 0;
      overflow-x: auto;
    }

    .tab-btn {
      padding: 14px 20px;
      border: none;
      background: transparent;
      font-size: 14px;
      font-weight: 500;
      color: var(--uui-color-text-alt);
      cursor: pointer;
      border-bottom: 2px solid transparent;
      transition: all 0.15s ease;
      white-space: nowrap;
    }

    .tab-btn:hover {
      color: var(--uui-color-text);
      background: var(--uui-color-surface-emphasis);
    }

    .tab-btn.active {
      color: var(--uui-color-selected);
      border-bottom-color: var(--uui-color-selected);
    }

    .tab-btn.has-error { color: var(--uui-color-danger); }
    .tab-btn.has-error.active { border-bottom-color: var(--uui-color-danger); }

    /* Content */
    .editor-content {
      flex: 1;
      overflow-y: auto;
      padding: 24px;
    }

    .tab-panel {
      display: none;
      max-width: 1200px;
    }

    .tab-panel.active { display: block; }

    /* Property Groups */
    .property-group {
      background: var(--uui-color-surface);
      border-radius: 8px;
      margin-bottom: 24px;
      box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
      overflow: hidden;
    }

    .property-group-header {
      padding: 16px 20px;
      background: var(--uui-color-surface-alt);
      border-bottom: 1px solid var(--uui-color-border);
    }

    .property-group-header h3 {
      margin: 0;
      font-size: 14px;
      font-weight: 600;
      color: var(--uui-color-text);
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .property-group-body { padding: 20px; }

    /* Property Row */
    .property-row {
      display: grid;
      grid-template-columns: 200px 1fr;
      gap: 20px;
      padding: 16px 0;
      border-bottom: 1px solid var(--uui-color-border);
    }

    .property-row:first-child { padding-top: 0; }
    .property-row:last-child { border-bottom: none; padding-bottom: 0; }

    .property-label {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .property-label label {
      font-size: 14px;
      font-weight: 500;
      color: var(--uui-color-text);
    }

    .property-label label.required::after {
      content: ' *';
      color: var(--uui-color-danger);
    }

    .property-label .property-description {
      font-size: 12px;
      color: var(--uui-color-text-alt);
      line-height: 1.4;
    }

    .property-editor {
      display: flex;
      flex-direction: column;
      gap: 6px;
    }

    .property-error {
      font-size: 12px;
      color: var(--uui-color-danger);
    }

    /* Form Controls */
    .form-input {
      width: 100%;
      height: 40px;
      padding: 0 12px;
      border: 1px solid var(--uui-color-border);
      border-radius: 4px;
      font-size: 14px;
      font-family: inherit;
      background: var(--uui-color-surface);
      color: var(--uui-color-text);
      transition: border-color 0.15s ease, box-shadow 0.15s ease;
    }

    .form-input:focus {
      outline: none;
      border-color: var(--uui-color-selected);
      box-shadow: 0 0 0 2px rgba(27, 107, 201, 0.15);
    }

    .form-input.error { border-color: var(--uui-color-danger); }
    .form-input::placeholder { color: var(--uui-color-text-alt); }
    .form-input:disabled { background: var(--uui-color-surface-alt); cursor: not-allowed; }

    textarea.form-input {
      height: auto;
      min-height: 100px;
      padding: 12px;
      resize: vertical;
    }

    select.form-input { cursor: pointer; }

    /* Input Group */
    .input-group {
      display: flex;
    }

    .input-group-prepend {
      display: flex;
      align-items: center;
      padding: 0 12px;
      background: var(--uui-color-surface-alt);
      border: 1px solid var(--uui-color-border);
      border-right: none;
      border-radius: 4px 0 0 4px;
      font-size: 14px;
      color: var(--uui-color-text-alt);
      font-weight: 500;
    }

    .input-group .form-input {
      border-radius: 0 4px 4px 0;
    }

    /* Status Grid */
    .status-grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 16px;
    }

    .status-card {
      padding: 16px;
      background: var(--uui-color-surface-alt);
      border-radius: 8px;
      border: 1px solid var(--uui-color-border);
    }

    .status-card label {
      display: block;
      font-size: 12px;
      font-weight: 600;
      color: var(--uui-color-text-alt);
      margin-bottom: 8px;
      text-transform: uppercase;
    }

    .status-card select {
      width: 100%;
    }

    /* Info Cards */
    .info-cards {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 20px;
    }

    .info-card {
      padding: 20px;
      background: var(--uui-color-surface-alt);
      border-radius: 8px;
      border: 1px solid var(--uui-color-border);
    }

    .info-card h4 {
      margin: 0 0 16px 0;
      font-size: 14px;
      font-weight: 600;
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .info-row {
      display: flex;
      justify-content: space-between;
      padding: 8px 0;
      font-size: 14px;
      border-bottom: 1px solid var(--uui-color-border);
    }

    .info-row:last-child { border-bottom: none; }
    .info-row .label { color: var(--uui-color-text-alt); }
    .info-row .value { font-weight: 500; }

    /* Line Items Table */
    .line-items-table {
      width: 100%;
      border-collapse: collapse;
    }

    .line-items-table th,
    .line-items-table td {
      padding: 12px;
      text-align: left;
      border-bottom: 1px solid var(--uui-color-border);
    }

    .line-items-table th {
      background: var(--uui-color-surface-alt);
      font-weight: 600;
      font-size: 12px;
      text-transform: uppercase;
      color: var(--uui-color-text-alt);
    }

    .line-items-table input,
    .line-items-table select {
      width: 100%;
      padding: 8px;
      border: 1px solid var(--uui-color-border);
      border-radius: 4px;
      font-size: 14px;
    }

    .remove-btn {
      padding: 6px 12px;
      background: var(--uui-color-danger);
      color: white;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-size: 12px;
    }

    .add-line-btn {
      display: inline-flex;
      align-items: center;
      gap: 8px;
      padding: 10px 16px;
      margin-top: 12px;
      background: var(--uui-color-surface-alt);
      border: 1px dashed var(--uui-color-border);
      border-radius: 4px;
      cursor: pointer;
      font-size: 14px;
    }

    .add-line-btn:hover {
      background: var(--uui-color-surface-emphasis);
      border-color: var(--uui-color-selected);
    }

    /* Order Totals */
    .order-totals {
      margin-top: 24px;
      padding: 20px;
      background: var(--uui-color-surface-alt);
      border-radius: 8px;
      max-width: 400px;
      margin-left: auto;
    }

    .total-row {
      display: flex;
      justify-content: space-between;
      padding: 8px 0;
      font-size: 14px;
    }

    .total-row.grand-total {
      border-top: 2px solid var(--uui-color-border);
      margin-top: 8px;
      padding-top: 16px;
      font-size: 18px;
      font-weight: 600;
    }

    .total-row input {
      width: 100px;
      text-align: right;
      padding: 6px;
      border: 1px solid var(--uui-color-border);
      border-radius: 4px;
    }

    /* Address Grid */
    .address-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 24px;
    }

    .address-card {
      padding: 20px;
      background: var(--uui-color-surface-alt);
      border-radius: 8px;
      border: 1px solid var(--uui-color-border);
    }

    .address-card h4 {
      margin: 0 0 16px 0;
      font-size: 14px;
      font-weight: 600;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .copy-btn {
      padding: 6px 12px;
      font-size: 12px;
      background: var(--uui-color-selected);
      color: white;
      border: none;
      border-radius: 4px;
      cursor: pointer;
    }

    .address-form {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 12px;
    }

    .address-form .full-width {
      grid-column: 1 / -1;
    }

    .address-form label {
      display: block;
      font-size: 12px;
      font-weight: 500;
      margin-bottom: 4px;
      color: var(--uui-color-text-alt);
    }

    .address-form label .required {
      color: var(--uui-color-danger);
    }

    /* Checkbox */
    .checkbox-wrapper {
      display: flex;
      align-items: center;
      gap: 10px;
      margin-bottom: 16px;
    }

    .checkbox-wrapper input[type="checkbox"] {
      width: 18px;
      height: 18px;
      cursor: pointer;
    }

    /* Timeline */
    .timeline {
      position: relative;
      padding-left: 32px;
    }

    .timeline::before {
      content: '';
      position: absolute;
      left: 8px;
      top: 0;
      bottom: 0;
      width: 2px;
      background: var(--uui-color-border);
    }

    .timeline-item {
      position: relative;
      padding-bottom: 24px;
    }

    .timeline-item::before {
      content: '';
      position: absolute;
      left: -24px;
      top: 4px;
      width: 12px;
      height: 12px;
      border-radius: 50%;
      background: var(--uui-color-border);
      border: 2px solid var(--uui-color-surface);
    }

    .timeline-item.completed::before { background: var(--uui-color-positive); }
    .timeline-item.current::before {
      background: var(--uui-color-selected);
      box-shadow: 0 0 0 4px var(--uui-color-interactive-emphasis);
    }

    .timeline-item .time {
      font-size: 12px;
      color: var(--uui-color-text-alt);
      margin-bottom: 4px;
    }

    .timeline-item .event {
      font-size: 14px;
      font-weight: 500;
    }

    /* Notes Container */
    .notes-container {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 24px;
    }

    .note-card {
      padding: 20px;
      background: var(--uui-color-surface-alt);
      border-radius: 8px;
    }

    .note-card h4 {
      margin: 0 0 12px 0;
      font-size: 14px;
      font-weight: 600;
    }

    .note-card textarea {
      width: 100%;
      min-height: 150px;
      padding: 12px;
      border: 1px solid var(--uui-color-border);
      border-radius: 4px;
      resize: vertical;
    }

    /* Transactions Table */
    .transactions-table {
      width: 100%;
      border-collapse: collapse;
      margin-top: 16px;
    }

    .transactions-table th,
    .transactions-table td {
      padding: 10px;
      text-align: left;
      border-bottom: 1px solid var(--uui-color-border);
      font-size: 13px;
    }

    .transactions-table th {
      background: var(--uui-color-surface);
      font-weight: 600;
    }

    /* Loading & Toast */
    .loading-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.4);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
    }

    .toast {
      position: fixed;
      bottom: 24px;
      right: 24px;
      padding: 12px 20px;
      border-radius: 6px;
      color: white;
      font-weight: 500;
      z-index: 1001;
      animation: slideIn 0.3s ease;
    }

    .toast.success { background: var(--uui-color-positive); }
    .toast.error { background: var(--uui-color-danger); }

    @keyframes slideIn {
      from { transform: translateX(100%); opacity: 0; }
      to { transform: translateX(0); opacity: 1; }
    }

    @media (max-width: 768px) {
      .status-grid { grid-template-columns: 1fr; }
      .address-grid { grid-template-columns: 1fr; }
      .notes-container { grid-template-columns: 1fr; }
    }
  `;

  // ============================================
  // PROPERTIES
  // ============================================

  static properties = {
    orderId: { type: String },
    _order: { type: Object, state: true },
    _activeTab: { type: String, state: true },
    _loading: { type: Boolean, state: true },
    _saving: { type: Boolean, state: true },
    _errors: { type: Object, state: true },
    _touched: { type: Object, state: true },
    _products: { type: Array, state: true },
    _toast: { type: Object, state: true },
    _isNew: { type: Boolean, state: true }
  };

  // ============================================
  // LIFECYCLE
  // ============================================

  constructor() {
    super();
    this.orderId = null;
    this._order = this._getEmptyOrder();
    this._activeTab = 'overview';
    this._loading = true;
    this._saving = false;
    this._errors = {};
    this._touched = {};
    this._products = [];
    this._toast = null;
    this._isNew = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadData();
  }

  updated(changedProperties) {
    super.updated(changedProperties);
    if (changedProperties.has('orderId')) {
      const oldValue = changedProperties.get('orderId');
      if (oldValue !== this.orderId) {
        this._loadData();
      }
    }
  }

  // ============================================
  // DATA MODEL
  // ============================================

  _getEmptyOrder() {
    return {
      id: null,
      orderNumber: '',
      status: 'Pending',
      paymentStatus: 'Pending',
      fulfillmentStatus: 'Unfulfilled',
      customerEmail: '',
      customerPhone: '',
      customerName: '',
      customerId: null,
      shippingAddress: this._getEmptyAddress(),
      billingAddress: this._getEmptyAddress(),
      billingSameAsShipping: true,
      lines: [],
      subtotal: 0,
      discountTotal: 0,
      shippingTotal: 0,
      taxTotal: 0,
      grandTotal: 0,
      paidAmount: 0,
      refundedAmount: 0,
      paymentMethod: '',
      paymentProvider: '',
      paymentIntentId: '',
      payments: [],
      shippingMethod: '',
      trackingNumber: '',
      carrier: '',
      shipments: [],
      estimatedDeliveryDate: null,
      customerNote: '',
      internalNote: '',
      placedAt: null,
      confirmedAt: null,
      paidAt: null,
      shippedAt: null,
      deliveredAt: null,
      completedAt: null,
      cancelledAt: null,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString()
    };
  }

  _getEmptyAddress() {
    return {
      firstName: '',
      lastName: '',
      company: '',
      addressLine1: '',
      addressLine2: '',
      city: '',
      state: '',
      postalCode: '',
      country: '',
      phone: ''
    };
  }

  // ============================================
  // DATA LOADING
  // ============================================

  async _loadData() {
    try {
      this._loading = true;
      await this._loadProducts();

      if (this.orderId && this.orderId !== 'create') {
        await this._loadOrder();
        this._isNew = false;
      } else {
        this._order = this._getEmptyOrder();
        this._order.orderNumber = this._generateOrderNumber();
        this._isNew = true;
      }
    } catch (error) {
      console.error('Error loading data:', error);
      this._showToast('Failed to load data', 'error');
    } finally {
      this._loading = false;
    }
  }

  async _loadOrder() {
    const response = await fetch(`/umbraco/management/api/v1/ecommerce/order/${this.orderId}`, {
      credentials: 'include',
      headers: { 'Accept': 'application/json' }
    });

    if (!response.ok) throw new Error('Failed to load order');
    const data = await response.json();
    this._order = { ...this._getEmptyOrder(), ...data };
  }

  async _loadProducts() {
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/product?pageSize=100', {
        credentials: 'include',
        headers: { 'Accept': 'application/json' }
      });

      if (response.ok) {
        const data = await response.json();
        this._products = data.items || [];
      }
    } catch (error) {
      console.error('Error loading products:', error);
    }
  }

  _generateOrderNumber() {
    const timestamp = Date.now().toString(36).toUpperCase();
    const random = Math.random().toString(36).substring(2, 6).toUpperCase();
    return `ORD-${timestamp}-${random}`;
  }

  // ============================================
  // INPUT HANDLERS
  // ============================================

  _handleTextInput(field, event) {
    this._order = { ...this._order, [field]: event.target.value };
    this._touched = { ...this._touched, [field]: true };
    this._validateField(field);
  }

  _handleAddressInput(addressType, field, event) {
    const address = { ...this._order[addressType], [field]: event.target.value };
    this._order = { ...this._order, [addressType]: address };
    this._touched = { ...this._touched, [`${addressType}.${field}`]: true };
    this._validateField(`${addressType}.${field}`);
  }

  _handleCheckboxInput(field, event) {
    this._order = { ...this._order, [field]: event.target.checked };
  }

  _handleSelectInput(field, event) {
    this._order = { ...this._order, [field]: event.target.value };
  }

  _handleNumberInput(field, event) {
    const value = parseFloat(event.target.value) || 0;
    this._order = { ...this._order, [field]: value };
    this._recalculateTotals();
  }

  _copyShippingToBilling() {
    this._order = {
      ...this._order,
      billingAddress: { ...this._order.shippingAddress },
      billingSameAsShipping: true
    };
  }

  // ============================================
  // LINE ITEMS
  // ============================================

  _addLineItem() {
    const newLine = {
      id: crypto.randomUUID(),
      productId: '',
      productName: '',
      sku: '',
      quantity: 1,
      unitPrice: 0,
      totalPrice: 0
    };

    this._order = {
      ...this._order,
      lines: [...this._order.lines, newLine]
    };
  }

  _removeLineItem(index) {
    const lines = [...this._order.lines];
    lines.splice(index, 1);
    this._order = { ...this._order, lines };
    this._recalculateTotals();
  }

  _updateLineItem(index, field, value) {
    const lines = [...this._order.lines];
    lines[index] = { ...lines[index], [field]: value };

    if (field === 'productId' && value) {
      const product = this._products.find(p => p.id === value);
      if (product) {
        lines[index].productName = product.name;
        lines[index].sku = product.sku;
        lines[index].unitPrice = product.salePrice || product.basePrice || product.price;
      }
    }

    this._order = { ...this._order, lines };
    this._recalculateTotals();
  }

  _recalculateTotals() {
    let subtotal = 0;

    this._order.lines.forEach(line => {
      const lineTotal = (line.quantity || 0) * (line.unitPrice || 0);
      line.totalPrice = lineTotal;
      subtotal += lineTotal;
    });

    this._order.subtotal = subtotal;
    this._order.grandTotal = subtotal - (this._order.discountTotal || 0) +
                             (this._order.shippingTotal || 0) +
                             (this._order.taxTotal || 0);
    this._order = { ...this._order };
  }

  // ============================================
  // VALIDATION
  // ============================================

  _validateField(field) {
    const errors = { ...this._errors };

    // Check nested field
    const isNested = field.includes('.');
    let value;
    if (isNested) {
      const [parent, child] = field.split('.');
      value = this._order[parent]?.[child];
    } else {
      value = this._order[field];
    }

    const rules = OrderEditor.VALIDATION_RULES[field];
    if (!rules) {
      delete errors[field];
      this._errors = errors;
      return;
    }

    if (rules.required && (!value || (typeof value === 'string' && !value.trim()))) {
      errors[field] = 'This field is required';
    }
    else if (rules.pattern && value && !rules.pattern.test(value)) {
      errors[field] = field === 'customerEmail' ? 'Invalid email format' : 'Invalid format';
    }
    else if (rules.minLength && value && value.length < rules.minLength) {
      errors[field] = `Must be at least ${rules.minLength} characters`;
    }
    else {
      delete errors[field];
    }

    this._errors = errors;
  }

  _validateAll() {
    Object.keys(OrderEditor.VALIDATION_RULES).forEach(field => {
      this._touched[field] = true;
      this._validateField(field);
    });

    if (this._order.lines.length === 0) {
      this._errors.lines = 'At least one line item is required';
    }

    this._touched = { ...this._touched };
    return Object.keys(this._errors).length === 0;
  }

  _getTabErrors(tabId) {
    const tabFields = {
      customer: ['customerEmail', 'customerName'],
      addresses: ['shippingAddress.addressLine1', 'shippingAddress.city', 'shippingAddress.postalCode', 'shippingAddress.country'],
      items: ['lines']
    };
    const fields = tabFields[tabId] || [];
    return fields.some(f => this._errors[f]);
  }

  // ============================================
  // SAVE & CANCEL
  // ============================================

  async _handleSave() {
    if (!this._validateAll()) {
      this._showToast('Please fix validation errors', 'error');
      for (const tab of OrderEditor.TABS) {
        if (this._getTabErrors(tab.id)) {
          this._activeTab = tab.id;
          break;
        }
      }
      return;
    }

    try {
      this._saving = true;
      this._recalculateTotals();

      const url = this._isNew
        ? '/umbraco/management/api/v1/ecommerce/order'
        : `/umbraco/management/api/v1/ecommerce/order/${this.orderId}`;

      const response = await fetch(url, {
        method: this._isNew ? 'POST' : 'PUT',
        credentials: 'include',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(this._order)
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || 'Failed to save order');
      }

      const savedOrder = await response.json();
      this._order = savedOrder;
      this._isNew = false;
      this.orderId = savedOrder.id;

      this._showToast('Order saved successfully', 'success');

      this.dispatchEvent(new CustomEvent('order-saved', {
        bubbles: true,
        composed: true,
        detail: { order: savedOrder }
      }));
    } catch (error) {
      console.error('Error saving order:', error);
      this._showToast(error.message || 'Failed to save order', 'error');
    } finally {
      this._saving = false;
    }
  }

  _handleCancel() {
    this.dispatchEvent(new CustomEvent('editor-cancel', {
      bubbles: true,
      composed: true
    }));
  }

  _showToast(message, type = 'success') {
    this._toast = { message, type };
    setTimeout(() => { this._toast = null; }, 3000);
  }

  // ============================================
  // UTILITY METHODS
  // ============================================

  _formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount || 0);
  }

  _formatDate(dateString) {
    if (!dateString) return '-';
    return new Date(dateString).toLocaleString();
  }

  _getStatusBadgeClass(status) {
    const s = (status || '').toLowerCase();
    if (['completed', 'paid', 'delivered', 'fulfilled'].includes(s)) return 'badge-completed';
    if (['shipped', 'confirmed'].includes(s)) return 'badge-shipped';
    if (['processing', 'partiallypaid', 'partiallyfulfilled'].includes(s)) return 'badge-processing';
    if (['cancelled', 'failed'].includes(s)) return 'badge-cancelled';
    if (['refunded'].includes(s)) return 'badge-refunded';
    return 'badge-pending';
  }

  // ============================================
  // REUSABLE FIELD RENDERERS
  // ============================================

  _renderTextField(field, label, description, options = {}) {
    const { placeholder = '', required = false, disabled = false, type = 'text' } = options;
    const hasError = this._touched[field] && this._errors[field];

    return html`
      <div class="property-row">
        <div class="property-label">
          <label class="${required ? 'required' : ''}">${label}</label>
          ${description ? html`<span class="property-description">${description}</span>` : ''}
        </div>
        <div class="property-editor">
          <input
            type="${type}"
            class="form-input ${hasError ? 'error' : ''}"
            .value=${this._order[field] || ''}
            @input=${(e) => this._handleTextInput(field, e)}
            placeholder="${placeholder}"
            ?disabled=${disabled}
          />
          ${hasError ? html`<span class="property-error">${this._errors[field]}</span>` : ''}
        </div>
      </div>
    `;
  }

  _renderPriceField(field, label, description, options = {}) {
    const { currency = '$' } = options;
    const value = this._order[field];

    return html`
      <div class="property-row">
        <div class="property-label">
          <label>${label}</label>
          ${description ? html`<span class="property-description">${description}</span>` : ''}
        </div>
        <div class="property-editor">
          <div class="input-group">
            <span class="input-group-prepend">${currency}</span>
            <input
              type="text"
              inputmode="decimal"
              class="form-input"
              .value=${value != null ? value.toString() : '0'}
              @input=${(e) => this._handleNumberInput(field, e)}
              placeholder="0.00"
            />
          </div>
        </div>
      </div>
    `;
  }

  _renderSelectField(field, label, description, selectOptions, options = {}) {
    const { emptyLabel = 'Select...' } = options;

    return html`
      <div class="property-row">
        <div class="property-label">
          <label>${label}</label>
          ${description ? html`<span class="property-description">${description}</span>` : ''}
        </div>
        <div class="property-editor">
          <select
            class="form-input"
            .value=${this._order[field] || ''}
            @change=${(e) => this._handleSelectInput(field, e)}
          >
            <option value="">${emptyLabel}</option>
            ${selectOptions.map(opt => html`
              <option value="${opt.value}" ?selected=${this._order[field] === opt.value}>${opt.label}</option>
            `)}
          </select>
        </div>
      </div>
    `;
  }

  _renderTextareaField(field, label, description, options = {}) {
    const { placeholder = '', rows = 4 } = options;

    return html`
      <div class="property-row">
        <div class="property-label">
          <label>${label}</label>
          ${description ? html`<span class="property-description">${description}</span>` : ''}
        </div>
        <div class="property-editor">
          <textarea
            class="form-input"
            .value=${this._order[field] || ''}
            @input=${(e) => this._handleTextInput(field, e)}
            placeholder="${placeholder}"
            rows="${rows}"
          ></textarea>
        </div>
      </div>
    `;
  }

  // ============================================
  // TAB CONTENT RENDERERS
  // ============================================

  _renderOverviewTab() {
    return html`
      <div class="property-group">
        <div class="property-group-header"><h3>Order Status</h3></div>
        <div class="property-group-body">
          <div class="status-grid">
            <div class="status-card">
              <label>Order Status</label>
              <select class="form-input" .value=${this._order.status} @change=${(e) => this._handleSelectInput('status', e)}>
                ${OrderEditor.STATUS_OPTIONS.order.map(s => html`<option value="${s}" ?selected=${this._order.status === s}>${s}</option>`)}
              </select>
            </div>
            <div class="status-card">
              <label>Payment Status</label>
              <select class="form-input" .value=${this._order.paymentStatus} @change=${(e) => this._handleSelectInput('paymentStatus', e)}>
                ${OrderEditor.STATUS_OPTIONS.payment.map(s => html`<option value="${s}" ?selected=${this._order.paymentStatus === s}>${s}</option>`)}
              </select>
            </div>
            <div class="status-card">
              <label>Fulfillment Status</label>
              <select class="form-input" .value=${this._order.fulfillmentStatus} @change=${(e) => this._handleSelectInput('fulfillmentStatus', e)}>
                ${OrderEditor.STATUS_OPTIONS.fulfillment.map(s => html`<option value="${s}" ?selected=${this._order.fulfillmentStatus === s}>${s}</option>`)}
              </select>
            </div>
          </div>
        </div>
      </div>

      <div class="property-group">
        <div class="property-group-header"><h3>Order Summary</h3></div>
        <div class="property-group-body">
          <div class="info-cards">
            <div class="info-card">
              <h4>Customer</h4>
              <div class="info-row"><span class="label">Name</span><span class="value">${this._order.customerName || '-'}</span></div>
              <div class="info-row"><span class="label">Email</span><span class="value">${this._order.customerEmail || '-'}</span></div>
              <div class="info-row"><span class="label">Phone</span><span class="value">${this._order.customerPhone || '-'}</span></div>
            </div>
            <div class="info-card">
              <h4>Totals</h4>
              <div class="info-row"><span class="label">Subtotal</span><span class="value">${this._formatCurrency(this._order.subtotal)}</span></div>
              <div class="info-row"><span class="label">Discount</span><span class="value">-${this._formatCurrency(this._order.discountTotal)}</span></div>
              <div class="info-row"><span class="label">Shipping</span><span class="value">${this._formatCurrency(this._order.shippingTotal)}</span></div>
              <div class="info-row"><span class="label">Tax</span><span class="value">${this._formatCurrency(this._order.taxTotal)}</span></div>
              <div class="info-row" style="font-weight: 600; border-top: 2px solid var(--uui-color-border); padding-top: 12px;">
                <span class="label">Grand Total</span><span class="value">${this._formatCurrency(this._order.grandTotal)}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    `;
  }

  _renderCustomerTab() {
    return html`
      <div class="property-group">
        <div class="property-group-header"><h3>Customer Information</h3></div>
        <div class="property-group-body">
          ${this._renderTextField('customerName', 'Customer Name', 'Full name of the customer', { required: true, placeholder: 'Enter customer name' })}
          ${this._renderTextField('customerEmail', 'Customer Email', 'Email address for order notifications', { required: true, placeholder: 'customer@example.com', type: 'email' })}
          ${this._renderTextField('customerPhone', 'Customer Phone', 'Contact phone number', { placeholder: '+1 (555) 000-0000', type: 'tel' })}
          ${this._renderTextField('customerId', 'Customer ID', 'Linked customer account (optional)', { placeholder: 'Customer account ID' })}
        </div>
      </div>
    `;
  }

  _renderLineItemsTab() {
    return html`
      <div class="property-group">
        <div class="property-group-header"><h3>Order Items</h3></div>
        <div class="property-group-body">
          ${this._errors.lines ? html`<div class="property-error" style="margin-bottom: 16px;">${this._errors.lines}</div>` : ''}
          <table class="line-items-table">
            <thead>
              <tr>
                <th>Product</th>
                <th>SKU</th>
                <th style="width: 100px;">Qty</th>
                <th style="width: 120px;">Unit Price</th>
                <th style="width: 120px;">Total</th>
                <th style="width: 80px;">Actions</th>
              </tr>
            </thead>
            <tbody>
              ${this._order.lines.map((line, index) => html`
                <tr>
                  <td>
                    <select .value=${line.productId} @change=${(e) => this._updateLineItem(index, 'productId', e.target.value)}>
                      <option value="">Select product...</option>
                      ${this._products.map(p => html`<option value="${p.id}" ?selected=${p.id === line.productId}>${p.name}</option>`)}
                    </select>
                  </td>
                  <td><input type="text" .value=${line.sku} @input=${(e) => this._updateLineItem(index, 'sku', e.target.value)} /></td>
                  <td><input type="number" .value=${line.quantity} @input=${(e) => this._updateLineItem(index, 'quantity', parseInt(e.target.value) || 0)} min="1" /></td>
                  <td><input type="number" .value=${line.unitPrice} @input=${(e) => this._updateLineItem(index, 'unitPrice', parseFloat(e.target.value) || 0)} step="0.01" min="0" /></td>
                  <td style="font-weight: 500;">${this._formatCurrency(line.totalPrice)}</td>
                  <td><button class="remove-btn" @click=${() => this._removeLineItem(index)}>Remove</button></td>
                </tr>
              `)}
            </tbody>
          </table>
          <button class="add-line-btn" @click=${this._addLineItem}>+ Add Line Item</button>

          <div class="order-totals">
            <div class="total-row"><span class="label">Subtotal</span><span class="value">${this._formatCurrency(this._order.subtotal)}</span></div>
            <div class="total-row"><span class="label">Discount</span><input type="number" .value=${this._order.discountTotal} @input=${(e) => this._handleNumberInput('discountTotal', e)} step="0.01" min="0" /></div>
            <div class="total-row"><span class="label">Shipping</span><input type="number" .value=${this._order.shippingTotal} @input=${(e) => this._handleNumberInput('shippingTotal', e)} step="0.01" min="0" /></div>
            <div class="total-row"><span class="label">Tax</span><input type="number" .value=${this._order.taxTotal} @input=${(e) => this._handleNumberInput('taxTotal', e)} step="0.01" min="0" /></div>
            <div class="total-row grand-total"><span class="label">Grand Total</span><span class="value">${this._formatCurrency(this._order.grandTotal)}</span></div>
          </div>
        </div>
      </div>
    `;
  }

  _renderAddressForm(addressType) {
    const address = this._order[addressType] || {};
    const prefix = addressType;

    return html`
      <div class="address-form">
        <div><label>First Name</label><input type="text" class="form-input" .value=${address.firstName || ''} @input=${(e) => this._handleAddressInput(addressType, 'firstName', e)} /></div>
        <div><label>Last Name</label><input type="text" class="form-input" .value=${address.lastName || ''} @input=${(e) => this._handleAddressInput(addressType, 'lastName', e)} /></div>
        <div class="full-width"><label>Company</label><input type="text" class="form-input" .value=${address.company || ''} @input=${(e) => this._handleAddressInput(addressType, 'company', e)} /></div>
        <div class="full-width">
          <label>Address Line 1 <span class="required">*</span></label>
          <input type="text" class="form-input ${this._errors[`${prefix}.addressLine1`] ? 'error' : ''}" .value=${address.addressLine1 || ''} @input=${(e) => this._handleAddressInput(addressType, 'addressLine1', e)} />
          ${this._errors[`${prefix}.addressLine1`] ? html`<span class="property-error">${this._errors[`${prefix}.addressLine1`]}</span>` : ''}
        </div>
        <div class="full-width"><label>Address Line 2</label><input type="text" class="form-input" .value=${address.addressLine2 || ''} @input=${(e) => this._handleAddressInput(addressType, 'addressLine2', e)} /></div>
        <div>
          <label>City <span class="required">*</span></label>
          <input type="text" class="form-input ${this._errors[`${prefix}.city`] ? 'error' : ''}" .value=${address.city || ''} @input=${(e) => this._handleAddressInput(addressType, 'city', e)} />
        </div>
        <div><label>State / Province</label><input type="text" class="form-input" .value=${address.state || ''} @input=${(e) => this._handleAddressInput(addressType, 'state', e)} /></div>
        <div>
          <label>Postal Code <span class="required">*</span></label>
          <input type="text" class="form-input ${this._errors[`${prefix}.postalCode`] ? 'error' : ''}" .value=${address.postalCode || ''} @input=${(e) => this._handleAddressInput(addressType, 'postalCode', e)} />
        </div>
        <div>
          <label>Country <span class="required">*</span></label>
          <input type="text" class="form-input ${this._errors[`${prefix}.country`] ? 'error' : ''}" .value=${address.country || ''} @input=${(e) => this._handleAddressInput(addressType, 'country', e)} />
        </div>
        <div><label>Phone</label><input type="tel" class="form-input" .value=${address.phone || ''} @input=${(e) => this._handleAddressInput(addressType, 'phone', e)} /></div>
      </div>
    `;
  }

  _renderAddressesTab() {
    return html`
      <div class="property-group">
        <div class="property-group-header"><h3>Addresses</h3></div>
        <div class="property-group-body">
          <div class="address-grid">
            <div class="address-card">
              <h4>Shipping Address</h4>
              ${this._renderAddressForm('shippingAddress')}
            </div>
            <div class="address-card">
              <h4>Billing Address <button class="copy-btn" @click=${this._copyShippingToBilling}>Copy from Shipping</button></h4>
              <div class="checkbox-wrapper">
                <input type="checkbox" id="billingSameAsShipping" .checked=${this._order.billingSameAsShipping} @change=${(e) => this._handleCheckboxInput('billingSameAsShipping', e)} />
                <label for="billingSameAsShipping">Same as shipping address</label>
              </div>
              ${!this._order.billingSameAsShipping ? this._renderAddressForm('billingAddress') : ''}
            </div>
          </div>
        </div>
      </div>
    `;
  }

  _renderPaymentTab() {
    return html`
      <div class="property-group">
        <div class="property-group-header"><h3>Payment Information</h3></div>
        <div class="property-group-body">
          ${this._renderSelectField('paymentMethod', 'Payment Method', 'Method used for payment', OrderEditor.PAYMENT_METHODS, { emptyLabel: 'Select method...' })}
          ${this._renderTextField('paymentProvider', 'Payment Provider', 'e.g., Stripe, PayPal', { placeholder: 'Payment processor' })}
          ${this._renderTextField('paymentIntentId', 'Payment Intent ID', 'External payment reference', { placeholder: 'External reference ID' })}
        </div>
      </div>

      <div class="property-group">
        <div class="property-group-header"><h3>Payment Amounts</h3></div>
        <div class="property-group-body">
          <div class="info-cards">
            <div class="info-card">
              <div class="info-row"><span class="label">Grand Total</span><span class="value">${this._formatCurrency(this._order.grandTotal)}</span></div>
              ${this._renderPriceField('paidAmount', 'Paid Amount', '')}
              ${this._renderPriceField('refundedAmount', 'Refunded Amount', '')}
              <div class="info-row" style="font-weight: 600; border-top: 2px solid var(--uui-color-border); padding-top: 12px;">
                <span class="label">Balance Due</span>
                <span class="value">${this._formatCurrency(this._order.grandTotal - this._order.paidAmount + this._order.refundedAmount)}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    `;
  }

  _renderShippingTab() {
    return html`
      <div class="property-group">
        <div class="property-group-header"><h3>Shipping Information</h3></div>
        <div class="property-group-body">
          ${this._renderSelectField('shippingMethod', 'Shipping Method', 'Delivery method', OrderEditor.SHIPPING_METHODS, { emptyLabel: 'Select method...' })}
          ${this._renderTextField('carrier', 'Carrier', 'Shipping carrier', { placeholder: 'e.g., FedEx, UPS, USPS' })}
          ${this._renderTextField('trackingNumber', 'Tracking Number', 'Shipment tracking number', { placeholder: 'Enter tracking number' })}
        </div>
      </div>
    `;
  }

  _renderNotesTab() {
    return html`
      <div class="property-group">
        <div class="property-group-header"><h3>Order Notes</h3></div>
        <div class="property-group-body">
          <div class="notes-container">
            <div class="note-card">
              <h4>Customer Note</h4>
              <textarea .value=${this._order.customerNote || ''} @input=${(e) => this._handleTextInput('customerNote', e)} placeholder="Notes from the customer..."></textarea>
            </div>
            <div class="note-card">
              <h4>Internal Note</h4>
              <textarea .value=${this._order.internalNote || ''} @input=${(e) => this._handleTextInput('internalNote', e)} placeholder="Internal notes (not visible to customer)..."></textarea>
            </div>
          </div>
        </div>
      </div>
    `;
  }

  _renderTimelineTab() {
    const events = this._buildTimeline();

    return html`
      <div class="property-group">
        <div class="property-group-header"><h3>Order Timeline</h3></div>
        <div class="property-group-body">
          <div class="timeline">
            ${events.map(event => html`
              <div class="timeline-item ${event.completed ? 'completed' : ''} ${event.current ? 'current' : ''}">
                <div class="time">${event.time}</div>
                <div class="event">${event.title}</div>
              </div>
            `)}
          </div>
        </div>
      </div>
    `;
  }

  _buildTimeline() {
    const events = [];
    const statusOrder = ['placedAt', 'confirmedAt', 'paidAt', 'shippedAt', 'deliveredAt', 'completedAt'];
    const statusLabels = {
      placedAt: 'Order Placed',
      confirmedAt: 'Order Confirmed',
      paidAt: 'Payment Received',
      shippedAt: 'Order Shipped',
      deliveredAt: 'Order Delivered',
      completedAt: 'Order Completed'
    };

    let currentFound = false;

    statusOrder.forEach(status => {
      const timestamp = this._order[status];
      const completed = !!timestamp;
      const isCurrent = !currentFound && !completed;

      if (!completed && !currentFound) {
        currentFound = true;
      }

      events.push({
        title: statusLabels[status],
        time: timestamp ? this._formatDate(timestamp) : 'Pending',
        completed,
        current: isCurrent && events.some(e => e.completed)
      });
    });

    if (this._order.cancelledAt) {
      events.push({
        title: 'Order Cancelled',
        time: this._formatDate(this._order.cancelledAt),
        completed: true,
        current: false
      });
    }

    return events;
  }

  // ============================================
  // MAIN RENDER
  // ============================================

  render() {
    if (this._loading) {
      return html`<div class="loading-overlay"><uui-loader></uui-loader></div>`;
    }

    return html`
      <div class="editor-layout">
        <!-- Header with Algora Branding -->
        <div class="editor-header">
          <div class="header-title">
            <span class="algora-badge">
              <uui-icon name="icon-receipt-dollar"></uui-icon>
              Algora Order
            </span>
            <h1>${this._isNew ? 'New Order' : `#${this._order.orderNumber}`}</h1>
            ${!this._isNew && this._order.status ? html`
              <span class="status-badge ${this._getStatusBadgeClass(this._order.status)}">${this._order.status}</span>
            ` : ''}
          </div>
          <div class="header-actions">
            <button class="btn btn-secondary" @click=${this._handleCancel}>Cancel</button>
            <button class="btn btn-primary" @click=${this._handleSave} ?disabled=${this._saving}>
              ${this._saving ? 'Saving...' : this._isNew ? 'Create Order' : 'Save Changes'}
            </button>
          </div>
        </div>

        <div class="tabs-nav">
          ${OrderEditor.TABS.map(tab => html`
            <button
              class="tab-btn ${this._activeTab === tab.id ? 'active' : ''} ${this._getTabErrors(tab.id) ? 'has-error' : ''}"
              @click=${() => this._activeTab = tab.id}
            >
              ${tab.label}
            </button>
          `)}
        </div>

        <div class="editor-content">
          <div class="tab-panel ${this._activeTab === 'overview' ? 'active' : ''}">${this._renderOverviewTab()}</div>
          <div class="tab-panel ${this._activeTab === 'customer' ? 'active' : ''}">${this._renderCustomerTab()}</div>
          <div class="tab-panel ${this._activeTab === 'items' ? 'active' : ''}">${this._renderLineItemsTab()}</div>
          <div class="tab-panel ${this._activeTab === 'addresses' ? 'active' : ''}">${this._renderAddressesTab()}</div>
          <div class="tab-panel ${this._activeTab === 'payment' ? 'active' : ''}">${this._renderPaymentTab()}</div>
          <div class="tab-panel ${this._activeTab === 'shipping' ? 'active' : ''}">${this._renderShippingTab()}</div>
          <div class="tab-panel ${this._activeTab === 'notes' ? 'active' : ''}">${this._renderNotesTab()}</div>
          <div class="tab-panel ${this._activeTab === 'timeline' ? 'active' : ''}">${this._renderTimelineTab()}</div>
        </div>
      </div>

      ${this._saving ? html`<div class="loading-overlay"><uui-loader></uui-loader></div>` : ''}
      ${this._toast ? html`<div class="toast ${this._toast.type}">${this._toast.message}</div>` : ''}
    `;
  }
}

customElements.define('ecommerce-order-editor', OrderEditor);

export default OrderEditor;
