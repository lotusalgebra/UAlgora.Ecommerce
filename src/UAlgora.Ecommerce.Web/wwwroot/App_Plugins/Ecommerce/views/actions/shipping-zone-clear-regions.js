import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Shipping Zone Clear Regions Action
 * Quick action to clear all geographic regions from a zone.
 */
export class ShippingZoneClearRegionsAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .region-badge {
      font-size: 11px;
      padding: 2px 6px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-emphasis);
      margin-left: 4px;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _regionSummary: { type: String, state: true },
    _hasRegions: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._regionSummary = 'All regions';
    this._hasRegions = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-shipping-zone-workspace');
    if (workspace) {
      const zone = workspace.getShippingZone();
      this._regionSummary = zone?.regionSummary ?? 'All regions';
      this._hasRegions = (zone?.countries?.length > 0) ||
        (zone?.states?.length > 0) ||
        (zone?.postalCodePatterns?.length > 0) ||
        (zone?.cities?.length > 0);
    }
  }

  async _handleClear() {
    const workspace = document.querySelector('ecommerce-shipping-zone-workspace');
    if (!workspace) return;

    const zone = workspace.getShippingZone();
    if (!zone?.id) {
      alert('Please save the shipping zone first');
      return;
    }

    if (!this._hasRegions) {
      alert('This zone has no regions to clear');
      return;
    }

    if (!confirm(`Clear all geographic regions from "${zone.name}"?\n\nThis will remove all countries, states, postal codes, and cities.\nThe zone will match all regions (or act as a fallback if not default).`)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/shipping/zone/${zone.id}/clear-regions`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setShippingZone(result);
        this._regionSummary = result.regionSummary;
        this._hasRegions = false;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'All geographic regions have been cleared',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to clear regions');
      }
    } catch (error) {
      console.error('Error clearing regions:', error);
      alert('Failed to clear regions');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._hasRegions ? 'warning' : 'default'}"
        ?disabled=${this._processing || !this._hasRegions}
        @click=${this._handleClear}
      >
        <uui-icon name="icon-delete"></uui-icon>
        ${this._processing ? 'Clearing...' : 'Clear Regions'}
        <span class="region-badge">${this._regionSummary}</span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-shipping-zone-clear-regions-action', ShippingZoneClearRegionsAction);

export default ShippingZoneClearRegionsAction;
