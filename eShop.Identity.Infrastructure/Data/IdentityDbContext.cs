using eShop.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace eShop.Identity.Infrastructure.Data
{
    public class IdentityDbContext : DbContext
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

        public DbSet<ApplicationUser> Users => Set<ApplicationUser>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Map til tabelnavn "Users" for konsistens med eShopOnWeb
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(u => u.Id);

                entity.Property(u => u.UserName).IsRequired().HasMaxLength(256);
                entity.Property(u => u.NormalizedUserName).HasMaxLength(256);
                entity.Property(u => u.Email).HasMaxLength(256);
                entity.Property(u => u.NormalizedEmail).HasMaxLength(256);

                // ✅ Rolle tilføjet for microservices
                entity.Property(u => u.Role)
                      .IsRequired()
                      .HasMaxLength(50)
                      .HasDefaultValue("User");
            });
        }
    }
}
