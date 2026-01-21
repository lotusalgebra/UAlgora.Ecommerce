import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Supplier Products
 * View and manage products from this supplier.
 */
export class SupplierProducts extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .toolbar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--uui-size-layout-1);
    }

    .search-box {
      display: flex;
      gap: var(--uui-size-space-3);
    }

    uui-input {
      width: 300px;
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
      font-weight: 500;
    }

    .products-table tbody tr:hover {
      background: var(--uui-color-surface-emphasis);
    }

    .empty-state {
      text-align: center;
      padding: var(--uui-size-layout-4);
      color: var(--uui-color-text-alt);
    }

    .empty-state uui-icon {
      font-size: 48px;
      margin-bottom: var(--uui-size-space-4);
    }

    .price-cell {
      font-variant-numeric: tabular-nums;
    }

    .preferred-badge {
      display: inline-block;
      padding: var(--uui-size-space-1) var(--uui-size-space-2);
      background: var(--uui-color-warning-emphasis);
      color: var(--uui-color-warning);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
      font-weight: 500;
    }
  `;

  static properties = {
    _products: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _searchTerm: { type: String, state: true }
  };

  constructor() {
    super();
    this._products = [];
    this._loading = true;
    this._searchTerm = '';
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadProducts();
  }

  async _loadProducts() {
    const workspace = this.closest('ecommerce-supplier-workspace');
    if (!workspace) {
      setTimeout(() => this._loadProducts(), 100);
      return;
    }

    const supplier = workspace.getSupplier();
    if (!supplier.id) {
      this._loading = false;
      return;
    }

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/inventory/supplier/${supplier.id}/products`, {
        headers: { 'Accept': 'application/json' }
      });

      if (response.ok) {
        this._products = await response.json();
      }
    } catch (error) {
      console.error('Error loading supplier products:', error);
    } finally {
      this._loading = false;
    }
  }

  get _filteredProducts() {
    if (!this._searchTerm) return this._products;

    const term = this._searchTerm.toLowerCase();
    return this._products.filter(p =>
      p.productName?.toLowerCase().includes(term) ||
      p.sku?.toLowerCase().includes(term) ||
      p.supplierSku?.toLowerCase().includes(term)
    );
  }

  _formatCurrency(amount) {
    if (amount == null) return '-';
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  }

  render() {
    if (this._loading) {
      return html`<uui-loader></uui-loader>`;
    }

    return html`
      <uui-box>
        <div class="toolbar">
          <div class="search-box">
            <uui-input
              placeholder="Search products..."
              @input=${(e) => this._searchTerm = e.target.value}
            >
              <uui-icon name="icon-search" slot="prepend"></uui-icon>
            </uui-input>
          </div>
          <uui-button look="primary">
            <uui-icon name="icon-add"></uui-icon>
            Link Product
          </uui-button>
        </div>

        ${this._filteredProducts.length === 0 ? html`
          <div class="empty-state">
            <uui-icon name="icon-box"></uui-icon>
            <p>No products linked to this supplier</p>
          </div>
        ` : html`
          <table class="products-table">
            <thead>
              <tr>
                <th>Product</th>
                <th>SKU</th>
                <th>Supplier SKU</th>
                <th>Cost Price</th>
                <th>MOQ</th>
                <th>Lead Time</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              ${this._filteredProducts.map(product => html`
                <tr>
                  <td>
                    ${product.productName || 'Unknown Product'}
                    ${product.isPreferred ? html`
                      <span class="preferred-badge">Preferred</span>
                    ` : ''}
                  </td>
                  <td>${product.sku || '-'}</td>
                  <td>${product.supplierSku || '-'}</td>
                  <td class="price-cell">${this._formatCurrency(product.costPrice)}</td>
                  <td>${product.minimumOrderQuantity || '-'}</td>
                  <td>${product.leadTimeDays ? `${product.leadTimeDays} days` : '-'}</td>
                  <td>
                    <uui-button compact look="secondary">Edit</uui-button>
                  </td>
                </tr>
              `)}
            </tbody>
          </table>
        `}
      </uui-box>
    `;
  }
}

customElements.define('ecommerce-supplier-products', SupplierProducts);

export default SupplierProducts;
