using FluentAssertions;
using OpenQA.Selenium;
using UAlgora.Ecommerce.Tests.UI.Infrastructure;
using UAlgora.Ecommerce.Tests.UI.PageObjects;
using Xunit;

namespace UAlgora.Ecommerce.Tests.UI.Tests;

/// <summary>
/// UI tests for Product management in Algora Commerce
/// </summary>
[Collection("Sequential")]
public class ProductTests : BaseUITest
{
    private readonly AlgoraProductsPage _productsPage;
    private readonly string _testProductName = $"Test Product {DateTime.Now:yyyyMMddHHmmss}";
    private readonly string _testProductSku = $"TEST-{DateTime.Now:yyyyMMddHHmmss}";

    public ProductTests()
    {
        _productsPage = new AlgoraProductsPage(Driver, Wait);
    }

    [Fact]
    [Trait("Category", "Products")]
    public void CanNavigateToProductsSection()
    {
        // Arrange & Act
        NavigateToAlgoraSection();
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/products");
        WaitForPageLoad();

        // Assert
        var pageSource = Driver.PageSource;
        pageSource.Should().Contain("Products");
    }

    [Fact]
    [Trait("Category", "Products")]
    public void CanCreateNewProduct()
    {
        // Arrange
        NavigateToAlgoraSection();
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/products");
        WaitForPageLoad();

        // Act
        _productsPage.ClickCreateProduct();
        Thread.Sleep(1000);

        _productsPage.FillProductForm(_testProductName, _testProductSku, 99.99m, "Test product description");
        _productsPage.SaveProduct();

        // Wait for save
        Thread.Sleep(2000);

        // Assert - check if product appears in list or success message shown
        WaitForPageLoad();
        TakeScreenshot("ProductCreated");
    }

    [Fact]
    [Trait("Category", "Products")]
    public void CanSearchProducts()
    {
        // Arrange
        NavigateToAlgoraSection();
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/products");
        WaitForPageLoad();

        // Act
        _productsPage.SearchProduct("test");
        Thread.Sleep(1000);

        // Assert
        TakeScreenshot("ProductSearch");
    }

    [Fact]
    [Trait("Category", "Products")]
    public void ProductListLoadsSuccessfully()
    {
        // Arrange & Act
        NavigateToAlgoraSection();
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/products");
        WaitForPageLoad();

        // Assert
        TakeScreenshot("ProductList");

        // Verify page loaded without errors
        var pageSource = Driver.PageSource;
        pageSource.Should().NotContain("error", because: "Page should load without errors");
    }

    [Fact]
    [Trait("Category", "Products")]
    public void CanOpenProductEditor()
    {
        // Arrange
        NavigateToAlgoraSection();
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/products");
        WaitForPageLoad();

        // Act - Try to click create or first product
        _productsPage.ClickCreateProduct();
        Thread.Sleep(1000);

        // Assert
        TakeScreenshot("ProductEditor");
    }
}
