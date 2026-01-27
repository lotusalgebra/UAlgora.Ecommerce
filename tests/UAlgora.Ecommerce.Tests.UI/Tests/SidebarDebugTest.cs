using FluentAssertions;
using OpenQA.Selenium;
using UAlgora.Ecommerce.Tests.UI.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace UAlgora.Ecommerce.Tests.UI.Tests;

/// <summary>
/// Debug tests for the Algora sidebar functionality
/// </summary>
[Collection("Sequential")]
public class SidebarDebugTest : BaseUITest
{
    private readonly ITestOutputHelper _output;

    public SidebarDebugTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    [Trait("Category", "Debug")]
    public void DebugSidebarContent()
    {
        // Navigate to Algora section
        LoginToUmbraco();
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce");
        WaitForPageLoad();
        Thread.Sleep(5000); // Wait for extensions to load

        // Take screenshot
        TakeScreenshot("AlgoraSectionDebug");

        // Log the page source
        var pageSource = Driver.PageSource;
        _output.WriteLine("=== PAGE SOURCE (first 5000 chars) ===");
        _output.WriteLine(pageSource.Substring(0, Math.Min(5000, pageSource.Length)));

        // Check for sidebar element
        var sidebarElements = Driver.FindElements(By.CssSelector("ecommerce-sidebar-tree, .tree-root, umb-section-sidebar"));
        _output.WriteLine($"\nSidebar elements found: {sidebarElements.Count}");

        foreach (var element in sidebarElements)
        {
            _output.WriteLine($"  - Tag: {element.TagName}, Text: {element.Text.Substring(0, Math.Min(100, element.Text.Length))}");
        }

        // Check for tree items
        var treeItems = Driver.FindElements(By.CssSelector(".tree-item, .tree-root > div"));
        _output.WriteLine($"\nTree items found: {treeItems.Count}");

        // Check for section sidebar apps
        var sidebarApps = Driver.FindElements(By.CssSelector("umb-section-sidebar-app, [slot='sidebar']"));
        _output.WriteLine($"\nSidebar apps found: {sidebarApps.Count}");

        // Check console for JavaScript errors
        var logs = Driver.Manage().Logs.GetLog(LogType.Browser);
        _output.WriteLine($"\n=== BROWSER CONSOLE LOGS ===");
        foreach (var log in logs)
        {
            _output.WriteLine($"{log.Level}: {log.Message}");
        }

        // Check if the section is loaded
        var url = Driver.Url;
        _output.WriteLine($"\nCurrent URL: {url}");

        // Verify we're on the ecommerce section
        url.Should().Contain("ecommerce");
    }

    [Fact]
    [Trait("Category", "Debug")]
    public void DebugSectionNavigation()
    {
        LoginToUmbraco();
        WaitForPageLoad();
        Thread.Sleep(3000);

        // Take screenshot of initial state
        TakeScreenshot("AfterLogin");

        // Look for section tabs/links
        var sectionLinks = Driver.FindElements(By.CssSelector("umb-section-main-views, nav a, [href*='section']"));
        _output.WriteLine($"Section links found: {sectionLinks.Count}");

        foreach (var link in sectionLinks.Take(10))
        {
            _output.WriteLine($"  - Tag: {link.TagName}, Href: {link.GetAttribute("href")}, Text: {link.Text}");
        }

        // Look for Algora specifically
        var algoraLinks = Driver.FindElements(By.CssSelector("[href*='ecommerce'], [data-element*='algora'], [title*='Algora']"));
        _output.WriteLine($"\nAlgora-specific links found: {algoraLinks.Count}");

        foreach (var link in algoraLinks)
        {
            _output.WriteLine($"  - Tag: {link.TagName}, Href: {link.GetAttribute("href")}, Text: {link.Text}");
        }

        // Check the header sections
        var headerSections = Driver.FindElements(By.CssSelector("umb-header-app-button, .section-nav, header a, nav li"));
        _output.WriteLine($"\nHeader sections found: {headerSections.Count}");

        foreach (var section in headerSections.Take(20))
        {
            var text = section.Text.Length > 0 ? section.Text : section.GetAttribute("label") ?? section.GetAttribute("title") ?? "";
            _output.WriteLine($"  - Tag: {section.TagName}, Text/Label: {text}");
        }
    }
}
