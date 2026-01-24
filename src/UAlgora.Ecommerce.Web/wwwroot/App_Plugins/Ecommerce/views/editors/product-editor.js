import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Simple Product Editor Component
 */
export class ProductEditor extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      height: 100%;
      background: #f5f5f5;
    }

    .editor-container {
      max-width: 900px;
      margin: 0 auto;
      padding: 20px;
    }

    .editor-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px 20px;
      background: linear-gradient(135deg, #ecfdf5 0%, #d1fae5 100%);
      border-bottom: 3px solid #10b981;
      border-radius: 8px 8px 0 0;
      margin-bottom: 0;
    }

    .header-title {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .header-title h1 {
      margin: 0;
      font-size: 20px;
      color: #065f46;
    }

    .algora-badge {
      background: #10b981;
      color: white;
      padding: 4px 10px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: 600;
    }

    .header-actions {
      display: flex;
      gap: 10px;
    }

    .editor-body {
      background: white;
      border: 1px solid #e0e0e0;
      border-top: none;
      border-radius: 0 0 8px 8px;
      padding: 24px;
    }

    .form-section {
      margin-bottom: 24px;
    }

    .form-section h3 {
      margin: 0 0 16px 0;
      padding-bottom: 8px;
      border-bottom: 1px solid #e0e0e0;
      color: #333;
      font-size: 16px;
    }

    .form-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
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

    .form-row {
      display: flex;
      gap: 16px;
    }

    .form-row .form-group {
      flex: 1;
    }

    .checkbox-group {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .checkbox-group input {
      width: auto;
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
  `;

  static properties = {
    productId: { type: String },
    _product: { state: true },
    _loading: { state: true },
    _saving: { state: true },
    _toast: { state: true }
  };

  constructor() {
    super();
    this.productId = null;
    this._product = this._createEmptyProduct();
    this._loading = false;
    this._saving = false;
    this._toast = null;
  }

  connectedCallback() {
    super.connectedCallback();
    console.log('ProductEditor connected, productId:', this.productId);
    if (this.productId) {
      this._loadProduct();
    } else {
      this._product = this._createEmptyProduct();
    }
  }

  updated(changedProperties) {
    if (changedProperties.has('productId') && this.productId) {
      console.log('ProductEditor productId changed:', this.productId);
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
      isVisible: true,
      isFeatured: false,
      metaTitle: '',
      metaDescription: ''
    };
  }

  async _loadProduct() {
    if (!this.productId) return;

    console.log('Loading product:', this.productId);
    this._loading = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/product/${this.productId}`, {
        credentials: 'include',
        headers: { 'Accept': 'application/json' }
      });

      if (response.ok) {
        const data = await response.json();
        this._product = { ...this._createEmptyProduct(), ...data };
        console.log('Product loaded:', this._product);
      } else {
        console.error('Failed to load product:', response.status);
        this._showToast('Failed to load product', 'error');
      }
    } catch (error) {
      console.error('Error loading product:', error);
      this._showToast('Error loading product', 'error');
    } finally {
      this._loading = false;
    }
  }

  _updateField(field, value) {
    this._product = { ...this._product, [field]: value };
  }

  async _save() {
    console.log('Saving product:', this._product);
    this._saving = true;

    try {
      const isNew = !this._product.id;
      const url = isNew
        ? '/umbraco/management/api/v1/ecommerce/product'
        : `/umbraco/management/api/v1/ecommerce/product/${this._product.id}`;

      const response = await fetch(url, {
        method: isNew ? 'POST' : 'PUT',
        credentials: 'include',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(this._product)
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
        const error = await response.text();
        console.error('Save failed:', error);
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
    this.dispatchEvent(new CustomEvent('editor-cancel', {
      bubbles: true,
      composed: true
    }));
  }

  async _delete() {
    if (!this._product.id) return;

    if (!confirm(`Are you sure you want to delete "${this._product.name}"? This action cannot be undone.`)) {
      return;
    }

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/product/${this._product.id}`, {
        method: 'DELETE',
        credentials: 'include'
      });

      if (response.ok) {
        this._showToast('Product deleted successfully', 'success');
        this.dispatchEvent(new CustomEvent('editor-cancel', {
          bubbles: true,
          composed: true
        }));
      } else {
        this._showToast('Failed to delete product', 'error');
      }
    } catch (error) {
      console.error('Error deleting product:', error);
      this._showToast('Error deleting product', 'error');
    }
  }

  _showToast(message, type) {
    this._toast = { message, type };
    setTimeout(() => { this._toast = null; }, 3000);
  }

  render() {
    console.log('ProductEditor render, loading:', this._loading, 'product:', this._product?.name);

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

    return html`
      <div class="editor-container">
        <div class="editor-header">
          <div class="header-title">
            <span class="algora-badge">ALGORA</span>
            <h1>${isNew ? 'New Product' : this._product.name || 'Edit Product'}</h1>
          </div>
          <div class="header-actions">
            ${!isNew ? html`
              <button class="btn btn-danger" @click=${this._delete}>Delete</button>
            ` : ''}
            <button class="btn btn-secondary" @click=${this._cancel}>Cancel</button>
            <button class="btn btn-primary" @click=${this._save} ?disabled=${this._saving}>
              ${this._saving ? 'Saving...' : 'Save Product'}
            </button>
          </div>
        </div>

        <div class="editor-body">
          <!-- Basic Info -->
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
              </div>
              <div class="form-group">
                <label>Brand</label>
                <input type="text" .value=${this._product.brand || ''}
                  @input=${(e) => this._updateField('brand', e.target.value)}
                  placeholder="Brand name" />
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
                  placeholder="Detailed product description"></textarea>
              </div>
            </div>
          </div>

          <!-- Pricing -->
          <div class="form-section">
            <h3>Pricing</h3>
            <div class="form-grid">
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
                  placeholder="Leave empty if no sale" />
              </div>
              <div class="form-group">
                <label>Cost Price</label>
                <input type="number" step="0.01" min="0" .value=${this._product.costPrice || ''}
                  @input=${(e) => this._updateField('costPrice', e.target.value ? parseFloat(e.target.value) : null)}
                  placeholder="Your cost" />
              </div>
            </div>
          </div>

          <!-- Inventory -->
          <div class="form-section">
            <h3>Inventory</h3>
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
              </div>
              <div class="form-group">
                <label>Weight (kg)</label>
                <input type="number" step="0.01" min="0" .value=${this._product.weight || ''}
                  @input=${(e) => this._updateField('weight', e.target.value ? parseFloat(e.target.value) : null)}
                  placeholder="For shipping calculation" />
              </div>
            </div>
          </div>

          <!-- Status -->
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
            </div>
          </div>

          <!-- SEO -->
          <div class="form-section">
            <h3>SEO</h3>
            <div class="form-grid">
              <div class="form-group full-width">
                <label>Meta Title</label>
                <input type="text" .value=${this._product.metaTitle || ''}
                  @input=${(e) => this._updateField('metaTitle', e.target.value)}
                  placeholder="SEO title (leave empty to use product name)" />
              </div>
              <div class="form-group full-width">
                <label>Meta Description</label>
                <textarea .value=${this._product.metaDescription || ''}
                  @input=${(e) => this._updateField('metaDescription', e.target.value)}
                  placeholder="SEO description for search engines"></textarea>
              </div>
            </div>
          </div>
        </div>
      </div>

      ${this._toast ? html`<div class="toast ${this._toast.type}">${this._toast.message}</div>` : ''}
    `;
  }
}

customElements.define('ecommerce-product-editor', ProductEditor);
export default ProductEditor;
