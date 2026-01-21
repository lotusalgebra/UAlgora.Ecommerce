import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Order Customer View
 * Displays customer information and addresses.
 */
export class OrderCustomer extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .customer-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: var(--uui-size-layout-1);
    }

    .customer-card {
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-1);
    }

    .customer-card h3 {
      margin: 0 0 var(--uui-size-space-4) 0;
      font-size: var(--uui-type-default-size);
      font-weight: bold;
      color: var(--uui-color-text);
      border-bottom: 1px solid var(--uui-color-border);
      padding-bottom: var(--uui-size-space-2);
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
    }

    .customer-card h3 uui-icon {
      color: var(--uui-color-text-alt);
    }

    .info-row {
      display: flex;
      align-items: flex-start;
      gap: var(--uui-size-space-2);
      padding: var(--uui-size-space-2) 0;
    }

    .info-row uui-icon {
      color: var(--uui-color-text-alt);
      margin-top: 2px;
    }

    .info-label {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
    }

    .info-value {
      color: var(--uui-color-text);
    }

    .address-block {
      line-height: 1.6;
    }

    .address-name {
      font-weight: 500;
    }

    .address-company {
      color: var(--uui-color-text-alt);
    }

    .customer-link {
      color: var(--uui-color-interactive);
      cursor: pointer;
      text-decoration: none;
    }

    .customer-link:hover {
      text-decoration: underline;
    }

    .empty-address {
      color: var(--uui-color-text-alt);
      font-style: italic;
    }
  `;

  static properties = {
    _order: { type: Object, state: true }
  };

  constructor() {
    super();
    this._order = null;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = this.closest('ecommerce-order-workspace');
    if (workspace) {
      this._order = workspace.getOrder();
    }
  }

  _handleCustomerClick() {
    if (this._order?.customerId) {
      window.history.pushState(null, '', `/umbraco/section/ecommerce/workspace/customer/edit/${this._order.customerId}`);
    }
  }

  _renderAddress(address, title, icon) {
    if (!address) {
      return html`
        <div class="customer-card">
          <h3><uui-icon name="${icon}"></uui-icon> ${title}</h3>
          <p class="empty-address">No address provided</p>
        </div>
      `;
    }

    return html`
      <div class="customer-card">
        <h3><uui-icon name="${icon}"></uui-icon> ${title}</h3>
        <div class="address-block">
          <div class="address-name">${address.firstName} ${address.lastName}</div>
          ${address.company ? html`<div class="address-company">${address.company}</div>` : ''}
          <div>${address.addressLine1}</div>
          ${address.addressLine2 ? html`<div>${address.addressLine2}</div>` : ''}
          <div>${address.city}${address.stateProvince ? `, ${address.stateProvince}` : ''} ${address.postalCode}</div>
          <div>${address.country}</div>
          ${address.phone ? html`
            <div class="info-row" style="margin-top: var(--uui-size-space-2);">
              <uui-icon name="icon-phone"></uui-icon>
              <span>${address.phone}</span>
            </div>
          ` : ''}
        </div>
      </div>
    `;
  }

  render() {
    if (!this._order) {
      return html`<uui-loader></uui-loader>`;
    }

    return html`
      <div class="customer-grid">
        <div class="customer-card">
          <h3><uui-icon name="icon-user"></uui-icon> Customer Information</h3>

          <div class="info-row">
            <uui-icon name="icon-user"></uui-icon>
            <div>
              <div class="info-label">Name</div>
              <div class="info-value">
                ${this._order.customerId ? html`
                  <span class="customer-link" @click=${this._handleCustomerClick}>
                    ${this._order.customerName || 'Guest'}
                  </span>
                ` : html`
                  ${this._order.customerName || 'Guest Customer'}
                `}
              </div>
            </div>
          </div>

          <div class="info-row">
            <uui-icon name="icon-message"></uui-icon>
            <div>
              <div class="info-label">Email</div>
              <div class="info-value">
                <a href="mailto:${this._order.customerEmail}" style="color: var(--uui-color-interactive);">
                  ${this._order.customerEmail || 'N/A'}
                </a>
              </div>
            </div>
          </div>

          ${this._order.customerId ? html`
            <div class="info-row">
              <uui-icon name="icon-keychain"></uui-icon>
              <div>
                <div class="info-label">Customer ID</div>
                <div class="info-value" style="font-family: monospace; font-size: var(--uui-type-small-size);">
                  ${this._order.customerId}
                </div>
              </div>
            </div>
          ` : html`
            <div class="info-row">
              <uui-icon name="icon-user"></uui-icon>
              <div>
                <div class="info-label">Account Type</div>
                <div class="info-value">Guest Checkout</div>
              </div>
            </div>
          `}
        </div>

        ${this._renderAddress(this._order.billingAddress, 'Billing Address', 'icon-invoice')}
      </div>

      <div class="customer-grid" style="margin-top: var(--uui-size-layout-1);">
        ${this._renderAddress(this._order.shippingAddress, 'Shipping Address', 'icon-truck')}

        <div class="customer-card">
          <h3><uui-icon name="icon-info"></uui-icon> Additional Information</h3>

          <div class="info-row">
            <uui-icon name="icon-calendar"></uui-icon>
            <div>
              <div class="info-label">Order Date</div>
              <div class="info-value">${new Date(this._order.createdAt).toLocaleString()}</div>
            </div>
          </div>

          ${this._order.updatedAt ? html`
            <div class="info-row">
              <uui-icon name="icon-calendar"></uui-icon>
              <div>
                <div class="info-label">Last Updated</div>
                <div class="info-value">${new Date(this._order.updatedAt).toLocaleString()}</div>
              </div>
            </div>
          ` : ''}

          ${this._order.customerNote ? html`
            <div class="info-row">
              <uui-icon name="icon-message"></uui-icon>
              <div>
                <div class="info-label">Customer Note</div>
                <div class="info-value" style="font-style: italic;">"${this._order.customerNote}"</div>
              </div>
            </div>
          ` : ''}
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-order-customer', OrderCustomer);

export default OrderCustomer;
