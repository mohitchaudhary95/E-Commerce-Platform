using ECommerce.Order.Domain.Entities;
using ECommerce.Order.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Order.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.UserId).IsRequired();

        builder.Property(o => o.UserEmail)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.ShippingAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        // Store enum as string for readability
        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Index for fetching a user's order history fast
        builder.HasIndex(o => o.UserId);

        // Composite index — orders by user ordered by date (most common query pattern)
        builder.HasIndex(o => new { o.UserId, o.CreatedAt });

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductId).IsRequired();

        builder.Property(i => i.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(i => i.Quantity).IsRequired();

        builder.Ignore(i => i.TotalPrice);
    }
}
