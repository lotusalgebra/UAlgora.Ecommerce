import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Category Editor View
 * Main form for editing category details.
 */
export class CategoryEditor extends UmbElementMixin(LitElement) {
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

    .parent-info {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
      padding: var(--uui-size-space-4);
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
      margin-bottom: var(--uui-size-layout-1);
    }

    .parent-info uui-icon {
      color: var(--uui-color-text-alt);
    }

    .image-preview {
      width: 150px;
      height: 150px;
      object-fit: cover;
      border-radius: var(--uui-border-radius);
      border: 1px solid var(--uui-color-border);
      background: var(--uui-color-surface-alt);
    }

    .image-placeholder {
      width: 150px;
      height: 150px;
      display: flex;
      align-items: center;
      justify-content: center;
      border: 2px dashed var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      color: var(--uui-color-text-alt);
    }
  `;

  static properties = {
    _category: { type: Object, state: true },
    _parentCategory: { type: Object, state: true },
    _allCategories: { type: Array, state: true }
  };

  constructor() {
    super();
    this._category = null;
    this._parentCategory = null;
    this._allCategories = [];
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
    this._loadCategories();
  }

  _loadFromWorkspace() {
    const workspace = this.closest('ecommerce-category-workspace');
    if (workspace) {
      this._category = workspace.getCategory();
      if (this._category?.parentId) {
        this._loadParentCategory(this._category.parentId);
      }
    }
  }

  async _loadCategories() {
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/category', {
        headers: { 'Accept': 'application/json' }
      });
      if (response.ok) {
        const data = await response.json();
        this._allCategories = data.items || [];
      }
    } catch (error) {
      console.error('Error loading categories:', error);
    }
  }

  async _loadParentCategory(parentId) {
    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/category/${parentId}`, {
        headers: { 'Accept': 'application/json' }
      });
      if (response.ok) {
        this._parentCategory = await response.json();
      }
    } catch (error) {
      console.error('Error loading parent category:', error);
    }
  }

  _handleInput(field, event) {
    const value = event.target.value;
    this._category = { ...this._category, [field]: value };
    this._updateWorkspace();
  }

  _handleNumberInput(field, event) {
    const value = event.target.value ? parseInt(event.target.value) : 0;
    this._category = { ...this._category, [field]: value };
    this._updateWorkspace();
  }

  _handleToggle(field, event) {
    const value = event.target.checked;
    this._category = { ...this._category, [field]: value };
    this._updateWorkspace();
  }

  _handleParentChange(event) {
    const value = event.target.value;
    this._category = { ...this._category, parentId: value || null };
    this._updateWorkspace();

    if (value) {
      this._loadParentCategory(value);
    } else {
      this._parentCategory = null;
    }
  }

  _updateWorkspace() {
    const workspace = this.closest('ecommerce-category-workspace');
    if (workspace) {
      workspace.setCategory(this._category);
    }
  }

  _getAvailableParents() {
    // Filter out current category and its children to prevent circular references
    if (!this._category?.id) {
      return this._allCategories;
    }
    return this._allCategories.filter(c => c.id !== this._category.id);
  }

  render() {
    if (!this._category) {
      return html`<uui-loader></uui-loader>`;
    }

    return html`
      ${this._parentCategory ? html`
        <div class="parent-info">
          <uui-icon name="icon-folder"></uui-icon>
          <span>Parent category: <strong>${this._parentCategory.name}</strong></span>
        </div>
      ` : ''}

      <uui-box>
        <div slot="headline">Basic Information</div>

        <div class="form-group">
          <label for="name">Category Name *</label>
          <uui-input
            id="name"
            .value=${this._category.name || ''}
            @input=${(e) => this._handleInput('name', e)}
            placeholder="Enter category name"
            required
          ></uui-input>
        </div>

        <div class="form-group">
          <label for="slug">URL Slug</label>
          <uui-input
            id="slug"
            .value=${this._category.slug || ''}
            @input=${(e) => this._handleInput('slug', e)}
            placeholder="category-url-slug"
          ></uui-input>
          <div class="hint">Leave empty to auto-generate from category name</div>
        </div>

        <div class="form-group">
          <label for="description">Description</label>
          <uui-textarea
            id="description"
            .value=${this._category.description || ''}
            @input=${(e) => this._handleInput('description', e)}
            placeholder="Category description"
            rows="4"
          ></uui-textarea>
        </div>

        <div class="form-group">
          <label for="parentId">Parent Category</label>
          <uui-select
            id="parentId"
            .value=${this._category.parentId || ''}
            @change=${this._handleParentChange}
          >
            <uui-select-option value="">None (Root Category)</uui-select-option>
            ${this._getAvailableParents().map(cat => html`
              <uui-select-option value=${cat.id} ?selected=${cat.id === this._category.parentId}>
                ${cat.name}
              </uui-select-option>
            `)}
          </uui-select>
          <div class="hint">Select a parent to create a subcategory</div>
        </div>
      </uui-box>

      <h3 class="section-title">Display Settings</h3>
      <uui-box>
        <div class="toggle-group">
          <div class="toggle-item">
            <uui-toggle
              id="isVisible"
              .checked=${this._category.isVisible ?? true}
              @change=${(e) => this._handleToggle('isVisible', e)}
            ></uui-toggle>
            <label for="isVisible">Visible in Navigation</label>
          </div>

          <div class="toggle-item">
            <uui-toggle
              id="isFeatured"
              .checked=${this._category.isFeatured ?? false}
              @change=${(e) => this._handleToggle('isFeatured', e)}
            ></uui-toggle>
            <label for="isFeatured">Featured Category</label>
          </div>
        </div>

        <div class="form-group" style="margin-top: var(--uui-size-layout-1);">
          <label for="sortOrder">Sort Order</label>
          <uui-input
            id="sortOrder"
            type="number"
            min="0"
            .value=${this._category.sortOrder?.toString() || '0'}
            @input=${(e) => this._handleNumberInput('sortOrder', e)}
          ></uui-input>
          <div class="hint">Lower numbers appear first</div>
        </div>
      </uui-box>

      <h3 class="section-title">Category Image</h3>
      <uui-box>
        <div class="form-group">
          <label for="imageUrl">Image URL</label>
          <uui-input
            id="imageUrl"
            .value=${this._category.imageUrl || ''}
            @input=${(e) => this._handleInput('imageUrl', e)}
            placeholder="https://example.com/image.jpg"
          ></uui-input>
        </div>

        <div style="margin-top: var(--uui-size-layout-1);">
          ${this._category.imageUrl ? html`
            <img class="image-preview" src="${this._category.imageUrl}" alt="Category image" />
          ` : html`
            <div class="image-placeholder">
              <uui-icon name="icon-picture"></uui-icon>
            </div>
          `}
        </div>
      </uui-box>
    `;
  }
}

customElements.define('ecommerce-category-editor', CategoryEditor);

export default CategoryEditor;
