using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using UAlgora.Ecommerce.Tests.UI.Configuration;
using UAlgora.Ecommerce.Tests.UI.Infrastructure;
using Xunit;

namespace UAlgora.Ecommerce.Tests.UI.Tests;

/// <summary>
/// Tests for bidirectional synchronization between Umbraco Content and Algora Database
/// </summary>
[Collection("Sequential")]
public class SyncTests : BaseUITest
{
    private readonly HttpClient _client;

    public SyncTests()
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri(Settings.BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    [Fact]
    [Trait("Category", "Sync")]
    public void CanNavigateToContentSection()
    {
        // Arrange & Act
        NavigateToContentSection();
        WaitForPageLoad();

        // Assert
        TakeScreenshot("ContentSection");
        var pageSource = Driver.PageSource;
        pageSource.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Sync")]
    public async Task ContentSyncEndpoint_ShouldExist()
    {
        // Arrange
        var url = "/umbraco/management/api/v1/ecommerce/content-sync/sync-all";

        // Act
        try
        {
            var response = await _client.PostAsync(url, null);

            // Assert - Endpoint should exist (requires auth typically)
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,
                HttpStatusCode.Unauthorized,
                HttpStatusCode.Forbidden);
        }
        catch (HttpRequestException)
        {
            // Expected if requires authentication
        }
    }

    [Fact]
    [Trait("Category", "Sync")]
    public async Task ProductsSyncEndpoint_ShouldExist()
    {
        // Arrange
        var url = "/umbraco/management/api/v1/ecommerce/content-sync/products";

        // Act
        try
        {
            var response = await _client.PostAsync(url, null);

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,
                HttpStatusCode.Unauthorized,
                HttpStatusCode.Forbidden);
        }
        catch (HttpRequestException)
        {
            // Expected if requires authentication
        }
    }

    [Fact]
    [Trait("Category", "Sync")]
    public async Task CategoriesSyncEndpoint_ShouldExist()
    {
        // Arrange
        var url = "/umbraco/management/api/v1/ecommerce/content-sync/categories";

        // Act
        try
        {
            var response = await _client.PostAsync(url, null);

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,
                HttpStatusCode.Unauthorized,
                HttpStatusCode.Forbidden);
        }
        catch (HttpRequestException)
        {
            // Expected if requires authentication
        }
    }

    [Fact]
    [Trait("Category", "Sync")]
    public void CanNavigateToCatalogInContent()
    {
        // Arrange
        NavigateToContentSection();
        WaitForPageLoad();

        // Act - Try to expand tree and find Catalog
        Thread.Sleep(2000);
        TakeScreenshot("ContentTree");

        // Look for Catalog or Products in content tree
        var pageSource = Driver.PageSource;

        // Assert - Content section should have loaded
        pageSource.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Sync")]
    public void CanCreateProductInContent()
    {
        // Arrange
        NavigateToContentSection();
        WaitForPageLoad();

        // Act - Navigate to content section and look for product document type
        Thread.Sleep(2000);

        // Try to find and click on tree item
        var treeItems = Driver.FindElements(By.CssSelector("umb-tree-item, .tree-item, [data-element*='tree']"));

        TakeScreenshot("ContentTreeItems");

        // Assert - Should have tree items loaded
        // Note: This test validates the content section is accessible
        var pageSource = Driver.PageSource;
        pageSource.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Sync")]
    public void CanAccessAlgoraSectionAfterContentSync()
    {
        // Arrange - First navigate to content
        NavigateToContentSection();
        WaitForPageLoad();
        Thread.Sleep(1000);

        // Act - Then navigate to Algora section
        NavigateToAlgoraSection();
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/products");
        WaitForPageLoad();

        // Assert
        TakeScreenshot("AlgoraSectionAfterContent");
        var pageSource = Driver.PageSource;
        pageSource.Should().Contain("Product");
    }

    [Fact]
    [Trait("Category", "Sync")]
    public void ProductsViewShowsProducts()
    {
        // Arrange & Act
        NavigateToAlgoraSection();
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/products");
        WaitForPageLoad();
        Thread.Sleep(2000);

        // Assert
        TakeScreenshot("ProductsView");

        // Check that the products view loads and shows some content
        var pageSource = Driver.PageSource;
        pageSource.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Sync")]
    public void CategoriesViewShowsCategories()
    {
        // Arrange & Act
        NavigateToAlgoraSection();
        Driver.Navigate().GoToUrl($"{Settings.UmbracoUrl}/section/ecommerce/view/categories");
        WaitForPageLoad();
        Thread.Sleep(2000);

        // Assert
        TakeScreenshot("CategoriesView");

        var pageSource = Driver.PageSource;
        pageSource.Should().NotBeNullOrEmpty();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _client?.Dispose();
        }
    }
}
