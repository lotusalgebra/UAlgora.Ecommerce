import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import './editors/category-editor.js';

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

    .workspace-container {
      height: 100%;
    }
  `;

  static properties = {
    categoryId: { type: String },
    _mode: { type: String, state: true }
  };

  constructor() {
    super();
    this.categoryId = null;
    this._mode = 'create';
  }

  connectedCallback() {
    super.connectedCallback();
    this._parseRoute();

    // Listen for route changes
    this._routeHandler = () => this._parseRoute();
    window.addEventListener('popstate', this._routeHandler);
  }

  disconnectedCallback() {
    super.disconnectedCallback();
    if (this._routeHandler) {
      window.removeEventListener('popstate', this._routeHandler);
    }
  }

  _parseRoute() {
    const path = window.location.pathname;
    console.log('Category workspace: Parsing route:', path);

    // Match patterns like:
    // /section/ecommerce/view/workspace/category/edit/{id}
    // /section/ecommerce/view/workspace/category/create
    // /section/ecommerce/view/workspace/category/{id}
    // Also support legacy patterns without /view/
    const editMatch = path.match(/\/(?:view\/)?workspace\/category\/edit\/([a-f0-9-]+)/i);
    const directMatch = path.match(/\/(?:view\/)?workspace\/category\/([a-f0-9-]+)$/i);
    const createMatch = path.match(/\/(?:view\/)?workspace\/category\/?(?:create)?$/i);

    if (editMatch) {
      this.categoryId = editMatch[1];
      this._mode = 'edit';
      console.log('Category workspace: Edit mode, ID:', this.categoryId);
    } else if (directMatch && directMatch[1] !== 'create') {
      this.categoryId = directMatch[1];
      this._mode = 'edit';
      console.log('Category workspace: Direct edit mode, ID:', this.categoryId);
    } else if (createMatch) {
      this.categoryId = null;
      this._mode = 'create';
      console.log('Category workspace: Create mode');
    }

    // Force re-render
    this.requestUpdate();
  }

  _handleCategorySaved(event) {
    const category = event.detail.category;
    if (category && category.id && this._mode === 'create') {
      // Update URL to edit mode without page reload
      const newUrl = `/section/ecommerce/view/workspace/category/edit/${category.id}`;
      window.history.replaceState({}, '', newUrl);
      this.categoryId = category.id;
      this._mode = 'edit';
    }
  }

  _handleEditorCancel() {
    // Navigate back to categories list
    window.history.pushState({}, '', '/section/ecommerce/view/categories');
    window.dispatchEvent(new PopStateEvent('popstate'));
  }

  render() {
    return html`
      <div class="workspace-container">
        <ecommerce-category-editor
          .categoryId=${this.categoryId}
          @category-saved=${this._handleCategorySaved}
          @editor-cancel=${this._handleEditorCancel}
        ></ecommerce-category-editor>
      </div>
    `;
  }
}

customElements.define('ecommerce-category-workspace', CategoryWorkspace);

export default CategoryWorkspace;
