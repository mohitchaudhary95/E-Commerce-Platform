using ECommerce.Cart.Application.DTOs;
using ECommerce.Cart.Domain.Entities;
 
using CartEntity = ECommerce.Cart.Domain.Entities.Cart;
 
namespace ECommerce.Cart.Application.Interfaces;
 
public interface ICartRepository
{
    Task<CartEntity?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CartEntity?> GetByIdAsync(Guid cartId, CancellationToken cancellationToken = default);
    Task AddAsync(CartEntity cart, CancellationToken cancellationToken = default);
    Task UpdateAsync(CartEntity cart, CancellationToken cancellationToken = default);
    Task DeleteAsync(CartEntity cart, CancellationToken cancellationToken = default);
 
    // Explicit item operations — no change tracker involved
    Task AddCartItemAsync(CartItem item, CancellationToken cancellationToken = default);
    Task UpdateCartItemQuantityAsync(Guid itemId, int newQuantity, CancellationToken cancellationToken = default);
    Task RemoveCartItemAsync(Guid itemId, CancellationToken cancellationToken = default);
    Task ClearCartItemsAsync(Guid cartId, CancellationToken cancellationToken = default);
}
 
public interface IProductServiceClient
{
    Task<ProductResponseDto?> GetProductByIdAsync(
        Guid productId, CancellationToken cancellationToken = default);
}