import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Category Products View
 * Shows products assigned to this category.
 */
export class CategoryProducts extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .products-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--uui-size-layout-1);
    }

    .products-table {
      width: 100%;
      border-collapse: collapse;
    }

    .products-table th,
    .products-table td {
      padding: var(--uui-size-space-4);
      text-align: left;
      border-bottom: 1px solid var(--uui-color-border);
    }

    .products-table th {
      background: var(--uui-color-surface-alt);
      font-weight: bold;
    }

    .products-table tr:hover {
      background: var(--uui-color-surface-emphasis);
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

    .product-image {
      width: 40px;
      height: 40px;
      object-fit: cover;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-alt);
    }

    .badge {
      display: inline-block;
      padding: var(--uui-size-space-1) var(--uui-size-space-2);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
    }

    .badge-active {
      background: var(--uui-color-positive);
      color: white;
    }

    .badge-inactive {
      background: var(--uui-color-danger);
      color: white;
    }

    .stats {
      display: flex;
      gap: var(--uui-size-layout-2);
      margin-bottom: var(--uui-size-layout-1);
    }

    .stat-item {
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-space-4);
      text-align: center;
    }

    .stat-value {
      font-size: var(--uui-type-h3-size);
      font-weight: bold;
      color: var(--uui-color-text);
    }

    .stat-label {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
    }

    .loading {
      display: flex;
      justify-content: center;
      padding: var(--uui-size-layout-2);
    }
  `;

  static properties = {
    _category: { type: Object, state: true },
    _products: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _totalCount: { type: Number, state: true }
  };

  constructor() {
    super();
    this._category = null;
    this._products = [];
    this._loading = true;
    this._totalCount = 0;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = this.closest('ecommerce-category-workspace');
    if (workspace) {
      this._category = workspace.getCategory();
      if (this._category?.id) {
        this._loadProducts();
      } else {
        this._loading = false;
      }
    }
  }

  async _loadProducts() {
    if (!this._category?.id) {
      this._loading = false;
      return;
    }

    try {
      this._loading = true;
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/product?categoryId=${this._category.id}&take=50`, {
        headers: { 'Accept': 'application/json' }
      });

      if (response.ok) {
        const data = await response.json();
        this._products = data.items || [];
        this._totalCount = data.total || 0;
      }
    } catch (error) {
      console.error('Error loading products:', error);
    } finally {
      this._loading = false;
    }
  }

  _formatPrice(price, currency = 'USD') {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency
    }).format(price || 0);
  }

  _handleProductClick(product) {
    window.history.pushState(null, '', `/umbraco/section/ecommerce/workspace/product/edit/${product.id}`);
  }

  render() {
    const isNew = !this._category?.id;

    if (isNew) {
      return html`
        <uui-box>
          <div class="empty-state">
            <uui-icon name="icon-box"></uui-icon>
            <h3>Save Category First</h3>
            <p>Save this category to start adding products.</p>
          </div>
        </uui-box>
      `;
    }

    if (this._loading) {
      return html`<div class="loading"><uui-loader></uui-loader></div>`;
    }

    return html`
      <div class="stats">
        <div class="stat-item">
          <div class="stat-value">${this._totalCount}</div>
          <div class="stat-label">Products in Category</div>
        </div>
        <div class="stat-item">
          <div class="stat-value">${this._products.filter(p => p.isVisible).length}</div>
          <div class="stat-label">Active Products</div>
        </div>
      </div>

      <uui-box>
        <div class="products-header">
          <div slot="headline">Products (${this._totalCount})</div>
        </div>

        ${this._products.length === 0
          ? this._renderEmptyState()
          : this._renderProductsTable()
        }
      </uui-box>
    `;
  }

  _renderEmptyState() {
    return html`
      <div class="empty-state">
        <uui-icon name="icon-box"></uui-icon>
        <p>No products in this category</p>
        <p>Add products to this category from the Products section.</p>
      </div>
    `;
  }

  _renderProductsTable() {
    return html`
      <table class="products-table">
        <thead>
          <tr>
            <th style="width: 50px;"></th>
            <th>Name</th>
            <th>SKU</th>
            <th>Price</th>
            <th>Stock</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          ${this._products.map(product => html`
            <tr @click=${() => this._handleProductClick(product)} style="cursor: pointer;">
              <td>
                ${product.imageUrl
                  ? html`<img class="product-image" src="${product.imageUrl}" alt="${product.name}" />`
                  : html`<div class="product-image"><uui-icon name="icon-box"></uui-icon></div>`
                }
              </td>
              <td>${product.name}</td>
              <td>${product.sku}</td>
              <td>${this._formatPrice(product.price)}</td>
              <td>${product.stockQuantity ?? '-'}</td>
              <td>
                <span class="badge ${product.isVisible ? 'badge-active' : 'badge-inactive'}">
                  ${product.isVisible ? 'Active' : 'Hidden'}
                </span>
              </td>
            </tr>
          `)}
        </tbody>
      </table>
    `;
  }
}

customElements.define('ecommerce-category-products', CategoryProducts);

export default CategoryProducts;
