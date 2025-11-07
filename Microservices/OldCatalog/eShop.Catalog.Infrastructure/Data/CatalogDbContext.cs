using eShop.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace eShop.Catalog.Infrastructure.Data
{
    public class CatalogDbContext : DbContext
    {
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

        public DbSet<CatalogItem> CatalogItems => Set<CatalogItem>();
        public DbSet<CatalogBrand> CatalogBrands => Set<CatalogBrand>();
        public DbSet<CatalogType> CatalogTypes => Set<CatalogType>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<CatalogItem>()
                .HasOne(c => c.CatalogBrand)
                .WithMany()
                .HasForeignKey(c => c.CatalogBrandId);

            builder.Entity<CatalogItem>()
                .HasOne(c => c.CatalogType)
                .WithMany()
                .HasForeignKey(c => c.CatalogTypeId);
        }
    }
}
