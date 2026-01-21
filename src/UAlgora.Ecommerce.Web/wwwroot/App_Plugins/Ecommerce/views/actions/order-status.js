import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Order Status Action
 * Provides a dropdown to change order status.
 */
export class OrderStatusAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: inline-block;
    }

    .status-dropdown {
      position: relative;
    }

    .status-menu {
      position: absolute;
      top: 100%;
      right: 0;
      z-index: 100;
      min-width: 200px;
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      box-shadow: var(--uui-shadow-depth-3);
      margin-top: var(--uui-size-space-2);
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
      border-bottom: 1px solid var(--uui-color-border-standalone);
    }

    .status-option:last-child {
      border-bottom: none;
    }

    .status-option:hover {
      background: var(--uui-color-surface-emphasis);
    }

    .status-option.disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .status-option uui-icon {
      width: 20px;
    }

    .status-dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
    }

    .dot-pending { background: #ffc107; }
    .dot-confirmed { background: #17a2b8; }
    .dot-processing { background: #007bff; }
    .dot-shipped { background: #6f42c1; }
    .dot-delivered { background: #28a745; }
    .dot-completed { background: #28a745; }
    .dot-cancelled { background: #dc3545; }
    .dot-refunded { background: #fd7e14; }
    .dot-onhold { background: #6c757d; }

    .divider {
      height: 1px;
      background: var(--uui-color-border);
      margin: var(--uui-size-space-2) 0;
    }
  `;

  static properties = {
    _order: { type: Object, state: true },
    _isOpen: { type: Boolean, state: true },
    _updating: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._order = null;
    this._isOpen = false;
    this._updating = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();

    // Close dropdown when clicking outside
    document.addEventListener('click', this._handleOutsideClick.bind(this));
  }

  disconnectedCallback() {
    super.disconnectedCallback();
    document.removeEventListener('click', this._handleOutsideClick.bind(this));
  }

  _handleOutsideClick(e) {
    if (!this.contains(e.target)) {
      this._isOpen = false;
    }
  }

  _loadFromWorkspace() {
    const workspace = this.closest('ecommerce-order-workspace');
    if (workspace) {
      this._order = workspace.getOrder();
    }
  }

  _toggleDropdown(e) {
    e.stopPropagation();
    this._isOpen = !this._isOpen;
  }

  _getStatusOptions() {
    const currentStatus = this._order?.status?.toLowerCase() || 'pending';

    const allOptions = [
      { status: 'Pending', key: 'pending', icon: 'icon-time' },
      { status: 'Confirmed', key: 'confirmed', icon: 'icon-check' },
      { status: 'Processing', key: 'processing', icon: 'icon-loading' },
      { status: 'Shipped', key: 'shipped', icon: 'icon-truck' },
      { status: 'Delivered', key: 'delivered', icon: 'icon-home' },
      { status: 'Completed', key: 'completed', icon: 'icon-checkbox-dotted' },
      { divider: true },
      { status: 'On Hold', key: 'onhold', icon: 'icon-pause' },
      { status: 'Cancelled', key: 'cancelled', icon: 'icon-delete', danger: true },
      { status: 'Refunded', key: 'refunded', icon: 'icon-coin-dollar', warning: true }
    ];

    return allOptions.map(opt => {
      if (opt.divider) return opt;
      return {
        ...opt,
        current: opt.key === currentStatus,
        disabled: opt.key === currentStatus
      };
    });
  }

  async _handleStatusChange(newStatus) {
    if (!this._order?.id || this._updating) return;

    this._isOpen = false;

    try {
      this._updating = true;

      const response = await fetch(`/umbraco/management/api/v1/ecommerce/order/${this._order.id}/status`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ status: newStatus })
      });

      if (!response.ok) {
        throw new Error('Failed to update order status');
      }

      const updatedOrder = await response.json();

      // Update workspace
      const workspace = this.closest('ecommerce-order-workspace');
      if (workspace) {
        workspace.setOrder(updatedOrder);
        this._order = updatedOrder;
      }

      this._showNotification('positive', `Order status updated to ${newStatus}`);
    } catch (error) {
      console.error('Error updating status:', error);
      this._showNotification('danger', 'Failed to update order status');
    } finally {
      this._updating = false;
    }
  }

  _showNotification(color, message) {
    const event = new CustomEvent('umb-notification', {
      bubbles: true,
      composed: true,
      detail: { headline: color === 'positive' ? 'Success' : 'Error', message, color }
    });
    this.dispatchEvent(event);
  }

  render() {
    const options = this._getStatusOptions();

    return html`
      <div class="status-dropdown">
        <uui-button
          look="primary"
          ?disabled=${this._updating}
          @click=${this._toggleDropdown}
        >
          ${this._updating ? html`<uui-loader-circle></uui-loader-circle>` : ''}
          Update Status
          <uui-icon name="icon-navigation-down"></uui-icon>
        </uui-button>

        <div class="status-menu ${this._isOpen ? 'open' : ''}">
          ${options.map(opt => {
            if (opt.divider) {
              return html`<div class="divider"></div>`;
            }
            return html`
              <div
                class="status-option ${opt.disabled ? 'disabled' : ''}"
                @click=${() => !opt.disabled && this._handleStatusChange(opt.status)}
              >
                <span class="status-dot dot-${opt.key}"></span>
                <span>${opt.status}</span>
                ${opt.current ? html`<uui-icon name="icon-check" style="margin-left: auto;"></uui-icon>` : ''}
              </div>
            `;
          })}
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-order-status-action', OrderStatusAction);

export default OrderStatusAction;
