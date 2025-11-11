using Catalog.API.Data;
using Catalog.API.Dto;
using Catalog.API.Entitites;
using Catalog.API.Events;
using Catalog.API.Services.EventBusService;
using Microsoft.AspNetCore.Mvc;
using RabbitMQEventBus.Abstractions;
using Serilog;

namespace Catalog.API.Controllers;

[ApiController]
[Route("/api/catalog-items")]
public class CatalogItemController : Controller
{
    private readonly CatalogDbContext _db;
    private readonly IEventBus _eventBus;

    public CatalogItemController(CatalogDbContext db, IEventBus eventBus)
    {
        _db = db;
        _eventBus = eventBus;
    }

    [HttpGet]
    public IActionResult GetCatalogItems(int pageSize, int pageIndex, int catalogBrandId, int catalogTypeId)
    {
        try
        {
            Log.Information($"GetCatalogItems endpoint hit");
            var itemsList = _db.Items.ToList();
            Log.Information($"Catalog items retrieved: {itemsList.Count}");
            return Ok(itemsList);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Exception retrieving catalog items");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{catalogItemId:int}")]
    public IActionResult GetCatalogItemById(int catalogItemId)
    {
        try
        {
            Log.Information($"GetCatalogItemById endpoint hit for Id {catalogItemId}");
            var item = _db.Items.First(item => item.Id == catalogItemId);
            Log.Information($"Catalog item retrieved: {catalogItemId}");
            return Ok(item);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Exception retrieving catalog item Id {catalogItemId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{catalogItemId:int}")]
    public async Task<IActionResult> DeleteCatalogItems(int catalogItemId)
    {
        try
        {
            Log.Information($"DeleteCatalogItems endpoint hit for Id {catalogItemId}");
            var item = _db.Items.First(item => item.Id == catalogItemId);

            _db.Items.Remove(item);
            await _db.SaveChangesAsync();

            Log.Information($"Catalog item deleted: {catalogItemId}");
            return Ok($"{item.Name} has been deleted.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Exception deleting catalog item Id {catalogItemId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost()]
    public async Task<IActionResult> AddCatalogItem([FromBody] ItemDto catalogItemDto)
    {
        try
        {
            Log.Information($"AddCatalogItem endpoint hit");
            Item catalogItem = new Item
            {
                Description = catalogItemDto.Description,
                Price = catalogItemDto.Price,
                PictureUri = catalogItemDto.PictureUri,
                CatalogBrandId = catalogItemDto.CatalogBrandId,
                CatalogTypeId = catalogItemDto.CatalogTypeId,
                Name = catalogItemDto.Name,
            };
            _db.Items.Add(catalogItem);
            await _db.SaveChangesAsync();

            Log.Information($"Catalog item added: {catalogItem.Name}");
            return Ok($"Item: '{catalogItem.Description}' has been added");
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Exception adding catalog item");
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/price")]
    public async Task<IActionResult> UpdatePrice(int id, [FromBody] decimal price)
    {
        try
        {
            Log.Information($"UpdatePrice endpoint hit for Id {id}");
            var item = await _db.Items.FindAsync(id);
            if (item == null)
            {
                Log.Warning($"Item not found for Id {id}");
                return NotFound();
            }

            item.Price = price;
            bool changesSaved = await _db.SaveChangesAsync() != 0;

            if (!changesSaved)
            {
                Log.Warning($"No changes saved for Id {id}");
                return BadRequest();
            }

            Log.Information($"Price updated for Id {id}. Publishing event.");
            await _eventBus.PublishAsync(new ProductPriceChangedEvent(item.Id, (decimal)item.Price));

            return Ok(item.Price);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Exception updating price for Id {id}");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("test-event/{fakeProductId:int}/{fakePrice:decimal}")]
    public async Task<IActionResult> EventTesting(int fakeProductId, decimal fakePrice)
    {
        try
        {
            Log.Information($"EventTesting endpoint hit with fakeProductId {fakeProductId} and fakePrice {fakePrice}");
            await _eventBus.PublishAsync(new ProductPriceChangedEvent(fakeProductId, fakePrice));
            Log.Information($"Test event published for fake product Id {fakeProductId}");
            return Ok("maybe it worked bro.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Exception publishing test event");
            return BadRequest(ex.Message);
        }
    }
}
