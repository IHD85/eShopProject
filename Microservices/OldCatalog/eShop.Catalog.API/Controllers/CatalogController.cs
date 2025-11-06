using eShop.Catalog.API.DTOs;
using eShop.Catalog.Domain.Entities;
using eShop.Catalog.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // 🔹 Tilføjet for JWT-roller

namespace eShop.Catalog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly CatalogDbContext _context;
        public CatalogController(CatalogDbContext context) => _context = context;

        // 🔹 Åben for alle brugere (User & Admin)
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _context.CatalogItems
                .Include(c => c.CatalogBrand)
                .Include(c => c.CatalogType)
                .ToListAsync());

        // 🔹 Åben for alle brugere (User & Admin)
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _context.CatalogItems
                .Include(c => c.CatalogBrand)
                .Include(c => c.CatalogType)
                .FirstOrDefaultAsync(x => x.Id == id);

            return item == null ? NotFound() : Ok(item);
        }

        // 🔹 Kun Admin må oprette nye produkter
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] CatalogItemCreateDto dto)
        {
            // ✅ Tjek at Brand og Type findes
            var brandExists = await _context.CatalogBrands.AnyAsync(b => b.Id == dto.CatalogBrandId);
            var typeExists = await _context.CatalogTypes.AnyAsync(t => t.Id == dto.CatalogTypeId);

            if (!brandExists || !typeExists)
                return BadRequest(new { message = "Invalid CatalogBrandId or CatalogTypeId" });

            var item = new CatalogItem
            {
                CatalogBrandId = dto.CatalogBrandId,
                CatalogTypeId = dto.CatalogTypeId,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                PictureUri = dto.PictureUri,
                AvailableStock = dto.AvailableStock
            };

            _context.CatalogItems.Add(item);
            await _context.SaveChangesAsync();
            return Ok(item);
        }

        // 🔹 Kun Admin må slette produkter
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _context.CatalogItems.FindAsync(id);
            if (item == null)
                return NotFound(new { message = "Item not found" });

            _context.CatalogItems.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
