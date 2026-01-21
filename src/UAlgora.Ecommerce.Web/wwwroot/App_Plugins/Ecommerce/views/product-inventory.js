import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Product Inventory View
 * Manages stock and inventory settings.
 */
export class ProductInventory extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .form-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: var(--uui-size-layout-1);
    }

    .form-group {
      margin-bottom: var(--uui-size-layout-1);
    }

    .form-group label {
      display: block;
      margin-bottom: var(--uui-size-space-2);
      font-weight: bold;
      color: var(--uui-color-text);
    }

    .form-group .hint {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      margin-top: var(--uui-size-space-1);
    }

    uui-input {
      width: 100%;
    }

    .toggle-group {
      display: flex;
      gap: var(--uui-size-layout-2);
      flex-wrap: wrap;
      margin-bottom: var(--uui-size-layout-1);
    }

    .toggle-item {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
    }

    .stock-status {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-4);
      padding: var(--uui-size-layout-1);
      border-radius: var(--uui-border-radius);
      margin-bottom: var(--uui-size-layout-1);
    }

    .stock-status.in-stock {
      background: var(--uui-color-positive-standalone);
      color: white;
    }

    .stock-status.low-stock {
      background: var(--uui-color-warning-standalone);
      color: white;
    }

    .stock-status.out-of-stock {
      background: var(--uui-color-danger-standalone);
      color: white;
    }

    .stock-status uui-icon {
      font-size: 24px;
    }

    .stock-status-text h4 {
      margin: 0;
      font-size: var(--uui-type-default-size);
    }

    .stock-status-text p {
      margin: 0;
      font-size: var(--uui-type-small-size);
      opacity: 0.9;
    }

    .section-title {
      font-size: var(--uui-type-h4-size);
      font-weight: bold;
      margin: var(--uui-size-layout-2) 0 var(--uui-size-layout-1) 0;
      padding-bottom: var(--uui-size-space-2);
      border-bottom: 1px solid var(--uui-color-border);
    }
  `;

  static properties = {
    _product: { type: Object, state: true }
  };

  constructor() {
    super();
    this._product = null;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = this.closest('ecommerce-product-workspace');
    if (workspace) {
      this._product = workspace.getProduct();
    }
  }

  _handleNumberInput(field, event) {
    const value = event.target.value ? parseInt(event.target.value) : 0;
    this._product = { ...this._product, [field]: value };
    this._updateWorkspace();
  }

  _handleToggle(field, event) {
    const value = event.target.checked;
    this._product = { ...this._product, [field]: value };
    this._updateWorkspace();
  }

  _updateWorkspace() {
    const workspace = this.closest('ecommerce-product-workspace');
    if (workspace) {
      workspace.setProduct(this._product);
    }
  }

  _getStockStatus() {
    if (!this._product?.trackInventory) {
      return { class: 'in-stock', icon: 'icon-check', title: 'Not Tracking', description: 'Inventory tracking disabled' };
    }

    const stock = this._product.stockQuantity || 0;
    const threshold = this._product.lowStockThreshold || 10;

    if (stock <= 0) {
      return { class: 'out-of-stock', icon: 'icon-alert', title: 'Out of Stock', description: 'No units available' };
    }

    if (stock <= threshold) {
      return { class: 'low-stock', icon: 'icon-alert', title: 'Low Stock', description: `Only ${stock} units remaining` };
    }

    return { class: 'in-stock', icon: 'icon-check', title: 'In Stock', description: `${stock} units available` };
  }

  render() {
    if (!this._product) {
      return html`<uui-loader></uui-loader>`;
    }

    const stockStatus = this._getStockStatus();

    return html`
      <div class="stock-status ${stockStatus.class}">
        <uui-icon name="${stockStatus.icon}"></uui-icon>
        <div class="stock-status-text">
          <h4>${stockStatus.title}</h4>
          <p>${stockStatus.description}</p>
        </div>
      </div>

      <uui-box>
        <div slot="headline">Inventory Settings</div>

        <div class="toggle-group">
          <div class="toggle-item">
            <uui-toggle
              id="trackInventory"
              .checked=${this._product.trackInventory ?? true}
              @change=${(e) => this._handleToggle('trackInventory', e)}
            ></uui-toggle>
            <label for="trackInventory">Track Inventory</label>
          </div>

          <div class="toggle-item">
            <uui-toggle
              id="allowBackorders"
              .checked=${this._product.allowBackorders ?? false}
              @change=${(e) => this._handleToggle('allowBackorders', e)}
              ?disabled=${!this._product.trackInventory}
            ></uui-toggle>
            <label for="allowBackorders">Allow Backorders</label>
          </div>
        </div>

        ${this._product.trackInventory ? html`
          <div class="form-grid">
            <div class="form-group">
              <label for="stockQuantity">Stock Quantity</label>
              <uui-input
                id="stockQuantity"
                type="number"
                min="0"
                .value=${this._product.stockQuantity?.toString() || '0'}
                @input=${(e) => this._handleNumberInput('stockQuantity', e)}
              ></uui-input>
              <div class="hint">Current available stock</div>
            </div>

            <div class="form-group">
              <label for="lowStockThreshold">Low Stock Threshold</label>
              <uui-input
                id="lowStockThreshold"
                type="number"
                min="0"
                .value=${this._product.lowStockThreshold?.toString() || '10'}
                @input=${(e) => this._handleNumberInput('lowStockThreshold', e)}
              ></uui-input>
              <div class="hint">Alert when stock falls below this level</div>
            </div>
          </div>
        ` : html`
          <p style="color: var(--uui-color-text-alt);">
            Inventory tracking is disabled. Enable it to manage stock levels.
          </p>
        `}
      </uui-box>

      <h3 class="section-title">Stock Adjustments</h3>
      <uui-box>
        <p style="color: var(--uui-color-text-alt);">
          Stock adjustments can be made through the main inventory view or via API integrations.
          Order processing will automatically adjust stock levels when tracking is enabled.
        </p>
      </uui-box>
    `;
  }
}

customElements.define('ecommerce-product-inventory', ProductInventory);

export default ProductInventory;
