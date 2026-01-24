import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Algora Sidebar Tree Element
 * Clean navigation tree for the Algora Commerce section with category groupings.
 */
export class EcommerceSidebarTree extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      height: 100%;
      overflow: auto;
    }

    .tree-root {
      padding: 12px;
    }

    .tree-header {
      font-weight: 600;
      font-size: 11px;
      color: var(--uui-color-text-alt);
      text-transform: uppercase;
      letter-spacing: 0.5px;
      padding: 12px 12px 6px;
      margin-top: 8px;
    }

    .tree-header:first-child {
      margin-top: 0;
    }

    .tree-item {
      display: flex;
      align-items: center;
      padding: 10px 12px;
      cursor: pointer;
      border-radius: 4px;
      margin-bottom: 2px;
      transition: background 0.15s ease;
      font-size: 14px;
      text-decoration: none;
      color: inherit;
    }

    .tree-item:hover {
      background: var(--uui-color-surface-emphasis);
    }

    .tree-item.active {
      background: var(--uui-color-selected);
      color: var(--uui-color-selected-contrast);
    }

    .tree-item uui-icon {
      margin-right: 10px;
      color: var(--uui-color-text-alt);
      font-size: 16px;
    }

    .tree-item.active uui-icon {
      color: var(--uui-color-selected-contrast);
    }

    .tree-item-label {
      flex: 1;
    }
  `;

  static properties = {
    _selectedItem: { type: String, state: true }
  };

  constructor() {
    super();
    this._selectedItem = 'dashboard';
    this._updateSelectedFromUrl();
  }

  connectedCallback() {
    super.connectedCallback();
    this._updateSelectedFromUrl();

    // Listen for popstate events (browser back/forward)
    window.addEventListener('popstate', () => this._updateSelectedFromUrl());

    // Check URL periodically as backup for SPA navigation
    this._interval = setInterval(() => this._updateSelectedFromUrl(), 500);
  }

  disconnectedCallback() {
    super.disconnectedCallback();
    window.removeEventListener('popstate', () => this._updateSelectedFromUrl());
    if (this._interval) clearInterval(this._interval);
  }

  _updateSelectedFromUrl() {
    const path = window.location.pathname;
    let newSelected = 'dashboard';

    if (path.includes('/products')) newSelected = 'products';
    else if (path.includes('/categories')) newSelected = 'categories';
    else if (path.includes('/brands')) newSelected = 'brands';
    else if (path.includes('/manufacturers')) newSelected = 'manufacturers';
    else if (path.includes('/orders')) newSelected = 'orders';
    else if (path.includes('/returns')) newSelected = 'returns';
    else if (path.includes('/paymentlinks')) newSelected = 'paymentlinks';
    else if (path.includes('/customers')) newSelected = 'customers';
    else if (path.includes('/giftcards')) newSelected = 'giftcards';
    else if (path.includes('/discounts')) newSelected = 'discounts';
    else if (path.includes('/emailtemplates')) newSelected = 'emailtemplates';
    else if (path.includes('/stores')) newSelected = 'stores';
    else if (path.includes('/currencies')) newSelected = 'currencies';
    else if (path.includes('/webhooks')) newSelected = 'webhooks';
    else if (path.includes('/license')) newSelected = 'license';
    else if (path.includes('/dashboard') || path.endsWith('/ecommerce')) newSelected = 'dashboard';

    if (newSelected !== this._selectedItem) {
      this._selectedItem = newSelected;
      this.requestUpdate();
    }
  }

  _navigateTo(item, pathname) {
    this._selectedItem = item;
    // Direct navigation to sectionView - Umbraco 15 format
    window.location.href = `/umbraco/section/ecommerce/${pathname}`;
  }

  render() {
    return html`
      <div class="tree-root">
        <!-- Dashboard -->
        <div class="tree-item ${this._selectedItem === 'dashboard' ? 'active' : ''}"
             @click=${() => this._navigateTo('dashboard', 'dashboard')}>
          <uui-icon name="icon-dashboard"></uui-icon>
          <span class="tree-item-label">Dashboard</span>
        </div>

        <!-- Catalog -->
        <div class="tree-header">Catalog</div>

        <div class="tree-item ${this._selectedItem === 'products' ? 'active' : ''}"
             @click=${() => this._navigateTo('products', 'products')}>
          <uui-icon name="icon-box"></uui-icon>
          <span class="tree-item-label">Products</span>
        </div>

        <div class="tree-item ${this._selectedItem === 'categories' ? 'active' : ''}"
             @click=${() => this._navigateTo('categories', 'categories')}>
          <uui-icon name="icon-folder"></uui-icon>
          <span class="tree-item-label">Categories</span>
        </div>

        <div class="tree-item ${this._selectedItem === 'brands' ? 'active' : ''}"
             @click=${() => this._navigateTo('brands', 'brands')}>
          <uui-icon name="icon-stamp"></uui-icon>
          <span class="tree-item-label">Brands</span>
        </div>

        <div class="tree-item ${this._selectedItem === 'manufacturers' ? 'active' : ''}"
             @click=${() => this._navigateTo('manufacturers', 'manufacturers')}>
          <uui-icon name="icon-factory"></uui-icon>
          <span class="tree-item-label">Manufacturers</span>
        </div>

        <!-- Sales -->
        <div class="tree-header">Sales</div>

        <div class="tree-item ${this._selectedItem === 'orders' ? 'active' : ''}"
             @click=${() => this._navigateTo('orders', 'orders')}>
          <uui-icon name="icon-receipt-dollar"></uui-icon>
          <span class="tree-item-label">Orders</span>
        </div>

        <div class="tree-item ${this._selectedItem === 'returns' ? 'active' : ''}"
             @click=${() => this._navigateTo('returns', 'returns')}>
          <uui-icon name="icon-undo"></uui-icon>
          <span class="tree-item-label">Returns</span>
        </div>

        <div class="tree-item ${this._selectedItem === 'paymentlinks' ? 'active' : ''}"
             @click=${() => this._navigateTo('paymentlinks', 'paymentlinks')}>
          <uui-icon name="icon-share-alt"></uui-icon>
          <span class="tree-item-label">Payment Links</span>
        </div>

        <!-- Customers -->
        <div class="tree-header">Customers</div>

        <div class="tree-item ${this._selectedItem === 'customers' ? 'active' : ''}"
             @click=${() => this._navigateTo('customers', 'customers')}>
          <uui-icon name="icon-users"></uui-icon>
          <span class="tree-item-label">Customers</span>
        </div>

        <div class="tree-item ${this._selectedItem === 'giftcards' ? 'active' : ''}"
             @click=${() => this._navigateTo('giftcards', 'giftcards')}>
          <uui-icon name="icon-gift"></uui-icon>
          <span class="tree-item-label">Gift Cards</span>
        </div>

        <!-- Marketing -->
        <div class="tree-header">Marketing</div>

        <div class="tree-item ${this._selectedItem === 'discounts' ? 'active' : ''}"
             @click=${() => this._navigateTo('discounts', 'discounts')}>
          <uui-icon name="icon-tag"></uui-icon>
          <span class="tree-item-label">Discounts</span>
        </div>

        <div class="tree-item ${this._selectedItem === 'emailtemplates' ? 'active' : ''}"
             @click=${() => this._navigateTo('emailtemplates', 'emailtemplates')}>
          <uui-icon name="icon-message"></uui-icon>
          <span class="tree-item-label">Email Templates</span>
        </div>

        <!-- Settings -->
        <div class="tree-header">Settings</div>

        <div class="tree-item ${this._selectedItem === 'stores' ? 'active' : ''}"
             @click=${() => this._navigateTo('stores', 'stores')}>
          <uui-icon name="icon-store"></uui-icon>
          <span class="tree-item-label">Stores</span>
        </div>

        <div class="tree-item ${this._selectedItem === 'currencies' ? 'active' : ''}"
             @click=${() => this._navigateTo('currencies', 'currencies')}>
          <uui-icon name="icon-coins-dollar-alt"></uui-icon>
          <span class="tree-item-label">Currencies</span>
        </div>

        <div class="tree-item ${this._selectedItem === 'webhooks' ? 'active' : ''}"
             @click=${() => this._navigateTo('webhooks', 'webhooks')}>
          <uui-icon name="icon-link"></uui-icon>
          <span class="tree-item-label">Webhooks</span>
        </div>

        <div class="tree-item ${this._selectedItem === 'license' ? 'active' : ''}"
             @click=${() => this._navigateTo('license', 'license')}>
          <uui-icon name="icon-key"></uui-icon>
          <span class="tree-item-label">License</span>
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-sidebar-tree', EcommerceSidebarTree);
export default EcommerceSidebarTree;
