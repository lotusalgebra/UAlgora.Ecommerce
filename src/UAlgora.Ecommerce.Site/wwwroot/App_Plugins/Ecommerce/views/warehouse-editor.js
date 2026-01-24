import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Warehouse Editor
 * Form for editing warehouse basic settings.
 */
export class WarehouseEditor extends UmbElementMixin(LitElement) {
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

    .checkbox-group {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
    }

    .help-text {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      margin-top: var(--uui-size-space-1);
    }

    .address-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: var(--uui-size-space-4);
    }
  `;

  static properties = {
    _warehouse: { type: Object, state: true }
  };

  constructor() {
    super();
    this._warehouse = {};
  }

  connectedCallback() {
    super.connectedCallback();

    const checkWorkspace = () => {
      const workspace = this.closest('ecommerce-warehouse-workspace');
      if (workspace) {
        this._warehouse = workspace.getWarehouse();
        this.requestUpdate();
      } else {
        setTimeout(checkWorkspace, 100);
      }
    };
    checkWorkspace();
  }

  _updateWarehouse(field, value) {
    this._warehouse = { ...this._warehouse, [field]: value };
    const workspace = this.closest('ecommerce-warehouse-workspace');
    if (workspace) {
      workspace.setWarehouse(this._warehouse);
    }
  }

  render() {
    return html`
      <div class="editor-grid">
        <uui-box headline="Basic Information">
          <div class="form-group">
            <label>Name *</label>
            <uui-input
              .value=${this._warehouse.name || ''}
              @input=${(e) => this._updateWarehouse('name', e.target.value)}
              placeholder="e.g., Main Warehouse"
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Code *</label>
            <uui-input
              .value=${this._warehouse.code || ''}
              @input=${(e) => this._updateWarehouse('code', e.target.value)}
              placeholder="e.g., WH-MAIN"
            ></uui-input>
            <p class="help-text">Unique identifier for the warehouse</p>
          </div>

          <div class="form-group">
            <label>Type</label>
            <uui-select
              .value=${this._warehouse.type || 'Warehouse'}
              @change=${(e) => this._updateWarehouse('type', e.target.value)}
            >
              <option value="Warehouse">Warehouse</option>
              <option value="Store">Retail Store</option>
              <option value="DistributionCenter">Distribution Center</option>
              <option value="DropShip">Drop Ship</option>
              <option value="Virtual">Virtual</option>
            </uui-select>
          </div>

          <div class="form-group">
            <label>Description</label>
            <uui-textarea
              .value=${this._warehouse.description || ''}
              @input=${(e) => this._updateWarehouse('description', e.target.value)}
              placeholder="Internal description"
            ></uui-textarea>
          </div>
        </uui-box>

        <uui-box headline="Settings">
          <div class="form-group">
            <label>Priority</label>
            <uui-input
              type="number"
              .value=${this._warehouse.priority || 0}
              @input=${(e) => this._updateWarehouse('priority', parseInt(e.target.value) || 0)}
            ></uui-input>
            <p class="help-text">Higher priority warehouses are selected first for fulfillment</p>
          </div>

          <div class="form-group">
            <div class="checkbox-group">
              <uui-checkbox
                ?checked=${this._warehouse.isActive}
                @change=${(e) => this._updateWarehouse('isActive', e.target.checked)}
              ></uui-checkbox>
              <label>Active</label>
            </div>
          </div>

          <div class="form-group">
            <div class="checkbox-group">
              <uui-checkbox
                ?checked=${this._warehouse.isDefault}
                @change=${(e) => this._updateWarehouse('isDefault', e.target.checked)}
              ></uui-checkbox>
              <label>Default Warehouse</label>
            </div>
            <p class="help-text">Default warehouse for new stock entries</p>
          </div>

          <div class="form-group">
            <div class="checkbox-group">
              <uui-checkbox
                ?checked=${this._warehouse.canFulfillOrders}
                @change=${(e) => this._updateWarehouse('canFulfillOrders', e.target.checked)}
              ></uui-checkbox>
              <label>Can Fulfill Orders</label>
            </div>
          </div>

          <div class="form-group">
            <div class="checkbox-group">
              <uui-checkbox
                ?checked=${this._warehouse.acceptsReturns}
                @change=${(e) => this._updateWarehouse('acceptsReturns', e.target.checked)}
              ></uui-checkbox>
              <label>Accepts Returns</label>
            </div>
          </div>
        </uui-box>

        <uui-box headline="Address">
          <div class="form-group">
            <label>Address Line 1</label>
            <uui-input
              .value=${this._warehouse.addressLine1 || ''}
              @input=${(e) => this._updateWarehouse('addressLine1', e.target.value)}
              placeholder="Street address"
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Address Line 2</label>
            <uui-input
              .value=${this._warehouse.addressLine2 || ''}
              @input=${(e) => this._updateWarehouse('addressLine2', e.target.value)}
              placeholder="Suite, unit, etc."
            ></uui-input>
          </div>

          <div class="address-grid">
            <div class="form-group">
              <label>City</label>
              <uui-input
                .value=${this._warehouse.city || ''}
                @input=${(e) => this._updateWarehouse('city', e.target.value)}
              ></uui-input>
            </div>

            <div class="form-group">
              <label>State/Province</label>
              <uui-input
                .value=${this._warehouse.state || ''}
                @input=${(e) => this._updateWarehouse('state', e.target.value)}
              ></uui-input>
            </div>

            <div class="form-group">
              <label>Postal Code</label>
              <uui-input
                .value=${this._warehouse.postalCode || ''}
                @input=${(e) => this._updateWarehouse('postalCode', e.target.value)}
              ></uui-input>
            </div>

            <div class="form-group">
              <label>Country Code</label>
              <uui-input
                .value=${this._warehouse.country || ''}
                @input=${(e) => this._updateWarehouse('country', e.target.value)}
                placeholder="e.g., US, GB"
                maxlength="2"
              ></uui-input>
            </div>
          </div>
        </uui-box>

        <uui-box headline="Contact Information">
          <div class="form-group">
            <label>Contact Name</label>
            <uui-input
              .value=${this._warehouse.contactName || ''}
              @input=${(e) => this._updateWarehouse('contactName', e.target.value)}
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Contact Email</label>
            <uui-input
              type="email"
              .value=${this._warehouse.contactEmail || ''}
              @input=${(e) => this._updateWarehouse('contactEmail', e.target.value)}
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Contact Phone</label>
            <uui-input
              .value=${this._warehouse.contactPhone || ''}
              @input=${(e) => this._updateWarehouse('contactPhone', e.target.value)}
            ></uui-input>
          </div>
        </uui-box>
      </div>
    `;
  }
}

customElements.define('ecommerce-warehouse-editor', WarehouseEditor);

export default WarehouseEditor;
