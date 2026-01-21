import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Tax Zone Editor
 * Form for editing tax zone basic properties.
 */
export class TaxZoneEditor extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .editor-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: var(--uui-size-layout-1);
    }

    .full-width {
      grid-column: 1 / -1;
    }

    uui-box {
      margin-bottom: var(--uui-size-layout-1);
    }

    .form-group {
      margin-bottom: var(--uui-size-space-5);
    }

    .form-group:last-child {
      margin-bottom: 0;
    }

    .form-group label {
      display: block;
      margin-bottom: var(--uui-size-space-2);
      font-weight: bold;
    }

    .form-group .hint {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      margin-top: var(--uui-size-space-1);
    }

    uui-input, uui-textarea {
      width: 100%;
    }

    .toggle-group {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-3);
    }

    .toggle-description {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
    }
  `;

  static properties = {
    _zone: { type: Object, state: true }
  };

  constructor() {
    super();
    this._zone = null;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = this.closest('ecommerce-tax-zone-workspace');
    if (workspace) {
      this._zone = workspace.getZone();
    }
  }

  _updateField(field, value) {
    this._zone = { ...this._zone, [field]: value };
    const workspace = this.closest('ecommerce-tax-zone-workspace');
    if (workspace) {
      workspace._zone = this._zone;
    }
  }

  render() {
    if (!this._zone) {
      return html`<uui-loader></uui-loader>`;
    }

    return html`
      <div class="editor-grid">
        <uui-box headline="Basic Information" class="full-width">
          <div class="form-group">
            <label>Zone Name *</label>
            <uui-input
              .value=${this._zone.name || ''}
              @input=${(e) => this._updateField('name', e.target.value)}
              placeholder="e.g., United States, European Union, California"
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Zone Code *</label>
            <uui-input
              .value=${this._zone.code || ''}
              @input=${(e) => this._updateField('code', e.target.value)}
              placeholder="e.g., US, EU, US-CA"
            ></uui-input>
            <div class="hint">Unique identifier used in code and API calls</div>
          </div>

          <div class="form-group">
            <label>Description</label>
            <uui-textarea
              .value=${this._zone.description || ''}
              @input=${(e) => this._updateField('description', e.target.value)}
              placeholder="Describe the geographic coverage of this zone..."
              rows="3"
            ></uui-textarea>
          </div>
        </uui-box>

        <uui-box headline="Settings">
          <div class="form-group">
            <div class="toggle-group">
              <uui-toggle
                .checked=${this._zone.isActive}
                @change=${(e) => this._updateField('isActive', e.target.checked)}
              ></uui-toggle>
              <div>
                <label>Active</label>
                <div class="toggle-description">Enable this tax zone for calculations</div>
              </div>
            </div>
          </div>

          <div class="form-group">
            <div class="toggle-group">
              <uui-toggle
                .checked=${this._zone.isDefault}
                @change=${(e) => this._updateField('isDefault', e.target.checked)}
              ></uui-toggle>
              <div>
                <label>Default Zone</label>
                <div class="toggle-description">Use when no other zone matches</div>
              </div>
            </div>
          </div>
        </uui-box>

        <uui-box headline="Priority">
          <div class="form-group">
            <label>Priority</label>
            <uui-input
              type="number"
              .value=${this._zone.priority?.toString() || '0'}
              @input=${(e) => this._updateField('priority', parseInt(e.target.value) || 0)}
            ></uui-input>
            <div class="hint">Higher priority zones are checked first (useful for more specific zones)</div>
          </div>

          <div class="form-group">
            <label>Sort Order</label>
            <uui-input
              type="number"
              .value=${this._zone.sortOrder?.toString() || '0'}
              @input=${(e) => this._updateField('sortOrder', parseInt(e.target.value) || 0)}
            ></uui-input>
            <div class="hint">Display order in lists (lower numbers appear first)</div>
          </div>
        </uui-box>
      </div>
    `;
  }
}

customElements.define('ecommerce-tax-zone-editor', TaxZoneEditor);

export default TaxZoneEditor;
