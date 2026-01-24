namespace UAlgora.Ecommerce.Tests.UI.Configuration;

/// <summary>
/// Test configuration settings loaded from appsettings.json
/// </summary>
public class TestSettings
{
    public string BaseUrl { get; set; } = "http://localhost:5204";
    public string UmbracoUrl { get; set; } = "http://localhost:5204/umbraco";
    public string ApiBaseUrl { get; set; } = "http://localhost:5204/umbraco/management/api/v1/ecommerce";
    public string AdminEmail { get; set; } = "admin@algora.com";
    public string AdminPassword { get; set; } = "Admin123!";
    public bool HeadlessMode { get; set; } = false;
    public int ImplicitWaitSeconds { get; set; } = 10;
    public int ExplicitWaitSeconds { get; set; } = 30;
    public bool ScreenshotOnFailure { get; set; } = true;
    public string ScreenshotPath { get; set; } = "Screenshots";
}
