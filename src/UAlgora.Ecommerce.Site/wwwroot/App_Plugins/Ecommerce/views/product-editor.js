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

    .form-grid.three-col {
      grid-template-columns: 1fr 1fr 1fr;
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

    select {
      width: 100%;
      padding: 10px;
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-default-size);
      background: var(--uui-color-surface);
    }

    select:focus {
      outline: none;
      border-color: var(--uui-color-interactive);
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

    .rich-editor-toolbar {
      display: flex;
      gap: 4px;
      padding: 8px;
      background: var(--uui-color-surface-alt);
      border: 1px solid var(--uui-color-border);
      border-bottom: none;
      border-radius: var(--uui-border-radius) var(--uui-border-radius) 0 0;
    }

    .rich-editor-toolbar button {
      padding: 6px 10px;
      border: 1px solid var(--uui-color-border);
      background: var(--uui-color-surface);
      border-radius: 4px;
      cursor: pointer;
      font-size: 14px;
    }

    .rich-editor-toolbar button:hover {
      background: var(--uui-color-surface-emphasis);
    }

    .rich-editor-toolbar button.active {
      background: var(--uui-color-interactive);
      color: white;
    }

    .rich-editor-content {
      min-height: 200px;
      padding: 12px;
      border: 1px solid var(--uui-color-border);
      border-radius: 0 0 var(--uui-border-radius) var(--uui-border-radius);
      background: var(--uui-color-surface);
      outline: none;
    }

    .rich-editor-content:focus {
      border-color: var(--uui-color-interactive);
    }

    .add-new-inline {
      display: flex;
      gap: 8px;
      margin-top: 8px;
    }

    .add-new-inline input {
      flex: 1;
      padding: 8px;
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
    }
  `;

  static properties = {
    _product: { type: Object, state: true },
    _manufacturers: { type: Array, state: true },
    _brands: { type: Array, state: true },
    _newManufacturer: { type: String, state: true },
    _newBrand: { type: String, state: true }
  };

  constructor() {
    super();
    this._product = null;
    this._manufacturers = [];
    this._brands = [];
    this._newManufacturer = '';
    this._newBrand = '';
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
    this._loadManufacturers();
    this._loadBrands();
  }

  _loadFromWorkspace() {
    // Find the parent workspace element
    const workspace = this.closest('ecommerce-product-workspace');
    if (workspace) {
      this._product = workspace.getProduct();
    }
  }

  async _loadManufacturers() {
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/manufacturer', {
        headers: { 'Accept': 'application/json' }
      });
      if (response.ok) {
        const data = await response.json();
        this._manufacturers = data.items || [];
      }
    } catch (error) {
      console.error('Error loading manufacturers:', error);
      this._manufacturers = [];
    }
  }

  async _loadBrands() {
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/brand', {
        headers: { 'Accept': 'application/json' }
      });
      if (response.ok) {
        const data = await response.json();
        this._brands = data.items || [];
      }
    } catch (error) {
      console.error('Error loading brands:', error);
      this._brands = [];
    }
  }

  async _addManufacturer() {
    if (!this._newManufacturer.trim()) return;
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/manufacturer', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
        body: JSON.stringify({ name: this._newManufacturer.trim() })
      });
      if (response.ok) {
        const newItem = await response.json();
        this._manufacturers = [...this._manufacturers, newItem];
        this._product = { ...this._product, manufacturerId: newItem.id };
        this._updateWorkspace();
        this._newManufacturer = '';
      }
    } catch (error) {
      console.error('Error adding manufacturer:', error);
    }
  }

  async _addBrand() {
    if (!this._newBrand.trim()) return;
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/brand', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
        body: JSON.stringify({ name: this._newBrand.trim() })
      });
      if (response.ok) {
        const newItem = await response.json();
        this._brands = [...this._brands, newItem];
        this._product = { ...this._product, brandId: newItem.id };
        this._updateWorkspace();
        this._newBrand = '';
      }
    } catch (error) {
      console.error('Error adding brand:', error);
    }
  }

  _handleSelectInput(field, event) {
    const value = event.target.value;
    this._product = { ...this._product, [field]: value };
    this._updateWorkspace();
  }

  _execCommand(command, value = null) {
    document.execCommand(command, false, value);
    this._updateDescriptionFromEditor();
  }

  _updateDescriptionFromEditor() {
    const editor = this.shadowRoot.querySelector('.rich-editor-content');
    if (editor) {
      this._product = { ...this._product, description: editor.innerHTML };
      this._updateWorkspace();
    }
  }

  _insertLink() {
    const url = prompt('Enter URL:');
    if (url) {
      this._execCommand('createLink', url);
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

        <div class="form-grid">
          <div class="form-group">
            <label for="manufacturerId">Manufacturer</label>
            <select
              id="manufacturerId"
              .value=${this._product.manufacturerId || ''}
              @change=${(e) => this._handleSelectInput('manufacturerId', e)}
            >
              <option value="">-- Select Manufacturer --</option>
              ${this._manufacturers.map(m => html`
                <option value="${m.id}" ?selected=${this._product.manufacturerId === m.id}>${m.name}</option>
              `)}
            </select>
            <div class="add-new-inline">
              <input
                type="text"
                placeholder="Add new manufacturer..."
                .value=${this._newManufacturer}
                @input=${(e) => this._newManufacturer = e.target.value}
                @keypress=${(e) => e.key === 'Enter' && this._addManufacturer()}
              />
              <uui-button look="primary" compact @click=${this._addManufacturer}>Add</uui-button>
            </div>
          </div>

          <div class="form-group">
            <label for="brandId">Brand</label>
            <select
              id="brandId"
              .value=${this._product.brandId || ''}
              @change=${(e) => this._handleSelectInput('brandId', e)}
            >
              <option value="">-- Select Brand --</option>
              ${this._brands.map(b => html`
                <option value="${b.id}" ?selected=${this._product.brandId === b.id}>${b.name}</option>
              `)}
            </select>
            <div class="add-new-inline">
              <input
                type="text"
                placeholder="Add new brand..."
                .value=${this._newBrand}
                @input=${(e) => this._newBrand = e.target.value}
                @keypress=${(e) => e.key === 'Enter' && this._addBrand()}
              />
              <uui-button look="primary" compact @click=${this._addBrand}>Add</uui-button>
            </div>
          </div>
        </div>

        <div class="form-group">
          <label for="shortDescription">Short Description</label>
          <uui-textarea
            id="shortDescription"
            .value=${this._product.shortDescription || ''}
            @input=${(e) => this._handleInput('shortDescription', e)}
            placeholder="Brief product summary for listings and search results"
            rows="2"
          ></uui-textarea>
          <div class="hint">Used in product listings and search results (max 200 characters recommended)</div>
        </div>

        <div class="form-group">
          <label>Full Description</label>
          <div class="rich-editor-toolbar">
            <button type="button" @click=${() => this._execCommand('bold')} title="Bold"><b>B</b></button>
            <button type="button" @click=${() => this._execCommand('italic')} title="Italic"><i>I</i></button>
            <button type="button" @click=${() => this._execCommand('underline')} title="Underline"><u>U</u></button>
            <button type="button" @click=${() => this._execCommand('strikeThrough')} title="Strikethrough"><s>S</s></button>
            <span style="border-left: 1px solid var(--uui-color-border); margin: 0 4px;"></span>
            <button type="button" @click=${() => this._execCommand('insertUnorderedList')} title="Bullet List">• List</button>
            <button type="button" @click=${() => this._execCommand('insertOrderedList')} title="Numbered List">1. List</button>
            <span style="border-left: 1px solid var(--uui-color-border); margin: 0 4px;"></span>
            <button type="button" @click=${() => this._execCommand('formatBlock', 'h2')} title="Heading 2">H2</button>
            <button type="button" @click=${() => this._execCommand('formatBlock', 'h3')} title="Heading 3">H3</button>
            <button type="button" @click=${() => this._execCommand('formatBlock', 'p')} title="Paragraph">P</button>
            <span style="border-left: 1px solid var(--uui-color-border); margin: 0 4px;"></span>
            <button type="button" @click=${() => this._insertLink()} title="Insert Link">Link</button>
            <button type="button" @click=${() => this._execCommand('removeFormat')} title="Clear Formatting">Clear</button>
          </div>
          <div
            class="rich-editor-content"
            contenteditable="true"
            @blur=${this._updateDescriptionFromEditor}
            .innerHTML=${this._product.description || ''}
          ></div>
          <div class="hint">Use the toolbar for formatting. Supports bold, italic, lists, headings, and links.</div>
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
