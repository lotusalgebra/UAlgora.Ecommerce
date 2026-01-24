import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import './editors/checkout-step-editor.js';

/**
 * Checkout Step Workspace Element
 * Main workspace container for editing checkout steps in the Umbraco backoffice.
 */
export class CheckoutStepWorkspace extends UmbElementMixin(LitElement) {
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
    stepId: { type: String },
    _mode: { type: String, state: true }
  };

  constructor() {
    super();
    this.stepId = null;
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
    console.log('Checkout step workspace: Parsing route:', path);

    // Match patterns like:
    // /section/ecommerce/view/workspace/checkout-step/edit/{id}
    // /section/ecommerce/view/workspace/checkout-step/create
    // /section/ecommerce/view/workspace/checkout-step/{id}
    // Also support legacy patterns without /view/
    const editMatch = path.match(/\/(?:view\/)?workspace\/checkout-step\/edit\/([a-f0-9-]+)/i);
    const directMatch = path.match(/\/(?:view\/)?workspace\/checkout-step\/([a-f0-9-]+)$/i);
    const createMatch = path.match(/\/(?:view\/)?workspace\/checkout-step\/?(?:create)?$/i);

    if (editMatch) {
      this.stepId = editMatch[1];
      this._mode = 'edit';
      console.log('Checkout step workspace: Edit mode, ID:', this.stepId);
    } else if (directMatch && directMatch[1] !== 'create') {
      this.stepId = directMatch[1];
      this._mode = 'edit';
      console.log('Checkout step workspace: Direct edit mode, ID:', this.stepId);
    } else if (createMatch) {
      this.stepId = null;
      this._mode = 'create';
      console.log('Checkout step workspace: Create mode');
    }

    // Force re-render
    this.requestUpdate();
  }

  _handleStepSaved(event) {
    const step = event.detail.step;
    if (step && step.id && this._mode === 'create') {
      // Update URL to edit mode without page reload
      const newUrl = `/section/ecommerce/view/workspace/checkout-step/edit/${step.id}`;
      window.history.replaceState({}, '', newUrl);
      this.stepId = step.id;
      this._mode = 'edit';
    }
  }

  _handleEditorCancel() {
    // Navigate back to checkout steps list or dashboard
    window.history.pushState({}, '', '/section/ecommerce/view/dashboard');
    window.dispatchEvent(new PopStateEvent('popstate'));
  }

  render() {
    return html`
      <div class="workspace-container">
        <ecommerce-checkout-step-editor
          .stepId=${this.stepId}
          @step-saved=${this._handleStepSaved}
          @editor-cancel=${this._handleEditorCancel}
        ></ecommerce-checkout-step-editor>
      </div>
    `;
  }
}

customElements.define('ecommerce-checkout-step-workspace', CheckoutStepWorkspace);

export default CheckoutStepWorkspace;
