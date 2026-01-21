import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { UMB_WORKSPACE_CONTEXT } from "@umbraco-cms/backoffice/workspace";

/**
 * Category Workspace Element
 * Main workspace container for editing categories in the Umbraco backoffice.
 */
export class CategoryWorkspace extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      width: 100%;
      height: 100%;
    }
  `;

  static properties = {
    _category: { type: Object, state: true },
    _loading: { type: Boolean, state: true },
    _error: { type: String, state: true },
    _isNew: { type: Boolean, state: true },
    _parentId: { type: String, state: true }
  };

  #workspaceContext;

  constructor() {
    super();
    this._category = null;
    this._loading = true;
    this._error = null;
    this._isNew = false;
    this._parentId = null;
  }

  connectedCallback() {
    super.connectedCallback();

    this.consumeContext(UMB_WORKSPACE_CONTEXT, (context) => {
      this.#workspaceContext = context;
      this._loadCategory();
    });
  }

  async _loadCategory() {
    try {
      this._loading = true;

      // Get category ID from route/context
      const categoryId = this.#workspaceContext?.getUnique();

      // Check for parent ID in URL params for creating child categories
      const urlParams = new URLSearchParams(window.location.search);
      this._parentId = urlParams.get('parentId') || null;

      if (!categoryId || categoryId === 'create') {
        this._isNew = true;
        this._category = this._getEmptyCategory();
        this._loading = false;
        return;
      }

      const response = await fetch(`/umbraco/management/api/v1/ecommerce/category/${categoryId}`, {
        headers: {
          'Accept': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error('Failed to load category');
      }

      this._category = await response.json();
      this._error = null;
    } catch (error) {
      this._error = error.message;
      console.error('Error loading category:', error);
    } finally {
      this._loading = false;
    }
  }

  _getEmptyCategory() {
    return {
      id: null,
      name: '',
      slug: '',
      description: '',
      parentId: this._parentId,
      isVisible: true,
      isFeatured: false,
      sortOrder: 0,
      imageUrl: '',
      metaTitle: '',
      metaDescription: ''
    };
  }

  getCategory() {
    return this._category;
  }

  setCategory(category) {
    this._category = { ...category };
    this.requestUpdate();
  }

  updateCategory(updates) {
    this._category = { ...this._category, ...updates };
    this.requestUpdate();
  }

  isNew() {
    return this._isNew;
  }

  isLoading() {
    return this._loading;
  }

  getParentId() {
    return this._parentId;
  }

  render() {
    if (this._loading) {
      return html`
        <div style="display: flex; justify-content: center; padding: var(--uui-size-layout-2);">
          <uui-loader></uui-loader>
        </div>
      `;
    }

    if (this._error) {
      return html`
        <uui-box>
          <div slot="headline">Error</div>
          <p>${this._error}</p>
          <uui-button @click=${this._loadCategory}>Retry</uui-button>
        </uui-box>
      `;
    }

    return html`<slot></slot>`;
  }
}

customElements.define('ecommerce-category-workspace', CategoryWorkspace);

export default CategoryWorkspace;
