import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Category SEO View
 * Manages SEO meta information for categories.
 */
export class CategorySeo extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
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

    .form-group .char-count {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      margin-top: var(--uui-size-space-1);
      text-align: right;
    }

    .form-group .char-count.warning {
      color: var(--uui-color-warning);
    }

    .form-group .char-count.error {
      color: var(--uui-color-danger);
    }

    uui-input, uui-textarea {
      width: 100%;
    }

    .seo-preview {
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-1);
      margin-top: var(--uui-size-layout-1);
    }

    .seo-preview-title {
      color: #1a0dab;
      font-size: 18px;
      margin-bottom: var(--uui-size-space-2);
      text-decoration: none;
      cursor: pointer;
    }

    .seo-preview-title:hover {
      text-decoration: underline;
    }

    .seo-preview-url {
      color: #006621;
      font-size: 14px;
      margin-bottom: var(--uui-size-space-2);
    }

    .seo-preview-description {
      color: #545454;
      font-size: 14px;
      line-height: 1.4;
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
    const workspace = this.closest('ecommerce-category-workspace');
    if (workspace) {
      this._category = workspace.getCategory();
    }
  }

  _handleInput(field, event) {
    const value = event.target.value;
    this._category = { ...this._category, [field]: value };
    this._updateWorkspace();
  }

  _updateWorkspace() {
    const workspace = this.closest('ecommerce-category-workspace');
    if (workspace) {
      workspace.setCategory(this._category);
    }
  }

  _getCharCountClass(length, max) {
    if (length > max) return 'error';
    if (length > max * 0.9) return 'warning';
    return '';
  }

  render() {
    if (!this._category) {
      return html`<uui-loader></uui-loader>`;
    }

    const titleLength = (this._category.metaTitle || this._category.name || '').length;
    const descLength = (this._category.metaDescription || this._category.description || '').length;

    return html`
      <uui-box>
        <div slot="headline">Search Engine Optimization</div>

        <div class="form-group">
          <label for="metaTitle">Meta Title</label>
          <uui-input
            id="metaTitle"
            .value=${this._category.metaTitle || ''}
            @input=${(e) => this._handleInput('metaTitle', e)}
            placeholder=${this._category.name || 'Category title'}
          ></uui-input>
          <div class="hint">Recommended: 50-60 characters. Leave empty to use category name.</div>
          <div class="char-count ${this._getCharCountClass(titleLength, 60)}">
            ${titleLength} / 60 characters
          </div>
        </div>

        <div class="form-group">
          <label for="metaDescription">Meta Description</label>
          <uui-textarea
            id="metaDescription"
            .value=${this._category.metaDescription || ''}
            @input=${(e) => this._handleInput('metaDescription', e)}
            placeholder=${this._category.description || 'Category description for search results'}
            rows="3"
          ></uui-textarea>
          <div class="hint">Recommended: 150-160 characters. Leave empty to use category description.</div>
          <div class="char-count ${this._getCharCountClass(descLength, 160)}">
            ${descLength} / 160 characters
          </div>
        </div>
      </uui-box>

      <h3 class="section-title">Search Preview</h3>
      <uui-box>
        <p style="color: var(--uui-color-text-alt); margin-bottom: var(--uui-size-layout-1);">
          This is an approximation of how your category may appear in Google search results.
        </p>

        <div class="seo-preview">
          <div class="seo-preview-title">
            ${this._category.metaTitle || this._category.name || 'Category Title'}
          </div>
          <div class="seo-preview-url">
            https://yoursite.com/categories/${this._category.slug || 'category-slug'}
          </div>
          <div class="seo-preview-description">
            ${this._category.metaDescription || this._category.description || 'Category description will appear here. Add a meta description to control what appears in search results.'}
          </div>
        </div>
      </uui-box>

      <h3 class="section-title">URL Settings</h3>
      <uui-box>
        <div class="form-group">
          <label for="slug">URL Slug</label>
          <uui-input
            id="slug"
            .value=${this._category.slug || ''}
            @input=${(e) => this._handleInput('slug', e)}
            placeholder="category-url-slug"
          ></uui-input>
          <div class="hint">The URL-friendly version of the category name. Leave empty to auto-generate.</div>
        </div>
      </uui-box>
    `;
  }
}

customElements.define('ecommerce-category-seo', CategorySeo);

export default CategorySeo;
