using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Infrastructure.Data;

namespace UAlgora.Ecommerce.Site.Data;

public class DemoDataSeeder
{
    private readonly EcommerceDbContext _context;
    private readonly Random _random = new();

    public DemoDataSeeder(EcommerceDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Always seed currencies if not present
        await SeedCurrenciesAsync();

        if (_context.Products.Any())
        {
            return; // Products already seeded
        }

        var categories = await SeedCategoriesAsync();
        await SeedProductsAsync(categories);
        await _context.SaveChangesAsync();
    }

    public async Task<(int Categories, int Products)> ForceSeedAsync()
    {
        // Clear existing products and categories
        _context.Products.RemoveRange(_context.Products);
        _context.Categories.RemoveRange(_context.Categories);
        await _context.SaveChangesAsync();

        // Seed currencies if not present
        await SeedCurrenciesAsync();

        // Reseed categories and products
        var categories = await SeedCategoriesAsync();
        await SeedProductsAsync(categories);
        await _context.SaveChangesAsync();

        return (categories.Count, _context.Products.Count());
    }

    private async Task SeedCurrenciesAsync()
    {
        if (_context.Currencies.Any())
        {
            return; // Currencies already seeded
        }

        var currencies = new List<Currency>
        {
            new Currency
            {
                Code = "USD",
                Name = "US Dollar",
                Symbol = "$",
                NativeName = "US Dollar",
                DecimalPlaces = 2,
                DecimalSeparator = ".",
                ThousandsSeparator = ",",
                SymbolPosition = CurrencySymbolPosition.Before,
                SpaceBetweenSymbolAndAmount = false,
                IsDefault = true,
                IsActive = true,
                SortOrder = 1,
                Countries = new List<string> { "US" }
            },
            new Currency
            {
                Code = "EUR",
                Name = "Euro",
                Symbol = "€",
                NativeName = "Euro",
                DecimalPlaces = 2,
                DecimalSeparator = ",",
                ThousandsSeparator = ".",
                SymbolPosition = CurrencySymbolPosition.After,
                SpaceBetweenSymbolAndAmount = true,
                IsDefault = false,
                IsActive = true,
                SortOrder = 2,
                Countries = new List<string> { "DE", "FR", "IT", "ES", "NL", "BE", "AT", "IE", "PT", "FI" }
            },
            new Currency
            {
                Code = "GBP",
                Name = "British Pound",
                Symbol = "£",
                NativeName = "British Pound Sterling",
                DecimalPlaces = 2,
                DecimalSeparator = ".",
                ThousandsSeparator = ",",
                SymbolPosition = CurrencySymbolPosition.Before,
                SpaceBetweenSymbolAndAmount = false,
                IsDefault = false,
                IsActive = true,
                SortOrder = 3,
                Countries = new List<string> { "GB" }
            },
            new Currency
            {
                Code = "INR",
                Name = "Indian Rupee",
                Symbol = "₹",
                NativeName = "भारतीय रुपया",
                DecimalPlaces = 2,
                DecimalSeparator = ".",
                ThousandsSeparator = ",",
                SymbolPosition = CurrencySymbolPosition.Before,
                SpaceBetweenSymbolAndAmount = false,
                IsDefault = false,
                IsActive = true,
                SortOrder = 4,
                Countries = new List<string> { "IN" }
            },
            new Currency
            {
                Code = "JPY",
                Name = "Japanese Yen",
                Symbol = "¥",
                NativeName = "日本円",
                DecimalPlaces = 0,
                DecimalSeparator = ".",
                ThousandsSeparator = ",",
                SymbolPosition = CurrencySymbolPosition.Before,
                SpaceBetweenSymbolAndAmount = false,
                IsDefault = false,
                IsActive = true,
                SortOrder = 5,
                Countries = new List<string> { "JP" }
            },
            new Currency
            {
                Code = "CNY",
                Name = "Chinese Yuan",
                Symbol = "¥",
                NativeName = "人民币",
                DecimalPlaces = 2,
                DecimalSeparator = ".",
                ThousandsSeparator = ",",
                SymbolPosition = CurrencySymbolPosition.Before,
                SpaceBetweenSymbolAndAmount = false,
                IsDefault = false,
                IsActive = true,
                SortOrder = 6,
                Countries = new List<string> { "CN" }
            },
            new Currency
            {
                Code = "AUD",
                Name = "Australian Dollar",
                Symbol = "A$",
                NativeName = "Australian Dollar",
                DecimalPlaces = 2,
                DecimalSeparator = ".",
                ThousandsSeparator = ",",
                SymbolPosition = CurrencySymbolPosition.Before,
                SpaceBetweenSymbolAndAmount = false,
                IsDefault = false,
                IsActive = true,
                SortOrder = 7,
                Countries = new List<string> { "AU" }
            },
            new Currency
            {
                Code = "CAD",
                Name = "Canadian Dollar",
                Symbol = "C$",
                NativeName = "Canadian Dollar",
                DecimalPlaces = 2,
                DecimalSeparator = ".",
                ThousandsSeparator = ",",
                SymbolPosition = CurrencySymbolPosition.Before,
                SpaceBetweenSymbolAndAmount = false,
                IsDefault = false,
                IsActive = true,
                SortOrder = 8,
                Countries = new List<string> { "CA" }
            },
            new Currency
            {
                Code = "CHF",
                Name = "Swiss Franc",
                Symbol = "CHF",
                NativeName = "Schweizer Franken",
                DecimalPlaces = 2,
                DecimalSeparator = ".",
                ThousandsSeparator = "'",
                SymbolPosition = CurrencySymbolPosition.Before,
                SpaceBetweenSymbolAndAmount = true,
                IsDefault = false,
                IsActive = true,
                SortOrder = 9,
                Rounding = CurrencyRounding.ToIncrement,
                RoundingIncrement = 0.05m,
                Countries = new List<string> { "CH" }
            },
            new Currency
            {
                Code = "AED",
                Name = "UAE Dirham",
                Symbol = "د.إ",
                NativeName = "درهم إماراتي",
                DecimalPlaces = 2,
                DecimalSeparator = ".",
                ThousandsSeparator = ",",
                SymbolPosition = CurrencySymbolPosition.Before,
                SpaceBetweenSymbolAndAmount = true,
                IsDefault = false,
                IsActive = true,
                SortOrder = 10,
                Countries = new List<string> { "AE" }
            }
        };

        _context.Currencies.AddRange(currencies);
        await _context.SaveChangesAsync();
    }

    private async Task<Dictionary<string, Category>> SeedCategoriesAsync()
    {
        var categories = new Dictionary<string, Category>();

        var categoryData = new[]
        {
            ("Electronics", "electronic-devices", new[] { "Smartphones", "Laptops", "Tablets", "Audio", "Cameras", "Gaming" }),
            ("Clothing", "fashion-clothing", new[] { "Men's Wear", "Women's Wear", "Kids", "Shoes", "Accessories" }),
            ("Home & Garden", "home-garden", new[] { "Furniture", "Kitchen", "Decor", "Garden", "Bedding" }),
            ("Sports & Outdoors", "sports-outdoors", new[] { "Fitness", "Outdoor Gear", "Team Sports", "Water Sports", "Cycling" }),
            ("Books & Media", "books-media", new[] { "Fiction", "Non-Fiction", "Children's Books", "E-Books", "Audiobooks" }),
            ("Beauty & Health", "beauty-health", new[] { "Skincare", "Makeup", "Haircare", "Wellness", "Fragrances" }),
            ("Toys & Games", "toys-games", new[] { "Action Figures", "Board Games", "Puzzles", "Educational", "Outdoor Toys" }),
            ("Automotive", "automotive", new[] { "Car Electronics", "Parts", "Tools", "Accessories", "Care Products" })
        };

        foreach (var (name, slug, children) in categoryData)
        {
            var parent = new Category
            {
                Name = name,
                Slug = slug,
                Description = $"Browse our selection of {name.ToLower()} products",
                IsVisible = true,
                IsFeatured = true,
                SortOrder = categories.Count
            };
            _context.Categories.Add(parent);
            categories[name] = parent;

            var sortOrder = 0;
            foreach (var childName in children)
            {
                var child = new Category
                {
                    Name = childName,
                    Slug = $"{slug}-{childName.ToLower().Replace(" ", "-").Replace("'", "")}",
                    Description = $"Shop {childName}",
                    ParentId = parent.Id,
                    IsVisible = true,
                    SortOrder = sortOrder++
                };
                _context.Categories.Add(child);
                categories[childName] = child;
            }
        }

        await _context.SaveChangesAsync();
        return categories;
    }

    private async Task SeedProductsAsync(Dictionary<string, Category> categories)
    {
        var products = new List<Product>();

        // Electronics - 15 products
        products.AddRange(CreateElectronicsProducts(categories));

        // Clothing - 15 products
        products.AddRange(CreateClothingProducts(categories));

        // Home & Garden - 15 products
        products.AddRange(CreateHomeProducts(categories));

        // Sports - 12 products
        products.AddRange(CreateSportsProducts(categories));

        // Books - 12 products
        products.AddRange(CreateBooksProducts(categories));

        // Beauty - 12 products
        products.AddRange(CreateBeautyProducts(categories));

        // Toys - 12 products
        products.AddRange(CreateToysProducts(categories));

        // Automotive - 12 products
        products.AddRange(CreateAutomotiveProducts(categories));

        _context.Products.AddRange(products);
        await _context.SaveChangesAsync();
    }

    private IEnumerable<Product> CreateElectronicsProducts(Dictionary<string, Category> categories)
    {
        var electronicsId = categories["Electronics"].Id;
        var smartphonesId = categories["Smartphones"].Id;
        var laptopsId = categories["Laptops"].Id;
        var audioId = categories["Audio"].Id;

        return new[]
        {
            CreateProduct("iPhone 15 Pro Max", "IPHONE15PM", 1199.99m, 1099.99m, [electronicsId, smartphonesId], true, true, "The most powerful iPhone ever with A17 Pro chip"),
            CreateProduct("Samsung Galaxy S24 Ultra", "SAMS24U", 1299.99m, null, [electronicsId, smartphonesId], true, false, "Ultimate Android experience with S Pen"),
            CreateProduct("Google Pixel 8 Pro", "PIXEL8P", 999.99m, 899.99m, [electronicsId, smartphonesId], false, true, "AI-powered photography and pure Android"),
            CreateProduct("MacBook Pro 16\" M3", "MBP16M3", 2499.99m, null, [electronicsId, laptopsId], true, false, "Supercharged for pros with M3 chip"),
            CreateProduct("Dell XPS 15", "DELLXPS15", 1799.99m, 1599.99m, [electronicsId, laptopsId], true, true, "Stunning 4K OLED display"),
            CreateProduct("Lenovo ThinkPad X1 Carbon", "LNX1C", 1649.99m, null, [electronicsId, laptopsId], false, false, "Business laptop with legendary reliability"),
            CreateProduct("Sony WH-1000XM5", "SONYWH5", 399.99m, 349.99m, [electronicsId, audioId], true, true, "Industry-leading noise cancellation"),
            CreateProduct("Apple AirPods Pro 2", "AIRPODSP2", 249.99m, null, [electronicsId, audioId], true, false, "Active Noise Cancellation with spatial audio"),
            CreateProduct("Bose QuietComfort Ultra", "BOSEQCU", 429.99m, 379.99m, [electronicsId, audioId], false, true, "Immersive audio anywhere"),
            CreateProduct("iPad Pro 12.9\" M2", "IPADPM2", 1099.99m, 999.99m, [electronicsId], true, true, "The ultimate iPad experience"),
            CreateProduct("Samsung Galaxy Tab S9 Ultra", "TABS9U", 1199.99m, null, [electronicsId], false, false, "Massive 14.6\" display"),
            CreateProduct("Nintendo Switch OLED", "NSWITCH", 349.99m, 329.99m, [electronicsId], true, true, "Gaming on the go"),
            CreateProduct("PlayStation 5", "PS5", 499.99m, null, [electronicsId], true, false, "Next-gen gaming experience"),
            CreateProduct("Xbox Series X", "XBXSX", 499.99m, 449.99m, [electronicsId], false, true, "Most powerful Xbox ever"),
            CreateProduct("Canon EOS R6 Mark II", "CANONR6", 2499.99m, null, [electronicsId], false, false, "Professional mirrorless camera")
        };
    }

    private IEnumerable<Product> CreateClothingProducts(Dictionary<string, Category> categories)
    {
        var clothingId = categories["Clothing"].Id;
        var mensId = categories["Men's Wear"].Id;
        var womensId = categories["Women's Wear"].Id;
        var shoesId = categories["Shoes"].Id;

        return new[]
        {
            CreateProduct("Classic Cotton T-Shirt", "MCTEE", 29.99m, 24.99m, [clothingId, mensId], false, true, "Essential everyday comfort"),
            CreateProduct("Slim Fit Jeans", "MSLIMJ", 79.99m, null, [clothingId, mensId], true, false, "Perfect fit denim"),
            CreateProduct("Oxford Button-Down Shirt", "MOXFORD", 69.99m, 59.99m, [clothingId, mensId], false, true, "Business casual essential"),
            CreateProduct("Wool Blend Blazer", "MBLAZR", 199.99m, null, [clothingId, mensId], true, false, "Sophisticated style"),
            CreateProduct("Floral Summer Dress", "WFLORAL", 89.99m, 69.99m, [clothingId, womensId], true, true, "Perfect for sunny days"),
            CreateProduct("High-Waisted Yoga Pants", "WYOGA", 59.99m, null, [clothingId, womensId], true, false, "Ultimate comfort and flexibility"),
            CreateProduct("Cashmere Sweater", "WCASH", 149.99m, 129.99m, [clothingId, womensId], false, true, "Luxuriously soft"),
            CreateProduct("Silk Blouse", "WSILK", 119.99m, null, [clothingId, womensId], false, false, "Elegant and timeless"),
            CreateProduct("Nike Air Max 90", "NKAM90", 139.99m, 119.99m, [clothingId, shoesId], true, true, "Iconic streetwear"),
            CreateProduct("Adidas Ultraboost", "ADULB", 189.99m, null, [clothingId, shoesId], true, false, "Ultimate running comfort"),
            CreateProduct("Classic Leather Boots", "LBOOTS", 199.99m, 169.99m, [clothingId, shoesId], false, true, "Durable and stylish"),
            CreateProduct("Canvas Sneakers", "CNVSN", 49.99m, null, [clothingId, shoesId], false, false, "Casual everyday wear"),
            CreateProduct("Waterproof Rain Jacket", "WRAIN", 129.99m, 99.99m, [clothingId], true, true, "Stay dry in style"),
            CreateProduct("Cozy Fleece Hoodie", "FLEECH", 69.99m, null, [clothingId], true, false, "Warmth meets comfort"),
            CreateProduct("Performance Running Shorts", "RUNSH", 44.99m, 39.99m, [clothingId], false, true, "Lightweight and breathable")
        };
    }

    private IEnumerable<Product> CreateHomeProducts(Dictionary<string, Category> categories)
    {
        var homeId = categories["Home & Garden"].Id;
        var furnitureId = categories["Furniture"].Id;
        var kitchenId = categories["Kitchen"].Id;
        var decorId = categories["Decor"].Id;

        return new[]
        {
            CreateProduct("Modern Sectional Sofa", "SECSOFA", 1499.99m, 1299.99m, [homeId, furnitureId], true, true, "Spacious and comfortable"),
            CreateProduct("Ergonomic Office Chair", "ERGOCH", 349.99m, null, [homeId, furnitureId], true, false, "Work in comfort"),
            CreateProduct("Solid Oak Dining Table", "OAKDIN", 899.99m, 799.99m, [homeId, furnitureId], false, true, "Timeless craftsmanship"),
            CreateProduct("Memory Foam Mattress Queen", "MFMATT", 599.99m, null, [homeId, furnitureId], true, false, "Best sleep of your life"),
            CreateProduct("KitchenAid Stand Mixer", "KAMIX", 449.99m, 379.99m, [homeId, kitchenId], true, true, "Baking made easy"),
            CreateProduct("Instant Pot Duo", "INSTPOT", 99.99m, null, [homeId, kitchenId], true, false, "7-in-1 cooking"),
            CreateProduct("Ninja Air Fryer", "NJAFRYR", 129.99m, 99.99m, [homeId, kitchenId], false, true, "Crispy with less oil"),
            CreateProduct("Vitamix Professional Blender", "VTMXBL", 549.99m, null, [homeId, kitchenId], false, false, "Restaurant quality at home"),
            CreateProduct("Handwoven Area Rug 8x10", "AREARUG", 399.99m, 349.99m, [homeId, decorId], true, true, "Add warmth to any room"),
            CreateProduct("Decorative Throw Pillows Set", "THROWP", 79.99m, null, [homeId, decorId], false, false, "Instant room refresh"),
            CreateProduct("Modern Floor Lamp", "FLRLAMP", 149.99m, 129.99m, [homeId, decorId], true, true, "Ambient lighting"),
            CreateProduct("Canvas Wall Art Set", "WALLART", 199.99m, null, [homeId, decorId], false, false, "Gallery-worthy decor"),
            CreateProduct("Smart Robot Vacuum", "ROBOVAC", 499.99m, 399.99m, [homeId], true, true, "Effortless cleaning"),
            CreateProduct("Air Purifier HEPA", "AIRPUR", 249.99m, null, [homeId], true, false, "Breathe cleaner air"),
            CreateProduct("Electric Fireplace Insert", "ELECFP", 399.99m, 349.99m, [homeId], false, true, "Cozy ambiance")
        };
    }

    private IEnumerable<Product> CreateSportsProducts(Dictionary<string, Category> categories)
    {
        var sportsId = categories["Sports & Outdoors"].Id;
        var fitnessId = categories["Fitness"].Id;
        var outdoorId = categories["Outdoor Gear"].Id;

        return new[]
        {
            CreateProduct("Adjustable Dumbbell Set", "DUMBSET", 299.99m, 249.99m, [sportsId, fitnessId], true, true, "5-50 lbs adjustable"),
            CreateProduct("Yoga Mat Premium", "YOGAMAT", 49.99m, null, [sportsId, fitnessId], false, false, "Extra thick comfort"),
            CreateProduct("Resistance Bands Set", "RESBAND", 34.99m, 29.99m, [sportsId, fitnessId], false, true, "Full body workout"),
            CreateProduct("Treadmill Pro", "TREADP", 1299.99m, null, [sportsId, fitnessId], true, false, "Commercial grade"),
            CreateProduct("2-Person Camping Tent", "TENT2P", 199.99m, 169.99m, [sportsId, outdoorId], true, true, "Weatherproof adventure"),
            CreateProduct("Hiking Backpack 65L", "HIKEBP", 149.99m, null, [sportsId, outdoorId], false, false, "Trail ready"),
            CreateProduct("Portable Hammock", "PORTHAM", 79.99m, 64.99m, [sportsId, outdoorId], true, true, "Relax anywhere"),
            CreateProduct("LED Headlamp", "HEADLMP", 39.99m, null, [sportsId, outdoorId], false, false, "1000 lumens brightness"),
            CreateProduct("Mountain Bike 29\"", "MTBIKE", 799.99m, 699.99m, [sportsId], true, true, "Trail-ready performance"),
            CreateProduct("Basketball Official Size", "BBALL", 34.99m, null, [sportsId], false, false, "Indoor/outdoor use"),
            CreateProduct("Tennis Racket Pro", "TENNISP", 179.99m, 159.99m, [sportsId], false, true, "Tournament quality"),
            CreateProduct("Swimming Goggles Set", "SWIMGOG", 29.99m, null, [sportsId], false, false, "Anti-fog technology")
        };
    }

    private IEnumerable<Product> CreateBooksProducts(Dictionary<string, Category> categories)
    {
        var booksId = categories["Books & Media"].Id;
        var fictionId = categories["Fiction"].Id;
        var nonfictionId = categories["Non-Fiction"].Id;

        return new[]
        {
            CreateProduct("The Art of Programming", "ARTPROG", 49.99m, 39.99m, [booksId, nonfictionId], true, true, "Master coding concepts"),
            CreateProduct("Mystery Bestseller Collection", "MYSTCOL", 59.99m, null, [booksId, fictionId], true, false, "5-book box set"),
            CreateProduct("Self-Improvement Guide", "SELFGD", 24.99m, 19.99m, [booksId, nonfictionId], false, true, "Transform your life"),
            CreateProduct("Sci-Fi Epic Trilogy", "SCIFI3", 44.99m, null, [booksId, fictionId], false, false, "Space adventure awaits"),
            CreateProduct("Cookbook: World Cuisines", "WLDCOOK", 39.99m, 34.99m, [booksId, nonfictionId], true, true, "500+ recipes"),
            CreateProduct("Biography of Innovators", "BIOINOV", 29.99m, null, [booksId, nonfictionId], false, false, "Inspiring stories"),
            CreateProduct("Fantasy Kingdom Series", "FANTKNG", 54.99m, 44.99m, [booksId, fictionId], true, true, "Epic world-building"),
            CreateProduct("History of Ancient Worlds", "ANCWRLD", 34.99m, null, [booksId, nonfictionId], false, false, "Discover the past"),
            CreateProduct("Romance Novel Bundle", "ROMANBDL", 39.99m, 29.99m, [booksId, fictionId], false, true, "Heart-warming stories"),
            CreateProduct("Business Strategy Manual", "BIZSTRT", 44.99m, null, [booksId, nonfictionId], true, false, "Grow your success"),
            CreateProduct("Children's Adventure Series", "KIDADV", 29.99m, 24.99m, [booksId], true, true, "Ages 8-12"),
            CreateProduct("Photography Masterclass", "PHOTOMST", 49.99m, null, [booksId, nonfictionId], false, false, "Capture perfect shots")
        };
    }

    private IEnumerable<Product> CreateBeautyProducts(Dictionary<string, Category> categories)
    {
        var beautyId = categories["Beauty & Health"].Id;
        var skincareId = categories["Skincare"].Id;
        var makeupId = categories["Makeup"].Id;

        return new[]
        {
            CreateProduct("Vitamin C Serum", "VITCSER", 49.99m, 39.99m, [beautyId, skincareId], true, true, "Brightening formula"),
            CreateProduct("Hyaluronic Acid Moisturizer", "HYALMST", 44.99m, null, [beautyId, skincareId], true, false, "Deep hydration"),
            CreateProduct("Retinol Night Cream", "RETNCRM", 59.99m, 49.99m, [beautyId, skincareId], false, true, "Anti-aging powerhouse"),
            CreateProduct("SPF 50 Sunscreen", "SPF50", 24.99m, null, [beautyId, skincareId], false, false, "Daily protection"),
            CreateProduct("Eyeshadow Palette Pro", "EYEPAL", 54.99m, 44.99m, [beautyId, makeupId], true, true, "24 stunning shades"),
            CreateProduct("Liquid Foundation", "LIQFND", 39.99m, null, [beautyId, makeupId], true, false, "Flawless coverage"),
            CreateProduct("Mascara Volume Max", "MASCVOL", 24.99m, 19.99m, [beautyId, makeupId], false, true, "Bold lashes"),
            CreateProduct("Lipstick Collection", "LIPCOL", 49.99m, null, [beautyId, makeupId], false, false, "6 classic shades"),
            CreateProduct("Hair Growth Serum", "HAIRGRW", 69.99m, 59.99m, [beautyId], true, true, "Thicker, healthier hair"),
            CreateProduct("Electric Face Cleanser", "ELECFCC", 79.99m, null, [beautyId], true, false, "Deep pore cleaning"),
            CreateProduct("Aromatherapy Essential Oils", "AROMA12", 44.99m, 34.99m, [beautyId], false, true, "12-oil set"),
            CreateProduct("Luxury Perfume Set", "LUXPERF", 129.99m, null, [beautyId], false, false, "Designer fragrances")
        };
    }

    private IEnumerable<Product> CreateToysProducts(Dictionary<string, Category> categories)
    {
        var toysId = categories["Toys & Games"].Id;
        var boardId = categories["Board Games"].Id;
        var educationalId = categories["Educational"].Id;

        return new[]
        {
            CreateProduct("Building Blocks 1000pc", "BLOCKS1K", 49.99m, 39.99m, [toysId, educationalId], true, true, "Endless creativity"),
            CreateProduct("Remote Control Car", "RCCAR", 79.99m, null, [toysId], true, false, "High-speed fun"),
            CreateProduct("Strategy Board Game Deluxe", "STRTGDLX", 59.99m, 49.99m, [toysId, boardId], false, true, "2-6 players"),
            CreateProduct("Science Experiment Kit", "SCIKIT", 44.99m, null, [toysId, educationalId], true, false, "50+ experiments"),
            CreateProduct("Puzzle 1000 Pieces", "PZL1000", 24.99m, 19.99m, [toysId], false, true, "Beautiful landscape"),
            CreateProduct("Drone with Camera", "DRONECM", 149.99m, null, [toysId], true, false, "4K aerial footage"),
            CreateProduct("Art Supply Set", "ARTSET", 69.99m, 59.99m, [toysId, educationalId], false, true, "150+ pieces"),
            CreateProduct("Coding Robot for Kids", "CODEROBT", 89.99m, null, [toysId, educationalId], true, false, "Learn programming"),
            CreateProduct("Classic Card Game Set", "CARDSET", 29.99m, 24.99m, [toysId, boardId], false, true, "10 classic games"),
            CreateProduct("Stuffed Animal Giant", "STUFFGNT", 49.99m, null, [toysId], false, false, "Super soft cuddles"),
            CreateProduct("Electric Train Set", "TRAINSET", 129.99m, 99.99m, [toysId], true, true, "Classic railway"),
            CreateProduct("Musical Instrument Set", "MUSICSET", 79.99m, null, [toysId, educationalId], false, false, "Start playing today")
        };
    }

    private IEnumerable<Product> CreateAutomotiveProducts(Dictionary<string, Category> categories)
    {
        var autoId = categories["Automotive"].Id;
        var electronicsAutoId = categories["Car Electronics"].Id;
        var toolsId = categories["Tools"].Id;

        return new[]
        {
            CreateProduct("Dash Cam 4K", "DASHCM4K", 129.99m, 99.99m, [autoId, electronicsAutoId], true, true, "Crystal clear recording"),
            CreateProduct("Bluetooth Car Adapter", "BTCAR", 29.99m, null, [autoId, electronicsAutoId], false, false, "Stream music wirelessly"),
            CreateProduct("GPS Navigation System", "GPSNAV", 199.99m, 179.99m, [autoId, electronicsAutoId], true, true, "Real-time traffic"),
            CreateProduct("Car Jump Starter", "CARJUMP", 79.99m, null, [autoId, toolsId], true, false, "Emergency power"),
            CreateProduct("Tire Inflator Portable", "TIREINF", 49.99m, 39.99m, [autoId, toolsId], false, true, "Digital pressure gauge"),
            CreateProduct("Car Vacuum Cleaner", "CARVAC", 44.99m, null, [autoId], false, false, "Powerful suction"),
            CreateProduct("LED Headlight Bulbs", "LEDHEAD", 59.99m, 49.99m, [autoId], true, true, "Brighter nights"),
            CreateProduct("Seat Covers Premium", "SEATCVR", 89.99m, null, [autoId], false, false, "Universal fit"),
            CreateProduct("Car Cleaning Kit", "CARCLEAN", 39.99m, 29.99m, [autoId], true, true, "Complete detail set"),
            CreateProduct("Phone Mount Wireless", "PHNEMNT", 34.99m, null, [autoId, electronicsAutoId], false, false, "Wireless charging"),
            CreateProduct("Mechanic Tool Set 200pc", "MCHTOOL", 149.99m, 129.99m, [autoId, toolsId], false, true, "Professional grade"),
            CreateProduct("Floor Mats All Weather", "FLRMATS", 69.99m, null, [autoId], true, false, "Heavy-duty protection")
        };
    }

    private Product CreateProduct(string name, string sku, decimal basePrice, decimal? salePrice, List<Guid> categoryIds, bool isFeatured, bool isOnSale, string description)
    {
        var slug = name.ToLower()
            .Replace(" ", "-")
            .Replace("\"", "")
            .Replace("'", "")
            .Replace("&", "and");

        var product = new Product
        {
            Name = name,
            Sku = sku,
            Slug = slug,
            Description = $"<p>{description}</p><p>This is a high-quality product that meets all your needs. Crafted with precision and attention to detail.</p><ul><li>Premium quality materials</li><li>Excellent durability</li><li>Great value for money</li></ul>",
            ShortDescription = description,
            BasePrice = basePrice,
            SalePrice = isOnSale ? salePrice : null,
            CurrencyCode = "USD",
            TrackInventory = true,
            StockQuantity = _random.Next(10, 100),
            LowStockThreshold = 5,
            AllowBackorders = false,
            Weight = (decimal)(_random.NextDouble() * 5 + 0.5),
            WeightUnit = "kg",
            CategoryIds = categoryIds,
            Status = ProductStatus.Published,
            IsFeatured = isFeatured,
            IsVisible = true,
            SortOrder = _random.Next(1, 100),
            PrimaryImageUrl = $"https://picsum.photos/seed/{sku}/400/400",
            MetaTitle = name,
            MetaDescription = description,
            Brand = GetRandomBrand(),
            Tags = GetRandomTags()
        };

        return product;
    }

    public async Task<int> SeedDiscountsAsync()
    {
        // Remove existing discounts
        _context.Discounts.RemoveRange(_context.Discounts);
        await _context.SaveChangesAsync();

        // Get some product IDs for product-specific discounts
        var productIds = _context.Products.Select(p => p.Id).Take(6).ToList();
        var categoryIds = _context.Categories.Select(c => c.Id).Take(3).ToList();

        var discounts = new List<Discount>
        {
            // 1. BOGO - Buy One Get One Free
            new()
            {
                Name = "BOGO: Buy 1 Get 1 Free",
                Description = "Buy one product and get one of equal or lesser value for free! Great for stocking up on your favorites.",
                Code = "BOGO2025",
                Type = DiscountType.BuyXGetY,
                Scope = DiscountScope.Product,
                Value = 0,
                BuyQuantity = 1,
                GetQuantity = 1,
                ApplicableProductIds = productIds.Take(3).ToList(),
                IsActive = true,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(30),
                TotalUsageLimit = 500,
                PerCustomerLimit = 2,
                CanCombine = false,
                Priority = 10
            },
            // 2. Percentage Sale
            new()
            {
                Name = "Flash Sale: 25% Off Sitewide",
                Description = "Take 25% off your entire order! Limited time offer. Percentage discount applied to all eligible items in your cart.",
                Code = "FLASH25",
                Type = DiscountType.Percentage,
                Scope = DiscountScope.Order,
                Value = 25,
                MaxDiscountAmount = 100,
                MinimumOrderAmount = 50,
                IsActive = true,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(7),
                TotalUsageLimit = 1000,
                PerCustomerLimit = 1,
                CanCombine = false,
                Priority = 8
            },
            // 3. Early Payment Discount
            new()
            {
                Name = "Early Payment: 2% Off Net 30",
                Description = "Pay within 10 days of invoice and receive 2% off your order total. Standard payment term is 30 days.",
                Type = DiscountType.EarlyPayment,
                Scope = DiscountScope.Order,
                Value = 2,
                EarlyPaymentDays = 10,
                StandardPaymentDays = 30,
                IsActive = true,
                CanCombine = true,
                Priority = 1
            },
            // 4. Overstock / Clearance Sale
            new()
            {
                Name = "Clearance: Up to 40% Off Overstock",
                Description = "We're clearing out surplus inventory! Grab these items at 40% off before they're gone. Limited quantities available.",
                Code = "CLEARANCE40",
                Type = DiscountType.Overstock,
                Scope = DiscountScope.Product,
                Value = 40,
                IsOverstockClearance = true,
                ApplicableProductIds = productIds.Skip(3).Take(3).ToList(),
                IsActive = true,
                StartDate = DateTime.UtcNow.AddDays(-5),
                EndDate = DateTime.UtcNow.AddDays(14),
                CanCombine = false,
                Priority = 5
            },
            // 5. Free Shipping
            new()
            {
                Name = "Free Shipping on Orders $75+",
                Description = "Enjoy free standard shipping on all orders of $75 or more. No code needed — discount applied automatically at checkout.",
                Type = DiscountType.FreeShipping,
                Scope = DiscountScope.Shipping,
                Value = 0,
                MinimumOrderAmount = 75,
                IsActive = true,
                CanCombine = true,
                Priority = 2
            },
            // 6. Price Bundle
            new()
            {
                Name = "Tech Bundle: 15% Off When You Buy All 3",
                Description = "Purchase all three featured tech products together and save 15%! Bundle includes products from our top sellers.",
                Code = "TECHBUNDLE",
                Type = DiscountType.Bundle,
                Scope = DiscountScope.Product,
                Value = 15,
                BundleProductIds = productIds.Take(3).ToList(),
                BundleDiscountValue = 15,
                IsActive = true,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(60),
                TotalUsageLimit = 200,
                CanCombine = false,
                Priority = 7
            },
            // 7. Bulk / Volume Discount
            new()
            {
                Name = "Bulk Order Volume Discounts",
                Description = "The more you buy, the more you save! Buy 10+ items for 10% off, 25+ for 15% off, 50+ for 20% off, or 100+ for 25% off.",
                Type = DiscountType.BulkVolume,
                Scope = DiscountScope.Order,
                Value = 10,
                VolumeTiers = [
                    new VolumeTier { MinQuantity = 10, DiscountPercent = 10 },
                    new VolumeTier { MinQuantity = 25, DiscountPercent = 15 },
                    new VolumeTier { MinQuantity = 50, DiscountPercent = 20 },
                    new VolumeTier { MinQuantity = 100, DiscountPercent = 25 }
                ],
                IsActive = true,
                CanCombine = false,
                Priority = 6
            },
            // 8. Seasonal Discount
            new()
            {
                Name = "Summer Sale: 30% Off Selected Items",
                Description = "Beat the heat with our Summer Sale! 30% off selected seasonal items. Perfect time to stock up on summer essentials.",
                Code = "SUMMER30",
                Type = DiscountType.Seasonal,
                Scope = DiscountScope.Product,
                Value = 30,
                SeasonLabel = "Summer Sale 2026",
                ApplicableCategoryIds = categoryIds.Take(2).ToList(),
                IsActive = true,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(45),
                TotalUsageLimit = 2000,
                CanCombine = false,
                Priority = 9
            },
            // 9. Referral Discount
            new()
            {
                Name = "Refer a Friend: Both Get 15% Off",
                Description = "Share your unique referral code with friends! Both you and your friend get 15% off your next purchase. Two-way savings!",
                Code = "REFER15",
                Type = DiscountType.Referral,
                Scope = DiscountScope.Order,
                Value = 15,
                ReferralTwoWay = true,
                ReferralNewCustomerValue = 15,
                MinimumOrderAmount = 30,
                IsActive = true,
                PerCustomerLimit = 5,
                CanCombine = false,
                Priority = 4
            },
            // 10. Loyalty Program Discount
            new()
            {
                Name = "Gold Members: 20% Off Exclusive",
                Description = "Exclusive for Gold tier loyalty members! Earn points with every purchase and unlock 20% off when you reach 500 points.",
                Type = DiscountType.LoyaltyProgram,
                Scope = DiscountScope.Order,
                Value = 20,
                LoyaltyPointsThreshold = 500,
                LoyaltyTierRequired = "Gold",
                IsActive = true,
                CanCombine = true,
                Priority = 3
            },
            // 11. Email Subscription Discount
            new()
            {
                Name = "Newsletter Signup: 10% Off First Order",
                Description = "Welcome! Sign up for our newsletter and get 10% off your first order. Also used for cart abandonment recovery emails.",
                Code = "WELCOME10",
                Type = DiscountType.EmailSubscription,
                Scope = DiscountScope.Order,
                Value = 10,
                RequiresEmailSubscription = true,
                IsCartAbandonmentRecovery = true,
                FirstTimeCustomerOnly = true,
                IsActive = true,
                PerCustomerLimit = 1,
                CanCombine = false,
                Priority = 3
            },
            // 12. Trade-In Credit
            new()
            {
                Name = "Trade-In: $50 Credit Per Device",
                Description = "Trade in your old device and receive $50 credit toward the purchase of a new one. Bring in any qualifying product.",
                Code = "TRADEIN50",
                Type = DiscountType.TradeInCredit,
                Scope = DiscountScope.Product,
                Value = 50,
                TradeInCreditPerItem = 50,
                TradeInProductIds = productIds.Take(2).ToList(),
                TradeInTargetProductIds = productIds.Skip(2).Take(2).ToList(),
                IsActive = true,
                CanCombine = false,
                Priority = 5
            }
        };

        _context.Discounts.AddRange(discounts);
        await _context.SaveChangesAsync();
        return discounts.Count;
    }

    private string GetRandomBrand()
    {
        var brands = new[] { "TechPro", "StyleLife", "HomeEssentials", "ActiveGear", "PureBeauty", "SmartLiving", "EcoChoice", "PremiumPlus" };
        return brands[_random.Next(brands.Length)];
    }

    private List<string> GetRandomTags()
    {
        var allTags = new[] { "bestseller", "new-arrival", "trending", "eco-friendly", "premium", "value", "gift-idea", "popular" };
        return allTags.OrderBy(_ => _random.Next()).Take(_random.Next(2, 4)).ToList();
    }
}
