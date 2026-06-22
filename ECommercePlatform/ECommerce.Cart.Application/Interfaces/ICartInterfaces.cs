using ECommerce.Cart.Application.DTOs;
using ECommerce.Cart.Domain.Entities;

// Same alias as CartRepository — keeps the type signatures consistent
using CartEntity = ECommerce.Cart.Domain.Entities.Cart;

namespace ECommerce.Cart.Application.Interfaces;

/// <summary>
/// Cart data access contract.
/// Uses CartEntity alias because 'Cart' is both a namespace and a class —
/// the alias removes the ambiguity across the entire project.
/// </summary>
public interface ICartRepository
{
    Task<CartEntity?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CartEntity?> GetByIdAsync(Guid cartId, CancellationToken cancellationToken = default);
    Task AddAsync(CartEntity cart, CancellationToken cancellationToken = default);
    Task UpdateAsync(CartEntity cart, CancellationToken cancellationToken = default);
    Task DeleteAsync(CartEntity cart, CancellationToken cancellationToken = default);
}

/// <summary>
/// HTTP client contract for calling ProductService.
/// Defined here so handlers can depend on it without knowing about HttpClient.
/// </summary>
public interface IProductServiceClient
{
    Task<ProductResponseDto?> GetProductByIdAsync(
        Guid productId, CancellationToken cancellationToken = default);
}
