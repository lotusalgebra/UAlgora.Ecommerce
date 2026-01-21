import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Product Variants View
 * Manages product variants (size, color, etc.)
 */
export class ProductVariants extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .variants-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--uui-size-layout-1);
    }

    .variants-table {
      width: 100%;
      border-collapse: collapse;
    }

    .variants-table th,
    .variants-table td {
      padding: var(--uui-size-space-4);
      text-align: left;
      border-bottom: 1px solid var(--uui-color-border);
    }

    .variants-table th {
      background: var(--uui-color-surface-alt);
      font-weight: bold;
    }

    .variants-table tr:hover {
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

    .variant-actions {
      display: flex;
      gap: var(--uui-size-space-2);
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
  `;

  static properties = {
    _product: { type: Object, state: true },
    _variants: { type: Array, state: true },
    _loading: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._product = null;
    this._variants = [];
    this._loading = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = this.closest('ecommerce-product-workspace');
    if (workspace) {
      this._product = workspace.getProduct();
      this._variants = this._product?.variants || [];
    }
  }

  _handleAddVariant() {
    const newVariant = {
      id: crypto.randomUUID(),
      sku: '',
      name: '',
      options: {},
      priceAdjustment: 0,
      stockQuantity: 0,
      isActive: true
    };

    this._variants = [...this._variants, newVariant];
    this._updateWorkspace();
  }

  _handleDeleteVariant(variantId) {
    this._variants = this._variants.filter(v => v.id !== variantId);
    this._updateWorkspace();
  }

  _handleVariantInput(variantId, field, value) {
    this._variants = this._variants.map(v =>
      v.id === variantId ? { ...v, [field]: value } : v
    );
    this._updateWorkspace();
  }

  _updateWorkspace() {
    const workspace = this.closest('ecommerce-product-workspace');
    if (workspace) {
      workspace.updateProduct({ variants: this._variants });
    }
  }

  render() {
    return html`
      <uui-box>
        <div class="variants-header">
          <div slot="headline">Product Variants</div>
          <uui-button
            look="primary"
            @click=${this._handleAddVariant}
          >
            <uui-icon name="icon-add"></uui-icon>
            Add Variant
          </uui-button>
        </div>

        ${this._variants.length === 0
          ? this._renderEmptyState()
          : this._renderVariantsTable()
        }
      </uui-box>
    `;
  }

  _renderEmptyState() {
    return html`
      <div class="empty-state">
        <uui-icon name="icon-layers"></uui-icon>
        <p>No variants configured</p>
        <p>Add variants to offer different options like size, color, or material.</p>
        <uui-button look="primary" @click=${this._handleAddVariant}>
          Add Your First Variant
        </uui-button>
      </div>
    `;
  }

  _renderVariantsTable() {
    return html`
      <table class="variants-table">
        <thead>
          <tr>
            <th>SKU</th>
            <th>Name</th>
            <th>Price Adjustment</th>
            <th>Stock</th>
            <th>Status</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          ${this._variants.map(variant => html`
            <tr>
              <td>
                <uui-input
                  .value=${variant.sku || ''}
                  @input=${(e) => this._handleVariantInput(variant.id, 'sku', e.target.value)}
                  placeholder="Variant SKU"
                ></uui-input>
              </td>
              <td>
                <uui-input
                  .value=${variant.name || ''}
                  @input=${(e) => this._handleVariantInput(variant.id, 'name', e.target.value)}
                  placeholder="Variant name"
                ></uui-input>
              </td>
              <td>
                <uui-input
                  type="number"
                  step="0.01"
                  .value=${variant.priceAdjustment?.toString() || '0'}
                  @input=${(e) => this._handleVariantInput(variant.id, 'priceAdjustment', parseFloat(e.target.value) || 0)}
                ></uui-input>
              </td>
              <td>
                <uui-input
                  type="number"
                  min="0"
                  .value=${variant.stockQuantity?.toString() || '0'}
                  @input=${(e) => this._handleVariantInput(variant.id, 'stockQuantity', parseInt(e.target.value) || 0)}
                ></uui-input>
              </td>
              <td>
                <span class="badge ${variant.isActive ? 'badge-active' : 'badge-inactive'}">
                  ${variant.isActive ? 'Active' : 'Inactive'}
                </span>
              </td>
              <td>
                <div class="variant-actions">
                  <uui-button
                    look="secondary"
                    compact
                    @click=${() => this._handleVariantInput(variant.id, 'isActive', !variant.isActive)}
                  >
                    ${variant.isActive ? 'Disable' : 'Enable'}
                  </uui-button>
                  <uui-button
                    look="secondary"
                    color="danger"
                    compact
                    @click=${() => this._handleDeleteVariant(variant.id)}
                  >
                    <uui-icon name="icon-delete"></uui-icon>
                  </uui-button>
                </div>
              </td>
            </tr>
          `)}
        </tbody>
      </table>
    `;
  }
}

customElements.define('ecommerce-product-variants', ProductVariants);

export default ProductVariants;
