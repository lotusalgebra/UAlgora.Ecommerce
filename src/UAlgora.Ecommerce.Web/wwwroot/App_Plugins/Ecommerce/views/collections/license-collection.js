import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * License Collection with Management Dashboard
 * Comprehensive license management for Algora Commerce.
 */
export class LicenseCollection extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: flex;
      height: 100%;
      background: #f5f5f5;
    }

    /* Main Container */
    .license-container {
      flex: 1;
      display: flex;
      flex-direction: column;
      height: 100%;
      overflow: hidden;
    }

    .license-header {
      padding: 24px 32px;
      background: #fff;
      border-bottom: 1px solid #e0e0e0;
    }

    .license-header h1 {
      margin: 0 0 8px 0;
      font-size: 24px;
      font-weight: 700;
      color: #1b264f;
    }

    .license-header p {
      margin: 0;
      color: #666;
      font-size: 14px;
    }

    .license-content {
      flex: 1;
      overflow-y: auto;
      padding: 24px 32px;
    }

    /* License Status Card */
    .status-card {
      background: #fff;
      border-radius: 16px;
      padding: 28px;
      margin-bottom: 24px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.06);
    }

    .status-card.valid {
      border-left: 4px solid #22c55e;
    }

    .status-card.trial {
      border-left: 4px solid #f59e0b;
    }

    .status-card.expired {
      border-left: 4px solid #ef4444;
    }

    .status-card.unlicensed {
      border-left: 4px solid #6b7280;
    }

    .status-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 24px;
    }

    .status-info h2 {
      margin: 0 0 8px 0;
      font-size: 20px;
      font-weight: 600;
      color: #1b264f;
    }

    .status-badge {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 6px 14px;
      border-radius: 20px;
      font-size: 13px;
      font-weight: 600;
    }

    .status-badge.valid { background: #dcfce7; color: #16a34a; }
    .status-badge.trial { background: #fef3c7; color: #d97706; }
    .status-badge.expired { background: #fee2e2; color: #dc2626; }
    .status-badge.unlicensed { background: #f3f4f6; color: #6b7280; }
    .status-badge.grace { background: #fef3c7; color: #d97706; }

    .tier-badge {
      padding: 10px 20px;
      border-radius: 12px;
      font-size: 16px;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 1px;
    }

    .tier-badge.trial { background: linear-gradient(135deg, #fbbf24, #f59e0b); color: #fff; }
    .tier-badge.standard { background: linear-gradient(135deg, #667eea, #764ba2); color: #fff; }
    .tier-badge.enterprise { background: linear-gradient(135deg, #1b264f, #2d3a6d); color: #fff; }
    .tier-badge.unlicensed { background: #e5e7eb; color: #6b7280; }

    .license-details {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 20px;
    }

    .detail-item {
      padding: 16px;
      background: #f8f9fa;
      border-radius: 10px;
    }

    .detail-label {
      font-size: 12px;
      color: #888;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      margin-bottom: 6px;
    }

    .detail-value {
      font-size: 16px;
      font-weight: 600;
      color: #1b264f;
    }

    .detail-value.warning { color: #f59e0b; }
    .detail-value.danger { color: #ef4444; }
    .detail-value.success { color: #22c55e; }

    /* License Key Input */
    .license-key-section {
      background: #fff;
      border-radius: 16px;
      padding: 28px;
      margin-bottom: 24px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.06);
    }

    .section-title {
      margin: 0 0 20px 0;
      font-size: 18px;
      font-weight: 600;
      color: #1b264f;
    }

    .key-input-group {
      display: flex;
      gap: 12px;
    }

    .key-input {
      flex: 1;
      padding: 14px 18px;
      border: 2px solid #e0e0e0;
      border-radius: 10px;
      font-size: 15px;
      font-family: 'Courier New', monospace;
      letter-spacing: 1px;
      transition: border-color 0.2s;
    }

    .key-input:focus {
      outline: none;
      border-color: #667eea;
    }

    .key-input::placeholder {
      color: #aaa;
      font-family: inherit;
    }

    /* Buttons */
    .btn {
      padding: 12px 24px;
      border-radius: 10px;
      font-size: 14px;
      font-weight: 600;
      cursor: pointer;
      border: none;
      transition: all 0.2s;
      display: inline-flex;
      align-items: center;
      gap: 8px;
    }

    .btn-primary {
      background: linear-gradient(135deg, #667eea, #764ba2);
      color: #fff;
    }

    .btn-primary:hover {
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
    }

    .btn-secondary {
      background: #f5f5f5;
      color: #333;
    }

    .btn-secondary:hover {
      background: #e5e5e5;
    }

    .btn-success {
      background: linear-gradient(135deg, #22c55e, #16a34a);
      color: #fff;
    }

    .btn-warning {
      background: linear-gradient(135deg, #f59e0b, #d97706);
      color: #fff;
    }

    .btn-danger {
      background: linear-gradient(135deg, #ef4444, #dc2626);
      color: #fff;
    }

    .btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
      transform: none;
    }

    /* Features Grid */
    .features-section {
      background: #fff;
      border-radius: 16px;
      padding: 28px;
      margin-bottom: 24px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.06);
    }

    .features-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
      gap: 12px;
    }

    .feature-item {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 12px 16px;
      background: #f8f9fa;
      border-radius: 8px;
      font-size: 14px;
    }

    .feature-item.enabled {
      background: #dcfce7;
      color: #16a34a;
    }

    .feature-item.disabled {
      background: #f3f4f6;
      color: #9ca3af;
    }

    .feature-icon {
      width: 20px;
      height: 20px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 12px;
    }

    .feature-icon.enabled { background: #22c55e; color: #fff; }
    .feature-icon.disabled { background: #d1d5db; color: #fff; }

    /* Pricing Cards */
    .pricing-section {
      background: #fff;
      border-radius: 16px;
      padding: 28px;
      margin-bottom: 24px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.06);
    }

    .pricing-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
      gap: 20px;
    }

    .pricing-card {
      border: 2px solid #e0e0e0;
      border-radius: 16px;
      padding: 24px;
      text-align: center;
      transition: all 0.2s;
    }

    .pricing-card:hover {
      border-color: #667eea;
      transform: translateY(-4px);
      box-shadow: 0 8px 24px rgba(0,0,0,0.1);
    }

    .pricing-card.current {
      border-color: #667eea;
      background: #f8f9ff;
    }

    .pricing-card.recommended {
      border-color: #667eea;
      position: relative;
    }

    .recommended-badge {
      position: absolute;
      top: -12px;
      left: 50%;
      transform: translateX(-50%);
      background: linear-gradient(135deg, #667eea, #764ba2);
      color: #fff;
      padding: 4px 16px;
      border-radius: 20px;
      font-size: 11px;
      font-weight: 600;
      text-transform: uppercase;
    }

    .pricing-tier {
      font-size: 18px;
      font-weight: 700;
      color: #1b264f;
      margin-bottom: 8px;
    }

    .pricing-price {
      font-size: 36px;
      font-weight: 800;
      color: #667eea;
      margin-bottom: 4px;
    }

    .pricing-period {
      font-size: 14px;
      color: #888;
      margin-bottom: 20px;
    }

    .pricing-features {
      text-align: left;
      margin-bottom: 20px;
    }

    .pricing-feature {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px 0;
      font-size: 14px;
      color: #555;
    }

    .pricing-feature::before {
      content: "✓";
      color: #22c55e;
      font-weight: bold;
    }

    /* Trial Section */
    .trial-section {
      background: linear-gradient(135deg, #fef3c7, #fde68a);
      border-radius: 16px;
      padding: 28px;
      margin-bottom: 24px;
      text-align: center;
    }

    .trial-section h3 {
      margin: 0 0 12px 0;
      font-size: 20px;
      color: #92400e;
    }

    .trial-section p {
      margin: 0 0 20px 0;
      color: #a16207;
    }

    /* Activation History */
    .history-section {
      background: #fff;
      border-radius: 16px;
      padding: 28px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.06);
    }

    .history-table {
      width: 100%;
      border-collapse: collapse;
    }

    .history-table th,
    .history-table td {
      padding: 12px 16px;
      text-align: left;
      border-bottom: 1px solid #e0e0e0;
    }

    .history-table th {
      font-size: 12px;
      color: #888;
      text-transform: uppercase;
      font-weight: 600;
    }

    .history-table td {
      font-size: 14px;
      color: #333;
    }

    /* Empty State */
    .empty-state {
      text-align: center;
      padding: 60px 40px;
    }

    .empty-icon {
      font-size: 64px;
      margin-bottom: 20px;
    }

    .empty-state h3 {
      margin: 0 0 12px 0;
      font-size: 20px;
      color: #1b264f;
    }

    .empty-state p {
      margin: 0 0 24px 0;
      color: #666;
    }

    /* Loading */
    .loading {
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 60px;
    }

    .spinner {
      width: 40px;
      height: 40px;
      border: 3px solid #e0e0e0;
      border-top-color: #667eea;
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    /* Toast Messages */
    .toast {
      position: fixed;
      bottom: 24px;
      right: 24px;
      padding: 16px 24px;
      border-radius: 10px;
      color: #fff;
      font-weight: 500;
      box-shadow: 0 4px 12px rgba(0,0,0,0.15);
      z-index: 1000;
      animation: slideIn 0.3s ease;
    }

    .toast.success { background: #22c55e; }
    .toast.error { background: #ef4444; }
    .toast.warning { background: #f59e0b; }

    @keyframes slideIn {
      from {
        transform: translateX(100%);
        opacity: 0;
      }
      to {
        transform: translateX(0);
        opacity: 1;
      }
    }
  `;

  static properties = {
    loading: { type: Boolean },
    license: { type: Object },
    licenseKey: { type: String },
    features: { type: Array },
    activationHistory: { type: Array },
    toast: { type: Object },
    validating: { type: Boolean }
  };

  constructor() {
    super();
    this.loading = true;
    this.license = null;
    this.licenseKey = '';
    this.features = [];
    this.activationHistory = [];
    this.toast = null;
    this.validating = false;

    // All available features
    this.allFeatures = [
      { key: 'multi_store', name: 'Multi-Store', tier: 'Enterprise' },
      { key: 'gift_cards', name: 'Gift Cards', tier: 'Trial' },
      { key: 'returns', name: 'Returns Management', tier: 'Trial' },
      { key: 'email_templates', name: 'Email Templates', tier: 'Trial' },
      { key: 'advanced_reporting', name: 'Advanced Reporting', tier: 'Standard' },
      { key: 'audit_logging', name: 'Audit Logging', tier: 'Trial' },
      { key: 'webhooks', name: 'Webhooks', tier: 'Trial' },
      { key: 'api_access', name: 'API Access', tier: 'Trial' },
      { key: 'custom_integrations', name: 'Custom Integrations', tier: 'Enterprise' },
      { key: 'white_labeling', name: 'White Labeling', tier: 'Enterprise' },
      { key: 'priority_support', name: 'Priority Support', tier: 'Enterprise' },
      { key: 'advanced_discounts', name: 'Advanced Discounts', tier: 'Standard' },
      { key: 'b2b_features', name: 'B2B Features', tier: 'Enterprise' },
      { key: 'subscriptions', name: 'Subscriptions', tier: 'Enterprise' },
      { key: 'multi_currency', name: 'Multi-Currency', tier: 'Standard' },
      { key: 'multi_language', name: 'Multi-Language', tier: 'Enterprise' },
      { key: 'advanced_inventory', name: 'Advanced Inventory', tier: 'Enterprise' },
      { key: 'advanced_shipping', name: 'Advanced Shipping', tier: 'Enterprise' },
      { key: 'advanced_tax', name: 'Advanced Tax', tier: 'Enterprise' },
      { key: 'import_export', name: 'Import/Export', tier: 'Standard' }
    ];
  }

  connectedCallback() {
    super.connectedCallback();
    this.loadLicenseInfo();
  }

  async loadLicenseInfo() {
    this.loading = true;
    try {
      // Get active licenses
      const response = await fetch('/umbraco/management/api/v1/ecommerce/license/active');
      if (response.ok) {
        const licenses = await response.json();
        if (licenses.length > 0) {
          this.license = licenses[0];
          // Get features for this license
          const featuresResponse = await fetch(`/umbraco/management/api/v1/ecommerce/license/${this.license.id}/features`);
          if (featuresResponse.ok) {
            this.features = await featuresResponse.json();
          }
        }
      }
    } catch (error) {
      console.error('Error loading license info:', error);
    }
    this.loading = false;
  }

  async validateLicenseKey() {
    if (!this.licenseKey.trim()) {
      this.showToast('Please enter a license key', 'warning');
      return;
    }

    this.validating = true;
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/license/validate', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          key: this.licenseKey.trim(),
          domain: window.location.hostname
        })
      });

      const result = await response.json();
      if (result.isValid) {
        this.showToast('License validated successfully!', 'success');
        await this.loadLicenseInfo();
      } else {
        this.showToast(result.errorMessage || 'Invalid license key', 'error');
      }
    } catch (error) {
      this.showToast('Error validating license', 'error');
    }
    this.validating = false;
  }

  async activateLicense() {
    if (!this.licenseKey.trim()) {
      this.showToast('Please enter a license key', 'warning');
      return;
    }

    this.validating = true;
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/license/activate', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          key: this.licenseKey.trim(),
          domain: window.location.hostname
        })
      });

      const result = await response.json();
      if (result.success) {
        this.showToast('License activated successfully!', 'success');
        this.licenseKey = '';
        await this.loadLicenseInfo();
      } else {
        this.showToast(result.errorMessage || 'Activation failed', 'error');
      }
    } catch (error) {
      this.showToast('Error activating license', 'error');
    }
    this.validating = false;
  }

  async startTrial() {
    const email = prompt('Enter your email address to start a 14-day trial:');
    if (!email) return;

    const name = prompt('Enter your name or company name:');
    if (!name) return;

    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/license/trial', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          customerName: name,
          customerEmail: email,
          trialDays: 14
        })
      });

      if (response.ok) {
        const license = await response.json();
        this.showToast(`Trial started! Your key: ${license.key}`, 'success');
        await this.loadLicenseInfo();
      } else {
        this.showToast('Error starting trial', 'error');
      }
    } catch (error) {
      this.showToast('Error starting trial', 'error');
    }
  }

  showToast(message, type = 'success') {
    this.toast = { message, type };
    setTimeout(() => {
      this.toast = null;
    }, 4000);
  }

  getLicenseStatus() {
    if (!this.license) return 'unlicensed';
    if (this.license.type === 0) return 'trial'; // Trial
    if (this.license.status === 0) return 'valid'; // Active
    if (this.license.status === 4) return 'grace'; // Grace Period
    return 'expired';
  }

  getTierName() {
    if (!this.license) return 'Unlicensed';
    switch (this.license.type) {
      case 0: return 'Trial';
      case 1: return 'Standard';
      case 2: return 'Enterprise';
      default: return 'Unknown';
    }
  }

  formatDate(dateString) {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  isFeatureEnabled(featureKey) {
    return this.features.includes(featureKey);
  }

  render() {
    if (this.loading) {
      return html`
        <div class="license-container">
          <div class="loading">
            <div class="spinner"></div>
          </div>
        </div>
      `;
    }

    const status = this.getLicenseStatus();
    const tierName = this.getTierName();

    return html`
      <div class="license-container">
        <div class="license-header">
          <h1>License Management</h1>
          <p>Manage your Algora Commerce license and features</p>
        </div>

        <div class="license-content">
          <!-- Current License Status -->
          <div class="status-card ${status}">
            <div class="status-header">
              <div class="status-info">
                <h2>Current License</h2>
                <span class="status-badge ${status}">
                  ${status === 'valid' ? '✓ Active' :
                    status === 'trial' ? '⏱ Trial' :
                    status === 'grace' ? '⚠ Grace Period' :
                    status === 'expired' ? '✕ Expired' : '○ Unlicensed'}
                </span>
              </div>
              <div class="tier-badge ${tierName.toLowerCase()}">${tierName}</div>
            </div>

            ${this.license ? html`
              <div class="license-details">
                <div class="detail-item">
                  <div class="detail-label">License Key</div>
                  <div class="detail-value" style="font-family: monospace;">${this.license.key}</div>
                </div>
                <div class="detail-item">
                  <div class="detail-label">Customer</div>
                  <div class="detail-value">${this.license.customerName}</div>
                </div>
                <div class="detail-item">
                  <div class="detail-label">Email</div>
                  <div class="detail-value">${this.license.customerEmail}</div>
                </div>
                <div class="detail-item">
                  <div class="detail-label">Valid From</div>
                  <div class="detail-value">${this.formatDate(this.license.validFrom)}</div>
                </div>
                <div class="detail-item">
                  <div class="detail-label">Expires</div>
                  <div class="detail-value ${this.license.daysUntilExpiration <= 7 ? 'danger' : this.license.daysUntilExpiration <= 30 ? 'warning' : ''}">
                    ${this.license.isLifetime ? 'Never (Lifetime)' : this.formatDate(this.license.validUntil)}
                    ${this.license.daysUntilExpiration && !this.license.isLifetime ? html`<br><small>${this.license.daysUntilExpiration} days remaining</small>` : ''}
                  </div>
                </div>
                <div class="detail-item">
                  <div class="detail-label">Activations</div>
                  <div class="detail-value">${this.license.activationCount} / ${this.license.maxActivations}</div>
                </div>
              </div>
            ` : html`
              <div class="empty-state">
                <div class="empty-icon">🔐</div>
                <h3>No Active License</h3>
                <p>Enter your license key below or start a 14-day free trial.</p>
              </div>
            `}
          </div>

          <!-- License Key Input -->
          <div class="license-key-section">
            <h3 class="section-title">${this.license ? 'Update License Key' : 'Activate License'}</h3>
            <div class="key-input-group">
              <input
                type="text"
                class="key-input"
                placeholder="Enter your license key (e.g., STD-XXXX-XXXX-XXXX-XXXX)"
                .value=${this.licenseKey}
                @input=${e => this.licenseKey = e.target.value}
              />
              <button class="btn btn-primary" @click=${this.activateLicense} ?disabled=${this.validating}>
                ${this.validating ? 'Validating...' : 'Activate'}
              </button>
              <button class="btn btn-secondary" @click=${this.validateLicenseKey} ?disabled=${this.validating}>
                Validate Only
              </button>
            </div>
          </div>

          <!-- Trial Section (only if unlicensed) -->
          ${!this.license ? html`
            <div class="trial-section">
              <h3>🎁 Start Your Free Trial</h3>
              <p>Try Algora Commerce free for 14 days with full features. No credit card required.</p>
              <button class="btn btn-warning" @click=${this.startTrial}>
                Start 14-Day Trial
              </button>
            </div>
          ` : ''}

          <!-- Features Section -->
          <div class="features-section">
            <h3 class="section-title">Features</h3>
            <div class="features-grid">
              ${this.allFeatures.map(feature => {
                const enabled = this.isFeatureEnabled(feature.key);
                return html`
                  <div class="feature-item ${enabled ? 'enabled' : 'disabled'}">
                    <span class="feature-icon ${enabled ? 'enabled' : 'disabled'}">
                      ${enabled ? '✓' : '✕'}
                    </span>
                    <span>${feature.name}</span>
                  </div>
                `;
              })}
            </div>
          </div>

          <!-- Pricing Section -->
          <div class="pricing-section">
            <h3 class="section-title">Upgrade Your License</h3>
            <div class="pricing-grid">
              <!-- Trial -->
              <div class="pricing-card ${this.license?.type === 0 ? 'current' : ''}">
                <div class="pricing-tier">Trial</div>
                <div class="pricing-price">Free</div>
                <div class="pricing-period">14 days</div>
                <div class="pricing-features">
                  <div class="pricing-feature">1 Store</div>
                  <div class="pricing-feature">100 Products</div>
                  <div class="pricing-feature">50 Orders/month</div>
                  <div class="pricing-feature">Basic Features</div>
                </div>
                ${!this.license ? html`
                  <button class="btn btn-warning" @click=${this.startTrial}>Start Trial</button>
                ` : html`
                  <button class="btn btn-secondary" disabled>Current</button>
                `}
              </div>

              <!-- Standard -->
              <div class="pricing-card recommended ${this.license?.type === 1 ? 'current' : ''}">
                <div class="recommended-badge">Most Popular</div>
                <div class="pricing-tier">Standard</div>
                <div class="pricing-price">$1,500</div>
                <div class="pricing-period">per year</div>
                <div class="pricing-features">
                  <div class="pricing-feature">1 Store</div>
                  <div class="pricing-feature">Unlimited Products</div>
                  <div class="pricing-feature">Unlimited Orders</div>
                  <div class="pricing-feature">Advanced Reporting</div>
                  <div class="pricing-feature">Multi-Currency</div>
                  <div class="pricing-feature">Import/Export</div>
                </div>
                <button class="btn btn-primary" @click=${() => window.open('https://algoracommerce.com/pricing', '_blank')}>
                  ${this.license?.type === 1 ? 'Renew' : 'Upgrade'}
                </button>
              </div>

              <!-- Enterprise -->
              <div class="pricing-card ${this.license?.type === 2 ? 'current' : ''}">
                <div class="pricing-tier">Enterprise</div>
                <div class="pricing-price">$3,000</div>
                <div class="pricing-period">per year</div>
                <div class="pricing-features">
                  <div class="pricing-feature">Unlimited Stores</div>
                  <div class="pricing-feature">All Standard Features</div>
                  <div class="pricing-feature">White Labeling</div>
                  <div class="pricing-feature">Priority Support</div>
                  <div class="pricing-feature">B2B Features</div>
                  <div class="pricing-feature">Custom Integrations</div>
                </div>
                <button class="btn btn-primary" @click=${() => window.open('https://algoracommerce.com/enterprise', '_blank')}>
                  ${this.license?.type === 2 ? 'Renew' : 'Contact Sales'}
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      ${this.toast ? html`
        <div class="toast ${this.toast.type}">${this.toast.message}</div>
      ` : ''}
    `;
  }
}

customElements.define("algora-license-collection", LicenseCollection);
export default LicenseCollection;
