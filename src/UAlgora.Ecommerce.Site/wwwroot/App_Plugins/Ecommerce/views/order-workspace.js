import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import './editors/order-editor.js';

/**
 * Order Workspace Element
 * Main workspace container for editing orders in the Umbraco backoffice.
 */
export class OrderWorkspace extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      width: 100%;
      height: 100%;
    }

    .workspace-container {
      height: 100%;
    }
  `;

  static properties = {
    orderId: { type: String },
    _mode: { type: String, state: true }
  };

  constructor() {
    super();
    this.orderId = null;
    this._mode = 'create';
  }

  connectedCallback() {
    super.connectedCallback();
    this._parseRoute();

    // Listen for route changes
    this._routeHandler = () => this._parseRoute();
    window.addEventListener('popstate', this._routeHandler);
  }

  disconnectedCallback() {
    super.disconnectedCallback();
    if (this._routeHandler) {
      window.removeEventListener('popstate', this._routeHandler);
    }
  }

  _parseRoute() {
    const path = window.location.pathname;
    console.log('Order workspace: Parsing route:', path);

    // Match patterns like:
    // /section/ecommerce/view/workspace/order/edit/{id}
    // /section/ecommerce/view/workspace/order/create
    // /section/ecommerce/view/workspace/order/{id}
    // Also support legacy patterns without /view/
    const editMatch = path.match(/\/(?:view\/)?workspace\/order\/edit\/([a-f0-9-]+)/i);
    const directMatch = path.match(/\/(?:view\/)?workspace\/order\/([a-f0-9-]+)$/i);
    const createMatch = path.match(/\/(?:view\/)?workspace\/order\/?(?:create)?$/i);

    if (editMatch) {
      this.orderId = editMatch[1];
      this._mode = 'edit';
      console.log('Order workspace: Edit mode, ID:', this.orderId);
    } else if (directMatch && directMatch[1] !== 'create') {
      this.orderId = directMatch[1];
      this._mode = 'edit';
      console.log('Order workspace: Direct edit mode, ID:', this.orderId);
    } else if (createMatch) {
      this.orderId = null;
      this._mode = 'create';
      console.log('Order workspace: Create mode');
    }

    // Force re-render
    this.requestUpdate();
  }

  _handleOrderSaved(event) {
    const order = event.detail.order;
    if (order && order.id && this._mode === 'create') {
      // Update URL to edit mode without page reload
      const newUrl = `/section/ecommerce/view/workspace/order/edit/${order.id}`;
      window.history.replaceState({}, '', newUrl);
      this.orderId = order.id;
      this._mode = 'edit';
    }
  }

  _handleEditorCancel() {
    // Navigate back to orders list
    window.history.pushState({}, '', '/section/ecommerce/view/orders');
    window.dispatchEvent(new PopStateEvent('popstate'));
  }

  render() {
    return html`
      <div class="workspace-container">
        <ecommerce-order-editor
          .orderId=${this.orderId}
          @order-saved=${this._handleOrderSaved}
          @editor-cancel=${this._handleEditorCancel}
        ></ecommerce-order-editor>
      </div>
    `;
  }
}

customElements.define('ecommerce-order-workspace', OrderWorkspace);

export default OrderWorkspace;
