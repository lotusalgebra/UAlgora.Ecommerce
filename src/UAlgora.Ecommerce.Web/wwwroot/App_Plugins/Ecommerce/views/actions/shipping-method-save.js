import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Shipping Method Save Action
 */
export class ShippingMethodSaveAction extends UmbElementMixin(LitElement) {
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
    const workspace = this.closest('ecommerce-shipping-method-workspace');
    if (!workspace) return;

    const method = workspace.getMethod();
    const isNew = workspace.isNewMethod();

    if (!method.name) {
      this._showNotification('warning', 'Please enter a method name');
      return;
    }

    if (!method.code) {
      this._showNotification('warning', 'Please enter a method code');
      return;
    }

    try {
      this._saving = true;

      const url = isNew
        ? '/umbraco/management/api/v1/ecommerce/shipping/method'
        : `/umbraco/management/api/v1/ecommerce/shipping/method/${method.id}`;

      const response = await fetch(url, {
        method: isNew ? 'POST' : 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(method)
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || 'Failed to save shipping method');
      }

      const savedMethod = await response.json();
      workspace.setMethod(savedMethod);

      this._showNotification('positive', isNew ? 'Shipping method created' : 'Shipping method saved');

      if (isNew && savedMethod.id) {
        window.history.replaceState(null, '', `/umbraco/section/ecommerce/workspace/shipping-method/edit/${savedMethod.id}`);
      }
    } catch (error) {
      console.error('Error saving shipping method:', error);
      this._showNotification('danger', error.message || 'Failed to save shipping method');
    } finally {
      this._saving = false;
    }
  }

  _showNotification(color, message) {
    const event = new CustomEvent('umb-notification', {
      bubbles: true,
      composed: true,
      detail: {
        headline: color === 'positive' ? 'Success' : color === 'warning' ? 'Warning' : 'Error',
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

customElements.define('ecommerce-shipping-method-save-action', ShippingMethodSaveAction);

export default ShippingMethodSaveAction;
