using FluentAssertions;
using OpenQA.Selenium;
using UAlgora.Ecommerce.Tests.UI.Infrastructure;
using UAlgora.Ecommerce.Tests.UI.PageObjects;
using Xunit;

namespace UAlgora.Ecommerce.Tests.UI.Tests;

/// <summary>
/// UI tests for Category management in Algora Commerce
/// </summary>
[Collection("Sequential")]
public class CategoryTests : BaseUITest
{
    private readonly AlgoraCategoriesPage _categoriesPage;
    private readonly string _testCategoryName = $"Test Category {DateTime.Now:yyyyMMddHHmmss}";
    private readonly string _testCategorySlug = $"test-category-{DateTime.Now:yyyyMMddHHmmss}";

    public CategoryTests()
    {
        _categoriesPage = new AlgoraCategoriesPage(Driver, Wait);
    }

    [Fact]
    [Trait("Category", "Categories")]
    public void CanNavigateToCategoriesSection()
    {
        // Arrange & Act
        NavigateToAlgoraSection();
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/categories");
        WaitForPageLoad();

        // Assert
        var pageSource = Driver.PageSource;
        pageSource.Should().Contain("Categor");
    }

    [Fact]
    [Trait("Category", "Categories")]
    public void CanCreateNewCategory()
    {
        // Arrange
        NavigateToAlgoraSection();
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/categories");
        WaitForPageLoad();

        // Act
        _categoriesPage.ClickCreateCategory();
        Thread.Sleep(1000);

        _categoriesPage.FillCategoryForm(_testCategoryName, _testCategorySlug, "Test category description");
        _categoriesPage.SaveCategory();

        // Wait for save
        Thread.Sleep(2000);

        // Assert
        WaitForPageLoad();
        TakeScreenshot("CategoryCreated");
    }

    [Fact]
    [Trait("Category", "Categories")]
    public void CategoryListLoadsSuccessfully()
    {
        // Arrange & Act
        NavigateToAlgoraSection();
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/categories");
        WaitForPageLoad();

        // Assert
        TakeScreenshot("CategoryList");

        // Verify page loaded without errors
        var pageSource = Driver.PageSource;
        pageSource.Should().NotContain("error", because: "Page should load without errors");
    }

    [Fact]
    [Trait("Category", "Categories")]
    public void CanOpenCategoryEditor()
    {
        // Arrange
        NavigateToAlgoraSection();
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/categories");
        WaitForPageLoad();

        // Act - Try to click create or first category
        _categoriesPage.ClickCreateCategory();
        Thread.Sleep(1000);

        // Assert
        TakeScreenshot("CategoryEditor");
    }

    [Fact]
    [Trait("Category", "Categories")]
    public void CanSearchCategories()
    {
        // Arrange
        NavigateToAlgoraSection();
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/categories");
        WaitForPageLoad();

        // Act
        _categoriesPage.SearchCategory("test");
        Thread.Sleep(1000);

        // Assert
        TakeScreenshot("CategorySearch");
    }
}
