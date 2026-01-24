import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Algora Checkout Step Editor Component
 * Enterprise-grade editor for checkout steps with proper tab-based document type management.
 *
 * Architecture:
 * - Tab-based property groups matching Umbraco document type patterns
 * - Centralized state management with validation pipeline
 * - Reusable field rendering methods
 * - Configuration-driven design
 */
export class CheckoutStepEditor extends UmbElementMixin(LitElement) {
  // ============================================
  // BRANDING CONFIGURATION
  // ============================================
  static DOCUMENT_TYPE = {
    name: 'Checkout Step',
    prefix: 'Algora',
    icon: 'icon-checkbox',
    color: '#8b5cf6', // Purple theme
    colorLight: '#ede9fe'
  };

  // ============================================
  // CONFIGURATION
  // ============================================

  static TABS = [
    { id: 'general', label: 'General', icon: 'settings' },
    { id: 'content', label: 'Content', icon: 'document' },
    { id: 'validation', label: 'Validation', icon: 'check' },
    { id: 'advanced', label: 'Advanced', icon: 'code' }
  ];

  static VALIDATION_RULES = {
    stepName: { required: true, minLength: 2, maxLength: 50 },
    stepOrder: { required: true, type: 'integer', min: 1, max: 20 },
    stepTitle: { maxLength: 100 }
  };

  static STEP_TYPES = [
    { value: 'information', label: 'Information', icon: 'icon-user', description: 'Customer details' },
    { value: 'shipping', label: 'Shipping', icon: 'icon-truck', description: 'Delivery options' },
    { value: 'payment', label: 'Payment', icon: 'icon-credit-card', description: 'Payment method' },
    { value: 'review', label: 'Review', icon: 'icon-check', description: 'Order summary' },
    { value: 'custom', label: 'Custom', icon: 'icon-settings', description: 'Custom step' }
  ];

  static COMMON_VALIDATIONS = [
    { value: 'required', label: 'Required Fields' },
    { value: 'email', label: 'Valid Email' },
    { value: 'phone', label: 'Valid Phone' },
    { value: 'address', label: 'Complete Address' },
    { value: 'payment', label: 'Payment Valid' },
    { value: 'shipping', label: 'Shipping Selected' },
    { value: 'terms', label: 'Terms Accepted' }
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
      background: linear-gradient(135deg, #f5f3ff 0%, #ede9fe 100%);
      border-bottom: 3px solid #8b5cf6;
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
      background: linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%);
      border-radius: 6px;
      color: white;
      font-weight: 600;
      font-size: 12px;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      box-shadow: 0 2px 4px rgba(139, 92, 246, 0.3);
    }

    .algora-badge uui-icon {
      font-size: 16px;
    }

    .header-title h1 {
      margin: 0;
      font-size: 20px;
      font-weight: 600;
      color: #5b21b6;
    }

    .status-badge {
      padding: 4px 10px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: 500;
      text-transform: uppercase;
    }

    .status-badge.enabled { background: #d1fae5; color: #065f46; }
    .status-badge.disabled { background: #fee2e2; color: #991b1b; }
    .status-badge.required { background: #ede9fe; color: #5b21b6; }

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
    .btn:disabled { opacity: 0.6; cursor: not-allowed; }

    /* Tabs */
    .tabs-nav {
      display: flex;
      background: var(--uui-color-surface);
      border-bottom: 1px solid var(--uui-color-border);
      padding: 0 24px;
      flex-shrink: 0;
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
      max-width: 960px;
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

    .form-input.code {
      font-family: monospace;
      background: var(--uui-color-surface-alt);
    }

    /* Checkbox */
    .checkbox-wrapper {
      display: flex;
      align-items: center;
      gap: 10px;
    }

    .checkbox-wrapper input[type="checkbox"] {
      width: 18px;
      height: 18px;
      cursor: pointer;
      accent-color: var(--uui-color-selected);
    }

    .checkbox-wrapper span {
      font-size: 14px;
      color: var(--uui-color-text);
    }

    /* Step Type Selector */
    .step-type-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
      gap: 12px;
    }

    .step-type-card {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 20px 16px;
      border: 2px solid var(--uui-color-border);
      border-radius: 8px;
      cursor: pointer;
      transition: all 0.15s ease;
      text-align: center;
    }

    .step-type-card:hover {
      border-color: var(--uui-color-selected);
      background: var(--uui-color-surface-emphasis);
    }

    .step-type-card.selected {
      border-color: var(--uui-color-selected);
      background: var(--uui-color-selected);
      color: white;
    }

    .step-type-card uui-icon {
      font-size: 28px;
      margin-bottom: 8px;
    }

    .step-type-card .type-label {
      font-weight: 600;
      font-size: 14px;
    }

    .step-type-card .type-desc {
      font-size: 12px;
      opacity: 0.8;
      margin-top: 4px;
    }

    /* Tags */
    .tags-container {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
      padding: 12px;
      border: 1px solid var(--uui-color-border);
      border-radius: 4px;
      min-height: 60px;
      background: var(--uui-color-surface);
    }

    .tag {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 6px 10px;
      background: var(--uui-color-selected);
      color: white;
      border-radius: 4px;
      font-size: 13px;
    }

    .tag button {
      background: none;
      border: none;
      color: white;
      cursor: pointer;
      padding: 0;
      font-size: 14px;
      opacity: 0.8;
    }

    .tag button:hover { opacity: 1; }

    .tag-buttons {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
      margin-top: 12px;
    }

    .tag-buttons button {
      padding: 6px 12px;
      border: 1px solid var(--uui-color-border);
      border-radius: 4px;
      background: var(--uui-color-surface);
      cursor: pointer;
      font-size: 13px;
      transition: all 0.15s ease;
    }

    .tag-buttons button:hover {
      background: var(--uui-color-surface-emphasis);
      border-color: var(--uui-color-selected);
    }

    .tag-buttons button:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    /* Preview */
    .step-preview {
      padding: 20px;
      background: var(--uui-color-surface-alt);
      border-radius: 8px;
    }

    .preview-header {
      display: flex;
      align-items: center;
      gap: 12px;
      margin-bottom: 16px;
    }

    .preview-number {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      background: var(--uui-color-selected);
      color: white;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 600;
    }

    .preview-title {
      font-size: 18px;
      font-weight: 600;
    }

    .preview-content {
      padding: 24px;
      background: white;
      border-radius: 6px;
      text-align: center;
      color: var(--uui-color-text-alt);
    }

    .preview-content uui-icon {
      font-size: 48px;
      margin-bottom: 12px;
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
  `;

  // ============================================
  // PROPERTIES
  // ============================================

  static properties = {
    stepId: { type: String },
    _step: { type: Object, state: true },
    _activeTab: { type: String, state: true },
    _loading: { type: Boolean, state: true },
    _saving: { type: Boolean, state: true },
    _errors: { type: Object, state: true },
    _touched: { type: Object, state: true },
    _toast: { type: Object, state: true },
    _isNew: { type: Boolean, state: true }
  };

  // ============================================
  // LIFECYCLE
  // ============================================

  constructor() {
    super();
    this.stepId = null;
    this._step = this._getEmptyStep();
    this._activeTab = 'general';
    this._loading = true;
    this._saving = false;
    this._errors = {};
    this._touched = {};
    this._toast = null;
    this._isNew = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadData();
  }

  updated(changedProperties) {
    super.updated(changedProperties);
    if (changedProperties.has('stepId')) {
      const oldValue = changedProperties.get('stepId');
      if (oldValue !== this.stepId) {
        this._loadData();
      }
    }
  }

  // ============================================
  // DATA MODEL
  // ============================================

  _getEmptyStep() {
    return {
      id: null,
      stepName: '',
      stepType: 'information',
      stepOrder: 1,
      isRequired: true,
      isEnabled: true,
      stepTitle: '',
      stepDescription: '',
      helpText: '',
      errorMessage: '',
      validationRules: [],
      skipCondition: '',
      templatePath: '',
      icon: 'icon-user'
    };
  }

  // ============================================
  // DATA LOADING
  // ============================================

  async _loadData() {
    try {
      this._loading = true;

      if (this.stepId && this.stepId !== 'create') {
        await this._loadStep();
        this._isNew = false;
      } else {
        this._step = this._getEmptyStep();
        this._isNew = true;
      }
    } catch (error) {
      console.error('Error loading data:', error);
      this._showToast('Failed to load data', 'error');
    } finally {
      this._loading = false;
    }
  }

  async _loadStep() {
    const response = await fetch(`/umbraco/management/api/v1/ecommerce/checkout-step/${this.stepId}`, {
      credentials: 'include',
      headers: { 'Accept': 'application/json' }
    });

    if (!response.ok) throw new Error('Failed to load checkout step');
    const data = await response.json();
    this._step = { ...this._getEmptyStep(), ...data };
  }

  // ============================================
  // INPUT HANDLERS
  // ============================================

  _handleTextInput(field, event) {
    this._step = { ...this._step, [field]: event.target.value };
    this._touched = { ...this._touched, [field]: true };
    this._validateField(field);
  }

  _handleNumberInput(field, event) {
    const value = event.target.value;
    const numValue = value === '' ? 1 : parseInt(value, 10);
    this._step = { ...this._step, [field]: isNaN(numValue) ? 1 : numValue };
    this._touched = { ...this._touched, [field]: true };
    this._validateField(field);
  }

  _handleCheckboxInput(field, event) {
    this._step = { ...this._step, [field]: event.target.checked };
  }

  _handleStepTypeSelect(stepType) {
    const typeConfig = CheckoutStepEditor.STEP_TYPES.find(t => t.value === stepType);
    this._step = {
      ...this._step,
      stepType,
      icon: typeConfig?.icon || 'icon-settings',
      stepName: this._step.stepName || typeConfig?.label || ''
    };
  }

  _addValidationRule(rule) {
    if (rule && !this._step.validationRules.includes(rule)) {
      this._step = {
        ...this._step,
        validationRules: [...this._step.validationRules, rule]
      };
    }
  }

  _removeValidationRule(rule) {
    this._step = {
      ...this._step,
      validationRules: this._step.validationRules.filter(r => r !== rule)
    };
  }

  _setSkipCondition(condition) {
    this._step = { ...this._step, skipCondition: condition };
  }

  // ============================================
  // VALIDATION
  // ============================================

  _validateField(field) {
    const rules = CheckoutStepEditor.VALIDATION_RULES[field];
    if (!rules) return;

    const errors = { ...this._errors };
    const value = this._step[field];

    if (rules.required && (!value || (typeof value === 'string' && !value.trim()))) {
      errors[field] = `${this._getFieldLabel(field)} is required`;
    }
    else if (rules.minLength && value && value.length < rules.minLength) {
      errors[field] = `Must be at least ${rules.minLength} characters`;
    }
    else if (rules.maxLength && value && value.length > rules.maxLength) {
      errors[field] = `Must be less than ${rules.maxLength} characters`;
    }
    else if (rules.type === 'integer' && value !== null && value !== undefined) {
      if (!Number.isInteger(value)) {
        errors[field] = 'Must be a whole number';
      } else if (rules.min !== undefined && value < rules.min) {
        errors[field] = `Must be at least ${rules.min}`;
      } else if (rules.max !== undefined && value > rules.max) {
        errors[field] = `Must be at most ${rules.max}`;
      } else {
        delete errors[field];
      }
    }
    else {
      delete errors[field];
    }

    this._errors = errors;
  }

  _getFieldLabel(field) {
    const labels = {
      stepName: 'Step name',
      stepOrder: 'Step order',
      stepTitle: 'Display title'
    };
    return labels[field] || field;
  }

  _validateAll() {
    Object.keys(CheckoutStepEditor.VALIDATION_RULES).forEach(field => {
      this._touched[field] = true;
      this._validateField(field);
    });
    this._touched = { ...this._touched };
    return Object.keys(this._errors).length === 0;
  }

  _getTabErrors(tabId) {
    const tabFields = {
      general: ['stepName', 'stepOrder'],
      content: ['stepTitle']
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
      for (const tab of CheckoutStepEditor.TABS) {
        if (this._getTabErrors(tab.id)) {
          this._activeTab = tab.id;
          break;
        }
      }
      return;
    }

    try {
      this._saving = true;

      const url = this._isNew
        ? '/umbraco/management/api/v1/ecommerce/checkout-step'
        : `/umbraco/management/api/v1/ecommerce/checkout-step/${this.stepId}`;

      const response = await fetch(url, {
        method: this._isNew ? 'POST' : 'PUT',
        credentials: 'include',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(this._step)
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || 'Failed to save checkout step');
      }

      const savedStep = await response.json();
      this._step = savedStep;
      this._isNew = false;
      this.stepId = savedStep.id;

      this._showToast('Checkout step saved successfully', 'success');

      this.dispatchEvent(new CustomEvent('step-saved', {
        bubbles: true,
        composed: true,
        detail: { step: savedStep }
      }));
    } catch (error) {
      console.error('Error saving checkout step:', error);
      this._showToast(error.message || 'Failed to save checkout step', 'error');
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
  // REUSABLE FIELD RENDERERS
  // ============================================

  _renderTextField(field, label, description, options = {}) {
    const { placeholder = '', required = false, isCode = false } = options;
    const hasError = this._touched[field] && this._errors[field];

    return html`
      <div class="property-row">
        <div class="property-label">
          <label class="${required ? 'required' : ''}">${label}</label>
          ${description ? html`<span class="property-description">${description}</span>` : ''}
        </div>
        <div class="property-editor">
          <input
            type="text"
            class="form-input ${hasError ? 'error' : ''} ${isCode ? 'code' : ''}"
            .value=${this._step[field] || ''}
            @input=${(e) => this._handleTextInput(field, e)}
            placeholder="${placeholder}"
          />
          ${hasError ? html`<span class="property-error">${this._errors[field]}</span>` : ''}
        </div>
      </div>
    `;
  }

  _renderTextareaField(field, label, description, options = {}) {
    const { placeholder = '', required = false, rows = 3 } = options;
    const hasError = this._touched[field] && this._errors[field];

    return html`
      <div class="property-row">
        <div class="property-label">
          <label class="${required ? 'required' : ''}">${label}</label>
          ${description ? html`<span class="property-description">${description}</span>` : ''}
        </div>
        <div class="property-editor">
          <textarea
            class="form-input ${hasError ? 'error' : ''}"
            .value=${this._step[field] || ''}
            @input=${(e) => this._handleTextInput(field, e)}
            placeholder="${placeholder}"
            rows="${rows}"
          ></textarea>
          ${hasError ? html`<span class="property-error">${this._errors[field]}</span>` : ''}
        </div>
      </div>
    `;
  }

  _renderNumberField(field, label, description, options = {}) {
    const { required = false, min = 1, max = 20 } = options;
    const hasError = this._touched[field] && this._errors[field];

    return html`
      <div class="property-row">
        <div class="property-label">
          <label class="${required ? 'required' : ''}">${label}</label>
          ${description ? html`<span class="property-description">${description}</span>` : ''}
        </div>
        <div class="property-editor">
          <input
            type="number"
            class="form-input ${hasError ? 'error' : ''}"
            .value=${this._step[field] ?? 1}
            @input=${(e) => this._handleNumberInput(field, e)}
            min="${min}"
            max="${max}"
            style="max-width: 120px;"
          />
          ${hasError ? html`<span class="property-error">${this._errors[field]}</span>` : ''}
        </div>
      </div>
    `;
  }

  _renderCheckboxField(field, label, description) {
    return html`
      <div class="property-row">
        <div class="property-label">
          <label>${label}</label>
          ${description ? html`<span class="property-description">${description}</span>` : ''}
        </div>
        <div class="property-editor">
          <div class="checkbox-wrapper">
            <input
              type="checkbox"
              id="${field}"
              .checked=${this._step[field] || false}
              @change=${(e) => this._handleCheckboxInput(field, e)}
            />
            <span>${this._step[field] ? 'Yes' : 'No'}</span>
          </div>
        </div>
      </div>
    `;
  }

  // ============================================
  // TAB CONTENT RENDERERS
  // ============================================

  _renderGeneralTab() {
    return html`
      <div class="property-group">
        <div class="property-group-header"><h3>Step Type</h3></div>
        <div class="property-group-body">
          <div class="property-row">
            <div class="property-label">
              <label>Select Type</label>
              <span class="property-description">Choose the purpose of this checkout step</span>
            </div>
            <div class="property-editor">
              <div class="step-type-grid">
                ${CheckoutStepEditor.STEP_TYPES.map(type => html`
                  <div
                    class="step-type-card ${this._step.stepType === type.value ? 'selected' : ''}"
                    @click=${() => this._handleStepTypeSelect(type.value)}
                  >
                    <uui-icon name="${type.icon}"></uui-icon>
                    <span class="type-label">${type.label}</span>
                    <span class="type-desc">${type.description}</span>
                  </div>
                `)}
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="property-group">
        <div class="property-group-header"><h3>Step Details</h3></div>
        <div class="property-group-body">
          ${this._renderTextField('stepName', 'Step Name', 'Internal name for this checkout step', { required: true, placeholder: 'e.g., Customer Information' })}
          ${this._renderNumberField('stepOrder', 'Step Order', 'Position in checkout flow (1 = first)', { required: true, min: 1, max: 20 })}
        </div>
      </div>

      <div class="property-group">
        <div class="property-group-header"><h3>Status</h3></div>
        <div class="property-group-body">
          ${this._renderCheckboxField('isEnabled', 'Enabled', 'Step is active in checkout flow')}
          ${this._renderCheckboxField('isRequired', 'Required', 'Customers cannot skip this step')}
        </div>
      </div>
    `;
  }

  _renderContentTab() {
    return html`
      <div class="property-group">
        <div class="property-group-header"><h3>Display Content</h3></div>
        <div class="property-group-body">
          ${this._renderTextField('stepTitle', 'Display Title', 'Title shown to customers (defaults to Step Name if empty)', { placeholder: 'e.g., Enter Your Information' })}
          ${this._renderTextareaField('stepDescription', 'Description', 'Instructions displayed at the top of this step', { placeholder: 'Brief instructions for customers...' })}
          ${this._renderTextareaField('helpText', 'Help Text', 'Displayed as a help tooltip or sidebar', { placeholder: 'Additional help or tips...', rows: 2 })}
        </div>
      </div>

      <div class="property-group">
        <div class="property-group-header"><h3>Error Messages</h3></div>
        <div class="property-group-body">
          ${this._renderTextareaField('errorMessage', 'Validation Error Message', 'Message shown when validation fails', { placeholder: 'Please complete all required fields before continuing', rows: 2 })}
        </div>
      </div>
    `;
  }

  _renderValidationTab() {
    return html`
      <div class="property-group">
        <div class="property-group-header"><h3>Validation Rules</h3></div>
        <div class="property-group-body">
          <div class="property-row">
            <div class="property-label">
              <label>Active Rules</label>
              <span class="property-description">Validation rules applied to this step</span>
            </div>
            <div class="property-editor">
              <div class="tags-container">
                ${this._step.validationRules.length === 0 ? html`
                  <span style="color: var(--uui-color-text-alt); font-size: 14px;">No rules added</span>
                ` : this._step.validationRules.map(rule => {
                  const ruleInfo = CheckoutStepEditor.COMMON_VALIDATIONS.find(r => r.value === rule);
                  return html`
                    <span class="tag">
                      ${ruleInfo?.label || rule}
                      <button @click=${() => this._removeValidationRule(rule)}>&times;</button>
                    </span>
                  `;
                })}
              </div>
              <div class="tag-buttons">
                ${CheckoutStepEditor.COMMON_VALIDATIONS.map(rule => html`
                  <button
                    ?disabled=${this._step.validationRules.includes(rule.value)}
                    @click=${() => this._addValidationRule(rule.value)}
                  >
                    + ${rule.label}
                  </button>
                `)}
              </div>
            </div>
          </div>
        </div>
      </div>
    `;
  }

  _renderAdvancedTab() {
    return html`
      <div class="property-group">
        <div class="property-group-header"><h3>Conditional Logic</h3></div>
        <div class="property-group-body">
          ${this._renderTextField('skipCondition', 'Skip Condition', 'JavaScript expression - step is skipped when true', { placeholder: 'e.g., cart.isDigitalOnly || cart.hasPickupOnly', isCode: true })}
          <div class="property-row">
            <div class="property-label">
              <label>Quick Conditions</label>
              <span class="property-description">Common skip conditions</span>
            </div>
            <div class="property-editor">
              <div class="tag-buttons">
                <button @click=${() => this._setSkipCondition('cart.isDigitalOnly')}>Digital Only</button>
                <button @click=${() => this._setSkipCondition('cart.hasPickupOnly')}>Pickup Only</button>
                <button @click=${() => this._setSkipCondition('customer.isGuest')}>Guest Checkout</button>
                <button @click=${() => this._setSkipCondition('cart.total === 0')}>Free Order</button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="property-group">
        <div class="property-group-header"><h3>Custom Template</h3></div>
        <div class="property-group-body">
          ${this._renderTextField('templatePath', 'Template Path', 'Leave empty to use default template for step type', { placeholder: '~/Views/Checkout/CustomShipping.cshtml', isCode: true })}
        </div>
      </div>

      <div class="property-group">
        <div class="property-group-header"><h3>Preview</h3></div>
        <div class="property-group-body">
          <div class="step-preview">
            <div class="preview-header">
              <div class="preview-number">${this._step.stepOrder}</div>
              <div class="preview-title">${this._step.stepTitle || this._step.stepName || 'Untitled Step'}</div>
              ${!this._step.isEnabled ? html`<span class="status-badge disabled">Disabled</span>` : ''}
            </div>
            ${this._step.stepDescription ? html`
              <p style="margin: 0 0 16px; color: var(--uui-color-text-alt);">${this._step.stepDescription}</p>
            ` : ''}
            <div class="preview-content">
              <uui-icon name="${CheckoutStepEditor.STEP_TYPES.find(t => t.value === this._step.stepType)?.icon || 'icon-settings'}"></uui-icon>
              <p>Step content will appear here</p>
            </div>
          </div>
        </div>
      </div>
    `;
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
              <uui-icon name="icon-checkbox"></uui-icon>
              Algora Checkout Step
            </span>
            <h1>${this._isNew ? 'New Step' : this._step.stepName || 'Untitled'}</h1>
            ${!this._isNew ? html`
              <span class="status-badge ${this._step.isEnabled ? 'enabled' : 'disabled'}">
                ${this._step.isEnabled ? 'Enabled' : 'Disabled'}
              </span>
              ${this._step.isRequired ? html`<span class="status-badge required">Required</span>` : ''}
            ` : ''}
          </div>
          <div class="header-actions">
            <button class="btn btn-secondary" @click=${this._handleCancel}>Cancel</button>
            <button class="btn btn-primary" @click=${this._handleSave} ?disabled=${this._saving}>
              ${this._saving ? 'Saving...' : this._isNew ? 'Create Step' : 'Save Changes'}
            </button>
          </div>
        </div>

        <div class="tabs-nav">
          ${CheckoutStepEditor.TABS.map(tab => html`
            <button
              class="tab-btn ${this._activeTab === tab.id ? 'active' : ''} ${this._getTabErrors(tab.id) ? 'has-error' : ''}"
              @click=${() => this._activeTab = tab.id}
            >
              ${tab.label}
            </button>
          `)}
        </div>

        <div class="editor-content">
          <div class="tab-panel ${this._activeTab === 'general' ? 'active' : ''}">${this._renderGeneralTab()}</div>
          <div class="tab-panel ${this._activeTab === 'content' ? 'active' : ''}">${this._renderContentTab()}</div>
          <div class="tab-panel ${this._activeTab === 'validation' ? 'active' : ''}">${this._renderValidationTab()}</div>
          <div class="tab-panel ${this._activeTab === 'advanced' ? 'active' : ''}">${this._renderAdvancedTab()}</div>
        </div>
      </div>

      ${this._saving ? html`<div class="loading-overlay"><uui-loader></uui-loader></div>` : ''}
      ${this._toast ? html`<div class="toast ${this._toast.type}">${this._toast.message}</div>` : ''}
    `;
  }
}

customElements.define('ecommerce-checkout-step-editor', CheckoutStepEditor);

export default CheckoutStepEditor;
