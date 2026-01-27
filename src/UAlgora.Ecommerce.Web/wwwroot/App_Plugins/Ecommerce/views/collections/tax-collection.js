import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Tax Collection View - Inline Split-Pane Editor
 * Manages tax categories, zones, and rates with GST support
 */
export class TaxCollection extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      height: 100%;
    }

    .split-layout {
      display: grid;
      grid-template-columns: 350px 1fr;
      height: calc(100vh - 120px);
      gap: 0;
    }

    .list-panel {
      border-right: 1px solid var(--uui-color-border);
      display: flex;
      flex-direction: column;
      overflow: hidden;
    }

    .panel-header {
      padding: var(--uui-size-space-4);
      border-bottom: 1px solid var(--uui-color-border);
      background: var(--uui-color-surface);
    }

    .panel-header h2 {
      margin: 0 0 var(--uui-size-space-3) 0;
      font-size: var(--uui-type-h5-size);
    }

    .tab-buttons {
      display: flex;
      gap: var(--uui-size-space-2);
    }

    .tab-button {
      flex: 1;
      padding: var(--uui-size-space-3);
      border: 1px solid var(--uui-color-border);
      background: var(--uui-color-surface);
      cursor: pointer;
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
      transition: all 0.15s;
    }

    .tab-button:hover {
      background: var(--uui-color-surface-emphasis);
    }

    .tab-button.active {
      background: var(--uui-color-selected);
      color: var(--uui-color-selected-contrast);
      border-color: var(--uui-color-selected);
    }

    .list-content {
      flex: 1;
      overflow-y: auto;
      padding: var(--uui-size-space-3);
    }

    .list-item {
      padding: var(--uui-size-space-4);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      margin-bottom: var(--uui-size-space-2);
      cursor: pointer;
      transition: all 0.15s;
      background: var(--uui-color-surface);
    }

    .list-item:hover {
      border-color: var(--uui-color-border-emphasis);
      background: var(--uui-color-surface-emphasis);
    }

    .list-item.selected {
      border-color: var(--uui-color-selected);
      background: var(--uui-color-selected-emphasis);
    }

    .list-item-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--uui-size-space-2);
    }

    .list-item-name {
      font-weight: 600;
    }

    .list-item-code {
      font-family: monospace;
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
    }

    .badge {
      display: inline-block;
      padding: 2px 8px;
      border-radius: 12px;
      font-size: 11px;
      font-weight: 500;
    }

    .badge-active {
      background: var(--uui-color-positive-emphasis);
      color: var(--uui-color-positive);
    }

    .badge-inactive {
      background: var(--uui-color-surface-alt);
      color: var(--uui-color-text-alt);
    }

    .badge-default {
      background: var(--uui-color-warning-emphasis);
      color: var(--uui-color-warning);
      margin-left: var(--uui-size-space-2);
    }

    .badge-exempt {
      background: #e3f2fd;
      color: #1565c0;
      margin-left: var(--uui-size-space-2);
    }

    .badge-gst {
      background: #fff3e0;
      color: #e65100;
      margin-left: var(--uui-size-space-2);
    }

    .editor-panel {
      display: flex;
      flex-direction: column;
      overflow: hidden;
      background: var(--uui-color-surface);
    }

    .editor-header {
      padding: var(--uui-size-space-4) var(--uui-size-space-5);
      border-bottom: 1px solid var(--uui-color-border);
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .editor-title {
      font-size: var(--uui-type-h4-size);
      font-weight: 600;
      margin: 0;
    }

    .editor-actions {
      display: flex;
      gap: var(--uui-size-space-2);
    }

    .editor-content {
      flex: 1;
      overflow-y: auto;
      padding: var(--uui-size-space-5);
    }

    .editor-tabs {
      display: flex;
      gap: var(--uui-size-space-4);
      border-bottom: 1px solid var(--uui-color-border);
      padding: 0 var(--uui-size-space-5);
      background: var(--uui-color-surface-alt);
    }

    .editor-tab {
      padding: var(--uui-size-space-3) var(--uui-size-space-4);
      cursor: pointer;
      border-bottom: 2px solid transparent;
      font-weight: 500;
      color: var(--uui-color-text-alt);
      transition: all 0.15s;
    }

    .editor-tab:hover {
      color: var(--uui-color-text);
    }

    .editor-tab.active {
      border-bottom-color: var(--uui-color-current);
      color: var(--uui-color-current);
    }

    .form-group {
      margin-bottom: var(--uui-size-space-5);
    }

    .form-group label {
      display: block;
      font-weight: 500;
      margin-bottom: var(--uui-size-space-2);
    }

    .form-group small {
      display: block;
      color: var(--uui-color-text-alt);
      margin-top: var(--uui-size-space-1);
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: var(--uui-size-space-4);
    }

    .checkbox-row {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-3);
      padding: var(--uui-size-space-3);
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
      margin-bottom: var(--uui-size-space-2);
    }

    .checkbox-row label {
      margin: 0;
      font-weight: normal;
    }

    .rate-card {
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-space-4);
      margin-bottom: var(--uui-size-space-3);
      background: var(--uui-color-surface);
    }

    .rate-card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--uui-size-space-3);
    }

    .rate-value {
      font-size: var(--uui-type-h4-size);
      font-weight: 600;
      color: var(--uui-color-current);
    }

    .region-list {
      display: flex;
      flex-wrap: wrap;
      gap: var(--uui-size-space-2);
      margin-top: var(--uui-size-space-2);
    }

    .region-tag {
      display: inline-flex;
      align-items: center;
      padding: 4px 12px;
      background: var(--uui-color-surface-alt);
      border-radius: 16px;
      font-size: var(--uui-type-small-size);
    }

    .region-tag button {
      background: none;
      border: none;
      cursor: pointer;
      margin-left: 6px;
      padding: 0;
      color: var(--uui-color-text-alt);
    }

    .gst-section {
      background: #fff3e0;
      border: 1px solid #ffcc80;
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-space-4);
      margin-bottom: var(--uui-size-space-5);
    }

    .gst-section h4 {
      margin: 0 0 var(--uui-size-space-3) 0;
      color: #e65100;
    }

    .empty-state {
      text-align: center;
      padding: var(--uui-size-layout-3);
      color: var(--uui-color-text-alt);
    }

    .empty-state uui-icon {
      font-size: 48px;
      margin-bottom: var(--uui-size-space-4);
    }

    .loading {
      display: flex;
      justify-content: center;
      padding: var(--uui-size-layout-2);
    }
  `;

  static properties = {
    _activeTab: { type: String, state: true },
    _categories: { type: Array, state: true },
    _zones: { type: Array, state: true },
    _selectedCategory: { type: Object, state: true },
    _selectedZone: { type: Object, state: true },
    _editorTab: { type: String, state: true },
    _loading: { type: Boolean, state: true },
    _saving: { type: Boolean, state: true },
    _isNew: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._activeTab = 'categories';
    this._categories = [];
    this._zones = [];
    this._selectedCategory = null;
    this._selectedZone = null;
    this._editorTab = 'general';
    this._loading = true;
    this._saving = false;
    this._isNew = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadData();
  }

  async _loadData() {
    try {
      this._loading = true;
      const [categoriesResponse, zonesResponse] = await Promise.all([
        fetch('/umbraco/management/api/v1/ecommerce/tax/category?includeInactive=true'),
        fetch('/umbraco/management/api/v1/ecommerce/tax/zone?includeInactive=true')
      ]);

      if (categoriesResponse.ok) {
        const data = await categoriesResponse.json();
        this._categories = data.items || [];
      }

      if (zonesResponse.ok) {
        const data = await zonesResponse.json();
        this._zones = data.items || [];
      }
    } catch (error) {
      console.error('Error loading tax data:', error);
    } finally {
      this._loading = false;
    }
  }

  _selectTab(tab) {
    this._activeTab = tab;
    this._selectedCategory = null;
    this._selectedZone = null;
    this._isNew = false;
  }

  _selectCategory(category) {
    this._selectedCategory = { ...category };
    this._selectedZone = null;
    this._editorTab = 'general';
    this._isNew = false;
  }

  _selectZone(zone) {
    this._selectedZone = { ...zone };
    this._selectedCategory = null;
    this._editorTab = 'general';
    this._isNew = false;
  }

  _createNew() {
    this._isNew = true;
    this._editorTab = 'general';
    if (this._activeTab === 'categories') {
      this._selectedCategory = {
        name: '',
        code: '',
        description: '',
        isActive: true,
        isDefault: false,
        isTaxExempt: false,
        externalTaxCode: '',
        sortOrder: 0,
        // GST fields
        isGst: false,
        gstType: 'CGST+SGST',
        cgstRate: 0,
        sgstRate: 0,
        igstRate: 0,
        hsnCode: '',
        sacCode: ''
      };
      this._selectedZone = null;
    } else {
      this._selectedZone = {
        name: '',
        code: '',
        description: '',
        isActive: true,
        isDefault: false,
        priority: 0,
        sortOrder: 0,
        countries: [],
        states: [],
        postalCodePatterns: [],
        cities: [],
        excludedCountries: [],
        excludedStates: [],
        excludedPostalCodes: []
      };
      this._selectedCategory = null;
    }
  }

  async _saveCategory() {
    if (!this._selectedCategory) return;
    this._saving = true;
    try {
      const method = this._isNew ? 'POST' : 'PUT';
      const url = this._isNew
        ? '/umbraco/management/api/v1/ecommerce/tax/category'
        : `/umbraco/management/api/v1/ecommerce/tax/category/${this._selectedCategory.id}`;

      const response = await fetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(this._selectedCategory)
      });

      if (response.ok) {
        await this._loadData();
        if (this._isNew) {
          const created = await response.json();
          this._selectedCategory = created;
          this._isNew = false;
        }
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to save category');
      }
    } catch (error) {
      console.error('Error saving category:', error);
      alert('Failed to save category');
    } finally {
      this._saving = false;
    }
  }

  async _saveZone() {
    if (!this._selectedZone) return;
    this._saving = true;
    try {
      const method = this._isNew ? 'POST' : 'PUT';
      const url = this._isNew
        ? '/umbraco/management/api/v1/ecommerce/tax/zone'
        : `/umbraco/management/api/v1/ecommerce/tax/zone/${this._selectedZone.id}`;

      const response = await fetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(this._selectedZone)
      });

      if (response.ok) {
        await this._loadData();
        if (this._isNew) {
          const created = await response.json();
          this._selectedZone = created;
          this._isNew = false;
        }
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to save zone');
      }
    } catch (error) {
      console.error('Error saving zone:', error);
      alert('Failed to save zone');
    } finally {
      this._saving = false;
    }
  }

  async _deleteCategory() {
    if (!this._selectedCategory || this._isNew) return;
    if (!confirm('Are you sure you want to delete this category?')) return;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/tax/category/${this._selectedCategory.id}`, {
        method: 'DELETE'
      });
      if (response.ok) {
        this._selectedCategory = null;
        await this._loadData();
      }
    } catch (error) {
      console.error('Error deleting category:', error);
    }
  }

  async _deleteZone() {
    if (!this._selectedZone || this._isNew) return;
    if (!confirm('Are you sure you want to delete this zone?')) return;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/tax/zone/${this._selectedZone.id}`, {
        method: 'DELETE'
      });
      if (response.ok) {
        this._selectedZone = null;
        await this._loadData();
      }
    } catch (error) {
      console.error('Error deleting zone:', error);
    }
  }

  _addCountry(event) {
    if (event.key === 'Enter' && event.target.value.trim()) {
      const country = event.target.value.trim().toUpperCase();
      if (!this._selectedZone.countries.includes(country)) {
        this._selectedZone = {
          ...this._selectedZone,
          countries: [...this._selectedZone.countries, country]
        };
      }
      event.target.value = '';
    }
  }

  _removeCountry(country) {
    this._selectedZone = {
      ...this._selectedZone,
      countries: this._selectedZone.countries.filter(c => c !== country)
    };
  }

  _addState(event) {
    if (event.key === 'Enter' && event.target.value.trim()) {
      const state = event.target.value.trim().toUpperCase();
      if (!this._selectedZone.states.includes(state)) {
        this._selectedZone = {
          ...this._selectedZone,
          states: [...this._selectedZone.states, state]
        };
      }
      event.target.value = '';
    }
  }

  _removeState(state) {
    this._selectedZone = {
      ...this._selectedZone,
      states: this._selectedZone.states.filter(s => s !== state)
    };
  }

  render() {
    if (this._loading) {
      return html`<div class="loading"><uui-loader></uui-loader></div>`;
    }

    return html`
      <div class="split-layout">
        <div class="list-panel">
          ${this._renderListPanel()}
        </div>
        <div class="editor-panel">
          ${this._renderEditorPanel()}
        </div>
      </div>
    `;
  }

  _renderListPanel() {
    return html`
      <div class="panel-header">
        <h2>Tax Configuration</h2>
        <div class="tab-buttons">
          <button class="tab-button ${this._activeTab === 'categories' ? 'active' : ''}"
                  @click=${() => this._selectTab('categories')}>
            Categories
          </button>
          <button class="tab-button ${this._activeTab === 'zones' ? 'active' : ''}"
                  @click=${() => this._selectTab('zones')}>
            Zones
          </button>
        </div>
      </div>
      <div style="padding: var(--uui-size-space-3); border-bottom: 1px solid var(--uui-color-border);">
        <uui-button look="primary" style="width: 100%;" @click=${this._createNew}>
          <uui-icon name="icon-add"></uui-icon>
          Add ${this._activeTab === 'categories' ? 'Category' : 'Zone'}
        </uui-button>
      </div>
      <div class="list-content">
        ${this._activeTab === 'categories' ? this._renderCategoriesList() : this._renderZonesList()}
      </div>
    `;
  }

  _renderCategoriesList() {
    if (this._categories.length === 0) {
      return html`
        <div class="empty-state">
          <uui-icon name="icon-bill-dollar"></uui-icon>
          <h3>No tax categories</h3>
          <p>Create categories to classify products for tax purposes</p>
        </div>
      `;
    }

    return this._categories.map(category => html`
      <div class="list-item ${this._selectedCategory?.id === category.id ? 'selected' : ''}"
           @click=${() => this._selectCategory(category)}>
        <div class="list-item-header">
          <span class="list-item-name">${category.name}</span>
          <span class="badge ${category.isActive ? 'badge-active' : 'badge-inactive'}">
            ${category.isActive ? 'Active' : 'Inactive'}
          </span>
        </div>
        <div class="list-item-code">
          ${category.code}
          ${category.isDefault ? html`<span class="badge badge-default">Default</span>` : ''}
          ${category.isTaxExempt ? html`<span class="badge badge-exempt">Exempt</span>` : ''}
          ${category.isGst ? html`<span class="badge badge-gst">GST</span>` : ''}
        </div>
      </div>
    `);
  }

  _renderZonesList() {
    if (this._zones.length === 0) {
      return html`
        <div class="empty-state">
          <uui-icon name="icon-globe"></uui-icon>
          <h3>No tax zones</h3>
          <p>Create zones to define geographic tax regions</p>
        </div>
      `;
    }

    return this._zones.map(zone => html`
      <div class="list-item ${this._selectedZone?.id === zone.id ? 'selected' : ''}"
           @click=${() => this._selectZone(zone)}>
        <div class="list-item-header">
          <span class="list-item-name">${zone.name}</span>
          <span class="badge ${zone.isActive ? 'badge-active' : 'badge-inactive'}">
            ${zone.isActive ? 'Active' : 'Inactive'}
          </span>
        </div>
        <div class="list-item-code">
          ${zone.code}
          ${zone.isDefault ? html`<span class="badge badge-default">Default</span>` : ''}
          <span style="color: var(--uui-color-text-alt); margin-left: 8px;">
            ${this._getRegionCount(zone)} regions
          </span>
        </div>
      </div>
    `);
  }

  _getRegionCount(zone) {
    return (zone.countries?.length || 0) + (zone.states?.length || 0);
  }

  _renderEditorPanel() {
    if (!this._selectedCategory && !this._selectedZone) {
      return html`
        <div class="empty-state" style="margin-top: 100px;">
          <uui-icon name="icon-bill-dollar"></uui-icon>
          <h3>Select an item</h3>
          <p>Choose a tax category or zone from the list to edit</p>
        </div>
      `;
    }

    if (this._selectedCategory) {
      return this._renderCategoryEditor();
    } else {
      return this._renderZoneEditor();
    }
  }

  _renderCategoryEditor() {
    const category = this._selectedCategory;
    return html`
      <div class="editor-header">
        <h3 class="editor-title">${this._isNew ? 'New Category' : category.name}</h3>
        <div class="editor-actions">
          ${!this._isNew ? html`
            <uui-button look="secondary" color="danger" @click=${this._deleteCategory}>
              <uui-icon name="icon-delete"></uui-icon> Delete
            </uui-button>
          ` : ''}
          <uui-button look="primary" @click=${this._saveCategory} ?disabled=${this._saving}>
            ${this._saving ? html`<uui-loader-circle></uui-loader-circle>` : html`<uui-icon name="icon-check"></uui-icon>`}
            ${this._isNew ? 'Create' : 'Save'}
          </uui-button>
        </div>
      </div>

      <div class="editor-tabs">
        <div class="editor-tab ${this._editorTab === 'general' ? 'active' : ''}"
             @click=${() => this._editorTab = 'general'}>General</div>
        <div class="editor-tab ${this._editorTab === 'gst' ? 'active' : ''}"
             @click=${() => this._editorTab = 'gst'}>GST Settings</div>
      </div>

      <div class="editor-content">
        ${this._editorTab === 'general' ? this._renderCategoryGeneral() : this._renderCategoryGst()}
      </div>
    `;
  }

  _renderCategoryGeneral() {
    const category = this._selectedCategory;
    return html`
      <div class="form-row">
        <div class="form-group">
          <label>Name *</label>
          <uui-input style="width: 100%;"
                     .value=${category.name || ''}
                     @input=${e => this._selectedCategory = {...this._selectedCategory, name: e.target.value}}>
          </uui-input>
        </div>
        <div class="form-group">
          <label>Code *</label>
          <uui-input style="width: 100%;"
                     .value=${category.code || ''}
                     @input=${e => this._selectedCategory = {...this._selectedCategory, code: e.target.value}}>
          </uui-input>
          <small>Unique identifier for this category</small>
        </div>
      </div>

      <div class="form-group">
        <label>Description</label>
        <uui-textarea style="width: 100%;"
                      .value=${category.description || ''}
                      @input=${e => this._selectedCategory = {...this._selectedCategory, description: e.target.value}}>
        </uui-textarea>
      </div>

      <div class="form-group">
        <label>External Tax Code</label>
        <uui-input style="width: 100%;"
                   .value=${category.externalTaxCode || ''}
                   @input=${e => this._selectedCategory = {...this._selectedCategory, externalTaxCode: e.target.value}}>
        </uui-input>
        <small>For integration with external tax providers (Avalara, TaxJar, etc.)</small>
      </div>

      <div class="form-group">
        <label>Sort Order</label>
        <uui-input type="number" style="width: 120px;"
                   .value=${category.sortOrder || 0}
                   @input=${e => this._selectedCategory = {...this._selectedCategory, sortOrder: parseInt(e.target.value) || 0}}>
        </uui-input>
      </div>

      <div class="form-group">
        <label>Options</label>
        <div class="checkbox-row">
          <uui-toggle .checked=${category.isActive}
                      @change=${e => this._selectedCategory = {...this._selectedCategory, isActive: e.target.checked}}>
          </uui-toggle>
          <label>Active</label>
        </div>
        <div class="checkbox-row">
          <uui-toggle .checked=${category.isDefault}
                      @change=${e => this._selectedCategory = {...this._selectedCategory, isDefault: e.target.checked}}>
          </uui-toggle>
          <label>Default Category</label>
        </div>
        <div class="checkbox-row">
          <uui-toggle .checked=${category.isTaxExempt}
                      @change=${e => this._selectedCategory = {...this._selectedCategory, isTaxExempt: e.target.checked}}>
          </uui-toggle>
          <label>Tax Exempt</label>
        </div>
      </div>
    `;
  }

  _renderCategoryGst() {
    const category = this._selectedCategory;
    return html`
      <div class="gst-section">
        <h4>🇮🇳 GST Configuration (India)</h4>
        <p style="margin-bottom: var(--uui-size-space-4); color: var(--uui-color-text-alt);">
          Enable GST to automatically calculate CGST, SGST, and IGST based on transaction type.
        </p>

        <div class="checkbox-row" style="background: white;">
          <uui-toggle .checked=${category.isGst || false}
                      @change=${e => this._selectedCategory = {...this._selectedCategory, isGst: e.target.checked}}>
          </uui-toggle>
          <label>Enable GST for this category</label>
        </div>
      </div>

      ${category.isGst ? html`
        <div class="form-group">
          <label>GST Type</label>
          <uui-select style="width: 100%;"
                      .value=${category.gstType || 'CGST+SGST'}
                      @change=${e => this._selectedCategory = {...this._selectedCategory, gstType: e.target.value}}>
            <uui-select-option value="CGST+SGST">CGST + SGST (Intra-state)</uui-select-option>
            <uui-select-option value="IGST">IGST (Inter-state)</uui-select-option>
            <uui-select-option value="AUTO">Auto-detect based on address</uui-select-option>
          </uui-select>
          <small>CGST/SGST for same-state transactions, IGST for cross-state</small>
        </div>

        <div class="form-row">
          <div class="form-group">
            <label>CGST Rate (%)</label>
            <uui-input type="number" step="0.01" style="width: 100%;"
                       .value=${category.cgstRate || 0}
                       @input=${e => this._selectedCategory = {...this._selectedCategory, cgstRate: parseFloat(e.target.value) || 0}}>
            </uui-input>
          </div>
          <div class="form-group">
            <label>SGST Rate (%)</label>
            <uui-input type="number" step="0.01" style="width: 100%;"
                       .value=${category.sgstRate || 0}
                       @input=${e => this._selectedCategory = {...this._selectedCategory, sgstRate: parseFloat(e.target.value) || 0}}>
            </uui-input>
          </div>
        </div>

        <div class="form-group">
          <label>IGST Rate (%)</label>
          <uui-input type="number" step="0.01" style="width: 200px;"
                     .value=${category.igstRate || 0}
                     @input=${e => this._selectedCategory = {...this._selectedCategory, igstRate: parseFloat(e.target.value) || 0}}>
          </uui-input>
          <small>IGST rate for inter-state transactions (typically CGST + SGST)</small>
        </div>

        <div class="form-row">
          <div class="form-group">
            <label>HSN Code</label>
            <uui-input style="width: 100%;" placeholder="e.g., 6109"
                       .value=${category.hsnCode || ''}
                       @input=${e => this._selectedCategory = {...this._selectedCategory, hsnCode: e.target.value}}>
            </uui-input>
            <small>Harmonized System of Nomenclature code for goods</small>
          </div>
          <div class="form-group">
            <label>SAC Code</label>
            <uui-input style="width: 100%;" placeholder="e.g., 998311"
                       .value=${category.sacCode || ''}
                       @input=${e => this._selectedCategory = {...this._selectedCategory, sacCode: e.target.value}}>
            </uui-input>
            <small>Services Accounting Code for services</small>
          </div>
        </div>

        <div class="rate-card">
          <h4 style="margin: 0 0 var(--uui-size-space-2) 0;">Preview</h4>
          <div style="display: grid; grid-template-columns: repeat(3, 1fr); gap: var(--uui-size-space-4);">
            <div>
              <div class="rate-value">${category.cgstRate || 0}%</div>
              <div style="color: var(--uui-color-text-alt); font-size: var(--uui-type-small-size);">CGST</div>
            </div>
            <div>
              <div class="rate-value">${category.sgstRate || 0}%</div>
              <div style="color: var(--uui-color-text-alt); font-size: var(--uui-type-small-size);">SGST</div>
            </div>
            <div>
              <div class="rate-value">${category.igstRate || 0}%</div>
              <div style="color: var(--uui-color-text-alt); font-size: var(--uui-type-small-size);">IGST</div>
            </div>
          </div>
          <div style="margin-top: var(--uui-size-space-3); padding-top: var(--uui-size-space-3); border-top: 1px solid var(--uui-color-border);">
            <strong>Total GST (Intra-state):</strong> ${(category.cgstRate || 0) + (category.sgstRate || 0)}%
          </div>
        </div>
      ` : ''}
    `;
  }

  _renderZoneEditor() {
    const zone = this._selectedZone;
    return html`
      <div class="editor-header">
        <h3 class="editor-title">${this._isNew ? 'New Zone' : zone.name}</h3>
        <div class="editor-actions">
          ${!this._isNew ? html`
            <uui-button look="secondary" color="danger" @click=${this._deleteZone}>
              <uui-icon name="icon-delete"></uui-icon> Delete
            </uui-button>
          ` : ''}
          <uui-button look="primary" @click=${this._saveZone} ?disabled=${this._saving}>
            ${this._saving ? html`<uui-loader-circle></uui-loader-circle>` : html`<uui-icon name="icon-check"></uui-icon>`}
            ${this._isNew ? 'Create' : 'Save'}
          </uui-button>
        </div>
      </div>

      <div class="editor-tabs">
        <div class="editor-tab ${this._editorTab === 'general' ? 'active' : ''}"
             @click=${() => this._editorTab = 'general'}>General</div>
        <div class="editor-tab ${this._editorTab === 'regions' ? 'active' : ''}"
             @click=${() => this._editorTab = 'regions'}>Regions</div>
      </div>

      <div class="editor-content">
        ${this._editorTab === 'general' ? this._renderZoneGeneral() : this._renderZoneRegions()}
      </div>
    `;
  }

  _renderZoneGeneral() {
    const zone = this._selectedZone;
    return html`
      <div class="form-row">
        <div class="form-group">
          <label>Name *</label>
          <uui-input style="width: 100%;"
                     .value=${zone.name || ''}
                     @input=${e => this._selectedZone = {...this._selectedZone, name: e.target.value}}>
          </uui-input>
        </div>
        <div class="form-group">
          <label>Code *</label>
          <uui-input style="width: 100%;"
                     .value=${zone.code || ''}
                     @input=${e => this._selectedZone = {...this._selectedZone, code: e.target.value}}>
          </uui-input>
        </div>
      </div>

      <div class="form-group">
        <label>Description</label>
        <uui-textarea style="width: 100%;"
                      .value=${zone.description || ''}
                      @input=${e => this._selectedZone = {...this._selectedZone, description: e.target.value}}>
        </uui-textarea>
      </div>

      <div class="form-row">
        <div class="form-group">
          <label>Priority</label>
          <uui-input type="number" style="width: 100%;"
                     .value=${zone.priority || 0}
                     @input=${e => this._selectedZone = {...this._selectedZone, priority: parseInt(e.target.value) || 0}}>
          </uui-input>
          <small>Higher priority zones are checked first</small>
        </div>
        <div class="form-group">
          <label>Sort Order</label>
          <uui-input type="number" style="width: 100%;"
                     .value=${zone.sortOrder || 0}
                     @input=${e => this._selectedZone = {...this._selectedZone, sortOrder: parseInt(e.target.value) || 0}}>
          </uui-input>
        </div>
      </div>

      <div class="form-group">
        <label>Options</label>
        <div class="checkbox-row">
          <uui-toggle .checked=${zone.isActive}
                      @change=${e => this._selectedZone = {...this._selectedZone, isActive: e.target.checked}}>
          </uui-toggle>
          <label>Active</label>
        </div>
        <div class="checkbox-row">
          <uui-toggle .checked=${zone.isDefault}
                      @change=${e => this._selectedZone = {...this._selectedZone, isDefault: e.target.checked}}>
          </uui-toggle>
          <label>Default Zone (fallback when no other zone matches)</label>
        </div>
      </div>
    `;
  }

  _renderZoneRegions() {
    const zone = this._selectedZone;
    return html`
      <div class="form-group">
        <label>Countries</label>
        <uui-input style="width: 100%;" placeholder="Type country code and press Enter (e.g., US, IN, UK)"
                   @keypress=${this._addCountry}>
        </uui-input>
        <div class="region-list">
          ${(zone.countries || []).map(country => html`
            <span class="region-tag">
              ${country}
              <button @click=${() => this._removeCountry(country)}>×</button>
            </span>
          `)}
        </div>
      </div>

      <div class="form-group">
        <label>States/Provinces</label>
        <uui-input style="width: 100%;" placeholder="Type state code and press Enter (e.g., CA, TX, MH)"
                   @keypress=${this._addState}>
        </uui-input>
        <div class="region-list">
          ${(zone.states || []).map(state => html`
            <span class="region-tag">
              ${state}
              <button @click=${() => this._removeState(state)}>×</button>
            </span>
          `)}
        </div>
      </div>

      <div class="form-group">
        <label>Postal Code Patterns</label>
        <uui-textarea style="width: 100%;" placeholder="One pattern per line (e.g., 90*, 100-199)"
                      .value=${(zone.postalCodePatterns || []).join('\n')}
                      @input=${e => this._selectedZone = {...this._selectedZone, postalCodePatterns: e.target.value.split('\n').filter(p => p.trim())}}>
        </uui-textarea>
        <small>Use * as wildcard, - for ranges</small>
      </div>

      ${(zone.countries?.length || zone.states?.length || zone.postalCodePatterns?.length) ? html`
        <div class="rate-card">
          <h4 style="margin: 0 0 var(--uui-size-space-2) 0;">Zone Coverage Summary</h4>
          <div style="display: grid; grid-template-columns: repeat(3, 1fr); gap: var(--uui-size-space-4);">
            <div>
              <div class="rate-value">${zone.countries?.length || 0}</div>
              <div style="color: var(--uui-color-text-alt); font-size: var(--uui-type-small-size);">Countries</div>
            </div>
            <div>
              <div class="rate-value">${zone.states?.length || 0}</div>
              <div style="color: var(--uui-color-text-alt); font-size: var(--uui-type-small-size);">States</div>
            </div>
            <div>
              <div class="rate-value">${zone.postalCodePatterns?.length || 0}</div>
              <div style="color: var(--uui-color-text-alt); font-size: var(--uui-type-small-size);">Postal Patterns</div>
            </div>
          </div>
        </div>
      ` : ''}
    `;
  }
}

customElements.define('ecommerce-tax-collection', TaxCollection);

export default TaxCollection;
