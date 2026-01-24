import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Customer Save Action
 * Saves customer data to the server.
 */
export class CustomerSaveAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: inline-block;
    }
  `;

  static properties = {
    _saving: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._saving = false;
  }

  async _handleSave() {
    const workspace = this.closest('ecommerce-customer-workspace');
    if (!workspace) return;

    const customer = workspace.getCustomer();
    const isNew = workspace.isNewCustomer();

    // Validate required fields
    if (!customer.email || !customer.firstName || !customer.lastName) {
      this._showNotification('warning', 'Please fill in all required fields (Email, First Name, Last Name)');
      return;
    }

    // Validate email format
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(customer.email)) {
      this._showNotification('warning', 'Please enter a valid email address');
      return;
    }

    try {
      this._saving = true;

      const url = isNew
        ? '/umbraco/management/api/v1/ecommerce/customer'
        : `/umbraco/management/api/v1/ecommerce/customer/${customer.id}`;

      const method = isNew ? 'POST' : 'PUT';

      const response = await fetch(url, {
        method,
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({
          email: customer.email,
          firstName: customer.firstName,
          lastName: customer.lastName,
          phone: customer.phone,
          company: customer.company,
          notes: customer.notes,
          tags: customer.tags || [],
          status: customer.status,
          acceptsMarketing: customer.acceptsMarketing,
          addresses: customer.addresses || []
        })
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || 'Failed to save customer');
      }

      const savedCustomer = await response.json();
      workspace.setCustomer(savedCustomer);

      this._showNotification('positive', isNew ? 'Customer created successfully' : 'Customer saved successfully');

      // If new customer, update URL to edit mode
      if (isNew && savedCustomer.id) {
        window.history.replaceState(null, '', `/umbraco/section/ecommerce/workspace/customer/edit/${savedCustomer.id}`);
      }
    } catch (error) {
      console.error('Error saving customer:', error);
      this._showNotification('danger', error.message || 'Failed to save customer');
    } finally {
      this._saving = false;
    }
  }

  _showNotification(color, message) {
    const headlines = {
      positive: 'Success',
      warning: 'Warning',
      danger: 'Error'
    };

    const event = new CustomEvent('umb-notification', {
      bubbles: true,
      composed: true,
      detail: {
        headline: headlines[color] || 'Notice',
        message,
        color
      }
    });
    this.dispatchEvent(event);
  }

  render() {
    return html`
      <uui-button
        look="primary"
        color="positive"
        ?disabled=${this._saving}
        @click=${this._handleSave}
      >
        ${this._saving ? html`<uui-loader-circle></uui-loader-circle>` : ''}
        Save
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-customer-save-action', CustomerSaveAction);

export default CustomerSaveAction;
