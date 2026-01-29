using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Site.Data;

namespace UAlgora.Ecommerce.Site.Controllers.Api;

/// <summary>
/// API controller for seeding database with demo data.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class DatabaseSeederController : ControllerBase
{
    private readonly DemoDataSeeder _seeder;

    public DatabaseSeederController(DemoDataSeeder seeder)
    {
        _seeder = seeder;
    }

    /// <summary>
    /// Force reseeds the database with demo products including images.
    /// WARNING: This will delete all existing products and categories!
    /// GET /api/databaseseeder/seed
    /// </summary>
    [HttpGet("seed")]
    public async Task<IActionResult> SeedDatabase()
    {
        try
        {
            var (categories, products) = await _seeder.ForceSeedAsync();

            return Ok(new
            {
                success = true,
                categories,
                products,
                message = $"Database reseeded with {categories} categories and {products} products (with images from picsum.photos)"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// Seeds test discounts for all 12 discount types.
    /// WARNING: This will delete all existing discounts!
    /// GET /api/databaseseeder/seed-discounts
    /// </summary>
    [HttpGet("seed-discounts")]
    public async Task<IActionResult> SeedDiscounts()
    {
        try
        {
            var count = await _seeder.SeedDiscountsAsync();
            return Ok(new
            {
                success = true,
                discounts = count,
                message = $"Seeded {count} test discounts covering all 12 discount types"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message, stackTrace = ex.StackTrace });
        }
    }
}
