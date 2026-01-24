import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Category Tree Element
 * Displays the hierarchical category navigation tree in the sidebar.
 */
export class CategoryTree extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
    }

    .tree-root {
      padding: var(--uui-size-space-2);
    }

    .tree-item {
      display: flex;
      align-items: center;
      padding: var(--uui-size-space-2) var(--uui-size-space-3);
      cursor: pointer;
      border-radius: var(--uui-border-radius);
      margin-bottom: var(--uui-size-space-1);
    }

    .tree-item:hover {
      background: var(--uui-color-surface-emphasis);
    }

    .tree-item.active {
      background: var(--uui-color-selected);
    }

    .tree-item.is-hidden {
      opacity: 0.5;
    }

    .tree-item uui-icon {
      margin-right: var(--uui-size-space-2);
      color: var(--uui-color-text-alt);
    }

    .tree-item-label {
      flex: 1;
    }

    .tree-item-count {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      background: var(--uui-color-surface-alt);
      padding: var(--uui-size-space-1) var(--uui-size-space-2);
      border-radius: var(--uui-border-radius);
    }

    .tree-children {
      margin-left: var(--uui-size-layout-1);
      border-left: 1px solid var(--uui-color-border);
      padding-left: var(--uui-size-space-2);
    }

    .loading {
      display: flex;
      justify-content: center;
      padding: var(--uui-size-layout-1);
    }

    .expand-icon {
      width: 20px;
      display: flex;
      justify-content: center;
    }

    .featured-badge {
      background: var(--uui-color-warning-standalone);
      color: white;
      padding: 2px 6px;
      border-radius: var(--uui-border-radius);
      font-size: 10px;
      margin-left: var(--uui-size-space-2);
    }
  `;

  static properties = {
    _treeNodes: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _expandedNodes: { type: Set, state: true },
    _selectedNode: { type: String, state: true },
    _childrenCache: { type: Object, state: true }
  };

  constructor() {
    super();
    this._treeNodes = [];
    this._loading = true;
    this._expandedNodes = new Set();
    this._selectedNode = null;
    this._childrenCache = {};
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadTree();
  }

  async _loadTree() {
    try {
      this._loading = true;

      const response = await fetch('/umbraco/management/api/v1/ecommerce/category/tree', {
        credentials: 'include',
        headers: {
          'Accept': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error('Failed to load category tree');
      }

      const data = await response.json();
      this._treeNodes = data.nodes || [];
    } catch (error) {
      console.error('Error loading category tree:', error);
      this._treeNodes = [];
    } finally {
      this._loading = false;
    }
  }

  async _loadChildren(categoryId) {
    if (this._childrenCache[categoryId]) {
      return this._childrenCache[categoryId];
    }

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/category/tree/${categoryId}/children`, {
        credentials: 'include',
        headers: {
          'Accept': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error('Failed to load tree children');
      }

      const data = await response.json();
      this._childrenCache[categoryId] = data.nodes || [];
      return this._childrenCache[categoryId];
    } catch (error) {
      console.error('Error loading tree children:', error);
      return [];
    }
  }

  async _handleNodeClick(node) {
    this._selectedNode = node.id;

    // Navigate to category editor
    this._navigateToCategory(node.id);
  }

  async _handleExpandClick(e, node) {
    e.stopPropagation();

    if (this._expandedNodes.has(node.id)) {
      this._expandedNodes.delete(node.id);
    } else {
      this._expandedNodes.add(node.id);
      // Load children if needed
      if (!this._childrenCache[node.id]) {
        await this._loadChildren(node.id);
      }
    }
    this._expandedNodes = new Set(this._expandedNodes);
    this.requestUpdate();
  }

  _navigateToCategory(categoryId) {
    const event = new CustomEvent('umb-navigate', {
      bubbles: true,
      composed: true,
      detail: {
        path: `/section/ecommerce/workspace/category/edit/${categoryId}`
      }
    });
    this.dispatchEvent(event);
  }

  render() {
    if (this._loading) {
      return html`
        <div class="loading">
          <uui-loader></uui-loader>
        </div>
      `;
    }

    return html`
      <div class="tree-root">
        ${this._treeNodes.length === 0 ? html`
          <p style="color: var(--uui-color-text-alt); padding: var(--uui-size-space-4);">
            No categories yet. Create your first category.
          </p>
        ` : ''}
        ${this._treeNodes.map(node => this._renderTreeNode(node, 0))}
      </div>
    `;
  }

  _renderTreeNode(node, level) {
    const isExpanded = this._expandedNodes.has(node.id);
    const isSelected = this._selectedNode === node.id;
    const children = this._childrenCache[node.id] || [];
    const cssClasses = node.cssClasses || [];

    return html`
      <div>
        <div
          class="tree-item ${isSelected ? 'active' : ''} ${cssClasses.includes('is-hidden') ? 'is-hidden' : ''}"
          @click=${() => this._handleNodeClick(node)}
        >
          <div class="expand-icon">
            ${node.hasChildren ? html`
              <uui-icon
                name="${isExpanded ? 'icon-navigation-down' : 'icon-navigation-right'}"
                @click=${(e) => this._handleExpandClick(e, node)}
              ></uui-icon>
            ` : ''}
          </div>
          <uui-icon name="${node.icon || 'icon-folder'}"></uui-icon>
          <span class="tree-item-label">${node.name}</span>
          ${node.isFeatured ? html`<span class="featured-badge">Featured</span>` : ''}
          ${node.productCount != null ? html`
            <span class="tree-item-count">${node.productCount}</span>
          ` : ''}
        </div>
        ${node.hasChildren && isExpanded ? html`
          <div class="tree-children">
            ${children.map(child => this._renderTreeNode(child, level + 1))}
          </div>
        ` : ''}
      </div>
    `;
  }
}

customElements.define('ecommerce-category-tree', CategoryTree);

export default CategoryTree;
