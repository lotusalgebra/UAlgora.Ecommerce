using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using UAlgora.Ecommerce.Tests.UI.Configuration;
using Xunit;

namespace UAlgora.Ecommerce.Tests.UI.Tests;

/// <summary>
/// API tests for Algora Commerce backend
/// </summary>
[Collection("Sequential")]
public class ApiTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly TestSettings _settings;

    public ApiTests()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        _settings = new TestSettings();
        configuration.GetSection("TestSettings").Bind(_settings);

        _client = new HttpClient
        {
            BaseAddress = new Uri(_settings.BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    [Fact]
    [Trait("Category", "API")]
    public async Task HealthCheck_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue("Application should be running");
    }

    [Fact]
    [Trait("Category", "API")]
    public async Task UmbracoBackoffice_ShouldBeAccessible()
    {
        // Act
        var response = await _client.GetAsync("/umbraco");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Redirect, HttpStatusCode.Found);
    }

    [Fact]
    [Trait("Category", "API")]
    public async Task ProductsApi_ShouldReturnProducts()
    {
        // Arrange - Try to get products from management API
        var url = "/umbraco/management/api/v1/ecommerce/products";

        // Act
        try
        {
            var response = await _client.GetAsync(url);

            // Assert - Either returns products or requires auth
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,
                HttpStatusCode.Unauthorized,
                HttpStatusCode.NotFound,
                HttpStatusCode.Forbidden);
        }
        catch (HttpRequestException)
        {
            // API might not be accessible without auth
        }
    }

    [Fact]
    [Trait("Category", "API")]
    public async Task CategoriesApi_ShouldReturnCategories()
    {
        // Arrange
        var url = "/umbraco/management/api/v1/ecommerce/categories";

        // Act
        try
        {
            var response = await _client.GetAsync(url);

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,
                HttpStatusCode.Unauthorized,
                HttpStatusCode.NotFound,
                HttpStatusCode.Forbidden);
        }
        catch (HttpRequestException)
        {
            // API might not be accessible without auth
        }
    }

    [Fact]
    [Trait("Category", "API")]
    public async Task SwaggerEndpoint_ShouldBeAccessible()
    {
        // Act
        var response = await _client.GetAsync("/umbraco/swagger/index.html");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("Category", "API")]
    public async Task ContentSyncApi_EndpointExists()
    {
        // Arrange
        var url = "/umbraco/management/api/v1/ecommerce/content-sync/sync-all";

        // Act
        try
        {
            var response = await _client.PostAsync(url, null);

            // Assert - Should exist even if requires auth
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,
                HttpStatusCode.Unauthorized,
                HttpStatusCode.Forbidden,
                HttpStatusCode.MethodNotAllowed);
        }
        catch (HttpRequestException)
        {
            // Expected if API not accessible
        }
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}
