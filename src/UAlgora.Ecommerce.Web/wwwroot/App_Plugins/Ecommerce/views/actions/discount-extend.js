import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Discount Extend Action
 * Quick action to extend the end date of a discount.
 */
export class DiscountExtendAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .date-badge {
      font-size: 11px;
      padding: 2px 6px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-emphasis);
      margin-left: 4px;
    }

    .expired {
      color: var(--uui-color-danger);
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _endDate: { type: String, state: true },
    _isExpired: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._endDate = null;
    this._isExpired = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-discount-workspace');
    if (workspace) {
      const discount = workspace.getDiscount();
      this._endDate = discount?.endDate;
      if (this._endDate) {
        this._isExpired = new Date(this._endDate) < new Date();
      }
    }
  }

  _formatDate(dateStr) {
    if (!dateStr) return 'No end date';
    const date = new Date(dateStr);
    return date.toLocaleDateString();
  }

  async _handleExtend() {
    const workspace = document.querySelector('ecommerce-discount-workspace');
    if (!workspace) return;

    const discount = workspace.getDiscount();
    if (!discount?.id) {
      alert('Please save the discount first');
      return;
    }

    // Calculate default new end date (30 days from now or 30 days from current end date)
    const baseDate = this._endDate ? new Date(this._endDate) : new Date();
    const defaultDate = new Date(baseDate);
    defaultDate.setDate(defaultDate.getDate() + 30);
    const defaultDateStr = defaultDate.toISOString().split('T')[0];

    const input = prompt(
      `Current end date: ${this._formatDate(this._endDate)}\n` +
      `Enter new end date (YYYY-MM-DD):`,
      defaultDateStr
    );
    if (input === null) return;

    const newEndDate = new Date(input);
    if (isNaN(newEndDate.getTime())) {
      alert('Please enter a valid date in YYYY-MM-DD format');
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/discount/${discount.id}/extend`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ newEndDate: newEndDate.toISOString() })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setDiscount(result);
        this._endDate = result.endDate;
        this._isExpired = new Date(this._endDate) < new Date();

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Discount extended to ${this._formatDate(this._endDate)}`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to extend discount');
      }
    } catch (error) {
      console.error('Error extending discount:', error);
      alert('Failed to extend discount');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._isExpired ? 'warning' : 'default'}"
        ?disabled=${this._processing}
        @click=${this._handleExtend}
      >
        <uui-icon name="icon-calendar"></uui-icon>
        ${this._processing ? 'Extending...' : 'Extend'}
        <span class="date-badge ${this._isExpired ? 'expired' : ''}">${this._formatDate(this._endDate)}</span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-discount-extend-action', DiscountExtendAction);

export default DiscountExtendAction;
