import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

export class SettingsMenuItem extends UmbElementMixin(LitElement) {
  static styles = css`
    :host { display: block; }
    .menu-section { margin-top: var(--uui-size-space-6); padding-top: var(--uui-size-space-4); border-top: 1px solid var(--uui-color-border); }
    .menu-header {
      display: flex;
      align-items: center;
      padding: var(--uui-size-space-3) var(--uui-size-space-4);
      cursor: pointer;
      font-weight: 600;
      color: var(--uui-color-text);
    }
    .menu-header:hover { background: var(--uui-color-surface-emphasis); border-radius: var(--uui-border-radius); }
    .menu-header uui-icon { margin-right: var(--uui-size-space-3); }
    .submenu { margin-left: var(--uui-size-space-4); }
    .submenu-item {
      display: flex;
      align-items: center;
      padding: var(--uui-size-space-2) var(--uui-size-space-3);
      cursor: pointer;
      border-radius: var(--uui-border-radius);
      margin-bottom: var(--uui-size-space-1);
      text-decoration: none;
      color: var(--uui-color-text);
    }
    .submenu-item:hover { background: var(--uui-color-surface-emphasis); }
    .submenu-item uui-icon { margin-right: var(--uui-size-space-2); color: var(--uui-color-text-alt); }
  `;

  static properties = {
    _expanded: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._expanded = false;
  }

  render() {
    return html`
      <div class="menu-section">
        <div class="menu-header" @click=${() => this._expanded = !this._expanded}>
          <uui-icon name="${this._expanded ? 'icon-navigation-down' : 'icon-navigation-right'}"></uui-icon>
          <uui-icon name="icon-settings"></uui-icon>
          <span>Settings</span>
        </div>
        ${this._expanded ? html`
          <div class="submenu">
            <a class="submenu-item" href="/umbraco/section/ecommerce/settings/general">
              <uui-icon name="icon-wrench"></uui-icon>
              <span>General</span>
            </a>
            <a class="submenu-item" href="/umbraco/section/ecommerce/settings/payment">
              <uui-icon name="icon-coins"></uui-icon>
              <span>Payment Methods</span>
            </a>
            <a class="submenu-item" href="/umbraco/section/ecommerce/settings/shipping">
              <uui-icon name="icon-truck"></uui-icon>
              <span>Shipping</span>
            </a>
            <a class="submenu-item" href="/umbraco/section/ecommerce/settings/tax">
              <uui-icon name="icon-bill"></uui-icon>
              <span>Tax Settings</span>
            </a>
            <a class="submenu-item" href="/umbraco/section/ecommerce/settings/email">
              <uui-icon name="icon-message"></uui-icon>
              <span>Email Templates</span>
            </a>
          </div>
        ` : ''}
      </div>
    `;
  }
}

customElements.define('ecommerce-settings-menu-item', SettingsMenuItem);
export default SettingsMenuItem;
