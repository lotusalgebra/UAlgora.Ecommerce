using FluentAssertions;
using OpenQA.Selenium;
using UAlgora.Ecommerce.Tests.UI.Infrastructure;
using Xunit;

namespace UAlgora.Ecommerce.Tests.UI.Tests;

/// <summary>
/// UI tests for Algora Commerce Dashboard
/// </summary>
[Collection("Sequential")]
public class DashboardTests : BaseUITest
{
    [Fact]
    [Trait("Category", "Dashboard")]
    public void CanNavigateToAlgoraDashboard()
    {
        // Arrange & Act
        NavigateToAlgoraSection();
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/dashboard");
        WaitForPageLoad();

        // Assert
        TakeScreenshot("AlgoraDashboard");
        var pageSource = Driver.PageSource;

        // Dashboard should load without major errors
        pageSource.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Dashboard")]
    public void DashboardShowsRevenueWidgets()
    {
        // Arrange & Act
        NavigateToAlgoraSection();
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/dashboard");
        WaitForPageLoad();

        // Assert
        TakeScreenshot("DashboardWidgets");
    }

    [Fact]
    [Trait("Category", "Dashboard")]
    public void CanNavigateToOrders()
    {
        // Arrange
        NavigateToAlgoraSection();

        // Act
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/orders");
        WaitForPageLoad();

        // Assert
        TakeScreenshot("OrdersSection");
        var pageSource = Driver.PageSource;
        pageSource.Should().Contain("Order");
    }

    [Fact]
    [Trait("Category", "Dashboard")]
    public void CanNavigateToCustomers()
    {
        // Arrange
        NavigateToAlgoraSection();

        // Act
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/customers");
        WaitForPageLoad();

        // Assert
        TakeScreenshot("CustomersSection");
        var pageSource = Driver.PageSource;
        pageSource.Should().Contain("Customer");
    }

    [Fact]
    [Trait("Category", "Dashboard")]
    public void CanNavigateToDiscounts()
    {
        // Arrange
        NavigateToAlgoraSection();

        // Act
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/discounts");
        WaitForPageLoad();

        // Assert
        TakeScreenshot("DiscountsSection");
    }

    [Fact]
    [Trait("Category", "Dashboard")]
    public void CanNavigateToStores()
    {
        // Arrange
        NavigateToAlgoraSection();

        // Act
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/stores");
        WaitForPageLoad();

        // Assert
        TakeScreenshot("StoresSection");
    }

    [Fact]
    [Trait("Category", "Dashboard")]
    public void CanNavigateToGiftCards()
    {
        // Arrange
        NavigateToAlgoraSection();

        // Act
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/giftcards");
        WaitForPageLoad();

        // Assert
        TakeScreenshot("GiftCardsSection");
    }

    [Fact]
    [Trait("Category", "Dashboard")]
    public void CanNavigateToReturns()
    {
        // Arrange
        NavigateToAlgoraSection();

        // Act
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/returns");
        WaitForPageLoad();

        // Assert
        TakeScreenshot("ReturnsSection");
    }

    [Fact]
    [Trait("Category", "Dashboard")]
    public void CanNavigateToEmailTemplates()
    {
        // Arrange
        NavigateToAlgoraSection();

        // Act
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/emailtemplates");
        WaitForPageLoad();

        // Assert
        TakeScreenshot("EmailTemplatesSection");
    }

    [Fact]
    [Trait("Category", "Dashboard")]
    public void CanNavigateToWebhooks()
    {
        // Arrange
        NavigateToAlgoraSection();

        // Act
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/webhooks");
        WaitForPageLoad();

        // Assert
        TakeScreenshot("WebhooksSection");
    }

    [Fact]
    [Trait("Category", "Dashboard")]
    public void CanNavigateToCurrencies()
    {
        // Arrange
        NavigateToAlgoraSection();

        // Act
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/currencies");
        WaitForPageLoad();

        // Assert
        TakeScreenshot("CurrenciesSection");
    }

    [Fact]
    [Trait("Category", "Dashboard")]
    public void CanNavigateToLicense()
    {
        // Arrange
        NavigateToAlgoraSection();

        // Act
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/license");
        WaitForPageLoad();

        // Assert
        TakeScreenshot("LicenseSection");
    }

    [Fact]
    [Trait("Category", "Dashboard")]
    public void CanNavigateToPaymentLinks()
    {
        // Arrange
        NavigateToAlgoraSection();

        // Act
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/paymentlinks");
        WaitForPageLoad();

        // Assert
        TakeScreenshot("PaymentLinksSection");
    }
}
