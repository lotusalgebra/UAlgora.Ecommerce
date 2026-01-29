import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";

/**
 * Invoice Template Collection with Inline Editor
 * Manages invoice and packing slip templates with customizable styling and content.
 */
export class InvoiceTemplateCollection extends UmbElementMixin(LitElement) {
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
      width: 350px;
      min-width: 350px;
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

    .btn-create {
      padding: 10px 16px;
      background: linear-gradient(135deg, #667eea, #764ba2);
      color: white;
      border: none;
      border-radius: 8px;
      font-weight: 500;
      cursor: pointer;
      font-size: 13px;
      display: flex;
      align-items: center;
      gap: 6px;
      transition: all 0.2s;
    }

    .btn-create:hover {
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(102, 126, 234, 0.35);
    }

    .btn-seed {
      padding: 10px 16px;
      background: #f5f5f5;
      color: #666;
      border: 1px solid #e0e0e0;
      border-radius: 8px;
      font-weight: 500;
      cursor: pointer;
      font-size: 13px;
      transition: all 0.2s;
    }

    .btn-seed:hover {
      background: #e8e8e8;
    }

    /* Type Filter */
    .type-filters {
      display: flex;
      gap: 6px;
      padding: 12px 20px;
      border-bottom: 1px solid #e0e0e0;
      flex-wrap: wrap;
    }

    .type-chip {
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

    .type-chip:hover { background: #f5f5f5; }
    .type-chip.active { border-color: transparent; }
    .type-chip.all.active { background: #1b264f; color: #fff; }
    .type-chip.invoice.active { background: #3b82f6; color: #fff; }
    .type-chip.packagingslip.active { background: #22c55e; color: #fff; }
    .type-chip.receipt.active { background: #f59e0b; color: #fff; }

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

    .template-icon.invoice { background: linear-gradient(135deg, #3b82f6, #2563eb); color: #fff; }
    .template-icon.packagingslip { background: linear-gradient(135deg, #22c55e, #16a34a); color: #fff; }
    .template-icon.receipt { background: linear-gradient(135deg, #f59e0b, #d97706); color: #fff; }
    .template-icon.creditnote { background: linear-gradient(135deg, #ef4444, #dc2626); color: #fff; }
    .template-icon.quotation { background: linear-gradient(135deg, #8b5cf6, #7c3aed); color: #fff; }

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
      color: #666;
      display: flex;
      gap: 8px;
      margin-top: 4px;
    }

    .template-badge {
      padding: 2px 8px;
      border-radius: 4px;
      font-size: 10px;
      font-weight: 600;
      text-transform: uppercase;
    }

    .badge-default { background: #22c55e; color: #fff; }
    .badge-inactive { background: #9ca3af; color: #fff; }

    /* Editor Panel */
    .editor-panel {
      flex: 1;
      display: flex;
      flex-direction: column;
      background: #fff;
      min-width: 0;
    }

    .editor-header {
      padding: 20px 24px;
      border-bottom: 1px solid #e0e0e0;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .editor-title {
      font-size: 18px;
      font-weight: 600;
      color: #1b264f;
    }

    .editor-actions {
      display: flex;
      gap: 8px;
    }

    .btn-save {
      padding: 10px 20px;
      background: linear-gradient(135deg, #22c55e, #16a34a);
      color: white;
      border: none;
      border-radius: 8px;
      font-weight: 500;
      cursor: pointer;
      font-size: 13px;
      transition: all 0.2s;
    }

    .btn-save:hover {
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(34, 197, 94, 0.35);
    }

    .btn-delete {
      padding: 10px 20px;
      background: #fff;
      color: #ef4444;
      border: 1px solid #ef4444;
      border-radius: 8px;
      font-weight: 500;
      cursor: pointer;
      font-size: 13px;
    }

    .btn-delete:hover {
      background: #fef2f2;
    }

    /* Tabs */
    .tabs {
      display: flex;
      border-bottom: 1px solid #e0e0e0;
      padding: 0 24px;
      background: #fafafa;
    }

    .tab {
      padding: 14px 20px;
      font-size: 13px;
      font-weight: 500;
      color: #666;
      cursor: pointer;
      border-bottom: 2px solid transparent;
      margin-bottom: -1px;
      transition: all 0.2s;
    }

    .tab:hover { color: #1b264f; }
    .tab.active {
      color: #667eea;
      border-bottom-color: #667eea;
      background: #fff;
    }

    /* Tab Content */
    .tab-content {
      flex: 1;
      overflow-y: auto;
      padding: 24px;
    }

    .form-section {
      margin-bottom: 28px;
    }

    .form-section-title {
      font-size: 14px;
      font-weight: 600;
      color: #1b264f;
      margin-bottom: 16px;
      padding-bottom: 8px;
      border-bottom: 1px solid #e0e0e0;
    }

    .form-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 16px;
    }

    .form-grid.three-col {
      grid-template-columns: repeat(3, 1fr);
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: 6px;
    }

    .form-group.full-width {
      grid-column: 1 / -1;
    }

    .form-group label {
      font-size: 12px;
      font-weight: 500;
      color: #444;
    }

    .form-group input,
    .form-group select,
    .form-group textarea {
      padding: 10px 12px;
      border: 1px solid #e0e0e0;
      border-radius: 8px;
      font-size: 14px;
      transition: all 0.2s;
    }

    .form-group input:focus,
    .form-group select:focus,
    .form-group textarea:focus {
      outline: none;
      border-color: #667eea;
      box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
    }

    .form-group textarea {
      min-height: 100px;
      resize: vertical;
    }

    /* Toggle Switch */
    .toggle-row {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 12px 0;
      border-bottom: 1px solid #f0f0f0;
    }

    .toggle-label {
      font-size: 14px;
      color: #333;
    }

    .toggle-desc {
      font-size: 12px;
      color: #666;
      margin-top: 2px;
    }

    .toggle-switch {
      position: relative;
      width: 44px;
      height: 24px;
    }

    .toggle-switch input {
      opacity: 0;
      width: 0;
      height: 0;
    }

    .toggle-slider {
      position: absolute;
      cursor: pointer;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background-color: #ccc;
      transition: 0.3s;
      border-radius: 24px;
    }

    .toggle-slider:before {
      position: absolute;
      content: "";
      height: 18px;
      width: 18px;
      left: 3px;
      bottom: 3px;
      background-color: white;
      transition: 0.3s;
      border-radius: 50%;
    }

    .toggle-switch input:checked + .toggle-slider {
      background: linear-gradient(135deg, #667eea, #764ba2);
    }

    .toggle-switch input:checked + .toggle-slider:before {
      transform: translateX(20px);
    }

    /* Color Picker */
    .color-picker-row {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .color-preview {
      width: 40px;
      height: 40px;
      border-radius: 8px;
      border: 2px solid #e0e0e0;
      cursor: pointer;
    }

    .color-input {
      flex: 1;
    }

    /* Preview Panel */
    .preview-container {
      border: 1px solid #e0e0e0;
      border-radius: 8px;
      overflow: hidden;
      background: #fff;
    }

    .preview-header {
      padding: 12px 16px;
      background: #f5f5f5;
      border-bottom: 1px solid #e0e0e0;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .preview-title {
      font-weight: 600;
      color: #1b264f;
    }

    .preview-content {
      padding: 20px;
      min-height: 400px;
    }

    .preview-mock {
      max-width: 600px;
      margin: 0 auto;
      border: 1px solid #e0e0e0;
      border-radius: 4px;
      overflow: hidden;
    }

    .preview-mock-header {
      padding: 20px;
      color: white;
    }

    .preview-mock-body {
      padding: 20px;
    }

    /* Empty State */
    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      height: 100%;
      color: #666;
      text-align: center;
      padding: 40px;
    }

    .empty-state-icon {
      font-size: 48px;
      margin-bottom: 16px;
      opacity: 0.5;
    }

    .empty-state-title {
      font-size: 18px;
      font-weight: 600;
      color: #1b264f;
      margin-bottom: 8px;
    }
  `;

  static properties = {
    _templates: { state: true },
    _selectedTemplate: { state: true },
    _activeTab: { state: true },
    _typeFilter: { state: true },
    _loading: { state: true }
  };

  constructor() {
    super();
    this._templates = [];
    this._selectedTemplate = null;
    this._activeTab = 'general';
    this._typeFilter = 'all';
    this._loading = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadTemplates();
  }

  async _loadTemplates() {
    this._loading = true;
    try {
      const response = await this._authFetch('/umbraco/management/api/v1/ecommerce/invoice/templates');
      if (response.ok) {
        const data = await response.json();
        this._templates = data.items || [];
      }
    } catch (error) {
      console.error('Failed to load templates:', error);
    }
    this._loading = false;
  }

  _getFilteredTemplates() {
    if (this._typeFilter === 'all') return this._templates;
    return this._templates.filter(t => t.templateType?.toLowerCase() === this._typeFilter);
  }

  _selectTemplate(template) {
    this._selectedTemplate = { ...template };
    this._activeTab = 'general';
  }

  _createNew() {
    this._selectedTemplate = {
      id: null,
      name: '',
      code: '',
      description: '',
      isDefault: false,
      isActive: true,
      templateType: 'Invoice',
      companyName: '',
      companyAddress: '',
      companyPhone: '',
      companyEmail: '',
      companyWebsite: '',
      taxId: '',
      logoUrl: '',
      primaryColor: '#1e3a5f',
      secondaryColor: '#424242',
      accentColor: '#2563eb',
      headerColor: '#1e3a5f',
      headerTextColor: '#ffffff',
      fontFamily: 'Arial, sans-serif',
      customCss: '',
      showLogo: true,
      showShippingAddress: true,
      showProductImages: false,
      showSku: true,
      showTaxBreakdown: true,
      showPaymentInstructions: true,
      showBarcode: false,
      showQrCode: false,
      // GST fields
      companyGstin: '',
      defaultPlaceOfSupply: '',
      supplyTypeCode: 'B2C',
      documentTypeCode: 'INV',
      generateIrn: false,
      showAmountInWords: true,
      showHsnSacCodes: true,
      // Signature
      signatureImageUrl: '',
      signatureLabel: 'Authorized Signatory',
      thankYouMessage: 'Thank you for your business!',
      // Default text
      defaultNotes: '',
      defaultTerms: 'Payment is due within 30 days of invoice date.',
      defaultFooter: '',
      defaultPaymentInstructions: '',
      // Numbering
      invoiceTitle: 'INVOICE',
      packingSlipTitle: 'PACKING SLIP',
      invoiceNumberPrefix: 'INV-',
      includeYearInNumber: true,
      numberPadding: 6,
      dateFormat: 'MMM dd, yyyy',
      paymentTermsDays: 30
    };
    this._activeTab = 'general';
  }

  async _saveTemplate() {
    if (!this._selectedTemplate.name) {
      alert('Please enter a template name');
      return;
    }

    const isNew = !this._selectedTemplate.id;
    const url = isNew
      ? '/umbraco/management/api/v1/ecommerce/invoice/templates'
      : `/umbraco/management/api/v1/ecommerce/invoice/templates/${this._selectedTemplate.id}`;

    try {
      const response = await this._authFetch(url, {
        method: isNew ? 'POST' : 'PUT',
        body: JSON.stringify(this._selectedTemplate)
      });

      if (response.ok) {
        await this._loadTemplates();
        const data = await response.json();
        this._selectedTemplate = data;
      } else {
        alert('Failed to save template');
      }
    } catch (error) {
      console.error('Failed to save template:', error);
      alert('Failed to save template');
    }
  }

  async _deleteTemplate() {
    if (!this._selectedTemplate?.id) return;
    if (!confirm('Are you sure you want to delete this template?')) return;

    try {
      const response = await this._authFetch(`/umbraco/management/api/v1/ecommerce/invoice/templates/${this._selectedTemplate.id}`, {
        method: 'DELETE'
      });

      if (response.ok) {
        this._selectedTemplate = null;
        await this._loadTemplates();
      }
    } catch (error) {
      console.error('Failed to delete template:', error);
    }
  }

  async _seedTemplates() {
    try {
      const response = await this._authFetch('/umbraco/management/api/v1/ecommerce/invoice/templates/seed', {
        method: 'POST'
      });
      if (response.ok) {
        await this._loadTemplates();
      }
    } catch (error) {
      console.error('Failed to seed templates:', error);
    }
  }

  _updateField(field, value) {
    this._selectedTemplate = { ...this._selectedTemplate, [field]: value };
  }

  _getTypeIcon(type) {
    const icons = {
      'invoice': '📄',
      'packagingslip': '📦',
      'receipt': '🧾',
      'creditnote': '↩️',
      'quotation': '💬'
    };
    return icons[type?.toLowerCase()] || '📄';
  }

  render() {
    return html`
      <div class="list-panel">
        <div class="list-header">
          <h2>Invoice Templates</h2>
          <div class="header-actions">
            <button class="btn-create" @click=${this._createNew}>
              + New Template
            </button>
            <button class="btn-seed" @click=${this._seedTemplates}>
              Seed Defaults
            </button>
          </div>
        </div>

        <div class="type-filters">
          ${['all', 'invoice', 'packagingslip', 'receipt'].map(type => html`
            <span
              class="type-chip ${type} ${this._typeFilter === type ? 'active' : ''}"
              @click=${() => this._typeFilter = type}
            >
              ${type === 'all' ? 'All' : type === 'packagingslip' ? 'Packing Slip' : type.charAt(0).toUpperCase() + type.slice(1)}
            </span>
          `)}
        </div>

        <div class="template-list">
          ${this._getFilteredTemplates().map(template => html`
            <div
              class="template-item ${this._selectedTemplate?.id === template.id ? 'selected' : ''}"
              @click=${() => this._selectTemplate(template)}
            >
              <div class="template-icon ${template.templateType?.toLowerCase() || 'invoice'}">
                ${this._getTypeIcon(template.templateType)}
              </div>
              <div class="template-info">
                <div class="template-name">${template.name}</div>
                <div class="template-meta">
                  <span>${template.templateType || 'Invoice'}</span>
                  ${template.isDefault ? html`<span class="template-badge badge-default">Default</span>` : ''}
                  ${!template.isActive ? html`<span class="template-badge badge-inactive">Inactive</span>` : ''}
                </div>
              </div>
            </div>
          `)}
        </div>
      </div>

      <div class="editor-panel">
        ${this._selectedTemplate ? this._renderEditor() : this._renderEmptyState()}
      </div>
    `;
  }

  _renderEmptyState() {
    return html`
      <div class="empty-state">
        <div class="empty-state-icon">📄</div>
        <div class="empty-state-title">No Template Selected</div>
        <p>Select a template from the list or create a new one to edit its settings.</p>
      </div>
    `;
  }

  _renderEditor() {
    const t = this._selectedTemplate;
    return html`
      <div class="editor-header">
        <div class="editor-title">
          ${t.id ? t.name : 'New Template'}
        </div>
        <div class="editor-actions">
          ${t.id ? html`<button class="btn-delete" @click=${this._deleteTemplate}>Delete</button>` : ''}
          <button class="btn-save" @click=${this._saveTemplate}>Save Template</button>
        </div>
      </div>

      <div class="tabs">
        ${['general', 'company', 'styling', 'content', 'gst', 'text', 'preview'].map(tab => html`
          <div
            class="tab ${this._activeTab === tab ? 'active' : ''}"
            @click=${() => this._activeTab = tab}
          >
            ${tab.charAt(0).toUpperCase() + tab.slice(1)}
          </div>
        `)}
      </div>

      <div class="tab-content">
        ${this._activeTab === 'general' ? this._renderGeneralTab() : ''}
        ${this._activeTab === 'company' ? this._renderCompanyTab() : ''}
        ${this._activeTab === 'styling' ? this._renderStylingTab() : ''}
        ${this._activeTab === 'content' ? this._renderContentTab() : ''}
        ${this._activeTab === 'gst' ? this._renderGstTab() : ''}
        ${this._activeTab === 'text' ? this._renderTextTab() : ''}
        ${this._activeTab === 'preview' ? this._renderPreviewTab() : ''}
      </div>
    `;
  }

  _renderGeneralTab() {
    const t = this._selectedTemplate;
    return html`
      <div class="form-section">
        <div class="form-section-title">Basic Information</div>
        <div class="form-grid">
          <div class="form-group">
            <label>Template Name *</label>
            <input type="text" .value=${t.name || ''}
              @input=${e => this._updateField('name', e.target.value)}
              placeholder="e.g., Standard Invoice" />
          </div>
          <div class="form-group">
            <label>Code</label>
            <input type="text" .value=${t.code || ''}
              @input=${e => this._updateField('code', e.target.value)}
              placeholder="e.g., standard-invoice" />
          </div>
          <div class="form-group">
            <label>Template Type</label>
            <select .value=${t.templateType || 'Invoice'}
              @change=${e => this._updateField('templateType', e.target.value)}>
              <option value="Invoice">Invoice</option>
              <option value="PackagingSlip">Packing Slip</option>
              <option value="Receipt">Receipt</option>
              <option value="CreditNote">Credit Note</option>
              <option value="Quotation">Quotation</option>
            </select>
          </div>
          <div class="form-group full-width">
            <label>Description</label>
            <textarea .value=${t.description || ''}
              @input=${e => this._updateField('description', e.target.value)}
              placeholder="Brief description of this template"></textarea>
          </div>
        </div>
      </div>

      <div class="form-section">
        <div class="form-section-title">Status</div>
        <div class="toggle-row">
          <div>
            <div class="toggle-label">Default Template</div>
            <div class="toggle-desc">Use this template by default for its type</div>
          </div>
          <label class="toggle-switch">
            <input type="checkbox" .checked=${t.isDefault}
              @change=${e => this._updateField('isDefault', e.target.checked)} />
            <span class="toggle-slider"></span>
          </label>
        </div>
        <div class="toggle-row">
          <div>
            <div class="toggle-label">Active</div>
            <div class="toggle-desc">Enable this template for use</div>
          </div>
          <label class="toggle-switch">
            <input type="checkbox" .checked=${t.isActive !== false}
              @change=${e => this._updateField('isActive', e.target.checked)} />
            <span class="toggle-slider"></span>
          </label>
        </div>
      </div>

      <div class="form-section">
        <div class="form-section-title">Invoice Numbering</div>
        <div class="form-grid three-col">
          <div class="form-group">
            <label>Invoice Number Prefix</label>
            <input type="text" .value=${t.invoiceNumberPrefix || 'INV-'}
              @input=${e => this._updateField('invoiceNumberPrefix', e.target.value)} />
          </div>
          <div class="form-group">
            <label>Number Padding</label>
            <input type="number" .value=${t.numberPadding || 6}
              @input=${e => this._updateField('numberPadding', parseInt(e.target.value))} />
          </div>
          <div class="form-group">
            <label>Payment Terms (Days)</label>
            <input type="number" .value=${t.paymentTermsDays || 30}
              @input=${e => this._updateField('paymentTermsDays', parseInt(e.target.value))} />
          </div>
        </div>
        <div class="toggle-row">
          <div>
            <div class="toggle-label">Include Year in Number</div>
            <div class="toggle-desc">e.g., INV-2024-000001</div>
          </div>
          <label class="toggle-switch">
            <input type="checkbox" .checked=${t.includeYearInNumber !== false}
              @change=${e => this._updateField('includeYearInNumber', e.target.checked)} />
            <span class="toggle-slider"></span>
          </label>
        </div>
      </div>
    `;
  }

  _renderCompanyTab() {
    const t = this._selectedTemplate;
    return html`
      <div class="form-section">
        <div class="form-section-title">Company Details</div>
        <div class="form-grid">
          <div class="form-group">
            <label>Company Name</label>
            <input type="text" .value=${t.companyName || ''}
              @input=${e => this._updateField('companyName', e.target.value)} />
          </div>
          <div class="form-group">
            <label>Logo URL</label>
            <input type="text" .value=${t.logoUrl || ''}
              @input=${e => this._updateField('logoUrl', e.target.value)}
              placeholder="https://..." />
          </div>
          <div class="form-group full-width">
            <label>Address</label>
            <textarea .value=${t.companyAddress || ''}
              @input=${e => this._updateField('companyAddress', e.target.value)}
              rows="2"></textarea>
          </div>
          <div class="form-group">
            <label>Phone</label>
            <input type="text" .value=${t.companyPhone || ''}
              @input=${e => this._updateField('companyPhone', e.target.value)} />
          </div>
          <div class="form-group">
            <label>Email</label>
            <input type="email" .value=${t.companyEmail || ''}
              @input=${e => this._updateField('companyEmail', e.target.value)} />
          </div>
          <div class="form-group">
            <label>Website</label>
            <input type="text" .value=${t.companyWebsite || ''}
              @input=${e => this._updateField('companyWebsite', e.target.value)} />
          </div>
          <div class="form-group">
            <label>Tax ID / VAT Number</label>
            <input type="text" .value=${t.taxId || ''}
              @input=${e => this._updateField('taxId', e.target.value)} />
          </div>
        </div>
      </div>

      <div class="form-section">
        <div class="form-section-title">Signature</div>
        <div class="form-grid">
          <div class="form-group">
            <label>Signature Image URL</label>
            <input type="text" .value=${t.signatureImageUrl || ''}
              @input=${e => this._updateField('signatureImageUrl', e.target.value)}
              placeholder="https://..." />
          </div>
          <div class="form-group">
            <label>Signature Label</label>
            <input type="text" .value=${t.signatureLabel || 'Authorized Signatory'}
              @input=${e => this._updateField('signatureLabel', e.target.value)} />
          </div>
        </div>
      </div>
    `;
  }

  _renderStylingTab() {
    const t = this._selectedTemplate;
    return html`
      <div class="form-section">
        <div class="form-section-title">Colors</div>
        <div class="form-grid">
          <div class="form-group">
            <label>Header Background Color</label>
            <div class="color-picker-row">
              <input type="color" class="color-preview" .value=${t.headerColor || '#1e3a5f'}
                @input=${e => this._updateField('headerColor', e.target.value)} />
              <input type="text" class="color-input" .value=${t.headerColor || '#1e3a5f'}
                @input=${e => this._updateField('headerColor', e.target.value)} />
            </div>
          </div>
          <div class="form-group">
            <label>Header Text Color</label>
            <div class="color-picker-row">
              <input type="color" class="color-preview" .value=${t.headerTextColor || '#ffffff'}
                @input=${e => this._updateField('headerTextColor', e.target.value)} />
              <input type="text" class="color-input" .value=${t.headerTextColor || '#ffffff'}
                @input=${e => this._updateField('headerTextColor', e.target.value)} />
            </div>
          </div>
          <div class="form-group">
            <label>Primary Color</label>
            <div class="color-picker-row">
              <input type="color" class="color-preview" .value=${t.primaryColor || '#1e3a5f'}
                @input=${e => this._updateField('primaryColor', e.target.value)} />
              <input type="text" class="color-input" .value=${t.primaryColor || '#1e3a5f'}
                @input=${e => this._updateField('primaryColor', e.target.value)} />
            </div>
          </div>
          <div class="form-group">
            <label>Accent Color</label>
            <div class="color-picker-row">
              <input type="color" class="color-preview" .value=${t.accentColor || '#2563eb'}
                @input=${e => this._updateField('accentColor', e.target.value)} />
              <input type="text" class="color-input" .value=${t.accentColor || '#2563eb'}
                @input=${e => this._updateField('accentColor', e.target.value)} />
            </div>
          </div>
        </div>
      </div>

      <div class="form-section">
        <div class="form-section-title">Typography</div>
        <div class="form-grid">
          <div class="form-group">
            <label>Font Family</label>
            <select .value=${t.fontFamily || 'Arial, sans-serif'}
              @change=${e => this._updateField('fontFamily', e.target.value)}>
              <option value="Arial, sans-serif">Arial</option>
              <option value="Helvetica, sans-serif">Helvetica</option>
              <option value="Georgia, serif">Georgia</option>
              <option value="Times New Roman, serif">Times New Roman</option>
              <option value="Courier New, monospace">Courier New</option>
            </select>
          </div>
          <div class="form-group">
            <label>Date Format</label>
            <select .value=${t.dateFormat || 'MMM dd, yyyy'}
              @change=${e => this._updateField('dateFormat', e.target.value)}>
              <option value="MMM dd, yyyy">Jan 15, 2024</option>
              <option value="dd/MM/yyyy">15/01/2024</option>
              <option value="MM/dd/yyyy">01/15/2024</option>
              <option value="yyyy-MM-dd">2024-01-15</option>
              <option value="dd MMM yyyy">15 Jan 2024</option>
            </select>
          </div>
        </div>
      </div>

      <div class="form-section">
        <div class="form-section-title">Custom CSS (Advanced)</div>
        <div class="form-group full-width">
          <textarea .value=${t.customCss || ''}
            @input=${e => this._updateField('customCss', e.target.value)}
            placeholder="/* Custom CSS styles */"
            style="font-family: monospace; min-height: 150px;"></textarea>
        </div>
      </div>
    `;
  }

  _renderContentTab() {
    const t = this._selectedTemplate;
    return html`
      <div class="form-section">
        <div class="form-section-title">Document Titles</div>
        <div class="form-grid">
          <div class="form-group">
            <label>Invoice Title</label>
            <input type="text" .value=${t.invoiceTitle || 'INVOICE'}
              @input=${e => this._updateField('invoiceTitle', e.target.value)} />
          </div>
          <div class="form-group">
            <label>Packing Slip Title</label>
            <input type="text" .value=${t.packingSlipTitle || 'PACKING SLIP'}
              @input=${e => this._updateField('packingSlipTitle', e.target.value)} />
          </div>
        </div>
      </div>

      <div class="form-section">
        <div class="form-section-title">Display Options</div>

        <div class="toggle-row">
          <div>
            <div class="toggle-label">Show Company Logo</div>
          </div>
          <label class="toggle-switch">
            <input type="checkbox" .checked=${t.showLogo !== false}
              @change=${e => this._updateField('showLogo', e.target.checked)} />
            <span class="toggle-slider"></span>
          </label>
        </div>

        <div class="toggle-row">
          <div>
            <div class="toggle-label">Show Shipping Address</div>
          </div>
          <label class="toggle-switch">
            <input type="checkbox" .checked=${t.showShippingAddress !== false}
              @change=${e => this._updateField('showShippingAddress', e.target.checked)} />
            <span class="toggle-slider"></span>
          </label>
        </div>

        <div class="toggle-row">
          <div>
            <div class="toggle-label">Show Product Images</div>
          </div>
          <label class="toggle-switch">
            <input type="checkbox" .checked=${t.showProductImages}
              @change=${e => this._updateField('showProductImages', e.target.checked)} />
            <span class="toggle-slider"></span>
          </label>
        </div>

        <div class="toggle-row">
          <div>
            <div class="toggle-label">Show SKU</div>
          </div>
          <label class="toggle-switch">
            <input type="checkbox" .checked=${t.showSku !== false}
              @change=${e => this._updateField('showSku', e.target.checked)} />
            <span class="toggle-slider"></span>
          </label>
        </div>

        <div class="toggle-row">
          <div>
            <div class="toggle-label">Show Tax Breakdown</div>
          </div>
          <label class="toggle-switch">
            <input type="checkbox" .checked=${t.showTaxBreakdown !== false}
              @change=${e => this._updateField('showTaxBreakdown', e.target.checked)} />
            <span class="toggle-slider"></span>
          </label>
        </div>

        <div class="toggle-row">
          <div>
            <div class="toggle-label">Show Payment Instructions</div>
          </div>
          <label class="toggle-switch">
            <input type="checkbox" .checked=${t.showPaymentInstructions !== false}
              @change=${e => this._updateField('showPaymentInstructions', e.target.checked)} />
            <span class="toggle-slider"></span>
          </label>
        </div>

        <div class="toggle-row">
          <div>
            <div class="toggle-label">Show Barcode</div>
          </div>
          <label class="toggle-switch">
            <input type="checkbox" .checked=${t.showBarcode}
              @change=${e => this._updateField('showBarcode', e.target.checked)} />
            <span class="toggle-slider"></span>
          </label>
        </div>

        <div class="toggle-row">
          <div>
            <div class="toggle-label">Show QR Code</div>
          </div>
          <label class="toggle-switch">
            <input type="checkbox" .checked=${t.showQrCode}
              @change=${e => this._updateField('showQrCode', e.target.checked)} />
            <span class="toggle-slider"></span>
          </label>
        </div>
      </div>
    `;
  }

  _renderGstTab() {
    const t = this._selectedTemplate;
    return html`
      <div class="form-section">
        <div class="form-section-title">GST Configuration (India)</div>
        <div class="form-grid">
          <div class="form-group">
            <label>Company GSTIN</label>
            <input type="text" .value=${t.companyGstin || ''}
              @input=${e => this._updateField('companyGstin', e.target.value)}
              placeholder="22AAAAA0000A1Z5" />
          </div>
          <div class="form-group">
            <label>Default Place of Supply</label>
            <input type="text" .value=${t.defaultPlaceOfSupply || ''}
              @input=${e => this._updateField('defaultPlaceOfSupply', e.target.value)}
              placeholder="e.g., Maharashtra (27)" />
          </div>
          <div class="form-group">
            <label>Supply Type Code</label>
            <select .value=${t.supplyTypeCode || 'B2C'}
              @change=${e => this._updateField('supplyTypeCode', e.target.value)}>
              <option value="B2B">B2B - Business to Business</option>
              <option value="B2C">B2C - Business to Consumer</option>
              <option value="SEZWP">SEZWP - SEZ with Payment</option>
              <option value="SEZWOP">SEZWOP - SEZ without Payment</option>
              <option value="EXPWP">EXPWP - Export with Payment</option>
              <option value="EXPWOP">EXPWOP - Export without Payment</option>
            </select>
          </div>
          <div class="form-group">
            <label>Document Type Code</label>
            <select .value=${t.documentTypeCode || 'INV'}
              @change=${e => this._updateField('documentTypeCode', e.target.value)}>
              <option value="INV">INV - Invoice</option>
              <option value="CRN">CRN - Credit Note</option>
              <option value="DBN">DBN - Debit Note</option>
            </select>
          </div>
        </div>
      </div>

      <div class="form-section">
        <div class="form-section-title">GST Display Options</div>

        <div class="toggle-row">
          <div>
            <div class="toggle-label">Generate IRN (e-Invoice)</div>
            <div class="toggle-desc">Generate Invoice Reference Number for e-invoicing compliance</div>
          </div>
          <label class="toggle-switch">
            <input type="checkbox" .checked=${t.generateIrn}
              @change=${e => this._updateField('generateIrn', e.target.checked)} />
            <span class="toggle-slider"></span>
          </label>
        </div>

        <div class="toggle-row">
          <div>
            <div class="toggle-label">Show Amount in Words</div>
            <div class="toggle-desc">Display total amount in words (Indian numbering)</div>
          </div>
          <label class="toggle-switch">
            <input type="checkbox" .checked=${t.showAmountInWords !== false}
              @change=${e => this._updateField('showAmountInWords', e.target.checked)} />
            <span class="toggle-slider"></span>
          </label>
        </div>

        <div class="toggle-row">
          <div>
            <div class="toggle-label">Show HSN/SAC Codes</div>
            <div class="toggle-desc">Display HSN/SAC codes for each line item</div>
          </div>
          <label class="toggle-switch">
            <input type="checkbox" .checked=${t.showHsnSacCodes !== false}
              @change=${e => this._updateField('showHsnSacCodes', e.target.checked)} />
            <span class="toggle-slider"></span>
          </label>
        </div>
      </div>
    `;
  }

  _renderTextTab() {
    const t = this._selectedTemplate;
    return html`
      <div class="form-section">
        <div class="form-section-title">Thank You Message</div>
        <div class="form-group full-width">
          <input type="text" .value=${t.thankYouMessage || 'Thank you for your business!'}
            @input=${e => this._updateField('thankYouMessage', e.target.value)} />
        </div>
      </div>

      <div class="form-section">
        <div class="form-section-title">Default Notes</div>
        <div class="form-group full-width">
          <textarea .value=${t.defaultNotes || ''}
            @input=${e => this._updateField('defaultNotes', e.target.value)}
            placeholder="Notes that appear on invoices"></textarea>
        </div>
      </div>

      <div class="form-section">
        <div class="form-section-title">Default Terms & Conditions</div>
        <div class="form-group full-width">
          <textarea .value=${t.defaultTerms || 'Payment is due within 30 days of invoice date.'}
            @input=${e => this._updateField('defaultTerms', e.target.value)}
            placeholder="Terms and conditions"></textarea>
        </div>
      </div>

      <div class="form-section">
        <div class="form-section-title">Default Footer</div>
        <div class="form-group full-width">
          <textarea .value=${t.defaultFooter || ''}
            @input=${e => this._updateField('defaultFooter', e.target.value)}
            placeholder="Footer text"></textarea>
        </div>
      </div>

      <div class="form-section">
        <div class="form-section-title">Default Payment Instructions</div>
        <div class="form-group full-width">
          <textarea .value=${t.defaultPaymentInstructions || ''}
            @input=${e => this._updateField('defaultPaymentInstructions', e.target.value)}
            placeholder="Bank details, payment methods, etc."></textarea>
        </div>
      </div>
    `;
  }

  _renderPreviewTab() {
    const t = this._selectedTemplate;
    return html`
      <div class="preview-container">
        <div class="preview-header">
          <span class="preview-title">Template Preview</span>
        </div>
        <div class="preview-content">
          <div class="preview-mock">
            <div class="preview-mock-header" style="background: ${t.headerColor || '#1e3a5f'}; color: ${t.headerTextColor || '#fff'}">
              <div style="display: flex; justify-content: space-between;">
                <div>
                  <strong style="font-size: 16px;">${t.companyName || 'Your Company Name'}</strong>
                  ${t.companyAddress ? html`<div style="font-size: 12px; margin-top: 4px;">${t.companyAddress}</div>` : ''}
                  ${t.companyGstin ? html`<div style="font-size: 11px; margin-top: 4px;">GSTIN: ${t.companyGstin}</div>` : ''}
                </div>
                <div style="text-align: right;">
                  <strong>Customer Name</strong>
                  <div style="font-size: 12px; margin-top: 4px;">123 Main Street</div>
                </div>
              </div>
            </div>
            <div class="preview-mock-body" style="font-family: ${t.fontFamily || 'Arial, sans-serif'}">
              <div style="display: flex; justify-content: space-between; margin-bottom: 20px;">
                <h2 style="margin: 0; color: ${t.primaryColor || '#1e3a5f'}; text-transform: lowercase; font-size: 28px;">
                  ${t.invoiceTitle?.toLowerCase() || 'invoice'}.
                </h2>
                <div style="text-align: right; font-size: 12px;">
                  <div>Invoice No.: INV-2024-000001</div>
                  <div>Date: Jan 15, 2024</div>
                </div>
              </div>

              <table style="width: 100%; border-collapse: collapse; margin: 20px 0; font-size: 12px;">
                <thead>
                  <tr style="border-bottom: 2px solid ${t.primaryColor || '#1e3a5f'};">
                    <th style="padding: 8px; text-align: left;">#</th>
                    <th style="padding: 8px; text-align: left;">Item Name</th>
                    <th style="padding: 8px; text-align: right;">Qty</th>
                    <th style="padding: 8px; text-align: right;">Price</th>
                    <th style="padding: 8px; text-align: right;">Amount</th>
                  </tr>
                </thead>
                <tbody>
                  <tr style="border-bottom: 1px solid #eee;">
                    <td style="padding: 8px;">1</td>
                    <td style="padding: 8px;">Sample Product</td>
                    <td style="padding: 8px; text-align: right;">2</td>
                    <td style="padding: 8px; text-align: right;">$50.00</td>
                    <td style="padding: 8px; text-align: right;">$100.00</td>
                  </tr>
                </tbody>
              </table>

              <div style="display: flex; justify-content: space-between; margin-top: 30px;">
                <div>
                  <div style="color: #666; font-size: 12px;">Signature</div>
                  <div style="width: 150px; border-bottom: 1px solid #666; margin-top: 40px;"></div>
                  <div style="font-size: 11px; color: #666; margin-top: 4px;">${t.signatureLabel || 'Authorized Signatory'}</div>
                  <div style="margin-top: 20px; color: ${t.accentColor || '#2563eb'}; font-weight: bold;">
                    ${t.thankYouMessage || 'Thank you for your business!'}
                  </div>
                </div>
                <div style="text-align: right;">
                  <div style="font-size: 12px; color: #666;">Total Due</div>
                  <div style="font-size: 28px; font-weight: bold; color: ${t.primaryColor || '#1e3a5f'};">$118.00</div>
                  ${t.showAmountInWords !== false ? html`<div style="font-size: 11px; color: #666; font-style: italic;">One Hundred Eighteen Dollars Only</div>` : ''}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    `;
  }
}

customElements.define('algora-invoicetemplate-collection', InvoiceTemplateCollection);
export default InvoiceTemplateCollection;
