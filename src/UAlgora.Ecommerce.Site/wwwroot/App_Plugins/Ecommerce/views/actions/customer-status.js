import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Customer Status Action
 * Quick action to change customer status (Active, Suspended, Inactive).
 */
export class CustomerStatusAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .status-dropdown {
      position: relative;
      display: inline-block;
    }

    .status-menu {
      position: absolute;
      top: 100%;
      right: 0;
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      box-shadow: var(--uui-shadow-depth-3);
      z-index: 1000;
      min-width: 150px;
      display: none;
    }

    .status-menu.open {
      display: block;
    }

    .status-option {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
      padding: var(--uui-size-space-3) var(--uui-size-space-4);
      cursor: pointer;
      border: none;
      background: none;
      width: 100%;
      text-align: left;
    }

    .status-option:hover {
      background: var(--uui-color-surface-emphasis);
    }

    .status-option.active {
      color: var(--uui-color-positive);
    }

    .status-option.suspended {
      color: var(--uui-color-warning);
    }

    .status-option.inactive {
      color: var(--uui-color-text-alt);
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _status: { type: String, state: true },
    _showMenu: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._status = 'Active';
    this._showMenu = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
    document.addEventListener('click', this._handleOutsideClick);
  }

  disconnectedCallback() {
    super.disconnectedCallback();
    document.removeEventListener('click', this._handleOutsideClick);
  }

  _handleOutsideClick = (e) => {
    if (!this.contains(e.target)) {
      this._showMenu = false;
    }
  };

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-customer-workspace');
    if (workspace) {
      const customer = workspace.getCustomer();
      this._status = customer?.status || 'Active';
    }
  }

  _toggleMenu(e) {
    e.stopPropagation();
    this._showMenu = !this._showMenu;
  }

  async _handleStatusChange(newStatus) {
    const workspace = document.querySelector('ecommerce-customer-workspace');
    if (!workspace) return;

    const customer = workspace.getCustomer();
    if (!customer?.id) {
      alert('Please save the customer first');
      return;
    }

    if (this._status === newStatus) {
      this._showMenu = false;
      return;
    }

    this._processing = true;
    this._showMenu = false;

    const endpoint = newStatus === 'Active' ? 'activate' :
                     newStatus === 'Suspended' ? 'suspend' :
                     'toggle-status';

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/customers/${customer.id}/${endpoint}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setCustomer(result);
        this._status = result.status;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Customer status changed to ${result.status}`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to change customer status');
      }
    } catch (error) {
      console.error('Error changing customer status:', error);
      alert('Failed to change customer status');
    } finally {
      this._processing = false;
    }
  }

  _getStatusColor() {
    switch (this._status) {
      case 'Active': return 'positive';
      case 'Suspended': return 'warning';
      case 'Inactive': return 'default';
      default: return 'default';
    }
  }

  _getStatusIcon() {
    switch (this._status) {
      case 'Active': return 'icon-check';
      case 'Suspended': return 'icon-block';
      case 'Inactive': return 'icon-delete';
      default: return 'icon-user';
    }
  }

  render() {
    return html`
      <div class="status-dropdown">
        <uui-button
          look="secondary"
          color="${this._getStatusColor()}"
          ?disabled=${this._processing}
          @click=${this._toggleMenu}
        >
          <uui-icon name="${this._getStatusIcon()}"></uui-icon>
          ${this._processing ? 'Changing...' : this._status}
          <uui-icon name="icon-navigation-down" style="font-size: 10px; margin-left: 4px;"></uui-icon>
        </uui-button>

        <div class="status-menu ${this._showMenu ? 'open' : ''}">
          <button class="status-option active" @click=${() => this._handleStatusChange('Active')}>
            <uui-icon name="icon-check"></uui-icon>
            Active
          </button>
          <button class="status-option suspended" @click=${() => this._handleStatusChange('Suspended')}>
            <uui-icon name="icon-block"></uui-icon>
            Suspended
          </button>
          <button class="status-option inactive" @click=${() => this._handleStatusChange('Inactive')}>
            <uui-icon name="icon-delete"></uui-icon>
            Inactive
          </button>
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-customer-status-action', CustomerStatusAction);

export default CustomerStatusAction;
