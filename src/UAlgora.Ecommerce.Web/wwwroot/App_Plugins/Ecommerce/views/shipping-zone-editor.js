import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Shipping Zone Editor
 * Form for editing shipping zone basic details.
 */
export class ShippingZoneEditor extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .form-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: var(--uui-size-layout-1);
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: var(--uui-size-space-2);
    }

    .form-group.full-width {
      grid-column: 1 / -1;
    }

    .form-group label {
      font-weight: 500;
    }

    .form-group .hint {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
    }

    uui-box {
      margin-bottom: var(--uui-size-layout-1);
    }
  `;

  static properties = {
    _zone: { type: Object, state: true }
  };

  constructor() {
    super();
    this._zone = {};
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = this.closest('ecommerce-shipping-zone-workspace');
    if (workspace) {
      this._zone = workspace.getZone();
    }
  }

  _updateZone(field, value) {
    const workspace = this.closest('ecommerce-shipping-zone-workspace');
    if (workspace) {
      workspace.setZone({ [field]: value });
      this._zone = workspace.getZone();
    }
  }

  render() {
    return html`
      <uui-box headline="Basic Information">
        <div class="form-grid">
          <div class="form-group">
            <label>Zone Name *</label>
            <uui-input
              placeholder="e.g., United States"
              .value=${this._zone.name || ''}
              @input=${(e) => this._updateZone('name', e.target.value)}
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Code *</label>
            <uui-input
              placeholder="e.g., us"
              .value=${this._zone.code || ''}
              @input=${(e) => this._updateZone('code', e.target.value)}
            ></uui-input>
            <span class="hint">Unique identifier for this zone</span>
          </div>

          <div class="form-group full-width">
            <label>Description</label>
            <uui-textarea
              placeholder="Describe this shipping zone..."
              .value=${this._zone.description || ''}
              @input=${(e) => this._updateZone('description', e.target.value)}
            ></uui-textarea>
          </div>

          <div class="form-group">
            <label>Sort Order</label>
            <uui-input
              type="number"
              .value=${this._zone.sortOrder?.toString() || '0'}
              @input=${(e) => this._updateZone('sortOrder', parseInt(e.target.value) || 0)}
            ></uui-input>
          </div>
        </div>
      </uui-box>

      <uui-box headline="Status">
        <div class="form-grid">
          <div class="form-group">
            <uui-toggle
              ?checked=${this._zone.isActive}
              @change=${(e) => this._updateZone('isActive', e.target.checked)}
            >
              Active
            </uui-toggle>
            <span class="hint">Inactive zones won't be used for shipping calculations</span>
          </div>

          <div class="form-group">
            <uui-toggle
              ?checked=${this._zone.isDefault}
              @change=${(e) => this._updateZone('isDefault', e.target.checked)}
            >
              Default Zone
            </uui-toggle>
            <span class="hint">Used when no other zone matches the shipping address</span>
          </div>
        </div>
      </uui-box>
    `;
  }
}

customElements.define('ecommerce-shipping-zone-editor', ShippingZoneEditor);

export default ShippingZoneEditor;
