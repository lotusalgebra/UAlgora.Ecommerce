import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Discount Save Action
 * Saves discount data to the server.
 */
export class DiscountSaveAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: inline-block;
    }
  `;

  static properties = {
    _saving: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._saving = false;
  }

  async _handleSave() {
    const workspace = this.closest('ecommerce-discount-workspace');
    if (!workspace) return;

    const discount = workspace.getDiscount();
    const isNew = workspace.isNewDiscount();

    // Validate required fields
    if (!discount.name) {
      this._showNotification('warning', 'Please enter a discount name');
      return;
    }

    if ((discount.type === 'Percentage' || discount.type === 'FixedAmount') && !discount.value) {
      this._showNotification('warning', 'Please enter a discount value');
      return;
    }

    if (discount.type === 'Percentage' && (discount.value < 0 || discount.value > 100)) {
      this._showNotification('warning', 'Percentage must be between 0 and 100');
      return;
    }

    if (discount.type === 'BuyXGetY' && (!discount.buyQuantity || !discount.getQuantity)) {
      this._showNotification('warning', 'Please configure Buy X Get Y quantities');
      return;
    }

    if (discount.startDate && discount.endDate && new Date(discount.startDate) >= new Date(discount.endDate)) {
      this._showNotification('warning', 'End date must be after start date');
      return;
    }

    try {
      this._saving = true;

      const url = isNew
        ? '/umbraco/management/api/v1/ecommerce/discount'
        : `/umbraco/management/api/v1/ecommerce/discount/${discount.id}`;

      const method = isNew ? 'POST' : 'PUT';

      const response = await fetch(url, {
        method,
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({
          name: discount.name,
          description: discount.description,
          code: discount.code || null,
          type: discount.type,
          scope: discount.scope,
          value: discount.value,
          maxDiscountAmount: discount.maxDiscountAmount,
          minimumOrderAmount: discount.minimumOrderAmount,
          minimumQuantity: discount.minimumQuantity,
          maximumQuantity: discount.maximumQuantity,
          applicableProductIds: discount.applicableProductIds || [],
          applicableCategoryIds: discount.applicableCategoryIds || [],
          eligibleCustomerIds: discount.eligibleCustomerIds || [],
          eligibleCustomerTiers: discount.eligibleCustomerTiers || [],
          excludedProductIds: discount.excludedProductIds || [],
          excludedCategoryIds: discount.excludedCategoryIds || [],
          excludeSaleItems: discount.excludeSaleItems,
          firstTimeCustomerOnly: discount.firstTimeCustomerOnly,
          buyQuantity: discount.buyQuantity,
          getQuantity: discount.getQuantity,
          getProductIds: discount.getProductIds || [],
          totalUsageLimit: discount.totalUsageLimit,
          perCustomerLimit: discount.perCustomerLimit,
          isActive: discount.isActive,
          startDate: discount.startDate,
          endDate: discount.endDate,
          canCombine: discount.canCombine,
          priority: discount.priority || 0
        })
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || 'Failed to save discount');
      }

      const savedDiscount = await response.json();
      workspace.setDiscount(savedDiscount);

      this._showNotification('positive', isNew ? 'Discount created successfully' : 'Discount saved successfully');

      // If new discount, update URL to edit mode
      if (isNew && savedDiscount.id) {
        window.history.replaceState(null, '', `/umbraco/section/ecommerce/workspace/discount/edit/${savedDiscount.id}`);
      }
    } catch (error) {
      console.error('Error saving discount:', error);
      this._showNotification('danger', error.message || 'Failed to save discount');
    } finally {
      this._saving = false;
    }
  }

  _showNotification(color, message) {
    const headlines = {
      positive: 'Success',
      warning: 'Warning',
      danger: 'Error'
    };

    const event = new CustomEvent('umb-notification', {
      bubbles: true,
      composed: true,
      detail: {
        headline: headlines[color] || 'Notice',
        message,
        color
      }
    });
    this.dispatchEvent(event);
  }

  render() {
    return html`
      <uui-button
        look="primary"
        color="positive"
        ?disabled=${this._saving}
        @click=${this._handleSave}
      >
        ${this._saving ? html`<uui-loader-circle></uui-loader-circle>` : ''}
        Save
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-discount-save-action', DiscountSaveAction);

export default DiscountSaveAction;
