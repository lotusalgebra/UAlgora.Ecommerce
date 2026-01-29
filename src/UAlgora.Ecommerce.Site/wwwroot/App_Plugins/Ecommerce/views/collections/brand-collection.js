import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";

export class BrandCollection extends UmbElementMixin(LitElement) {
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
    if (options.headers?.['Content-Type'] === undefined) {
      delete headers['Content-Type'];
    }
    return fetch(url, {
      ...options,
      headers: { ...headers, ...options.headers }
    });
  }

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
    .list-item-img { width: 48px; height: 48px; border-radius: 8px; object-fit: contain; background: #fff; border: 1px solid #e0e0e0; display: flex; align-items: center; justify-content: center; }
    .list-item-img img { max-width: 100%; max-height: 100%; }
    .list-item-info { flex: 1; min-width: 0; }
    .list-item-name { font-weight: 600; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .list-item-meta { font-size: 12px; color: #666; display: flex; gap: 8px; }
    .badge { padding: 2px 6px; border-radius: 3px; font-size: 10px; font-weight: 500; }
    .badge-active { background: #d4edda; color: #155724; }
    .badge-inactive { background: #f8d7da; color: #721c24; }
    .badge-featured { background: #fff3cd; color: #856404; }
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
    .form-group { margin-bottom: 16px; }
    .form-group label { display: block; margin-bottom: 6px; font-weight: 500; font-size: 13px; color: #333; }
    .form-group input, .form-group select, .form-group textarea { width: 100%; padding: 10px 12px; border: 1px solid #ddd; border-radius: 4px; font-size: 14px; box-sizing: border-box; }
    .form-group input:focus, .form-group select:focus, .form-group textarea:focus { outline: none; border-color: #1976d2; box-shadow: 0 0 0 2px rgba(25,118,210,0.1); }
    .form-group textarea { min-height: 100px; resize: vertical; }
    .form-group small { display: block; margin-top: 4px; color: #666; font-size: 12px; }
    .input-with-btn { display: flex; gap: 8px; }
    .input-with-btn input { flex: 1; }
    .checkbox-row { display: flex; gap: 24px; margin-bottom: 16px; }
    .checkbox-item { display: flex; align-items: center; gap: 8px; }
    .checkbox-item input { width: 18px; height: 18px; }
    .wysiwyg-toolbar { display: flex; gap: 4px; padding: 8px; background: #f5f5f5; border: 1px solid #ddd; border-bottom: none; border-radius: 4px 4px 0 0; flex-wrap: wrap; }
    .wysiwyg-toolbar button { padding: 6px 10px; border: 1px solid #ddd; background: #fff; border-radius: 3px; cursor: pointer; font-size: 12px; }
    .wysiwyg-toolbar button:hover { background: #e0e0e0; }
    .wysiwyg-editor { min-height: 150px; padding: 12px; border: 1px solid #ddd; border-radius: 0 0 4px 4px; outline: none; background: #fff; }
    .upload-zone { border: 2px dashed #ddd; border-radius: 8px; padding: 40px 20px; text-align: center; cursor: pointer; transition: all 0.2s; }
    .upload-zone:hover { border-color: #1976d2; background: #f8f9fa; }
    .upload-zone.dragover { border-color: #1976d2; background: #e3f2fd; }
    .upload-zone uui-icon { font-size: 32px; color: #999; margin-bottom: 8px; }
    .image-preview { margin-top: 16px; text-align: center; }
    .image-preview img { max-width: 200px; max-height: 150px; border-radius: 8px; border: 1px solid #ddd; }
    .seo-preview { background: #fff; border: 1px solid #ddd; border-radius: 8px; padding: 16px; margin-top: 16px; }
    .seo-preview-title { color: #1a0dab; font-size: 18px; margin-bottom: 4px; text-decoration: underline; }
    .seo-preview-url { color: #006621; font-size: 14px; margin-bottom: 4px; }
    .seo-preview-desc { color: #545454; font-size: 13px; line-height: 1.4; }
    .stats-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 16px; margin-bottom: 24px; }
    .stat-card { background: #f8f9fa; border-radius: 8px; padding: 16px; text-align: center; }
    .stat-value { font-size: 24px; font-weight: 700; color: #1976d2; }
    .stat-label { font-size: 12px; color: #666; margin-top: 4px; }
    .empty-state { display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100%; color: #666; }
    .empty-state uui-icon { font-size: 64px; margin-bottom: 16px; opacity: 0.5; }
    .loading { display: flex; justify-content: center; align-items: center; height: 100%; }
  `;

  static properties = {
    _brands: { state: true },
    _loading: { state: true },
    _searchTerm: { state: true },
    _mode: { state: true },
    _editingBrand: { state: true },
    _activeTab: { state: true },
    _saving: { state: true }
  };

  constructor() {
    super();
    this._brands = [];
    this._loading = true;
    this._searchTerm = '';
    this._mode = 'list';
    this._editingBrand = null;
    this._activeTab = 'brand';
    this._saving = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadBrands();
  }

  async _loadBrands() {
    try {
      this._loading = true;
      const res = await this._authFetch('/umbraco/management/api/v1/ecommerce/brand');
      if (res.ok) {
        const data = await res.json();
        this._brands = data.items || data || [];
      }
    } catch (e) { console.error('Error loading brands:', e); }
    finally { this._loading = false; }
  }

  _getFilteredBrands() {
    if (!this._searchTerm) return this._brands;
    const term = this._searchTerm.toLowerCase();
    return this._brands.filter(b => b.name?.toLowerCase().includes(term) || b.slug?.toLowerCase().includes(term));
  }

  _selectBrand(brand) {
    this._editingBrand = { ...brand };
    this._mode = 'edit';
    this._activeTab = 'brand';
  }

  _createNew() {
    this._editingBrand = { name: '', slug: '', description: '', logoUrl: '', websiteUrl: '', isActive: true, isFeatured: false, sortOrder: 0, metaTitle: '', metaDescription: '' };
    this._mode = 'create';
    this._activeTab = 'brand';
  }

  _backToList() {
    this._mode = 'list';
    this._editingBrand = null;
  }

  _handleInput(field, value) {
    this._editingBrand = { ...this._editingBrand, [field]: value };
  }

  _generateSlug() {
    if (this._editingBrand?.name) {
      const slug = this._editingBrand.name.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '');
      this._handleInput('slug', slug);
    }
  }

  _execCommand(cmd, value = null) {
    document.execCommand(cmd, false, value);
    const editor = this.shadowRoot.querySelector('.wysiwyg-editor');
    if (editor) this._handleInput('description', editor.innerHTML);
  }

  _handleDescriptionInput(e) {
    this._handleInput('description', e.target.innerHTML);
  }

  _handleImageDrop(e) {
    e.preventDefault();
    e.currentTarget.classList.remove('dragover');
    const file = e.dataTransfer?.files?.[0];
    if (file && file.type.startsWith('image/')) this._uploadImage(file);
  }

  _handleImageSelect(e) {
    const file = e.target.files?.[0];
    if (file) this._uploadImage(file);
  }

  async _uploadImage(file) {
    const formData = new FormData();
    formData.append('file', file);
    try {
      const headers = await this._getAuthHeaders();
      delete headers['Content-Type'];
      const res = await fetch('/umbraco/management/api/v1/ecommerce/media/upload', { method: 'POST', headers, body: formData });
      if (res.ok) {
        const data = await res.json();
        this._handleInput('logoUrl', data.url || data.path);
      }
    } catch (e) { console.error('Upload failed:', e); }
  }

  async _save() {
    if (!this._editingBrand?.name) { alert('Name is required'); return; }
    this._saving = true;
    try {
      const isNew = !this._editingBrand.id;
      const url = isNew ? '/umbraco/management/api/v1/ecommerce/brand' : `/umbraco/management/api/v1/ecommerce/brand/${this._editingBrand.id}`;
      const res = await this._authFetch(url, { method: isNew ? 'POST' : 'PUT', body: JSON.stringify(this._editingBrand) });
      if (!res.ok) throw new Error('Save failed');
      await this._loadBrands();
      if (isNew) {
        const data = await res.json();
        if (data?.id) this._selectBrand(data);
        else this._mode = 'list';
      }
    } catch (e) { alert('Failed to save: ' + e.message); }
    finally { this._saving = false; }
  }

  async _delete() {
    if (!confirm(`Delete "${this._editingBrand?.name}"?`)) return;
    try {
      await this._authFetch(`/umbraco/management/api/v1/ecommerce/brand/${this._editingBrand.id}`, { method: 'DELETE' });
      this._loadBrands();
      this._backToList();
    } catch (e) { alert('Delete failed'); }
  }

  render() {
    return html`<div class="container">
      <div class="list-panel">
        <div class="list-header">
          <h2>Brands</h2>
          <div class="search-row">
            <uui-input placeholder="Search..." .value=${this._searchTerm} @input=${e => this._searchTerm = e.target.value}><uui-icon name="icon-search" slot="prepend"></uui-icon></uui-input>
            <uui-button look="primary" compact @click=${this._createNew}><uui-icon name="icon-add"></uui-icon></uui-button>
          </div>
        </div>
        <div class="list-content">
          ${this._loading ? html`<div class="loading"><uui-loader></uui-loader></div>` :
            this._getFilteredBrands().map(b => html`
              <div class="list-item ${this._editingBrand?.id === b.id ? 'active' : ''}" @click=${() => this._selectBrand(b)}>
                <div class="list-item-img">${b.logoUrl ? html`<img src="${b.logoUrl}" />` : html`<uui-icon name="icon-stamp"></uui-icon>`}</div>
                <div class="list-item-info">
                  <div class="list-item-name">${b.name}</div>
                  <div class="list-item-meta">
                    ${b.slug ? html`<span>/${b.slug}</span>` : ''}
                    <span class="badge ${b.isActive !== false ? 'badge-active' : 'badge-inactive'}">${b.isActive !== false ? 'Active' : 'Inactive'}</span>
                    ${b.isFeatured ? html`<span class="badge badge-featured">Featured</span>` : ''}
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
    return html`<div class="empty-state"><uui-icon name="icon-stamp"></uui-icon><h3>Select a brand to edit</h3><p>Or click + to create a new one</p></div>`;
  }

  _renderEditor() {
    const isNew = this._mode === 'create';
    return html`
      <div class="editor-header">
        <div style="display:flex;align-items:center;gap:12px;">
          <uui-button look="secondary" compact @click=${this._backToList}><uui-icon name="icon-arrow-left"></uui-icon></uui-button>
          <h2>${isNew ? 'New Brand' : this._editingBrand?.name || 'Edit Brand'}</h2>
        </div>
        <div class="editor-actions">
          ${!isNew ? html`<uui-button look="secondary" color="danger" @click=${this._delete}>Delete</uui-button>` : ''}
          <uui-button look="primary" @click=${this._save} ?disabled=${this._saving}>${this._saving ? 'Saving...' : 'Save'}</uui-button>
        </div>
      </div>
      <div class="tabs">
        <div class="tab ${this._activeTab === 'brand' ? 'active' : ''}" @click=${() => this._activeTab = 'brand'}>Brand</div>
        <div class="tab ${this._activeTab === 'media' ? 'active' : ''}" @click=${() => this._activeTab = 'media'}>Media</div>
        <div class="tab ${this._activeTab === 'seo' ? 'active' : ''}" @click=${() => this._activeTab = 'seo'}>SEO</div>
      </div>
      <div class="editor-body">
        <div class="tab-content ${this._activeTab === 'brand' ? 'active' : ''}">${this._renderBrandTab()}</div>
        <div class="tab-content ${this._activeTab === 'media' ? 'active' : ''}">${this._renderMediaTab()}</div>
        <div class="tab-content ${this._activeTab === 'seo' ? 'active' : ''}">${this._renderSeoTab()}</div>
      </div>
    `;
  }

  _renderBrandTab() {
    const b = this._editingBrand || {};
    return html`
      <div class="form-section">
        <div class="form-section-title">Basic Information</div>
        <div class="form-group">
          <label>Brand Name *</label>
          <input type="text" .value=${b.name || ''} @input=${e => this._handleInput('name', e.target.value)} placeholder="Enter brand name" />
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>URL Slug</label>
            <div class="input-with-btn">
              <input type="text" .value=${b.slug || ''} @input=${e => this._handleInput('slug', e.target.value)} placeholder="brand-slug" />
              <uui-button look="secondary" compact @click=${this._generateSlug}>Generate</uui-button>
            </div>
            <small>Used in URL: /brand/${b.slug || 'slug'}</small>
          </div>
          <div class="form-group">
            <label>Website URL</label>
            <input type="url" .value=${b.websiteUrl || ''} @input=${e => this._handleInput('websiteUrl', e.target.value)} placeholder="https://www.brand.com" />
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Sort Order</label>
            <input type="number" .value=${b.sortOrder || 0} @input=${e => this._handleInput('sortOrder', parseInt(e.target.value) || 0)} />
          </div>
        </div>
      </div>
      <div class="form-section">
        <div class="form-section-title">Description</div>
        <div class="wysiwyg-toolbar">
          <button @click=${() => this._execCommand('bold')}><strong>B</strong></button>
          <button @click=${() => this._execCommand('italic')}><em>I</em></button>
          <button @click=${() => this._execCommand('underline')}><u>U</u></button>
          <button @click=${() => this._execCommand('insertUnorderedList')}>• List</button>
          <button @click=${() => this._execCommand('insertOrderedList')}>1. List</button>
          <button @click=${() => { const url = prompt('Enter URL:'); if(url) this._execCommand('createLink', url); }}>Link</button>
        </div>
        <div class="wysiwyg-editor" contenteditable="true" @input=${this._handleDescriptionInput} .innerHTML=${b.description || ''}></div>
      </div>
      <div class="form-section">
        <div class="form-section-title">Settings</div>
        <div class="checkbox-row">
          <label class="checkbox-item"><input type="checkbox" .checked=${b.isActive !== false} @change=${e => this._handleInput('isActive', e.target.checked)} /> Active</label>
          <label class="checkbox-item"><input type="checkbox" .checked=${b.isFeatured || false} @change=${e => this._handleInput('isFeatured', e.target.checked)} /> Featured brand</label>
        </div>
      </div>
    `;
  }

  _renderMediaTab() {
    const b = this._editingBrand || {};
    return html`
      <div class="form-section">
        <div class="form-section-title">Brand Logo</div>
        <div class="upload-zone" @click=${() => this.shadowRoot.querySelector('#logoInput').click()} @dragover=${e => { e.preventDefault(); e.currentTarget.classList.add('dragover'); }} @dragleave=${e => e.currentTarget.classList.remove('dragover')} @drop=${this._handleImageDrop}>
          <uui-icon name="icon-picture"></uui-icon>
          <div>Drag & drop logo or click to browse</div>
          <small>Recommended: 400x200px, PNG with transparent background</small>
        </div>
        <input type="file" id="logoInput" accept="image/*" style="display:none" @change=${this._handleImageSelect} />
        ${b.logoUrl ? html`<div class="image-preview"><img src="${b.logoUrl}" /><br/><uui-button look="secondary" compact @click=${() => this._handleInput('logoUrl', '')}>Remove</uui-button></div>` : ''}
        <div class="form-group" style="margin-top:16px;">
          <label>Or enter logo URL</label>
          <input type="text" .value=${b.logoUrl || ''} @input=${e => this._handleInput('logoUrl', e.target.value)} placeholder="https://..." />
        </div>
      </div>
    `;
  }

  _renderSeoTab() {
    const b = this._editingBrand || {};
    return html`
      <div class="form-section">
        <div class="form-section-title">Search Engine Optimization</div>
        <div class="form-group">
          <label>Meta Title</label>
          <input type="text" .value=${b.metaTitle || ''} @input=${e => this._handleInput('metaTitle', e.target.value)} placeholder="Page title for search engines" maxlength="60" />
          <small>${(b.metaTitle || '').length}/60 characters</small>
        </div>
        <div class="form-group">
          <label>Meta Description</label>
          <textarea .value=${b.metaDescription || ''} @input=${e => this._handleInput('metaDescription', e.target.value)} placeholder="Brief description for search results" maxlength="160" rows="3"></textarea>
          <small>${(b.metaDescription || '').length}/160 characters</small>
        </div>
      </div>
      <div class="form-section">
        <div class="form-section-title">Search Preview</div>
        <div class="seo-preview">
          <div class="seo-preview-title">${b.metaTitle || b.name || 'Brand Name'}</div>
          <div class="seo-preview-url">https://yourstore.com/brand/${b.slug || 'brand-slug'}</div>
          <div class="seo-preview-desc">${b.metaDescription || 'Add a meta description to see how your brand page will appear in search results.'}</div>
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-brand-collection', BrandCollection);
export default BrandCollection;
