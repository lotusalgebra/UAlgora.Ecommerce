import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

export class CheckoutStepCollection extends UmbElementMixin(LitElement) {
  static styles = css`
    :host { display: block; height: 100%; }
    .container { display: flex; height: 100%; }
    .list-panel { width: 350px; border-right: 1px solid #e0e0e0; display: flex; flex-direction: column; background: #fafafa; }
    .editor-panel { flex: 1; overflow-y: auto; background: #fff; }
    .list-header { padding: 16px; border-bottom: 1px solid #e0e0e0; background: #fff; }
    .list-header h2 { margin: 0 0 12px 0; font-size: 18px; }
    .header-actions { display: flex; gap: 8px; }
    .list-content { flex: 1; overflow-y: auto; }
    .step-item { padding: 14px 16px; border-bottom: 1px solid #e9e9e9; cursor: pointer; display: flex; align-items: center; gap: 12px; }
    .step-item:hover { background: #f0f0f0; }
    .step-item.active { background: #e3f2fd; border-left: 3px solid #1976d2; }
    .step-item.disabled { opacity: 0.5; }
    .step-icon { width: 40px; height: 40px; border-radius: 8px; background: #1976d2; color: #fff; display: flex; align-items: center; justify-content: center; font-size: 18px; }
    .step-icon.disabled { background: #9e9e9e; }
    .step-info { flex: 1; min-width: 0; }
    .step-name { font-weight: 600; display: flex; align-items: center; gap: 8px; }
    .step-meta { font-size: 12px; color: #666; margin-top: 2px; }
    .step-order { width: 24px; height: 24px; border-radius: 50%; background: #e0e0e0; display: flex; align-items: center; justify-content: center; font-weight: 600; font-size: 12px; }
    .badge { padding: 2px 6px; border-radius: 3px; font-size: 10px; font-weight: 500; }
    .badge-required { background: #fff3cd; color: #856404; }
    .badge-optional { background: #e2e3e5; color: #383d41; }
    .drag-handle { cursor: move; color: #999; padding: 4px; }
    .editor-header { padding: 20px 24px; border-bottom: 1px solid #e0e0e0; display: flex; justify-content: space-between; align-items: center; background: #fff; position: sticky; top: 0; z-index: 10; }
    .editor-header h2 { margin: 0; font-size: 20px; }
    .editor-actions { display: flex; gap: 8px; }
    .editor-body { padding: 24px; max-width: 800px; }
    .form-section { margin-bottom: 24px; }
    .form-section-title { font-size: 14px; font-weight: 600; color: #333; margin-bottom: 16px; padding-bottom: 8px; border-bottom: 1px solid #e0e0e0; }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin-bottom: 16px; }
    .form-row.three { grid-template-columns: 1fr 1fr 1fr; }
    .form-group { margin-bottom: 16px; }
    .form-group label { display: block; margin-bottom: 6px; font-weight: 500; font-size: 13px; color: #333; }
    .form-group input, .form-group select, .form-group textarea { width: 100%; padding: 10px 12px; border: 1px solid #ddd; border-radius: 4px; font-size: 14px; box-sizing: border-box; }
    .form-group input:focus, .form-group select:focus, .form-group textarea:focus { outline: none; border-color: #1976d2; box-shadow: 0 0 0 2px rgba(25,118,210,0.1); }
    .form-group small { display: block; margin-top: 4px; color: #666; font-size: 12px; }
    .checkbox-row { display: flex; gap: 24px; margin-bottom: 16px; flex-wrap: wrap; }
    .checkbox-item { display: flex; align-items: center; gap: 8px; }
    .checkbox-item input { width: 18px; height: 18px; }
    .icon-selector { display: grid; grid-template-columns: repeat(6, 1fr); gap: 8px; margin-top: 8px; }
    .icon-option { padding: 10px; border: 2px solid #e0e0e0; border-radius: 6px; cursor: pointer; text-align: center; }
    .icon-option:hover { border-color: #1976d2; background: #f5f5f5; }
    .icon-option.selected { border-color: #1976d2; background: #e3f2fd; }
    .icon-option uui-icon { font-size: 20px; }
    .preview-card { background: #f8f9fa; border: 1px solid #e0e0e0; border-radius: 8px; padding: 20px; margin-top: 16px; }
    .preview-step { display: flex; align-items: center; gap: 12px; padding: 12px; background: #fff; border-radius: 6px; }
    .empty-state { display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100%; color: #666; text-align: center; padding: 40px; }
    .empty-state uui-icon { font-size: 64px; margin-bottom: 16px; opacity: 0.5; }
    .loading { display: flex; justify-content: center; align-items: center; height: 100%; }
  `;

  static properties = {
    _steps: { state: true },
    _loading: { state: true },
    _mode: { state: true },
    _editingStep: { state: true },
    _saving: { state: true }
  };

  constructor() {
    super();
    this._steps = [];
    this._loading = true;
    this._mode = 'list';
    this._editingStep = null;
    this._saving = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadSteps();
  }

  async _loadSteps() {
    try {
      this._loading = true;
      const res = await fetch('/umbraco/management/api/v1/ecommerce/checkoutstep', { credentials: 'include' });
      if (res.ok) {
        const data = await res.json();
        this._steps = data.items || [];
      }
    } catch (e) { console.error('Error loading checkout steps:', e); }
    finally { this._loading = false; }
  }

  _selectStep(step) {
    this._editingStep = { ...step };
    this._mode = 'edit';
  }

  _createNew() {
    const maxOrder = Math.max(0, ...this._steps.map(s => s.sortOrder || 0));
    this._editingStep = {
      code: '',
      name: '',
      title: '',
      description: '',
      instructions: '',
      icon: 'icon-document',
      sortOrder: maxOrder + 1,
      isRequired: true,
      isEnabled: true,
      showOrderSummary: true,
      allowBackNavigation: true,
      cssClass: '',
      validationRules: '',
      configuration: ''
    };
    this._mode = 'create';
  }

  _backToList() {
    this._mode = 'list';
    this._editingStep = null;
  }

  _handleInput(field, value) {
    this._editingStep = { ...this._editingStep, [field]: value };
  }

  async _save() {
    if (!this._editingStep?.code || !this._editingStep?.name) {
      alert('Code and Name are required');
      return;
    }

    this._saving = true;
    try {
      const isNew = !this._editingStep.id;
      const url = isNew ? '/umbraco/management/api/v1/ecommerce/checkoutstep' : `/umbraco/management/api/v1/ecommerce/checkoutstep/${this._editingStep.id}`;
      const res = await fetch(url, {
        method: isNew ? 'POST' : 'PUT',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(this._editingStep)
      });

      if (!res.ok) {
        const err = await res.json();
        throw new Error(err.message || 'Save failed');
      }

      await this._loadSteps();
      if (isNew) {
        const data = await res.json();
        if (data?.id) this._selectStep(data);
        else this._backToList();
      }
    } catch (e) {
      alert('Failed to save: ' + e.message);
    } finally {
      this._saving = false;
    }
  }

  async _delete() {
    if (!confirm(`Delete checkout step "${this._editingStep?.name}"?`)) return;

    try {
      await fetch(`/umbraco/management/api/v1/ecommerce/checkoutstep/${this._editingStep.id}`, {
        method: 'DELETE',
        credentials: 'include'
      });
      await this._loadSteps();
      this._backToList();
    } catch (e) {
      alert('Delete failed');
    }
  }

  async _toggle(step, e) {
    e.stopPropagation();
    try {
      await fetch(`/umbraco/management/api/v1/ecommerce/checkoutstep/${step.id}/toggle`, {
        method: 'POST',
        credentials: 'include'
      });
      await this._loadSteps();
    } catch (e) {
      console.error('Toggle failed:', e);
    }
  }

  async _seedDefaults() {
    if (this._steps.length > 0) {
      if (!confirm('This will only work if no steps exist. Delete existing steps first?')) return;
    }

    try {
      const res = await fetch('/umbraco/management/api/v1/ecommerce/checkoutstep/seed-defaults', {
        method: 'POST',
        credentials: 'include'
      });

      if (!res.ok) {
        const err = await res.json();
        alert(err.message || 'Seed failed');
        return;
      }

      await this._loadSteps();
    } catch (e) {
      alert('Seed failed: ' + e.message);
    }
  }

  _getIconOptions() {
    return [
      'icon-user', 'icon-users', 'icon-truck', 'icon-credit-card',
      'icon-check', 'icon-document', 'icon-settings', 'icon-lock',
      'icon-home', 'icon-shopping-basket', 'icon-receipt-dollar', 'icon-gift'
    ];
  }

  render() {
    if (this._loading) return html`<div class="loading"><uui-loader></uui-loader></div>`;

    return html`
      <div class="container">
        <div class="list-panel">
          <div class="list-header">
            <h2>Checkout Steps</h2>
            <div class="header-actions">
              ${this._steps.length === 0 ? html`
                <uui-button look="secondary" @click=${this._seedDefaults}>
                  <uui-icon name="icon-wand"></uui-icon> Seed Defaults
                </uui-button>
              ` : ''}
              <uui-button look="primary" @click=${this._createNew}>
                <uui-icon name="icon-add"></uui-icon> Add Step
              </uui-button>
            </div>
          </div>
          <div class="list-content">
            ${this._steps.length === 0 ? html`
              <div class="empty-state">
                <uui-icon name="icon-directions-alt"></uui-icon>
                <h3>No checkout steps</h3>
                <p>Create custom checkout steps or seed the defaults.</p>
              </div>
            ` : this._steps.map(step => html`
              <div class="step-item ${this._editingStep?.id === step.id ? 'active' : ''} ${!step.isEnabled ? 'disabled' : ''}"
                   @click=${() => this._selectStep(step)}>
                <span class="step-order">${step.sortOrder}</span>
                <div class="step-icon ${!step.isEnabled ? 'disabled' : ''}">
                  <uui-icon name="${step.icon || 'icon-document'}"></uui-icon>
                </div>
                <div class="step-info">
                  <div class="step-name">
                    ${step.name}
                    <span class="badge ${step.isRequired ? 'badge-required' : 'badge-optional'}">
                      ${step.isRequired ? 'Required' : 'Optional'}
                    </span>
                  </div>
                  <div class="step-meta">${step.code} - ${step.title}</div>
                </div>
                <uui-button look="secondary" compact @click=${(e) => this._toggle(step, e)}>
                  ${step.isEnabled ? 'Disable' : 'Enable'}
                </uui-button>
              </div>
            `)}
          </div>
        </div>
        <div class="editor-panel">
          ${this._mode === 'list' ? this._renderEmptyEditor() : this._renderEditor()}
        </div>
      </div>
    `;
  }

  _renderEmptyEditor() {
    return html`
      <div class="empty-state">
        <uui-icon name="icon-directions-alt"></uui-icon>
        <h3>Select a step to edit</h3>
        <p>Or create a new checkout step</p>
      </div>
    `;
  }

  _renderEditor() {
    const s = this._editingStep || {};
    const isNew = this._mode === 'create';

    return html`
      <div class="editor-header">
        <h2>${isNew ? 'New Checkout Step' : `Edit: ${s.name}`}</h2>
        <div class="editor-actions">
          <uui-button look="secondary" @click=${this._backToList}>Cancel</uui-button>
          ${!isNew ? html`
            <uui-button look="secondary" color="danger" @click=${this._delete}>
              <uui-icon name="icon-delete"></uui-icon> Delete
            </uui-button>
          ` : ''}
          <uui-button look="primary" @click=${this._save} ?disabled=${this._saving}>
            ${this._saving ? 'Saving...' : 'Save'}
          </uui-button>
        </div>
      </div>
      <div class="editor-body">
        <div class="form-section">
          <div class="form-section-title">Basic Information</div>
          <div class="form-row">
            <div class="form-group">
              <label>Code *</label>
              <input type="text" .value=${s.code || ''} @input=${e => this._handleInput('code', e.target.value)}
                     placeholder="e.g., information, shipping, payment" />
              <small>Unique identifier for this step</small>
            </div>
            <div class="form-group">
              <label>Name *</label>
              <input type="text" .value=${s.name || ''} @input=${e => this._handleInput('name', e.target.value)}
                     placeholder="e.g., Contact Info" />
              <small>Display name in admin</small>
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Title</label>
              <input type="text" .value=${s.title || ''} @input=${e => this._handleInput('title', e.target.value)}
                     placeholder="e.g., Contact Information" />
              <small>Title shown to customers</small>
            </div>
            <div class="form-group">
              <label>Order</label>
              <input type="number" min="1" .value=${s.sortOrder || 1} @input=${e => this._handleInput('sortOrder', parseInt(e.target.value) || 1)} />
              <small>Step order in checkout flow</small>
            </div>
          </div>
          <div class="form-group">
            <label>Description</label>
            <textarea rows="2" .value=${s.description || ''} @input=${e => this._handleInput('description', e.target.value)}
                      placeholder="Brief description of this step"></textarea>
          </div>
          <div class="form-group">
            <label>Instructions</label>
            <textarea rows="3" .value=${s.instructions || ''} @input=${e => this._handleInput('instructions', e.target.value)}
                      placeholder="Help text shown to customers"></textarea>
          </div>
        </div>

        <div class="form-section">
          <div class="form-section-title">Icon</div>
          <div class="icon-selector">
            ${this._getIconOptions().map(icon => html`
              <div class="icon-option ${s.icon === icon ? 'selected' : ''}"
                   @click=${() => this._handleInput('icon', icon)}>
                <uui-icon name="${icon}"></uui-icon>
              </div>
            `)}
          </div>
        </div>

        <div class="form-section">
          <div class="form-section-title">Settings</div>
          <div class="checkbox-row">
            <label class="checkbox-item">
              <input type="checkbox" ?checked=${s.isEnabled} @change=${e => this._handleInput('isEnabled', e.target.checked)} />
              <span>Enabled</span>
            </label>
            <label class="checkbox-item">
              <input type="checkbox" ?checked=${s.isRequired} @change=${e => this._handleInput('isRequired', e.target.checked)} />
              <span>Required</span>
            </label>
            <label class="checkbox-item">
              <input type="checkbox" ?checked=${s.showOrderSummary} @change=${e => this._handleInput('showOrderSummary', e.target.checked)} />
              <span>Show Order Summary</span>
            </label>
            <label class="checkbox-item">
              <input type="checkbox" ?checked=${s.allowBackNavigation} @change=${e => this._handleInput('allowBackNavigation', e.target.checked)} />
              <span>Allow Back Navigation</span>
            </label>
          </div>
        </div>

        <div class="form-section">
          <div class="form-section-title">Advanced</div>
          <div class="form-group">
            <label>CSS Class</label>
            <input type="text" .value=${s.cssClass || ''} @input=${e => this._handleInput('cssClass', e.target.value)}
                   placeholder="Custom CSS class for styling" />
          </div>
          <div class="form-group">
            <label>Validation Rules (JSON)</label>
            <textarea rows="3" .value=${s.validationRules || ''} @input=${e => this._handleInput('validationRules', e.target.value)}
                      placeholder='{"requirePhone": true, "minAddressLength": 10}'></textarea>
          </div>
        </div>

        <div class="form-section">
          <div class="form-section-title">Preview</div>
          <div class="preview-card">
            <div class="preview-step">
              <span class="step-order">${s.sortOrder || 1}</span>
              <div class="step-icon">
                <uui-icon name="${s.icon || 'icon-document'}"></uui-icon>
              </div>
              <div class="step-info">
                <div class="step-name">${s.name || 'Step Name'}</div>
                <div class="step-meta">${s.title || 'Step Title'}</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-checkoutstep-collection', CheckoutStepCollection);
export default CheckoutStepCollection;
