using Catalog.API.Controllers;
using Catalog.API.Data;
using Catalog.API.Entitites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Catalog.API.UnitTests
{
    public class CatalogBrandControllerTests
    {
        [Fact]
        public async Task GetCatalogBrands_ReturnsOkWithBrands()
        {
            // ARRANGE
            var testBrands = new List<Brand>
        {
            new Brand { Id = 1, BrandName = "Nike" },
            new Brand { Id = 2, BrandName = "Adidas" }
        }.AsQueryable();

            // Mock DbSet for Brands
            var mockSet = testBrands.AsMockDbSet();

            // Mock DbContext
            var mockContext = new Mock<CatalogDbContext>();
            mockContext.Setup(c => c.Brands).Returns(mockSet.Object);

            var controller = new CatalogBrandController(mockContext.Object);

            // ACT
            var result = await controller.GetCatalogBrands();

            // ASSERT
            // Tjek at resultatet er HTTP 200 OK
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Tjek at den returnerede liste har det forventede antal elementer
            var returnedBrands = Assert.IsAssignableFrom<List<Brand>>(okResult.Value);
            Assert.Equal(2, returnedBrands.Count);
        }

        [Fact]
        public async Task GetCatalogBrands_ThrowsException_ReturnsBadRequest()
        {
            // ARRANGE
            var mockContext = new Mock<CatalogDbContext>();

            // Tving en exception, når Brands tilgås (simulerer DB-fejl)
            mockContext.Setup(c => c.Brands).Throws(new InvalidOperationException("Simulated DB connection error"));

            var controller = new CatalogBrandController(mockContext.Object);

            // ACT
            var result = await controller.GetCatalogBrands();

            // ASSERT
            // Tjek at resultatet er HTTP 400 BadRequest
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task AddCatalogBrands_SavesBrand_ReturnsOk()
        {
            // ARRANGE
            const string newBrandName = "Puma";

            // 1. Mock DbSet
            var mockSet = new Mock<DbSet<Brand>>();

            // 2. Mock DbContext
            var mockContext = new Mock<CatalogDbContext>();

            // Setup Brands DbSet
            mockContext.Setup(c => c.Brands).Returns(mockSet.Object);

            // Setup SaveChangesAsync til at simulere succes
            mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1); // Returner 1 række påvirket

            var controller = new CatalogBrandController(mockContext.Object);

            // ACT
            var result = await controller.AddCatalogBrands(newBrandName);

            // ASSERT
            // Tjek at resultatet er HTTP 200 OK
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal($"{newBrandName} has been added successfully", okResult.Value);

            // Verificer (VIGTIGT!) at Add metoden blev kaldt på DbSet'en én gang.
            // Vi bruger It.IsAny<Brand>() fordi vi ikke er interesseret i det præcise objekt,
            // men at et objekt af typen Brand blev sendt til Add().
            mockSet.Verify(m => m.Add(It.IsAny<Brand>()), Times.Once());

            // Verificer (VIGTIGT!) at SaveChangesAsync blev kaldt på DbContext én gang.
            mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once());
        }

        [Fact]
        public async Task AddCatalogBrands_SaveChangesAsyncThrowsException_ReturnsBadRequest()
        {
            // ARRANGE
            const string brandName = "FailingBrand";

            var mockSet = new Mock<DbSet<Brand>>();
            var mockContext = new Mock<CatalogDbContext>();

            mockContext.Setup(c => c.Brands).Returns(mockSet.Object);

            // Tving SaveChangesAsync til at kaste en exception
            mockContext.Setup(c => c.SaveChangesAsync(default))
                .ThrowsAsync(new InvalidOperationException("Simulated unique constraint error"));

            var controller = new CatalogBrandController(mockContext.Object);

            // ACT
            var result = await controller.AddCatalogBrands(brandName);

            // ASSERT
            // Tjek at resultatet er HTTP 400 BadRequest
            Assert.IsType<BadRequestObjectResult>(result);

            // Verificer at Add() stadig blev kaldt (da exceptionen kommer efter Add())
            mockSet.Verify(m => m.Add(It.IsAny<Brand>()), Times.Once());

            // Verificer at SaveChangesAsync blev kaldt
            mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once());
        }
    }


}



