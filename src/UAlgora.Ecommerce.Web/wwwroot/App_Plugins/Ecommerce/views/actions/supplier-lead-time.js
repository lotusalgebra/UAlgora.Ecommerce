import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Supplier Lead Time Action
 * Quick action to update supplier lead time.
 */
export class SupplierLeadTimeAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .lead-badge {
      font-size: 11px;
      padding: 2px 6px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-emphasis);
      margin-left: 4px;
    }

    .fast {
      background: var(--uui-color-positive-emphasis);
      color: var(--uui-color-positive);
    }

    .slow {
      background: var(--uui-color-warning-emphasis);
      color: var(--uui-color-warning);
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _leadTimeDays: { type: Number, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._leadTimeDays = null;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-supplier-workspace');
    if (workspace) {
      const supplier = workspace.getSupplier();
      this._leadTimeDays = supplier?.leadTimeDays ?? null;
    }
  }

  async _handleLeadTime() {
    const workspace = document.querySelector('ecommerce-supplier-workspace');
    if (!workspace) return;

    const supplier = workspace.getSupplier();
    if (!supplier?.id) {
      alert('Please save the supplier first');
      return;
    }

    const input = prompt(
      `Current lead time: ${this._leadTimeDays ?? 'Not set'} days\n` +
      `Enter new lead time in days (average time from order to delivery):`,
      String(this._leadTimeDays ?? 7)
    );
    if (input === null) return;

    const newLeadTime = parseInt(input, 10);
    if (isNaN(newLeadTime) || newLeadTime < 0) {
      alert('Please enter a valid number of days (0 or greater)');
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/inventory/supplier/${supplier.id}/update-lead-time`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ leadTimeDays: newLeadTime })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setSupplier(result);
        this._leadTimeDays = result.leadTimeDays;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Lead time updated to ${newLeadTime} days`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to update lead time');
      }
    } catch (error) {
      console.error('Error updating lead time:', error);
      alert('Failed to update lead time');
    } finally {
      this._processing = false;
    }
  }

  render() {
    const isFast = this._leadTimeDays !== null && this._leadTimeDays <= 3;
    const isSlow = this._leadTimeDays !== null && this._leadTimeDays >= 14;
    const displayDays = this._leadTimeDays !== null ? `${this._leadTimeDays}d` : 'N/A';

    return html`
      <uui-button
        look="secondary"
        color="default"
        ?disabled=${this._processing}
        @click=${this._handleLeadTime}
      >
        <uui-icon name="icon-timer"></uui-icon>
        Lead Time
        <span class="lead-badge ${isFast ? 'fast' : isSlow ? 'slow' : ''}">${displayDays}</span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-supplier-lead-time-action', SupplierLeadTimeAction);

export default SupplierLeadTimeAction;
