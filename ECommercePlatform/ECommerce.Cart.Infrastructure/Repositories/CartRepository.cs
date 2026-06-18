using ECommerce.Cart.Application.Interfaces;
using ECommerce.Cart.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

using DomainCart = ECommerce.Cart.Domain.Entities.Cart;

namespace ECommerce.Cart.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly CartDbContext _context;

    public CartRepository(CartDbContext context)
    {
        _context = context;
    }

    public async Task<DomainCart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

    public async Task<DomainCart?> GetByIdAsync(Guid cartId, CancellationToken cancellationToken = default)
        => await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);

    public async Task AddAsync(DomainCart cart, CancellationToken cancellationToken = default)
    {
        await _context.Carts.AddAsync(cart, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(DomainCart cart, CancellationToken cancellationToken = default)
    {
        _context.Carts.Update(cart);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(DomainCart cart, CancellationToken cancellationToken = default)
    {
        _context.Carts.Remove(cart);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
