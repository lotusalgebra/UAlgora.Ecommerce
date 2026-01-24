import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

export class StoreCollection extends UmbElementMixin(LitElement) {
  static styles = css`
    :host { display: block; height: 100%; }
    .container { display: flex; height: 100%; }
    .list-panel { width: 350px; border-right: 1px solid #e0e0e0; display: flex; flex-direction: column; background: #fafafa; }
    .editor-panel { flex: 1; overflow-y: auto; background: #fff; }
    .list-header { padding: 16px; border-bottom: 1px solid #e0e0e0; background: #fff; }
    .list-header h2 { margin: 0 0 12px 0; font-size: 18px; }
    .search-row { display: flex; gap: 8px; }
    .search-row uui-input { flex: 1; }
    .list-content { flex: 1; overflow-y: auto; }
    .list-item { padding: 12px 16px; border-bottom: 1px solid #e9e9e9; cursor: pointer; display: flex; align-items: center; gap: 12px; }
    .list-item:hover { background: #f0f0f0; }
    .list-item.active { background: #e3f2fd; border-left: 3px solid #1976d2; }
    .list-item-icon { width: 44px; height: 44px; border-radius: 8px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); display: flex; align-items: center; justify-content: center; color: white; font-weight: 700; font-size: 16px; }
    .list-item-info { flex: 1; min-width: 0; }
    .list-item-name { font-weight: 600; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .list-item-meta { font-size: 12px; color: #666; display: flex; gap: 8px; flex-wrap: wrap; align-items: center; }
    .badge { padding: 2px 6px; border-radius: 3px; font-size: 10px; font-weight: 500; }
    .badge-active { background: #d4edda; color: #155724; }
    .badge-suspended { background: #f8d7da; color: #721c24; }
    .badge-trial { background: #fff3cd; color: #856404; }
    .editor-header { padding: 20px 24px; border-bottom: 1px solid #e0e0e0; display: flex; justify-content: space-between; align-items: center; background: #fff; position: sticky; top: 0; z-index: 10; }
    .editor-header h2 { margin: 0; font-size: 20px; }
    .editor-actions { display: flex; gap: 8px; }
    .editor-body { padding: 24px; max-width: 900px; }
    .tabs { display: flex; gap: 4px; border-bottom: 1px solid #e0e0e0; padding: 0 24px; background: #fafafa; }
    .tab { padding: 12px 20px; cursor: pointer; border-bottom: 2px solid transparent; color: #666; font-weight: 500; }
    .tab:hover { color: #333; }
    .tab.active { color: #1976d2; border-bottom-color: #1976d2; }
    .tab-content { display: none; }
    .tab-content.active { display: block; }
    .form-section { margin-bottom: 24px; }
    .form-section-title { font-size: 14px; font-weight: 600; color: #333; margin-bottom: 16px; padding-bottom: 8px; border-bottom: 1px solid #e0e0e0; }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin-bottom: 16px; }
    .form-row.three { grid-template-columns: 1fr 1fr 1fr; }
    .form-group { margin-bottom: 16px; }
    .form-group label { display: block; margin-bottom: 6px; font-weight: 500; font-size: 13px; color: #333; }
    .form-group input, .form-group select, .form-group textarea { width: 100%; padding: 10px 12px; border: 1px solid #ddd; border-radius: 4px; font-size: 14px; box-sizing: border-box; }
    .form-group input:focus, .form-group select:focus, .form-group textarea:focus { outline: none; border-color: #1976d2; box-shadow: 0 0 0 2px rgba(25,118,210,0.1); }
    .form-group input:disabled { background: #f5f5f5; color: #666; }
    .form-group small { display: block; margin-top: 4px; color: #666; font-size: 12px; }
    .input-with-btn { display: flex; gap: 8px; }
    .input-with-btn input { flex: 1; }
    .checkbox-row { display: flex; gap: 24px; margin-bottom: 16px; }
    .checkbox-item { display: flex; align-items: center; gap: 8px; }
    .checkbox-item input { width: 18px; height: 18px; }
    .currency-select-wrapper { position: relative; }
    .currency-preview { display: flex; align-items: center; gap: 12px; padding: 12px; background: #f8f9fa; border-radius: 8px; margin-top: 12px; }
    .currency-symbol { font-size: 24px; font-weight: 700; color: #1976d2; width: 50px; height: 50px; background: #e3f2fd; border-radius: 8px; display: flex; align-items: center; justify-content: center; }
    .currency-info { flex: 1; }
    .currency-name { font-weight: 600; }
    .currency-code { font-size: 12px; color: #666; }
    .stats-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; margin-bottom: 24px; }
    .stat-card { background: #f8f9fa; border-radius: 8px; padding: 16px; text-align: center; }
    .stat-value { font-size: 24px; font-weight: 700; color: #1976d2; }
    .stat-label { font-size: 12px; color: #666; margin-top: 4px; }
    .status-card { padding: 16px; border-radius: 8px; margin-bottom: 16px; }
    .status-card.active { background: #d4edda; border: 1px solid #c3e6cb; }
    .status-card.trial { background: #fff3cd; border: 1px solid #ffeeba; }
    .status-card.suspended { background: #f8d7da; border: 1px solid #f5c6cb; }
    .status-card-title { font-weight: 600; margin-bottom: 4px; }
    .status-card-desc { font-size: 13px; opacity: 0.8; }
    .upload-zone { border: 2px dashed #ddd; border-radius: 8px; padding: 30px 20px; text-align: center; cursor: pointer; transition: all 0.2s; }
    .upload-zone:hover { border-color: #1976d2; background: #f8f9fa; }
    .upload-zone uui-icon { font-size: 32px; color: #999; margin-bottom: 8px; }
    .logo-preview { margin-top: 16px; text-align: center; }
    .logo-preview img { max-width: 200px; max-height: 100px; border-radius: 8px; border: 1px solid #ddd; }
    .color-picker-row { display: flex; gap: 16px; }
    .color-picker-item { flex: 1; }
    .color-picker-item label { display: block; margin-bottom: 6px; font-weight: 500; font-size: 13px; }
    .color-picker-item input[type="color"] { width: 100%; height: 44px; padding: 4px; border: 1px solid #ddd; border-radius: 4px; cursor: pointer; }
    .empty-state { display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100%; color: #666; }
    .empty-state uui-icon { font-size: 64px; margin-bottom: 16px; opacity: 0.5; }
    .loading { display: flex; justify-content: center; align-items: center; height: 100%; }
  `;

  static properties = {
    _stores: { state: true },
    _currencies: { state: true },
    _loading: { state: true },
    _searchTerm: { state: true },
    _mode: { state: true },
    _editingStore: { state: true },
    _activeTab: { state: true },
    _saving: { state: true }
  };

  constructor() {
    super();
    this._stores = [];
    this._currencies = [];
    this._loading = true;
    this._searchTerm = '';
    this._mode = 'list';
    this._editingStore = null;
    this._activeTab = 'general';
    this._saving = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadStores();
    this._loadCurrencies();
  }

  async _loadStores() {
    try {
      this._loading = true;
      const res = await fetch('/umbraco/management/api/v1/ecommerce/stores', { credentials: 'include', headers: { 'Accept': 'application/json' } });
      if (res.ok) {
        const data = await res.json();
        this._stores = data.items || data || [];
      }
    } catch (e) { console.error('Error loading stores:', e); }
    finally { this._loading = false; }
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

  _getFilteredStores() {
    if (!this._searchTerm) return this._stores;
    const term = this._searchTerm.toLowerCase();
    return this._stores.filter(s => s.name?.toLowerCase().includes(term) || s.code?.toLowerCase().includes(term) || s.domain?.toLowerCase().includes(term));
  }

  _getSelectedCurrency() {
    return this._currencies.find(c => c.code === this._editingStore?.defaultCurrencyCode) || null;
  }

  _selectStore(store) {
    this._editingStore = { ...store };
    this._mode = 'edit';
    this._activeTab = 'general';
  }

  _createNew() {
    this._editingStore = {
      code: '', name: '', domain: '',
      contactEmail: '', supportEmail: '', phone: '',
      defaultCurrencyCode: 'USD', defaultLanguage: 'en-US',
      logoUrl: '', faviconUrl: '',
      primaryColor: '#1976d2', secondaryColor: '#424242',
      taxIncludedInPrice: false, showPricesWithTax: true,
      status: 0, isDefault: false
    };
    this._mode = 'create';
    this._activeTab = 'general';
  }

  _backToList() {
    this._mode = 'list';
    this._editingStore = null;
  }

  _handleInput(field, value) {
    this._editingStore = { ...this._editingStore, [field]: value };
  }

  _generateCode() {
    if (this._editingStore?.name) {
      const code = this._editingStore.name.toUpperCase().replace(/[^A-Z0-9]+/g, '_').substring(0, 10);
      this._handleInput('code', code);
    }
  }

  _handleLogoUpload(e) {
    const file = e.target.files?.[0];
    if (file) this._uploadImage(file, 'logoUrl');
  }

  async _uploadImage(file, field) {
    const formData = new FormData();
    formData.append('file', file);
    try {
      const res = await fetch('/umbraco/management/api/v1/ecommerce/media/upload', { method: 'POST', credentials: 'include', body: formData });
      if (res.ok) {
        const data = await res.json();
        this._handleInput(field, data.url || data.path);
      }
    } catch (e) { console.error('Upload failed:', e); }
  }

  async _save() {
    if (!this._editingStore?.code || !this._editingStore?.name) { alert('Code and Name are required'); return; }
    this._saving = true;
    try {
      const isNew = !this._editingStore.id;
      const url = isNew ? '/umbraco/management/api/v1/ecommerce/stores' : `/umbraco/management/api/v1/ecommerce/stores/${this._editingStore.id}`;
      const res = await fetch(url, { method: isNew ? 'POST' : 'PUT', credentials: 'include', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(this._editingStore) });
      if (!res.ok) throw new Error('Save failed');
      await this._loadStores();
      if (isNew) {
        const data = await res.json();
        if (data?.id) this._selectStore(data);
        else this._mode = 'list';
      }
    } catch (e) { alert('Failed to save: ' + e.message); }
    finally { this._saving = false; }
  }

  async _delete() {
    if (!confirm(`Delete store "${this._editingStore?.name}"? This action cannot be undone.`)) return;
    try {
      await fetch(`/umbraco/management/api/v1/ecommerce/stores/${this._editingStore.id}`, { method: 'DELETE', credentials: 'include' });
      this._loadStores();
      this._backToList();
    } catch (e) { alert('Delete failed'); }
  }

  _getStatusClass(status) {
    return status === 0 ? 'active' : status === 2 ? 'trial' : 'suspended';
  }

  _getStatusText(status) {
    return status === 0 ? 'Active' : status === 2 ? 'Trial' : 'Suspended';
  }

  render() {
    return html`<div class="container">
      <div class="list-panel">
        <div class="list-header">
          <h2>Stores</h2>
          <div class="search-row">
            <uui-input placeholder="Search..." .value=${this._searchTerm} @input=${e => this._searchTerm = e.target.value}><uui-icon name="icon-search" slot="prepend"></uui-icon></uui-input>
            <uui-button look="primary" compact @click=${this._createNew}><uui-icon name="icon-add"></uui-icon></uui-button>
          </div>
        </div>
        <div class="list-content">
          ${this._loading ? html`<div class="loading"><uui-loader></uui-loader></div>` :
            this._getFilteredStores().map(s => html`
              <div class="list-item ${this._editingStore?.id === s.id ? 'active' : ''}" @click=${() => this._selectStore(s)}>
                <div class="list-item-icon">${s.code?.substring(0, 2) || 'ST'}</div>
                <div class="list-item-info">
                  <div class="list-item-name">${s.name}</div>
                  <div class="list-item-meta">
                    <span>${s.code}</span>
                    <span>${s.defaultCurrencyCode}</span>
                    <span class="badge badge-${this._getStatusClass(s.status)}">${this._getStatusText(s.status)}</span>
                  </div>
                </div>
              </div>
            `)}
        </div>
      </div>
      <div class="editor-panel">
        ${this._mode === 'list' ? this._renderEmptyState() : this._renderEditor()}
      </div>
    </div>`;
  }

  _renderEmptyState() {
    return html`<div class="empty-state"><uui-icon name="icon-store"></uui-icon><h3>Select a store to edit</h3><p>Or click + to create a new one</p></div>`;
  }

  _renderEditor() {
    const isNew = this._mode === 'create';
    return html`
      <div class="editor-header">
        <div style="display:flex;align-items:center;gap:12px;">
          <uui-button look="secondary" compact @click=${this._backToList}><uui-icon name="icon-arrow-left"></uui-icon></uui-button>
          <h2>${isNew ? 'New Store' : this._editingStore?.name || 'Edit Store'}</h2>
        </div>
        <div class="editor-actions">
          ${!isNew ? html`<uui-button look="secondary" color="danger" @click=${this._delete}>Delete</uui-button>` : ''}
          <uui-button look="primary" @click=${this._save} ?disabled=${this._saving}>${this._saving ? 'Saving...' : 'Save'}</uui-button>
        </div>
      </div>
      <div class="tabs">
        <div class="tab ${this._activeTab === 'general' ? 'active' : ''}" @click=${() => this._activeTab = 'general'}>General</div>
        <div class="tab ${this._activeTab === 'currency' ? 'active' : ''}" @click=${() => this._activeTab = 'currency'}>Currency & Locale</div>
        <div class="tab ${this._activeTab === 'contact' ? 'active' : ''}" @click=${() => this._activeTab = 'contact'}>Contact</div>
        <div class="tab ${this._activeTab === 'branding' ? 'active' : ''}" @click=${() => this._activeTab = 'branding'}>Branding</div>
        <div class="tab ${this._activeTab === 'settings' ? 'active' : ''}" @click=${() => this._activeTab = 'settings'}>Settings</div>
      </div>
      <div class="editor-body">
        <div class="tab-content ${this._activeTab === 'general' ? 'active' : ''}">${this._renderGeneralTab()}</div>
        <div class="tab-content ${this._activeTab === 'currency' ? 'active' : ''}">${this._renderCurrencyTab()}</div>
        <div class="tab-content ${this._activeTab === 'contact' ? 'active' : ''}">${this._renderContactTab()}</div>
        <div class="tab-content ${this._activeTab === 'branding' ? 'active' : ''}">${this._renderBrandingTab()}</div>
        <div class="tab-content ${this._activeTab === 'settings' ? 'active' : ''}">${this._renderSettingsTab()}</div>
      </div>
    `;
  }

  _renderGeneralTab() {
    const s = this._editingStore || {};
    const isNew = this._mode === 'create';
    return html`
      ${!isNew ? html`
        <div class="status-card ${this._getStatusClass(s.status)}">
          <div class="status-card-title">Store Status: ${this._getStatusText(s.status)}</div>
          <div class="status-card-desc">${s.status === 0 ? 'This store is active and operational.' : s.status === 2 ? 'This store is in trial mode.' : 'This store is suspended.'}</div>
        </div>
      ` : ''}
      <div class="form-section">
        <div class="form-section-title">Store Information</div>
        <div class="form-row">
          <div class="form-group">
            <label>Store Code *</label>
            <div class="input-with-btn">
              <input type="text" .value=${s.code || ''} @input=${e => this._handleInput('code', e.target.value.toUpperCase())} ?disabled=${!isNew} placeholder="STORE1" style="text-transform:uppercase;" maxlength="10" />
              ${isNew ? html`<uui-button look="secondary" compact @click=${this._generateCode}>Generate</uui-button>` : ''}
            </div>
            <small>Unique identifier for this store (cannot be changed after creation)</small>
          </div>
          <div class="form-group">
            <label>Store Name *</label>
            <input type="text" .value=${s.name || ''} @input=${e => this._handleInput('name', e.target.value)} placeholder="My Store" />
          </div>
        </div>
        <div class="form-group">
          <label>Domain</label>
          <input type="text" .value=${s.domain || ''} @input=${e => this._handleInput('domain', e.target.value)} placeholder="store.example.com" />
          <small>The domain where this store is accessible</small>
        </div>
      </div>
      <div class="form-section">
        <div class="form-section-title">Status</div>
        <div class="form-group">
          <label>Store Status</label>
          <select .value=${s.status ?? 0} @change=${e => this._handleInput('status', parseInt(e.target.value))}>
            <option value="0">Active</option>
            <option value="2">Trial</option>
            <option value="1">Suspended</option>
          </select>
        </div>
        <div class="checkbox-row">
          <label class="checkbox-item"><input type="checkbox" .checked=${s.isDefault || false} @change=${e => this._handleInput('isDefault', e.target.checked)} /> Default store</label>
        </div>
      </div>
    `;
  }

  _renderCurrencyTab() {
    const s = this._editingStore || {};
    const selectedCurrency = this._getSelectedCurrency();
    return html`
      <div class="form-section">
        <div class="form-section-title">Store Currency</div>
        <div class="form-group">
          <label>Default Currency *</label>
          <select .value=${s.defaultCurrencyCode || 'USD'} @change=${e => this._handleInput('defaultCurrencyCode', e.target.value)}>
            ${this._currencies.length === 0 ? html`<option value="">-- Loading currencies... --</option>` : ''}
            ${this._currencies.map(c => html`<option value="${c.code}" ?selected=${s.defaultCurrencyCode === c.code}>${c.code} - ${c.name}</option>`)}
          </select>
          <small>All prices in this store will be displayed in this currency</small>
        </div>
        ${selectedCurrency ? html`
          <div class="currency-preview">
            <div class="currency-symbol">${selectedCurrency.symbol || selectedCurrency.code?.substring(0, 1)}</div>
            <div class="currency-info">
              <div class="currency-name">${selectedCurrency.name}</div>
              <div class="currency-code">${selectedCurrency.code} • ${selectedCurrency.decimalPlaces || 2} decimal places • Exchange rate: ${selectedCurrency.exchangeRate || 1}</div>
            </div>
          </div>
        ` : ''}
      </div>
      <div class="form-section">
        <div class="form-section-title">Locale Settings</div>
        <div class="form-row">
          <div class="form-group">
            <label>Default Language</label>
            <select .value=${s.defaultLanguage || 'en-US'} @change=${e => this._handleInput('defaultLanguage', e.target.value)}>
              <option value="en-US">English (US)</option>
              <option value="en-GB">English (UK)</option>
              <option value="es-ES">Spanish (Spain)</option>
              <option value="fr-FR">French (France)</option>
              <option value="de-DE">German (Germany)</option>
              <option value="it-IT">Italian (Italy)</option>
              <option value="pt-BR">Portuguese (Brazil)</option>
              <option value="ja-JP">Japanese</option>
              <option value="zh-CN">Chinese (Simplified)</option>
              <option value="hi-IN">Hindi (India)</option>
            </select>
          </div>
          <div class="form-group">
            <label>Timezone</label>
            <select .value=${s.timezone || 'UTC'} @change=${e => this._handleInput('timezone', e.target.value)}>
              <option value="UTC">UTC</option>
              <option value="America/New_York">Eastern Time (US)</option>
              <option value="America/Los_Angeles">Pacific Time (US)</option>
              <option value="Europe/London">London</option>
              <option value="Europe/Paris">Paris</option>
              <option value="Asia/Tokyo">Tokyo</option>
              <option value="Asia/Kolkata">India (IST)</option>
              <option value="Australia/Sydney">Sydney</option>
            </select>
          </div>
        </div>
      </div>
      <div class="form-section">
        <div class="form-section-title">Tax Display</div>
        <div class="checkbox-row">
          <label class="checkbox-item"><input type="checkbox" .checked=${s.taxIncludedInPrice || false} @change=${e => this._handleInput('taxIncludedInPrice', e.target.checked)} /> Prices include tax</label>
          <label class="checkbox-item"><input type="checkbox" .checked=${s.showPricesWithTax !== false} @change=${e => this._handleInput('showPricesWithTax', e.target.checked)} /> Show prices with tax</label>
        </div>
      </div>
    `;
  }

  _renderContactTab() {
    const s = this._editingStore || {};
    return html`
      <div class="form-section">
        <div class="form-section-title">Contact Information</div>
        <div class="form-row">
          <div class="form-group">
            <label>Contact Email</label>
            <input type="email" .value=${s.contactEmail || ''} @input=${e => this._handleInput('contactEmail', e.target.value)} placeholder="contact@store.com" />
          </div>
          <div class="form-group">
            <label>Support Email</label>
            <input type="email" .value=${s.supportEmail || ''} @input=${e => this._handleInput('supportEmail', e.target.value)} placeholder="support@store.com" />
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Phone Number</label>
            <input type="tel" .value=${s.phone || ''} @input=${e => this._handleInput('phone', e.target.value)} placeholder="+1 234 567 8900" />
          </div>
          <div class="form-group">
            <label>Fax Number</label>
            <input type="tel" .value=${s.fax || ''} @input=${e => this._handleInput('fax', e.target.value)} placeholder="+1 234 567 8901" />
          </div>
        </div>
      </div>
      <div class="form-section">
        <div class="form-section-title">Address</div>
        <div class="form-group">
          <label>Street Address</label>
          <input type="text" .value=${s.address || ''} @input=${e => this._handleInput('address', e.target.value)} placeholder="123 Commerce St" />
        </div>
        <div class="form-row.three">
          <div class="form-group">
            <label>City</label>
            <input type="text" .value=${s.city || ''} @input=${e => this._handleInput('city', e.target.value)} placeholder="New York" />
          </div>
          <div class="form-group">
            <label>State/Province</label>
            <input type="text" .value=${s.state || ''} @input=${e => this._handleInput('state', e.target.value)} placeholder="NY" />
          </div>
          <div class="form-group">
            <label>Postal Code</label>
            <input type="text" .value=${s.postalCode || ''} @input=${e => this._handleInput('postalCode', e.target.value)} placeholder="10001" />
          </div>
        </div>
        <div class="form-group">
          <label>Country</label>
          <input type="text" .value=${s.country || ''} @input=${e => this._handleInput('country', e.target.value)} placeholder="United States" />
        </div>
      </div>
    `;
  }

  _renderBrandingTab() {
    const s = this._editingStore || {};
    return html`
      <div class="form-section">
        <div class="form-section-title">Logo</div>
        <div class="upload-zone" @click=${() => this.shadowRoot.querySelector('#logoInput').click()}>
          <uui-icon name="icon-picture"></uui-icon>
          <div>Click to upload store logo</div>
          <small>Recommended: 400x100px, PNG with transparent background</small>
        </div>
        <input type="file" id="logoInput" accept="image/*" style="display:none" @change=${this._handleLogoUpload} />
        ${s.logoUrl ? html`<div class="logo-preview"><img src="${s.logoUrl}" /><br/><uui-button look="secondary" compact @click=${() => this._handleInput('logoUrl', '')}>Remove</uui-button></div>` : ''}
        <div class="form-group" style="margin-top:16px;">
          <label>Or enter logo URL</label>
          <input type="text" .value=${s.logoUrl || ''} @input=${e => this._handleInput('logoUrl', e.target.value)} placeholder="https://..." />
        </div>
      </div>
      <div class="form-section">
        <div class="form-section-title">Favicon</div>
        <div class="form-group">
          <label>Favicon URL</label>
          <input type="text" .value=${s.faviconUrl || ''} @input=${e => this._handleInput('faviconUrl', e.target.value)} placeholder="https://..." />
          <small>Small icon shown in browser tabs (recommended: 32x32px)</small>
        </div>
      </div>
      <div class="form-section">
        <div class="form-section-title">Brand Colors</div>
        <div class="color-picker-row">
          <div class="color-picker-item">
            <label>Primary Color</label>
            <input type="color" .value=${s.primaryColor || '#1976d2'} @input=${e => this._handleInput('primaryColor', e.target.value)} />
          </div>
          <div class="color-picker-item">
            <label>Secondary Color</label>
            <input type="color" .value=${s.secondaryColor || '#424242'} @input=${e => this._handleInput('secondaryColor', e.target.value)} />
          </div>
          <div class="color-picker-item">
            <label>Accent Color</label>
            <input type="color" .value=${s.accentColor || '#ff5722'} @input=${e => this._handleInput('accentColor', e.target.value)} />
          </div>
        </div>
      </div>
    `;
  }

  _renderSettingsTab() {
    const s = this._editingStore || {};
    return html`
      <div class="form-section">
        <div class="form-section-title">Checkout Settings</div>
        <div class="checkbox-row">
          <label class="checkbox-item"><input type="checkbox" .checked=${s.guestCheckoutEnabled !== false} @change=${e => this._handleInput('guestCheckoutEnabled', e.target.checked)} /> Allow guest checkout</label>
        </div>
        <div class="checkbox-row">
          <label class="checkbox-item"><input type="checkbox" .checked=${s.requirePhoneNumber || false} @change=${e => this._handleInput('requirePhoneNumber', e.target.checked)} /> Require phone number at checkout</label>
        </div>
        <div class="form-group">
          <label>Minimum Order Amount</label>
          <input type="number" step="0.01" min="0" .value=${s.minimumOrderAmount || 0} @input=${e => this._handleInput('minimumOrderAmount', parseFloat(e.target.value) || 0)} />
          <small>Set to 0 for no minimum</small>
        </div>
      </div>
      <div class="form-section">
        <div class="form-section-title">Order Settings</div>
        <div class="form-group">
          <label>Order Number Prefix</label>
          <input type="text" .value=${s.orderNumberPrefix || ''} @input=${e => this._handleInput('orderNumberPrefix', e.target.value)} placeholder="ORD-" />
          <small>Prefix for order numbers (e.g., ORD-10001)</small>
        </div>
        <div class="form-group">
          <label>Order Number Start</label>
          <input type="number" min="1" .value=${s.orderNumberStart || 1000} @input=${e => this._handleInput('orderNumberStart', parseInt(e.target.value) || 1000)} />
        </div>
      </div>
      <div class="form-section">
        <div class="form-section-title">Inventory Settings</div>
        <div class="checkbox-row">
          <label class="checkbox-item"><input type="checkbox" .checked=${s.trackInventory !== false} @change=${e => this._handleInput('trackInventory', e.target.checked)} /> Track inventory</label>
        </div>
        <div class="checkbox-row">
          <label class="checkbox-item"><input type="checkbox" .checked=${s.allowBackorders || false} @change=${e => this._handleInput('allowBackorders', e.target.checked)} /> Allow backorders</label>
        </div>
        <div class="form-group">
          <label>Low Stock Threshold</label>
          <input type="number" min="0" .value=${s.lowStockThreshold || 10} @input=${e => this._handleInput('lowStockThreshold', parseInt(e.target.value) || 10)} />
          <small>Alert when stock falls below this number</small>
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-store-collection', StoreCollection);
export default StoreCollection;
