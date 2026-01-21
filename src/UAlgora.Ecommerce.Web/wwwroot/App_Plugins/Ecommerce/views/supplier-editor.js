import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Supplier Editor
 * Form for editing supplier basic settings.
 */
export class SupplierEditor extends UmbElementMixin(LitElement) {
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
    _supplier: { type: Object, state: true }
  };

  constructor() {
    super();
    this._supplier = {};
  }

  connectedCallback() {
    super.connectedCallback();

    const checkWorkspace = () => {
      const workspace = this.closest('ecommerce-supplier-workspace');
      if (workspace) {
        this._supplier = workspace.getSupplier();
        this.requestUpdate();
      } else {
        setTimeout(checkWorkspace, 100);
      }
    };
    checkWorkspace();
  }

  _updateSupplier(field, value) {
    this._supplier = { ...this._supplier, [field]: value };
    const workspace = this.closest('ecommerce-supplier-workspace');
    if (workspace) {
      workspace.setSupplier(this._supplier);
    }
  }

  render() {
    return html`
      <div class="editor-grid">
        <uui-box headline="Basic Information">
          <div class="form-group">
            <label>Name *</label>
            <uui-input
              .value=${this._supplier.name || ''}
              @input=${(e) => this._updateSupplier('name', e.target.value)}
              placeholder="e.g., ABC Manufacturing"
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Code *</label>
            <uui-input
              .value=${this._supplier.code || ''}
              @input=${(e) => this._updateSupplier('code', e.target.value)}
              placeholder="e.g., SUP-ABC"
            ></uui-input>
            <p class="help-text">Unique identifier for the supplier</p>
          </div>

          <div class="form-group">
            <label>Type</label>
            <uui-select
              .value=${this._supplier.type || 'Manufacturer'}
              @change=${(e) => this._updateSupplier('type', e.target.value)}
            >
              <option value="Manufacturer">Manufacturer</option>
              <option value="Distributor">Distributor</option>
              <option value="Wholesaler">Wholesaler</option>
              <option value="DropShipper">Drop Shipper</option>
              <option value="Importer">Importer</option>
              <option value="Other">Other</option>
            </uui-select>
          </div>

          <div class="form-group">
            <label>Description</label>
            <uui-textarea
              .value=${this._supplier.description || ''}
              @input=${(e) => this._updateSupplier('description', e.target.value)}
              placeholder="Internal description"
            ></uui-textarea>
          </div>

          <div class="form-group">
            <div class="checkbox-group">
              <uui-checkbox
                ?checked=${this._supplier.isActive}
                @change=${(e) => this._updateSupplier('isActive', e.target.checked)}
              ></uui-checkbox>
              <label>Active</label>
            </div>
          </div>

          <div class="form-group">
            <div class="checkbox-group">
              <uui-checkbox
                ?checked=${this._supplier.isPreferred}
                @change=${(e) => this._updateSupplier('isPreferred', e.target.checked)}
              ></uui-checkbox>
              <label>Preferred Supplier</label>
            </div>
            <p class="help-text">Preferred supplier for products</p>
          </div>
        </uui-box>

        <uui-box headline="Contact Information">
          <div class="form-group">
            <label>Contact Name</label>
            <uui-input
              .value=${this._supplier.contactName || ''}
              @input=${(e) => this._updateSupplier('contactName', e.target.value)}
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Email</label>
            <uui-input
              type="email"
              .value=${this._supplier.email || ''}
              @input=${(e) => this._updateSupplier('email', e.target.value)}
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Phone</label>
            <uui-input
              .value=${this._supplier.phone || ''}
              @input=${(e) => this._updateSupplier('phone', e.target.value)}
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Website</label>
            <uui-input
              type="url"
              .value=${this._supplier.website || ''}
              @input=${(e) => this._updateSupplier('website', e.target.value)}
              placeholder="https://"
            ></uui-input>
          </div>
        </uui-box>

        <uui-box headline="Address">
          <div class="form-group">
            <label>Address Line 1</label>
            <uui-input
              .value=${this._supplier.addressLine1 || ''}
              @input=${(e) => this._updateSupplier('addressLine1', e.target.value)}
              placeholder="Street address"
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Address Line 2</label>
            <uui-input
              .value=${this._supplier.addressLine2 || ''}
              @input=${(e) => this._updateSupplier('addressLine2', e.target.value)}
              placeholder="Suite, unit, etc."
            ></uui-input>
          </div>

          <div class="address-grid">
            <div class="form-group">
              <label>City</label>
              <uui-input
                .value=${this._supplier.city || ''}
                @input=${(e) => this._updateSupplier('city', e.target.value)}
              ></uui-input>
            </div>

            <div class="form-group">
              <label>State/Province</label>
              <uui-input
                .value=${this._supplier.state || ''}
                @input=${(e) => this._updateSupplier('state', e.target.value)}
              ></uui-input>
            </div>

            <div class="form-group">
              <label>Postal Code</label>
              <uui-input
                .value=${this._supplier.postalCode || ''}
                @input=${(e) => this._updateSupplier('postalCode', e.target.value)}
              ></uui-input>
            </div>

            <div class="form-group">
              <label>Country Code</label>
              <uui-input
                .value=${this._supplier.country || ''}
                @input=${(e) => this._updateSupplier('country', e.target.value)}
                placeholder="e.g., US, GB"
                maxlength="2"
              ></uui-input>
            </div>
          </div>
        </uui-box>

        <uui-box headline="Business Details">
          <div class="form-group">
            <label>Tax ID</label>
            <uui-input
              .value=${this._supplier.taxId || ''}
              @input=${(e) => this._updateSupplier('taxId', e.target.value)}
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Payment Terms</label>
            <uui-input
              .value=${this._supplier.paymentTerms || ''}
              @input=${(e) => this._updateSupplier('paymentTerms', e.target.value)}
              placeholder="e.g., Net 30"
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Currency</label>
            <uui-select
              .value=${this._supplier.currencyCode || 'USD'}
              @change=${(e) => this._updateSupplier('currencyCode', e.target.value)}
            >
              <option value="USD">USD - US Dollar</option>
              <option value="EUR">EUR - Euro</option>
              <option value="GBP">GBP - British Pound</option>
              <option value="CAD">CAD - Canadian Dollar</option>
              <option value="AUD">AUD - Australian Dollar</option>
            </uui-select>
          </div>

          <div class="form-group">
            <label>Lead Time (Days)</label>
            <uui-input
              type="number"
              .value=${this._supplier.leadTimeDays || ''}
              @input=${(e) => this._updateSupplier('leadTimeDays', parseInt(e.target.value) || null)}
            ></uui-input>
            <p class="help-text">Average days to receive an order</p>
          </div>

          <div class="form-group">
            <label>Minimum Order Quantity</label>
            <uui-input
              type="number"
              .value=${this._supplier.minimumOrderQuantity || ''}
              @input=${(e) => this._updateSupplier('minimumOrderQuantity', parseInt(e.target.value) || null)}
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Minimum Order Value</label>
            <uui-input
              type="number"
              step="0.01"
              .value=${this._supplier.minOrderValue || ''}
              @input=${(e) => this._updateSupplier('minOrderValue', parseFloat(e.target.value) || null)}
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Notes</label>
            <uui-textarea
              .value=${this._supplier.notes || ''}
              @input=${(e) => this._updateSupplier('notes', e.target.value)}
              placeholder="Internal notes about this supplier"
            ></uui-textarea>
          </div>
        </uui-box>
      </div>
    `;
  }
}

customElements.define('ecommerce-supplier-editor', SupplierEditor);

export default SupplierEditor;
