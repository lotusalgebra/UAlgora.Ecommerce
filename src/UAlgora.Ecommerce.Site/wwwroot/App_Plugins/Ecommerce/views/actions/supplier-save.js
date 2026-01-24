import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Supplier Save Action
 * Saves the supplier to the server.
 */
export class SupplierSaveAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _saving: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._saving = false;
  }

  async _save() {
    const workspace = document.querySelector('ecommerce-supplier-workspace');
    if (!workspace) return;

    const supplier = workspace.getSupplier();
    const isNew = workspace.isNewSupplier();

    if (!supplier.name || !supplier.code) {
      alert('Please fill in all required fields');
      return;
    }

    this._saving = true;

    try {
      const url = isNew
        ? '/umbraco/management/api/v1/ecommerce/inventory/supplier'
        : `/umbraco/management/api/v1/ecommerce/inventory/supplier/${supplier.id}`;

      const response = await fetch(url, {
        method: isNew ? 'POST' : 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(supplier)
      });

      if (response.ok) {
        const result = await response.json();
        if (isNew) {
          window.history.replaceState({}, '', `/umbraco/section/ecommerce/supplier/edit/${result.id}`);
        }
        workspace.setSupplier(result);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to save supplier');
      }
    } catch (error) {
      console.error('Error saving supplier:', error);
      alert('Failed to save supplier');
    } finally {
      this._saving = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="primary"
        color="positive"
        ?disabled=${this._saving}
        @click=${this._save}
      >
        ${this._saving ? 'Saving...' : 'Save'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-supplier-save-action', SupplierSaveAction);

export default SupplierSaveAction;
