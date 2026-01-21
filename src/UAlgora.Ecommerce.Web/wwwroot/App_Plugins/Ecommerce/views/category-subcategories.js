import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Category Subcategories View
 * Manages child categories of the current category.
 */
export class CategorySubcategories extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .subcategories-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--uui-size-layout-1);
    }

    .subcategories-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: var(--uui-size-layout-1);
    }

    .subcategory-card {
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-1);
      cursor: pointer;
      transition: all 0.2s;
    }

    .subcategory-card:hover {
      border-color: var(--uui-color-interactive);
      box-shadow: var(--uui-shadow-depth-1);
    }

    .subcategory-card.hidden {
      opacity: 0.6;
    }

    .subcategory-header {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-3);
      margin-bottom: var(--uui-size-space-3);
    }

    .subcategory-icon {
      width: 48px;
      height: 48px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-alt);
      display: flex;
      align-items: center;
      justify-content: center;
      overflow: hidden;
    }

    .subcategory-icon img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .subcategory-icon uui-icon {
      font-size: 24px;
      color: var(--uui-color-text-alt);
    }

    .subcategory-info {
      flex: 1;
      min-width: 0;
    }

    .subcategory-name {
      font-weight: bold;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .subcategory-slug {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      font-family: monospace;
    }

    .subcategory-stats {
      display: flex;
      gap: var(--uui-size-layout-1);
      margin-top: var(--uui-size-space-3);
      padding-top: var(--uui-size-space-3);
      border-top: 1px solid var(--uui-color-border);
    }

    .stat {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
    }

    .stat-value {
      font-weight: bold;
      color: var(--uui-color-text);
    }

    .badge {
      display: inline-flex;
      align-items: center;
      gap: var(--uui-size-space-1);
      padding: var(--uui-size-space-1) var(--uui-size-space-2);
      border-radius: var(--uui-border-radius);
      font-size: 10px;
      font-weight: bold;
      text-transform: uppercase;
    }

    .badge-featured {
      background: var(--uui-color-warning-emphasis);
      color: var(--uui-color-warning-standalone);
    }

    .badge-hidden {
      background: var(--uui-color-surface-alt);
      color: var(--uui-color-text-alt);
    }

    .empty-state {
      text-align: center;
      padding: var(--uui-size-layout-3);
      color: var(--uui-color-text-alt);
    }

    .empty-state uui-icon {
      font-size: 64px;
      margin-bottom: var(--uui-size-space-4);
    }

    .loading {
      display: flex;
      justify-content: center;
      padding: var(--uui-size-layout-2);
    }

    .sort-controls {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
      margin-bottom: var(--uui-size-layout-1);
      padding: var(--uui-size-space-3);
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
    }

    .reorder-table {
      width: 100%;
      border-collapse: collapse;
    }

    .reorder-table td {
      padding: var(--uui-size-space-3);
      border-bottom: 1px solid var(--uui-color-border);
      vertical-align: middle;
    }

    .reorder-table tr:hover {
      background: var(--uui-color-surface-emphasis);
    }

    .drag-handle {
      cursor: grab;
      color: var(--uui-color-text-alt);
    }

    .drag-handle:active {
      cursor: grabbing;
    }
  `;

  static properties = {
    _category: { type: Object, state: true },
    _subcategories: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _reorderMode: { type: Boolean, state: true },
    _saving: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._category = null;
    this._subcategories = [];
    this._loading = true;
    this._reorderMode = false;
    this._saving = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = this.closest('ecommerce-category-workspace');
    if (workspace) {
      this._category = workspace.getCategory();
      if (this._category?.id) {
        this._loadSubcategories();
      } else {
        this._loading = false;
      }
    }
  }

  async _loadSubcategories() {
    if (!this._category?.id) {
      this._loading = false;
      return;
    }

    try {
      this._loading = true;
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/category/tree/${this._category.id}/children`, {
        headers: { 'Accept': 'application/json' }
      });

      if (response.ok) {
        const data = await response.json();
        this._subcategories = data.nodes || [];
      }
    } catch (error) {
      console.error('Error loading subcategories:', error);
    } finally {
      this._loading = false;
    }
  }

  _handleSubcategoryClick(subcategory) {
    window.location.href = `/umbraco/section/ecommerce/workspace/category/edit/${subcategory.id}`;
  }

  _handleCreateSubcategory() {
    window.location.href = `/umbraco/section/ecommerce/workspace/category/create?parentId=${this._category.id}`;
  }

  _toggleReorderMode() {
    this._reorderMode = !this._reorderMode;
  }

  _handleMoveUp(index) {
    if (index === 0) return;
    const newOrder = [...this._subcategories];
    [newOrder[index - 1], newOrder[index]] = [newOrder[index], newOrder[index - 1]];
    this._subcategories = newOrder;
  }

  _handleMoveDown(index) {
    if (index === this._subcategories.length - 1) return;
    const newOrder = [...this._subcategories];
    [newOrder[index], newOrder[index + 1]] = [newOrder[index + 1], newOrder[index]];
    this._subcategories = newOrder;
  }

  async _handleSaveOrder() {
    this._saving = true;

    try {
      const sortOrders = this._subcategories.map((cat, index) => ({
        id: cat.id,
        sortOrder: index
      }));

      // Update each category's sort order
      for (const item of sortOrders) {
        const response = await fetch(`/umbraco/management/api/v1/ecommerce/category/${item.id}`, {
          headers: { 'Accept': 'application/json' }
        });

        if (response.ok) {
          const category = await response.json();
          category.sortOrder = item.sortOrder;

          await fetch(`/umbraco/management/api/v1/ecommerce/category/${item.id}`, {
            method: 'PUT',
            headers: {
              'Content-Type': 'application/json',
              'Accept': 'application/json'
            },
            body: JSON.stringify(category)
          });
        }
      }

      this._reorderMode = false;

      const event = new CustomEvent('umb-notification', {
        bubbles: true,
        composed: true,
        detail: {
          headline: 'Success',
          message: 'Category order saved',
          color: 'positive'
        }
      });
      this.dispatchEvent(event);
    } catch (error) {
      console.error('Error saving order:', error);
    } finally {
      this._saving = false;
    }
  }

  render() {
    const isNew = !this._category?.id;

    if (isNew) {
      return html`
        <uui-box>
          <div class="empty-state">
            <uui-icon name="icon-folders"></uui-icon>
            <h3>Save Category First</h3>
            <p>Save this category to add subcategories.</p>
          </div>
        </uui-box>
      `;
    }

    if (this._loading) {
      return html`<div class="loading"><uui-loader></uui-loader></div>`;
    }

    return html`
      <uui-box>
        <div class="subcategories-header">
          <div slot="headline">Subcategories (${this._subcategories.length})</div>
          <div style="display: flex; gap: var(--uui-size-space-2);">
            ${this._subcategories.length > 1 ? html`
              <uui-button
                look="secondary"
                @click=${this._toggleReorderMode}
                ?disabled=${this._saving}
              >
                <uui-icon name="${this._reorderMode ? 'icon-check' : 'icon-navigation'}"></uui-icon>
                ${this._reorderMode ? 'Done' : 'Reorder'}
              </uui-button>
            ` : ''}
            <uui-button
              look="primary"
              @click=${this._handleCreateSubcategory}
            >
              <uui-icon name="icon-add"></uui-icon>
              Add Subcategory
            </uui-button>
          </div>
        </div>

        ${this._reorderMode ? html`
          <div class="sort-controls">
            <uui-icon name="icon-info"></uui-icon>
            <span>Use the arrows to reorder subcategories, then click Save Order.</span>
            <uui-button
              look="primary"
              compact
              @click=${this._handleSaveOrder}
              ?disabled=${this._saving}
            >
              ${this._saving ? 'Saving...' : 'Save Order'}
            </uui-button>
          </div>

          <table class="reorder-table">
            ${this._subcategories.map((subcategory, index) => html`
              <tr>
                <td style="width: 40px;">
                  <span class="drag-handle">
                    <uui-icon name="icon-navigation"></uui-icon>
                  </span>
                </td>
                <td>
                  <div class="subcategory-name">${subcategory.name}</div>
                </td>
                <td style="width: 100px; text-align: right;">
                  <uui-button
                    compact
                    look="secondary"
                    ?disabled=${index === 0}
                    @click=${() => this._handleMoveUp(index)}
                  >
                    <uui-icon name="icon-arrow-up"></uui-icon>
                  </uui-button>
                  <uui-button
                    compact
                    look="secondary"
                    ?disabled=${index === this._subcategories.length - 1}
                    @click=${() => this._handleMoveDown(index)}
                  >
                    <uui-icon name="icon-arrow-down"></uui-icon>
                  </uui-button>
                </td>
              </tr>
            `)}
          </table>
        ` : html`
          ${this._subcategories.length === 0
            ? this._renderEmptyState()
            : this._renderSubcategoriesGrid()
          }
        `}
      </uui-box>
    `;
  }

  _renderEmptyState() {
    return html`
      <div class="empty-state">
        <uui-icon name="icon-folders"></uui-icon>
        <h3>No Subcategories</h3>
        <p>This category has no child categories yet.</p>
        <uui-button look="primary" @click=${this._handleCreateSubcategory}>
          <uui-icon name="icon-add"></uui-icon>
          Create First Subcategory
        </uui-button>
      </div>
    `;
  }

  _renderSubcategoriesGrid() {
    return html`
      <div class="subcategories-grid">
        ${this._subcategories.map(subcategory => html`
          <div
            class="subcategory-card ${!subcategory.isVisible ? 'hidden' : ''}"
            @click=${() => this._handleSubcategoryClick(subcategory)}
          >
            <div class="subcategory-header">
              <div class="subcategory-icon">
                ${subcategory.imageUrl
                  ? html`<img src="${subcategory.imageUrl}" alt="${subcategory.name}" />`
                  : html`<uui-icon name="icon-folder"></uui-icon>`
                }
              </div>
              <div class="subcategory-info">
                <div class="subcategory-name">${subcategory.name}</div>
                <div style="display: flex; gap: var(--uui-size-space-2); margin-top: var(--uui-size-space-1);">
                  ${subcategory.isFeatured ? html`<span class="badge badge-featured">Featured</span>` : ''}
                  ${!subcategory.isVisible ? html`<span class="badge badge-hidden">Hidden</span>` : ''}
                </div>
              </div>
            </div>
            <div class="subcategory-stats">
              <div class="stat">
                <uui-icon name="icon-box"></uui-icon>
                <span class="stat-value">${subcategory.productCount || 0}</span>
                <span>products</span>
              </div>
              ${subcategory.hasChildren ? html`
                <div class="stat">
                  <uui-icon name="icon-folder"></uui-icon>
                  <span>Has subcategories</span>
                </div>
              ` : ''}
            </div>
          </div>
        `)}
      </div>
    `;
  }
}

customElements.define('ecommerce-category-subcategories', CategorySubcategories);

export default CategorySubcategories;
