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
            
            var testBrands = new List<Brand>
        {
            new Brand { Id = 1, BrandName = "Nike" },
            new Brand { Id = 2, BrandName = "Adidas" }
        }.AsQueryable();

           
            var mockSet = testBrands.AsMockDbSet();

           
            var mockContext = new Mock<CatalogDbContext>();
            mockContext.Setup(c => c.Brands).Returns(mockSet.Object);

            var controller = new CatalogBrandController(mockContext.Object);

            
            var result = await controller.GetCatalogBrands();

           
            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnedBrands = Assert.IsAssignableFrom<List<Brand>>(okResult.Value);
            Assert.Equal(2, returnedBrands.Count);
        }

        [Fact]
        public async Task GetCatalogBrands_ThrowsException_ReturnsBadRequest()
        {
           
            var mockContext = new Mock<CatalogDbContext>();

           
            mockContext.Setup(c => c.Brands).Throws(new InvalidOperationException("Simulated DB connection error"));

            var controller = new CatalogBrandController(mockContext.Object);

          
            var result = await controller.GetCatalogBrands();

           
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task AddCatalogBrands_SavesBrand_ReturnsOk()
        {
           
            const string newBrandName = "Puma";

            
            var mockSet = new Mock<DbSet<Brand>>();

          
            var mockContext = new Mock<CatalogDbContext>();

           
            mockContext.Setup(c => c.Brands).Returns(mockSet.Object);

         
            mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1); // Returner 1 række påvirket

            var controller = new CatalogBrandController(mockContext.Object);

            var result = await controller.AddCatalogBrands(newBrandName);

            
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal($"{newBrandName} has been added successfully", okResult.Value);

           
            mockSet.Verify(m => m.Add(It.IsAny<Brand>()), Times.Once());

         
            mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once());
        }

        [Fact]
        public async Task AddCatalogBrands_SaveChangesAsyncThrowsException_ReturnsBadRequest()
        {
     
            const string brandName = "FailingBrand";

            var mockSet = new Mock<DbSet<Brand>>();
            var mockContext = new Mock<CatalogDbContext>();

            mockContext.Setup(c => c.Brands).Returns(mockSet.Object);

           
            mockContext.Setup(c => c.SaveChangesAsync(default))
                .ThrowsAsync(new InvalidOperationException("Simulated unique constraint error"));

            var controller = new CatalogBrandController(mockContext.Object);

            
            var result = await controller.AddCatalogBrands(brandName);

           
            Assert.IsType<BadRequestObjectResult>(result);

            mockSet.Verify(m => m.Add(It.IsAny<Brand>()), Times.Once());

          
            mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once());
        }
    }


}



