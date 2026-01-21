import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Product Tree Element
 * Displays the product navigation tree in the sidebar.
 */
export class ProductTree extends UmbElementMixin(LitElement) {
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
    }

    .loading {
      display: flex;
      justify-content: center;
      padding: var(--uui-size-layout-1);
    }
  `;

  static properties = {
    _treeNodes: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _expandedNodes: { type: Set, state: true },
    _selectedNode: { type: String, state: true }
  };

  constructor() {
    super();
    this._treeNodes = [];
    this._loading = true;
    this._expandedNodes = new Set();
    this._selectedNode = null;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadTree();
  }

  async _loadTree() {
    try {
      this._loading = true;

      const response = await fetch('/umbraco/management/api/v1/ecommerce/product/tree', {
        headers: {
          'Accept': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error('Failed to load product tree');
      }

      const data = await response.json();
      this._treeNodes = data.nodes || [];
    } catch (error) {
      console.error('Error loading product tree:', error);
      this._treeNodes = [];
    } finally {
      this._loading = false;
    }
  }

  async _loadChildren(nodeId) {
    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/product/tree/${nodeId}/children`, {
        headers: {
          'Accept': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error('Failed to load tree children');
      }

      const data = await response.json();
      return data.items || [];
    } catch (error) {
      console.error('Error loading tree children:', error);
      return [];
    }
  }

  _handleNodeClick(node) {
    this._selectedNode = node.id;

    if (node.hasChildren) {
      if (this._expandedNodes.has(node.id)) {
        this._expandedNodes.delete(node.id);
      } else {
        this._expandedNodes.add(node.id);
      }
      this._expandedNodes = new Set(this._expandedNodes);
    }

    // Navigate to the appropriate view
    if (node.filterType === 'all') {
      this._navigateToCollection();
    } else if (node.entityType === 'ecommerce-product') {
      this._navigateToProduct(node.id);
    }
  }

  _navigateToCollection() {
    // Dispatch navigation event
    const event = new CustomEvent('umb-navigate', {
      bubbles: true,
      composed: true,
      detail: {
        path: '/section/ecommerce/products'
      }
    });
    this.dispatchEvent(event);
  }

  _navigateToProduct(productId) {
    const event = new CustomEvent('umb-navigate', {
      bubbles: true,
      composed: true,
      detail: {
        path: `/section/ecommerce/workspace/product/edit/${productId}`
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
        ${this._treeNodes.map(node => this._renderTreeNode(node))}
      </div>
    `;
  }

  _renderTreeNode(node) {
    const isExpanded = this._expandedNodes.has(node.id);
    const isSelected = this._selectedNode === node.id;

    return html`
      <div>
        <div
          class="tree-item ${isSelected ? 'active' : ''}"
          @click=${() => this._handleNodeClick(node)}
        >
          ${node.hasChildren ? html`
            <uui-icon name="${isExpanded ? 'icon-navigation-down' : 'icon-navigation-right'}"></uui-icon>
          ` : html`
            <uui-icon name="${node.icon || 'icon-box'}"></uui-icon>
          `}
          <span class="tree-item-label">${node.name}</span>
          ${node.count != null ? html`
            <span class="tree-item-count">${node.count}</span>
          ` : ''}
        </div>
        ${node.hasChildren && isExpanded && node.children ? html`
          <div class="tree-children">
            ${node.children.map(child => this._renderTreeNode(child))}
          </div>
        ` : ''}
      </div>
    `;
  }
}

customElements.define('ecommerce-product-tree', ProductTree);

export default ProductTree;
