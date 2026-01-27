using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.LicensePortal.Models;

namespace UAlgora.Ecommerce.LicensePortal.Controllers;

public class HomeController : Controller
{
    private readonly LicensePortalOptions _options;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IOptions<LicensePortalOptions> options,
        ILogger<HomeController> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View(GetPricingViewModel());
    }

    public IActionResult Pricing()
    {
        return View(GetPricingViewModel());
    }

    public IActionResult Features()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }

    private PricingViewModel GetPricingViewModel()
    {
        return new PricingViewModel
        {
            Trial = new LicenseTierViewModel
            {
                Type = LicenseType.Trial,
                Name = "Trial",
                Description = "Try Algora Commerce free for 14 days",
                Price = 0,
                Currency = "USD",
                PriceDisplay = "Free",
                BillingPeriod = "14 days",
                Features =
                [
                    "1 Store",
                    "100 Products",
                    "50 Orders/month",
                    "Basic features",
                    "Community support"
                ],
                CtaText = "Start Free Trial",
                CtaUrl = "/checkout/trial"
            },
            Standard = new LicenseTierViewModel
            {
                Type = LicenseType.Standard,
                Name = "Standard",
                Description = "Perfect for growing businesses",
                Price = _options.Pricing.Standard.AnnualPrice,
                Currency = _options.Pricing.Standard.Currency,
                PriceDisplay = $"${_options.Pricing.Standard.AnnualPrice:N0}",
                BillingPeriod = "year",
                Features = _options.Pricing.Standard.Features,
                IsPopular = true,
                CtaText = "Get Started",
                CtaUrl = "/checkout/standard"
            },
            Enterprise = new LicenseTierViewModel
            {
                Type = LicenseType.Enterprise,
                Name = "Enterprise",
                Description = "For large-scale operations",
                Price = _options.Pricing.Enterprise.AnnualPrice,
                Currency = _options.Pricing.Enterprise.Currency,
                PriceDisplay = $"${_options.Pricing.Enterprise.AnnualPrice:N0}",
                BillingPeriod = "year",
                Features = _options.Pricing.Enterprise.Features,
                CtaText = "Contact Sales",
                CtaUrl = "/checkout/enterprise"
            }
        };
    }
}
