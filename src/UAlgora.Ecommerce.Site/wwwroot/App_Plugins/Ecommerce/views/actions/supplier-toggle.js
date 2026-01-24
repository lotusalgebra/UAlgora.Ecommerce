import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Supplier Toggle Action
 * Quick action to activate/deactivate a supplier.
 */
export class SupplierToggleAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _isActive: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._isActive = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-supplier-workspace');
    if (workspace) {
      const supplier = workspace.getSupplier();
      this._isActive = supplier?.isActive ?? true;
    }
  }

  async _handleToggle() {
    const workspace = document.querySelector('ecommerce-supplier-workspace');
    if (!workspace) return;

    const supplier = workspace.getSupplier();
    if (!supplier?.id) {
      alert('Please save the supplier first');
      return;
    }

    this._processing = true;

    try {
      // Get current supplier data
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/inventory/supplier/${supplier.id}`, {
        headers: { 'Accept': 'application/json' }
      });

      if (!response.ok) {
        throw new Error('Failed to load supplier');
      }

      const currentSupplier = await response.json();
      currentSupplier.isActive = !currentSupplier.isActive;

      // Update supplier
      const updateResponse = await fetch(`/umbraco/management/api/v1/ecommerce/inventory/supplier/${supplier.id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(currentSupplier)
      });

      if (updateResponse.ok) {
        const result = await updateResponse.json();
        workspace.setSupplier(result);
        this._isActive = result.isActive;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: result.isActive ? 'Supplier activated' : 'Supplier deactivated',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await updateResponse.json();
        alert(error.message || 'Failed to update supplier');
      }
    } catch (error) {
      console.error('Error toggling supplier:', error);
      alert('Failed to toggle supplier');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._isActive ? 'warning' : 'positive'}"
        ?disabled=${this._processing}
        @click=${this._handleToggle}
      >
        <uui-icon name="${this._isActive ? 'icon-block' : 'icon-check'}"></uui-icon>
        ${this._processing ? 'Processing...' : this._isActive ? 'Deactivate' : 'Activate'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-supplier-toggle-action', SupplierToggleAction);

export default SupplierToggleAction;
