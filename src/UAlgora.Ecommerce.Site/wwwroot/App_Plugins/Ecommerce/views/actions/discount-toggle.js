import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Discount Toggle Action
 * Quick action to toggle discount active/inactive status.
 */
export class DiscountToggleAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _isActive: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._isActive = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-discount-workspace');
    if (workspace) {
      const discount = workspace.getDiscount();
      this._isActive = discount?.isActive ?? true;
    }
  }

  async _handleToggle() {
    const workspace = document.querySelector('ecommerce-discount-workspace');
    if (!workspace) return;

    const discount = workspace.getDiscount();
    if (!discount?.id) {
      alert('Please save the discount first');
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/discount/${discount.id}/toggle-status`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setDiscount(result);
        this._isActive = result.isActive;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: result.isActive ? 'Discount activated' : 'Discount deactivated',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to toggle discount status');
      }
    } catch (error) {
      console.error('Error toggling discount:', error);
      alert('Failed to toggle discount status');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._isActive ? 'warning' : 'positive'}"
        ?disabled=${this._processing}
        @click=${this._handleToggle}
      >
        <uui-icon name="${this._isActive ? 'icon-block' : 'icon-check'}"></uui-icon>
        ${this._processing ? 'Processing...' : this._isActive ? 'Deactivate' : 'Activate'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-discount-toggle-action', DiscountToggleAction);

export default DiscountToggleAction;
