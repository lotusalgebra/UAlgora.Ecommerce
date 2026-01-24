import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Discount Workspace
 * Main container for discount editing with tabs for different views.
 */
export class DiscountWorkspace extends UmbElementMixin(LitElement) {
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

    .discount-icon {
      width: 64px;
      height: 64px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-alt);
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 28px;
    }

    .discount-icon.active {
      background: var(--uui-color-positive-emphasis);
      color: var(--uui-color-positive);
    }

    .discount-icon.scheduled {
      background: var(--uui-color-warning-emphasis);
      color: var(--uui-color-warning);
    }

    .discount-icon.expired {
      background: var(--uui-color-danger-emphasis);
      color: var(--uui-color-danger);
    }

    .discount-icon.inactive {
      background: var(--uui-color-surface-alt);
      color: var(--uui-color-text-alt);
    }

    .discount-info {
      flex: 1;
    }

    .discount-name {
      font-size: var(--uui-type-h3-size);
      font-weight: bold;
      margin: 0 0 var(--uui-size-space-2) 0;
    }

    .discount-code {
      font-family: monospace;
      background: var(--uui-color-surface-alt);
      padding: var(--uui-size-space-1) var(--uui-size-space-3);
      border-radius: var(--uui-border-radius);
      color: var(--uui-color-interactive);
      display: inline-block;
      margin-bottom: var(--uui-size-space-2);
    }

    .discount-meta {
      display: flex;
      gap: var(--uui-size-layout-1);
      flex-wrap: wrap;
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

    .meta-badge.status-active {
      background: var(--uui-color-positive-emphasis);
      color: var(--uui-color-positive);
    }

    .meta-badge.status-scheduled {
      background: var(--uui-color-warning-emphasis);
      color: var(--uui-color-warning);
    }

    .meta-badge.status-expired {
      background: var(--uui-color-danger-emphasis);
      color: var(--uui-color-danger);
    }

    .meta-badge.status-inactive {
      background: var(--uui-color-surface-alt);
      color: var(--uui-color-text-alt);
    }

    .discount-value {
      font-size: var(--uui-type-h4-size);
      font-weight: bold;
      color: var(--uui-color-positive);
    }

    .workspace-content {
      height: calc(100% - 120px);
      overflow: auto;
    }
  `;

  static properties = {
    _discount: { type: Object, state: true },
    _loading: { type: Boolean, state: true },
    _isNew: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._discount = null;
    this._loading = true;
    this._isNew = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadDiscount();
  }

  async _loadDiscount() {
    const pathParts = window.location.pathname.split('/');
    const editIndex = pathParts.indexOf('edit');

    if (editIndex !== -1 && pathParts[editIndex + 1]) {
      const discountId = pathParts[editIndex + 1];
      await this._fetchDiscount(discountId);
    } else {
      this._isNew = true;
      this._discount = {
        name: '',
        description: '',
        code: '',
        type: 'Percentage',
        scope: 'Order',
        value: 0,
        maxDiscountAmount: null,
        minimumOrderAmount: null,
        minimumQuantity: null,
        maximumQuantity: null,
        applicableProductIds: [],
        applicableCategoryIds: [],
        eligibleCustomerIds: [],
        eligibleCustomerTiers: [],
        excludedProductIds: [],
        excludedCategoryIds: [],
        excludeSaleItems: false,
        firstTimeCustomerOnly: false,
        buyQuantity: null,
        getQuantity: null,
        getProductIds: [],
        totalUsageLimit: null,
        perCustomerLimit: null,
        usageCount: 0,
        isActive: true,
        startDate: null,
        endDate: null,
        canCombine: false,
        priority: 0
      };
      this._loading = false;
    }
  }

  async _fetchDiscount(id) {
    try {
      this._loading = true;
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/discount/${id}`, {
        headers: {
          'Accept': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error('Failed to load discount');
      }

      this._discount = await response.json();
    } catch (error) {
      console.error('Error loading discount:', error);
    } finally {
      this._loading = false;
    }
  }

  getDiscount() {
    return this._discount;
  }

  setDiscount(discount) {
    this._discount = { ...discount };
    this.requestUpdate();
  }

  isNewDiscount() {
    return this._isNew;
  }

  _getStatusClass() {
    if (!this._discount?.isActive) return 'inactive';
    const now = new Date();
    const startDate = this._discount.startDate ? new Date(this._discount.startDate) : null;
    const endDate = this._discount.endDate ? new Date(this._discount.endDate) : null;

    if (startDate && startDate > now) return 'scheduled';
    if (endDate && endDate <= now) return 'expired';
    return 'active';
  }

  _getStatusLabel() {
    const statusClass = this._getStatusClass();
    return statusClass.charAt(0).toUpperCase() + statusClass.slice(1);
  }

  _getDisplayValue() {
    if (!this._discount) return '';
    switch (this._discount.type) {
      case 'Percentage':
        return `${this._discount.value}% off`;
      case 'FixedAmount':
        return `$${this._discount.value.toFixed(2)} off`;
      case 'FreeShipping':
        return 'Free Shipping';
      case 'BuyXGetY':
        return `Buy ${this._discount.buyQuantity || 0} Get ${this._discount.getQuantity || 0}`;
      default:
        return this._discount.value.toString();
    }
  }

  render() {
    if (this._loading) {
      return html`
        <div style="display: flex; justify-content: center; padding: var(--uui-size-layout-3);">
          <uui-loader></uui-loader>
        </div>
      `;
    }

    const statusClass = this._getStatusClass();

    return html`
      <div class="workspace-header">
        <div class="discount-icon ${statusClass}">
          <uui-icon name="icon-tag"></uui-icon>
        </div>
        <div class="discount-info">
          <h2 class="discount-name">
            ${this._isNew ? 'New Discount' : this._discount?.name || 'Untitled Discount'}
          </h2>
          ${this._discount?.code ? html`
            <span class="discount-code">${this._discount.code}</span>
          ` : ''}
          ${!this._isNew ? html`
            <div class="discount-meta">
              <span class="meta-badge status-${statusClass}">
                ${this._getStatusLabel()}
              </span>
              <span class="meta-badge">
                <uui-icon name="icon-tag"></uui-icon>
                ${this._discount?.type || 'Percentage'}
              </span>
              <span class="discount-value">
                ${this._getDisplayValue()}
              </span>
              ${this._discount?.usageCount > 0 ? html`
                <span class="meta-badge">
                  <uui-icon name="icon-users"></uui-icon>
                  ${this._discount.usageCount} uses
                </span>
              ` : ''}
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

customElements.define('ecommerce-discount-workspace', DiscountWorkspace);

export default DiscountWorkspace;
