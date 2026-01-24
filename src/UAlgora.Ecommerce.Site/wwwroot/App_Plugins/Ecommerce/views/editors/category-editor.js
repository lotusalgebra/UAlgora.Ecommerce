import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Algora Category Editor Component
 * Enterprise-grade editor for categories with proper tab-based document type management.
 *
 * Architecture:
 * - Tab-based property groups matching Umbraco document type patterns
 * - Centralized state management with validation pipeline
 * - Reusable field rendering methods
 * - Configuration-driven design
 */
export class CategoryEditor extends UmbElementMixin(LitElement) {
  // ============================================
  // BRANDING CONFIGURATION
  // ============================================
  static DOCUMENT_TYPE = {
    name: 'Category',
    prefix: 'Algora',
    icon: 'icon-folder',
    color: '#3b82f6', // Blue theme
    colorLight: '#dbeafe'
  };

  // ============================================
  // CONFIGURATION
  // ============================================

  static TABS = [
    { id: 'content', label: 'Content', icon: 'document' },
    { id: 'media', label: 'Media', icon: 'picture' },
    { id: 'seo', label: 'SEO', icon: 'search' },
    { id: 'settings', label: 'Settings', icon: 'settings' }
  ];

  static VALIDATION_RULES = {
    name: { required: true, minLength: 2, maxLength: 100 },
    slug: { pattern: /^[a-z0-9-]*$/, maxLength: 100 },
    metaTitle: { maxLength: 70 },
    metaDescription: { maxLength: 160 },
    sortOrder: { type: 'integer', min: 0 }
  };

  // ============================================
  // STYLES
  // ============================================

  static styles = css`
    :host {
      display: block;
      height: 100%;
      background: var(--uui-color-background);
    }

    * {
      box-sizing: border-box;
    }

    /* Layout Structure */
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
      background: linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%);
      border-bottom: 3px solid #3b82f6;
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
      background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
      border-radius: 6px;
      color: white;
      font-weight: 600;
      font-size: 12px;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      box-shadow: 0 2px 4px rgba(59, 130, 246, 0.3);
    }

    .algora-badge uui-icon {
      font-size: 16px;
    }

    .header-title h1 {
      margin: 0;
      font-size: 20px;
      font-weight: 600;
      color: #1e40af;
    }

    .header-title .status-badge {
      padding: 4px 10px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: 500;
      text-transform: uppercase;
    }

    .status-badge.visible { background: #d1fae5; color: #065f46; }
    .status-badge.hidden { background: #fee2e2; color: #991b1b; }
    .status-badge.featured { background: #dbeafe; color: #1e40af; }

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

    .btn-secondary:hover {
      background: var(--uui-color-surface-emphasis);
    }

    .btn-primary {
      background: var(--uui-color-positive);
      color: white;
    }

    .btn-primary:hover {
      background: var(--uui-color-positive-emphasis);
    }

    .btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    /* Tabs Navigation */
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

    .tab-btn.has-error {
      color: var(--uui-color-danger);
    }

    .tab-btn.has-error.active {
      border-bottom-color: var(--uui-color-danger);
    }

    /* Content Area */
    .editor-content {
      flex: 1;
      overflow-y: auto;
      padding: 24px;
    }

    .tab-panel {
      display: none;
      max-width: 960px;
    }

    .tab-panel.active {
      display: block;
    }

    /* Property Groups (Sections) */
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

    .property-group-body {
      padding: 20px;
    }

    /* Property Row */
    .property-row {
      display: grid;
      grid-template-columns: 200px 1fr;
      gap: 20px;
      padding: 16px 0;
      border-bottom: 1px solid var(--uui-color-border);
    }

    .property-row:first-child {
      padding-top: 0;
    }

    .property-row:last-child {
      border-bottom: none;
      padding-bottom: 0;
    }

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
      display: flex;
      align-items: center;
      gap: 4px;
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

    .form-input.error {
      border-color: var(--uui-color-danger);
    }

    .form-input::placeholder {
      color: var(--uui-color-text-alt);
    }

    .form-input:disabled {
      background: var(--uui-color-surface-alt);
      cursor: not-allowed;
    }

    textarea.form-input {
      height: auto;
      min-height: 100px;
      padding: 12px;
      resize: vertical;
    }

    select.form-input {
      cursor: pointer;
    }

    /* Input Group */
    .input-group {
      display: flex;
    }

    .input-group-prepend,
    .input-group-append {
      display: flex;
      align-items: center;
      padding: 0 12px;
      background: var(--uui-color-surface-alt);
      border: 1px solid var(--uui-color-border);
      font-size: 14px;
      color: var(--uui-color-text-alt);
      font-weight: 500;
    }

    .input-group-prepend {
      border-right: none;
      border-radius: 4px 0 0 4px;
    }

    .input-group-append {
      border-left: none;
      border-radius: 0 4px 4px 0;
    }

    .input-group .form-input {
      border-radius: 0;
    }

    .input-group .form-input:first-child {
      border-radius: 4px 0 0 4px;
    }

    .input-group .form-input:last-child {
      border-radius: 0 4px 4px 0;
    }

    .input-group-prepend + .form-input {
      border-radius: 0 4px 4px 0;
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

    /* Image Picker */
    .image-picker-wrapper {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .image-preview {
      width: 200px;
      height: 150px;
      border-radius: 8px;
      overflow: hidden;
      background: var(--uui-color-surface-alt);
      border: 1px solid var(--uui-color-border);
    }

    .image-preview img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .image-placeholder {
      width: 100%;
      height: 100%;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      color: var(--uui-color-text-alt);
    }

    .image-placeholder uui-icon {
      font-size: 32px;
      margin-bottom: 8px;
    }

    /* Character Counter */
    .char-counter {
      font-size: 12px;
      color: var(--uui-color-text-alt);
      text-align: right;
    }

    .char-counter.warning {
      color: var(--uui-color-warning);
    }

    .char-counter.error {
      color: var(--uui-color-danger);
    }

    /* Hierarchy Display */
    .hierarchy-display {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 12px;
      background: var(--uui-color-surface-alt);
      border-radius: 4px;
      font-size: 14px;
    }

    .hierarchy-display uui-icon {
      color: var(--uui-color-text-alt);
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
    categoryId: { type: String },
    _category: { type: Object, state: true },
    _activeTab: { type: String, state: true },
    _loading: { type: Boolean, state: true },
    _saving: { type: Boolean, state: true },
    _errors: { type: Object, state: true },
    _touched: { type: Object, state: true },
    _parentCategories: { type: Array, state: true },
    _toast: { type: Object, state: true },
    _isNew: { type: Boolean, state: true }
  };

  // ============================================
  // LIFECYCLE
  // ============================================

  constructor() {
    super();
    this.categoryId = null;
    this._category = this._getEmptyCategory();
    this._activeTab = 'content';
    this._loading = true;
    this._saving = false;
    this._errors = {};
    this._touched = {};
    this._parentCategories = [];
    this._toast = null;
    this._isNew = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadData();
  }

  updated(changedProperties) {
    super.updated(changedProperties);
    if (changedProperties.has('categoryId')) {
      const oldValue = changedProperties.get('categoryId');
      if (oldValue !== this.categoryId) {
        this._loadData();
      }
    }
  }

  // ============================================
  // DATA MODEL
  // ============================================

  _getEmptyCategory() {
    return {
      id: null,
      name: '',
      slug: '',
      description: '',
      parentId: null,
      imageUrl: '',
      metaTitle: '',
      metaDescription: '',
      metaKeywords: '',
      isVisible: true,
      isFeatured: false,
      sortOrder: 0
    };
  }

  // ============================================
  // DATA LOADING
  // ============================================

  async _loadData() {
    try {
      this._loading = true;
      await this._loadParentCategories();

      if (this.categoryId && this.categoryId !== 'create') {
        await this._loadCategory();
        this._isNew = false;
      } else {
        this._category = this._getEmptyCategory();
        this._isNew = true;
      }
    } catch (error) {
      console.error('Error loading data:', error);
      this._showToast('Failed to load data', 'error');
    } finally {
      this._loading = false;
    }
  }

  async _loadCategory() {
    const response = await fetch(`/umbraco/management/api/v1/ecommerce/category/${this.categoryId}`, {
      credentials: 'include',
      headers: { 'Accept': 'application/json' }
    });

    if (!response.ok) throw new Error('Failed to load category');
    const data = await response.json();
    this._category = { ...this._getEmptyCategory(), ...data };
  }

  async _loadParentCategories() {
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/category?pageSize=100', {
        credentials: 'include',
        headers: { 'Accept': 'application/json' }
      });

      if (response.ok) {
        const data = await response.json();
        this._parentCategories = (data.items || []).filter(c => c.id !== this.categoryId);
      }
    } catch (error) {
      console.error('Error loading parent categories:', error);
    }
  }

  // ============================================
  // INPUT HANDLERS
  // ============================================

  _handleTextInput(field, event) {
    this._category = { ...this._category, [field]: event.target.value };
    this._touched = { ...this._touched, [field]: true };
    this._validateField(field);
  }

  _handleNumberInput(field, event) {
    const value = event.target.value;
    const numValue = value === '' ? 0 : parseInt(value, 10);
    this._category = { ...this._category, [field]: isNaN(numValue) ? 0 : numValue };
    this._touched = { ...this._touched, [field]: true };
    this._validateField(field);
  }

  _handleCheckboxInput(field, event) {
    this._category = { ...this._category, [field]: event.target.checked };
  }

  _handleSelectInput(field, event) {
    const value = event.target.value;
    this._category = { ...this._category, [field]: value || null };
    this._touched = { ...this._touched, [field]: true };
    this._validateField(field);
  }

  _generateSlug() {
    if (this._category.name) {
      const slug = this._category.name
        .toLowerCase()
        .replace(/[^a-z0-9]+/g, '-')
        .replace(/^-|-$/g, '');
      this._category = { ...this._category, slug };
      this._touched = { ...this._touched, slug: true };
      this._validateField('slug');
    }
  }

  // ============================================
  // VALIDATION
  // ============================================

  _validateField(field) {
    const rules = CategoryEditor.VALIDATION_RULES[field];
    if (!rules) return;

    const errors = { ...this._errors };
    const value = this._category[field];

    // Required
    if (rules.required && (!value || (typeof value === 'string' && !value.trim()))) {
      errors[field] = `${this._getFieldLabel(field)} is required`;
    }
    // Min Length
    else if (rules.minLength && value && value.length < rules.minLength) {
      errors[field] = `Must be at least ${rules.minLength} characters`;
    }
    // Max Length
    else if (rules.maxLength && value && value.length > rules.maxLength) {
      errors[field] = `Must be less than ${rules.maxLength} characters`;
    }
    // Pattern
    else if (rules.pattern && value && !rules.pattern.test(value)) {
      errors[field] = this._getPatternErrorMessage(field);
    }
    // Type: integer
    else if (rules.type === 'integer' && value !== null && value !== undefined) {
      if (!Number.isInteger(value)) {
        errors[field] = 'Must be a whole number';
      } else if (rules.min !== undefined && value < rules.min) {
        errors[field] = `Must be at least ${rules.min}`;
      }
    }
    else {
      delete errors[field];
    }

    this._errors = errors;
  }

  _getFieldLabel(field) {
    const labels = {
      name: 'Category name',
      slug: 'URL slug',
      metaTitle: 'Meta title',
      metaDescription: 'Meta description',
      sortOrder: 'Sort order'
    };
    return labels[field] || field;
  }

  _getPatternErrorMessage(field) {
    if (field === 'slug') {
      return 'Only lowercase letters, numbers, and hyphens allowed';
    }
    return 'Invalid format';
  }

  _validateAll() {
    Object.keys(CategoryEditor.VALIDATION_RULES).forEach(field => {
      this._touched[field] = true;
      this._validateField(field);
    });
    this._touched = { ...this._touched };
    return Object.keys(this._errors).length === 0;
  }

  _getTabErrors(tabId) {
    const tabFields = {
      content: ['name', 'slug'],
      seo: ['metaTitle', 'metaDescription']
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
      // Switch to first tab with errors
      for (const tab of CategoryEditor.TABS) {
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
        ? '/umbraco/management/api/v1/ecommerce/category'
        : `/umbraco/management/api/v1/ecommerce/category/${this.categoryId}`;

      const response = await fetch(url, {
        method: this._isNew ? 'POST' : 'PUT',
        credentials: 'include',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(this._category)
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || 'Failed to save category');
      }

      const savedCategory = await response.json();
      this._category = savedCategory;
      this._isNew = false;
      this.categoryId = savedCategory.id;

      this._showToast('Category saved successfully', 'success');

      this.dispatchEvent(new CustomEvent('category-saved', {
        bubbles: true,
        composed: true,
        detail: { category: savedCategory }
      }));
    } catch (error) {
      console.error('Error saving category:', error);
      this._showToast(error.message || 'Failed to save category', 'error');
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
    const { placeholder = '', required = false } = options;
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
            class="form-input ${hasError ? 'error' : ''}"
            .value=${this._category[field] || ''}
            @input=${(e) => this._handleTextInput(field, e)}
            placeholder="${placeholder}"
          />
          ${hasError ? html`<span class="property-error">${this._errors[field]}</span>` : ''}
        </div>
      </div>
    `;
  }

  _renderTextareaField(field, label, description, options = {}) {
    const { placeholder = '', required = false, rows = 4, maxLength = 0 } = options;
    const hasError = this._touched[field] && this._errors[field];
    const value = this._category[field] || '';
    const charCount = value.length;

    return html`
      <div class="property-row">
        <div class="property-label">
          <label class="${required ? 'required' : ''}">${label}</label>
          ${description ? html`<span class="property-description">${description}</span>` : ''}
        </div>
        <div class="property-editor">
          <textarea
            class="form-input ${hasError ? 'error' : ''}"
            .value=${value}
            @input=${(e) => this._handleTextInput(field, e)}
            placeholder="${placeholder}"
            rows="${rows}"
          ></textarea>
          ${maxLength > 0 ? html`
            <span class="char-counter ${charCount > maxLength ? 'error' : charCount > maxLength * 0.9 ? 'warning' : ''}">
              ${charCount}/${maxLength}
            </span>
          ` : ''}
          ${hasError ? html`<span class="property-error">${this._errors[field]}</span>` : ''}
        </div>
      </div>
    `;
  }

  _renderNumberField(field, label, description, options = {}) {
    const { required = false, min = 0, placeholder = '0' } = options;
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
            .value=${this._category[field] ?? 0}
            @input=${(e) => this._handleNumberInput(field, e)}
            min="${min}"
            placeholder="${placeholder}"
            style="max-width: 150px;"
          />
          ${hasError ? html`<span class="property-error">${this._errors[field]}</span>` : ''}
        </div>
      </div>
    `;
  }

  _renderSelectField(field, label, description, selectOptions, options = {}) {
    const { required = false, emptyLabel = 'Select...' } = options;
    const hasError = this._touched[field] && this._errors[field];

    return html`
      <div class="property-row">
        <div class="property-label">
          <label class="${required ? 'required' : ''}">${label}</label>
          ${description ? html`<span class="property-description">${description}</span>` : ''}
        </div>
        <div class="property-editor">
          <select
            class="form-input ${hasError ? 'error' : ''}"
            .value=${this._category[field] || ''}
            @change=${(e) => this._handleSelectInput(field, e)}
          >
            <option value="">${emptyLabel}</option>
            ${selectOptions.map(opt => html`
              <option value="${opt.value}" ?selected=${this._category[field] === opt.value}>${opt.label}</option>
            `)}
          </select>
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
              .checked=${this._category[field] || false}
              @change=${(e) => this._handleCheckboxInput(field, e)}
            />
            <span>${this._category[field] ? 'Yes' : 'No'}</span>
          </div>
        </div>
      </div>
    `;
  }

  _renderSlugField() {
    const hasError = this._touched.slug && this._errors.slug;

    return html`
      <div class="property-row">
        <div class="property-label">
          <label>URL Slug</label>
          <span class="property-description">URL-friendly identifier for this category</span>
        </div>
        <div class="property-editor">
          <div class="input-group">
            <span class="input-group-prepend">/category/</span>
            <input
              type="text"
              class="form-input ${hasError ? 'error' : ''}"
              .value=${this._category.slug || ''}
              @input=${(e) => this._handleTextInput('slug', e)}
              placeholder="category-slug"
            />
            <button class="btn btn-secondary" @click=${this._generateSlug} style="border-radius: 0 4px 4px 0; border-left: none;">
              Generate
            </button>
          </div>
          ${hasError ? html`<span class="property-error">${this._errors.slug}</span>` : ''}
        </div>
      </div>
    `;
  }

  _renderImageField() {
    return html`
      <div class="property-row">
        <div class="property-label">
          <label>Category Image</label>
          <span class="property-description">Main image displayed for this category</span>
        </div>
        <div class="property-editor">
          <div class="image-picker-wrapper">
            <div class="image-preview">
              ${this._category.imageUrl ? html`
                <img src="${this._category.imageUrl}" alt="Category image" @error=${(e) => e.target.style.display = 'none'} />
              ` : html`
                <div class="image-placeholder">
                  <uui-icon name="icon-picture"></uui-icon>
                  <span>No image</span>
                </div>
              `}
            </div>
            <input
              type="text"
              class="form-input"
              .value=${this._category.imageUrl || ''}
              @input=${(e) => this._handleTextInput('imageUrl', e)}
              placeholder="https://example.com/image.jpg"
            />
          </div>
        </div>
      </div>
    `;
  }

  // ============================================
  // TAB CONTENT RENDERERS
  // ============================================

  _renderContentTab() {
    const parentOptions = this._parentCategories.map(c => ({ value: c.id, label: c.name }));

    return html`
      <div class="property-group">
        <div class="property-group-header"><h3>Basic Information</h3></div>
        <div class="property-group-body">
          ${this._renderTextField('name', 'Category Name', 'The name displayed to customers', { required: true, placeholder: 'Enter category name' })}
          ${this._renderSlugField()}
          ${this._renderSelectField('parentId', 'Parent Category', 'Create a category hierarchy', parentOptions, { emptyLabel: 'None (Top Level)' })}
        </div>
      </div>

      <div class="property-group">
        <div class="property-group-header"><h3>Description</h3></div>
        <div class="property-group-body">
          ${this._renderTextareaField('description', 'Category Description', 'Detailed description shown on the category page', { placeholder: 'Enter category description...', rows: 5 })}
        </div>
      </div>

      <div class="property-group">
        <div class="property-group-header"><h3>Display Order</h3></div>
        <div class="property-group-body">
          ${this._renderNumberField('sortOrder', 'Sort Order', 'Lower numbers appear first in navigation', { min: 0 })}
        </div>
      </div>
    `;
  }

  _renderMediaTab() {
    return html`
      <div class="property-group">
        <div class="property-group-header"><h3>Category Image</h3></div>
        <div class="property-group-body">
          ${this._renderImageField()}
        </div>
      </div>
    `;
  }

  _renderSeoTab() {
    return html`
      <div class="property-group">
        <div class="property-group-header"><h3>Search Engine Optimization</h3></div>
        <div class="property-group-body">
          ${this._renderTextareaField('metaTitle', 'Meta Title', 'Page title for search engines (recommended: under 70 characters)', { placeholder: 'SEO page title', rows: 2, maxLength: 70 })}
          ${this._renderTextareaField('metaDescription', 'Meta Description', 'Brief description for search results (recommended: under 160 characters)', { placeholder: 'Brief description for search engines...', rows: 3, maxLength: 160 })}
          ${this._renderTextField('metaKeywords', 'Meta Keywords', 'Comma-separated keywords (optional)', { placeholder: 'keyword1, keyword2, keyword3' })}
        </div>
      </div>
    `;
  }

  _renderSettingsTab() {
    return html`
      <div class="property-group">
        <div class="property-group-header"><h3>Visibility</h3></div>
        <div class="property-group-body">
          ${this._renderCheckboxField('isVisible', 'Visible', 'Show this category in navigation and listings')}
          ${this._renderCheckboxField('isFeatured', 'Featured', 'Highlight this category on the homepage or special sections')}
        </div>
      </div>
    `;
  }

  // ============================================
  // MAIN RENDER
  // ============================================

  render() {
    if (this._loading) {
      return html`
        <div class="loading-overlay">
          <uui-loader></uui-loader>
        </div>
      `;
    }

    return html`
      <div class="editor-layout">
        <!-- Header with Algora Branding -->
        <div class="editor-header">
          <div class="header-title">
            <span class="algora-badge">
              <uui-icon name="icon-folder"></uui-icon>
              Algora Category
            </span>
            <h1>${this._isNew ? 'New Category' : this._category.name || 'Untitled'}</h1>
            ${!this._isNew ? html`
              <span class="status-badge ${this._category.isVisible ? 'visible' : 'hidden'}">
                ${this._category.isVisible ? 'Visible' : 'Hidden'}
              </span>
              ${this._category.isFeatured ? html`
                <span class="status-badge featured">Featured</span>
              ` : ''}
            ` : ''}
          </div>
          <div class="header-actions">
            <button class="btn btn-secondary" @click=${this._handleCancel}>Cancel</button>
            <button class="btn btn-primary" @click=${this._handleSave} ?disabled=${this._saving}>
              ${this._saving ? 'Saving...' : this._isNew ? 'Create Category' : 'Save Changes'}
            </button>
          </div>
        </div>

        <!-- Tabs -->
        <div class="tabs-nav">
          ${CategoryEditor.TABS.map(tab => html`
            <button
              class="tab-btn ${this._activeTab === tab.id ? 'active' : ''} ${this._getTabErrors(tab.id) ? 'has-error' : ''}"
              @click=${() => this._activeTab = tab.id}
            >
              ${tab.label}
            </button>
          `)}
        </div>

        <!-- Content -->
        <div class="editor-content">
          <div class="tab-panel ${this._activeTab === 'content' ? 'active' : ''}">${this._renderContentTab()}</div>
          <div class="tab-panel ${this._activeTab === 'media' ? 'active' : ''}">${this._renderMediaTab()}</div>
          <div class="tab-panel ${this._activeTab === 'seo' ? 'active' : ''}">${this._renderSeoTab()}</div>
          <div class="tab-panel ${this._activeTab === 'settings' ? 'active' : ''}">${this._renderSettingsTab()}</div>
        </div>
      </div>

      ${this._saving ? html`<div class="loading-overlay"><uui-loader></uui-loader></div>` : ''}
      ${this._toast ? html`<div class="toast ${this._toast.type}">${this._toast.message}</div>` : ''}
    `;
  }
}

customElements.define('ecommerce-category-editor', CategoryEditor);

export default CategoryEditor;
