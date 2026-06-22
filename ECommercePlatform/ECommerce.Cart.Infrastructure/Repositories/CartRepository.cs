using ECommerce.Cart.Application.Interfaces;
using ECommerce.Cart.Domain.Entities;
using ECommerce.Cart.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

// The compiler sees 'Cart' as a namespace (ECommerce.Cart.Domain.Entities.Cart)
// and also as a class inside that namespace — they clash.
// This alias tells the compiler exactly which one we mean when we say 'CartEntity'.
using CartEntity = ECommerce.Cart.Domain.Entities.Cart;

namespace ECommerce.Cart.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly CartDbContext _context;

    public CartRepository(CartDbContext context)
    {
        _context = context;
    }

    public async Task<CartEntity?> GetByUserIdAsync(
        Guid userId, CancellationToken cancellationToken = default)
        => await _context.Carts
            .Include(c => c.Items)
            .AsTracking()   // Explicit — EF tracks every change to loaded entities
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

    public async Task<CartEntity?> GetByIdAsync(
        Guid cartId, CancellationToken cancellationToken = default)
        => await _context.Carts
            .Include(c => c.Items)
            .AsTracking()
            .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);

    public async Task AddAsync(
        CartEntity cart, CancellationToken cancellationToken = default)
    {
        await _context.Carts.AddAsync(cart, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Saves changes to an already-tracked cart.
    ///
    /// WHY we do NOT call _context.Carts.Update(cart) here:
    /// Update() resets the entire entity graph state to Modified — this tells EF
    /// to try to UPDATE every CartItem row, even ones that were just Added.
    /// That causes a duplicate-key violation on the unique (CartId + ProductId) index.
    ///
    /// Instead: because we loaded the cart with AsTracking(), EF already knows
    /// the exact state of every item:
    ///   - Items added via cart.Items.Add()  → EF marks them as Added  → INSERT
    ///   - Items modified (qty change)        → EF marks them as Modified → UPDATE
    ///   - Items removed via cart.Items.Remove() → EF marks them as Deleted → DELETE
    /// SaveChangesAsync() generates exactly the right SQL for each — no collisions.
    /// </summary>
    public async Task UpdateAsync(
        CartEntity cart, CancellationToken cancellationToken = default)
    {
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        CartEntity cart, CancellationToken cancellationToken = default)
    {
        _context.Carts.Remove(cart);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
