using DomainProduct = global::ECommerce.Product.Domain.Entities.Product;
using DomainCategory = global::ECommerce.Product.Domain.Entities.Category;
using ECommerce.Product.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Product.Infrastructure.Persistence.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Domain.Entities.Product>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.Product> builder)
        {
            builder.ToTable("Products");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(2000);

            // Decimal precision — 18 digits total, 2 after decimal point
            // This matches SQL Server's money column precision
            builder.Property(p => p.Price)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(p => p.ImageUrl)
                .HasMaxLength(500);

            // Index on Name for fast search queries
            builder.HasIndex(p => p.Name);

            // Index on CategoryId for fast filtering by category
            builder.HasIndex(p => p.CategoryId);

            // Composite index — speeds up the common query: active products in a category
            builder.HasIndex(p => new { p.CategoryId, p.IsActive });

            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Don't cascade-delete products when category deleted
        }
    }

    public class CategoryConfiguration : IEntityTypeConfiguration<DomainCategory>
    {
        public void Configure(EntityTypeBuilder<DomainCategory> builder)
        {
            builder.ToTable("Categories");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            // Unique category names
            builder.HasIndex(c => c.Name).IsUnique();

            // Seed some initial categories so the app works out of the box
            builder.HasData(
                new DomainCategory { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Electronics", Description = "Electronic devices and accessories", IsActive = true, CreatedAt = DateTime.UtcNow },
                new DomainCategory { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Clothing", Description = "Apparel and fashion", IsActive = true, CreatedAt = DateTime.UtcNow },
                new DomainCategory { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Books", Description = "Physical and digital books", IsActive = true, CreatedAt = DateTime.UtcNow }
            );
        }
    }
}

