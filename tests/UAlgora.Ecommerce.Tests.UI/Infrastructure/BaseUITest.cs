using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using UAlgora.Ecommerce.Tests.UI.Configuration;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace UAlgora.Ecommerce.Tests.UI.Infrastructure;

/// <summary>
/// Base class for all UI tests providing WebDriver setup and common utilities
/// </summary>
public abstract class BaseUITest : IDisposable
{
    protected IWebDriver Driver { get; private set; } = null!;
    protected WebDriverWait Wait { get; private set; } = null!;
    protected TestSettings Settings { get; private set; } = null!;

    private bool _isLoggedIn = false;

    protected BaseUITest()
    {
        LoadConfiguration();
        InitializeDriver();
    }

    private void LoadConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        Settings = new TestSettings();
        configuration.GetSection("TestSettings").Bind(Settings);
    }

    private void InitializeDriver()
    {
        // Use WebDriverManager to automatically download matching ChromeDriver
        new DriverManager().SetUpDriver(new ChromeConfig(), WebDriverManager.Helpers.VersionResolveStrategy.MatchingBrowser);

        var options = new ChromeOptions();

        if (Settings.HeadlessMode)
        {
            options.AddArgument("--headless=new");
        }

        options.AddArgument("--start-maximized");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-extensions");
        options.AddArgument("--disable-popup-blocking");
        options.AddArgument("--window-size=1920,1080");
        options.AddArgument("--remote-allow-origins=*");

        Driver = new ChromeDriver(options);
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Settings.ImplicitWaitSeconds);
        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(Settings.ExplicitWaitSeconds));
    }

    /// <summary>
    /// Login to Umbraco backoffice
    /// </summary>
    protected void LoginToUmbraco()
    {
        if (_isLoggedIn) return;

        Driver.Navigate().GoToUrl(Settings.UmbracoUrl);
        WaitForPageLoad();
        Thread.Sleep(2000); // Wait for Umbraco 15 SPA to fully load

        // Check if already logged in by looking for section menu or backoffice header
        try
        {
            var sectionMenu = Driver.FindElements(By.CssSelector("umb-backoffice-header, umb-section-sidebar, [data-element='section']"));
            if (sectionMenu.Count > 0)
            {
                _isLoggedIn = true;
                return;
            }
        }
        catch { }

        // Umbraco 15 uses web components, wait for them to render
        Thread.Sleep(2000);

        // Try multiple selectors for Umbraco 15 login form
        var emailSelectors = new[] {
            "uui-input[name='email'] input",
            "uui-input[type='email'] input",
            "input[name='email']",
            "input[type='email']",
            "#email",
            "input[id*='email']",
            "umb-login-input input"
        };

        IWebElement? emailInput = null;
        foreach (var selector in emailSelectors)
        {
            emailInput = WaitForElement(By.CssSelector(selector));
            if (emailInput != null) break;
        }

        if (emailInput != null)
        {
            emailInput.Clear();
            emailInput.SendKeys(Settings.AdminEmail);
        }

        var passwordSelectors = new[] {
            "uui-input[name='password'] input",
            "uui-input[type='password'] input",
            "input[name='password']",
            "input[type='password']",
            "#password",
            "input[id*='password']"
        };

        IWebElement? passwordInput = null;
        foreach (var selector in passwordSelectors)
        {
            passwordInput = WaitForElement(By.CssSelector(selector));
            if (passwordInput != null) break;
        }

        if (passwordInput != null)
        {
            passwordInput.Clear();
            passwordInput.SendKeys(Settings.AdminPassword);
        }

        // Try multiple selectors for Umbraco 15 login button
        var buttonSelectors = new[] {
            "uui-button[type='submit']",
            "uui-button[label='Login']",
            "uui-button[look='primary']",
            "button[type='submit']",
            "button.uui-button",
            ".btn-login",
            "umb-button-login",
            "[data-element='button-login']",
            "form button"
        };

        bool clicked = false;
        foreach (var selector in buttonSelectors)
        {
            try
            {
                var loginButton = WaitForClickable(By.CssSelector(selector));
                if (loginButton != null)
                {
                    // Try JavaScript click as Umbraco 15 web components can be tricky
                    ExecuteScript("arguments[0].click();", loginButton);
                    clicked = true;
                    break;
                }
            }
            catch { }
        }

        // If no button found via selectors, try to find any button with "Login" text
        if (!clicked)
        {
            try
            {
                // Try XPath to find button containing "Login" text
                var loginByXpath = Driver.FindElements(By.XPath("//button[contains(text(), 'Login')] | //uui-button[contains(text(), 'Login')] | //*[contains(@label, 'Login')]"));
                if (loginByXpath.Count > 0)
                {
                    ExecuteScript("arguments[0].click();", loginByXpath[0]);
                    clicked = true;
                }
            }
            catch { }
        }

        // Final fallback - submit the form directly
        if (!clicked)
        {
            try
            {
                ExecuteScript("document.querySelector('form')?.submit();");
            }
            catch { }
        }

        // Wait for dashboard to load
        Thread.Sleep(5000);
        WaitForPageLoad();
        _isLoggedIn = true;
    }

    /// <summary>
    /// Navigate to Algora Commerce section
    /// </summary>
    protected void NavigateToAlgoraSection()
    {
        LoginToUmbraco();

        // Click on Algora section in the sidebar
        var sectionLink = WaitForClickable(By.CssSelector("a[href*='ecommerce'], [data-element='section-algora'], umb-section-sidebar-item[label='Algora']"));
        sectionLink?.Click();

        WaitForPageLoad();
    }

    /// <summary>
    /// Navigate to a specific view in Algora section
    /// </summary>
    protected void NavigateToAlgoraView(string viewName)
    {
        NavigateToAlgoraSection();

        // Click on the specific view (Products, Categories, Orders, etc.)
        var viewLink = WaitForClickable(By.CssSelector($"a[href*='{viewName.ToLower()}'], [data-element='tree-item-{viewName.ToLower()}']"));
        viewLink?.Click();

        WaitForPageLoad();
    }

    /// <summary>
    /// Navigate to Content section
    /// </summary>
    protected void NavigateToContentSection()
    {
        LoginToUmbraco();

        var contentLink = WaitForClickable(By.CssSelector("a[href*='content'], [data-element='section-content']"));
        contentLink?.Click();

        WaitForPageLoad();
    }

    /// <summary>
    /// Wait for page to fully load
    /// </summary>
    protected void WaitForPageLoad()
    {
        Wait.Until(driver => ((IJavaScriptExecutor)driver)
            .ExecuteScript("return document.readyState").Equals("complete"));
        Thread.Sleep(500); // Small buffer for JS frameworks
    }

    /// <summary>
    /// Wait for element to be visible
    /// </summary>
    protected IWebElement? WaitForElement(By locator)
    {
        try
        {
            return Wait.Until(d => d.FindElement(locator));
        }
        catch (WebDriverTimeoutException)
        {
            return null;
        }
    }

    /// <summary>
    /// Wait for element to be clickable
    /// </summary>
    protected IWebElement? WaitForClickable(By locator)
    {
        try
        {
            return Wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(locator));
        }
        catch (WebDriverTimeoutException)
        {
            return null;
        }
    }

    /// <summary>
    /// Find element with error handling
    /// </summary>
    protected IWebElement FindElement(By locator)
    {
        return Driver.FindElement(locator);
    }

    /// <summary>
    /// Find elements with error handling
    /// </summary>
    protected IReadOnlyCollection<IWebElement> FindElements(By locator)
    {
        return Driver.FindElements(locator);
    }

    /// <summary>
    /// Execute JavaScript
    /// </summary>
    protected object? ExecuteScript(string script, params object[] args)
    {
        return ((IJavaScriptExecutor)Driver).ExecuteScript(script, args);
    }

    /// <summary>
    /// Take screenshot
    /// </summary>
    protected void TakeScreenshot(string name)
    {
        try
        {
            var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
            var directory = Path.Combine(Directory.GetCurrentDirectory(), Settings.ScreenshotPath);
            Directory.CreateDirectory(directory);
            var filePath = Path.Combine(directory, $"{name}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
            screenshot.SaveAsFile(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to take screenshot: {ex.Message}");
        }
    }

    /// <summary>
    /// Fill input field
    /// </summary>
    protected void FillInput(By locator, string value)
    {
        var element = WaitForElement(locator);
        if (element != null)
        {
            element.Clear();
            element.SendKeys(value);
        }
    }

    /// <summary>
    /// Click element
    /// </summary>
    protected void Click(By locator)
    {
        var element = WaitForClickable(locator);
        element?.Click();
    }

    /// <summary>
    /// Check if element exists
    /// </summary>
    protected bool ElementExists(By locator)
    {
        try
        {
            Driver.FindElement(locator);
            return true;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    /// <summary>
    /// Get text from element
    /// </summary>
    protected string GetText(By locator)
    {
        var element = WaitForElement(locator);
        return element?.Text ?? string.Empty;
    }

    public void Dispose()
    {
        if (Settings.ScreenshotOnFailure)
        {
            TakeScreenshot("TestDispose");
        }

        Driver?.Quit();
        Driver?.Dispose();
    }
}
