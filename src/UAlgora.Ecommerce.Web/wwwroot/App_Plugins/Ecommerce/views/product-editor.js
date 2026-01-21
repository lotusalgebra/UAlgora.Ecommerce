import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Product Editor View
 * Main form for editing product details.
 */
export class ProductEditor extends UmbElementMixin(LitElement) {
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

    .form-grid.full-width {
      grid-template-columns: 1fr;
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

    uui-input, uui-textarea {
      width: 100%;
    }

    .section-title {
      font-size: var(--uui-type-h4-size);
      font-weight: bold;
      margin: var(--uui-size-layout-2) 0 var(--uui-size-layout-1) 0;
      padding-bottom: var(--uui-size-space-2);
      border-bottom: 1px solid var(--uui-color-border);
    }

    .price-group {
      display: flex;
      gap: var(--uui-size-space-4);
      align-items: flex-end;
    }

    .price-group .form-group {
      flex: 1;
    }

    .toggle-group {
      display: flex;
      gap: var(--uui-size-layout-2);
      flex-wrap: wrap;
    }

    .toggle-item {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
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
    // Find the parent workspace element
    const workspace = this.closest('ecommerce-product-workspace');
    if (workspace) {
      this._product = workspace.getProduct();
    }
  }

  _handleInput(field, event) {
    const value = event.target.value;
    this._product = { ...this._product, [field]: value };
    this._updateWorkspace();
  }

  _handleNumberInput(field, event) {
    const value = event.target.value ? parseFloat(event.target.value) : null;
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

  render() {
    if (!this._product) {
      return html`<uui-loader></uui-loader>`;
    }

    return html`
      <uui-box>
        <div slot="headline">Basic Information</div>

        <div class="form-grid">
          <div class="form-group">
            <label for="name">Product Name *</label>
            <uui-input
              id="name"
              .value=${this._product.name || ''}
              @input=${(e) => this._handleInput('name', e)}
              placeholder="Enter product name"
              required
            ></uui-input>
          </div>

          <div class="form-group">
            <label for="sku">SKU *</label>
            <uui-input
              id="sku"
              .value=${this._product.sku || ''}
              @input=${(e) => this._handleInput('sku', e)}
              placeholder="Enter SKU"
              required
            ></uui-input>
          </div>
        </div>

        <div class="form-group">
          <label for="slug">URL Slug</label>
          <uui-input
            id="slug"
            .value=${this._product.slug || ''}
            @input=${(e) => this._handleInput('slug', e)}
            placeholder="product-url-slug"
          ></uui-input>
          <div class="hint">Leave empty to auto-generate from product name</div>
        </div>

        <div class="form-group">
          <label for="shortDescription">Short Description</label>
          <uui-textarea
            id="shortDescription"
            .value=${this._product.shortDescription || ''}
            @input=${(e) => this._handleInput('shortDescription', e)}
            placeholder="Brief product summary"
            rows="2"
          ></uui-textarea>
        </div>

        <div class="form-group">
          <label for="description">Full Description</label>
          <uui-textarea
            id="description"
            .value=${this._product.description || ''}
            @input=${(e) => this._handleInput('description', e)}
            placeholder="Detailed product description"
            rows="6"
          ></uui-textarea>
        </div>
      </uui-box>

      <h3 class="section-title">Pricing</h3>
      <uui-box>
        <div class="price-group">
          <div class="form-group">
            <label for="basePrice">Base Price *</label>
            <uui-input
              id="basePrice"
              type="number"
              step="0.01"
              min="0"
              .value=${this._product.basePrice?.toString() || '0'}
              @input=${(e) => this._handleNumberInput('basePrice', e)}
            ></uui-input>
          </div>

          <div class="form-group">
            <label for="salePrice">Sale Price</label>
            <uui-input
              id="salePrice"
              type="number"
              step="0.01"
              min="0"
              .value=${this._product.salePrice?.toString() || ''}
              @input=${(e) => this._handleNumberInput('salePrice', e)}
              placeholder="Leave empty if not on sale"
            ></uui-input>
          </div>

          <div class="form-group">
            <label for="costPrice">Cost Price</label>
            <uui-input
              id="costPrice"
              type="number"
              step="0.01"
              min="0"
              .value=${this._product.costPrice?.toString() || ''}
              @input=${(e) => this._handleNumberInput('costPrice', e)}
              placeholder="For profit calculation"
            ></uui-input>
          </div>
        </div>
      </uui-box>

      <h3 class="section-title">Status & Visibility</h3>
      <uui-box>
        <div class="toggle-group">
          <div class="toggle-item">
            <uui-toggle
              id="isActive"
              .checked=${this._product.isActive ?? true}
              @change=${(e) => this._handleToggle('isActive', e)}
            ></uui-toggle>
            <label for="isActive">Active</label>
          </div>

          <div class="toggle-item">
            <uui-toggle
              id="isFeatured"
              .checked=${this._product.isFeatured ?? false}
              @change=${(e) => this._handleToggle('isFeatured', e)}
            ></uui-toggle>
            <label for="isFeatured">Featured Product</label>
          </div>
        </div>
      </uui-box>

      <h3 class="section-title">Dimensions & Weight</h3>
      <uui-box>
        <div class="form-grid">
          <div class="form-group">
            <label for="weight">Weight (kg)</label>
            <uui-input
              id="weight"
              type="number"
              step="0.01"
              min="0"
              .value=${this._product.weight?.toString() || ''}
              @input=${(e) => this._handleNumberInput('weight', e)}
            ></uui-input>
          </div>

          <div class="form-group">
            <label for="length">Length (cm)</label>
            <uui-input
              id="length"
              type="number"
              step="0.01"
              min="0"
              .value=${this._product.length?.toString() || ''}
              @input=${(e) => this._handleNumberInput('length', e)}
            ></uui-input>
          </div>

          <div class="form-group">
            <label for="width">Width (cm)</label>
            <uui-input
              id="width"
              type="number"
              step="0.01"
              min="0"
              .value=${this._product.width?.toString() || ''}
              @input=${(e) => this._handleNumberInput('width', e)}
            ></uui-input>
          </div>

          <div class="form-group">
            <label for="height">Height (cm)</label>
            <uui-input
              id="height"
              type="number"
              step="0.01"
              min="0"
              .value=${this._product.height?.toString() || ''}
              @input=${(e) => this._handleNumberInput('height', e)}
            ></uui-input>
          </div>
        </div>
      </uui-box>
    `;
  }
}

customElements.define('ecommerce-product-editor', ProductEditor);

export default ProductEditor;
