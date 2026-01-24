import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

export class ProductsMenuItem extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
    }
    .menu-section {
      margin-bottom: var(--uui-size-space-4);
    }
    .menu-header {
      display: flex;
      align-items: center;
      padding: var(--uui-size-space-3) var(--uui-size-space-4);
      cursor: pointer;
      font-weight: 600;
      color: var(--uui-color-text);
    }
    .menu-header:hover {
      background: var(--uui-color-surface-emphasis);
      border-radius: var(--uui-border-radius);
    }
    .menu-header uui-icon {
      margin-right: var(--uui-size-space-3);
    }
    .tree-container {
      margin-left: var(--uui-size-space-4);
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
    .tree-item uui-icon {
      margin-right: var(--uui-size-space-2);
      color: var(--uui-color-text-alt);
    }
    .tree-item-label {
      flex: 1;
    }
    .badge {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      background: var(--uui-color-surface-alt);
      padding: 2px 8px;
      border-radius: 10px;
    }
    .loading {
      padding: var(--uui-size-space-4);
      text-align: center;
    }
  `;

  static properties = {
    _expanded: { type: Boolean, state: true },
    _nodes: { type: Array, state: true },
    _loading: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._expanded = false;
    this._nodes = [];
    this._loading = false;
  }

  async _toggleExpand() {
    this._expanded = !this._expanded;
    if (this._expanded && this._nodes.length === 0) {
      await this._loadTree();
    }
  }

  async _loadTree() {
    try {
      this._loading = true;
      const response = await fetch('/umbraco/management/api/v1/ecommerce/product/tree', {
        credentials: 'include',
        headers: { 'Accept': 'application/json' }
      });
      if (response.ok) {
        const data = await response.json();
        this._nodes = data.nodes || [];
      }
    } catch (error) {
      console.error('Error loading product tree:', error);
    } finally {
      this._loading = false;
    }
  }

  _handleNodeClick(node) {
    if (node.nodeType === 'product') {
      window.location.href = `/umbraco/section/ecommerce/workspace/ecommerce-product/edit/${node.entityId}`;
    } else {
      window.location.href = `/umbraco/section/ecommerce/collection/ecommerce-product?categoryId=${node.entityId || 'all'}`;
    }
  }

  render() {
    return html`
      <div class="menu-section">
        <div class="menu-header" @click=${this._toggleExpand}>
          <uui-icon name="${this._expanded ? 'icon-navigation-down' : 'icon-navigation-right'}"></uui-icon>
          <uui-icon name="icon-box"></uui-icon>
          <span>Products</span>
        </div>
        ${this._expanded ? html`
          <div class="tree-container">
            ${this._loading ? html`
              <div class="loading"><uui-loader></uui-loader></div>
            ` : this._nodes.map(node => html`
              <div class="tree-item" @click=${() => this._handleNodeClick(node)}>
                <uui-icon name="${node.icon || 'icon-box'}"></uui-icon>
                <span class="tree-item-label">${node.name}</span>
                ${node.badge ? html`<span class="badge">${node.badge}</span>` : ''}
              </div>
            `)}
          </div>
        ` : ''}
      </div>
    `;
  }
}

customElements.define('ecommerce-products-menu-item', ProductsMenuItem);
export default ProductsMenuItem;
