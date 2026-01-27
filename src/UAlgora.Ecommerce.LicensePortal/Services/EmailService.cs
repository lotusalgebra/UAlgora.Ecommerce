using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.LicensePortal.Models;

namespace UAlgora.Ecommerce.LicensePortal.Services;

/// <summary>
/// Email service implementation using SendGrid.
/// </summary>
public class EmailService : IEmailService
{
    private readonly LicensePortalOptions _options;
    private readonly ILogger<EmailService> _logger;
    private readonly SendGridClient? _client;
    private readonly bool _isConfigured;

    public EmailService(
        IOptions<LicensePortalOptions> options,
        ILogger<EmailService> logger)
    {
        _options = options.Value;
        _logger = logger;

        // Check if SendGrid is configured
        var apiKey = _options.Email?.SendGridApiKey;
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _client = new SendGridClient(apiKey);
            _isConfigured = true;
        }
        else
        {
            _logger.LogWarning("SendGrid API key is not configured. Emails will be logged but not sent.");
            _isConfigured = false;
        }
    }

    public async Task SendLicensePurchasedEmailAsync(License license)
    {
        var subject = $"Your Algora Commerce {license.Type} License";

        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .license-key {{ background: #fff; border: 2px dashed #667eea; padding: 20px; text-align: center; margin: 20px 0; border-radius: 8px; }}
        .license-key code {{ font-size: 18px; font-weight: bold; color: #667eea; letter-spacing: 1px; }}
        .details {{ background: #fff; padding: 15px; border-radius: 8px; margin: 15px 0; }}
        .details p {{ margin: 8px 0; }}
        .button {{ display: inline-block; background: #667eea; color: white !important; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #888; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Welcome to Algora Commerce!</h1>
            <p>Your {license.Type} license is ready</p>
        </div>
        <div class='content'>
            <p>Hi {license.CustomerName},</p>
            <p>Thank you for purchasing Algora Commerce! Your license has been activated and is ready to use.</p>

            <div class='license-key'>
                <p style='margin: 0 0 10px 0; color: #666;'>Your License Key:</p>
                <code>{license.Key}</code>
            </div>

            <div class='details'>
                <p><strong>License Type:</strong> {license.Type}</p>
                <p><strong>Valid From:</strong> {license.ValidFrom:MMMM dd, yyyy}</p>
                <p><strong>Valid Until:</strong> {license.ValidUntil:MMMM dd, yyyy}</p>
                <p><strong>Auto-Renewal:</strong> {(license.AutoRenew ? "Enabled" : "Disabled")}</p>
            </div>

            <h3>Getting Started</h3>
            <ol>
                <li>Open your Umbraco backoffice</li>
                <li>Go to Settings → Algora Commerce → License</li>
                <li>Enter your license key</li>
                <li>Click 'Activate'</li>
            </ol>

            <p style='text-align: center;'>
                <a href='https://docs.algoracommerce.com/getting-started' class='button'>View Documentation</a>
            </p>

            <p>If you have any questions, our support team is here to help!</p>

            <p>Best regards,<br>The Algora Commerce Team</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.UtcNow.Year} Algora Commerce. All rights reserved.</p>
            <p>You received this email because you purchased an Algora Commerce license.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(license.CustomerEmail, license.CustomerName, subject, html);
    }

    public async Task SendLicenseRenewedEmailAsync(License license)
    {
        var subject = "Your Algora Commerce License Has Been Renewed";

        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .details {{ background: #fff; padding: 15px; border-radius: 8px; margin: 15px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #888; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✅ License Renewed</h1>
            <p>Your subscription has been renewed successfully</p>
        </div>
        <div class='content'>
            <p>Hi {license.CustomerName},</p>
            <p>Your Algora Commerce {license.Type} license has been automatically renewed. Thank you for your continued trust in our platform!</p>

            <div class='details'>
                <p><strong>License Key:</strong> {license.Key}</p>
                <p><strong>New Expiry Date:</strong> {license.ValidUntil:MMMM dd, yyyy}</p>
                <p><strong>Amount Charged:</strong> ${license.RenewalAmount} {license.RenewalCurrency}</p>
            </div>

            <p>Your license will continue to work without any interruption. No action is needed on your part.</p>

            <p>Best regards,<br>The Algora Commerce Team</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.UtcNow.Year} Algora Commerce. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(license.CustomerEmail, license.CustomerName, subject, html);
    }

    public async Task SendLicenseExpiringSoonEmailAsync(License license, int daysUntilExpiry)
    {
        var subject = $"Your Algora Commerce License Expires in {daysUntilExpiry} Days";

        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .warning {{ background: #fff3cd; border: 1px solid #ffc107; padding: 15px; border-radius: 8px; margin: 15px 0; }}
        .button {{ display: inline-block; background: #f5576c; color: white !important; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #888; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>⚠️ License Expiring Soon</h1>
            <p>{daysUntilExpiry} days remaining</p>
        </div>
        <div class='content'>
            <p>Hi {license.CustomerName},</p>

            <div class='warning'>
                <p><strong>Your Algora Commerce {license.Type} license will expire on {license.ValidUntil:MMMM dd, yyyy}.</strong></p>
            </div>

            {(license.AutoRenew
                ? "<p>Don't worry! Your license is set to auto-renew. Your payment method will be charged automatically and your license will continue without interruption.</p>"
                : "<p>To continue using Algora Commerce after this date, please renew your license.</p><p style='text-align: center;'><a href='https://licenses.algoracommerce.com/account' class='button'>Renew Now</a></p>"
            )}

            <p>Best regards,<br>The Algora Commerce Team</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.UtcNow.Year} Algora Commerce. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(license.CustomerEmail, license.CustomerName, subject, html);
    }

    public async Task SendPaymentFailedEmailAsync(License license, string reason)
    {
        var subject = "Action Required: Payment Failed for Your Algora Commerce License";

        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #f5af19 0%, #f12711 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .error {{ background: #f8d7da; border: 1px solid #f5c6cb; padding: 15px; border-radius: 8px; margin: 15px 0; }}
        .button {{ display: inline-block; background: #f12711; color: white !important; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #888; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>❌ Payment Failed</h1>
            <p>Action required to continue your subscription</p>
        </div>
        <div class='content'>
            <p>Hi {license.CustomerName},</p>
            <p>We were unable to process your payment for your Algora Commerce {license.Type} license renewal.</p>

            <div class='error'>
                <p><strong>Reason:</strong> {reason}</p>
            </div>

            <p>Please update your payment method to avoid any interruption to your service.</p>

            <p style='text-align: center;'>
                <a href='https://licenses.algoracommerce.com/account/payment' class='button'>Update Payment Method</a>
            </p>

            <p>If you have any questions, please contact our support team.</p>

            <p>Best regards,<br>The Algora Commerce Team</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.UtcNow.Year} Algora Commerce. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(license.CustomerEmail, license.CustomerName, subject, html);
    }

    public async Task SendSubscriptionCancelledEmailAsync(License license)
    {
        var subject = "Your Algora Commerce Subscription Has Been Cancelled";

        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #606c88 0%, #3f4c6b 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .info {{ background: #e7f3ff; border: 1px solid #b6d4fe; padding: 15px; border-radius: 8px; margin: 15px 0; }}
        .button {{ display: inline-block; background: #3f4c6b; color: white !important; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #888; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Subscription Cancelled</h1>
            <p>We're sorry to see you go</p>
        </div>
        <div class='content'>
            <p>Hi {license.CustomerName},</p>
            <p>Your Algora Commerce {license.Type} subscription has been cancelled as requested.</p>

            <div class='info'>
                <p><strong>Your license will remain active until:</strong> {license.ValidUntil:MMMM dd, yyyy}</p>
                <p>You can continue to use all features until this date.</p>
            </div>

            <p>Changed your mind? You can resubscribe anytime!</p>

            <p style='text-align: center;'>
                <a href='https://licenses.algoracommerce.com/pricing' class='button'>Resubscribe</a>
            </p>

            <p>We'd love to hear your feedback on how we can improve. Feel free to reply to this email.</p>

            <p>Best regards,<br>The Algora Commerce Team</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.UtcNow.Year} Algora Commerce. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(license.CustomerEmail, license.CustomerName, subject, html);
    }

    public async Task SendLoginCodeAsync(string email, string code)
    {
        var subject = "Your Algora Commerce Login Code";

        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .code-box {{ background: #fff; border: 2px solid #667eea; padding: 30px; text-align: center; margin: 20px 0; border-radius: 8px; }}
        .code {{ font-size: 36px; font-weight: bold; color: #667eea; letter-spacing: 8px; font-family: monospace; }}
        .footer {{ text-align: center; padding: 20px; color: #888; font-size: 12px; }}
        .warning {{ color: #666; font-size: 13px; margin-top: 15px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Login Code</h1>
            <p>Secure access to your account</p>
        </div>
        <div class='content'>
            <p>Hi,</p>
            <p>Use the following code to sign in to your Algora Commerce account:</p>

            <div class='code-box'>
                <div class='code'>{code}</div>
                <p class='warning'>This code expires in 10 minutes</p>
            </div>

            <p>If you didn't request this code, you can safely ignore this email.</p>

            <p>Best regards,<br>The Algora Commerce Team</p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.UtcNow.Year} Algora Commerce. All rights reserved.</p>
            <p>This is an automated security email.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(email, email, subject, html);
    }

    private async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlContent)
    {
        // If SendGrid is not configured, just log the email details
        if (!_isConfigured || _client == null)
        {
            _logger.LogWarning("Email not sent (SendGrid not configured). To: {Email}, Subject: {Subject}", toEmail, subject);
            // In development, log the content for debugging
            _logger.LogDebug("Email content for {Email}:\n{Content}", toEmail, htmlContent);
            await Task.CompletedTask;
            return;
        }

        var fromEmail = _options.Email?.FromEmail ?? "noreply@algoracommerce.com";
        var fromName = _options.Email?.FromName ?? "Algora Commerce";
        var from = new EmailAddress(fromEmail, fromName);
        var to = new EmailAddress(toEmail, toName);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);

        try
        {
            var response = await _client.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Sent email to {Email}: {Subject}", toEmail, subject);
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send email to {Email}: {StatusCode} - {Body}",
                    toEmail, response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email}", toEmail);
        }
    }
}
