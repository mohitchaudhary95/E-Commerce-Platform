using DomainOrder = ECommerce.Order.Domain.Entities.Order;
using ECommerce.Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Order.Infrastructure.Persistence;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<DomainOrder> Orders => Set<DomainOrder>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

