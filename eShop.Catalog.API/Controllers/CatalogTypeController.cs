using eShop.Catalog.Domain.Entities;
using eShop.Catalog.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eShop.Catalog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogTypeController : ControllerBase
    {
        private readonly CatalogDbContext _context;
        public CatalogTypeController(CatalogDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _context.CatalogTypes.ToListAsync());

        [HttpPost]
        public async Task<IActionResult> AddType([FromBody] CatalogType type)
        {
            if (string.IsNullOrWhiteSpace(type.Type))
                return BadRequest(new { message = "Type name is required" });

            _context.CatalogTypes.Add(type);
            await _context.SaveChangesAsync();
            return Ok(type);
        }
    }
}
