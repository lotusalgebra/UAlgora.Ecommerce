import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";

export class CategoryCollection extends UmbElementMixin(LitElement) {
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
    .list-item-img { width: 40px; height: 40px; border-radius: 4px; object-fit: cover; background: #e0e0e0; display: flex; align-items: center; justify-content: center; }
    .list-item-info { flex: 1; min-width: 0; }
    .list-item-name { font-weight: 600; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .list-item-meta { font-size: 12px; color: #666; display: flex; gap: 8px; flex-wrap: wrap; }
    .badge { padding: 2px 6px; border-radius: 3px; font-size: 10px; font-weight: 500; }
    .badge-visible { background: #d4edda; color: #155724; }
    .badge-hidden { background: #f8d7da; color: #721c24; }
    .badge-featured { background: #fff3cd; color: #856404; }
    .badge-sub { background: #e2e3e5; color: #383d41; }
    .editor-header { padding: 20px 24px; border-bottom: 1px solid #e0e0e0; display: flex; justify-content: space-between; align-items: center; background: #fff; position: sticky; top: 0; z-index: 10; }
    .editor-header h2 { margin: 0; font-size: 20px; }
    .editor-actions { display: flex; gap: 8px; }
    .editor-body { padding: 24px; }
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
    .image-preview { margin-top: 16px; }
    .image-preview img { max-width: 200px; max-height: 150px; border-radius: 8px; border: 1px solid #ddd; }
    .seo-preview { background: #fff; border: 1px solid #ddd; border-radius: 8px; padding: 16px; margin-top: 16px; }
    .seo-preview-title { color: #1a0dab; font-size: 18px; margin-bottom: 4px; text-decoration: underline; cursor: pointer; }
    .seo-preview-url { color: #006621; font-size: 14px; margin-bottom: 4px; }
    .seo-preview-desc { color: #545454; font-size: 13px; line-height: 1.4; }
    .subcategory-list { display: flex; flex-wrap: wrap; gap: 8px; }
    .subcategory-chip { padding: 6px 12px; background: #e3f2fd; border-radius: 16px; font-size: 13px; color: #1976d2; cursor: pointer; }
    .subcategory-chip:hover { background: #bbdefb; }
    .stats-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 16px; margin-bottom: 24px; }
    .stat-card { background: #f8f9fa; border-radius: 8px; padding: 16px; text-align: center; }
    .stat-value { font-size: 24px; font-weight: 700; color: #1976d2; }
    .stat-label { font-size: 12px; color: #666; margin-top: 4px; }
    .product-mini { display: flex; align-items: center; gap: 10px; padding: 8px; border: 1px solid #e0e0e0; border-radius: 4px; margin-bottom: 8px; }
    .product-mini img { width: 40px; height: 40px; border-radius: 4px; object-fit: cover; }
    .product-mini-info { flex: 1; }
    .product-mini-name { font-weight: 500; font-size: 13px; }
    .product-mini-sku { font-size: 11px; color: #666; }
    .empty-state { display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100%; color: #666; }
    .empty-state uui-icon { font-size: 64px; margin-bottom: 16px; opacity: 0.5; }
    .loading { display: flex; justify-content: center; align-items: center; height: 100%; }
  `;

  static properties = {
    _categories: { state: true },
    _loading: { state: true },
    _searchTerm: { state: true },
    _mode: { state: true },
    _editingCategory: { state: true },
    _activeTab: { state: true },
    _saving: { state: true },
    _productsInCategory: { state: true }
  };

  constructor() {
    super();
    this._categories = [];
    this._loading = true;
    this._searchTerm = '';
    this._mode = 'list';
    this._editingCategory = null;
    this._activeTab = 'category';
    this._saving = false;
    this._productsInCategory = [];
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadCategories();
  }

  async _loadCategories() {
    try {
      this._loading = true;
      const res = await this._authFetch('/umbraco/management/api/v1/ecommerce/category');
      if (res.ok) {
        const data = await res.json();
        this._categories = data.items || data || [];
      }
    } catch (e) { console.error('Error loading categories:', e); }
    finally { this._loading = false; }
  }

  async _loadProductsInCategory(categoryId) {
    try {
      const res = await this._authFetch(`/umbraco/management/api/v1/ecommerce/product?categoryId=${categoryId}&take=10`);
      if (res.ok) {
        const data = await res.json();
        this._productsInCategory = data.items || [];
      }
    } catch (e) { this._productsInCategory = []; }
  }

  _getFilteredCategories() {
    if (!this._searchTerm) return this._categories;
    const term = this._searchTerm.toLowerCase();
    return this._categories.filter(c => c.name?.toLowerCase().includes(term) || c.slug?.toLowerCase().includes(term));
  }

  _getParentCategories() {
    return this._categories.filter(c => !c.parentId && c.id !== this._editingCategory?.id);
  }

  _getSubcategories(parentId) {
    return this._categories.filter(c => c.parentId === parentId);
  }

  _selectCategory(category) {
    this._editingCategory = { ...category };
    this._mode = 'edit';
    this._activeTab = 'category';
    this._loadProductsInCategory(category.id);
  }

  _createNew() {
    this._editingCategory = { name: '', slug: '', description: '', isVisible: true, isFeatured: false, sortOrder: 0, parentId: null, imageUrl: '', bannerUrl: '', metaTitle: '', metaDescription: '', metaKeywords: '' };
    this._mode = 'create';
    this._activeTab = 'category';
    this._productsInCategory = [];
  }

  _backToList() {
    this._mode = 'list';
    this._editingCategory = null;
  }

  _handleInput(field, value) {
    this._editingCategory = { ...this._editingCategory, [field]: value };
  }

  _generateSlug() {
    if (this._editingCategory?.name) {
      const slug = this._editingCategory.name.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '');
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

  _handleImageDrop(e, field) {
    e.preventDefault();
    e.currentTarget.classList.remove('dragover');
    const file = e.dataTransfer?.files?.[0] || e.target.files?.[0];
    if (file && file.type.startsWith('image/')) this._uploadImage(file, field);
  }

  _handleImageSelect(e, field) {
    const file = e.target.files?.[0];
    if (file) this._uploadImage(file, field);
  }

  async _uploadImage(file, field) {
    const formData = new FormData();
    formData.append('file', file);
    try {
      const headers = await this._getAuthHeaders();
      delete headers['Content-Type'];
      const res = await fetch('/umbraco/management/api/v1/ecommerce/media/upload', { method: 'POST', headers, body: formData });
      if (res.ok) {
        const data = await res.json();
        this._handleInput(field, data.url || data.path);
      }
    } catch (e) { console.error('Upload failed:', e); }
  }

  async _save() {
    if (!this._editingCategory?.name) { alert('Name is required'); return; }
    this._saving = true;
    try {
      const isNew = !this._editingCategory.id;
      const url = isNew ? '/umbraco/management/api/v1/ecommerce/category' : `/umbraco/management/api/v1/ecommerce/category/${this._editingCategory.id}`;
      const res = await this._authFetch(url, { method: isNew ? 'POST' : 'PUT', body: JSON.stringify(this._editingCategory) });
      if (!res.ok) throw new Error('Save failed');
      await this._loadCategories();
      if (isNew) {
        const data = await res.json();
        if (data?.id) this._selectCategory(data);
        else this._mode = 'list';
      }
    } catch (e) { alert('Failed to save: ' + e.message); }
    finally { this._saving = false; }
  }

  async _delete() {
    if (!confirm(`Delete "${this._editingCategory?.name}"?`)) return;
    try {
      await this._authFetch(`/umbraco/management/api/v1/ecommerce/category/${this._editingCategory.id}`, { method: 'DELETE' });
      this._loadCategories();
      this._backToList();
    } catch (e) { alert('Delete failed'); }
  }

  render() {
    return html`<div class="container">
      <div class="list-panel">
        <div class="list-header">
          <h2>Categories</h2>
          <div class="search-row">
            <uui-input placeholder="Search..." .value=${this._searchTerm} @input=${e => this._searchTerm = e.target.value}><uui-icon name="icon-search" slot="prepend"></uui-icon></uui-input>
            <uui-button look="primary" compact @click=${this._createNew}><uui-icon name="icon-add"></uui-icon></uui-button>
          </div>
        </div>
        <div class="list-content">
          ${this._loading ? html`<div class="loading"><uui-loader></uui-loader></div>` :
            this._getFilteredCategories().map(c => html`
              <div class="list-item ${this._editingCategory?.id === c.id ? 'active' : ''}" @click=${() => this._selectCategory(c)}>
                <div class="list-item-img">${c.imageUrl ? html`<img src="${c.imageUrl}" />` : html`<uui-icon name="icon-folder"></uui-icon>`}</div>
                <div class="list-item-info">
                  <div class="list-item-name">${c.name}</div>
                  <div class="list-item-meta">
                    ${c.slug ? html`<span>/${c.slug}</span>` : ''}
                    ${c.parentId ? html`<span class="badge badge-sub">Sub</span>` : ''}
                    <span class="badge ${c.isVisible !== false ? 'badge-visible' : 'badge-hidden'}">${c.isVisible !== false ? 'Visible' : 'Hidden'}</span>
                    ${c.isFeatured ? html`<span class="badge badge-featured">Featured</span>` : ''}
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
    return html`<div class="empty-state"><uui-icon name="icon-folder"></uui-icon><h3>Select a category to edit</h3><p>Or click + to create a new one</p></div>`;
  }

  _renderEditor() {
    const isNew = this._mode === 'create';
    return html`
      <div class="editor-header">
        <div style="display:flex;align-items:center;gap:12px;">
          <uui-button look="secondary" compact @click=${this._backToList}><uui-icon name="icon-arrow-left"></uui-icon></uui-button>
          <h2>${isNew ? 'New Category' : this._editingCategory?.name || 'Edit Category'}</h2>
        </div>
        <div class="editor-actions">
          ${!isNew ? html`<uui-button look="secondary" color="danger" @click=${this._delete}>Delete</uui-button>` : ''}
          <uui-button look="primary" @click=${this._save} ?disabled=${this._saving}>${this._saving ? 'Saving...' : 'Save'}</uui-button>
        </div>
      </div>
      <div class="tabs">
        <div class="tab ${this._activeTab === 'category' ? 'active' : ''}" @click=${() => this._activeTab = 'category'}>Category</div>
        <div class="tab ${this._activeTab === 'media' ? 'active' : ''}" @click=${() => this._activeTab = 'media'}>Media</div>
        <div class="tab ${this._activeTab === 'seo' ? 'active' : ''}" @click=${() => this._activeTab = 'seo'}>SEO</div>
        ${!isNew ? html`<div class="tab ${this._activeTab === 'products' ? 'active' : ''}" @click=${() => this._activeTab = 'products'}>Products</div>` : ''}
      </div>
      <div class="editor-body">
        <div class="tab-content ${this._activeTab === 'category' ? 'active' : ''}">${this._renderCategoryTab()}</div>
        <div class="tab-content ${this._activeTab === 'media' ? 'active' : ''}">${this._renderMediaTab()}</div>
        <div class="tab-content ${this._activeTab === 'seo' ? 'active' : ''}">${this._renderSeoTab()}</div>
        ${!isNew ? html`<div class="tab-content ${this._activeTab === 'products' ? 'active' : ''}">${this._renderProductsTab()}</div>` : ''}
      </div>
    `;
  }

  _renderCategoryTab() {
    const c = this._editingCategory || {};
    return html`
      <div class="form-section">
        <div class="form-section-title">Basic Information</div>
        <div class="form-group">
          <label>Category Name *</label>
          <input type="text" .value=${c.name || ''} @input=${e => this._handleInput('name', e.target.value)} placeholder="Enter category name" />
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>URL Slug</label>
            <div class="input-with-btn">
              <input type="text" .value=${c.slug || ''} @input=${e => this._handleInput('slug', e.target.value)} placeholder="category-slug" />
              <uui-button look="secondary" compact @click=${this._generateSlug}>Generate</uui-button>
            </div>
            <small>Used in URL: /category/${c.slug || 'slug'}</small>
          </div>
          <div class="form-group">
            <label>Parent Category</label>
            <select .value=${c.parentId || ''} @change=${e => this._handleInput('parentId', e.target.value || null)}>
              <option value="">None (Top Level)</option>
              ${this._getParentCategories().map(p => html`<option value="${p.id}" ?selected=${c.parentId === p.id}>${p.name}</option>`)}
            </select>
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Sort Order</label>
            <input type="number" .value=${c.sortOrder || 0} @input=${e => this._handleInput('sortOrder', parseInt(e.target.value) || 0)} />
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
        <div class="wysiwyg-editor" contenteditable="true" @input=${this._handleDescriptionInput} .innerHTML=${c.description || ''}></div>
      </div>
      <div class="form-section">
        <div class="form-section-title">Visibility</div>
        <div class="checkbox-row">
          <label class="checkbox-item"><input type="checkbox" .checked=${c.isVisible !== false} @change=${e => this._handleInput('isVisible', e.target.checked)} /> Visible in store</label>
          <label class="checkbox-item"><input type="checkbox" .checked=${c.isFeatured || false} @change=${e => this._handleInput('isFeatured', e.target.checked)} /> Featured category</label>
        </div>
      </div>
    `;
  }

  _renderMediaTab() {
    const c = this._editingCategory || {};
    return html`
      <div class="form-section">
        <div class="form-section-title">Category Image</div>
        <div class="upload-zone" @click=${() => this.shadowRoot.querySelector('#imageInput').click()} @dragover=${e => { e.preventDefault(); e.currentTarget.classList.add('dragover'); }} @dragleave=${e => e.currentTarget.classList.remove('dragover')} @drop=${e => this._handleImageDrop(e, 'imageUrl')}>
          <uui-icon name="icon-picture"></uui-icon>
          <div>Drag & drop image or click to browse</div>
          <small>Recommended: 400x400px, PNG or JPG</small>
        </div>
        <input type="file" id="imageInput" accept="image/*" style="display:none" @change=${e => this._handleImageSelect(e, 'imageUrl')} />
        ${c.imageUrl ? html`<div class="image-preview"><img src="${c.imageUrl}" /><br/><uui-button look="secondary" compact @click=${() => this._handleInput('imageUrl', '')}>Remove</uui-button></div>` : ''}
        <div class="form-group" style="margin-top:16px;">
          <label>Or enter image URL</label>
          <input type="text" .value=${c.imageUrl || ''} @input=${e => this._handleInput('imageUrl', e.target.value)} placeholder="https://..." />
        </div>
      </div>
      <div class="form-section">
        <div class="form-section-title">Banner Image</div>
        <div class="upload-zone" @click=${() => this.shadowRoot.querySelector('#bannerInput').click()} @dragover=${e => { e.preventDefault(); e.currentTarget.classList.add('dragover'); }} @dragleave=${e => e.currentTarget.classList.remove('dragover')} @drop=${e => this._handleImageDrop(e, 'bannerUrl')}>
          <uui-icon name="icon-picture"></uui-icon>
          <div>Drag & drop banner or click to browse</div>
          <small>Recommended: 1200x300px, PNG or JPG</small>
        </div>
        <input type="file" id="bannerInput" accept="image/*" style="display:none" @change=${e => this._handleImageSelect(e, 'bannerUrl')} />
        ${c.bannerUrl ? html`<div class="image-preview"><img src="${c.bannerUrl}" style="max-width:400px;" /><br/><uui-button look="secondary" compact @click=${() => this._handleInput('bannerUrl', '')}>Remove</uui-button></div>` : ''}
      </div>
    `;
  }

  _renderSeoTab() {
    const c = this._editingCategory || {};
    return html`
      <div class="form-section">
        <div class="form-section-title">Search Engine Optimization</div>
        <div class="form-group">
          <label>Meta Title</label>
          <input type="text" .value=${c.metaTitle || ''} @input=${e => this._handleInput('metaTitle', e.target.value)} placeholder="Page title for search engines" maxlength="60" />
          <small>${(c.metaTitle || '').length}/60 characters</small>
        </div>
        <div class="form-group">
          <label>Meta Description</label>
          <textarea .value=${c.metaDescription || ''} @input=${e => this._handleInput('metaDescription', e.target.value)} placeholder="Brief description for search results" maxlength="160" rows="3"></textarea>
          <small>${(c.metaDescription || '').length}/160 characters</small>
        </div>
        <div class="form-group">
          <label>Meta Keywords</label>
          <input type="text" .value=${c.metaKeywords || ''} @input=${e => this._handleInput('metaKeywords', e.target.value)} placeholder="keyword1, keyword2, keyword3" />
        </div>
      </div>
      <div class="form-section">
        <div class="form-section-title">Search Preview</div>
        <div class="seo-preview">
          <div class="seo-preview-title">${c.metaTitle || c.name || 'Category Title'}</div>
          <div class="seo-preview-url">https://yourstore.com/category/${c.slug || 'category-slug'}</div>
          <div class="seo-preview-desc">${c.metaDescription || 'Add a meta description to see how your category will appear in search results.'}</div>
        </div>
      </div>
    `;
  }

  _renderProductsTab() {
    const c = this._editingCategory || {};
    const subcategories = this._getSubcategories(c.id);
    return html`
      <div class="stats-grid">
        <div class="stat-card"><div class="stat-value">${this._productsInCategory.length}</div><div class="stat-label">Products</div></div>
        <div class="stat-card"><div class="stat-value">${subcategories.length}</div><div class="stat-label">Subcategories</div></div>
        <div class="stat-card"><div class="stat-value">${c.sortOrder || 0}</div><div class="stat-label">Sort Order</div></div>
      </div>
      ${subcategories.length > 0 ? html`
        <div class="form-section">
          <div class="form-section-title">Subcategories</div>
          <div class="subcategory-list">${subcategories.map(s => html`<span class="subcategory-chip" @click=${() => this._selectCategory(s)}>${s.name}</span>`)}</div>
        </div>
      ` : ''}
      <div class="form-section">
        <div class="form-section-title">Products in this Category</div>
        ${this._productsInCategory.length === 0 ? html`<p style="color:#666;">No products in this category yet.</p>` :
          html`<div class="products-in-category">
            ${this._productsInCategory.map(p => html`
              <div class="product-mini">
                ${p.images?.[0] ? html`<img src="${p.images[0]}" />` : html`<div style="width:40px;height:40px;background:#f0f0f0;border-radius:4px;display:flex;align-items:center;justify-content:center;"><uui-icon name="icon-picture"></uui-icon></div>`}
                <div class="product-mini-info">
                  <div class="product-mini-name">${p.name}</div>
                  <div class="product-mini-sku">${p.sku || 'No SKU'}</div>
                </div>
              </div>
            `)}
          </div>`}
      </div>
    `;
  }
}

customElements.define('ecommerce-category-collection', CategoryCollection);
export default CategoryCollection;
