using Microsoft.EntityFrameworkCore;
using SmartShop.API.Models;

namespace SmartShop.API.Data;

public class SmartShopDbContext : DbContext
{
    public SmartShopDbContext(DbContextOptions<SmartShopDbContext> options) : base(options) { }
    public DbSet<Product> Products { get; set; }
    public DbSet<Review> Reviews { get; set; }
}
