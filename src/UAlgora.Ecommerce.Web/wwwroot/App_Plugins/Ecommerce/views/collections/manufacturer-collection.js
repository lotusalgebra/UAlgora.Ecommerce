import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

export class ManufacturerCollection extends UmbElementMixin(LitElement) {
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
    .list-item-meta { font-size: 12px; color: #666; display: flex; gap: 8px; flex-wrap: wrap; }
    .badge { padding: 2px 6px; border-radius: 3px; font-size: 10px; font-weight: 500; }
    .badge-active { background: #d4edda; color: #155724; }
    .badge-inactive { background: #f8d7da; color: #721c24; }
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
    .contact-card { background: #f8f9fa; border: 1px solid #e0e0e0; border-radius: 8px; padding: 16px; margin-top: 16px; }
    .contact-item { display: flex; align-items: center; gap: 8px; margin-bottom: 8px; }
    .contact-item:last-child { margin-bottom: 0; }
    .contact-item uui-icon { color: #666; }
    .empty-state { display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100%; color: #666; }
    .empty-state uui-icon { font-size: 64px; margin-bottom: 16px; opacity: 0.5; }
    .loading { display: flex; justify-content: center; align-items: center; height: 100%; }
  `;

  static properties = {
    _manufacturers: { state: true },
    _loading: { state: true },
    _searchTerm: { state: true },
    _mode: { state: true },
    _editingManufacturer: { state: true },
    _activeTab: { state: true },
    _saving: { state: true }
  };

  constructor() {
    super();
    this._manufacturers = [];
    this._loading = true;
    this._searchTerm = '';
    this._mode = 'list';
    this._editingManufacturer = null;
    this._activeTab = 'manufacturer';
    this._saving = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadManufacturers();
  }

  async _loadManufacturers() {
    try {
      this._loading = true;
      const res = await fetch('/umbraco/management/api/v1/ecommerce/manufacturer', { credentials: 'include', headers: { 'Accept': 'application/json' } });
      if (res.ok) {
        const data = await res.json();
        this._manufacturers = data.items || data || [];
      }
    } catch (e) { console.error('Error loading manufacturers:', e); }
    finally { this._loading = false; }
  }

  _getFilteredManufacturers() {
    if (!this._searchTerm) return this._manufacturers;
    const term = this._searchTerm.toLowerCase();
    return this._manufacturers.filter(m => m.name?.toLowerCase().includes(term) || m.country?.toLowerCase().includes(term));
  }

  _selectManufacturer(manufacturer) {
    this._editingManufacturer = { ...manufacturer };
    this._mode = 'edit';
    this._activeTab = 'manufacturer';
  }

  _createNew() {
    this._editingManufacturer = { name: '', slug: '', description: '', logoUrl: '', websiteUrl: '', email: '', phone: '', address: '', city: '', country: '', postalCode: '', isActive: true, sortOrder: 0 };
    this._mode = 'create';
    this._activeTab = 'manufacturer';
  }

  _backToList() {
    this._mode = 'list';
    this._editingManufacturer = null;
  }

  _handleInput(field, value) {
    this._editingManufacturer = { ...this._editingManufacturer, [field]: value };
  }

  _generateSlug() {
    if (this._editingManufacturer?.name) {
      const slug = this._editingManufacturer.name.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '');
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
      const res = await fetch('/umbraco/management/api/v1/ecommerce/media/upload', { method: 'POST', credentials: 'include', body: formData });
      if (res.ok) {
        const data = await res.json();
        this._handleInput('logoUrl', data.url || data.path);
      }
    } catch (e) { console.error('Upload failed:', e); }
  }

  async _save() {
    if (!this._editingManufacturer?.name) { alert('Name is required'); return; }
    this._saving = true;
    try {
      const isNew = !this._editingManufacturer.id;
      const url = isNew ? '/umbraco/management/api/v1/ecommerce/manufacturer' : `/umbraco/management/api/v1/ecommerce/manufacturer/${this._editingManufacturer.id}`;
      const res = await fetch(url, { method: isNew ? 'POST' : 'PUT', credentials: 'include', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(this._editingManufacturer) });
      if (!res.ok) throw new Error('Save failed');
      await this._loadManufacturers();
      if (isNew) {
        const data = await res.json();
        if (data?.id) this._selectManufacturer(data);
        else this._mode = 'list';
      }
    } catch (e) { alert('Failed to save: ' + e.message); }
    finally { this._saving = false; }
  }

  async _delete() {
    if (!confirm(`Delete "${this._editingManufacturer?.name}"?`)) return;
    try {
      await fetch(`/umbraco/management/api/v1/ecommerce/manufacturer/${this._editingManufacturer.id}`, { method: 'DELETE', credentials: 'include' });
      this._loadManufacturers();
      this._backToList();
    } catch (e) { alert('Delete failed'); }
  }

  render() {
    return html`<div class="container">
      <div class="list-panel">
        <div class="list-header">
          <h2>Manufacturers</h2>
          <div class="search-row">
            <uui-input placeholder="Search..." .value=${this._searchTerm} @input=${e => this._searchTerm = e.target.value}><uui-icon name="icon-search" slot="prepend"></uui-icon></uui-input>
            <uui-button look="primary" compact @click=${this._createNew}><uui-icon name="icon-add"></uui-icon></uui-button>
          </div>
        </div>
        <div class="list-content">
          ${this._loading ? html`<div class="loading"><uui-loader></uui-loader></div>` :
            this._getFilteredManufacturers().map(m => html`
              <div class="list-item ${this._editingManufacturer?.id === m.id ? 'active' : ''}" @click=${() => this._selectManufacturer(m)}>
                <div class="list-item-img">${m.logoUrl ? html`<img src="${m.logoUrl}" />` : html`<uui-icon name="icon-factory"></uui-icon>`}</div>
                <div class="list-item-info">
                  <div class="list-item-name">${m.name}</div>
                  <div class="list-item-meta">
                    ${m.country ? html`<span>${m.country}</span>` : ''}
                    <span class="badge ${m.isActive !== false ? 'badge-active' : 'badge-inactive'}">${m.isActive !== false ? 'Active' : 'Inactive'}</span>
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
    return html`<div class="empty-state"><uui-icon name="icon-factory"></uui-icon><h3>Select a manufacturer to edit</h3><p>Or click + to create a new one</p></div>`;
  }

  _renderEditor() {
    const isNew = this._mode === 'create';
    return html`
      <div class="editor-header">
        <div style="display:flex;align-items:center;gap:12px;">
          <uui-button look="secondary" compact @click=${this._backToList}><uui-icon name="icon-arrow-left"></uui-icon></uui-button>
          <h2>${isNew ? 'New Manufacturer' : this._editingManufacturer?.name || 'Edit Manufacturer'}</h2>
        </div>
        <div class="editor-actions">
          ${!isNew ? html`<uui-button look="secondary" color="danger" @click=${this._delete}>Delete</uui-button>` : ''}
          <uui-button look="primary" @click=${this._save} ?disabled=${this._saving}>${this._saving ? 'Saving...' : 'Save'}</uui-button>
        </div>
      </div>
      <div class="tabs">
        <div class="tab ${this._activeTab === 'manufacturer' ? 'active' : ''}" @click=${() => this._activeTab = 'manufacturer'}>Manufacturer</div>
        <div class="tab ${this._activeTab === 'contact' ? 'active' : ''}" @click=${() => this._activeTab = 'contact'}>Contact</div>
        <div class="tab ${this._activeTab === 'media' ? 'active' : ''}" @click=${() => this._activeTab = 'media'}>Media</div>
      </div>
      <div class="editor-body">
        <div class="tab-content ${this._activeTab === 'manufacturer' ? 'active' : ''}">${this._renderManufacturerTab()}</div>
        <div class="tab-content ${this._activeTab === 'contact' ? 'active' : ''}">${this._renderContactTab()}</div>
        <div class="tab-content ${this._activeTab === 'media' ? 'active' : ''}">${this._renderMediaTab()}</div>
      </div>
    `;
  }

  _renderManufacturerTab() {
    const m = this._editingManufacturer || {};
    return html`
      <div class="form-section">
        <div class="form-section-title">Basic Information</div>
        <div class="form-group">
          <label>Manufacturer Name *</label>
          <input type="text" .value=${m.name || ''} @input=${e => this._handleInput('name', e.target.value)} placeholder="Enter manufacturer name" />
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>URL Slug</label>
            <div class="input-with-btn">
              <input type="text" .value=${m.slug || ''} @input=${e => this._handleInput('slug', e.target.value)} placeholder="manufacturer-slug" />
              <uui-button look="secondary" compact @click=${this._generateSlug}>Generate</uui-button>
            </div>
            <small>Used in URL: /manufacturer/${m.slug || 'slug'}</small>
          </div>
          <div class="form-group">
            <label>Website URL</label>
            <input type="url" .value=${m.websiteUrl || ''} @input=${e => this._handleInput('websiteUrl', e.target.value)} placeholder="https://www.manufacturer.com" />
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Sort Order</label>
            <input type="number" .value=${m.sortOrder || 0} @input=${e => this._handleInput('sortOrder', parseInt(e.target.value) || 0)} />
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
        <div class="wysiwyg-editor" contenteditable="true" @input=${this._handleDescriptionInput} .innerHTML=${m.description || ''}></div>
      </div>
      <div class="form-section">
        <div class="form-section-title">Settings</div>
        <div class="checkbox-row">
          <label class="checkbox-item"><input type="checkbox" .checked=${m.isActive !== false} @change=${e => this._handleInput('isActive', e.target.checked)} /> Active</label>
        </div>
      </div>
    `;
  }

  _renderContactTab() {
    const m = this._editingManufacturer || {};
    return html`
      <div class="form-section">
        <div class="form-section-title">Contact Information</div>
        <div class="form-row">
          <div class="form-group">
            <label>Email</label>
            <input type="email" .value=${m.email || ''} @input=${e => this._handleInput('email', e.target.value)} placeholder="contact@manufacturer.com" />
          </div>
          <div class="form-group">
            <label>Phone</label>
            <input type="tel" .value=${m.phone || ''} @input=${e => this._handleInput('phone', e.target.value)} placeholder="+1 234 567 8900" />
          </div>
        </div>
      </div>
      <div class="form-section">
        <div class="form-section-title">Address</div>
        <div class="form-group">
          <label>Street Address</label>
          <input type="text" .value=${m.address || ''} @input=${e => this._handleInput('address', e.target.value)} placeholder="123 Manufacturing St" />
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>City</label>
            <input type="text" .value=${m.city || ''} @input=${e => this._handleInput('city', e.target.value)} placeholder="City" />
          </div>
          <div class="form-group">
            <label>Postal Code</label>
            <input type="text" .value=${m.postalCode || ''} @input=${e => this._handleInput('postalCode', e.target.value)} placeholder="12345" />
          </div>
        </div>
        <div class="form-group">
          <label>Country</label>
          <input type="text" .value=${m.country || ''} @input=${e => this._handleInput('country', e.target.value)} placeholder="United States" />
        </div>
      </div>
      ${(m.email || m.phone || m.address) ? html`
        <div class="form-section">
          <div class="form-section-title">Contact Summary</div>
          <div class="contact-card">
            ${m.email ? html`<div class="contact-item"><uui-icon name="icon-message"></uui-icon><span>${m.email}</span></div>` : ''}
            ${m.phone ? html`<div class="contact-item"><uui-icon name="icon-phone"></uui-icon><span>${m.phone}</span></div>` : ''}
            ${m.address ? html`<div class="contact-item"><uui-icon name="icon-map-location"></uui-icon><span>${[m.address, m.city, m.postalCode, m.country].filter(Boolean).join(', ')}</span></div>` : ''}
            ${m.websiteUrl ? html`<div class="contact-item"><uui-icon name="icon-globe"></uui-icon><a href="${m.websiteUrl}" target="_blank">${m.websiteUrl}</a></div>` : ''}
          </div>
        </div>
      ` : ''}
    `;
  }

  _renderMediaTab() {
    const m = this._editingManufacturer || {};
    return html`
      <div class="form-section">
        <div class="form-section-title">Manufacturer Logo</div>
        <div class="upload-zone" @click=${() => this.shadowRoot.querySelector('#logoInput').click()} @dragover=${e => { e.preventDefault(); e.currentTarget.classList.add('dragover'); }} @dragleave=${e => e.currentTarget.classList.remove('dragover')} @drop=${this._handleImageDrop}>
          <uui-icon name="icon-picture"></uui-icon>
          <div>Drag & drop logo or click to browse</div>
          <small>Recommended: 400x200px, PNG with transparent background</small>
        </div>
        <input type="file" id="logoInput" accept="image/*" style="display:none" @change=${this._handleImageSelect} />
        ${m.logoUrl ? html`<div class="image-preview"><img src="${m.logoUrl}" /><br/><uui-button look="secondary" compact @click=${() => this._handleInput('logoUrl', '')}>Remove</uui-button></div>` : ''}
        <div class="form-group" style="margin-top:16px;">
          <label>Or enter logo URL</label>
          <input type="text" .value=${m.logoUrl || ''} @input=${e => this._handleInput('logoUrl', e.target.value)} placeholder="https://..." />
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-manufacturer-collection', ManufacturerCollection);
export default ManufacturerCollection;
