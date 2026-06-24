using ECommerce.Cart.Application.Interfaces;
using ECommerce.Cart.Domain.Entities;
using ECommerce.Cart.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
 
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
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
 
    public async Task<CartEntity?> GetByIdAsync(
        Guid cartId, CancellationToken cancellationToken = default)
        => await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);
 
    public async Task AddAsync(
        CartEntity cart, CancellationToken cancellationToken = default)
    {
        await _context.Carts.AddAsync(cart, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
 
    public async Task UpdateAsync(
        CartEntity cart, CancellationToken cancellationToken = default)
    {
        // Only update the Cart scalar - UpdatedAt
        await _context.Carts
            .Where(c => c.Id == cart.Id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.UpdatedAt, DateTime.UtcNow),
            cancellationToken);
    }
 
    /// <summary>
    /// Explicitly inserts a single new CartItem row.
    /// Called directly from the handler instead of cart.Items.Add()
    /// which confuses EF's change tracker when the entity has a pre-set Guid.
    /// </summary>
    public async Task AddCartItemAsync(
        CartItem item, CancellationToken cancellationToken = default)
    {
        await _context.CartItems.AddAsync(item, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
 
    /// <summary>
    /// Explicitly updates quantity on an existing CartItem row.
    /// Uses ExecuteUpdateAsync — bypasses change tracker entirely.
    /// </summary>
    public async Task UpdateCartItemQuantityAsync(
        Guid itemId, int newQuantity, CancellationToken cancellationToken = default)
    {
        await _context.CartItems
            .Where(i => i.Id == itemId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(i => i.Quantity, newQuantity),
            cancellationToken);
    }
 
    /// <summary>
    /// Explicitly deletes a CartItem row by ID.
    /// </summary>
    public async Task RemoveCartItemAsync(
        Guid itemId, CancellationToken cancellationToken = default)
    {
        await _context.CartItems
            .Where(i => i.Id == itemId)
            .ExecuteDeleteAsync(cancellationToken);
    }
 
    /// <summary>
    /// Clears all items from a cart.
    /// </summary>
    public async Task ClearCartItemsAsync(
        Guid cartId, CancellationToken cancellationToken = default)
    {
        await _context.CartItems
            .Where(i => i.CartId == cartId)
            .ExecuteDeleteAsync(cancellationToken);
    }
 
    public async Task DeleteAsync(
        CartEntity cart, CancellationToken cancellationToken = default)
    {
        _context.Carts.Remove(cart);
        await _context.SaveChangesAsync(cancellationToken);
    }
}