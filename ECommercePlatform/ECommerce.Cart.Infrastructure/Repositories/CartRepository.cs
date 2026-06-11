using ECommerce.Cart.Application.Interfaces;
using ECommerce.Cart.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Cart.Infrastructure.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly CartDbContext _context;

        public CartRepository(CartDbContext context)
        {
            _context = context;
        }

        public async Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => await _context.Carts
                .Include(c => c.Items)  // Always load items — cart is useless without them
                .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        public async Task<Cart?> GetByIdAsync(Guid cartId, CancellationToken cancellationToken = default)
            => await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);

        public async Task AddAsync(Cart cart, CancellationToken cancellationToken = default)
        {
            await _context.Carts.AddAsync(cart, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default)
        {
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Cart cart, CancellationToken cancellationToken = default)
        {
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
