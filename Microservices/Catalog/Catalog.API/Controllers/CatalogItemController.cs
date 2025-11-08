using Catalog.API.Data;
using Catalog.API.Dto;
using Catalog.API.Entitites;
using Catalog.API.Events;
using Catalog.API.Services.EventBusService;
using Microsoft.AspNetCore.Mvc;
using RabbitMQEventBus.Abstractions;

namespace Catalog.API.Controllers;

[ApiController]
[Route("/api/catalog-items")]
public class CatalogItemController : Controller
{
    //private readonly EventBusService _eventBusService;
    private readonly CatalogDbContext _db;
    private readonly IEventBus _eventBus;

    public CatalogItemController(CatalogDbContext db,  IEventBus eventBus)
    {
        _db = db;
        _eventBus = eventBus;
    }

    [HttpGet]
    public IActionResult GetCatalogItems(int pageSize, int pageIndex, int catalogBrandId, int catalogTypeId)
    {
        try
        {
            var itemsList = _db.Items.ToList();

            return Ok(itemsList);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

    }

    [HttpGet("{catalogItemId:int}")]
    public IActionResult GetCatalogItemById(int catalogItemId)
    {
        try
        {
            var item = _db.Items.First(item => item.Id == catalogItemId);
            return Ok(item);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{catalogItemId:int}")]
    public async Task<IActionResult> DeleteCatalogItems(int catalogItemId)
    {
        try
        {
            var item = _db.Items.First(item => item.Id == catalogItemId);

            _db.Items.Remove(item);
            await _db.SaveChangesAsync();

            return Ok(item.Name + " has been deleted.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost()]
    public async Task<IActionResult> AddCatalogItem([FromBody] ItemDto catalogItemDto)
    {
        try
        {
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

            //await _eventBusService.PublishAsync(catalogItem, "routeMe");
            return Ok($"Item: '{catalogItem.Description}' has been added");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/price")]
    public async Task<IActionResult> UpdatePrice(int id, [FromBody] decimal price)
    {
        var item = await _db.Items.FindAsync(id);
        if (item == null) return NotFound();

        item.Price = price;
        bool changesSaved = await _db.SaveChangesAsync() != 0;
        
        if (!changesSaved) return BadRequest();

        if (changesSaved)
        {
            await _eventBus.PublishAsync(new ProductPriceChangedEvent(item.Id, (decimal)item.Price));
        }

        return Ok(item.Price);
    }
    
    [HttpGet("test-event/{fakeProductId:int}/{fakePrice:decimal}")]
    public async Task<IActionResult> EventTesting(int fakeProductId, decimal fakePrice)
    {
   
            await _eventBus.PublishAsync(new ProductPriceChangedEvent(fakeProductId, fakePrice));
        

        return Ok("maybe it worked bro.");
    }
    



}

