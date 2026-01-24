import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Payment Method Save Action
 * Handles saving payment method configuration.
 */
export class PaymentMethodSaveAction extends UmbElementMixin(LitElement) {
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
    if (this._saving) return;

    const workspace = document.querySelector('ecommerce-payment-method-workspace');
    if (!workspace) {
      this._showNotification('danger', 'Workspace not found');
      return;
    }

    const method = workspace.getMethod();
    const isNew = workspace.isNewMethod();

    // Validation
    if (!method.name?.trim()) {
      this._showNotification('danger', 'Name is required');
      return;
    }

    if (!method.code?.trim()) {
      this._showNotification('danger', 'Code is required');
      return;
    }

    this._saving = true;

    try {
      const url = isNew
        ? '/umbraco/management/api/v1/ecommerce/payment/method'
        : `/umbraco/management/api/v1/ecommerce/payment/method/${method.id}`;

      const response = await fetch(url, {
        method: isNew ? 'POST' : 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(method)
      });

      if (response.ok) {
        const result = await response.json();
        this._showNotification('positive', `Payment method ${isNew ? 'created' : 'updated'} successfully`);

        if (isNew && result.id) {
          window.history.replaceState({}, '', `/umbraco/section/ecommerce/workspace/ecommerce-payment-method/edit/${result.id}`);
          workspace.setMethod(result);
        }
      } else {
        const error = await response.json();
        this._showNotification('danger', error.message || 'Failed to save payment method');
      }
    } catch (error) {
      console.error('Error saving payment method:', error);
      this._showNotification('danger', 'An error occurred while saving');
    } finally {
      this._saving = false;
    }
  }

  _showNotification(color, message) {
    const event = new CustomEvent('umb-notification', {
      bubbles: true,
      composed: true,
      detail: {
        headline: color === 'positive' ? 'Success' : 'Error',
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
        @click=${this._save}
      >
        ${this._saving ? html`<uui-loader-bar></uui-loader-bar>` : 'Save'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-payment-method-save', PaymentMethodSaveAction);

export default PaymentMethodSaveAction;
