import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Category Collection View
 * Displays a list/grid of categories with search functionality.
 */
export class CategoryCollection extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .collection-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--uui-size-layout-1);
      gap: var(--uui-size-layout-1);
    }

    .search-box {
      flex: 1;
      max-width: 400px;
    }

    .search-box uui-input {
      width: 100%;
    }

    .collection-table {
      width: 100%;
      border-collapse: collapse;
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
    }

    .collection-table th,
    .collection-table td {
      padding: var(--uui-size-space-4);
      text-align: left;
      border-bottom: 1px solid var(--uui-color-border);
    }

    .collection-table th {
      background: var(--uui-color-surface-alt);
      font-weight: bold;
      color: var(--uui-color-text);
    }

    .collection-table tr:hover {
      background: var(--uui-color-surface-emphasis);
      cursor: pointer;
    }

    .collection-table tr:last-child td {
      border-bottom: none;
    }

    .category-image {
      width: 50px;
      height: 50px;
      object-fit: cover;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-alt);
    }

    .category-name {
      font-weight: 500;
    }

    .category-path {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
    }

    .badge {
      display: inline-block;
      padding: var(--uui-size-space-1) var(--uui-size-space-2);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
      font-weight: 500;
      margin-right: var(--uui-size-space-1);
    }

    .badge-visible {
      background: var(--uui-color-positive-standalone);
      color: white;
    }

    .badge-hidden {
      background: var(--uui-color-danger-standalone);
      color: white;
    }

    .badge-featured {
      background: var(--uui-color-warning-standalone);
      color: white;
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

    .loading {
      display: flex;
      justify-content: center;
      padding: var(--uui-size-layout-2);
    }

    .hierarchy-indent {
      display: inline-block;
      width: 20px;
      border-left: 2px solid var(--uui-color-border);
      margin-right: var(--uui-size-space-2);
    }
  `;

  static properties = {
    _categories: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _searchTerm: { type: String, state: true },
    _totalCount: { type: Number, state: true }
  };

  constructor() {
    super();
    this._categories = [];
    this._loading = true;
    this._searchTerm = '';
    this._totalCount = 0;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadCategories();
  }

  async _loadCategories() {
    try {
      this._loading = true;

      const response = await fetch('/umbraco/management/api/v1/ecommerce/category', {
        headers: {
          'Accept': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error('Failed to load categories');
      }

      const data = await response.json();
      this._categories = data.items || [];
      this._totalCount = data.total || 0;
    } catch (error) {
      console.error('Error loading categories:', error);
      this._categories = [];
    } finally {
      this._loading = false;
    }
  }

  _handleSearch(event) {
    this._searchTerm = event.target.value.toLowerCase();
  }

  _getFilteredCategories() {
    if (!this._searchTerm) {
      return this._categories;
    }
    return this._categories.filter(c =>
      c.name.toLowerCase().includes(this._searchTerm) ||
      (c.slug && c.slug.toLowerCase().includes(this._searchTerm))
    );
  }

  _handleRowClick(category) {
    window.history.pushState(null, '', `/umbraco/section/ecommerce/workspace/category/edit/${category.id}`);

    const event = new CustomEvent('umb-navigate', {
      bubbles: true,
      composed: true,
      detail: {
        path: `/section/ecommerce/workspace/category/edit/${category.id}`
      }
    });
    this.dispatchEvent(event);
  }

  _handleCreateCategory() {
    window.history.pushState(null, '', '/umbraco/section/ecommerce/workspace/category/create');

    const event = new CustomEvent('umb-navigate', {
      bubbles: true,
      composed: true,
      detail: {
        path: '/section/ecommerce/workspace/category/create'
      }
    });
    this.dispatchEvent(event);
  }

  render() {
    return html`
      <div class="collection-header">
        <div class="search-box">
          <uui-input
            placeholder="Search categories..."
            .value=${this._searchTerm}
            @input=${this._handleSearch}
          >
            <uui-icon name="icon-search" slot="prepend"></uui-icon>
          </uui-input>
        </div>
        <uui-button look="primary" @click=${this._handleCreateCategory}>
          <uui-icon name="icon-add"></uui-icon>
          Add Category
        </uui-button>
      </div>

      ${this._loading
        ? html`<div class="loading"><uui-loader></uui-loader></div>`
        : this._getFilteredCategories().length === 0
          ? this._renderEmptyState()
          : this._renderCategoriesTable()
      }
    `;
  }

  _renderEmptyState() {
    return html`
      <div class="empty-state">
        <uui-icon name="icon-folders"></uui-icon>
        <h3>No categories found</h3>
        <p>${this._searchTerm ? 'Try adjusting your search criteria' : 'Get started by creating your first category'}</p>
        ${!this._searchTerm ? html`
          <uui-button look="primary" @click=${this._handleCreateCategory}>
            Add Your First Category
          </uui-button>
        ` : ''}
      </div>
    `;
  }

  _renderCategoriesTable() {
    const filteredCategories = this._getFilteredCategories();

    return html`
      <table class="collection-table">
        <thead>
          <tr>
            <th style="width: 60px;"></th>
            <th>Name</th>
            <th>Slug</th>
            <th>Products</th>
            <th>Status</th>
            <th>Sort Order</th>
          </tr>
        </thead>
        <tbody>
          ${filteredCategories.map(category => html`
            <tr @click=${() => this._handleRowClick(category)}>
              <td>
                ${category.imageUrl
                  ? html`<img class="category-image" src="${category.imageUrl}" alt="${category.name}" />`
                  : html`<div class="category-image" style="display: flex; align-items: center; justify-content: center;"><uui-icon name="icon-folder"></uui-icon></div>`
                }
              </td>
              <td>
                <div class="category-name">${category.name}</div>
                ${category.parentId ? html`<div class="category-path">Subcategory</div>` : ''}
              </td>
              <td>${category.slug || '-'}</td>
              <td>-</td>
              <td>
                <span class="badge ${category.isVisible ? 'badge-visible' : 'badge-hidden'}">
                  ${category.isVisible ? 'Visible' : 'Hidden'}
                </span>
                ${category.isFeatured ? html`<span class="badge badge-featured">Featured</span>` : ''}
              </td>
              <td>${category.sortOrder}</td>
            </tr>
          `)}
        </tbody>
      </table>
    `;
  }
}

customElements.define('ecommerce-category-collection', CategoryCollection);

export default CategoryCollection;
