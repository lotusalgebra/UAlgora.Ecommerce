import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

export class ProductCollection extends UmbElementMixin(LitElement) {
  static styles = css`
    :host { display: block; padding: 20px; }
    .collection-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
    .search-box { flex: 1; max-width: 400px; }
    .search-box uui-input { width: 100%; }
    .search-box uui-icon { display: flex; align-items: center; padding: 0 8px; color: #666; }
    .collection-table { width: 100%; border-collapse: collapse; background: #fff; border: 1px solid #e0e0e0; border-radius: 8px; }
    .collection-table th, .collection-table td { padding: 12px 16px; text-align: left; border-bottom: 1px solid #e0e0e0; }
    .collection-table th { background: #f5f5f5; font-weight: 600; }
    .collection-table tr:hover { background: #f9f9f9; cursor: pointer; }
    .product-image { width: 50px; height: 50px; border-radius: 4px; background: #f0f0f0; display: flex; align-items: center; justify-content: center; }
    .product-image img { width: 100%; height: 100%; object-fit: cover; border-radius: 4px; }
    .badge { display: inline-block; padding: 4px 8px; border-radius: 4px; font-size: 12px; font-weight: 500; }
    .badge-active { background: #d4edda; color: #155724; }
    .badge-inactive { background: #f8d7da; color: #721c24; }
    .badge-featured { background: #fff3cd; color: #856404; margin-left: 4px; }
    .pagination { display: flex; justify-content: space-between; margin-top: 20px; padding: 12px; background: #f5f5f5; border-radius: 8px; }
    .empty-state { text-align: center; padding: 60px 20px; color: #666; }
    .empty-state uui-icon { font-size: 48px; margin-bottom: 16px; }
    .loading { display: flex; justify-content: center; padding: 40px; }
    .editor-container { background: #fff; border: 1px solid #e0e0e0; border-radius: 8px; }
    .editor-header { display: flex; justify-content: space-between; padding: 20px; border-bottom: 1px solid #e0e0e0; }
    .editor-header h2 { margin: 0; font-size: 20px; }
    .editor-actions { display: flex; gap: 10px; }
    .tabs { display: flex; border-bottom: 1px solid #e0e0e0; background: #fafafa; }
    .tab { padding: 14px 24px; cursor: pointer; border-bottom: 3px solid transparent; font-weight: 500; color: #666; }
    .tab:hover { background: #f0f0f0; }
    .tab.active { border-bottom-color: #1b264f; color: #1b264f; background: #fff; }
    .tab-content { padding: 24px; display: none; }
    .tab-content.active { display: block; }
    .form-group { margin-bottom: 20px; }
    .form-group label { display: block; margin-bottom: 8px; font-weight: 500; }
    .form-group input, .form-group select, .form-group textarea { width: 100%; padding: 10px; border: 1px solid #ddd; border-radius: 6px; font-size: 14px; box-sizing: border-box; }
    .form-group input:focus, .form-group select:focus { outline: none; border-color: #1b264f; }
    .form-row { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 20px; }
    .form-row-3 { display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px; }
    .form-hint { font-size: 12px; color: #888; margin-top: 4px; }
    .required::after { content: ' *'; color: #dc3545; }
    .wysiwyg-toolbar { display: flex; gap: 4px; padding: 8px; background: #f5f5f5; border: 1px solid #ddd; border-bottom: none; border-radius: 6px 6px 0 0; }
    .wysiwyg-toolbar button { padding: 6px 10px; border: 1px solid #ddd; background: #fff; cursor: pointer; border-radius: 4px; }
    .wysiwyg-editor { min-height: 200px; padding: 12px; border: 1px solid #ddd; border-radius: 0 0 6px 6px; }
    .dropdown-with-add { display: flex; gap: 8px; }
    .dropdown-with-add select { flex: 1; }
    .inline-add-form { display: flex; gap: 8px; margin-top: 8px; padding: 12px; background: #f8f9fa; border-radius: 6px; }
    .inline-add-form input { flex: 1; }
    .media-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(150px, 1fr)); gap: 16px; }
    .media-item { position: relative; aspect-ratio: 1; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden; }
    .media-item img { width: 100%; height: 100%; object-fit: cover; }
    .media-item-actions { position: absolute; top: 8px; right: 8px; display: flex; gap: 4px; }
    .media-item-actions button { width: 28px; height: 28px; border: none; border-radius: 4px; cursor: pointer; }
    .media-item.primary { border: 2px solid #1b264f; }
    .media-upload-zone { border: 2px dashed #ccc; border-radius: 8px; padding: 40px; text-align: center; cursor: pointer; }
    .media-upload-zone:hover { border-color: #1b264f; background: #f8f9fa; }
    .bulk-url-section { margin-top: 20px; padding: 16px; background: #f8f9fa; border-radius: 8px; }
    .variant-generator { padding: 20px; background: #f8f9fa; border-radius: 8px; margin-bottom: 20px; }
    .attribute-row { display: flex; gap: 12px; margin-bottom: 12px; }
    .attribute-row input { flex: 1; }
    .attribute-values { display: flex; flex-wrap: wrap; gap: 6px; margin-bottom: 16px; }
    .attribute-value-tag { display: inline-flex; align-items: center; gap: 4px; padding: 4px 10px; background: #e9ecef; border-radius: 20px; font-size: 13px; }
    .attribute-value-tag button { border: none; background: none; cursor: pointer; }
    .variants-table { width: 100%; border-collapse: collapse; margin-top: 20px; }
    .variants-table th, .variants-table td { padding: 10px; border: 1px solid #e0e0e0; }
    .variants-table th { background: #f5f5f5; }
    .variants-table input { width: 100%; padding: 6px; border: 1px solid #ddd; border-radius: 4px; }
    .csv-import { margin-top: 20px; padding: 16px; background: #fff3cd; border-radius: 8px; }
    .pricing-analysis { margin-top: 20px; padding: 20px; background: #f8f9fa; border-radius: 8px; }
    .analysis-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; }
    .analysis-item { text-align: center; padding: 16px; background: white; border-radius: 8px; }
    .analysis-item .value { font-size: 24px; font-weight: 700; color: #1b264f; }
    .analysis-item .label { font-size: 12px; color: #666; margin-top: 4px; }
    .section-divider { margin: 24px 0; padding-top: 12px; border-top: 1px solid #e0e0e0; }
    .section-divider h4 { margin: 0; color: #666; font-size: 14px; text-transform: uppercase; }
    .toggle-group { display: flex; align-items: center; gap: 12px; }
    .delete-confirm { padding: 20px; background: #fff3cd; border-radius: 8px; margin: 20px; }
    .stock-warning { padding: 12px; background: #f8d7da; border-radius: 6px; color: #721c24; margin-top: 12px; }
    .stock-ok { padding: 12px; background: #d4edda; border-radius: 6px; color: #155724; margin-top: 12px; }
    .category-multiselect { border: 1px solid #ddd; border-radius: 6px; padding: 8px; max-height: 200px; overflow-y: auto; background: #fff; }
    .category-item { display: flex; align-items: center; gap: 8px; padding: 6px 4px; border-radius: 4px; cursor: pointer; }
    .category-item:hover { background: #f5f5f5; }
    .category-item input { width: 16px; height: 16px; margin: 0; }
    .category-item.child { padding-left: 24px; }
    .selected-categories { display: flex; flex-wrap: wrap; gap: 6px; margin-top: 8px; }
    .category-tag { display: inline-flex; align-items: center; gap: 4px; padding: 4px 8px; background: #e3f2fd; border-radius: 4px; font-size: 12px; }
    .category-tag button { border: none; background: none; cursor: pointer; padding: 0 2px; font-size: 14px; }
  `;

  static properties = {
    _mode: { state: true }, _products: { state: true }, _loading: { state: true }, _saving: { state: true },
    _searchTerm: { state: true }, _page: { state: true }, _pageSize: { state: true }, _totalCount: { state: true },
    _activeTab: { state: true }, _product: { state: true }, _categories: { state: true }, _manufacturers: { state: true },
    _brands: { state: true }, _currencies: { state: true }, _taxCategories: { state: true }, _warehouses: { state: true },
    _showAddManufacturer: { state: true }, _showAddBrand: { state: true }, _newManufacturerName: { state: true },
    _newBrandName: { state: true }, _variantAttributes: { state: true }, _showDeleteConfirm: { state: true }
  };

  constructor() {
    super();
    this._mode = 'list'; this._products = []; this._loading = true; this._saving = false;
    this._searchTerm = ''; this._page = 1; this._pageSize = 25; this._totalCount = 0;
    this._activeTab = 'product'; this._product = this._getEmptyProduct();
    this._categories = []; this._manufacturers = []; this._brands = [];
    this._currencies = []; this._taxCategories = []; this._warehouses = [];
    this._showAddManufacturer = false; this._showAddBrand = false;
    this._newManufacturerName = ''; this._newBrandName = '';
    this._variantAttributes = []; this._showDeleteConfirm = false;
  }

  _getEmptyProduct() {
    return { name: '', sku: '', description: '', shortDescription: '', price: 0, costPrice: 0, salePrice: null,
      currencyCode: 'USD', taxCategoryId: null, taxIncluded: false, categoryIds: [], manufacturerId: null, brandId: null,
      stockQuantity: 0, lowStockThreshold: 5, manageStock: true, weight: 0, length: 0, width: 0, height: 0,
      isActive: true, isFeatured: false, isDigital: false, metaTitle: '', metaDescription: '', metaKeywords: '', slug: '',
      images: [], variants: [] };
  }

  connectedCallback() { super.connectedCallback(); this._loadAll(); }

  async _loadAll() {
    this._loadProducts(); this._loadCategories(); this._loadManufacturers();
    this._loadBrands(); this._loadCurrencies(); this._loadTaxCategories(); this._loadWarehouses();
  }

  async _loadProducts() {
    try { this._loading = true;
      const skip = (this._page - 1) * this._pageSize;
      const params = new URLSearchParams({ skip: skip.toString(), take: this._pageSize.toString() });
      if (this._searchTerm) params.append('search', this._searchTerm);
      const r = await fetch(`/umbraco/management/api/v1/ecommerce/product?${params}`, { credentials: 'include' });
      if (r.ok) { const d = await r.json(); this._products = d.items || []; this._totalCount = d.total || 0; }
    } catch (e) { console.error(e); } finally { this._loading = false; }
  }

  async _loadCategories() { try { const r = await fetch('/umbraco/management/api/v1/ecommerce/category', { credentials: 'include' }); if (r.ok) this._categories = (await r.json()).items || []; } catch (e) {} }
  async _loadManufacturers() { try { const r = await fetch('/umbraco/management/api/v1/ecommerce/manufacturer', { credentials: 'include' }); if (r.ok) this._manufacturers = await r.json(); } catch (e) { this._manufacturers = []; } }
  async _loadBrands() { try { const r = await fetch('/umbraco/management/api/v1/ecommerce/brand', { credentials: 'include' }); if (r.ok) this._brands = await r.json(); } catch (e) { this._brands = []; } }
  async _loadCurrencies() { try { const r = await fetch('/umbraco/management/api/v1/ecommerce/currency', { credentials: 'include' }); if (r.ok) { const d = await r.json(); this._currencies = d.items || d || []; } } catch (e) { console.error('Error loading currencies:', e); } }
  async _loadTaxCategories() { try { const r = await fetch('/umbraco/management/api/v1/ecommerce/taxcategory', { credentials: 'include' }); if (r.ok) this._taxCategories = await r.json(); } catch (e) {} }
  async _loadWarehouses() { try { const r = await fetch('/umbraco/management/api/v1/ecommerce/warehouse', { credentials: 'include' }); if (r.ok) this._warehouses = await r.json(); } catch (e) {} }

  _handleSearch(e) { this._searchTerm = e.target.value; this._page = 1; clearTimeout(this._st); this._st = setTimeout(() => this._loadProducts(), 300); }
  _handlePageChange(d) { const tp = Math.ceil(this._totalCount / this._pageSize); if (d === 'prev' && this._page > 1) { this._page--; this._loadProducts(); } else if (d === 'next' && this._page < tp) { this._page++; this._loadProducts(); } }
  _openCreateMode() { this._mode = 'create'; this._activeTab = 'product'; this._product = this._getEmptyProduct(); this._variantAttributes = []; this._showDeleteConfirm = false; }
  async _openEditMode(p) { this._loading = true; try { const r = await fetch(`/umbraco/management/api/v1/ecommerce/product/${p.id}`, { credentials: 'include' }); if (r.ok) { this._product = await r.json(); this._mode = 'edit'; this._activeTab = 'product'; } } catch (e) {} finally { this._loading = false; } }
  _backToList() { this._mode = 'list'; this._product = this._getEmptyProduct(); this._loadProducts(); }
  _handleInputChange(f, v) { this._product = { ...this._product, [f]: v }; }
  _toggleCategory(id) { const ids = this._product.categoryIds || []; if (ids.includes(id)) { this._product = { ...this._product, categoryIds: ids.filter(i => i !== id) }; } else { this._product = { ...this._product, categoryIds: [...ids, id] }; } }
  _formatPrice(p, c = 'USD') { return new Intl.NumberFormat('en-US', { style: 'currency', currency: c }).format(p || 0); }
  _execCommand(c, v = null) { document.execCommand(c, false, v); this._updateDescription(); }
  _updateDescription() { const e = this.shadowRoot.querySelector('.wysiwyg-editor'); if (e) this._product = { ...this._product, description: e.innerHTML }; }

  async _addManufacturer() { if (!this._newManufacturerName.trim()) return; try { const r = await fetch('/umbraco/management/api/v1/ecommerce/manufacturer', { method: 'POST', credentials: 'include', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ name: this._newManufacturerName }) }); if (r.ok) { const m = await r.json(); await this._loadManufacturers(); this._product = { ...this._product, manufacturerId: m.id }; this._showAddManufacturer = false; this._newManufacturerName = ''; } } catch (e) {} }
  async _addBrand() { if (!this._newBrandName.trim()) return; try { const r = await fetch('/umbraco/management/api/v1/ecommerce/brand', { method: 'POST', credentials: 'include', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ name: this._newBrandName }) }); if (r.ok) { const b = await r.json(); await this._loadBrands(); this._product = { ...this._product, brandId: b.id }; this._showAddBrand = false; this._newBrandName = ''; } } catch (e) {} }

  _handleFileUpload(e) { this._uploadFiles(e.target.files); }
  _handleDragOver(e) { e.preventDefault(); e.currentTarget.classList.add('dragover'); }
  _handleDragLeave(e) { e.currentTarget.classList.remove('dragover'); }
  _handleDrop(e) { e.preventDefault(); e.currentTarget.classList.remove('dragover'); this._uploadFiles(e.dataTransfer.files); }
  async _uploadFiles(files) { for (const f of files) { if (!f.type.startsWith('image/')) continue; const fd = new FormData(); fd.append('file', f); try { const r = await fetch('/umbraco/management/api/v1/ecommerce/media/upload', { method: 'POST', credentials: 'include', body: fd }); if (r.ok) { const res = await r.json(); this._product = { ...this._product, images: [...(this._product.images || []), { url: res.url, isPrimary: !this._product.images?.length }] }; } } catch (e) {} } }
  _handleBulkUrlAdd() { const t = this.shadowRoot.querySelector('#bulk-urls'); if (!t) return; const urls = t.value.split('\n').map(u => u.trim()).filter(u => u); this._product = { ...this._product, images: [...(this._product.images || []), ...urls.map((u, i) => ({ url: u, isPrimary: !this._product.images?.length && i === 0 }))] }; t.value = ''; }
  _setAsPrimary(i) { this._product = { ...this._product, images: this._product.images.map((img, idx) => ({ ...img, isPrimary: idx === i })) }; }
  _removeImage(i) { const imgs = [...this._product.images]; const wasPrimary = imgs[i].isPrimary; imgs.splice(i, 1); if (wasPrimary && imgs.length) imgs[0].isPrimary = true; this._product = { ...this._product, images: imgs }; }

  _addAttribute() { this._variantAttributes = [...this._variantAttributes, { name: '', values: [] }]; }
  _updateAttributeName(i, n) { const a = [...this._variantAttributes]; a[i] = { ...a[i], name: n }; this._variantAttributes = a; }
  _addAttributeValue(i) { const inp = this.shadowRoot.querySelector(`#attr-value-${i}`); if (!inp || !inp.value.trim()) return; const a = [...this._variantAttributes]; a[i] = { ...a[i], values: [...a[i].values, inp.value.trim()] }; this._variantAttributes = a; inp.value = ''; }
  _removeAttributeValue(ai, vi) { const a = [...this._variantAttributes]; a[ai] = { ...a[ai], values: a[ai].values.filter((_, i) => i !== vi) }; this._variantAttributes = a; }
  _removeAttribute(i) { this._variantAttributes = this._variantAttributes.filter((_, idx) => idx !== i); }
  _generateVariants() { if (!this._variantAttributes.length) return; const combos = this._cartesianProduct(this._variantAttributes.map(a => a.values)); const vars = combos.map(c => { const attrs = {}; this._variantAttributes.forEach((a, i) => { attrs[a.name] = c[i]; }); return { sku: `${this._product.sku}-${c.join('-').toUpperCase().replace(/\s+/g, '')}`, attributes: attrs, price: this._product.price, costPrice: this._product.costPrice, stockQuantity: 0, barcode: '' }; }); this._product = { ...this._product, variants: vars }; }
  _cartesianProduct(arr) { if (!arr.length) return [[]]; const [f, ...r] = arr; const rp = this._cartesianProduct(r); return f.flatMap(x => rp.map(y => [x, ...y])); }
  _updateVariant(i, f, v) { const vars = [...this._product.variants]; vars[i] = { ...vars[i], [f]: v }; this._product = { ...this._product, variants: vars }; }
  _removeVariant(i) { this._product = { ...this._product, variants: this._product.variants.filter((_, idx) => idx !== i) }; }

  _calculateMargin() { const p = parseFloat(this._product.price) || 0, c = parseFloat(this._product.costPrice) || 0; return p ? ((p - c) / p * 100).toFixed(1) : 0; }
  _calculateMarkup() { const p = parseFloat(this._product.price) || 0, c = parseFloat(this._product.costPrice) || 0; return c ? ((p - c) / c * 100).toFixed(1) : 0; }
  _calculateProfit() { return (parseFloat(this._product.price) || 0) - (parseFloat(this._product.costPrice) || 0); }
  _calculateBreakeven() { const m = parseFloat(this._calculateMargin()) || 0; return m ? Math.ceil(100 / m) : '-'; }

  async _saveProduct() { if (!this._product.name || !this._product.sku) { alert('Name and SKU required'); return; } this._saving = true; try { const isNew = this._mode === 'create'; const url = isNew ? '/umbraco/management/api/v1/ecommerce/product' : `/umbraco/management/api/v1/ecommerce/product/${this._product.id}`; const r = await fetch(url, { method: isNew ? 'POST' : 'PUT', credentials: 'include', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(this._product) }); if (!r.ok) throw new Error('Failed'); this._backToList(); } catch (e) { alert('Failed: ' + e.message); } finally { this._saving = false; } }
  async _deleteProduct() { this._saving = true; try { const r = await fetch(`/umbraco/management/api/v1/ecommerce/product/${this._product.id}`, { method: 'DELETE', credentials: 'include' }); if (!r.ok) throw new Error('Failed'); this._backToList(); } catch (e) { alert('Failed: ' + e.message); } finally { this._saving = false; } }

  render() { if (this._loading && this._mode === 'list') return html`<div class="loading"><uui-loader></uui-loader></div>`; if (this._mode !== 'list') return this._renderEditor(); return this._renderList(); }

  _renderList() { const tp = Math.ceil(this._totalCount / this._pageSize), s = ((this._page - 1) * this._pageSize) + 1, e = Math.min(this._page * this._pageSize, this._totalCount); return html`<div class="collection-header"><div class="search-box"><uui-input placeholder="Search products..." .value=${this._searchTerm} @input=${this._handleSearch}><uui-icon name="icon-search" slot="prepend"></uui-icon></uui-input></div><uui-button look="primary" @click=${this._openCreateMode}><uui-icon name="icon-add"></uui-icon> Add Product</uui-button></div>${this._products.length === 0 ? html`<div class="empty-state"><uui-icon name="icon-box"></uui-icon><h3>No products</h3><p>${this._searchTerm ? 'Try different search' : 'Add your first product'}</p>${!this._searchTerm ? html`<uui-button look="primary" @click=${this._openCreateMode}>Add Product</uui-button>` : ''}</div>` : html`<table class="collection-table"><thead><tr><th style="width:60px"></th><th>Product</th><th>SKU</th><th>Price</th><th>Stock</th><th>Status</th></tr></thead><tbody>${this._products.map(p => html`<tr @click=${() => this._openEditMode(p)}><td><div class="product-image">${p.imageUrl ? html`<img src="${p.imageUrl}" alt="${p.name}"/>` : html`<uui-icon name="icon-box"></uui-icon>`}</div></td><td><strong>${p.name}</strong>${p.isFeatured ? html`<span class="badge badge-featured">Featured</span>` : ''}</td><td>${p.sku}</td><td>${this._formatPrice(p.price)}</td><td>${p.stockQuantity ?? '-'}</td><td><span class="badge ${p.isActive ? 'badge-active' : 'badge-inactive'}">${p.isActive ? 'Active' : 'Inactive'}</span></td></tr>`)}</tbody></table><div class="pagination"><div>Showing ${s}-${e} of ${this._totalCount}</div><div style="display:flex;gap:8px"><uui-button look="secondary" compact ?disabled=${this._page === 1} @click=${() => this._handlePageChange('prev')}>Previous</uui-button><uui-button look="secondary" compact ?disabled=${this._page >= tp} @click=${() => this._handlePageChange('next')}>Next</uui-button></div></div>`}`; }

  _renderEditor() { const isNew = this._mode === 'create'; return html`<div class="editor-container"><div class="editor-header"><div><uui-button look="secondary" compact @click=${this._backToList}><uui-icon name="icon-arrow-left"></uui-icon> Back</uui-button><h2 style="display:inline;margin-left:16px">${isNew ? 'Add Product' : `Edit: ${this._product.name}`}</h2></div><div class="editor-actions">${!isNew ? html`<uui-button look="secondary" color="danger" @click=${() => this._showDeleteConfirm = true} ?disabled=${this._saving}><uui-icon name="icon-delete"></uui-icon> Delete</uui-button>` : ''}<uui-button look="primary" @click=${this._saveProduct} ?disabled=${this._saving}>${this._saving ? 'Saving...' : 'Save'}</uui-button></div></div>${this._showDeleteConfirm ? html`<div class="delete-confirm"><p><strong>Delete product?</strong></p><uui-button look="secondary" @click=${() => this._showDeleteConfirm = false}>Cancel</uui-button> <uui-button look="primary" color="danger" @click=${this._deleteProduct}>Delete</uui-button></div>` : ''}<div class="tabs"><div class="tab ${this._activeTab === 'product' ? 'active' : ''}" @click=${() => this._activeTab = 'product'}>Product</div><div class="tab ${this._activeTab === 'pricing' ? 'active' : ''}" @click=${() => this._activeTab = 'pricing'}>Pricing</div><div class="tab ${this._activeTab === 'inventory' ? 'active' : ''}" @click=${() => this._activeTab = 'inventory'}>Inventory</div><div class="tab ${this._activeTab === 'media' ? 'active' : ''}" @click=${() => this._activeTab = 'media'}>Media</div><div class="tab ${this._activeTab === 'variants' ? 'active' : ''}" @click=${() => this._activeTab = 'variants'}>Variants</div><div class="tab ${this._activeTab === 'seo' ? 'active' : ''}" @click=${() => this._activeTab = 'seo'}>SEO</div></div><div class="tab-content ${this._activeTab === 'product' ? 'active' : ''}">${this._renderProductTab()}</div><div class="tab-content ${this._activeTab === 'pricing' ? 'active' : ''}">${this._renderPricingTab()}</div><div class="tab-content ${this._activeTab === 'inventory' ? 'active' : ''}">${this._renderInventoryTab()}</div><div class="tab-content ${this._activeTab === 'media' ? 'active' : ''}">${this._renderMediaTab()}</div><div class="tab-content ${this._activeTab === 'variants' ? 'active' : ''}">${this._renderVariantsTab()}</div><div class="tab-content ${this._activeTab === 'seo' ? 'active' : ''}">${this._renderSeoTab()}</div></div>`; }

  _renderProductTab() {
    const categoryIds = this._product.categoryIds || [];
    const parentCategories = this._categories.filter(c => !c.parentId);
    const getCategoryName = (id) => this._categories.find(c => c.id === id)?.name || id;
    return html`<div class="form-row"><div class="form-group"><label class="required">Product Name</label><input type="text" .value=${this._product.name || ''} @input=${(e) => this._handleInputChange('name', e.target.value)} placeholder="Product name"/></div><div class="form-group"><label class="required">SKU</label><input type="text" .value=${this._product.sku || ''} @input=${(e) => this._handleInputChange('sku', e.target.value)} placeholder="PROD-001"/></div></div><div class="form-group"><label>Short Description</label><textarea rows="2" .value=${this._product.shortDescription || ''} @input=${(e) => this._handleInputChange('shortDescription', e.target.value)}></textarea></div><div class="form-group"><label>Full Description</label><div class="wysiwyg-toolbar"><button type="button" @click=${() => this._execCommand('bold')}><b>B</b></button><button type="button" @click=${() => this._execCommand('italic')}><i>I</i></button><button type="button" @click=${() => this._execCommand('underline')}><u>U</u></button><button type="button" @click=${() => this._execCommand('insertUnorderedList')}>List</button><button type="button" @click=${() => this._execCommand('formatBlock', 'h2')}>H2</button><button type="button" @click=${() => this._execCommand('formatBlock', 'h3')}>H3</button><button type="button" @click=${() => { const u = prompt('URL:'); if (u) this._execCommand('createLink', u); }}>Link</button></div><div class="wysiwyg-editor" contenteditable="true" @input=${this._updateDescription} .innerHTML=${this._product.description || ''}></div></div><div class="section-divider"><h4>Organization</h4></div><div class="form-row-3"><div class="form-group"><label>Categories</label><div class="category-multiselect">${parentCategories.map(parent => { const children = this._categories.filter(c => c.parentId === parent.id); return html`<div class="category-item" @click=${(e) => { if (e.target.tagName !== 'INPUT') this._toggleCategory(parent.id); }}><input type="checkbox" .checked=${categoryIds.includes(parent.id)} @change=${() => this._toggleCategory(parent.id)}/><strong>${parent.name}</strong></div>${children.map(child => html`<div class="category-item child" @click=${(e) => { if (e.target.tagName !== 'INPUT') this._toggleCategory(child.id); }}><input type="checkbox" .checked=${categoryIds.includes(child.id)} @change=${() => this._toggleCategory(child.id)}/><span>${child.name}</span></div>`)}`; })}</div>${categoryIds.length > 0 ? html`<div class="selected-categories">${categoryIds.map(id => html`<span class="category-tag">${getCategoryName(id)}<button type="button" @click=${() => this._toggleCategory(id)}>&times;</button></span>`)}</div>` : ''}</div><div class="form-group"><label>Manufacturer</label><div class="dropdown-with-add"><select .value=${this._product.manufacturerId || ''} @change=${(e) => this._handleInputChange('manufacturerId', e.target.value || null)}><option value="">-- Select --</option>${this._manufacturers.map(m => html`<option value="${m.id}" ?selected=${this._product.manufacturerId === m.id}>${m.name}</option>`)}</select><button type="button" @click=${() => this._showAddManufacturer = !this._showAddManufacturer}>+</button></div>${this._showAddManufacturer ? html`<div class="inline-add-form"><input type="text" placeholder="Name" .value=${this._newManufacturerName} @input=${(e) => this._newManufacturerName = e.target.value}/><uui-button look="primary" compact @click=${this._addManufacturer}>Add</uui-button></div>` : ''}</div><div class="form-group"><label>Brand</label><div class="dropdown-with-add"><select .value=${this._product.brandId || ''} @change=${(e) => this._handleInputChange('brandId', e.target.value || null)}><option value="">-- Select --</option>${this._brands.map(b => html`<option value="${b.id}" ?selected=${this._product.brandId === b.id}>${b.name}</option>`)}</select><button type="button" @click=${() => this._showAddBrand = !this._showAddBrand}>+</button></div>${this._showAddBrand ? html`<div class="inline-add-form"><input type="text" placeholder="Name" .value=${this._newBrandName} @input=${(e) => this._newBrandName = e.target.value}/><uui-button look="primary" compact @click=${this._addBrand}>Add</uui-button></div>` : ''}</div></div><div class="section-divider"><h4>Settings</h4></div><div class="form-row-3"><div class="form-group"><div class="toggle-group"><input type="checkbox" id="isActive" ?checked=${this._product.isActive} @change=${(e) => this._handleInputChange('isActive', e.target.checked)}/><label for="isActive">Active</label></div></div><div class="form-group"><div class="toggle-group"><input type="checkbox" id="isFeatured" ?checked=${this._product.isFeatured} @change=${(e) => this._handleInputChange('isFeatured', e.target.checked)}/><label for="isFeatured">Featured</label></div></div><div class="form-group"><div class="toggle-group"><input type="checkbox" id="isDigital" ?checked=${this._product.isDigital} @change=${(e) => this._handleInputChange('isDigital', e.target.checked)}/><label for="isDigital">Digital</label></div></div></div>`; }

  _renderPricingTab() { return html`<div class="form-row-3"><div class="form-group"><label class="required">Price</label><input type="number" step="0.01" min="0" .value=${this._product.price || 0} @input=${(e) => this._handleInputChange('price', parseFloat(e.target.value) || 0)}/></div><div class="form-group"><label>Cost Price</label><input type="number" step="0.01" min="0" .value=${this._product.costPrice || 0} @input=${(e) => this._handleInputChange('costPrice', parseFloat(e.target.value) || 0)}/><div class="form-hint">Your cost</div></div><div class="form-group"><label>Sale Price</label><input type="number" step="0.01" min="0" .value=${this._product.salePrice || ''} @input=${(e) => this._handleInputChange('salePrice', e.target.value ? parseFloat(e.target.value) : null)} placeholder="Empty = no sale"/></div></div><div class="section-divider"><h4>Currency & Tax</h4></div><div class="form-row-3"><div class="form-group"><label>Currency</label><select .value=${this._product.currencyCode || 'USD'} @change=${(e) => this._handleInputChange('currencyCode', e.target.value)}>${this._currencies.map(c => html`<option value="${c.code}" ?selected=${this._product.currencyCode === c.code}>${c.code} - ${c.name}</option>`)}</select></div><div class="form-group"><label>Tax Category</label><select .value=${this._product.taxCategoryId || ''} @change=${(e) => this._handleInputChange('taxCategoryId', e.target.value || null)}><option value="">-- No Tax --</option>${this._taxCategories.map(t => html`<option value="${t.id}" ?selected=${this._product.taxCategoryId === t.id}>${t.name} (${t.rate}%)</option>`)}</select></div><div class="form-group"><div class="toggle-group" style="margin-top:30px"><input type="checkbox" id="taxIncluded" ?checked=${this._product.taxIncluded} @change=${(e) => this._handleInputChange('taxIncluded', e.target.checked)}/><label for="taxIncluded">Tax Included</label></div></div></div><div class="pricing-analysis"><h4>Pricing Analysis</h4><div class="analysis-grid"><div class="analysis-item"><div class="value">${this._calculateMargin()}%</div><div class="label">Margin</div></div><div class="analysis-item"><div class="value">${this._calculateMarkup()}%</div><div class="label">Markup</div></div><div class="analysis-item"><div class="value">${this._formatPrice(this._calculateProfit())}</div><div class="label">Profit</div></div><div class="analysis-item"><div class="value">${this._calculateBreakeven()}</div><div class="label">Break-even</div></div></div></div>`; }

  _renderInventoryTab() { const stk = parseInt(this._product.stockQuantity) || 0, thr = parseInt(this._product.lowStockThreshold) || 5; return html`<div class="form-row-3"><div class="form-group"><label>Stock</label><input type="number" min="0" .value=${this._product.stockQuantity || 0} @input=${(e) => this._handleInputChange('stockQuantity', parseInt(e.target.value) || 0)}/></div><div class="form-group"><label>Low Stock Threshold</label><input type="number" min="0" .value=${this._product.lowStockThreshold || 5} @input=${(e) => this._handleInputChange('lowStockThreshold', parseInt(e.target.value) || 5)}/></div><div class="form-group"><div class="toggle-group" style="margin-top:30px"><input type="checkbox" id="manageStock" ?checked=${this._product.manageStock} @change=${(e) => this._handleInputChange('manageStock', e.target.checked)}/><label for="manageStock">Track Stock</label></div></div></div>${this._product.manageStock ? (stk <= thr ? html`<div class="stock-warning"><strong>Low Stock:</strong> ${stk} units</div>` : html`<div class="stock-ok"><strong>Stock OK:</strong> ${stk} units</div>`) : ''}<div class="section-divider"><h4>Dimensions</h4></div><div class="form-row"><div class="form-group"><label>Weight (kg)</label><input type="number" step="0.01" min="0" .value=${this._product.weight || 0} @input=${(e) => this._handleInputChange('weight', parseFloat(e.target.value) || 0)}/></div><div class="form-group"><label>Length (cm)</label><input type="number" step="0.1" min="0" .value=${this._product.length || 0} @input=${(e) => this._handleInputChange('length', parseFloat(e.target.value) || 0)}/></div><div class="form-group"><label>Width (cm)</label><input type="number" step="0.1" min="0" .value=${this._product.width || 0} @input=${(e) => this._handleInputChange('width', parseFloat(e.target.value) || 0)}/></div><div class="form-group"><label>Height (cm)</label><input type="number" step="0.1" min="0" .value=${this._product.height || 0} @input=${(e) => this._handleInputChange('height', parseFloat(e.target.value) || 0)}/></div></div>`; }

  _renderMediaTab() { return html`<div class="media-upload-zone" @click=${() => this.shadowRoot.querySelector('#file-upload').click()} @dragover=${this._handleDragOver} @dragleave=${this._handleDragLeave} @drop=${this._handleDrop}><uui-icon name="icon-cloud-upload" style="font-size:48px;color:#999"></uui-icon><h3>Drag & drop images</h3><p>or click to browse</p><input type="file" id="file-upload" multiple accept="image/*" style="display:none" @change=${this._handleFileUpload}/></div><div class="bulk-url-section"><h4>Bulk Add by URL</h4><textarea id="bulk-urls" placeholder="Paste URLs, one per line"></textarea><uui-button look="secondary" @click=${this._handleBulkUrlAdd} style="margin-top:12px">Add URLs</uui-button></div>${this._product.images?.length ? html`<div class="section-divider"><h4>Images (${this._product.images.length})</h4></div><div class="media-grid">${this._product.images.map((img, i) => html`<div class="media-item ${img.isPrimary ? 'primary' : ''}"><img src="${img.url}" alt="Image ${i+1}"/><div class="media-item-actions">${!img.isPrimary ? html`<button type="button" @click=${() => this._setAsPrimary(i)} style="background:#28a745;color:white">&#9733;</button>` : ''}<button type="button" @click=${() => this._removeImage(i)} style="background:#dc3545;color:white">&times;</button></div></div>`)}</div>` : ''}`; }

  _renderVariantsTab() { return html`<div class="variant-generator"><h4>Variant Generator</h4><p style="color:#666;margin-bottom:16px">Add attributes to generate variants automatically.</p>${this._variantAttributes.map((a, i) => html`<div class="attribute-row"><input type="text" placeholder="Attribute (Size)" .value=${a.name} @input=${(e) => this._updateAttributeName(i, e.target.value)}/><input type="text" id="attr-value-${i}" placeholder="Value, Enter" @keypress=${(e) => e.key === 'Enter' && this._addAttributeValue(i)}/><uui-button look="secondary" compact @click=${() => this._addAttributeValue(i)}>Add</uui-button><uui-button look="secondary" compact color="danger" @click=${() => this._removeAttribute(i)}>&times;</uui-button></div><div class="attribute-values">${a.values.map((v, vi) => html`<span class="attribute-value-tag">${v}<button type="button" @click=${() => this._removeAttributeValue(i, vi)}>&times;</button></span>`)}</div>`)}<div style="margin-top:16px"><uui-button look="secondary" @click=${this._addAttribute}>+ Add Attribute</uui-button>${this._variantAttributes.length && this._variantAttributes.every(a => a.name && a.values.length) ? html`<uui-button look="primary" @click=${this._generateVariants} style="margin-left:8px">Generate</uui-button>` : ''}</div></div>${this._product.variants?.length ? html`<h4>Variants (${this._product.variants.length})</h4><table class="variants-table"><thead><tr><th>SKU</th><th>Attributes</th><th>Price</th><th>Cost</th><th>Stock</th><th>Barcode</th><th></th></tr></thead><tbody>${this._product.variants.map((v, i) => html`<tr><td><input type="text" .value=${v.sku} @input=${(e) => this._updateVariant(i, 'sku', e.target.value)}/></td><td>${Object.entries(v.attributes || {}).map(([k, val]) => `${k}: ${val}`).join(', ')}</td><td><input type="number" step="0.01" .value=${v.price} @input=${(e) => this._updateVariant(i, 'price', parseFloat(e.target.value) || 0)}/></td><td><input type="number" step="0.01" .value=${v.costPrice} @input=${(e) => this._updateVariant(i, 'costPrice', parseFloat(e.target.value) || 0)}/></td><td><input type="number" .value=${v.stockQuantity} @input=${(e) => this._updateVariant(i, 'stockQuantity', parseInt(e.target.value) || 0)}/></td><td><input type="text" .value=${v.barcode || ''} @input=${(e) => this._updateVariant(i, 'barcode', e.target.value)} placeholder="UPC"/></td><td><uui-button look="secondary" compact color="danger" @click=${() => this._removeVariant(i)}>&times;</uui-button></td></tr>`)}</tbody></table>` : ''}<div class="csv-import"><h4>Bulk Import</h4><p>Download <a href="#" @click=${(e) => { e.preventDefault(); this._downloadVariantTemplate(); }}>CSV template</a></p><input type="file" accept=".csv" @change=${this._handleVariantCsvUpload}/></div>`; }

  _downloadVariantTemplate() { const csv = 'SKU,Attr1Name,Attr1Value,Attr2Name,Attr2Value,Price,Cost,Stock,Barcode\nPROD-S-RED,Size,Small,Color,Red,29.99,15,100,123456'; const a = document.createElement('a'); a.href = URL.createObjectURL(new Blob([csv], { type: 'text/csv' })); a.download = 'variant-template.csv'; a.click(); }
  _handleVariantCsvUpload(e) { const f = e.target.files[0]; if (!f) return; const r = new FileReader(); r.onload = (ev) => { const lines = ev.target.result.split('\n').slice(1); const vars = lines.filter(l => l.trim()).map(l => { const [sku, a1n, a1v, a2n, a2v, price, cost, stock, barcode] = l.split(','); const attrs = {}; if (a1n && a1v) attrs[a1n] = a1v; if (a2n && a2v) attrs[a2n] = a2v; return { sku, attributes: attrs, price: parseFloat(price) || 0, costPrice: parseFloat(cost) || 0, stockQuantity: parseInt(stock) || 0, barcode: barcode || '' }; }); this._product = { ...this._product, variants: [...(this._product.variants || []), ...vars] }; }; r.readAsText(f); }

  _renderSeoTab() { return html`<div class="form-group"><label>URL Slug</label><input type="text" .value=${this._product.slug || ''} @input=${(e) => this._handleInputChange('slug', e.target.value)} placeholder="product-name"/><div class="form-hint">Leave empty to auto-generate</div></div><div class="form-group"><label>Meta Title</label><input type="text" .value=${this._product.metaTitle || ''} @input=${(e) => this._handleInputChange('metaTitle', e.target.value)} maxlength="60"/><div class="form-hint">${(this._product.metaTitle || '').length}/60</div></div><div class="form-group"><label>Meta Description</label><textarea rows="3" .value=${this._product.metaDescription || ''} @input=${(e) => this._handleInputChange('metaDescription', e.target.value)} maxlength="160"></textarea><div class="form-hint">${(this._product.metaDescription || '').length}/160</div></div><div class="form-group"><label>Keywords</label><input type="text" .value=${this._product.metaKeywords || ''} @input=${(e) => this._handleInputChange('metaKeywords', e.target.value)} placeholder="keyword1, keyword2"/></div><div class="section-divider"><h4>SEO Preview</h4></div><div style="padding:16px;background:#fff;border:1px solid #ddd;border-radius:8px"><div style="color:#1a0dab;font-size:18px;margin-bottom:4px">${this._product.metaTitle || this._product.name || 'Product Title'}</div><div style="color:#006621;font-size:14px;margin-bottom:4px">yourstore.com/products/${this._product.slug || this._product.name?.toLowerCase().replace(/\s+/g, '-') || 'product-slug'}</div><div style="color:#545454;font-size:14px">${this._product.metaDescription || this._product.shortDescription || 'Description...'}</div></div>`; }
}

customElements.define('ecommerce-product-collection', ProductCollection);
export default ProductCollection;
