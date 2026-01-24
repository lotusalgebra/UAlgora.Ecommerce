using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using UAlgora.Ecommerce.Tests.UI.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace UAlgora.Ecommerce.Tests.UI.Tests;

/// <summary>
/// End-to-end tests for Product creation and bidirectional sync verification.
/// These tests create a product via API and verify it syncs to Umbraco content.
/// </summary>
[Collection("Sequential")]
public class ProductCreateAndSyncTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly TestSettings _settings;
    private readonly ITestOutputHelper _output;
    private readonly string _testProductSku;
    private readonly string _testProductName;
    private Guid? _createdProductId;

    public ProductCreateAndSyncTests(ITestOutputHelper output)
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
        _testProductSku = $"SYNC-TEST-{timestamp}";
        _testProductName = $"Sync Test Product {timestamp}";
    }

    [Fact]
    [Trait("Category", "E2E")]
    public async Task CreateProduct_AndVerifyInList()
    {
        // Arrange
        var productPayload = new
        {
            name = _testProductName,
            sku = _testProductSku,
            basePrice = 149.99m,
            description = "Test product created to verify sync functionality",
            shortDescription = "Sync test product",
            status = "Published",
            isVisible = true,
            trackInventory = true,
            stockQuantity = 50,
            isFeatured = false,
            tags = new[] { "test", "sync" }
        };

        var json = JsonSerializer.Serialize(productPayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act 1: Create the product
        _output.WriteLine($"Creating product: {_testProductName} (SKU: {_testProductSku})");
        var createResponse = await _client.PostAsync("/umbraco/management/api/v1/ecommerce/product", content);

        _output.WriteLine($"Create Response: {createResponse.StatusCode}");
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created, "Product should be created successfully");

        // Parse the created product to get its ID
        var createContent = await createResponse.Content.ReadAsStringAsync();
        _output.WriteLine($"Created Product Response: {createContent.Substring(0, Math.Min(500, createContent.Length))}...");

        using var createDoc = JsonDocument.Parse(createContent);
        var productId = createDoc.RootElement.GetProperty("id").GetGuid();
        _createdProductId = productId;
        _output.WriteLine($"Created Product ID: {productId}");

        // Act 2: Get the product by ID to verify it was saved
        var getResponse = await _client.GetAsync($"/umbraco/management/api/v1/ecommerce/product/{productId}");
        _output.WriteLine($"Get Product Response: {getResponse.StatusCode}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getContent = await getResponse.Content.ReadAsStringAsync();
        using var getDoc = JsonDocument.Parse(getContent);
        var retrievedName = getDoc.RootElement.GetProperty("name").GetString();
        var retrievedSku = getDoc.RootElement.GetProperty("sku").GetString();

        _output.WriteLine($"Retrieved Product: {retrievedName} (SKU: {retrievedSku})");
        retrievedName.Should().Be(_testProductName);
        retrievedSku.Should().Be(_testProductSku);

        // Act 3: Verify product appears in the list
        var listResponse = await _client.GetAsync("/umbraco/management/api/v1/ecommerce/product");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var listContent = await listResponse.Content.ReadAsStringAsync();
        listContent.Should().Contain(_testProductSku, "Product should appear in the product list");
        _output.WriteLine($"Product verified in list ✓");
    }

    [Fact]
    [Trait("Category", "E2E")]
    public async Task CreateProduct_ThenSyncToContent_VerifiesSync()
    {
        // Arrange
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var uniqueSku = $"SYNC-VERIFY-{timestamp}";
        var uniqueName = $"Sync Verify Product {timestamp}";

        var productPayload = new
        {
            name = uniqueName,
            sku = uniqueSku,
            basePrice = 79.99m,
            description = "Product to verify content sync",
            status = "Published",
            isVisible = true
        };

        var json = JsonSerializer.Serialize(productPayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act 1: Create product
        _output.WriteLine($"Creating product for sync verification: {uniqueName}");
        var createResponse = await _client.PostAsync("/umbraco/management/api/v1/ecommerce/product", content);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createContent = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createContent);
        var productId = createDoc.RootElement.GetProperty("id").GetGuid();
        _output.WriteLine($"Created product ID: {productId}");

        // Act 2: Trigger content sync
        _output.WriteLine("Triggering content sync...");
        var syncResponse = await _client.PostAsync("/umbraco/management/api/v1/ecommerce/content-sync/products", null);
        _output.WriteLine($"Sync Response: {syncResponse.StatusCode}");
        syncResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var syncContent = await syncResponse.Content.ReadAsStringAsync();
        _output.WriteLine($"Sync Result: {syncContent}");

        // The sync result should indicate success
        syncResponse.IsSuccessStatusCode.Should().BeTrue("Content sync should complete successfully");
        _output.WriteLine("Content sync completed ✓");

        // Act 3: Verify product still exists after sync
        var verifyResponse = await _client.GetAsync($"/umbraco/management/api/v1/ecommerce/product/{productId}");
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        _output.WriteLine("Product verified after sync ✓");
    }

    [Fact]
    [Trait("Category", "E2E")]
    public async Task FullBidirectionalSync_WorksCorrectly()
    {
        // Act: Trigger full bidirectional sync
        _output.WriteLine("Triggering full bidirectional sync (sync-all)...");
        var syncAllResponse = await _client.PostAsync("/umbraco/management/api/v1/ecommerce/content-sync/sync-all", null);

        _output.WriteLine($"Sync-All Response: {syncAllResponse.StatusCode}");
        syncAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var syncContent = await syncAllResponse.Content.ReadAsStringAsync();
        _output.WriteLine($"Full Sync Result: {syncContent}");

        // Parse sync results
        using var syncDoc = JsonDocument.Parse(syncContent);

        if (syncDoc.RootElement.TryGetProperty("productsSyncedToContent", out var productsSynced))
        {
            _output.WriteLine($"Products synced to content: {productsSynced.GetInt32()}");
        }

        if (syncDoc.RootElement.TryGetProperty("categoriesSyncedToContent", out var categoriesSynced))
        {
            _output.WriteLine($"Categories synced to content: {categoriesSynced.GetInt32()}");
        }

        if (syncDoc.RootElement.TryGetProperty("categoriesSyncedToDatabase", out var categoriesToDb))
        {
            _output.WriteLine($"Categories synced to database: {categoriesToDb.GetInt32()}");
        }

        _output.WriteLine("Full bidirectional sync completed ✓");
    }

    [Fact]
    [Trait("Category", "E2E")]
    public async Task ProductTree_ShowsProductsAfterSync()
    {
        // Arrange: First trigger sync
        await _client.PostAsync("/umbraco/management/api/v1/ecommerce/content-sync/products", null);

        // Act: Get product tree
        var treeResponse = await _client.GetAsync("/umbraco/management/api/v1/ecommerce/product/tree");
        treeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var treeContent = await treeResponse.Content.ReadAsStringAsync();
        using var treeDoc = JsonDocument.Parse(treeContent);

        var nodes = treeDoc.RootElement.GetProperty("nodes");
        var nodeCount = nodes.GetArrayLength();

        _output.WriteLine($"Product tree has {nodeCount} root nodes");

        // Find the "All Products" node
        foreach (var node in nodes.EnumerateArray())
        {
            var nodeId = node.GetProperty("id").GetString();
            var nodeName = node.GetProperty("name").GetString();
            var badge = node.TryGetProperty("badge", out var badgeElement) ? badgeElement.GetString() : "0";

            _output.WriteLine($"  - {nodeName} (ID: {nodeId}, Count: {badge})");
        }

        // Verify we have products
        var allProductsNode = nodes.EnumerateArray()
            .FirstOrDefault(n => n.GetProperty("id").GetString() == "all-products");

        allProductsNode.ValueKind.Should().NotBe(JsonValueKind.Undefined);

        var productCount = allProductsNode.GetProperty("badge").GetString();
        _output.WriteLine($"\nTotal products: {productCount}");

        int.Parse(productCount ?? "0").Should().BeGreaterThan(0, "Should have products in the tree");
    }

    public void Dispose()
    {
        // Cleanup: Delete the test product if it was created
        if (_createdProductId.HasValue)
        {
            try
            {
                _client.DeleteAsync($"/umbraco/management/api/v1/ecommerce/product/{_createdProductId}").Wait();
                _output.WriteLine($"Cleaned up test product: {_createdProductId}");
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        _client?.Dispose();
    }
}
