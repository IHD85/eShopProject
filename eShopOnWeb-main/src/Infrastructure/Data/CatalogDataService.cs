using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.Extensions.Logging;

namespace Microsoft.eShopWeb.Infrastructure.Data;

public class CatalogDataService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CatalogDataService> _logger;

    public CatalogDataService(HttpClient httpClient, ILogger<CatalogDataService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<CatalogBrand>> GetCatalogBrandsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<CatalogBrand>>("catalog/catalog-brands");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to load Brand stuff from the API");
            throw;
        }

    }

    public async Task<IEnumerable<CatalogType>> GetCatalogTypesAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<CatalogType>>("catalog/catalog-types");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load Type stuff from the API");
            throw;
        }

    }

    public async Task<IEnumerable<CatalogItem>> GetCatalogItemsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<CatalogItem>>("catalog/catalog-items");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load Catalog items stuff from the API");
            throw;
        }

    }
}
