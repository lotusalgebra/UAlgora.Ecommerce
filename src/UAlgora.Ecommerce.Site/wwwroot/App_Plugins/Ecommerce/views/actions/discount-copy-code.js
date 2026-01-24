import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Discount Copy Code Action
 * Quick action to copy the discount code to clipboard.
 */
export class DiscountCopyCodeAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .code-text {
      font-family: monospace;
      font-size: 11px;
      padding: 2px 6px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-emphasis);
      margin-left: 4px;
      max-width: 120px;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
      display: inline-block;
      vertical-align: middle;
    }
  `;

  static properties = {
    _copied: { type: Boolean, state: true },
    _code: { type: String, state: true }
  };

  constructor() {
    super();
    this._copied = false;
    this._code = null;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-discount-workspace');
    if (workspace) {
      const discount = workspace.getDiscount();
      this._code = discount?.code;
    }
  }

  async _handleCopy() {
    if (!this._code) {
      alert('This discount has no code');
      return;
    }

    try {
      await navigator.clipboard.writeText(this._code);
      this._copied = true;

      const event = new CustomEvent('umb-notification', {
        bubbles: true,
        composed: true,
        detail: {
          headline: 'Copied',
          message: `Code "${this._code}" copied to clipboard`,
          color: 'positive'
        }
      });
      this.dispatchEvent(event);

      // Reset after 2 seconds
      setTimeout(() => {
        this._copied = false;
      }, 2000);
    } catch (error) {
      console.error('Error copying to clipboard:', error);
      // Fallback for older browsers
      const textArea = document.createElement('textarea');
      textArea.value = this._code;
      document.body.appendChild(textArea);
      textArea.select();
      document.execCommand('copy');
      document.body.removeChild(textArea);

      this._copied = true;
      setTimeout(() => {
        this._copied = false;
      }, 2000);
    }
  }

  render() {
    if (!this._code) {
      return html``;
    }

    return html`
      <uui-button
        look="secondary"
        color="${this._copied ? 'positive' : 'default'}"
        @click=${this._handleCopy}
      >
        <uui-icon name="${this._copied ? 'icon-check' : 'icon-clipboard'}"></uui-icon>
        ${this._copied ? 'Copied!' : 'Copy Code'}
        <span class="code-text">${this._code}</span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-discount-copy-code-action', DiscountCopyCodeAction);

export default DiscountCopyCodeAction;
