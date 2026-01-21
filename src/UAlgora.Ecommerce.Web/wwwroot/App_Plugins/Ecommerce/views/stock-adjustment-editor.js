import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Stock Adjustment Editor
 * Form for editing stock adjustment details.
 */
export class StockAdjustmentEditor extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .editor-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: var(--uui-size-layout-1);
    }

    uui-box {
      margin-bottom: var(--uui-size-layout-1);
    }

    .form-group {
      margin-bottom: var(--uui-size-space-5);
    }

    .form-group label {
      display: block;
      margin-bottom: var(--uui-size-space-2);
      font-weight: 500;
    }

    uui-input, uui-textarea, uui-select {
      width: 100%;
    }

    .help-text {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      margin-top: var(--uui-size-space-1);
    }
  `;

  static properties = {
    _adjustment: { type: Object, state: true },
    _warehouses: { type: Array, state: true }
  };

  constructor() {
    super();
    this._adjustment = {};
    this._warehouses = [];
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadWarehouses();

    const checkWorkspace = () => {
      const workspace = this.closest('ecommerce-stock-adjustment-workspace');
      if (workspace) {
        this._adjustment = workspace.getAdjustment();
        this.requestUpdate();
      } else {
        setTimeout(checkWorkspace, 100);
      }
    };
    checkWorkspace();
  }

  async _loadWarehouses() {
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/inventory/warehouses', {
        headers: { 'Accept': 'application/json' }
      });
      if (response.ok) {
        this._warehouses = await response.json();
      }
    } catch (error) {
      console.error('Error loading warehouses:', error);
    }
  }

  _updateAdjustment(field, value) {
    this._adjustment = { ...this._adjustment, [field]: value };
    const workspace = this.closest('ecommerce-stock-adjustment-workspace');
    if (workspace) {
      workspace.setAdjustment(this._adjustment);
    }
  }

  render() {
    return html`
      <div class="editor-grid">
        <uui-box headline="Adjustment Details">
          <div class="form-group">
            <label>Warehouse *</label>
            <uui-select
              .value=${this._adjustment.warehouseId || ''}
              @change=${(e) => this._updateAdjustment('warehouseId', e.target.value)}
            >
              <option value="">Select a warehouse...</option>
              ${this._warehouses.map(w => html`
                <option value="${w.id}" ?selected=${w.id === this._adjustment.warehouseId}>${w.name}</option>
              `)}
            </uui-select>
          </div>

          <div class="form-group">
            <label>Adjustment Type *</label>
            <uui-select
              .value=${this._adjustment.type || 'InventoryCount'}
              @change=${(e) => this._updateAdjustment('type', e.target.value)}
            >
              <option value="InventoryCount">Inventory Count</option>
              <option value="Damage">Damage</option>
              <option value="Theft">Theft</option>
              <option value="Expired">Expired</option>
              <option value="CustomerReturn">Customer Return</option>
              <option value="SupplierReturn">Supplier Return</option>
              <option value="WriteOff">Write Off</option>
              <option value="Found">Found</option>
              <option value="Correction">Correction</option>
              <option value="Other">Other</option>
            </uui-select>
          </div>

          <div class="form-group">
            <label>Adjustment Date</label>
            <uui-input
              type="date"
              .value=${this._adjustment.adjustmentDate?.split('T')[0] || ''}
              @input=${(e) => this._updateAdjustment('adjustmentDate', e.target.value)}
            ></uui-input>
          </div>

          <div class="form-group">
            <label>External Reference</label>
            <uui-input
              .value=${this._adjustment.externalReference || ''}
              @input=${(e) => this._updateAdjustment('externalReference', e.target.value)}
              placeholder="e.g., Insurance claim number"
            ></uui-input>
            <p class="help-text">External reference for tracking (optional)</p>
          </div>
        </uui-box>

        <uui-box headline="Notes">
          <div class="form-group">
            <label>Reason / Notes</label>
            <uui-textarea
              .value=${this._adjustment.notes || ''}
              @input=${(e) => this._updateAdjustment('notes', e.target.value)}
              placeholder="Explain the reason for this adjustment"
              rows="6"
            ></uui-textarea>
            <p class="help-text">Provide details about why this adjustment is being made</p>
          </div>
        </uui-box>
      </div>
    `;
  }
}

customElements.define('ecommerce-stock-adjustment-editor', StockAdjustmentEditor);

export default StockAdjustmentEditor;
