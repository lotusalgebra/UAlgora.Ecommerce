import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Tax Tree Element
 * Displays tax categories and zones in the sidebar.
 */
export class TaxTree extends UmbElementMixin(LitElement) {
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

    .badge-default {
      font-size: 10px;
      padding: 2px 6px;
      background: var(--uui-color-warning-emphasis);
      color: var(--uui-color-warning);
      border-radius: var(--uui-border-radius);
      margin-left: var(--uui-size-space-2);
    }

    .badge-exempt {
      font-size: 10px;
      padding: 2px 6px;
      background: var(--uui-color-surface-alt);
      color: var(--uui-color-text-alt);
      border-radius: var(--uui-border-radius);
      margin-left: var(--uui-size-space-2);
    }
  `;

  static properties = {
    _categories: { type: Array, state: true },
    _zones: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _selectedId: { type: String, state: true }
  };

  constructor() {
    super();
    this._categories = [];
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

      const [categoriesResponse, zonesResponse] = await Promise.all([
        fetch('/umbraco/management/api/v1/ecommerce/tax/category?includeInactive=true', {
          headers: { 'Accept': 'application/json' }
        }),
        fetch('/umbraco/management/api/v1/ecommerce/tax/zone?includeInactive=true', {
          headers: { 'Accept': 'application/json' }
        })
      ]);

      if (categoriesResponse.ok) {
        const categoriesData = await categoriesResponse.json();
        this._categories = categoriesData.items || [];
      }

      if (zonesResponse.ok) {
        const zonesData = await zonesResponse.json();
        this._zones = zonesData.items || [];
      }
    } catch (error) {
      console.error('Error loading tax data:', error);
    } finally {
      this._loading = false;
    }
  }

  _handleCategoryClick(category) {
    this._selectedId = `category-${category.id}`;
    this._navigateTo(`/section/ecommerce/workspace/tax-category/edit/${category.id}`);
  }

  _handleZoneClick(zone) {
    this._selectedId = `zone-${zone.id}`;
    this._navigateTo(`/section/ecommerce/workspace/tax-zone/edit/${zone.id}`);
  }

  _handleCreateCategory() {
    this._navigateTo('/section/ecommerce/workspace/tax-category/create');
  }

  _handleCreateZone() {
    this._navigateTo('/section/ecommerce/workspace/tax-zone/create');
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
            <span>Tax Categories</span>
            <span class="tree-item-count">${this._categories.length}</span>
          </div>
          ${this._categories.map(category => html`
            <div
              class="tree-item ${this._selectedId === `category-${category.id}` ? 'active' : ''}"
              @click=${() => this._handleCategoryClick(category)}
            >
              <uui-icon name="icon-bill-dollar"></uui-icon>
              <span class="tree-item-label">
                ${category.name}
                ${category.isDefault ? html`<span class="badge-default">Default</span>` : ''}
                ${category.isTaxExempt ? html`<span class="badge-exempt">Exempt</span>` : ''}
              </span>
              <span class="status-dot ${category.isActive ? 'dot-active' : 'dot-inactive'}"></span>
            </div>
          `)}
          <uui-button class="add-button" look="secondary" @click=${this._handleCreateCategory}>
            <uui-icon name="icon-add"></uui-icon>
            Add Category
          </uui-button>
        </div>

        <div class="tree-section">
          <div class="section-header">
            <span>Tax Zones</span>
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
                ${zone.isDefault ? html`<span class="badge-default">Default</span>` : ''}
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

customElements.define('ecommerce-tax-tree', TaxTree);

export default TaxTree;
