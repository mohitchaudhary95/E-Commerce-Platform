using DomainProduct = global::ECommerce.Product.Domain.Entities.Product;
using DomainCategory = global::ECommerce.Product.Domain.Entities.Category;
using ECommerce.Product.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Product.Infrastructure.Persistence
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

        public DbSet<Domain.Entities.Product> Products => Set<Domain.Entities.Product>();
        public DbSet<DomainCategory> Categories => Set<DomainCategory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}

