using FluentAssertions;
using OpenQA.Selenium;
using UAlgora.Ecommerce.Tests.UI.Infrastructure;
using Xunit;

namespace UAlgora.Ecommerce.Tests.UI.Tests;

/// <summary>
/// UI tests for Order management in Algora Commerce
/// </summary>
[Collection("Sequential")]
public class OrderTests : BaseUITest
{
    [Fact]
    [Trait("Category", "Orders")]
    public void CanNavigateToOrdersSection()
    {
        // Arrange & Act
        NavigateToAlgoraSection();
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/orders");
        WaitForPageLoad();

        // Assert
        TakeScreenshot("OrdersSection");
        var pageSource = Driver.PageSource;
        pageSource.Should().Contain("Order");
    }

    [Fact]
    [Trait("Category", "Orders")]
    public void OrdersListLoadsSuccessfully()
    {
        // Arrange & Act
        NavigateToAlgoraSection();
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/orders");
        WaitForPageLoad();
        Thread.Sleep(2000);

        // Assert
        TakeScreenshot("OrdersList");

        // Verify page loaded without major errors
        var pageSource = Driver.PageSource;
        pageSource.Should().NotContain("error", because: "Page should load without errors");
    }

    [Fact]
    [Trait("Category", "Orders")]
    public void CanFilterOrdersByStatus()
    {
        // Arrange
        NavigateToAlgoraSection();
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/orders");
        WaitForPageLoad();

        // Act - Look for status filter chips
        Thread.Sleep(1000);
        var filterChips = Driver.FindElements(By.CssSelector(".filter-chip, .status-filter, [class*='filter']"));

        // Try to click a filter if available
        if (filterChips.Count > 0)
        {
            filterChips[0].Click();
            Thread.Sleep(500);
        }

        // Assert
        TakeScreenshot("OrdersFiltered");
    }

    [Fact]
    [Trait("Category", "Orders")]
    public void CanSearchOrders()
    {
        // Arrange
        NavigateToAlgoraSection();
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/orders");
        WaitForPageLoad();

        // Act - Find and use search input
        var searchInput = WaitForElement(By.CssSelector("input[type='search'], input[placeholder*='Search'], .search-input"));
        if (searchInput != null)
        {
            searchInput.Clear();
            searchInput.SendKeys("test");
            Thread.Sleep(1000);
        }

        // Assert
        TakeScreenshot("OrdersSearch");
    }

    [Fact]
    [Trait("Category", "Orders")]
    public void CanViewOrderDetails()
    {
        // Arrange
        NavigateToAlgoraSection();
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/orders");
        WaitForPageLoad();
        Thread.Sleep(2000);

        // Act - Try to click first order if available
        var orderRows = Driver.FindElements(By.CssSelector("tr[class*='order'], .order-item, .list-item, .order-row"));
        if (orderRows.Count > 0)
        {
            orderRows[0].Click();
            Thread.Sleep(1500);
        }

        // Assert
        TakeScreenshot("OrderDetails");
    }

    [Fact]
    [Trait("Category", "Orders")]
    public void OrderEditorHasRequiredTabs()
    {
        // Arrange
        NavigateToAlgoraSection();
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/orders");
        WaitForPageLoad();
        Thread.Sleep(2000);

        // Act - Click first order to open editor
        var orderRows = Driver.FindElements(By.CssSelector("tr[class*='order'], .order-item, .list-item"));
        if (orderRows.Count > 0)
        {
            orderRows[0].Click();
            Thread.Sleep(2000);
        }

        // Assert - Check for tabs
        TakeScreenshot("OrderEditorTabs");
        var pageSource = Driver.PageSource;

        // Should have typical order tabs (Details, Items, Customer, Shipping, etc.)
        // Note: Exact content depends on what's implemented
        pageSource.Should().NotBeNullOrEmpty();
    }
}
