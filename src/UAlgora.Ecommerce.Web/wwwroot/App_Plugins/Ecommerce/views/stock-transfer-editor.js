import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Stock Transfer Editor
 * Form for editing stock transfer details.
 */
export class StockTransferEditor extends UmbElementMixin(LitElement) {
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

    .transfer-visualization {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: var(--uui-size-space-4);
      padding: var(--uui-size-layout-1);
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
      margin-bottom: var(--uui-size-space-5);
    }

    .warehouse-box {
      flex: 1;
      text-align: center;
      padding: var(--uui-size-space-4);
      background: var(--uui-color-surface);
      border-radius: var(--uui-border-radius);
      border: 2px solid var(--uui-color-border);
    }

    .warehouse-box.source {
      border-color: var(--uui-color-warning);
    }

    .warehouse-box.destination {
      border-color: var(--uui-color-positive);
    }

    .warehouse-label {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      margin-bottom: var(--uui-size-space-2);
    }

    .warehouse-name {
      font-weight: 500;
    }

    .transfer-arrow {
      font-size: 24px;
      color: var(--uui-color-text-alt);
    }
  `;

  static properties = {
    _transfer: { type: Object, state: true },
    _warehouses: { type: Array, state: true }
  };

  constructor() {
    super();
    this._transfer = {};
    this._warehouses = [];
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadWarehouses();

    const checkWorkspace = () => {
      const workspace = this.closest('ecommerce-stock-transfer-workspace');
      if (workspace) {
        this._transfer = workspace.getTransfer();
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

  _updateTransfer(field, value) {
    this._transfer = { ...this._transfer, [field]: value };
    const workspace = this.closest('ecommerce-stock-transfer-workspace');
    if (workspace) {
      workspace.setTransfer(this._transfer);
    }
  }

  _getWarehouseName(id) {
    const warehouse = this._warehouses.find(w => w.id === id);
    return warehouse?.name || 'Select...';
  }

  render() {
    const sourceWarehouse = this._getWarehouseName(this._transfer.sourceWarehouseId);
    const destWarehouse = this._getWarehouseName(this._transfer.destinationWarehouseId);

    return html`
      <div class="editor-grid">
        <uui-box headline="Transfer Details">
          <div class="transfer-visualization">
            <div class="warehouse-box source">
              <div class="warehouse-label">From</div>
              <div class="warehouse-name">${sourceWarehouse}</div>
            </div>
            <div class="transfer-arrow">→</div>
            <div class="warehouse-box destination">
              <div class="warehouse-label">To</div>
              <div class="warehouse-name">${destWarehouse}</div>
            </div>
          </div>

          <div class="form-group">
            <label>Source Warehouse *</label>
            <uui-select
              .value=${this._transfer.sourceWarehouseId || ''}
              @change=${(e) => this._updateTransfer('sourceWarehouseId', e.target.value)}
            >
              <option value="">Select source warehouse...</option>
              ${this._warehouses.map(w => html`
                <option
                  value="${w.id}"
                  ?selected=${w.id === this._transfer.sourceWarehouseId}
                  ?disabled=${w.id === this._transfer.destinationWarehouseId}
                >${w.name}</option>
              `)}
            </uui-select>
          </div>

          <div class="form-group">
            <label>Destination Warehouse *</label>
            <uui-select
              .value=${this._transfer.destinationWarehouseId || ''}
              @change=${(e) => this._updateTransfer('destinationWarehouseId', e.target.value)}
            >
              <option value="">Select destination warehouse...</option>
              ${this._warehouses.map(w => html`
                <option
                  value="${w.id}"
                  ?selected=${w.id === this._transfer.destinationWarehouseId}
                  ?disabled=${w.id === this._transfer.sourceWarehouseId}
                >${w.name}</option>
              `)}
            </uui-select>
          </div>

          <div class="form-group">
            <label>Requested Date</label>
            <uui-input
              type="date"
              .value=${this._transfer.requestedDate?.split('T')[0] || ''}
              @input=${(e) => this._updateTransfer('requestedDate', e.target.value)}
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Expected Arrival Date</label>
            <uui-input
              type="date"
              .value=${this._transfer.expectedArrivalDate?.split('T')[0] || ''}
              @input=${(e) => this._updateTransfer('expectedArrivalDate', e.target.value)}
            ></uui-input>
          </div>
        </uui-box>

        <uui-box headline="Notes">
          <div class="form-group">
            <label>Notes</label>
            <uui-textarea
              .value=${this._transfer.notes || ''}
              @input=${(e) => this._updateTransfer('notes', e.target.value)}
              placeholder="Additional notes about this transfer"
              rows="6"
            ></uui-textarea>
          </div>
        </uui-box>
      </div>
    `;
  }
}

customElements.define('ecommerce-stock-transfer-editor', StockTransferEditor);

export default StockTransferEditor;
