using eShop.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace eShop.Identity.Infrastructure.Data
{
    public class IdentityDbContext : DbContext
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }
        public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    }
}
