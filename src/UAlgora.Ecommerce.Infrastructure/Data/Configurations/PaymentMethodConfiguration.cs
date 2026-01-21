using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for PaymentMethodConfig.
/// </summary>
public class PaymentMethodConfigConfiguration : IEntityTypeConfiguration<PaymentMethodConfig>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public void Configure(EntityTypeBuilder<PaymentMethodConfig> builder)
    {
        builder.ToTable("PaymentMethods");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(m => m.Description)
            .HasMaxLength(1000);

        builder.Property(m => m.CheckoutInstructions)
            .HasMaxLength(2000);

        builder.Property(m => m.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(m => m.FeeType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(m => m.CaptureMode)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(m => m.FlatFee)
            .HasPrecision(18, 4);

        builder.Property(m => m.PercentageFee)
            .HasPrecision(5, 2);

        builder.Property(m => m.MaxFee)
            .HasPrecision(18, 4);

        builder.Property(m => m.MinOrderAmount)
            .HasPrecision(18, 4);

        builder.Property(m => m.MaxOrderAmount)
            .HasPrecision(18, 4);

        builder.Property(m => m.IconName)
            .HasMaxLength(100);

        builder.Property(m => m.ImageUrl)
            .HasMaxLength(500);

        builder.Property(m => m.CssClass)
            .HasMaxLength(200);

        // JSON conversions for list properties
        builder.Property(m => m.AllowedCountries)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        builder.Property(m => m.ExcludedCountries)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        builder.Property(m => m.AllowedCurrencies)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        builder.Property(m => m.AllowedCustomerGroups)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        // Relationship to gateway
        builder.HasOne(m => m.Gateway)
            .WithMany(g => g.PaymentMethods)
            .HasForeignKey(m => m.GatewayId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(m => m.Code).IsUnique();
        builder.HasIndex(m => m.Name);
        builder.HasIndex(m => m.Type);
        builder.HasIndex(m => m.IsActive);
        builder.HasIndex(m => m.IsDefault);
        builder.HasIndex(m => m.SortOrder);
        builder.HasIndex(m => m.GatewayId);
    }
}

/// <summary>
/// Entity configuration for PaymentGateway.
/// </summary>
public class PaymentGatewayConfiguration : IEntityTypeConfiguration<PaymentGateway>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public void Configure(EntityTypeBuilder<PaymentGateway> builder)
    {
        builder.ToTable("PaymentGateways");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(g => g.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(g => g.Description)
            .HasMaxLength(1000);

        builder.Property(g => g.ProviderType)
            .HasConversion<string>()
            .HasMaxLength(50);

        // API Credentials - use encrypted storage in production
        builder.Property(g => g.ApiKey)
            .HasMaxLength(500);

        builder.Property(g => g.SecretKey)
            .HasMaxLength(500);

        builder.Property(g => g.MerchantId)
            .HasMaxLength(200);

        builder.Property(g => g.ClientId)
            .HasMaxLength(500);

        builder.Property(g => g.ClientSecret)
            .HasMaxLength(500);

        // Sandbox Credentials
        builder.Property(g => g.SandboxApiKey)
            .HasMaxLength(500);

        builder.Property(g => g.SandboxSecretKey)
            .HasMaxLength(500);

        builder.Property(g => g.SandboxMerchantId)
            .HasMaxLength(200);

        // Webhook Settings
        builder.Property(g => g.WebhookUrl)
            .HasMaxLength(1000);

        builder.Property(g => g.WebhookSecret)
            .HasMaxLength(500);

        builder.Property(g => g.SandboxWebhookSecret)
            .HasMaxLength(500);

        // Provider-specific Settings
        builder.Property(g => g.StatementDescriptor)
            .HasMaxLength(22); // Stripe limit

        builder.Property(g => g.StatementDescriptorSuffix)
            .HasMaxLength(22);

        builder.Property(g => g.BrandName)
            .HasMaxLength(127); // PayPal limit

        builder.Property(g => g.LandingPage)
            .HasMaxLength(50);

        builder.Property(g => g.UserAction)
            .HasMaxLength(50);

        // JSON conversions for list properties
        builder.Property(g => g.SupportedCurrencies)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        builder.Property(g => g.SupportedCountries)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        builder.Property(g => g.SupportedPaymentMethods)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        builder.Property(g => g.Settings)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, JsonOptions) ?? new Dictionary<string, string>())
            .HasColumnType("nvarchar(max)");

        // Indexes
        builder.HasIndex(g => g.Code).IsUnique();
        builder.HasIndex(g => g.Name);
        builder.HasIndex(g => g.ProviderType);
        builder.HasIndex(g => g.IsActive);
        builder.HasIndex(g => g.IsSandbox);
        builder.HasIndex(g => g.SortOrder);
    }
}
