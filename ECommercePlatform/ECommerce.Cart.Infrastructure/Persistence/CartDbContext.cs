using DomainCart = ECommerce.Cart.Domain.Entities.Cart;
using ECommerce.Cart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Cart.Infrastructure.Persistence
{
    public class CartDbContext : DbContext
    {
        public CartDbContext(DbContextOptions<CartDbContext> options) : base(options) { }

        public DbSet<DomainCart> Carts => Set<DomainCart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CartDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}

