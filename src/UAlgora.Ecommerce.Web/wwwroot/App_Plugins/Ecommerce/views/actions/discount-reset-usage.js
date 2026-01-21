import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Discount Reset Usage Action
 * Quick action to reset the usage count of a discount.
 */
export class DiscountResetUsageAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .usage-badge {
      font-size: 11px;
      padding: 2px 6px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-emphasis);
      margin-left: 4px;
    }

    .limit-reached {
      color: var(--uui-color-danger);
      background: var(--uui-color-danger-emphasis);
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _usageCount: { type: Number, state: true },
    _usageLimit: { type: Number, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._usageCount = 0;
    this._usageLimit = null;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-discount-workspace');
    if (workspace) {
      const discount = workspace.getDiscount();
      this._usageCount = discount?.usageCount ?? 0;
      this._usageLimit = discount?.totalUsageLimit;
    }
  }

  _isLimitReached() {
    return this._usageLimit && this._usageCount >= this._usageLimit;
  }

  _getUsageDisplay() {
    if (this._usageLimit) {
      return `${this._usageCount}/${this._usageLimit}`;
    }
    return `${this._usageCount}`;
  }

  async _handleReset() {
    const workspace = document.querySelector('ecommerce-discount-workspace');
    if (!workspace) return;

    const discount = workspace.getDiscount();
    if (!discount?.id) {
      alert('Please save the discount first');
      return;
    }

    if (this._usageCount === 0) {
      alert('Usage count is already zero');
      return;
    }

    if (!confirm(`Reset usage count from ${this._usageCount} to 0?\n\nThis action cannot be undone.`)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/discount/${discount.id}/reset-usage`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setDiscount(result);
        this._usageCount = result.usageCount;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'Usage count has been reset to 0',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to reset usage count');
      }
    } catch (error) {
      console.error('Error resetting usage count:', error);
      alert('Failed to reset usage count');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._isLimitReached() ? 'warning' : 'default'}"
        ?disabled=${this._processing || this._usageCount === 0}
        @click=${this._handleReset}
      >
        <uui-icon name="icon-undo"></uui-icon>
        ${this._processing ? 'Resetting...' : 'Reset Usage'}
        <span class="usage-badge ${this._isLimitReached() ? 'limit-reached' : ''}">${this._getUsageDisplay()}</span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-discount-reset-usage-action', DiscountResetUsageAction);

export default DiscountResetUsageAction;
