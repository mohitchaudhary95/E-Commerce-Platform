using DomainInventory = ECommerce.Inventory.Domain.Entities.Inventory;
using ECommerce.Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Inventory.Infrastructure.Persistence;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

    public DbSet<DomainInventory> Inventories => Set<DomainInventory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

public class InventoryConfiguration : IEntityTypeConfiguration<DomainInventory>
{
    public void Configure(EntityTypeBuilder<DomainInventory> builder)
    {
        builder.ToTable("Inventories");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductId).IsRequired();
        builder.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(i => i.StockQuantity).IsRequired();
        builder.Property(i => i.ReservedQuantity).IsRequired();
        builder.Property(i => i.LowStockThreshold).IsRequired();

        // One inventory record per product
        builder.HasIndex(i => i.ProductId).IsUnique();

        builder.Ignore(i => i.AvailableQuantity);
        builder.Ignore(i => i.IsLowStock);
    }
}

