import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Tax Category Editor
 * Form for editing tax category properties.
 */
export class TaxCategoryEditor extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .editor-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: var(--uui-size-layout-1);
    }

    .full-width {
      grid-column: 1 / -1;
    }

    uui-box {
      margin-bottom: var(--uui-size-layout-1);
    }

    .form-group {
      margin-bottom: var(--uui-size-space-5);
    }

    .form-group:last-child {
      margin-bottom: 0;
    }

    .form-group label {
      display: block;
      margin-bottom: var(--uui-size-space-2);
      font-weight: bold;
    }

    .form-group .hint {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      margin-top: var(--uui-size-space-1);
    }

    uui-input, uui-textarea {
      width: 100%;
    }

    .toggle-group {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-3);
    }

    .toggle-description {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
    }
  `;

  static properties = {
    _category: { type: Object, state: true }
  };

  constructor() {
    super();
    this._category = null;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = this.closest('ecommerce-tax-category-workspace');
    if (workspace) {
      this._category = workspace.getCategory();
    }
  }

  _updateField(field, value) {
    this._category = { ...this._category, [field]: value };
    const workspace = this.closest('ecommerce-tax-category-workspace');
    if (workspace) {
      workspace._category = this._category;
    }
  }

  render() {
    if (!this._category) {
      return html`<uui-loader></uui-loader>`;
    }

    return html`
      <div class="editor-grid">
        <uui-box headline="Basic Information" class="full-width">
          <div class="form-group">
            <label>Category Name *</label>
            <uui-input
              .value=${this._category.name || ''}
              @input=${(e) => this._updateField('name', e.target.value)}
              placeholder="e.g., Standard Rate, Reduced Rate, Zero Rate"
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Category Code *</label>
            <uui-input
              .value=${this._category.code || ''}
              @input=${(e) => this._updateField('code', e.target.value)}
              placeholder="e.g., STANDARD, REDUCED, ZERO"
            ></uui-input>
            <div class="hint">Unique identifier used in code and API calls</div>
          </div>

          <div class="form-group">
            <label>Description</label>
            <uui-textarea
              .value=${this._category.description || ''}
              @input=${(e) => this._updateField('description', e.target.value)}
              placeholder="Describe what products belong to this category..."
              rows="3"
            ></uui-textarea>
          </div>
        </uui-box>

        <uui-box headline="Settings">
          <div class="form-group">
            <div class="toggle-group">
              <uui-toggle
                .checked=${this._category.isActive}
                @change=${(e) => this._updateField('isActive', e.target.checked)}
              ></uui-toggle>
              <div>
                <label>Active</label>
                <div class="toggle-description">Enable this tax category for use</div>
              </div>
            </div>
          </div>

          <div class="form-group">
            <div class="toggle-group">
              <uui-toggle
                .checked=${this._category.isDefault}
                @change=${(e) => this._updateField('isDefault', e.target.checked)}
              ></uui-toggle>
              <div>
                <label>Default Category</label>
                <div class="toggle-description">Use for products without a specific tax category</div>
              </div>
            </div>
          </div>

          <div class="form-group">
            <div class="toggle-group">
              <uui-toggle
                .checked=${this._category.isTaxExempt}
                @change=${(e) => this._updateField('isTaxExempt', e.target.checked)}
              ></uui-toggle>
              <div>
                <label>Tax Exempt</label>
                <div class="toggle-description">Products in this category are not taxed</div>
              </div>
            </div>
          </div>
        </uui-box>

        <uui-box headline="Integration">
          <div class="form-group">
            <label>External Tax Code</label>
            <uui-input
              .value=${this._category.externalTaxCode || ''}
              @input=${(e) => this._updateField('externalTaxCode', e.target.value)}
              placeholder="e.g., P0000000 (Avalara tax code)"
            ></uui-input>
            <div class="hint">Tax code used by external tax services like Avalara or TaxJar</div>
          </div>

          <div class="form-group">
            <label>Sort Order</label>
            <uui-input
              type="number"
              .value=${this._category.sortOrder?.toString() || '0'}
              @input=${(e) => this._updateField('sortOrder', parseInt(e.target.value) || 0)}
            ></uui-input>
            <div class="hint">Display order in lists (lower numbers appear first)</div>
          </div>
        </uui-box>
      </div>
    `;
  }
}

customElements.define('ecommerce-tax-category-editor', TaxCategoryEditor);

export default TaxCategoryEditor;
