import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Supplier Duplicate Action
 * Quick action to create a copy of the current supplier.
 */
export class SupplierDuplicateAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
  }

  async _handleDuplicate() {
    const workspace = document.querySelector('ecommerce-supplier-workspace');
    if (!workspace) return;

    const supplier = workspace.getSupplier();
    if (!supplier?.id) {
      alert('Please save the supplier first before duplicating');
      return;
    }

    const confirmed = confirm(`Are you sure you want to duplicate "${supplier.name}"?`);
    if (!confirmed) return;

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/inventory/supplier/${supplier.id}/duplicate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const duplicatedSupplier = await response.json();

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Supplier duplicated: "${duplicatedSupplier.name}"`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);

        // Navigate to the duplicated supplier
        window.location.href = `/umbraco/section/ecommerce/workspace/supplier/edit/${duplicatedSupplier.id}`;
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to duplicate supplier');
      }
    } catch (error) {
      console.error('Error duplicating supplier:', error);
      alert('Failed to duplicate supplier');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        ?disabled=${this._processing}
        @click=${this._handleDuplicate}
      >
        <uui-icon name="icon-documents"></uui-icon>
        ${this._processing ? 'Duplicating...' : 'Duplicate'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-supplier-duplicate-action', SupplierDuplicateAction);

export default SupplierDuplicateAction;
