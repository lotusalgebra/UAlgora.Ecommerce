import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Order Tree Element
 * Displays orders organized by status in the sidebar.
 */
export class OrderTree extends UmbElementMixin(LitElement) {
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

    .tree-item.has-pending {
      font-weight: bold;
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
      min-width: 24px;
      text-align: center;
    }

    .count-pending {
      background: #ffc107;
      color: #000;
    }

    .count-default {
      background: var(--uui-color-surface-alt);
      color: var(--uui-color-text-alt);
    }

    .loading {
      display: flex;
      justify-content: center;
      padding: var(--uui-size-layout-1);
    }

    .status-dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      margin-right: var(--uui-size-space-2);
    }

    .dot-pending { background: #ffc107; }
    .dot-confirmed { background: #17a2b8; }
    .dot-processing { background: #007bff; }
    .dot-shipped { background: #6f42c1; }
    .dot-delivered { background: #28a745; }
    .dot-completed { background: #28a745; }
    .dot-cancelled { background: #dc3545; }
    .dot-refunded { background: #fd7e14; }
    .dot-onhold { background: #6c757d; }
    .dot-failed { background: #dc3545; }
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

      const response = await fetch('/umbraco/management/api/v1/ecommerce/order/tree', {
        headers: {
          'Accept': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error('Failed to load order tree');
      }

      const data = await response.json();
      this._treeNodes = data.nodes || [];
    } catch (error) {
      console.error('Error loading order tree:', error);
      this._treeNodes = [];
    } finally {
      this._loading = false;
    }
  }

  _handleNodeClick(node) {
    this._selectedNode = node.id;

    if (node.id === 'all-orders') {
      this._navigateToCollection();
    } else {
      this._navigateToCollection(node.status);
    }
  }

  _navigateToCollection(status = null) {
    const path = status
      ? `/section/ecommerce/orders?status=${status}`
      : '/section/ecommerce/orders';

    const event = new CustomEvent('umb-navigate', {
      bubbles: true,
      composed: true,
      detail: { path }
    });
    this.dispatchEvent(event);
  }

  _getStatusKey(status) {
    if (!status) return 'default';
    return status.toString().toLowerCase().replace(/\s+/g, '');
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
        ${this._treeNodes.map(node => {
          const isSelected = this._selectedNode === node.id;
          const isPending = node.id === 'status-0' && node.count > 0;
          const statusKey = this._getStatusKey(node.status);

          return html`
            <div
              class="tree-item ${isSelected ? 'active' : ''} ${isPending ? 'has-pending' : ''}"
              @click=${() => this._handleNodeClick(node)}
            >
              ${node.status != null ? html`
                <span class="status-dot dot-${statusKey}"></span>
              ` : html`
                <uui-icon name="${node.icon}"></uui-icon>
              `}
              <span class="tree-item-label">${node.name}</span>
              <span class="tree-item-count ${isPending ? 'count-pending' : 'count-default'}">
                ${node.count}
              </span>
            </div>
          `;
        })}
      </div>
    `;
  }
}

customElements.define('ecommerce-order-tree', OrderTree);

export default OrderTree;
