using Catalog.API.Data;
using Catalog.API.Entitites;
using Catalog.API.Events;
using Microsoft.AspNetCore.Mvc;
using RabbitMQEventBus.Abstractions;
using Serilog;

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
        try
        {
            Log.Information($"GetTypes endpoint hit");
            var types = _db.Types.ToList();
            Log.Information($"Catalog types retrieved: {types.Count}");
            return Ok(types);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Exception retrieving catalog types");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("add")]
    public async Task<IActionResult> AddTypes(string typeName)
    {
        try
        {
            Log.Information($"AddTypes endpoint hit with name {typeName}");
            _db.Types.Add(new Entitites.Type { TypeName = typeName });
            _db.SaveChanges();
            await _eventBus.PublishAsync(new TestEvent("MashAllah det virker inshaAllah."));
            Log.Information($"Catalog type added: {typeName}");
            return Ok($"Type with name '{typeName}' has been added");
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Exception adding catalog type {typeName}");
            return BadRequest(ex.Message);
        }
    }
}