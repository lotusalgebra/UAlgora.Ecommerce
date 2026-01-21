import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Discount Tree Element
 * Displays discounts organized by status in the sidebar.
 */
export class DiscountTree extends UmbElementMixin(LitElement) {
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
      min-width: 24px;
      text-align: center;
    }

    .count-active {
      background: var(--uui-color-positive-emphasis);
      color: var(--uui-color-positive);
    }

    .count-scheduled {
      background: var(--uui-color-warning-emphasis);
      color: var(--uui-color-warning);
    }

    .count-expired {
      background: var(--uui-color-danger-emphasis);
      color: var(--uui-color-danger);
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

    .dot-active { background: var(--uui-color-positive); }
    .dot-scheduled { background: var(--uui-color-warning); }
    .dot-expired { background: var(--uui-color-danger); }
    .dot-all { background: var(--uui-color-interactive); }
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

      const response = await fetch('/umbraco/management/api/v1/ecommerce/discount/tree', {
        headers: {
          'Accept': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error('Failed to load discount tree');
      }

      const data = await response.json();
      this._treeNodes = data.nodes || [];
    } catch (error) {
      console.error('Error loading discount tree:', error);
      this._treeNodes = [
        { id: 'active', name: 'Active Discounts', icon: 'icon-check', count: 0, filterType: 'active' },
        { id: 'scheduled', name: 'Scheduled', icon: 'icon-calendar', count: 0, filterType: 'scheduled' },
        { id: 'expired', name: 'Expired', icon: 'icon-time', count: 0, filterType: 'expired' },
        { id: 'all-discounts', name: 'All Discounts', icon: 'icon-tag', count: 0, filterType: 'all' }
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
      ? `/section/ecommerce/discounts?filter=${filterType}`
      : '/section/ecommerce/discounts';

    const event = new CustomEvent('umb-navigate', {
      bubbles: true,
      composed: true,
      detail: { path }
    });
    this.dispatchEvent(event);
  }

  _getCountClass(filterType) {
    switch (filterType) {
      case 'active': return 'count-active';
      case 'scheduled': return 'count-scheduled';
      case 'expired': return 'count-expired';
      default: return 'count-default';
    }
  }

  _getDotClass(filterType) {
    switch (filterType) {
      case 'active': return 'dot-active';
      case 'scheduled': return 'dot-scheduled';
      case 'expired': return 'dot-expired';
      default: return 'dot-all';
    }
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
            <span class="status-dot ${this._getDotClass(node.filterType)}"></span>
            <span class="tree-item-label">${node.name}</span>
            <span class="tree-item-count ${this._getCountClass(node.filterType)}">
              ${node.count}
            </span>
          </div>
        `)}
      </div>
    `;
  }
}

customElements.define('ecommerce-discount-tree', DiscountTree);

export default DiscountTree;
