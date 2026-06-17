using DomainCart = ECommerce.Cart.Domain.Entities.Cart;
using ECommerce.Cart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Cart.Infrastructure.Persistence.Configurations
{
    public class CartConfiguration : IEntityTypeConfiguration<DomainCart>
    {
        public void Configure(EntityTypeBuilder<DomainCart> builder)
        {
            builder.ToTable("Carts");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.UserId).IsRequired();

            // One user ? one cart. Fast lookup when loading cart by user.
            builder.HasIndex(c => c.UserId).IsUnique();

            builder.HasMany(c => c.Items)
                .WithOne(i => i.Cart)
                .HasForeignKey(i => i.CartId)
                .OnDelete(DeleteBehavior.Cascade); // Delete items when cart is deleted

            // Computed properties — EF Core must not try to map these to columns
            builder.Ignore(c => c.TotalAmount);
            builder.Ignore(c => c.TotalItems);
        }
    }

    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.ToTable("CartItems");
            builder.HasKey(i => i.Id);

            // No FK constraint to Products table — Products live in a DIFFERENT database
            builder.Property(i => i.ProductId).IsRequired();

            builder.Property(i => i.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(i => i.ProductImageUrl)
                .HasMaxLength(500);

            builder.Property(i => i.UnitPrice)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(i => i.Quantity).IsRequired();

            // Unique constraint: a product can only appear once per cart
            // If user adds same product twice ? quantity increases, not two rows
            builder.HasIndex(i => new { i.CartId, i.ProductId }).IsUnique();

            builder.Ignore(i => i.TotalPrice);
        }
    }

}

