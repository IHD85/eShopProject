using Catalog.API.Entitites;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Data
{
    public partial class CatalogDbContext : DbContext
    {
        public CatalogDbContext()
        {
        }

        public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Brand> Brands { get; set; }

        public virtual DbSet<Item> Items { get; set; }

        public virtual DbSet<Entitites.Type> Types { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Brand>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("brand_pkey");

                entity.ToTable("brand");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.BrandName).HasColumnName("brand_name");
            });

            modelBuilder.Entity<Item>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("item_pkey");

                entity.ToTable("item");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CatalogBrandId).HasColumnName("catalog_brand_id");
                entity.Property(e => e.CatalogTypeId).HasColumnName("catalog_type_id");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.PictureUri).HasColumnName("picture_uri");
                entity.Property(e => e.Price).HasColumnName("price");
                entity.Property(e => e.Name).HasColumnName("name");

                entity.HasOne(d => d.CatalogBrand).WithMany()
                    .HasForeignKey(d => d.CatalogBrandId)
                    .HasConstraintName("item_catalog_brand_id_fkey");

                entity.HasOne(d => d.CatalogType).WithMany()
                    .HasForeignKey(d => d.CatalogTypeId)
                    .HasConstraintName("item_catalog_type_id_fkey");
            });

            modelBuilder.Entity<Entitites.Type>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("type_pkey");

                entity.ToTable("type");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.TypeName).HasColumnName("type_name");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
