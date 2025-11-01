using eShop.Order.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace eShop.Order.Infrastructure.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

        public DbSet<OrderEntity> Orders { get; set; }
        public DbSet<OrderItemEntity> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<OrderEntity>().ToTable("Orders");
            builder.Entity<OrderItemEntity>().ToTable("OrderItems");

            builder.Entity<OrderEntity>()
                .HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey(i => i.OrderEntityId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
