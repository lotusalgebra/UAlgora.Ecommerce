import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Full Product Editor Component with Tabs
 */
export class ProductEditor extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      height: 100%;
      background: #f5f5f5;
    }

    .editor-container {
      height: 100%;
      display: flex;
      flex-direction: column;
    }

    .editor-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px 24px;
      background: linear-gradient(135deg, #ecfdf5 0%, #d1fae5 100%);
      border-bottom: 3px solid #10b981;
    }

    .header-title {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .header-title h1 {
      margin: 0;
      font-size: 22px;
      color: #065f46;
    }

    .algora-badge {
      background: #10b981;
      color: white;
      padding: 4px 12px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: 600;
    }

    .header-actions {
      display: flex;
      gap: 10px;
    }

    .tabs-container {
      background: white;
      border-bottom: 1px solid #e0e0e0;
      padding: 0 24px;
    }

    .tabs {
      display: flex;
      gap: 0;
    }

    .tab {
      padding: 14px 20px;
      cursor: pointer;
      border: none;
      background: none;
      font-size: 14px;
      font-weight: 500;
      color: #666;
      border-bottom: 3px solid transparent;
      transition: all 0.2s;
    }

    .tab:hover {
      color: #10b981;
      background: #f9fafb;
    }

    .tab.active {
      color: #10b981;
      border-bottom-color: #10b981;
    }

    .tab-content {
      flex: 1;
      overflow: auto;
      padding: 24px;
    }

    .form-section {
      background: white;
      border: 1px solid #e0e0e0;
      border-radius: 8px;
      padding: 20px;
      margin-bottom: 20px;
    }

    .form-section h3 {
      margin: 0 0 16px 0;
      padding-bottom: 12px;
      border-bottom: 1px solid #e0e0e0;
      color: #333;
      font-size: 16px;
      font-weight: 600;
    }

    .form-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
    }

    .form-grid-3 {
      display: grid;
      grid-template-columns: 1fr 1fr 1fr;
      gap: 16px;
    }

    .form-group {
      margin-bottom: 16px;
    }

    .form-group.full-width {
      grid-column: 1 / -1;
    }

    .form-group label {
      display: block;
      margin-bottom: 6px;
      font-weight: 500;
      color: #333;
      font-size: 14px;
    }

    .form-group .hint {
      font-size: 12px;
      color: #666;
      margin-top: 4px;
    }

    .form-group input,
    .form-group textarea,
    .form-group select {
      width: 100%;
      padding: 10px 12px;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 14px;
      box-sizing: border-box;
    }

    .form-group input:focus,
    .form-group textarea:focus,
    .form-group select:focus {
      outline: none;
      border-color: #10b981;
      box-shadow: 0 0 0 2px rgba(16, 185, 129, 0.1);
    }

    .form-group textarea {
      min-height: 100px;
      resize: vertical;
    }

    .checkbox-group {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px 0;
    }

    .checkbox-group input[type="checkbox"] {
      width: 18px;
      height: 18px;
    }

    .btn {
      padding: 10px 20px;
      border: none;
      border-radius: 4px;
      font-size: 14px;
      font-weight: 500;
      cursor: pointer;
      transition: all 0.2s;
    }

    .btn-primary {
      background: #10b981;
      color: white;
    }

    .btn-primary:hover {
      background: #059669;
    }

    .btn-secondary {
      background: #e5e7eb;
      color: #374151;
    }

    .btn-secondary:hover {
      background: #d1d5db;
    }

    .btn-danger {
      background: #ef4444;
      color: white;
    }

    .btn-danger:hover {
      background: #dc2626;
    }

    .btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .loading {
      display: flex;
      justify-content: center;
      align-items: center;
      padding: 60px;
      color: #666;
    }

    .toast {
      position: fixed;
      bottom: 20px;
      right: 20px;
      padding: 12px 20px;
      border-radius: 4px;
      color: white;
      font-weight: 500;
      z-index: 10000;
    }

    .toast.success { background: #10b981; }
    .toast.error { background: #ef4444; }

    /* Images section */
    .image-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
      gap: 16px;
    }

    .image-item {
      position: relative;
      border: 1px solid #ddd;
      border-radius: 8px;
      overflow: hidden;
      aspect-ratio: 1;
      background: #f5f5f5;
    }

    .image-item img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .image-item .remove-btn {
      position: absolute;
      top: 8px;
      right: 8px;
      background: #ef4444;
      color: white;
      border: none;
      border-radius: 50%;
      width: 24px;
      height: 24px;
      cursor: pointer;
      font-size: 14px;
    }

    .add-image-btn {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      border: 2px dashed #ddd;
      border-radius: 8px;
      aspect-ratio: 1;
      cursor: pointer;
      color: #666;
      transition: all 0.2s;
    }

    .add-image-btn:hover {
      border-color: #10b981;
      color: #10b981;
    }

    /* Categories section */
    .category-list {
      max-height: 300px;
      overflow-y: auto;
      border: 1px solid #ddd;
      border-radius: 4px;
      padding: 8px;
    }

    .category-item {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px;
      border-radius: 4px;
    }

    .category-item:hover {
      background: #f5f5f5;
    }

    /* Variants section */
    .variant-table {
      width: 100%;
      border-collapse: collapse;
    }

    .variant-table th,
    .variant-table td {
      padding: 12px;
      text-align: left;
      border-bottom: 1px solid #e0e0e0;
    }

    .variant-table th {
      background: #f5f5f5;
      font-weight: 600;
    }

    .variant-table input {
      width: 100%;
      padding: 8px;
      border: 1px solid #ddd;
      border-radius: 4px;
    }

    .add-variant-btn {
      margin-top: 16px;
    }

    /* Attributes section */
    .attribute-row {
      display: flex;
      gap: 12px;
      align-items: center;
      margin-bottom: 12px;
    }

    .attribute-row input {
      flex: 1;
    }

    .attribute-row .remove-attr-btn {
      background: #ef4444;
      color: white;
      border: none;
      border-radius: 4px;
      padding: 8px 12px;
      cursor: pointer;
    }
  `;

  static properties = {
    productId: { type: String },
    _product: { state: true },
    _loading: { state: true },
    _saving: { state: true },
    _toast: { state: true },
    _activeTab: { state: true },
    _categories: { state: true },
    _selectedCategories: { state: true }
  };

  constructor() {
    super();
    this.productId = null;
    this._product = this._createEmptyProduct();
    this._loading = false;
    this._saving = false;
    this._toast = null;
    this._activeTab = 'basic';
    this._categories = [];
    this._selectedCategories = [];
  }

  connectedCallback() {
    super.connectedCallback();
    if (this.productId) {
      this._loadProduct();
    } else {
      this._product = this._createEmptyProduct();
    }
    this._loadCategories();
  }

  updated(changedProperties) {
    if (changedProperties.has('productId') && this.productId) {
      this._loadProduct();
    }
  }

  _createEmptyProduct() {
    return {
      id: null,
      name: '',
      sku: '',
      slug: '',
      shortDescription: '',
      description: '',
      basePrice: 0,
      salePrice: null,
      costPrice: null,
      stockQuantity: 0,
      lowStockThreshold: 5,
      weight: null,
      length: null,
      width: null,
      height: null,
      isVisible: true,
      isFeatured: false,
      isDigital: false,
      metaTitle: '',
      metaDescription: '',
      metaKeywords: '',
      brand: '',
      manufacturer: '',
      barcode: '',
      taxClass: 'standard',
      images: [],
      variants: [],
      attributes: [],
      categoryIds: []
    };
  }

  async _loadProduct() {
    if (!this.productId) return;
    this._loading = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/product/${this.productId}`, {
        credentials: 'include',
        headers: { 'Accept': 'application/json' }
      });

      if (response.ok) {
        const data = await response.json();
        this._product = { ...this._createEmptyProduct(), ...data };
        this._selectedCategories = data.categoryIds || [];
      } else {
        this._showToast('Failed to load product', 'error');
      }
    } catch (error) {
      console.error('Error loading product:', error);
      this._showToast('Error loading product', 'error');
    } finally {
      this._loading = false;
    }
  }

  async _loadCategories() {
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/category?take=100', {
        credentials: 'include',
        headers: { 'Accept': 'application/json' }
      });

      if (response.ok) {
        const data = await response.json();
        this._categories = data.items || [];
      }
    } catch (error) {
      console.error('Error loading categories:', error);
    }
  }

  _updateField(field, value) {
    this._product = { ...this._product, [field]: value };
  }

  async _save() {
    this._saving = true;

    try {
      const isNew = !this._product.id;
      const url = isNew
        ? '/umbraco/management/api/v1/ecommerce/product'
        : `/umbraco/management/api/v1/ecommerce/product/${this._product.id}`;

      const productData = {
        ...this._product,
        categoryIds: this._selectedCategories
      };

      const response = await fetch(url, {
        method: isNew ? 'POST' : 'PUT',
        credentials: 'include',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(productData)
      });

      if (response.ok) {
        const savedProduct = await response.json();
        this._product = savedProduct;
        this._showToast('Product saved successfully!', 'success');

        this.dispatchEvent(new CustomEvent('product-saved', {
          detail: { product: savedProduct },
          bubbles: true,
          composed: true
        }));
      } else {
        this._showToast('Failed to save product', 'error');
      }
    } catch (error) {
      console.error('Error saving product:', error);
      this._showToast('Error saving product', 'error');
    } finally {
      this._saving = false;
    }
  }

  _cancel() {
    this.dispatchEvent(new CustomEvent('editor-cancel', { bubbles: true, composed: true }));
  }

  async _delete() {
    if (!this._product.id) return;
    if (!confirm(`Are you sure you want to delete "${this._product.name}"? This cannot be undone.`)) return;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/product/${this._product.id}`, {
        method: 'DELETE',
        credentials: 'include'
      });

      if (response.ok) {
        this._showToast('Product deleted successfully', 'success');
        this.dispatchEvent(new CustomEvent('editor-cancel', { bubbles: true, composed: true }));
      } else {
        this._showToast('Failed to delete product', 'error');
      }
    } catch (error) {
      this._showToast('Error deleting product', 'error');
    }
  }

  _showToast(message, type) {
    this._toast = { message, type };
    setTimeout(() => { this._toast = null; }, 3000);
  }

  _toggleCategory(categoryId) {
    if (this._selectedCategories.includes(categoryId)) {
      this._selectedCategories = this._selectedCategories.filter(id => id !== categoryId);
    } else {
      this._selectedCategories = [...this._selectedCategories, categoryId];
    }
  }

  _addVariant() {
    const variants = this._product.variants || [];
    this._product = {
      ...this._product,
      variants: [...variants, { id: null, sku: '', name: '', price: 0, stock: 0, attributes: {} }]
    };
  }

  _removeVariant(index) {
    const variants = [...(this._product.variants || [])];
    variants.splice(index, 1);
    this._product = { ...this._product, variants };
  }

  _updateVariant(index, field, value) {
    const variants = [...(this._product.variants || [])];
    variants[index] = { ...variants[index], [field]: value };
    this._product = { ...this._product, variants };
  }

  _addAttribute() {
    const attributes = this._product.attributes || [];
    this._product = {
      ...this._product,
      attributes: [...attributes, { name: '', value: '' }]
    };
  }

  _removeAttribute(index) {
    const attributes = [...(this._product.attributes || [])];
    attributes.splice(index, 1);
    this._product = { ...this._product, attributes };
  }

  _updateAttribute(index, field, value) {
    const attributes = [...(this._product.attributes || [])];
    attributes[index] = { ...attributes[index], [field]: value };
    this._product = { ...this._product, attributes };
  }

  render() {
    if (this._loading) {
      return html`
        <div class="editor-container">
          <div class="loading">
            <uui-loader></uui-loader>
            <span style="margin-left: 10px;">Loading product...</span>
          </div>
        </div>
      `;
    }

    const isNew = !this._product.id;
    const tabs = [
      { id: 'basic', label: 'Basic Info' },
      { id: 'pricing', label: 'Pricing' },
      { id: 'inventory', label: 'Inventory' },
      { id: 'images', label: 'Images' },
      { id: 'categories', label: 'Categories' },
      { id: 'variants', label: 'Variants' },
      { id: 'attributes', label: 'Attributes' },
      { id: 'shipping', label: 'Shipping' },
      { id: 'seo', label: 'SEO' }
    ];

    return html`
      <div class="editor-container">
        <div class="editor-header">
          <div class="header-title">
            <span class="algora-badge">ALGORA</span>
            <h1>${isNew ? 'New Product' : this._product.name || 'Edit Product'}</h1>
          </div>
          <div class="header-actions">
            ${!isNew ? html`<button class="btn btn-danger" @click=${this._delete}>Delete</button>` : ''}
            <button class="btn btn-secondary" @click=${this._cancel}>Cancel</button>
            <button class="btn btn-primary" @click=${this._save} ?disabled=${this._saving}>
              ${this._saving ? 'Saving...' : 'Save Product'}
            </button>
          </div>
        </div>

        <div class="tabs-container">
          <div class="tabs">
            ${tabs.map(tab => html`
              <button class="tab ${this._activeTab === tab.id ? 'active' : ''}"
                      @click=${() => this._activeTab = tab.id}>
                ${tab.label}
              </button>
            `)}
          </div>
        </div>

        <div class="tab-content">
          ${this._renderTabContent()}
        </div>
      </div>

      ${this._toast ? html`<div class="toast ${this._toast.type}">${this._toast.message}</div>` : ''}
    `;
  }

  _renderTabContent() {
    switch (this._activeTab) {
      case 'basic': return this._renderBasicTab();
      case 'pricing': return this._renderPricingTab();
      case 'inventory': return this._renderInventoryTab();
      case 'images': return this._renderImagesTab();
      case 'categories': return this._renderCategoriesTab();
      case 'variants': return this._renderVariantsTab();
      case 'attributes': return this._renderAttributesTab();
      case 'shipping': return this._renderShippingTab();
      case 'seo': return this._renderSeoTab();
      default: return this._renderBasicTab();
    }
  }

  _renderBasicTab() {
    return html`
      <div class="form-section">
        <h3>Basic Information</h3>
        <div class="form-grid">
          <div class="form-group">
            <label>Product Name *</label>
            <input type="text" .value=${this._product.name || ''}
              @input=${(e) => this._updateField('name', e.target.value)}
              placeholder="Enter product name" />
          </div>
          <div class="form-group">
            <label>SKU *</label>
            <input type="text" .value=${this._product.sku || ''}
              @input=${(e) => this._updateField('sku', e.target.value)}
              placeholder="Enter SKU" />
          </div>
          <div class="form-group">
            <label>URL Slug</label>
            <input type="text" .value=${this._product.slug || ''}
              @input=${(e) => this._updateField('slug', e.target.value)}
              placeholder="product-url-slug" />
            <div class="hint">Leave empty to auto-generate from name</div>
          </div>
          <div class="form-group">
            <label>Brand</label>
            <input type="text" .value=${this._product.brand || ''}
              @input=${(e) => this._updateField('brand', e.target.value)}
              placeholder="Brand name" />
          </div>
          <div class="form-group">
            <label>Manufacturer</label>
            <input type="text" .value=${this._product.manufacturer || ''}
              @input=${(e) => this._updateField('manufacturer', e.target.value)}
              placeholder="Manufacturer name" />
          </div>
          <div class="form-group">
            <label>Barcode / GTIN</label>
            <input type="text" .value=${this._product.barcode || ''}
              @input=${(e) => this._updateField('barcode', e.target.value)}
              placeholder="UPC, EAN, ISBN, etc." />
          </div>
          <div class="form-group full-width">
            <label>Short Description</label>
            <input type="text" .value=${this._product.shortDescription || ''}
              @input=${(e) => this._updateField('shortDescription', e.target.value)}
              placeholder="Brief description for listings" />
          </div>
          <div class="form-group full-width">
            <label>Full Description</label>
            <textarea .value=${this._product.description || ''}
              @input=${(e) => this._updateField('description', e.target.value)}
              placeholder="Detailed product description" rows="6"></textarea>
          </div>
        </div>
      </div>

      <div class="form-section">
        <h3>Status</h3>
        <div class="form-grid">
          <div class="form-group">
            <label class="checkbox-group">
              <input type="checkbox" .checked=${this._product.isVisible !== false}
                @change=${(e) => this._updateField('isVisible', e.target.checked)} />
              Visible on storefront
            </label>
          </div>
          <div class="form-group">
            <label class="checkbox-group">
              <input type="checkbox" .checked=${this._product.isFeatured === true}
                @change=${(e) => this._updateField('isFeatured', e.target.checked)} />
              Featured product
            </label>
          </div>
          <div class="form-group">
            <label class="checkbox-group">
              <input type="checkbox" .checked=${this._product.isDigital === true}
                @change=${(e) => this._updateField('isDigital', e.target.checked)} />
              Digital product (no shipping)
            </label>
          </div>
        </div>
      </div>
    `;
  }

  _renderPricingTab() {
    return html`
      <div class="form-section">
        <h3>Pricing</h3>
        <div class="form-grid-3">
          <div class="form-group">
            <label>Base Price *</label>
            <input type="number" step="0.01" min="0" .value=${this._product.basePrice || 0}
              @input=${(e) => this._updateField('basePrice', parseFloat(e.target.value) || 0)}
              placeholder="0.00" />
          </div>
          <div class="form-group">
            <label>Sale Price</label>
            <input type="number" step="0.01" min="0" .value=${this._product.salePrice || ''}
              @input=${(e) => this._updateField('salePrice', e.target.value ? parseFloat(e.target.value) : null)}
              placeholder="Leave empty if not on sale" />
          </div>
          <div class="form-group">
            <label>Cost Price</label>
            <input type="number" step="0.01" min="0" .value=${this._product.costPrice || ''}
              @input=${(e) => this._updateField('costPrice', e.target.value ? parseFloat(e.target.value) : null)}
              placeholder="For profit calculation" />
          </div>
        </div>
      </div>

      <div class="form-section">
        <h3>Tax Settings</h3>
        <div class="form-grid">
          <div class="form-group">
            <label>Tax Class</label>
            <select .value=${this._product.taxClass || 'standard'}
              @change=${(e) => this._updateField('taxClass', e.target.value)}>
              <option value="standard">Standard Rate</option>
              <option value="reduced">Reduced Rate</option>
              <option value="zero">Zero Rate</option>
              <option value="exempt">Tax Exempt</option>
            </select>
          </div>
        </div>
      </div>
    `;
  }

  _renderInventoryTab() {
    return html`
      <div class="form-section">
        <h3>Stock Management</h3>
        <div class="form-grid">
          <div class="form-group">
            <label>Stock Quantity</label>
            <input type="number" min="0" .value=${this._product.stockQuantity || 0}
              @input=${(e) => this._updateField('stockQuantity', parseInt(e.target.value) || 0)} />
          </div>
          <div class="form-group">
            <label>Low Stock Threshold</label>
            <input type="number" min="0" .value=${this._product.lowStockThreshold || 5}
              @input=${(e) => this._updateField('lowStockThreshold', parseInt(e.target.value) || 0)} />
            <div class="hint">Alert when stock falls below this level</div>
          </div>
          <div class="form-group">
            <label class="checkbox-group">
              <input type="checkbox" .checked=${this._product.trackInventory !== false}
                @change=${(e) => this._updateField('trackInventory', e.target.checked)} />
              Track inventory for this product
            </label>
          </div>
          <div class="form-group">
            <label class="checkbox-group">
              <input type="checkbox" .checked=${this._product.allowBackorder === true}
                @change=${(e) => this._updateField('allowBackorder', e.target.checked)} />
              Allow backorders
            </label>
          </div>
        </div>
      </div>
    `;
  }

  _renderImagesTab() {
    const images = this._product.images || [];
    return html`
      <div class="form-section">
        <h3>Product Images</h3>
        <div class="image-grid">
          ${images.map((img, index) => html`
            <div class="image-item">
              <img src="${img.url}" alt="${img.alt || ''}" />
              <button class="remove-btn" @click=${() => this._removeImage(index)}>&times;</button>
            </div>
          `)}
          <div class="add-image-btn" @click=${this._addImage}>
            <uui-icon name="icon-add" style="font-size: 24px;"></uui-icon>
            <span>Add Image</span>
          </div>
        </div>
        <div class="hint" style="margin-top: 16px;">
          Drag images to reorder. First image will be the main product image.
        </div>
      </div>

      <div class="form-section">
        <h3>Image URL</h3>
        <div class="form-group">
          <label>Main Image URL</label>
          <input type="text" .value=${this._product.imageUrl || ''}
            @input=${(e) => this._updateField('imageUrl', e.target.value)}
            placeholder="https://example.com/image.jpg" />
        </div>
      </div>
    `;
  }

  _addImage() {
    const url = prompt('Enter image URL:');
    if (url) {
      const images = this._product.images || [];
      this._product = {
        ...this._product,
        images: [...images, { url, alt: '' }]
      };
    }
  }

  _removeImage(index) {
    const images = [...(this._product.images || [])];
    images.splice(index, 1);
    this._product = { ...this._product, images };
  }

  _renderCategoriesTab() {
    return html`
      <div class="form-section">
        <h3>Product Categories</h3>
        <p style="color: #666; margin-bottom: 16px;">Select the categories this product belongs to.</p>
        <div class="category-list">
          ${this._categories.length === 0
            ? html`<p style="color: #666; padding: 20px; text-align: center;">No categories found. Create categories first.</p>`
            : this._categories.map(cat => html`
              <label class="category-item">
                <input type="checkbox"
                  .checked=${this._selectedCategories.includes(cat.id)}
                  @change=${() => this._toggleCategory(cat.id)} />
                ${cat.name}
              </label>
            `)
          }
        </div>
      </div>
    `;
  }

  _renderVariantsTab() {
    const variants = this._product.variants || [];
    return html`
      <div class="form-section">
        <h3>Product Variants</h3>
        <p style="color: #666; margin-bottom: 16px;">
          Add variants for different sizes, colors, or other options.
        </p>

        ${variants.length > 0 ? html`
          <table class="variant-table">
            <thead>
              <tr>
                <th>Variant SKU</th>
                <th>Name</th>
                <th>Price</th>
                <th>Stock</th>
                <th style="width: 60px;"></th>
              </tr>
            </thead>
            <tbody>
              ${variants.map((variant, index) => html`
                <tr>
                  <td>
                    <input type="text" .value=${variant.sku || ''}
                      @input=${(e) => this._updateVariant(index, 'sku', e.target.value)}
                      placeholder="SKU" />
                  </td>
                  <td>
                    <input type="text" .value=${variant.name || ''}
                      @input=${(e) => this._updateVariant(index, 'name', e.target.value)}
                      placeholder="e.g., Red - Large" />
                  </td>
                  <td>
                    <input type="number" step="0.01" .value=${variant.price || ''}
                      @input=${(e) => this._updateVariant(index, 'price', parseFloat(e.target.value) || 0)} />
                  </td>
                  <td>
                    <input type="number" .value=${variant.stock || 0}
                      @input=${(e) => this._updateVariant(index, 'stock', parseInt(e.target.value) || 0)} />
                  </td>
                  <td>
                    <button class="btn btn-danger" style="padding: 6px 10px;"
                      @click=${() => this._removeVariant(index)}>&times;</button>
                  </td>
                </tr>
              `)}
            </tbody>
          </table>
        ` : html`
          <p style="color: #666; padding: 20px; text-align: center; background: #f9f9f9; border-radius: 8px;">
            No variants added yet.
          </p>
        `}

        <button class="btn btn-secondary add-variant-btn" @click=${this._addVariant}>
          + Add Variant
        </button>
      </div>
    `;
  }

  _renderAttributesTab() {
    const attributes = this._product.attributes || [];
    return html`
      <div class="form-section">
        <h3>Product Attributes</h3>
        <p style="color: #666; margin-bottom: 16px;">
          Add custom attributes like Material, Color, Size, etc.
        </p>

        ${attributes.map((attr, index) => html`
          <div class="attribute-row">
            <input type="text" .value=${attr.name || ''}
              @input=${(e) => this._updateAttribute(index, 'name', e.target.value)}
              placeholder="Attribute name (e.g., Material)" />
            <input type="text" .value=${attr.value || ''}
              @input=${(e) => this._updateAttribute(index, 'value', e.target.value)}
              placeholder="Value (e.g., Cotton)" />
            <button class="remove-attr-btn" @click=${() => this._removeAttribute(index)}>&times;</button>
          </div>
        `)}

        <button class="btn btn-secondary" @click=${this._addAttribute}>
          + Add Attribute
        </button>
      </div>
    `;
  }

  _renderShippingTab() {
    return html`
      <div class="form-section">
        <h3>Dimensions & Weight</h3>
        <div class="form-grid">
          <div class="form-group">
            <label>Weight (kg)</label>
            <input type="number" step="0.01" min="0" .value=${this._product.weight || ''}
              @input=${(e) => this._updateField('weight', e.target.value ? parseFloat(e.target.value) : null)}
              placeholder="0.00" />
          </div>
          <div class="form-group">
            <label>Length (cm)</label>
            <input type="number" step="0.01" min="0" .value=${this._product.length || ''}
              @input=${(e) => this._updateField('length', e.target.value ? parseFloat(e.target.value) : null)}
              placeholder="0.00" />
          </div>
          <div class="form-group">
            <label>Width (cm)</label>
            <input type="number" step="0.01" min="0" .value=${this._product.width || ''}
              @input=${(e) => this._updateField('width', e.target.value ? parseFloat(e.target.value) : null)}
              placeholder="0.00" />
          </div>
          <div class="form-group">
            <label>Height (cm)</label>
            <input type="number" step="0.01" min="0" .value=${this._product.height || ''}
              @input=${(e) => this._updateField('height', e.target.value ? parseFloat(e.target.value) : null)}
              placeholder="0.00" />
          </div>
        </div>
      </div>

      <div class="form-section">
        <h3>Shipping Options</h3>
        <div class="form-group">
          <label class="checkbox-group">
            <input type="checkbox" .checked=${this._product.requiresShipping !== false}
              @change=${(e) => this._updateField('requiresShipping', e.target.checked)} />
            This product requires shipping
          </label>
        </div>
        <div class="form-group">
          <label class="checkbox-group">
            <input type="checkbox" .checked=${this._product.freeShipping === true}
              @change=${(e) => this._updateField('freeShipping', e.target.checked)} />
            Free shipping
          </label>
        </div>
      </div>
    `;
  }

  _renderSeoTab() {
    return html`
      <div class="form-section">
        <h3>Search Engine Optimization</h3>
        <div class="form-group">
          <label>Meta Title</label>
          <input type="text" .value=${this._product.metaTitle || ''}
            @input=${(e) => this._updateField('metaTitle', e.target.value)}
            placeholder="SEO title (leave empty to use product name)" />
          <div class="hint">Recommended: 50-60 characters</div>
        </div>
        <div class="form-group">
          <label>Meta Description</label>
          <textarea .value=${this._product.metaDescription || ''}
            @input=${(e) => this._updateField('metaDescription', e.target.value)}
            placeholder="SEO description for search engines" rows="3"></textarea>
          <div class="hint">Recommended: 150-160 characters</div>
        </div>
        <div class="form-group">
          <label>Meta Keywords</label>
          <input type="text" .value=${this._product.metaKeywords || ''}
            @input=${(e) => this._updateField('metaKeywords', e.target.value)}
            placeholder="keyword1, keyword2, keyword3" />
          <div class="hint">Comma-separated keywords</div>
        </div>
      </div>

      <div class="form-section">
        <h3>URL Preview</h3>
        <div style="padding: 12px; background: #f5f5f5; border-radius: 4px; font-family: monospace;">
          /products/${this._product.slug || this._product.name?.toLowerCase().replace(/\s+/g, '-') || 'product-url'}
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-product-editor', ProductEditor);
export default ProductEditor;
