import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Customer Tree Element
 * Displays customers organized by filters in the sidebar.
 */
export class CustomerTree extends UmbElementMixin(LitElement) {
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
    }

    .tree-item-label {
      flex: 1;
    }

    .tree-item-count {
      font-size: var(--uui-type-small-size);
      padding: var(--uui-size-space-1) var(--uui-size-space-2);
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-alt);
      color: var(--uui-color-text-alt);
      min-width: 24px;
      text-align: center;
    }

    .loading {
      display: flex;
      justify-content: center;
      padding: var(--uui-size-layout-1);
    }

    .section-divider {
      height: 1px;
      background: var(--uui-color-border);
      margin: var(--uui-size-space-3) 0;
    }

    .section-label {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      padding: var(--uui-size-space-2) var(--uui-size-space-3);
      font-weight: 500;
      text-transform: uppercase;
    }
  `;

  static properties = {
    _treeNodes: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _selectedNode: { type: String, state: true }
  };

  constructor() {
    super();
    this._treeNodes = [];
    this._loading = true;
    this._selectedNode = null;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadTree();
  }

  async _loadTree() {
    try {
      this._loading = true;

      const response = await fetch('/umbraco/management/api/v1/ecommerce/customer/tree', {
        headers: {
          'Accept': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error('Failed to load customer tree');
      }

      const data = await response.json();
      this._treeNodes = data.nodes || [];
    } catch (error) {
      console.error('Error loading customer tree:', error);
      this._treeNodes = [
        {
          id: 'all-customers',
          name: 'All Customers',
          icon: 'icon-users',
          filterType: 'all'
        },
        {
          id: 'top-spenders',
          name: 'Top Spenders',
          icon: 'icon-medal',
          filterType: 'top-spenders'
        },
        {
          id: 'recent-customers',
          name: 'Recently Active',
          icon: 'icon-calendar',
          filterType: 'recent'
        }
      ];
    } finally {
      this._loading = false;
    }
  }

  _handleNodeClick(node) {
    this._selectedNode = node.id;
    this._navigateToCollection(node.filterType);
  }

  _navigateToCollection(filterType = 'all') {
    const path = filterType && filterType !== 'all'
      ? `/section/ecommerce/customers?filter=${filterType}`
      : '/section/ecommerce/customers';

    const event = new CustomEvent('umb-navigate', {
      bubbles: true,
      composed: true,
      detail: { path }
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
        ${this._treeNodes.map(node => html`
          <div
            class="tree-item ${this._selectedNode === node.id ? 'active' : ''}"
            @click=${() => this._handleNodeClick(node)}
          >
            <uui-icon name="${node.icon}"></uui-icon>
            <span class="tree-item-label">${node.name}</span>
            ${node.count !== undefined ? html`
              <span class="tree-item-count">${node.count}</span>
            ` : ''}
          </div>
        `)}
      </div>
    `;
  }
}

customElements.define('ecommerce-customer-tree', CustomerTree);

export default CustomerTree;
