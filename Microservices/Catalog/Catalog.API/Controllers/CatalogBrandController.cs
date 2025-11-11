using Catalog.API.Data;
using Catalog.API.Entitites;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Catalog.API.Controllers;

[ApiController]
[Route("api/catalog-brands")]
public class CatalogBrandController : Controller
{
    private readonly CatalogDbContext _db;

    public CatalogBrandController(CatalogDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetCatalogBrands()
    {
        try
        {
            Log.Information("GetCatalogBrands endpoint hit");
            var brandsList = _db.Brands.ToList();
            Log.Information("Catalog brands retrieved: {Count}", brandsList.Count);
            return Ok(brandsList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception retrieving catalog brands");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("add")]
    public async Task<IActionResult> AddCatalogBrands(string brandName)
    {
        try
        {
            Log.Information("AddCatalogBrands endpoint hit with name {BrandName}", brandName);
            var brand = new Brand { BrandName = brandName };

            _db.Brands.Add(brand);
            await _db.SaveChangesAsync();

            Log.Information("Brand added successfully: {BrandName}", brand.BrandName);
            return Ok($"{brand.BrandName} has been added successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception adding catalog brand {BrandName}", brandName);
            return BadRequest($"{ex.Message}");
        }
    }
    
}
