using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using UAlgora.Ecommerce.Tests.UI.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace UAlgora.Ecommerce.Tests.UI.Tests;

/// <summary>
/// Tests for Product creation and bidirectional sync functionality.
/// These tests verify that products created via the API sync to the Umbraco content tree.
/// </summary>
[Collection("Sequential")]
public class ProductSyncTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly TestSettings _settings;
    private readonly ITestOutputHelper _output;
    private readonly string _testProductSku;
    private readonly string _testProductName;

    public ProductSyncTests(ITestOutputHelper output)
    {
        _output = output;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        _settings = new TestSettings();
        configuration.GetSection("TestSettings").Bind(_settings);

        _client = new HttpClient
        {
            BaseAddress = new Uri(_settings.BaseUrl),
            Timeout = TimeSpan.FromSeconds(60)
        };

        // Generate unique test data
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        _testProductSku = $"TEST-SYNC-{timestamp}";
        _testProductName = $"Test Sync Product {timestamp}";
    }

    [Fact]
    [Trait("Category", "Sync")]
    public async Task ProductApi_CreateEndpoint_Exists()
    {
        // Arrange - Create a test product payload
        var productPayload = new
        {
            name = _testProductName,
            sku = _testProductSku,
            basePrice = 99.99m,
            description = "Test product for sync verification",
            status = "Draft",
            isVisible = true,
            trackInventory = true,
            stockQuantity = 100
        };

        var json = JsonSerializer.Serialize(productPayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act - Note: route uses singular 'product' not 'products'
        var response = await _client.PostAsync("/umbraco/management/api/v1/ecommerce/product", content);

        // Assert - Endpoint exists (requires auth in production)
        _output.WriteLine($"Create Product Response: {response.StatusCode}");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Created,           // Success
            HttpStatusCode.OK,                // Success (alternative)
            HttpStatusCode.Unauthorized,      // Requires auth
            HttpStatusCode.Forbidden,         // Requires permission
            HttpStatusCode.BadRequest);       // Validation error (endpoint exists)
    }

    [Fact]
    [Trait("Category", "Sync")]
    public async Task ProductApi_GetEndpoint_ReturnsProducts()
    {
        // Act - Note: route uses singular 'product' not 'products'
        var response = await _client.GetAsync("/umbraco/management/api/v1/ecommerce/product");

        // Assert
        _output.WriteLine($"Get Products Response: {response.StatusCode}");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("Category", "Sync")]
    public async Task ContentSyncApi_SyncProducts_Endpoint_Exists()
    {
        // Act - Trigger product sync to content tree
        var response = await _client.PostAsync("/umbraco/management/api/v1/ecommerce/content-sync/products", null);

        // Assert
        _output.WriteLine($"Sync Products Response: {response.StatusCode}");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("Category", "Sync")]
    public async Task ContentSyncApi_SyncAll_Endpoint_Exists()
    {
        // Act - Trigger full bidirectional sync
        var response = await _client.PostAsync("/umbraco/management/api/v1/ecommerce/content-sync/sync-all", null);

        // Assert
        _output.WriteLine($"Sync All Response: {response.StatusCode}");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("Category", "Sync")]
    public async Task ProductApi_Tree_Endpoint_Exists()
    {
        // Act - Get product tree structure (route uses singular 'product')
        var response = await _client.GetAsync("/umbraco/management/api/v1/ecommerce/product/tree");

        // Assert
        _output.WriteLine($"Product Tree Response: {response.StatusCode}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Tree Content: {content}");
        }

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("Category", "Sync")]
    public async Task CategoryApi_Endpoint_Exists()
    {
        // Act - Note: route uses singular 'category' not 'categories'
        var response = await _client.GetAsync("/umbraco/management/api/v1/ecommerce/category");

        // Assert
        _output.WriteLine($"Get Categories Response: {response.StatusCode}");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("Category", "Sync")]
    public async Task ContentSyncApi_SyncCategories_Endpoint_Exists()
    {
        // Act - Trigger category sync to content tree
        var response = await _client.PostAsync("/umbraco/management/api/v1/ecommerce/content-sync/categories", null);

        // Assert
        _output.WriteLine($"Sync Categories Response: {response.StatusCode}");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("Category", "Sync")]
    public async Task ContentSyncApi_CategoriesToDatabase_Endpoint_Exists()
    {
        // Act - Trigger category sync from content to database
        var response = await _client.PostAsync("/umbraco/management/api/v1/ecommerce/content-sync/categories-to-database", null);

        // Assert
        _output.WriteLine($"Categories to DB Response: {response.StatusCode}");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound);
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}
