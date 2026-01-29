import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";

export class CurrencyCollection extends UmbElementMixin(LitElement) {
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
    .container { display: flex; height: 100%; }
    .list-panel { width: 320px; border-right: 1px solid #e0e0e0; display: flex; flex-direction: column; background: #fafafa; }
    .editor-panel { flex: 1; overflow-y: auto; background: #fff; }
    .list-header { padding: 16px; border-bottom: 1px solid #e0e0e0; background: #fff; }
    .list-header h2 { margin: 0 0 12px 0; font-size: 18px; }
    .search-row { display: flex; gap: 8px; }
    .search-row uui-input { flex: 1; }
    .list-content { flex: 1; overflow-y: auto; }
    .list-item { padding: 12px 16px; border-bottom: 1px solid #e9e9e9; cursor: pointer; display: flex; align-items: center; gap: 12px; }
    .list-item:hover { background: #f0f0f0; }
    .list-item.active { background: #e3f2fd; border-left: 3px solid #1976d2; }
    .list-item-icon { width: 40px; height: 40px; border-radius: 50%; background: #e0e0e0; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 14px; }
    .list-item-info { flex: 1; min-width: 0; }
    .list-item-name { font-weight: 600; }
    .list-item-meta { font-size: 12px; color: #666; }
    .badge { padding: 2px 6px; border-radius: 3px; font-size: 10px; font-weight: 500; }
    .badge-default { background: #d4edda; color: #155724; }
    .badge-active { background: #cce5ff; color: #004085; }
    .badge-inactive { background: #f8d7da; color: #721c24; }
    .editor-header { padding: 20px 24px; border-bottom: 1px solid #e0e0e0; display: flex; justify-content: space-between; align-items: center; background: #fff; position: sticky; top: 0; z-index: 10; }
    .editor-header h2 { margin: 0; font-size: 20px; }
    .editor-actions { display: flex; gap: 8px; }
    .editor-body { padding: 24px; max-width: 800px; }
    .form-section { margin-bottom: 24px; }
    .form-section-title { font-size: 14px; font-weight: 600; color: #333; margin-bottom: 16px; padding-bottom: 8px; border-bottom: 1px solid #e0e0e0; }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin-bottom: 16px; }
    .form-row.three { grid-template-columns: 1fr 1fr 1fr; }
    .form-group { margin-bottom: 16px; }
    .form-group label { display: block; margin-bottom: 6px; font-weight: 500; font-size: 13px; color: #333; }
    .form-group input, .form-group select { width: 100%; padding: 10px 12px; border: 1px solid #ddd; border-radius: 4px; font-size: 14px; box-sizing: border-box; }
    .form-group input:focus, .form-group select:focus { outline: none; border-color: #1976d2; box-shadow: 0 0 0 2px rgba(25,118,210,0.1); }
    .form-group small { display: block; margin-top: 4px; color: #666; font-size: 12px; }
    .checkbox-row { display: flex; gap: 24px; margin-bottom: 16px; }
    .checkbox-item { display: flex; align-items: center; gap: 8px; }
    .checkbox-item input { width: 18px; height: 18px; }
    .preview-box { background: #f8f9fa; border: 1px solid #e0e0e0; border-radius: 8px; padding: 20px; text-align: center; margin-top: 16px; }
    .preview-amount { font-size: 32px; font-weight: 700; color: #1976d2; }
    .preview-label { font-size: 12px; color: #666; margin-top: 8px; }
    .empty-state { display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100%; color: #666; }
    .empty-state uui-icon { font-size: 64px; margin-bottom: 16px; opacity: 0.5; }
    .loading { display: flex; justify-content: center; align-items: center; height: 100%; }
  `;

  static properties = {
    _currencies: { state: true },
    _loading: { state: true },
    _searchTerm: { state: true },
    _mode: { state: true },
    _editingCurrency: { state: true },
    _saving: { state: true }
  };

  constructor() {
    super();
    this._currencies = [];
    this._loading = true;
    this._searchTerm = '';
    this._mode = 'list';
    this._editingCurrency = null;
    this._saving = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadCurrencies();
  }

  async _loadCurrencies() {
    try {
      this._loading = true;
      const res = await this._authFetch('/umbraco/management/api/v1/ecommerce/currency');
      if (res.ok) {
        const data = await res.json();
        this._currencies = data.items || data || [];
      }
    } catch (e) { console.error('Error loading currencies:', e); }
    finally { this._loading = false; }
  }

  _getFilteredCurrencies() {
    if (!this._searchTerm) return this._currencies;
    const term = this._searchTerm.toLowerCase();
    return this._currencies.filter(c => c.code?.toLowerCase().includes(term) || c.name?.toLowerCase().includes(term));
  }

  _selectCurrency(currency) {
    this._editingCurrency = { ...currency };
    this._mode = 'edit';
  }

  _createNew() {
    this._editingCurrency = { code: '', name: '', symbol: '', decimalPlaces: 2, decimalSeparator: '.', thousandSeparator: ',', symbolPosition: 'before', exchangeRate: 1, isDefault: false, isActive: true };
    this._mode = 'create';
  }

  _backToList() {
    this._mode = 'list';
    this._editingCurrency = null;
  }

  _handleInput(field, value) {
    this._editingCurrency = { ...this._editingCurrency, [field]: value };
  }

  _formatPreview() {
    const c = this._editingCurrency || {};
    const amount = 1234.56;
    const parts = amount.toFixed(c.decimalPlaces || 2).split('.');
    const intPart = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, c.thousandSeparator || ',');
    const formatted = parts.length > 1 ? `${intPart}${c.decimalSeparator || '.'}${parts[1]}` : intPart;
    return c.symbolPosition === 'after' ? `${formatted} ${c.symbol || '$'}` : `${c.symbol || '$'}${formatted}`;
  }

  async _save() {
    if (!this._editingCurrency?.code || !this._editingCurrency?.name) { alert('Code and Name are required'); return; }
    this._saving = true;
    try {
      const isNew = !this._editingCurrency.id;
      const url = isNew ? '/umbraco/management/api/v1/ecommerce/currency' : `/umbraco/management/api/v1/ecommerce/currency/${this._editingCurrency.id}`;
      const res = await this._authFetch(url, { method: isNew ? 'POST' : 'PUT', body: JSON.stringify(this._editingCurrency) });
      if (!res.ok) throw new Error('Save failed');
      await this._loadCurrencies();
      if (isNew) {
        const data = await res.json();
        if (data?.id) this._selectCurrency(data);
        else this._mode = 'list';
      }
    } catch (e) { alert('Failed to save: ' + e.message); }
    finally { this._saving = false; }
  }

  async _delete() {
    if (!confirm(`Delete "${this._editingCurrency?.name}"?`)) return;
    try {
      await this._authFetch(`/umbraco/management/api/v1/ecommerce/currency/${this._editingCurrency.id}`, { method: 'DELETE' });
      this._loadCurrencies();
      this._backToList();
    } catch (e) { alert('Delete failed'); }
  }

  render() {
    return html`<div class="container">
      <div class="list-panel">
        <div class="list-header">
          <h2>Currencies</h2>
          <div class="search-row">
            <uui-input placeholder="Search..." .value=${this._searchTerm} @input=${e => this._searchTerm = e.target.value}><uui-icon name="icon-search" slot="prepend"></uui-icon></uui-input>
            <uui-button look="primary" compact @click=${this._createNew}><uui-icon name="icon-add"></uui-icon></uui-button>
          </div>
        </div>
        <div class="list-content">
          ${this._loading ? html`<div class="loading"><uui-loader></uui-loader></div>` :
            this._getFilteredCurrencies().map(c => html`
              <div class="list-item ${this._editingCurrency?.id === c.id ? 'active' : ''}" @click=${() => this._selectCurrency(c)}>
                <div class="list-item-icon">${c.symbol || c.code?.substring(0, 2)}</div>
                <div class="list-item-info">
                  <div class="list-item-name">${c.name} (${c.code})</div>
                  <div class="list-item-meta">
                    ${c.isDefault ? html`<span class="badge badge-default">Default</span>` : ''}
                    <span class="badge ${c.isActive !== false ? 'badge-active' : 'badge-inactive'}">${c.isActive !== false ? 'Active' : 'Inactive'}</span>
                    <span>Rate: ${c.exchangeRate || 1}</span>
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
    return html`<div class="empty-state"><uui-icon name="icon-coins-dollar-alt"></uui-icon><h3>Select a currency to edit</h3><p>Or click + to create a new one</p></div>`;
  }

  _renderEditor() {
    const isNew = this._mode === 'create';
    const c = this._editingCurrency || {};
    return html`
      <div class="editor-header">
        <div style="display:flex;align-items:center;gap:12px;">
          <uui-button look="secondary" compact @click=${this._backToList}><uui-icon name="icon-arrow-left"></uui-icon></uui-button>
          <h2>${isNew ? 'New Currency' : `${c.name} (${c.code})`}</h2>
        </div>
        <div class="editor-actions">
          ${!isNew ? html`<uui-button look="secondary" color="danger" @click=${this._delete}>Delete</uui-button>` : ''}
          <uui-button look="primary" @click=${this._save} ?disabled=${this._saving}>${this._saving ? 'Saving...' : 'Save'}</uui-button>
        </div>
      </div>
      <div class="editor-body">
        <div class="form-section">
          <div class="form-section-title">Basic Information</div>
          <div class="form-row.three">
            <div class="form-group">
              <label>Currency Code *</label>
              <input type="text" .value=${c.code || ''} @input=${e => this._handleInput('code', e.target.value.toUpperCase())} placeholder="USD" maxlength="3" style="text-transform:uppercase;" />
              <small>ISO 4217 code (e.g., USD, EUR, GBP)</small>
            </div>
            <div class="form-group">
              <label>Currency Name *</label>
              <input type="text" .value=${c.name || ''} @input=${e => this._handleInput('name', e.target.value)} placeholder="US Dollar" />
            </div>
            <div class="form-group">
              <label>Symbol</label>
              <input type="text" .value=${c.symbol || ''} @input=${e => this._handleInput('symbol', e.target.value)} placeholder="$" maxlength="5" />
            </div>
          </div>
        </div>
        <div class="form-section">
          <div class="form-section-title">Formatting</div>
          <div class="form-row">
            <div class="form-group">
              <label>Decimal Places</label>
              <input type="number" min="0" max="4" .value=${c.decimalPlaces ?? 2} @input=${e => this._handleInput('decimalPlaces', parseInt(e.target.value) || 0)} />
            </div>
            <div class="form-group">
              <label>Symbol Position</label>
              <select .value=${c.symbolPosition || 'before'} @change=${e => this._handleInput('symbolPosition', e.target.value)}>
                <option value="before">Before amount ($100)</option>
                <option value="after">After amount (100$)</option>
              </select>
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Decimal Separator</label>
              <select .value=${c.decimalSeparator || '.'} @change=${e => this._handleInput('decimalSeparator', e.target.value)}>
                <option value=".">Period (.)</option>
                <option value=",">Comma (,)</option>
              </select>
            </div>
            <div class="form-group">
              <label>Thousand Separator</label>
              <select .value=${c.thousandSeparator || ','} @change=${e => this._handleInput('thousandSeparator', e.target.value)}>
                <option value=",">Comma (,)</option>
                <option value=".">Period (.)</option>
                <option value=" ">Space ( )</option>
                <option value="">None</option>
              </select>
            </div>
          </div>
          <div class="preview-box">
            <div class="preview-amount">${this._formatPreview()}</div>
            <div class="preview-label">Preview (1,234.56)</div>
          </div>
        </div>
        <div class="form-section">
          <div class="form-section-title">Exchange Rate</div>
          <div class="form-group">
            <label>Exchange Rate</label>
            <input type="number" step="0.0001" min="0" .value=${c.exchangeRate ?? 1} @input=${e => this._handleInput('exchangeRate', parseFloat(e.target.value) || 1)} />
            <small>Rate relative to your base currency (1 = base currency)</small>
          </div>
        </div>
        <div class="form-section">
          <div class="form-section-title">Settings</div>
          <div class="checkbox-row">
            <label class="checkbox-item"><input type="checkbox" .checked=${c.isDefault || false} @change=${e => this._handleInput('isDefault', e.target.checked)} /> Default currency</label>
            <label class="checkbox-item"><input type="checkbox" .checked=${c.isActive !== false} @change=${e => this._handleInput('isActive', e.target.checked)} /> Active</label>
          </div>
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-currency-collection', CurrencyCollection);
export default CurrencyCollection;
