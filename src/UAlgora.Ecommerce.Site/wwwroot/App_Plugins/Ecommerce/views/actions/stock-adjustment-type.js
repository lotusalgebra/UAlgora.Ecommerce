import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Stock Adjustment Type Action
 * Quick action to change adjustment type/reason.
 */
export class StockAdjustmentTypeAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .type-badge {
      font-size: 11px;
      padding: 2px 6px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-emphasis);
      margin-left: 4px;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _type: { type: String, state: true },
    _status: { type: String, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._type = 'ManualCorrection';
    this._status = 'Draft';
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-stock-adjustment-workspace');
    if (workspace) {
      const adjustment = workspace.getStockAdjustment();
      this._type = adjustment?.type ?? 'ManualCorrection';
      this._status = adjustment?.status ?? 'Draft';
    }
  }

  _getTypeLabel(type) {
    const labels = {
      'InventoryCount': 'Inventory Count',
      'Damage': 'Damage',
      'Theft': 'Theft/Loss',
      'Expired': 'Expired',
      'ReturnToVendor': 'Return to Vendor',
      'CustomerReturn': 'Customer Return',
      'InternalUse': 'Internal Use',
      'Found': 'Found Stock',
      'InitialStock': 'Initial Stock',
      'ManualCorrection': 'Manual Correction',
      'Other': 'Other'
    };
    return labels[type] || type;
  }

  async _handleType() {
    const workspace = document.querySelector('ecommerce-stock-adjustment-workspace');
    if (!workspace) return;

    const adjustment = workspace.getStockAdjustment();
    if (!adjustment?.id) {
      alert('Please save the stock adjustment first');
      return;
    }

    if (this._status !== 'Draft') {
      alert('Only draft adjustments can be modified');
      return;
    }

    const types = [
      'InventoryCount', 'Damage', 'Theft', 'Expired',
      'ReturnToVendor', 'CustomerReturn', 'InternalUse',
      'Found', 'InitialStock', 'ManualCorrection', 'Other'
    ];

    const currentIndex = types.indexOf(this._type);

    const typeList = types.map((t, i) => `${i} - ${this._getTypeLabel(t)}`).join('\n');
    const input = prompt(
      `Current type: ${this._getTypeLabel(this._type)}\n\nSelect adjustment type:\n${typeList}\n\nEnter number (0-${types.length - 1}):`,
      String(currentIndex >= 0 ? currentIndex : 9)
    );

    if (input === null) return;

    const typeIndex = parseInt(input, 10);
    if (isNaN(typeIndex) || typeIndex < 0 || typeIndex >= types.length) {
      alert(`Please enter a valid number (0-${types.length - 1})`);
      return;
    }

    const newType = types[typeIndex];

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/inventory/stock-adjustment/${adjustment.id}/update-type`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ type: newType })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setStockAdjustment(result);
        this._type = result.type;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Adjustment type updated to ${this._getTypeLabel(newType)}`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to update type');
      }
    } catch (error) {
      console.error('Error updating type:', error);
      alert('Failed to update type');
    } finally {
      this._processing = false;
    }
  }

  render() {
    const canEdit = this._status === 'Draft';

    return html`
      <uui-button
        look="secondary"
        color="default"
        ?disabled=${this._processing || !canEdit}
        @click=${this._handleType}
      >
        <uui-icon name="icon-tag"></uui-icon>
        Type
        <span class="type-badge">${this._getTypeLabel(this._type)}</span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-stock-adjustment-type-action', StockAdjustmentTypeAction);

export default StockAdjustmentTypeAction;
