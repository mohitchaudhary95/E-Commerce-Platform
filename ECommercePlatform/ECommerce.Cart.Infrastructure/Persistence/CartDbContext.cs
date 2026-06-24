using CartEntity = ECommerce.Cart.Domain.Entities.Cart;
using Microsoft.EntityFrameworkCore;
using ECommerce.Cart.Domain.Entities;
 
namespace ECommerce.Cart.Infrastructure.Persistence;
 
public class CartDbContext : DbContext
{
    public CartDbContext(DbContextOptions<CartDbContext> options) : base(options) { }
 
    public DbSet<CartEntity> Carts => Set<CartEntity>();
 
    public DbSet<CartItem> CartItems => Set<CartItem>();
 
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CartDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}