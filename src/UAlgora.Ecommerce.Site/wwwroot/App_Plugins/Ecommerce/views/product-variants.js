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
      gap: var(--uui-size-space-2);
    }

    .variants-header-actions {
      display: flex;
      gap: var(--uui-size-space-2);
    }

    .variants-table {
      width: 100%;
      border-collapse: collapse;
      font-size: 13px;
    }

    .variants-table th,
    .variants-table td {
      padding: var(--uui-size-space-3);
      text-align: left;
      border-bottom: 1px solid var(--uui-color-border);
    }

    .variants-table th {
      background: var(--uui-color-surface-alt);
      font-weight: bold;
      white-space: nowrap;
    }

    .variants-table tr:hover {
      background: var(--uui-color-surface-emphasis);
    }

    .variants-table input {
      width: 100%;
      padding: 6px 8px;
      border: 1px solid var(--uui-color-border);
      border-radius: 4px;
      font-size: 13px;
    }

    .variants-table input:focus {
      outline: none;
      border-color: var(--uui-color-interactive);
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
      cursor: pointer;
    }

    .badge-active {
      background: var(--uui-color-positive);
      color: white;
    }

    .badge-inactive {
      background: var(--uui-color-danger);
      color: white;
    }

    .section-title {
      font-size: var(--uui-type-h4-size);
      font-weight: bold;
      margin: var(--uui-size-layout-2) 0 var(--uui-size-layout-1) 0;
      padding-bottom: var(--uui-size-space-2);
      border-bottom: 1px solid var(--uui-color-border);
    }

    .attribute-generator {
      display: grid;
      gap: var(--uui-size-layout-1);
    }

    .attribute-row {
      display: flex;
      gap: var(--uui-size-space-3);
      align-items: center;
    }

    .attribute-row input {
      padding: 8px 12px;
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
    }

    .attribute-row input:first-child {
      width: 150px;
    }

    .attribute-row input:nth-child(2) {
      flex: 1;
    }

    .attribute-tags {
      display: flex;
      flex-wrap: wrap;
      gap: var(--uui-size-space-2);
      margin-top: var(--uui-size-space-2);
    }

    .attribute-tag {
      background: var(--uui-color-surface-alt);
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 12px;
      display: flex;
      align-items: center;
      gap: 4px;
    }

    .attribute-tag button {
      background: none;
      border: none;
      cursor: pointer;
      padding: 0;
      font-size: 14px;
      color: var(--uui-color-danger);
    }

    .bulk-upload-area {
      margin-top: var(--uui-size-layout-1);
    }

    .bulk-upload-area textarea {
      width: 100%;
      min-height: 100px;
      padding: var(--uui-size-space-3);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      font-family: monospace;
      font-size: 12px;
    }

    .variant-image-cell {
      width: 50px;
    }

    .variant-image {
      width: 40px;
      height: 40px;
      object-fit: cover;
      border-radius: 4px;
      cursor: pointer;
    }

    .variant-image-placeholder {
      width: 40px;
      height: 40px;
      background: var(--uui-color-surface-alt);
      border-radius: 4px;
      display: flex;
      align-items: center;
      justify-content: center;
      cursor: pointer;
      font-size: 16px;
      color: var(--uui-color-text-alt);
    }
  `;

  static properties = {
    _product: { type: Object, state: true },
    _variants: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _attributes: { type: Array, state: true },
    _newAttributeName: { type: String, state: true },
    _newAttributeValues: { type: String, state: true },
    _bulkImportData: { type: String, state: true },
    _showGenerator: { type: Boolean, state: true },
    _showBulkImport: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._product = null;
    this._variants = [];
    this._loading = false;
    this._attributes = [];
    this._newAttributeName = '';
    this._newAttributeValues = '';
    this._bulkImportData = '';
    this._showGenerator = false;
    this._showBulkImport = false;
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
      this._attributes = this._product?.variantAttributes || [];
    }
  }

  _handleAddVariant() {
    const baseSku = this._product?.sku || 'PROD';
    const newVariant = {
      id: crypto.randomUUID(),
      sku: `${baseSku}-V${this._variants.length + 1}`,
      name: '',
      options: {},
      priceAdjustment: 0,
      costPrice: null,
      stockQuantity: 0,
      weight: null,
      barcode: '',
      imageUrl: '',
      isActive: true
    };

    this._variants = [...this._variants, newVariant];
    this._updateWorkspace();
  }

  _handleDeleteVariant(variantId) {
    if (!confirm('Delete this variant?')) return;
    this._variants = this._variants.filter(v => v.id !== variantId);
    this._updateWorkspace();
  }

  _handleDeleteAllVariants() {
    if (!confirm('Delete all variants? This cannot be undone.')) return;
    this._variants = [];
    this._updateWorkspace();
  }

  _handleVariantInput(variantId, field, value) {
    this._variants = this._variants.map(v =>
      v.id === variantId ? { ...v, [field]: value } : v
    );
    this._updateWorkspace();
  }

  _handleAddAttribute() {
    if (!this._newAttributeName.trim()) return;
    const values = this._newAttributeValues.split(',').map(v => v.trim()).filter(v => v);
    if (values.length === 0) return;

    this._attributes = [...this._attributes, {
      name: this._newAttributeName.trim(),
      values: values
    }];
    this._newAttributeName = '';
    this._newAttributeValues = '';
    this._updateWorkspace();
  }

  _handleRemoveAttribute(index) {
    this._attributes = this._attributes.filter((_, i) => i !== index);
    this._updateWorkspace();
  }

  _handleRemoveAttributeValue(attrIndex, valueIndex) {
    this._attributes = this._attributes.map((attr, i) => {
      if (i !== attrIndex) return attr;
      return { ...attr, values: attr.values.filter((_, vi) => vi !== valueIndex) };
    }).filter(attr => attr.values.length > 0);
    this._updateWorkspace();
  }

  _generateVariantsFromAttributes() {
    if (this._attributes.length === 0) return;

    const combinations = this._getCombinations(this._attributes.map(a => a.values));
    const baseSku = this._product?.sku || 'PROD';
    const basePrice = this._product?.basePrice || 0;

    const newVariants = combinations.map((combo, index) => {
      const options = {};
      this._attributes.forEach((attr, i) => {
        options[attr.name] = combo[i];
      });
      const name = combo.join(' / ');
      const skuSuffix = combo.map(v => v.substring(0, 2).toUpperCase()).join('-');

      return {
        id: crypto.randomUUID(),
        sku: `${baseSku}-${skuSuffix}`,
        name: name,
        options: options,
        priceAdjustment: 0,
        costPrice: null,
        stockQuantity: 0,
        weight: null,
        barcode: '',
        imageUrl: '',
        isActive: true
      };
    });

    this._variants = [...this._variants, ...newVariants];
    this._showGenerator = false;
    this._updateWorkspace();
  }

  _getCombinations(arrays) {
    if (arrays.length === 0) return [[]];
    const [first, ...rest] = arrays;
    const restCombinations = this._getCombinations(rest);
    return first.flatMap(item => restCombinations.map(combo => [item, ...combo]));
  }

  _handleBulkImport() {
    if (!this._bulkImportData.trim()) return;

    const lines = this._bulkImportData.split('\n').filter(l => l.trim());
    const baseSku = this._product?.sku || 'PROD';
    const newVariants = [];

    for (const line of lines) {
      const parts = line.split(',').map(p => p.trim());
      if (parts.length >= 2) {
        newVariants.push({
          id: crypto.randomUUID(),
          sku: parts[0] || `${baseSku}-V${this._variants.length + newVariants.length + 1}`,
          name: parts[1] || '',
          options: {},
          priceAdjustment: parseFloat(parts[2]) || 0,
          stockQuantity: parseInt(parts[3]) || 0,
          costPrice: parseFloat(parts[4]) || null,
          weight: parseFloat(parts[5]) || null,
          barcode: parts[6] || '',
          imageUrl: '',
          isActive: true
        });
      }
    }

    if (newVariants.length > 0) {
      this._variants = [...this._variants, ...newVariants];
      this._bulkImportData = '';
      this._showBulkImport = false;
      this._updateWorkspace();
    }
  }

  _handleVariantImageClick(variantId) {
    const url = prompt('Enter image URL for this variant:');
    if (url) {
      this._handleVariantInput(variantId, 'imageUrl', url);
    }
  }

  _updateWorkspace() {
    const workspace = this.closest('ecommerce-product-workspace');
    if (workspace) {
      workspace.updateProduct({
        variants: this._variants,
        variantAttributes: this._attributes
      });
    }
  }

  render() {
    return html`
      <uui-box>
        <div class="variants-header">
          <div slot="headline">Product Variants (${this._variants.length})</div>
          <div class="variants-header-actions">
            <uui-button look="secondary" @click=${() => this._showGenerator = !this._showGenerator}>
              ${this._showGenerator ? 'Hide Generator' : 'Generate from Attributes'}
            </uui-button>
            <uui-button look="secondary" @click=${() => this._showBulkImport = !this._showBulkImport}>
              ${this._showBulkImport ? 'Hide Import' : 'Bulk Import'}
            </uui-button>
            <uui-button look="primary" @click=${this._handleAddVariant}>
              <uui-icon name="icon-add"></uui-icon> Add Variant
            </uui-button>
            ${this._variants.length > 0 ? html`
              <uui-button look="secondary" color="danger" @click=${this._handleDeleteAllVariants}>
                Delete All
              </uui-button>
            ` : ''}
          </div>
        </div>

        ${this._showGenerator ? this._renderAttributeGenerator() : ''}
        ${this._showBulkImport ? this._renderBulkImport() : ''}

        ${this._variants.length === 0
          ? this._renderEmptyState()
          : this._renderVariantsTable()
        }
      </uui-box>
    `;
  }

  _renderAttributeGenerator() {
    return html`
      <div style="margin-bottom: var(--uui-size-layout-1); padding: var(--uui-size-layout-1); background: var(--uui-color-surface-alt); border-radius: var(--uui-border-radius);">
        <h4 style="margin: 0 0 var(--uui-size-space-4) 0;">Generate Variants from Attributes</h4>
        <p style="color: var(--uui-color-text-alt); font-size: var(--uui-type-small-size); margin-bottom: var(--uui-size-space-4);">
          Define attributes (e.g., Size, Color) and their values to automatically generate all variant combinations.
        </p>

        ${this._attributes.length > 0 ? html`
          <div style="margin-bottom: var(--uui-size-layout-1);">
            ${this._attributes.map((attr, attrIndex) => html`
              <div style="margin-bottom: var(--uui-size-space-3);">
                <strong>${attr.name}:</strong>
                <div class="attribute-tags">
                  ${attr.values.map((val, valIndex) => html`
                    <span class="attribute-tag">
                      ${val}
                      <button @click=${() => this._handleRemoveAttributeValue(attrIndex, valIndex)}>&times;</button>
                    </span>
                  `)}
                  <button class="attribute-tag" style="cursor: pointer; background: var(--uui-color-danger); color: white;"
                    @click=${() => this._handleRemoveAttribute(attrIndex)}>
                    Remove Attribute
                  </button>
                </div>
              </div>
            `)}
          </div>
        ` : ''}

        <div class="attribute-row">
          <input
            type="text"
            placeholder="Attribute name (e.g., Size)"
            .value=${this._newAttributeName}
            @input=${(e) => this._newAttributeName = e.target.value}
          />
          <input
            type="text"
            placeholder="Values (comma-separated, e.g., S, M, L, XL)"
            .value=${this._newAttributeValues}
            @input=${(e) => this._newAttributeValues = e.target.value}
          />
          <uui-button look="primary" compact @click=${this._handleAddAttribute}>Add Attribute</uui-button>
        </div>

        ${this._attributes.length > 0 ? html`
          <div style="margin-top: var(--uui-size-layout-1);">
            <p style="color: var(--uui-color-text-alt); font-size: var(--uui-type-small-size);">
              This will generate ${this._attributes.reduce((acc, a) => acc * a.values.length, 1)} variants.
            </p>
            <uui-button look="primary" @click=${this._generateVariantsFromAttributes}>
              Generate ${this._attributes.reduce((acc, a) => acc * a.values.length, 1)} Variants
            </uui-button>
          </div>
        ` : ''}
      </div>
    `;
  }

  _renderBulkImport() {
    return html`
      <div class="bulk-upload-area" style="margin-bottom: var(--uui-size-layout-1); padding: var(--uui-size-layout-1); background: var(--uui-color-surface-alt); border-radius: var(--uui-border-radius);">
        <h4 style="margin: 0 0 var(--uui-size-space-4) 0;">Bulk Import Variants</h4>
        <p style="color: var(--uui-color-text-alt); font-size: var(--uui-type-small-size); margin-bottom: var(--uui-size-space-4);">
          Paste CSV data (one variant per line): SKU, Name, Price Adjustment, Stock, Cost Price, Weight, Barcode
        </p>
        <textarea
          placeholder="SKU-001, Red Small, 0, 100, 10.00, 0.5, 123456789&#10;SKU-002, Red Medium, 5, 50, 10.00, 0.6, 123456790"
          .value=${this._bulkImportData}
          @input=${(e) => this._bulkImportData = e.target.value}
        ></textarea>
        <div style="margin-top: var(--uui-size-space-3);">
          <uui-button look="primary" @click=${this._handleBulkImport}>Import Variants</uui-button>
        </div>
      </div>
    `;
  }

  _renderEmptyState() {
    return html`
      <div class="empty-state">
        <uui-icon name="icon-layers"></uui-icon>
        <p>No variants configured</p>
        <p>Add variants to offer different options like size, color, or material.</p>
        <div style="display: flex; gap: var(--uui-size-space-3); justify-content: center; margin-top: var(--uui-size-space-4);">
          <uui-button look="primary" @click=${this._handleAddVariant}>Add Single Variant</uui-button>
          <uui-button look="secondary" @click=${() => this._showGenerator = true}>Generate from Attributes</uui-button>
        </div>
      </div>
    `;
  }

  _renderVariantsTable() {
    return html`
      <div style="overflow-x: auto;">
        <table class="variants-table">
          <thead>
            <tr>
              <th class="variant-image-cell">Image</th>
              <th>SKU</th>
              <th>Name</th>
              <th style="width: 90px;">Price +/-</th>
              <th style="width: 70px;">Stock</th>
              <th style="width: 90px;">Cost</th>
              <th style="width: 70px;">Weight</th>
              <th style="width: 100px;">Barcode</th>
              <th style="width: 60px;">Status</th>
              <th style="width: 80px;">Actions</th>
            </tr>
          </thead>
          <tbody>
            ${this._variants.map(variant => html`
              <tr>
                <td class="variant-image-cell">
                  ${variant.imageUrl ? html`
                    <img class="variant-image" src="${variant.imageUrl}" alt="${variant.name}"
                      @click=${() => this._handleVariantImageClick(variant.id)} />
                  ` : html`
                    <div class="variant-image-placeholder" @click=${() => this._handleVariantImageClick(variant.id)}>+</div>
                  `}
                </td>
                <td>
                  <input type="text" .value=${variant.sku || ''} placeholder="SKU"
                    @input=${(e) => this._handleVariantInput(variant.id, 'sku', e.target.value)} />
                </td>
                <td>
                  <input type="text" .value=${variant.name || ''} placeholder="Name"
                    @input=${(e) => this._handleVariantInput(variant.id, 'name', e.target.value)} />
                </td>
                <td>
                  <input type="number" step="0.01" .value=${variant.priceAdjustment?.toString() || '0'}
                    @input=${(e) => this._handleVariantInput(variant.id, 'priceAdjustment', parseFloat(e.target.value) || 0)} />
                </td>
                <td>
                  <input type="number" min="0" .value=${variant.stockQuantity?.toString() || '0'}
                    @input=${(e) => this._handleVariantInput(variant.id, 'stockQuantity', parseInt(e.target.value) || 0)} />
                </td>
                <td>
                  <input type="number" step="0.01" .value=${variant.costPrice?.toString() || ''} placeholder="Cost"
                    @input=${(e) => this._handleVariantInput(variant.id, 'costPrice', e.target.value ? parseFloat(e.target.value) : null)} />
                </td>
                <td>
                  <input type="number" step="0.01" .value=${variant.weight?.toString() || ''} placeholder="kg"
                    @input=${(e) => this._handleVariantInput(variant.id, 'weight', e.target.value ? parseFloat(e.target.value) : null)} />
                </td>
                <td>
                  <input type="text" .value=${variant.barcode || ''} placeholder="Barcode"
                    @input=${(e) => this._handleVariantInput(variant.id, 'barcode', e.target.value)} />
                </td>
                <td>
                  <span class="badge ${variant.isActive ? 'badge-active' : 'badge-inactive'}"
                    @click=${() => this._handleVariantInput(variant.id, 'isActive', !variant.isActive)}>
                    ${variant.isActive ? 'On' : 'Off'}
                  </span>
                </td>
                <td>
                  <uui-button look="secondary" color="danger" compact
                    @click=${() => this._handleDeleteVariant(variant.id)}>
                    <uui-icon name="icon-delete"></uui-icon>
                  </uui-button>
                </td>
              </tr>
            `)}
          </tbody>
        </table>
      </div>
    `;
  }
}

customElements.define('ecommerce-product-variants', ProductVariants);

export default ProductVariants;
