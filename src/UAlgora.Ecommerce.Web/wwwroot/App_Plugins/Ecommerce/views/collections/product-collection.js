import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Product Collection View
 * Displays a table/grid of products with search and filtering.
 */
export class ProductCollection extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .collection-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--uui-size-layout-1);
      gap: var(--uui-size-layout-1);
    }

    .search-box {
      flex: 1;
      max-width: 400px;
    }

    .search-box uui-input {
      width: 100%;
    }

    .collection-table {
      width: 100%;
      border-collapse: collapse;
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
    }

    .collection-table th,
    .collection-table td {
      padding: var(--uui-size-space-4);
      text-align: left;
      border-bottom: 1px solid var(--uui-color-border);
    }

    .collection-table th {
      background: var(--uui-color-surface-alt);
      font-weight: bold;
      color: var(--uui-color-text);
    }

    .collection-table tr:hover {
      background: var(--uui-color-surface-emphasis);
      cursor: pointer;
    }

    .collection-table tr:last-child td {
      border-bottom: none;
    }

    .product-image {
      width: 50px;
      height: 50px;
      object-fit: cover;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-alt);
    }

    .product-name {
      font-weight: 500;
    }

    .product-sku {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
    }

    .badge {
      display: inline-block;
      padding: var(--uui-size-space-1) var(--uui-size-space-2);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
      font-weight: 500;
    }

    .badge-active {
      background: var(--uui-color-positive-standalone);
      color: white;
    }

    .badge-inactive {
      background: var(--uui-color-danger-standalone);
      color: white;
    }

    .badge-featured {
      background: var(--uui-color-warning-standalone);
      color: white;
    }

    .pagination {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-top: var(--uui-size-layout-1);
      padding: var(--uui-size-space-4);
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
    }

    .pagination-info {
      color: var(--uui-color-text-alt);
    }

    .pagination-buttons {
      display: flex;
      gap: var(--uui-size-space-2);
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

    .loading {
      display: flex;
      justify-content: center;
      padding: var(--uui-size-layout-2);
    }
  `;

  static properties = {
    _products: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _searchTerm: { type: String, state: true },
    _page: { type: Number, state: true },
    _pageSize: { type: Number, state: true },
    _totalCount: { type: Number, state: true }
  };

  constructor() {
    super();
    this._products = [];
    this._loading = true;
    this._searchTerm = '';
    this._page = 1;
    this._pageSize = 25;
    this._totalCount = 0;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadProducts();
  }

  async _loadProducts() {
    try {
      this._loading = true;

      const skip = (this._page - 1) * this._pageSize;
      const params = new URLSearchParams({
        skip: skip.toString(),
        take: this._pageSize.toString()
      });

      if (this._searchTerm) {
        params.append('search', this._searchTerm);
      }

      const response = await fetch(`/umbraco/management/api/v1/ecommerce/product?${params}`, {
        headers: {
          'Accept': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error('Failed to load products');
      }

      const data = await response.json();
      this._products = data.items || [];
      this._totalCount = data.total || 0;
    } catch (error) {
      console.error('Error loading products:', error);
      this._products = [];
    } finally {
      this._loading = false;
    }
  }

  _handleSearch(event) {
    this._searchTerm = event.target.value;
    this._page = 1;

    // Debounce the search
    clearTimeout(this._searchTimeout);
    this._searchTimeout = setTimeout(() => {
      this._loadProducts();
    }, 300);
  }

  _handlePageChange(direction) {
    const totalPages = Math.ceil(this._totalCount / this._pageSize);

    if (direction === 'prev' && this._page > 1) {
      this._page--;
      this._loadProducts();
    } else if (direction === 'next' && this._page < totalPages) {
      this._page++;
      this._loadProducts();
    }
  }

  _handleRowClick(product) {
    // Navigate to product editor
    window.history.pushState(null, '', `/umbraco/section/ecommerce/workspace/product/edit/${product.id}`);

    const event = new CustomEvent('umb-navigate', {
      bubbles: true,
      composed: true,
      detail: {
        path: `/section/ecommerce/workspace/product/edit/${product.id}`
      }
    });
    this.dispatchEvent(event);
  }

  _handleCreateProduct() {
    window.history.pushState(null, '', '/umbraco/section/ecommerce/workspace/product/create');

    const event = new CustomEvent('umb-navigate', {
      bubbles: true,
      composed: true,
      detail: {
        path: '/section/ecommerce/workspace/product/create'
      }
    });
    this.dispatchEvent(event);
  }

  _formatPrice(price, currency = 'USD') {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency
    }).format(price || 0);
  }

  render() {
    return html`
      <div class="collection-header">
        <div class="search-box">
          <uui-input
            placeholder="Search products..."
            .value=${this._searchTerm}
            @input=${this._handleSearch}
          >
            <uui-icon name="icon-search" slot="prepend"></uui-icon>
          </uui-input>
        </div>
        <uui-button look="primary" @click=${this._handleCreateProduct}>
          <uui-icon name="icon-add"></uui-icon>
          Add Product
        </uui-button>
      </div>

      ${this._loading
        ? html`<div class="loading"><uui-loader></uui-loader></div>`
        : this._products.length === 0
          ? this._renderEmptyState()
          : this._renderProductsTable()
      }
    `;
  }

  _renderEmptyState() {
    return html`
      <div class="empty-state">
        <uui-icon name="icon-box"></uui-icon>
        <h3>No products found</h3>
        <p>${this._searchTerm ? 'Try adjusting your search criteria' : 'Get started by adding your first product'}</p>
        ${!this._searchTerm ? html`
          <uui-button look="primary" @click=${this._handleCreateProduct}>
            Add Your First Product
          </uui-button>
        ` : ''}
      </div>
    `;
  }

  _renderProductsTable() {
    const totalPages = Math.ceil(this._totalCount / this._pageSize);
    const startItem = ((this._page - 1) * this._pageSize) + 1;
    const endItem = Math.min(this._page * this._pageSize, this._totalCount);

    return html`
      <table class="collection-table">
        <thead>
          <tr>
            <th style="width: 60px;"></th>
            <th>Product</th>
            <th>SKU</th>
            <th>Price</th>
            <th>Stock</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          ${this._products.map(product => html`
            <tr @click=${() => this._handleRowClick(product)}>
              <td>
                ${product.imageUrl
                  ? html`<img class="product-image" src="${product.imageUrl}" alt="${product.name}" />`
                  : html`<div class="product-image"><uui-icon name="icon-box"></uui-icon></div>`
                }
              </td>
              <td>
                <div class="product-name">${product.name}</div>
                ${product.isFeatured ? html`<span class="badge badge-featured">Featured</span>` : ''}
              </td>
              <td>
                <span class="product-sku">${product.sku}</span>
              </td>
              <td>${this._formatPrice(product.price)}</td>
              <td>${product.stockQuantity ?? '-'}</td>
              <td>
                <span class="badge ${product.isActive ? 'badge-active' : 'badge-inactive'}">
                  ${product.isActive ? 'Active' : 'Inactive'}
                </span>
              </td>
            </tr>
          `)}
        </tbody>
      </table>

      <div class="pagination">
        <div class="pagination-info">
          Showing ${startItem}-${endItem} of ${this._totalCount} products
        </div>
        <div class="pagination-buttons">
          <uui-button
            look="secondary"
            compact
            ?disabled=${this._page === 1}
            @click=${() => this._handlePageChange('prev')}
          >
            Previous
          </uui-button>
          <uui-button
            look="secondary"
            compact
            ?disabled=${this._page >= totalPages}
            @click=${() => this._handlePageChange('next')}
          >
            Next
          </uui-button>
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-product-collection', ProductCollection);

export default ProductCollection;
