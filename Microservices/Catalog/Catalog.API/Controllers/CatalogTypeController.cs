using Catalog.API.Data;
using Catalog.API.Entitites;
using Catalog.API.Events;
using Microsoft.AspNetCore.Mvc;
using RabbitMQEventBus.Abstractions;

namespace Catalog.API.Controllers;

[ApiController]
[Route("api/catalog-types")]
public class CatalogTypeController : Controller
{

    private readonly CatalogDbContext _db;
    private readonly IEventBus _eventBus;

    public CatalogTypeController(CatalogDbContext db, IEventBus eventBus)
    {
        _db = db;
        _eventBus = eventBus;
    }

    [HttpGet()]
    public async Task<IActionResult> GetTypes()
    {
        var types = _db.Types.ToList();
        return Ok(types);
    }

    [HttpGet("add")]
    public async Task<IActionResult> AddTypes(string typeName)
    {
        try
        {
            _db.Types.Add(new Entitites.Type { TypeName = typeName });
            _db.SaveChanges();
            await _eventBus.PublishAsync(new TestEvent("MashAllah det virker inshaAllah."));
            return Ok($"Type with name '{typeName}' has been added");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

}


