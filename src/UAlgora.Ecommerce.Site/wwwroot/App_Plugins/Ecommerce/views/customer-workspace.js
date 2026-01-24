import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Customer Workspace
 * Main container for customer editing with tabs for different views.
 */
export class CustomerWorkspace extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      height: 100%;
    }

    .workspace-header {
      display: flex;
      align-items: center;
      gap: var(--uui-size-layout-1);
      padding: var(--uui-size-layout-1);
      background: var(--uui-color-surface);
      border-bottom: 1px solid var(--uui-color-border);
    }

    .customer-avatar {
      width: 64px;
      height: 64px;
      border-radius: 50%;
      background: var(--uui-color-surface-alt);
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 24px;
      font-weight: bold;
      color: var(--uui-color-text-alt);
    }

    .customer-info {
      flex: 1;
    }

    .customer-name {
      font-size: var(--uui-type-h3-size);
      font-weight: bold;
      margin: 0 0 var(--uui-size-space-2) 0;
    }

    .customer-email {
      color: var(--uui-color-text-alt);
      margin: 0;
    }

    .customer-meta {
      display: flex;
      gap: var(--uui-size-layout-1);
      margin-top: var(--uui-size-space-2);
    }

    .meta-badge {
      display: inline-flex;
      align-items: center;
      gap: var(--uui-size-space-2);
      padding: var(--uui-size-space-1) var(--uui-size-space-3);
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
    }

    .meta-badge.tier-gold {
      background: #ffc107;
      color: #000;
    }

    .meta-badge.tier-silver {
      background: #c0c0c0;
      color: #000;
    }

    .meta-badge.tier-bronze {
      background: #cd7f32;
      color: #fff;
    }

    .status-active {
      background: var(--uui-color-positive-emphasis);
      color: var(--uui-color-positive);
    }

    .status-inactive {
      background: var(--uui-color-danger-emphasis);
      color: var(--uui-color-danger);
    }

    .workspace-content {
      height: calc(100% - 100px);
      overflow: auto;
    }
  `;

  static properties = {
    _customer: { type: Object, state: true },
    _loading: { type: Boolean, state: true },
    _isNew: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._customer = null;
    this._loading = true;
    this._isNew = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadCustomer();
  }

  async _loadCustomer() {
    const pathParts = window.location.pathname.split('/');
    const editIndex = pathParts.indexOf('edit');

    if (editIndex !== -1 && pathParts[editIndex + 1]) {
      const customerId = pathParts[editIndex + 1];
      await this._fetchCustomer(customerId);
    } else {
      this._isNew = true;
      this._customer = {
        email: '',
        firstName: '',
        lastName: '',
        phone: '',
        company: '',
        notes: '',
        tags: [],
        addresses: [],
        status: 'Active',
        loyaltyPoints: 0,
        storeCreditBalance: 0,
        customerTier: 'Standard'
      };
      this._loading = false;
    }
  }

  async _fetchCustomer(id) {
    try {
      this._loading = true;
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/customer/${id}`, {
        headers: {
          'Accept': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error('Failed to load customer');
      }

      this._customer = await response.json();
    } catch (error) {
      console.error('Error loading customer:', error);
    } finally {
      this._loading = false;
    }
  }

  getCustomer() {
    return this._customer;
  }

  setCustomer(customer) {
    this._customer = { ...customer };
    this.requestUpdate();
  }

  isNewCustomer() {
    return this._isNew;
  }

  _getInitials() {
    if (!this._customer) return '?';
    const first = this._customer.firstName?.charAt(0) || '';
    const last = this._customer.lastName?.charAt(0) || '';
    return (first + last).toUpperCase() || this._customer.email?.charAt(0)?.toUpperCase() || '?';
  }

  _getTierClass() {
    const tier = this._customer?.customerTier?.toLowerCase();
    if (tier === 'gold' || tier === 'vip') return 'tier-gold';
    if (tier === 'silver') return 'tier-silver';
    if (tier === 'bronze') return 'tier-bronze';
    return '';
  }

  render() {
    if (this._loading) {
      return html`
        <div style="display: flex; justify-content: center; padding: var(--uui-size-layout-3);">
          <uui-loader></uui-loader>
        </div>
      `;
    }

    return html`
      <div class="workspace-header">
        <div class="customer-avatar">
          ${this._getInitials()}
        </div>
        <div class="customer-info">
          <h2 class="customer-name">
            ${this._isNew ? 'New Customer' : this._customer?.fullName || 'Unknown Customer'}
          </h2>
          <p class="customer-email">${this._customer?.email || ''}</p>
          ${!this._isNew ? html`
            <div class="customer-meta">
              <span class="meta-badge status-${this._customer?.status?.toLowerCase() || 'active'}">
                ${this._customer?.status || 'Active'}
              </span>
              ${this._customer?.customerTier && this._customer.customerTier !== 'Standard' ? html`
                <span class="meta-badge ${this._getTierClass()}">
                  <uui-icon name="icon-medal"></uui-icon>
                  ${this._customer.customerTier}
                </span>
              ` : ''}
              <span class="meta-badge">
                <uui-icon name="icon-receipt-dollar"></uui-icon>
                ${this._customer?.totalOrders || 0} orders
              </span>
            </div>
          ` : ''}
        </div>
      </div>
      <div class="workspace-content">
        <slot></slot>
      </div>
    `;
  }
}

customElements.define('ecommerce-customer-workspace', CustomerWorkspace);

export default CustomerWorkspace;
