import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Discount Products View
 * Manages product and category selection for discount targeting.
 */
export class DiscountProducts extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .section-title {
      font-size: var(--uui-type-h4-size);
      font-weight: bold;
      margin: var(--uui-size-layout-2) 0 var(--uui-size-layout-1) 0;
      padding-bottom: var(--uui-size-space-3);
      border-bottom: 1px solid var(--uui-color-border);
    }

    .section-title:first-child {
      margin-top: 0;
    }

    .help-text {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      margin-bottom: var(--uui-size-layout-1);
    }

    .scope-indicator {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-3);
      padding: var(--uui-size-layout-1);
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
      margin-bottom: var(--uui-size-layout-1);
    }

    .scope-indicator uui-icon {
      font-size: 24px;
      color: var(--uui-color-interactive);
    }

    .picker-container {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: var(--uui-size-layout-1);
    }

    .picker-section {
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-1);
    }

    .picker-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--uui-size-space-4);
    }

    .picker-title {
      font-weight: 500;
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
    }

    .picker-count {
      background: var(--uui-color-surface-alt);
      padding: var(--uui-size-space-1) var(--uui-size-space-3);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
    }

    .item-list {
      max-height: 300px;
      overflow-y: auto;
      margin-bottom: var(--uui-size-space-4);
    }

    .item-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: var(--uui-size-space-3);
      border-bottom: 1px solid var(--uui-color-border);
    }

    .item-row:last-child {
      border-bottom: none;
    }

    .item-row:hover {
      background: var(--uui-color-surface-emphasis);
    }

    .item-info {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-3);
    }

    .item-icon {
      width: 32px;
      height: 32px;
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .item-name {
      font-weight: 500;
    }

    .item-meta {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
    }

    .empty-state {
      text-align: center;
      padding: var(--uui-size-layout-2);
      color: var(--uui-color-text-alt);
    }

    .empty-state uui-icon {
      font-size: 32px;
      margin-bottom: var(--uui-size-space-2);
    }

    .search-input {
      margin-bottom: var(--uui-size-space-4);
    }

    .search-input uui-input {
      width: 100%;
    }

    .search-results {
      max-height: 200px;
      overflow-y: auto;
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      margin-bottom: var(--uui-size-space-4);
    }

    .search-result-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: var(--uui-size-space-3);
      cursor: pointer;
    }

    .search-result-item:hover {
      background: var(--uui-color-surface-emphasis);
    }

    .badge {
      display: inline-flex;
      align-items: center;
      gap: var(--uui-size-space-1);
      padding: var(--uui-size-space-1) var(--uui-size-space-2);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
    }

    .badge.include {
      background: var(--uui-color-positive-emphasis);
      color: var(--uui-color-positive);
    }

    .badge.exclude {
      background: var(--uui-color-danger-emphasis);
      color: var(--uui-color-danger);
    }
  `;

  static properties = {
    _discount: { type: Object, state: true },
    _products: { type: Array, state: true },
    _categories: { type: Array, state: true },
    _productSearch: { type: String, state: true },
    _categorySearch: { type: String, state: true },
    _searchResults: { type: Array, state: true },
    _loading: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._discount = null;
    this._products = [];
    this._categories = [];
    this._productSearch = '';
    this._categorySearch = '';
    this._searchResults = [];
    this._loading = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
    this._loadSelectedItems();
  }

  _loadFromWorkspace() {
    const workspace = this.closest('ecommerce-discount-workspace');
    if (workspace) {
      this._discount = workspace.getDiscount();
    }
  }

  async _loadSelectedItems() {
    if (!this._discount) return;

    // Load selected products
    const productIds = [
      ...(this._discount.applicableProductIds || []),
      ...(this._discount.excludedProductIds || [])
    ];

    if (productIds.length > 0) {
      try {
        const response = await fetch('/umbraco/management/api/v1/ecommerce/product', {
          headers: { 'Accept': 'application/json' }
        });
        if (response.ok) {
          const data = await response.json();
          this._products = data.items.filter(p => productIds.includes(p.id));
        }
      } catch (error) {
        console.error('Error loading products:', error);
      }
    }

    // Load selected categories
    const categoryIds = [
      ...(this._discount.applicableCategoryIds || []),
      ...(this._discount.excludedCategoryIds || [])
    ];

    if (categoryIds.length > 0) {
      try {
        const response = await fetch('/umbraco/management/api/v1/ecommerce/category', {
          headers: { 'Accept': 'application/json' }
        });
        if (response.ok) {
          const data = await response.json();
          this._categories = data.items.filter(c => categoryIds.includes(c.id));
        }
      } catch (error) {
        console.error('Error loading categories:', error);
      }
    }
  }

  async _searchProducts(query) {
    if (!query || query.length < 2) {
      this._searchResults = [];
      return;
    }

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/product?search=${encodeURIComponent(query)}&take=10`, {
        headers: { 'Accept': 'application/json' }
      });

      if (response.ok) {
        const data = await response.json();
        this._searchResults = data.items || [];
      }
    } catch (error) {
      console.error('Error searching products:', error);
    }
  }

  async _searchCategories(query) {
    if (!query || query.length < 2) {
      this._searchResults = [];
      return;
    }

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/category?search=${encodeURIComponent(query)}&take=10`, {
        headers: { 'Accept': 'application/json' }
      });

      if (response.ok) {
        const data = await response.json();
        this._searchResults = data.items || [];
      }
    } catch (error) {
      console.error('Error searching categories:', error);
    }
  }

  _addProduct(product, type) {
    const field = type === 'include' ? 'applicableProductIds' : 'excludedProductIds';
    const currentIds = this._discount[field] || [];

    if (!currentIds.includes(product.id)) {
      this._handleInputChange(field, [...currentIds, product.id]);
      this._products = [...this._products, product];
    }

    this._productSearch = '';
    this._searchResults = [];
  }

  _removeProduct(productId) {
    // Remove from both include and exclude lists
    const applicableIds = (this._discount.applicableProductIds || []).filter(id => id !== productId);
    const excludedIds = (this._discount.excludedProductIds || []).filter(id => id !== productId);

    this._handleInputChange('applicableProductIds', applicableIds);
    this._handleInputChange('excludedProductIds', excludedIds);
    this._products = this._products.filter(p => p.id !== productId);
  }

  _addCategory(category, type) {
    const field = type === 'include' ? 'applicableCategoryIds' : 'excludedCategoryIds';
    const currentIds = this._discount[field] || [];

    if (!currentIds.includes(category.id)) {
      this._handleInputChange(field, [...currentIds, category.id]);
      this._categories = [...this._categories, category];
    }

    this._categorySearch = '';
    this._searchResults = [];
  }

  _removeCategory(categoryId) {
    // Remove from both include and exclude lists
    const applicableIds = (this._discount.applicableCategoryIds || []).filter(id => id !== categoryId);
    const excludedIds = (this._discount.excludedCategoryIds || []).filter(id => id !== categoryId);

    this._handleInputChange('applicableCategoryIds', applicableIds);
    this._handleInputChange('excludedCategoryIds', excludedIds);
    this._categories = this._categories.filter(c => c.id !== categoryId);
  }

  _handleInputChange(field, value) {
    if (!this._discount) return;

    this._discount = {
      ...this._discount,
      [field]: value
    };

    const workspace = this.closest('ecommerce-discount-workspace');
    if (workspace) {
      workspace.setDiscount(this._discount);
    }
  }

  _isIncluded(id, type) {
    const field = type === 'product' ? 'applicableProductIds' : 'applicableCategoryIds';
    return (this._discount[field] || []).includes(id);
  }

  _isExcluded(id, type) {
    const field = type === 'product' ? 'excludedProductIds' : 'excludedCategoryIds';
    return (this._discount[field] || []).includes(id);
  }

  render() {
    if (!this._discount) {
      return html`<uui-loader></uui-loader>`;
    }

    const scope = this._discount.scope || 'Order';

    return html`
      <uui-box>
        <div class="scope-indicator">
          <uui-icon name="${scope === 'Order' ? 'icon-receipt-dollar' : scope === 'Product' ? 'icon-box' : scope === 'Category' ? 'icon-folders' : 'icon-truck'}"></uui-icon>
          <div>
            <strong>Discount Scope: ${scope}</strong>
            <div style="font-size: var(--uui-type-small-size); color: var(--uui-color-text-alt);">
              ${scope === 'Order' ? 'Applies to entire order. Use product/category targeting to limit which items qualify.' :
                scope === 'Product' ? 'Applies to specific products only.' :
                scope === 'Category' ? 'Applies to products in specific categories.' :
                'Applies to shipping costs only.'}
            </div>
          </div>
        </div>
      </uui-box>

      <h3 class="section-title">Products</h3>
      <uui-box>
        <p class="help-text">
          Add products to include in or exclude from this discount. Leave empty to apply to all products.
        </p>

        <div class="picker-container">
          <div class="picker-section">
            <div class="picker-header">
              <span class="picker-title">
                <uui-icon name="icon-check"></uui-icon>
                Included Products
              </span>
              <span class="picker-count">
                ${(this._discount.applicableProductIds || []).length}
              </span>
            </div>

            <div class="search-input">
              <uui-input
                placeholder="Search products to include..."
                .value=${this._productSearch}
                @input=${(e) => {
                  this._productSearch = e.target.value;
                  this._searchProducts(e.target.value);
                }}
              ></uui-input>
            </div>

            ${this._searchResults.length > 0 && this._productSearch ? html`
              <div class="search-results">
                ${this._searchResults.map(product => html`
                  <div class="search-result-item" @click=${() => this._addProduct(product, 'include')}>
                    <span>${product.name} <small style="color: var(--uui-color-text-alt)">(${product.sku})</small></span>
                    <uui-button compact look="primary">Add</uui-button>
                  </div>
                `)}
              </div>
            ` : ''}

            <div class="item-list">
              ${this._products.filter(p => this._isIncluded(p.id, 'product')).map(product => html`
                <div class="item-row">
                  <div class="item-info">
                    <div class="item-icon"><uui-icon name="icon-box"></uui-icon></div>
                    <div>
                      <div class="item-name">${product.name}</div>
                      <div class="item-meta">${product.sku}</div>
                    </div>
                  </div>
                  <uui-button compact look="secondary" color="danger" @click=${() => this._removeProduct(product.id)}>
                    <uui-icon name="icon-delete"></uui-icon>
                  </uui-button>
                </div>
              `)}
              ${(this._discount.applicableProductIds || []).length === 0 ? html`
                <div class="empty-state">
                  <uui-icon name="icon-box"></uui-icon>
                  <p>No products included</p>
                  <p style="font-size: var(--uui-type-small-size);">Discount applies to all products</p>
                </div>
              ` : ''}
            </div>
          </div>

          <div class="picker-section">
            <div class="picker-header">
              <span class="picker-title">
                <uui-icon name="icon-block"></uui-icon>
                Excluded Products
              </span>
              <span class="picker-count">
                ${(this._discount.excludedProductIds || []).length}
              </span>
            </div>

            <div class="search-input">
              <uui-input
                placeholder="Search products to exclude..."
                @input=${async (e) => {
                  await this._searchProducts(e.target.value);
                }}
              ></uui-input>
            </div>

            ${this._searchResults.length > 0 ? html`
              <div class="search-results">
                ${this._searchResults.map(product => html`
                  <div class="search-result-item" @click=${() => this._addProduct(product, 'exclude')}>
                    <span>${product.name}</span>
                    <uui-button compact look="secondary" color="danger">Exclude</uui-button>
                  </div>
                `)}
              </div>
            ` : ''}

            <div class="item-list">
              ${this._products.filter(p => this._isExcluded(p.id, 'product')).map(product => html`
                <div class="item-row">
                  <div class="item-info">
                    <div class="item-icon"><uui-icon name="icon-box"></uui-icon></div>
                    <div>
                      <div class="item-name">${product.name}</div>
                      <div class="item-meta">${product.sku}</div>
                    </div>
                  </div>
                  <uui-button compact look="secondary" @click=${() => this._removeProduct(product.id)}>
                    <uui-icon name="icon-delete"></uui-icon>
                  </uui-button>
                </div>
              `)}
              ${(this._discount.excludedProductIds || []).length === 0 ? html`
                <div class="empty-state">
                  <uui-icon name="icon-check"></uui-icon>
                  <p>No exclusions</p>
                </div>
              ` : ''}
            </div>
          </div>
        </div>
      </uui-box>

      <h3 class="section-title">Categories</h3>
      <uui-box>
        <p class="help-text">
          Add categories to include in or exclude from this discount. Products in included categories qualify for the discount.
        </p>

        <div class="picker-container">
          <div class="picker-section">
            <div class="picker-header">
              <span class="picker-title">
                <uui-icon name="icon-check"></uui-icon>
                Included Categories
              </span>
              <span class="picker-count">
                ${(this._discount.applicableCategoryIds || []).length}
              </span>
            </div>

            <div class="search-input">
              <uui-input
                placeholder="Search categories to include..."
                .value=${this._categorySearch}
                @input=${(e) => {
                  this._categorySearch = e.target.value;
                  this._searchCategories(e.target.value);
                }}
              ></uui-input>
            </div>

            <div class="item-list">
              ${this._categories.filter(c => this._isIncluded(c.id, 'category')).map(category => html`
                <div class="item-row">
                  <div class="item-info">
                    <div class="item-icon"><uui-icon name="icon-folder"></uui-icon></div>
                    <div>
                      <div class="item-name">${category.name}</div>
                    </div>
                  </div>
                  <uui-button compact look="secondary" color="danger" @click=${() => this._removeCategory(category.id)}>
                    <uui-icon name="icon-delete"></uui-icon>
                  </uui-button>
                </div>
              `)}
              ${(this._discount.applicableCategoryIds || []).length === 0 ? html`
                <div class="empty-state">
                  <uui-icon name="icon-folders"></uui-icon>
                  <p>No categories included</p>
                  <p style="font-size: var(--uui-type-small-size);">Discount applies to all categories</p>
                </div>
              ` : ''}
            </div>
          </div>

          <div class="picker-section">
            <div class="picker-header">
              <span class="picker-title">
                <uui-icon name="icon-block"></uui-icon>
                Excluded Categories
              </span>
              <span class="picker-count">
                ${(this._discount.excludedCategoryIds || []).length}
              </span>
            </div>

            <div class="search-input">
              <uui-input
                placeholder="Search categories to exclude..."
                @input=${async (e) => {
                  await this._searchCategories(e.target.value);
                }}
              ></uui-input>
            </div>

            <div class="item-list">
              ${this._categories.filter(c => this._isExcluded(c.id, 'category')).map(category => html`
                <div class="item-row">
                  <div class="item-info">
                    <div class="item-icon"><uui-icon name="icon-folder"></uui-icon></div>
                    <div>
                      <div class="item-name">${category.name}</div>
                    </div>
                  </div>
                  <uui-button compact look="secondary" @click=${() => this._removeCategory(category.id)}>
                    <uui-icon name="icon-delete"></uui-icon>
                  </uui-button>
                </div>
              `)}
              ${(this._discount.excludedCategoryIds || []).length === 0 ? html`
                <div class="empty-state">
                  <uui-icon name="icon-check"></uui-icon>
                  <p>No exclusions</p>
                </div>
              ` : ''}
            </div>
          </div>
        </div>
      </uui-box>
    `;
  }
}

customElements.define('ecommerce-discount-products', DiscountProducts);

export default DiscountProducts;
