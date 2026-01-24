import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Customer Editor View
 * Form for editing customer details.
 */
export class CustomerEditor extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .form-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: var(--uui-size-layout-1);
    }

    .form-group {
      margin-bottom: var(--uui-size-layout-1);
    }

    .form-group.full-width {
      grid-column: 1 / -1;
    }

    .form-group label {
      display: block;
      margin-bottom: var(--uui-size-space-2);
      font-weight: 500;
    }

    .form-group uui-input,
    .form-group uui-textarea,
    .form-group uui-select {
      width: 100%;
    }

    .section-title {
      font-size: var(--uui-type-h4-size);
      font-weight: bold;
      margin: var(--uui-size-layout-2) 0 var(--uui-size-layout-1) 0;
      padding-bottom: var(--uui-size-space-3);
      border-bottom: 1px solid var(--uui-color-border);
    }

    .section-title:first-child {
      margin-top: 0;
    }

    .tags-container {
      display: flex;
      flex-wrap: wrap;
      gap: var(--uui-size-space-2);
      margin-bottom: var(--uui-size-space-2);
    }

    .tag {
      display: inline-flex;
      align-items: center;
      gap: var(--uui-size-space-2);
      padding: var(--uui-size-space-1) var(--uui-size-space-3);
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
    }

    .tag uui-icon {
      cursor: pointer;
      opacity: 0.6;
    }

    .tag uui-icon:hover {
      opacity: 1;
    }

    .tag-input-group {
      display: flex;
      gap: var(--uui-size-space-2);
    }

    .tag-input-group uui-input {
      flex: 1;
    }

    .checkbox-group {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-3);
    }

    .status-options {
      display: flex;
      gap: var(--uui-size-space-4);
    }

    .status-option {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
      cursor: pointer;
    }

    .status-dot {
      width: 10px;
      height: 10px;
      border-radius: 50%;
    }

    .status-dot.active { background: var(--uui-color-positive); }
    .status-dot.inactive { background: var(--uui-color-danger); }
    .status-dot.suspended { background: var(--uui-color-warning); }
  `;

  static properties = {
    _customer: { type: Object, state: true },
    _newTag: { type: String, state: true }
  };

  constructor() {
    super();
    this._customer = null;
    this._newTag = '';
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = this.closest('ecommerce-customer-workspace');
    if (workspace) {
      this._customer = workspace.getCustomer();
    }
  }

  _handleInputChange(field, value) {
    if (!this._customer) return;

    this._customer = {
      ...this._customer,
      [field]: value
    };

    const workspace = this.closest('ecommerce-customer-workspace');
    if (workspace) {
      workspace.setCustomer(this._customer);
    }
  }

  _handleAddTag() {
    if (!this._newTag.trim() || !this._customer) return;

    const tags = [...(this._customer.tags || []), this._newTag.trim()];
    this._handleInputChange('tags', tags);
    this._newTag = '';
  }

  _handleRemoveTag(index) {
    if (!this._customer) return;

    const tags = [...(this._customer.tags || [])];
    tags.splice(index, 1);
    this._handleInputChange('tags', tags);
  }

  _handleTagKeyPress(e) {
    if (e.key === 'Enter') {
      e.preventDefault();
      this._handleAddTag();
    }
  }

  render() {
    if (!this._customer) {
      return html`<uui-loader></uui-loader>`;
    }

    return html`
      <uui-box>
        <h3 class="section-title">Basic Information</h3>
        <div class="form-grid">
          <div class="form-group">
            <label>First Name *</label>
            <uui-input
              .value=${this._customer.firstName || ''}
              @input=${(e) => this._handleInputChange('firstName', e.target.value)}
              placeholder="Enter first name"
            ></uui-input>
          </div>
          <div class="form-group">
            <label>Last Name *</label>
            <uui-input
              .value=${this._customer.lastName || ''}
              @input=${(e) => this._handleInputChange('lastName', e.target.value)}
              placeholder="Enter last name"
            ></uui-input>
          </div>
          <div class="form-group">
            <label>Email *</label>
            <uui-input
              type="email"
              .value=${this._customer.email || ''}
              @input=${(e) => this._handleInputChange('email', e.target.value)}
              placeholder="Enter email address"
            ></uui-input>
          </div>
          <div class="form-group">
            <label>Phone</label>
            <uui-input
              type="tel"
              .value=${this._customer.phone || ''}
              @input=${(e) => this._handleInputChange('phone', e.target.value)}
              placeholder="Enter phone number"
            ></uui-input>
          </div>
          <div class="form-group full-width">
            <label>Company</label>
            <uui-input
              .value=${this._customer.company || ''}
              @input=${(e) => this._handleInputChange('company', e.target.value)}
              placeholder="Enter company name"
            ></uui-input>
          </div>
        </div>

        <h3 class="section-title">Status</h3>
        <div class="form-group">
          <div class="status-options">
            <label class="status-option" @click=${() => this._handleInputChange('status', 'Active')}>
              <input
                type="radio"
                name="status"
                value="Active"
                ?checked=${this._customer.status === 'Active'}
              />
              <span class="status-dot active"></span>
              Active
            </label>
            <label class="status-option" @click=${() => this._handleInputChange('status', 'Inactive')}>
              <input
                type="radio"
                name="status"
                value="Inactive"
                ?checked=${this._customer.status === 'Inactive'}
              />
              <span class="status-dot inactive"></span>
              Inactive
            </label>
            <label class="status-option" @click=${() => this._handleInputChange('status', 'Suspended')}>
              <input
                type="radio"
                name="status"
                value="Suspended"
                ?checked=${this._customer.status === 'Suspended'}
              />
              <span class="status-dot suspended"></span>
              Suspended
            </label>
          </div>
        </div>

        <h3 class="section-title">Marketing</h3>
        <div class="form-group">
          <label class="checkbox-group">
            <uui-checkbox
              ?checked=${this._customer.acceptsMarketing}
              @change=${(e) => this._handleInputChange('acceptsMarketing', e.target.checked)}
            ></uui-checkbox>
            Accepts marketing emails
          </label>
        </div>

        <h3 class="section-title">Tags</h3>
        <div class="form-group">
          <div class="tags-container">
            ${(this._customer.tags || []).map((tag, index) => html`
              <span class="tag">
                ${tag}
                <uui-icon
                  name="icon-delete"
                  @click=${() => this._handleRemoveTag(index)}
                ></uui-icon>
              </span>
            `)}
          </div>
          <div class="tag-input-group">
            <uui-input
              .value=${this._newTag}
              @input=${(e) => this._newTag = e.target.value}
              @keypress=${this._handleTagKeyPress}
              placeholder="Add a tag"
            ></uui-input>
            <uui-button
              look="secondary"
              @click=${this._handleAddTag}
            >
              Add
            </uui-button>
          </div>
        </div>

        <h3 class="section-title">Notes</h3>
        <div class="form-group full-width">
          <uui-textarea
            .value=${this._customer.notes || ''}
            @input=${(e) => this._handleInputChange('notes', e.target.value)}
            placeholder="Add internal notes about this customer..."
            rows="4"
          ></uui-textarea>
        </div>
      </uui-box>
    `;
  }
}

customElements.define('ecommerce-customer-editor', CustomerEditor);

export default CustomerEditor;
