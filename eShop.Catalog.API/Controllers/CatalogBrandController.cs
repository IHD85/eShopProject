using eShop.Catalog.Domain.Entities;
using eShop.Catalog.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eShop.Catalog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogBrandController : ControllerBase
    {
        private readonly CatalogDbContext _context;
        public CatalogBrandController(CatalogDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _context.CatalogBrands.ToListAsync());

        [HttpPost]
        public async Task<IActionResult> AddBrand([FromBody] CatalogBrand brand)
        {
            if (string.IsNullOrWhiteSpace(brand.Brand))
                return BadRequest(new { message = "Brand name is required" });

            _context.CatalogBrands.Add(brand);
            await _context.SaveChangesAsync();
            return Ok(brand);
        }
    }
}
