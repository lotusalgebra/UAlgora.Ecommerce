import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Customer Addresses View
 * Manages customer shipping and billing addresses.
 */
export class CustomerAddresses extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .addresses-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--uui-size-layout-1);
    }

    .addresses-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
      gap: var(--uui-size-layout-1);
    }

    .address-card {
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-1);
    }

    .address-card.default {
      border-color: var(--uui-color-positive);
      border-width: 2px;
    }

    .address-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: var(--uui-size-space-4);
    }

    .address-type {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
    }

    .address-badges {
      display: flex;
      gap: var(--uui-size-space-2);
    }

    .badge {
      padding: var(--uui-size-space-1) var(--uui-size-space-2);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
      font-weight: 500;
    }

    .badge-shipping {
      background: #17a2b8;
      color: #fff;
    }

    .badge-billing {
      background: #6f42c1;
      color: #fff;
    }

    .badge-default {
      background: var(--uui-color-positive);
      color: #fff;
    }

    .address-actions {
      display: flex;
      gap: var(--uui-size-space-2);
    }

    .address-name {
      font-weight: bold;
      margin-bottom: var(--uui-size-space-2);
    }

    .address-line {
      color: var(--uui-color-text-alt);
      margin-bottom: var(--uui-size-space-1);
    }

    .address-phone {
      margin-top: var(--uui-size-space-3);
      color: var(--uui-color-text-alt);
    }

    .empty-state {
      text-align: center;
      padding: var(--uui-size-layout-3);
      color: var(--uui-color-text-alt);
    }

    .empty-state uui-icon {
      font-size: 48px;
      margin-bottom: var(--uui-size-space-4);
    }

    /* Address Form Modal */
    .address-form-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
    }

    .address-form {
      background: var(--uui-color-surface);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-2);
      width: 100%;
      max-width: 600px;
      max-height: 90vh;
      overflow-y: auto;
    }

    .form-title {
      font-size: var(--uui-type-h4-size);
      font-weight: bold;
      margin-bottom: var(--uui-size-layout-1);
    }

    .form-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: var(--uui-size-space-4);
    }

    .form-group {
      margin-bottom: var(--uui-size-space-4);
    }

    .form-group.full-width {
      grid-column: 1 / -1;
    }

    .form-group label {
      display: block;
      margin-bottom: var(--uui-size-space-2);
      font-weight: 500;
    }

    .form-group uui-input {
      width: 100%;
    }

    .form-actions {
      display: flex;
      justify-content: flex-end;
      gap: var(--uui-size-space-3);
      margin-top: var(--uui-size-layout-1);
      padding-top: var(--uui-size-layout-1);
      border-top: 1px solid var(--uui-color-border);
    }

    .checkbox-row {
      display: flex;
      gap: var(--uui-size-layout-1);
      margin-top: var(--uui-size-space-4);
    }

    .checkbox-group {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
    }
  `;

  static properties = {
    _customer: { type: Object, state: true },
    _addresses: { type: Array, state: true },
    _showForm: { type: Boolean, state: true },
    _editingIndex: { type: Number, state: true },
    _formData: { type: Object, state: true }
  };

  constructor() {
    super();
    this._customer = null;
    this._addresses = [];
    this._showForm = false;
    this._editingIndex = -1;
    this._formData = this._getEmptyForm();
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = this.closest('ecommerce-customer-workspace');
    if (workspace) {
      this._customer = workspace.getCustomer();
      this._addresses = this._customer?.addresses || [];
    }
  }

  _getEmptyForm() {
    return {
      firstName: '',
      lastName: '',
      company: '',
      addressLine1: '',
      addressLine2: '',
      city: '',
      stateProvince: '',
      postalCode: '',
      country: '',
      phone: '',
      isDefaultShipping: false,
      isDefaultBilling: false
    };
  }

  _handleAddAddress() {
    this._formData = this._getEmptyForm();
    this._editingIndex = -1;
    this._showForm = true;
  }

  _handleEditAddress(index) {
    this._formData = { ...this._addresses[index] };
    this._editingIndex = index;
    this._showForm = true;
  }

  _handleDeleteAddress(index) {
    if (confirm('Are you sure you want to delete this address?')) {
      const newAddresses = [...this._addresses];
      newAddresses.splice(index, 1);
      this._updateAddresses(newAddresses);
    }
  }

  _handleFormInput(field, value) {
    this._formData = {
      ...this._formData,
      [field]: value
    };
  }

  _handleSaveAddress() {
    if (!this._formData.firstName || !this._formData.addressLine1 || !this._formData.city || !this._formData.postalCode || !this._formData.country) {
      alert('Please fill in all required fields');
      return;
    }

    let newAddresses = [...this._addresses];

    if (this._editingIndex >= 0) {
      newAddresses[this._editingIndex] = { ...this._formData };
    } else {
      newAddresses.push({ ...this._formData });
    }

    // Handle default flags
    if (this._formData.isDefaultShipping) {
      newAddresses = newAddresses.map((addr, i) => ({
        ...addr,
        isDefaultShipping: i === (this._editingIndex >= 0 ? this._editingIndex : newAddresses.length - 1)
      }));
    }

    if (this._formData.isDefaultBilling) {
      newAddresses = newAddresses.map((addr, i) => ({
        ...addr,
        isDefaultBilling: i === (this._editingIndex >= 0 ? this._editingIndex : newAddresses.length - 1)
      }));
    }

    this._updateAddresses(newAddresses);
    this._showForm = false;
  }

  _updateAddresses(addresses) {
    this._addresses = addresses;
    const workspace = this.closest('ecommerce-customer-workspace');
    if (workspace) {
      const customer = workspace.getCustomer();
      workspace.setCustomer({
        ...customer,
        addresses: addresses
      });
    }
  }

  _handleCancelForm() {
    this._showForm = false;
    this._formData = this._getEmptyForm();
    this._editingIndex = -1;
  }

  _formatAddress(address) {
    const parts = [];
    if (address.addressLine1) parts.push(address.addressLine1);
    if (address.addressLine2) parts.push(address.addressLine2);
    const cityLine = [address.city, address.stateProvince, address.postalCode].filter(Boolean).join(', ');
    if (cityLine) parts.push(cityLine);
    if (address.country) parts.push(address.country);
    return parts;
  }

  render() {
    return html`
      <div class="addresses-header">
        <h3>Customer Addresses</h3>
        <uui-button
          look="primary"
          @click=${this._handleAddAddress}
        >
          <uui-icon name="icon-add"></uui-icon>
          Add Address
        </uui-button>
      </div>

      ${this._addresses.length === 0 ? html`
        <div class="empty-state">
          <uui-icon name="icon-home"></uui-icon>
          <h3>No addresses</h3>
          <p>This customer doesn't have any addresses yet.</p>
        </div>
      ` : html`
        <div class="addresses-grid">
          ${this._addresses.map((address, index) => html`
            <div class="address-card ${address.isDefaultShipping || address.isDefaultBilling ? 'default' : ''}">
              <div class="address-header">
                <div class="address-badges">
                  ${address.isDefaultShipping ? html`<span class="badge badge-shipping">Shipping</span>` : ''}
                  ${address.isDefaultBilling ? html`<span class="badge badge-billing">Billing</span>` : ''}
                  ${address.isDefaultShipping || address.isDefaultBilling ? html`<span class="badge badge-default">Default</span>` : ''}
                </div>
                <div class="address-actions">
                  <uui-button
                    look="secondary"
                    compact
                    @click=${() => this._handleEditAddress(index)}
                  >
                    <uui-icon name="icon-edit"></uui-icon>
                  </uui-button>
                  <uui-button
                    look="secondary"
                    compact
                    @click=${() => this._handleDeleteAddress(index)}
                  >
                    <uui-icon name="icon-delete"></uui-icon>
                  </uui-button>
                </div>
              </div>
              <div class="address-name">
                ${address.firstName} ${address.lastName}
                ${address.company ? html`<br><small>${address.company}</small>` : ''}
              </div>
              ${this._formatAddress(address).map(line => html`
                <div class="address-line">${line}</div>
              `)}
              ${address.phone ? html`
                <div class="address-phone">
                  <uui-icon name="icon-phone"></uui-icon>
                  ${address.phone}
                </div>
              ` : ''}
            </div>
          `)}
        </div>
      `}

      ${this._showForm ? html`
        <div class="address-form-overlay" @click=${(e) => e.target === e.currentTarget && this._handleCancelForm()}>
          <div class="address-form">
            <h3 class="form-title">${this._editingIndex >= 0 ? 'Edit Address' : 'Add New Address'}</h3>
            <div class="form-grid">
              <div class="form-group">
                <label>First Name *</label>
                <uui-input
                  .value=${this._formData.firstName}
                  @input=${(e) => this._handleFormInput('firstName', e.target.value)}
                ></uui-input>
              </div>
              <div class="form-group">
                <label>Last Name *</label>
                <uui-input
                  .value=${this._formData.lastName}
                  @input=${(e) => this._handleFormInput('lastName', e.target.value)}
                ></uui-input>
              </div>
              <div class="form-group full-width">
                <label>Company</label>
                <uui-input
                  .value=${this._formData.company}
                  @input=${(e) => this._handleFormInput('company', e.target.value)}
                ></uui-input>
              </div>
              <div class="form-group full-width">
                <label>Address Line 1 *</label>
                <uui-input
                  .value=${this._formData.addressLine1}
                  @input=${(e) => this._handleFormInput('addressLine1', e.target.value)}
                ></uui-input>
              </div>
              <div class="form-group full-width">
                <label>Address Line 2</label>
                <uui-input
                  .value=${this._formData.addressLine2}
                  @input=${(e) => this._handleFormInput('addressLine2', e.target.value)}
                ></uui-input>
              </div>
              <div class="form-group">
                <label>City *</label>
                <uui-input
                  .value=${this._formData.city}
                  @input=${(e) => this._handleFormInput('city', e.target.value)}
                ></uui-input>
              </div>
              <div class="form-group">
                <label>State/Province</label>
                <uui-input
                  .value=${this._formData.stateProvince}
                  @input=${(e) => this._handleFormInput('stateProvince', e.target.value)}
                ></uui-input>
              </div>
              <div class="form-group">
                <label>Postal Code *</label>
                <uui-input
                  .value=${this._formData.postalCode}
                  @input=${(e) => this._handleFormInput('postalCode', e.target.value)}
                ></uui-input>
              </div>
              <div class="form-group">
                <label>Country *</label>
                <uui-input
                  .value=${this._formData.country}
                  @input=${(e) => this._handleFormInput('country', e.target.value)}
                ></uui-input>
              </div>
              <div class="form-group full-width">
                <label>Phone</label>
                <uui-input
                  type="tel"
                  .value=${this._formData.phone}
                  @input=${(e) => this._handleFormInput('phone', e.target.value)}
                ></uui-input>
              </div>
              <div class="form-group full-width">
                <div class="checkbox-row">
                  <label class="checkbox-group">
                    <uui-checkbox
                      ?checked=${this._formData.isDefaultShipping}
                      @change=${(e) => this._handleFormInput('isDefaultShipping', e.target.checked)}
                    ></uui-checkbox>
                    Default shipping address
                  </label>
                  <label class="checkbox-group">
                    <uui-checkbox
                      ?checked=${this._formData.isDefaultBilling}
                      @change=${(e) => this._handleFormInput('isDefaultBilling', e.target.checked)}
                    ></uui-checkbox>
                    Default billing address
                  </label>
                </div>
              </div>
            </div>
            <div class="form-actions">
              <uui-button look="secondary" @click=${this._handleCancelForm}>
                Cancel
              </uui-button>
              <uui-button look="primary" @click=${this._handleSaveAddress}>
                ${this._editingIndex >= 0 ? 'Update Address' : 'Add Address'}
              </uui-button>
            </div>
          </div>
        </div>
      ` : ''}
    `;
  }
}

customElements.define('ecommerce-customer-addresses', CustomerAddresses);

export default CustomerAddresses;
