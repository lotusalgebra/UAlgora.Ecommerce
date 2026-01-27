using Microsoft.Extensions.Logging;
using UAlgora.Ecommerce.Web.DocumentTypes.Providers;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace UAlgora.Ecommerce.Web.Services;

/// <summary>
/// Seeds complete enterprise content structure for a production-ready e-commerce site.
/// Creates site settings, home page, categories, products, and content pages.
/// </summary>
public class EnterpriseContentSeeder
{
    private readonly IContentService _contentService;
    private readonly IContentTypeService _contentTypeService;
    private readonly ILogger<EnterpriseContentSeeder> _logger;
    private readonly Random _random = new();

    // Document type references
    private IContentType? _siteSettingsType;
    private IContentType? _homePageType;
    private IContentType? _heroSlideType;
    private IContentType? _bannerType;
    private IContentType? _testimonialType;
    private IContentType? _featureType;
    private IContentType? _catalogType;
    private IContentType? _categoryType;
    private IContentType? _productType;
    private IContentType? _checkoutStepType;

    public EnterpriseContentSeeder(
        IContentService contentService,
        IContentTypeService contentTypeService,
        ILogger<EnterpriseContentSeeder> logger)
    {
        _contentService = contentService;
        _contentTypeService = contentTypeService;
        _logger = logger;
    }

    /// <summary>
    /// Seeds complete enterprise content structure
    /// </summary>
    public SeedResult SeedEnterpriseContent(bool clearExisting = false)
    {
        var result = new SeedResult();

        try
        {
            _logger.LogInformation("Algora Commerce: Starting enterprise content seeding...");

            // Load document types
            if (!LoadDocumentTypes(result))
            {
                return result;
            }

            // Clear existing content if requested
            if (clearExisting)
            {
                ClearExistingContent(result);
            }

            // Create Site Settings
            var siteSettings = CreateSiteSettings(result);

            // Create Home Page with children
            var homePage = CreateHomePage(result);
            if (homePage != null)
            {
                CreateHeroSlides(homePage.Id, result);
                CreateBanners(homePage.Id, result);
                CreateTestimonials(homePage.Id, result);
                CreateFeatures(homePage.Id, result);
            }

            // Create Shop/Catalog with categories and products
            var catalog = CreateCatalog(result);
            if (catalog != null)
            {
                CreateEnterpriseCatalogStructure(catalog.Id, result);
            }

            // Create Checkout Steps
            CreateCheckoutSteps(result);

            _logger.LogInformation("Algora Commerce: Enterprise seeding complete. Created {Count} items.", result.Created);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding enterprise content");
            result.Errors.Add(ex.Message);
            return result;
        }
    }

    private bool LoadDocumentTypes(SeedResult result)
    {
        _siteSettingsType = _contentTypeService.Get(AlgoraDocumentTypeConstants.SiteSettingsAlias);
        _homePageType = _contentTypeService.Get(AlgoraDocumentTypeConstants.HomePageAlias);
        _heroSlideType = _contentTypeService.Get(AlgoraDocumentTypeConstants.HeroSlideAlias);
        _bannerType = _contentTypeService.Get(AlgoraDocumentTypeConstants.BannerAlias);
        _testimonialType = _contentTypeService.Get(AlgoraDocumentTypeConstants.TestimonialAlias);
        _featureType = _contentTypeService.Get(AlgoraDocumentTypeConstants.FeatureAlias);
        _catalogType = _contentTypeService.Get(AlgoraDocumentTypeConstants.CatalogAlias);
        _categoryType = _contentTypeService.Get(AlgoraDocumentTypeConstants.CategoryAlias);
        _productType = _contentTypeService.Get(AlgoraDocumentTypeConstants.ProductAlias);
        _checkoutStepType = _contentTypeService.Get(AlgoraDocumentTypeConstants.CheckoutStepAlias);

        var missingTypes = new List<string>();
        if (_catalogType == null) missingTypes.Add("Catalog");
        if (_categoryType == null) missingTypes.Add("Category");
        if (_productType == null) missingTypes.Add("Product");

        if (missingTypes.Any())
        {
            result.Errors.Add($"Missing required document types: {string.Join(", ", missingTypes)}. Please install document types first.");
            return false;
        }

        return true;
    }

    private void ClearExistingContent(SeedResult result)
    {
        _logger.LogInformation("Clearing existing content...");
        var rootContent = _contentService.GetRootContent().ToList();

        foreach (var content in rootContent)
        {
            _contentService.Delete(content);
            result.Messages.Add($"Deleted: {content.Name}");
        }
    }

    private IContent? CreateSiteSettings(SeedResult result)
    {
        if (_siteSettingsType == null) return null;

        var existing = _contentService.GetRootContent()
            .FirstOrDefault(c => c.ContentType.Alias == AlgoraDocumentTypeConstants.SiteSettingsAlias);
        if (existing != null)
        {
            result.Messages.Add("Site Settings already exists");
            return existing;
        }

        var settings = _contentService.Create("Site Settings", Constants.System.Root, _siteSettingsType);

        settings.SetValue("siteName", "Algora Fashion Store");
        settings.SetValue("tagline", "Discover Your Style");
        settings.SetValue("email", "hello@algora-store.com");
        settings.SetValue("phone", "+1 (555) 123-4567");
        settings.SetValue("address", "123 Fashion Avenue, New York, NY 10001");
        settings.SetValue("copyrightText", $"© {DateTime.Now.Year} Algora Fashion Store. All rights reserved.");
        settings.SetValue("facebook", "https://facebook.com/algorastore");
        settings.SetValue("instagram", "https://instagram.com/algorastore");
        settings.SetValue("twitter", "https://twitter.com/algorastore");
        settings.SetValue("pinterest", "https://pinterest.com/algorastore");

        _contentService.Save(settings);
        _contentService.Publish(settings, Array.Empty<string>());

        result.Created++;
        result.Messages.Add("Created: Site Settings");
        return settings;
    }

    private IContent? CreateHomePage(SeedResult result)
    {
        if (_homePageType == null) return null;

        var existing = _contentService.GetRootContent()
            .FirstOrDefault(c => c.ContentType.Alias == AlgoraDocumentTypeConstants.HomePageAlias);
        if (existing != null)
        {
            result.Messages.Add("Home Page already exists");
            return existing;
        }

        var homePage = _contentService.Create("Home", Constants.System.Root, _homePageType);

        // Hero Section
        homePage.SetValue("heroEnabled", true);
        homePage.SetValue("heroAutoplay", true);
        homePage.SetValue("heroInterval", 5000);

        // Categories Section
        homePage.SetValue("categoriesEnabled", true);
        homePage.SetValue("categoriesTitle", "Shop by Category");
        homePage.SetValue("categoriesSubtitle", "Find exactly what you're looking for");
        homePage.SetValue("categoriesCount", 6);

        // Deals Section
        homePage.SetValue("dealsEnabled", true);
        homePage.SetValue("dealsTitle", "Deal of the Day");
        homePage.SetValue("dealProductsCount", 4);

        // Featured Products
        homePage.SetValue("featuredEnabled", true);
        homePage.SetValue("featuredTitle", "Featured Products");
        homePage.SetValue("featuredCount", 8);

        // New Arrivals
        homePage.SetValue("newArrivalsEnabled", true);
        homePage.SetValue("newArrivalsTitle", "New Arrivals");
        homePage.SetValue("newArrivalsCount", 8);

        // Best Sellers
        homePage.SetValue("bestSellersEnabled", true);
        homePage.SetValue("bestSellersTitle", "Best Sellers");
        homePage.SetValue("bestSellersCount", 8);

        // Banners
        homePage.SetValue("bannersEnabled", true);

        // Features/USP
        homePage.SetValue("featuresEnabled", true);
        homePage.SetValue("featuresTitle", "Why Choose Us");

        // Testimonials
        homePage.SetValue("testimonialsEnabled", true);
        homePage.SetValue("testimonialsTitle", "What Our Customers Say");
        homePage.SetValue("testimonialsSubtitle", "Real reviews from real customers");

        // SEO
        homePage.SetValue("metaTitle", "Algora Fashion Store - Discover Your Style");
        homePage.SetValue("metaDescription", "Shop the latest fashion trends at Algora. Premium quality clothing, accessories, and more with fast shipping and easy returns.");

        _contentService.Save(homePage);
        _contentService.Publish(homePage, Array.Empty<string>());

        result.Created++;
        result.Messages.Add("Created: Home Page");
        return homePage;
    }

    private void CreateHeroSlides(int parentId, SeedResult result)
    {
        if (_heroSlideType == null) return;

        var slides = new[]
        {
            ("Summer Collection 2024", "Embrace the warmth with our latest summer styles", "Shop Summer", "/products?category=summer", 1),
            ("New Arrivals", "Fresh styles just landed - be the first to wear them", "Explore New", "/products?sort=newest", 2),
            ("Sale Up to 50% Off", "Don't miss our biggest sale of the season", "Shop Sale", "/deals", 3),
            ("Premium Quality", "Crafted with care, designed to last", "Discover More", "/about", 4)
        };

        foreach (var (title, subtitle, cta, url, order) in slides)
        {
            var slide = _contentService.Create(title, parentId, _heroSlideType);
            slide.SetValue("title", title);
            slide.SetValue("subtitle", subtitle);
            slide.SetValue("buttonText", cta);
            slide.SetValue("buttonLink", url);
            slide.SetValue("textAlignment", "center");
            slide.SetValue("overlayOpacity", 30);

            _contentService.Save(slide);
            _contentService.Publish(slide, Array.Empty<string>());
            result.Created++;
        }
        result.Messages.Add($"Created: {slides.Length} Hero Slides");
    }

    private void CreateBanners(int parentId, SeedResult result)
    {
        if (_bannerType == null) return;

        var banners = new[]
        {
            ("Free Shipping", "On orders over $50", "Shop Now", "/products", "small", 1),
            ("Easy Returns", "30-day return policy", "Learn More", "/returns", "small", 2),
            ("Secure Payment", "100% secure checkout", "Shop Safely", "/products", "small", 3)
        };

        foreach (var (title, subtitle, btnText, btnLink, size, order) in banners)
        {
            var banner = _contentService.Create(title, parentId, _bannerType);
            banner.SetValue("title", title);
            banner.SetValue("subtitle", subtitle);
            banner.SetValue("buttonText", btnText);
            banner.SetValue("buttonLink", btnLink);
            banner.SetValue("bannerSize", size);
            banner.SetValue("sortOrder", order);

            _contentService.Save(banner);
            _contentService.Publish(banner, Array.Empty<string>());
            result.Created++;
        }
        result.Messages.Add($"Created: {banners.Length} Banners");
    }

    private void CreateTestimonials(int parentId, SeedResult result)
    {
        if (_testimonialType == null) return;

        var testimonials = new[]
        {
            ("Sarah Johnson", "Fashion Blogger", "Absolutely love the quality and style of everything I've ordered. Fast shipping too!", 5),
            ("Michael Chen", "Verified Buyer", "Best online shopping experience I've had. The customer service is top-notch.", 5),
            ("Emily Davis", "Regular Customer", "I've been shopping here for years. Always find exactly what I'm looking for.", 5),
            ("James Wilson", "First-time Buyer", "Impressed with the quality for the price. Will definitely be ordering again!", 4),
            ("Amanda Brown", "Style Enthusiast", "The collection is always on-trend. My go-to store for all fashion needs.", 5)
        };

        foreach (var (name, title, reviewText, rating) in testimonials)
        {
            var testimonial = _contentService.Create(name, parentId, _testimonialType);
            testimonial.SetValue("customerName", name);
            testimonial.SetValue("customerTitle", title);
            testimonial.SetValue("reviewText", reviewText);
            testimonial.SetValue("rating", rating);
            testimonial.SetValue("isVerified", true);

            _contentService.Save(testimonial);
            _contentService.Publish(testimonial, Array.Empty<string>());
            result.Created++;
        }
        result.Messages.Add($"Created: {testimonials.Length} Testimonials");
    }

    private void CreateFeatures(int parentId, SeedResult result)
    {
        if (_featureType == null) return;

        var features = new[]
        {
            ("Free Shipping", "Free delivery on orders over $50", "truck", 1),
            ("Easy Returns", "30-day hassle-free returns", "refresh", 2),
            ("Secure Payment", "SSL encrypted checkout", "shield", 3),
            ("24/7 Support", "Round the clock customer service", "headphones", 4),
            ("Quality Guarantee", "Premium materials and craftsmanship", "star", 5),
            ("Fast Delivery", "Express shipping available", "lightning", 6)
        };

        foreach (var (title, desc, icon, order) in features)
        {
            var feature = _contentService.Create(title, parentId, _featureType);
            feature.SetValue("title", title);
            feature.SetValue("description", desc);
            feature.SetValue("icon", icon);
            feature.SetValue("sortOrder", order);

            _contentService.Save(feature);
            _contentService.Publish(feature, Array.Empty<string>());
            result.Created++;
        }
        result.Messages.Add($"Created: {features.Length} Features");
    }

    private IContent? CreateCatalog(SeedResult result)
    {
        if (_catalogType == null) return null;

        var existing = _contentService.GetRootContent()
            .FirstOrDefault(c => c.ContentType.Alias == AlgoraDocumentTypeConstants.CatalogAlias);
        if (existing != null)
        {
            result.Messages.Add("Catalog already exists");
            return existing;
        }

        var catalog = _contentService.Create("Shop", Constants.System.Root, _catalogType);

        catalog.SetValue("title", "Shop All Products");
        catalog.SetValue("subtitle", "Discover our complete collection");
        catalog.SetValue("description", "<p>Browse our curated selection of premium fashion products. From everyday essentials to statement pieces, find everything you need to express your unique style.</p>");
        catalog.SetValue("productsPerPage", 24);
        catalog.SetValue("defaultSortOrder", "featured");
        catalog.SetValue("showFilters", true);
        catalog.SetValue("metaTitle", "Shop All Products - Algora Fashion Store");
        catalog.SetValue("metaDescription", "Browse our complete collection of fashion products. Quality clothing, accessories, and more with fast shipping.");

        _contentService.Save(catalog);
        _contentService.Publish(catalog, Array.Empty<string>());

        result.Created++;
        result.Messages.Add("Created: Shop Catalog");
        return catalog;
    }

    private void CreateEnterpriseCatalogStructure(int catalogId, SeedResult result)
    {
        // Define comprehensive category structure with subcategories and products
        var categories = GetEnterpriseCategoryStructure();
        var totalCategories = 0;
        var totalProducts = 0;

        foreach (var category in categories)
        {
            var categoryNode = CreateCategory(catalogId, category.Name, category.Description, category.Order, result);
            if (categoryNode != null)
            {
                totalCategories++;
                _logger.LogInformation("Created category: {CategoryName} (ID: {Id})", category.Name, categoryNode.Id);

                // Create subcategories
                foreach (var subCategory in category.SubCategories)
                {
                    var subCategoryNode = CreateCategory(categoryNode.Id, subCategory.Name, subCategory.Description, subCategory.Order, result);
                    if (subCategoryNode != null)
                    {
                        totalCategories++;
                        _logger.LogInformation("  Created subcategory: {SubCategoryName} under {CategoryName}", subCategory.Name, category.Name);

                        // Create products in subcategory
                        var productCount = 0;
                        foreach (var product in subCategory.Products)
                        {
                            CreateProduct(subCategoryNode.Id, product, result);
                            productCount++;
                            totalProducts++;
                        }
                        _logger.LogInformation("    Added {Count} products to {SubCategoryName}", productCount, subCategory.Name);
                    }
                }

                // Create products directly in category (if any)
                if (category.Products.Any())
                {
                    var productCount = 0;
                    foreach (var product in category.Products)
                    {
                        CreateProduct(categoryNode.Id, product, result);
                        productCount++;
                        totalProducts++;
                    }
                    _logger.LogInformation("  Added {Count} products directly to {CategoryName}", productCount, category.Name);
                }
            }
        }

        result.Messages.Add($"Created {totalCategories} categories with {totalProducts} products");
    }

    private IContent? CreateCategory(int parentId, string name, string description, int order, SeedResult result)
    {
        if (_categoryType == null) return null;

        var category = _contentService.Create(name, parentId, _categoryType);

        category.SetValue("categoryName", name);
        category.SetValue("description", $"<p>{description}</p>");
        category.SetValue("slug", GenerateSlug(name));
        category.SetValue("isVisible", true);
        category.SetValue("sortOrder", order);
        category.SetValue("metaTitle", $"{name} - Algora Fashion Store");
        category.SetValue("metaDescription", description);

        _contentService.Save(category);
        _contentService.Publish(category, Array.Empty<string>());

        result.Created++;
        return category;
    }

    private void CreateProduct(int parentId, ProductData product, SeedResult result)
    {
        if (_productType == null) return;

        var productNode = _contentService.Create(product.Name, parentId, _productType);

        // Content tab
        productNode.SetValue("productName", product.Name);
        productNode.SetValue("sku", product.Sku);
        productNode.SetValue("shortDescription", product.ShortDescription);
        productNode.SetValue("description", $"<p>{product.Description}</p>");
        productNode.SetValue("brand", product.Brand);
        productNode.SetValue("manufacturer", product.Brand);

        // Media tab - Set product image URL
        if (!string.IsNullOrEmpty(product.ImageUrl))
        {
            productNode.SetValue("primaryImage", product.ImageUrl);
        }

        // Commerce tab
        productNode.SetValue("basePrice", product.Price);
        if (product.SalePrice.HasValue)
        {
            productNode.SetValue("salePrice", product.SalePrice.Value);
            productNode.SetValue("compareAtPrice", product.Price);
        }
        productNode.SetValue("currencyCode", "USD");
        productNode.SetValue("trackInventory", true);
        productNode.SetValue("stockQuantity", _random.Next(20, 200));
        productNode.SetValue("lowStockThreshold", 10);
        productNode.SetValue("requiresShipping", true);
        productNode.SetValue("weight", Math.Round((decimal)(_random.NextDouble() * 2 + 0.2), 2));

        // Settings tab
        productNode.SetValue("slug", GenerateSlug(product.Name));
        productNode.SetValue("status", "Published");
        productNode.SetValue("isVisible", true);
        productNode.SetValue("isFeatured", product.IsFeatured);
        productNode.SetValue("sortOrder", 0);
        productNode.SetValue("metaTitle", $"{product.Name} - Buy Online | Algora");
        productNode.SetValue("metaDescription", product.ShortDescription);

        _contentService.Save(productNode);
        _contentService.Publish(productNode, Array.Empty<string>());

        result.Created++;
    }

    private void CreateCheckoutSteps(SeedResult result)
    {
        if (_checkoutStepType == null) return;

        // Find or create a checkout container
        var steps = new[]
        {
            ("Information", "Customer information and contact details", "user", 1, true),
            ("Shipping", "Select your shipping method", "truck", 2, true),
            ("Payment", "Enter payment details", "credit-card", 3, true),
            ("Review", "Review and confirm your order", "check-circle", 4, true)
        };

        var existingSteps = _contentService.GetRootContent()
            .Where(c => c.ContentType.Alias == AlgoraDocumentTypeConstants.CheckoutStepAlias)
            .ToList();

        if (existingSteps.Any())
        {
            result.Messages.Add($"Checkout steps already exist ({existingSteps.Count} found)");
            return;
        }

        foreach (var (name, desc, icon, order, enabled) in steps)
        {
            var step = _contentService.Create(name, Constants.System.Root, _checkoutStepType);
            step.SetValue("stepName", name);
            step.SetValue("stepTitle", name);
            step.SetValue("stepDescription", desc);
            step.SetValue("stepType", name);
            step.SetValue("stepOrder", order);
            step.SetValue("icon", icon);
            step.SetValue("isRequired", true);
            step.SetValue("isEnabled", enabled);

            _contentService.Save(step);
            _contentService.Publish(step, Array.Empty<string>());
            result.Created++;
        }
        result.Messages.Add($"Created: {steps.Length} Checkout Steps");
    }

    private string GenerateSlug(string name)
    {
        return name.ToLower()
            .Replace(" & ", "-and-")
            .Replace("&", "-and-")
            .Replace("'", "")
            .Replace(" ", "-")
            .Replace("--", "-");
    }

    private List<CategoryData> GetEnterpriseCategoryStructure()
    {
        return new List<CategoryData>
        {
            // Women's Fashion
            new CategoryData
            {
                Name = "Women",
                Description = "Explore our stunning collection of women's fashion, from casual wear to elegant evening attire.",
                Order = 1,
                SubCategories = new List<SubCategoryData>
                {
                    new SubCategoryData
                    {
                        Name = "Dresses",
                        Description = "Beautiful dresses for every occasion",
                        Order = 1,
                        Products = GenerateProducts("Women's Dress", "WD", "Algora Fashion", 89.99m, 15)
                    },
                    new SubCategoryData
                    {
                        Name = "Tops & Blouses",
                        Description = "Stylish tops and blouses for any look",
                        Order = 2,
                        Products = GenerateProducts("Women's Top", "WT", "Algora Fashion", 49.99m, 12)
                    },
                    new SubCategoryData
                    {
                        Name = "Jeans & Pants",
                        Description = "Comfortable and trendy bottoms",
                        Order = 3,
                        Products = GenerateProducts("Women's Pants", "WP", "Algora Denim", 79.99m, 10)
                    },
                    new SubCategoryData
                    {
                        Name = "Skirts",
                        Description = "From mini to maxi, find your perfect skirt",
                        Order = 4,
                        Products = GenerateProducts("Women's Skirt", "WS", "Algora Fashion", 59.99m, 8)
                    },
                    new SubCategoryData
                    {
                        Name = "Activewear",
                        Description = "Performance wear for your active lifestyle",
                        Order = 5,
                        Products = GenerateProducts("Women's Activewear", "WA", "Algora Sport", 54.99m, 10)
                    }
                }
            },

            // Men's Fashion
            new CategoryData
            {
                Name = "Men",
                Description = "Discover our collection of men's fashion, from casual basics to formal essentials.",
                Order = 2,
                SubCategories = new List<SubCategoryData>
                {
                    new SubCategoryData
                    {
                        Name = "Shirts",
                        Description = "Casual and formal shirts for every occasion",
                        Order = 1,
                        Products = GenerateProducts("Men's Shirt", "MS", "Algora Men", 69.99m, 12)
                    },
                    new SubCategoryData
                    {
                        Name = "T-Shirts & Polos",
                        Description = "Essential tees and polos",
                        Order = 2,
                        Products = GenerateProducts("Men's T-Shirt", "MT", "Algora Basics", 34.99m, 15)
                    },
                    new SubCategoryData
                    {
                        Name = "Jeans & Trousers",
                        Description = "Classic jeans and modern trousers",
                        Order = 3,
                        Products = GenerateProducts("Men's Jeans", "MJ", "Algora Denim", 89.99m, 10)
                    },
                    new SubCategoryData
                    {
                        Name = "Suits & Blazers",
                        Description = "Sharp suits and sophisticated blazers",
                        Order = 4,
                        Products = GenerateProducts("Men's Blazer", "MB", "Algora Formal", 199.99m, 8)
                    },
                    new SubCategoryData
                    {
                        Name = "Sportswear",
                        Description = "Athletic wear for peak performance",
                        Order = 5,
                        Products = GenerateProducts("Men's Sportswear", "MSP", "Algora Sport", 59.99m, 10)
                    }
                }
            },

            // Kids
            new CategoryData
            {
                Name = "Kids",
                Description = "Adorable and durable clothing for children of all ages.",
                Order = 3,
                SubCategories = new List<SubCategoryData>
                {
                    new SubCategoryData
                    {
                        Name = "Girls",
                        Description = "Cute and comfortable clothing for girls",
                        Order = 1,
                        Products = GenerateProducts("Girls' Outfit", "KG", "Algora Kids", 39.99m, 10)
                    },
                    new SubCategoryData
                    {
                        Name = "Boys",
                        Description = "Cool and practical clothing for boys",
                        Order = 2,
                        Products = GenerateProducts("Boys' Outfit", "KB", "Algora Kids", 39.99m, 10)
                    },
                    new SubCategoryData
                    {
                        Name = "Baby",
                        Description = "Soft and gentle clothing for babies",
                        Order = 3,
                        Products = GenerateProducts("Baby Clothes", "KBB", "Algora Baby", 29.99m, 8)
                    }
                }
            },

            // Accessories
            new CategoryData
            {
                Name = "Accessories",
                Description = "Complete your look with our stunning accessories collection.",
                Order = 4,
                SubCategories = new List<SubCategoryData>
                {
                    new SubCategoryData
                    {
                        Name = "Bags & Purses",
                        Description = "From totes to clutches, find your perfect bag",
                        Order = 1,
                        Products = GenerateProducts("Designer Bag", "AB", "Algora Accessories", 129.99m, 12)
                    },
                    new SubCategoryData
                    {
                        Name = "Jewelry",
                        Description = "Beautiful jewelry to elevate any outfit",
                        Order = 2,
                        Products = GenerateProducts("Jewelry Set", "AJ", "Algora Luxe", 79.99m, 15)
                    },
                    new SubCategoryData
                    {
                        Name = "Watches",
                        Description = "Timeless watches for every style",
                        Order = 3,
                        Products = GenerateProducts("Fashion Watch", "AW", "Algora Time", 149.99m, 8)
                    },
                    new SubCategoryData
                    {
                        Name = "Belts & Scarves",
                        Description = "Essential accessories for finishing touches",
                        Order = 4,
                        Products = GenerateProducts("Belt", "ABS", "Algora Accessories", 49.99m, 10)
                    },
                    new SubCategoryData
                    {
                        Name = "Sunglasses",
                        Description = "Stylish sunglasses for sun protection with flair",
                        Order = 5,
                        Products = GenerateProducts("Sunglasses", "ASG", "Algora Eyewear", 89.99m, 8)
                    }
                }
            },

            // Shoes
            new CategoryData
            {
                Name = "Shoes",
                Description = "Step out in style with our footwear collection.",
                Order = 5,
                SubCategories = new List<SubCategoryData>
                {
                    new SubCategoryData
                    {
                        Name = "Women's Shoes",
                        Description = "Heels, flats, boots and more for women",
                        Order = 1,
                        Products = GenerateProducts("Women's Heels", "SWH", "Algora Footwear", 99.99m, 12)
                    },
                    new SubCategoryData
                    {
                        Name = "Men's Shoes",
                        Description = "Formal, casual and athletic shoes for men",
                        Order = 2,
                        Products = GenerateProducts("Men's Oxford", "SMO", "Algora Footwear", 119.99m, 10)
                    },
                    new SubCategoryData
                    {
                        Name = "Sneakers",
                        Description = "Trendy sneakers for everyday comfort",
                        Order = 3,
                        Products = GenerateProducts("Sneakers", "SSN", "Algora Sport", 89.99m, 15)
                    },
                    new SubCategoryData
                    {
                        Name = "Boots",
                        Description = "Ankle boots to knee-highs for all seasons",
                        Order = 4,
                        Products = GenerateProducts("Fashion Boots", "SBT", "Algora Footwear", 149.99m, 8)
                    }
                }
            },

            // Beauty
            new CategoryData
            {
                Name = "Beauty",
                Description = "Premium beauty and skincare products for your self-care routine.",
                Order = 6,
                SubCategories = new List<SubCategoryData>
                {
                    new SubCategoryData
                    {
                        Name = "Skincare",
                        Description = "Cleansers, moisturizers, serums and more",
                        Order = 1,
                        Products = GenerateProducts("Skincare Set", "BSK", "Algora Beauty", 69.99m, 12)
                    },
                    new SubCategoryData
                    {
                        Name = "Makeup",
                        Description = "Professional quality makeup products",
                        Order = 2,
                        Products = GenerateProducts("Makeup Kit", "BMK", "Algora Beauty", 54.99m, 15)
                    },
                    new SubCategoryData
                    {
                        Name = "Fragrances",
                        Description = "Signature scents for every personality",
                        Order = 3,
                        Products = GenerateProducts("Perfume", "BPF", "Algora Scents", 89.99m, 10)
                    },
                    new SubCategoryData
                    {
                        Name = "Haircare",
                        Description = "Shampoos, conditioners and styling products",
                        Order = 4,
                        Products = GenerateProducts("Haircare Set", "BHC", "Algora Beauty", 44.99m, 8)
                    }
                }
            },

            // Home & Living
            new CategoryData
            {
                Name = "Home & Living",
                Description = "Stylish home decor and lifestyle products.",
                Order = 7,
                SubCategories = new List<SubCategoryData>
                {
                    new SubCategoryData
                    {
                        Name = "Home Decor",
                        Description = "Beautiful pieces to transform your space",
                        Order = 1,
                        Products = GenerateProducts("Decor Item", "HHD", "Algora Home", 59.99m, 12)
                    },
                    new SubCategoryData
                    {
                        Name = "Bedding",
                        Description = "Luxurious bedding for restful sleep",
                        Order = 2,
                        Products = GenerateProducts("Bedding Set", "HBD", "Algora Home", 129.99m, 8)
                    },
                    new SubCategoryData
                    {
                        Name = "Bath & Body",
                        Description = "Spa-quality bath and body products",
                        Order = 3,
                        Products = GenerateProducts("Bath Set", "HBB", "Algora Wellness", 49.99m, 10)
                    },
                    new SubCategoryData
                    {
                        Name = "Candles & Fragrance",
                        Description = "Aromatic candles and home fragrances",
                        Order = 4,
                        Products = GenerateProducts("Scented Candle", "HCF", "Algora Scents", 34.99m, 8)
                    }
                }
            },

            // Sale
            new CategoryData
            {
                Name = "Sale",
                Description = "Don't miss our amazing deals and discounts!",
                Order = 8,
                Products = GenerateSaleProducts(20)
            }
        };
    }

    private List<ProductData> GenerateProducts(string baseName, string skuPrefix, string brand, decimal basePrice, int count)
    {
        var products = new List<ProductData>();
        var styles = new[] { "Classic", "Modern", "Vintage", "Elegant", "Casual", "Premium", "Essential", "Luxe", "Urban", "Boho" };
        var colors = new[] { "Black", "White", "Navy", "Burgundy", "Olive", "Beige", "Grey", "Coral", "Teal", "Blush" };

        for (int i = 1; i <= count; i++)
        {
            var style = styles[_random.Next(styles.Length)];
            var color = colors[_random.Next(colors.Length)];
            var name = $"{style} {baseName} in {color}";
            var price = Math.Round(basePrice + (_random.Next(-20, 30) * 0.99m), 2);
            var hasSale = _random.Next(0, 4) == 0; // 25% chance of sale
            var salePrice = hasSale ? Math.Round(price * (decimal)(0.7 + _random.NextDouble() * 0.2), 2) : (decimal?)null;

            // Generate unique image URL using picsum.photos with product-specific seed
            var imageId = 100 + _random.Next(900); // Random image ID between 100-999
            var imageUrl = $"https://picsum.photos/seed/{skuPrefix}{i}/600/800";

            products.Add(new ProductData
            {
                Name = name,
                Sku = $"{skuPrefix}-{i:D3}",
                Brand = brand,
                Price = price,
                SalePrice = salePrice,
                ShortDescription = $"Premium quality {baseName.ToLower()} featuring {style.ToLower()} design in {color.ToLower()}.",
                Description = $"Discover this stunning {style.ToLower()} {baseName.ToLower()} in a beautiful {color.ToLower()} finish. Crafted with premium materials for lasting quality and style. Perfect for any occasion, this piece combines comfort with sophistication. Available in multiple sizes.",
                IsFeatured = _random.Next(0, 8) == 0, // ~12% featured
                ImageUrl = imageUrl
            });
        }

        return products;
    }

    private List<ProductData> GenerateSaleProducts(int count)
    {
        var products = new List<ProductData>();
        var categories = new[] { "Dress", "Top", "Jeans", "Jacket", "Sweater", "Skirt", "Bag", "Shoes", "Watch", "Jewelry" };
        var brands = new[] { "Algora Fashion", "Algora Luxe", "Algora Sport", "Algora Basics" };

        for (int i = 1; i <= count; i++)
        {
            var category = categories[_random.Next(categories.Length)];
            var brand = brands[_random.Next(brands.Length)];
            var originalPrice = Math.Round(50 + _random.Next(150) + _random.Next(100) * 0.99m, 2);
            var discountPercent = _random.Next(20, 51); // 20-50% off
            var salePrice = Math.Round(originalPrice * (100 - discountPercent) / 100, 2);

            // Generate unique image URL for sale products
            var imageUrl = $"https://picsum.photos/seed/SALE{i}/600/800";

            products.Add(new ProductData
            {
                Name = $"Sale {category} - {discountPercent}% Off",
                Sku = $"SALE-{i:D3}",
                Brand = brand,
                Price = originalPrice,
                SalePrice = salePrice,
                ShortDescription = $"Amazing deal! Save {discountPercent}% on this {category.ToLower()}.",
                Description = $"Don't miss this incredible offer! This {category.ToLower()} was originally ${originalPrice:F2} and is now just ${salePrice:F2}. Limited time only - grab it before it's gone!",
                IsFeatured = true,
                ImageUrl = imageUrl
            });
        }

        return products;
    }

    // Data structures for category/product organization
    private class CategoryData
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int Order { get; set; }
        public List<SubCategoryData> SubCategories { get; set; } = new();
        public List<ProductData> Products { get; set; } = new();
    }

    private class SubCategoryData
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int Order { get; set; }
        public List<ProductData> Products { get; set; } = new();
    }

    private class ProductData
    {
        public string Name { get; set; } = "";
        public string Sku { get; set; } = "";
        public string Brand { get; set; } = "";
        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public string ShortDescription { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsFeatured { get; set; }
        public string ImageUrl { get; set; } = "";
    }
}
