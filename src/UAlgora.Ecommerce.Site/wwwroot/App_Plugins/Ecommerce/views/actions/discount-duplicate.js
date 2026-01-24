import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Discount Duplicate Action
 * Quick action to create a copy of the current discount.
 */
export class DiscountDuplicateAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
  }

  async _handleDuplicate() {
    const workspace = document.querySelector('ecommerce-discount-workspace');
    if (!workspace) return;

    const discount = workspace.getDiscount();
    if (!discount?.id) {
      alert('Please save the discount first before duplicating');
      return;
    }

    const confirmed = confirm(`Are you sure you want to duplicate "${discount.name}"?`);
    if (!confirmed) return;

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/discount/${discount.id}/duplicate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const duplicatedDiscount = await response.json();
        alert(`Discount duplicated successfully! New discount: "${duplicatedDiscount.name}"`);

        // Navigate to the duplicated discount
        window.location.href = `/umbraco/section/ecommerce/workspace/discount/edit/${duplicatedDiscount.id}`;
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to duplicate discount');
      }
    } catch (error) {
      console.error('Error duplicating discount:', error);
      alert('Failed to duplicate discount');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        ?disabled=${this._processing}
        @click=${this._handleDuplicate}
      >
        <uui-icon name="icon-documents"></uui-icon>
        ${this._processing ? 'Duplicating...' : 'Duplicate'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-discount-duplicate-action', DiscountDuplicateAction);

export default DiscountDuplicateAction;
