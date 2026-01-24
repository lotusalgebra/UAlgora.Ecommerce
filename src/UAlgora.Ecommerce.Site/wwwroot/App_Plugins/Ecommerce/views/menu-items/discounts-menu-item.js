import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

export class DiscountsMenuItem extends UmbElementMixin(LitElement) {
  static styles = css`
    :host { display: block; }
    .menu-item {
      display: flex;
      align-items: center;
      padding: var(--uui-size-space-3) var(--uui-size-space-4);
      cursor: pointer;
      border-radius: var(--uui-border-radius);
      text-decoration: none;
      color: var(--uui-color-text);
    }
    .menu-item:hover { background: var(--uui-color-surface-emphasis); }
    uui-icon { margin-right: var(--uui-size-space-3); }
  `;

  render() {
    return html`
      <a class="menu-item" href="/umbraco/section/ecommerce/collection/ecommerce-discount">
        <uui-icon name="icon-tag"></uui-icon>
        <span>Discounts</span>
      </a>
    `;
  }
}

customElements.define('ecommerce-discounts-menu-item', DiscountsMenuItem);
export default DiscountsMenuItem;
