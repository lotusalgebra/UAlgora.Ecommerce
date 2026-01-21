import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Shipping Tree Element
 * Displays shipping methods and zones in the sidebar.
 */
export class ShippingTree extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
    }

    .tree-root {
      padding: var(--uui-size-space-2);
    }

    .tree-section {
      margin-bottom: var(--uui-size-space-4);
    }

    .section-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: var(--uui-size-space-2) var(--uui-size-space-3);
      font-weight: bold;
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      text-transform: uppercase;
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
      color: var(--uui-color-text-alt);
    }

    .status-dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      margin-left: var(--uui-size-space-2);
    }

    .dot-active { background: var(--uui-color-positive); }
    .dot-inactive { background: var(--uui-color-surface-alt); }

    .loading {
      display: flex;
      justify-content: center;
      padding: var(--uui-size-layout-1);
    }

    .add-button {
      width: 100%;
      margin-top: var(--uui-size-space-2);
    }
  `;

  static properties = {
    _methods: { type: Array, state: true },
    _zones: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _selectedId: { type: String, state: true }
  };

  constructor() {
    super();
    this._methods = [];
    this._zones = [];
    this._loading = true;
    this._selectedId = null;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadData();
  }

  async _loadData() {
    try {
      this._loading = true;

      const [methodsResponse, zonesResponse] = await Promise.all([
        fetch('/umbraco/management/api/v1/ecommerce/shipping/method?includeInactive=true', {
          headers: { 'Accept': 'application/json' }
        }),
        fetch('/umbraco/management/api/v1/ecommerce/shipping/zone?includeInactive=true', {
          headers: { 'Accept': 'application/json' }
        })
      ]);

      if (methodsResponse.ok) {
        const methodsData = await methodsResponse.json();
        this._methods = methodsData.items || [];
      }

      if (zonesResponse.ok) {
        const zonesData = await zonesResponse.json();
        this._zones = zonesData.items || [];
      }
    } catch (error) {
      console.error('Error loading shipping data:', error);
    } finally {
      this._loading = false;
    }
  }

  _handleMethodClick(method) {
    this._selectedId = `method-${method.id}`;
    this._navigateTo(`/section/ecommerce/workspace/shipping-method/edit/${method.id}`);
  }

  _handleZoneClick(zone) {
    this._selectedId = `zone-${zone.id}`;
    this._navigateTo(`/section/ecommerce/workspace/shipping-zone/edit/${zone.id}`);
  }

  _handleCreateMethod() {
    this._navigateTo('/section/ecommerce/workspace/shipping-method/create');
  }

  _handleCreateZone() {
    this._navigateTo('/section/ecommerce/workspace/shipping-zone/create');
  }

  _navigateTo(path) {
    window.history.pushState(null, '', `/umbraco${path}`);
    const event = new CustomEvent('umb-navigate', {
      bubbles: true,
      composed: true,
      detail: { path }
    });
    this.dispatchEvent(event);
  }

  render() {
    if (this._loading) {
      return html`<div class="loading"><uui-loader></uui-loader></div>`;
    }

    return html`
      <div class="tree-root">
        <div class="tree-section">
          <div class="section-header">
            <span>Shipping Methods</span>
            <span class="tree-item-count">${this._methods.length}</span>
          </div>
          ${this._methods.map(method => html`
            <div
              class="tree-item ${this._selectedId === `method-${method.id}` ? 'active' : ''}"
              @click=${() => this._handleMethodClick(method)}
            >
              <uui-icon name="icon-truck"></uui-icon>
              <span class="tree-item-label">${method.name}</span>
              <span class="status-dot ${method.isActive ? 'dot-active' : 'dot-inactive'}"></span>
            </div>
          `)}
          <uui-button class="add-button" look="secondary" @click=${this._handleCreateMethod}>
            <uui-icon name="icon-add"></uui-icon>
            Add Method
          </uui-button>
        </div>

        <div class="tree-section">
          <div class="section-header">
            <span>Shipping Zones</span>
            <span class="tree-item-count">${this._zones.length}</span>
          </div>
          ${this._zones.map(zone => html`
            <div
              class="tree-item ${this._selectedId === `zone-${zone.id}` ? 'active' : ''}"
              @click=${() => this._handleZoneClick(zone)}
            >
              <uui-icon name="icon-globe"></uui-icon>
              <span class="tree-item-label">
                ${zone.name}
                ${zone.isDefault ? html`<small>(Default)</small>` : ''}
              </span>
              <span class="status-dot ${zone.isActive ? 'dot-active' : 'dot-inactive'}"></span>
            </div>
          `)}
          <uui-button class="add-button" look="secondary" @click=${this._handleCreateZone}>
            <uui-icon name="icon-add"></uui-icon>
            Add Zone
          </uui-button>
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-shipping-tree', ShippingTree);

export default ShippingTree;
