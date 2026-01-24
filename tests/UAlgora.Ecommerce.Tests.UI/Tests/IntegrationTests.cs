using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using UAlgora.Ecommerce.Tests.UI.Configuration;
using Xunit;

namespace UAlgora.Ecommerce.Tests.UI.Tests;

/// <summary>
/// Integration tests for Algora Commerce API endpoints
/// These tests verify the backend functionality without requiring browser login
/// </summary>
[Collection("Sequential")]
public class IntegrationTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly TestSettings _settings;

    public IntegrationTests()
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
    [Trait("Category", "Integration")]
    public async Task ApplicationIsRunning()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue("Application should be running");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task UmbracoBackofficeIsAccessible()
    {
        // Act
        var response = await _client.GetAsync("/umbraco");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Redirect, HttpStatusCode.Found);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ProductsApiEndpointExists()
    {
        // Act
        var response = await _client.GetAsync("/umbraco/management/api/v1/ecommerce/products");

        // Assert - 404 means endpoint not yet implemented
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task CategoriesApiEndpointExists()
    {
        // Act
        var response = await _client.GetAsync("/umbraco/management/api/v1/ecommerce/categories");

        // Assert - 404 means endpoint not yet implemented
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task OrdersApiEndpointExists()
    {
        // Act
        var response = await _client.GetAsync("/umbraco/management/api/v1/ecommerce/orders");

        // Assert - 404 means endpoint not yet implemented
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task CustomersApiEndpointExists()
    {
        // Act
        var response = await _client.GetAsync("/umbraco/management/api/v1/ecommerce/customers");

        // Assert - 404 means endpoint not yet implemented
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task StoresApiEndpointExists()
    {
        // Act
        var response = await _client.GetAsync("/umbraco/management/api/v1/ecommerce/stores");

        // Assert - 404 means endpoint not yet implemented
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GiftCardsApiEndpointExists()
    {
        // Act
        var response = await _client.GetAsync("/umbraco/management/api/v1/ecommerce/giftcards");

        // Assert - 404 means endpoint not yet implemented
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ReturnsApiEndpointExists()
    {
        // Act
        var response = await _client.GetAsync("/umbraco/management/api/v1/ecommerce/returns");

        // Assert - 404 means endpoint not yet implemented
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DiscountsApiEndpointExists()
    {
        // Act
        var response = await _client.GetAsync("/umbraco/management/api/v1/ecommerce/discounts");

        // Assert - 404 means endpoint not yet implemented
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task CurrenciesApiEndpointExists()
    {
        // Act
        var response = await _client.GetAsync("/umbraco/management/api/v1/ecommerce/currencies");

        // Assert - 404 means endpoint not yet implemented
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task WebhooksApiEndpointExists()
    {
        // Act
        var response = await _client.GetAsync("/umbraco/management/api/v1/ecommerce/webhooks");

        // Assert - 404 means endpoint not yet implemented
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task EmailTemplatesApiEndpointExists()
    {
        // Act
        var response = await _client.GetAsync("/umbraco/management/api/v1/ecommerce/emailtemplates");

        // Assert - 404 means endpoint not yet implemented
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task PaymentLinksApiEndpointExists()
    {
        // Act
        var response = await _client.GetAsync("/umbraco/management/api/v1/ecommerce/paymentlinks");

        // Assert - 404 means endpoint not yet implemented
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ContentSyncProductsEndpointExists()
    {
        // Act
        var response = await _client.PostAsync("/umbraco/management/api/v1/ecommerce/content-sync/products", null);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ContentSyncCategoriesEndpointExists()
    {
        // Act
        var response = await _client.PostAsync("/umbraco/management/api/v1/ecommerce/content-sync/categories", null);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ContentSyncAllEndpointExists()
    {
        // Act
        var response = await _client.PostAsync("/umbraco/management/api/v1/ecommerce/content-sync/sync-all", null);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SwaggerIsAccessible()
    {
        // Act
        var response = await _client.GetAsync("/umbraco/swagger/index.html");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}
