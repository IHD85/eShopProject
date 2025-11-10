using Catalog.API.Data;
using Catalog.API.Entitites;
using Microsoft.AspNetCore.Mvc;

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
                var brandsList = _db.Brands.ToList();
                return Ok(brandsList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("add")]
        public async Task<IActionResult> AddCatalogBrands(string brandName)
        {
            try
            {
                var brand = new Brand { BrandName = brandName };

                _db.Brands.Add(brand);
                await _db.SaveChangesAsync();

                return Ok($"{brand.BrandName} has been added successfully");
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }
        }

            [HttpGet("besked")]
            public ActionResult<string> GetHelloMessage()
            {
                // Opretter en simpel tekst-streng
                string besked = "Hej fra min API Controller!";

                // Returnerer et "200 OK" svar med strengen som indhold
                return Ok(besked);
            }

            // En endnu simplere version, der returnerer direkte
            // Denne vil blive fundet på /api/hello
            [HttpGet]
            public string GetSimpleText()
            {
                return "Dette er en simpel tekst.";
            }

}

