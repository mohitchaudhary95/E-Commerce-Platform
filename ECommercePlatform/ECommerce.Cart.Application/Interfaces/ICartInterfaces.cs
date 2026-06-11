using ECommerce.Cart.Application.DTOs;
using ECommerce.Cart.Domain.Entities;

namespace ECommerce.Cart.Application.Interfaces;

/// <summary>
/// Cart data access contract.
/// GetByUserIdAsync is the most-called method — fetches cart with all items.
/// </summary>
public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Cart?> GetByIdAsync(Guid cartId, CancellationToken cancellationToken = default);
    Task AddAsync(Cart cart, CancellationToken cancellationToken = default);
    Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default);
    Task DeleteAsync(Cart cart, CancellationToken cancellationToken = default);
}

/// <summary>
/// HTTP client contract for calling ProductService.
///
/// Why an interface instead of calling HttpClient directly in handlers?
/// 1. Testability — we can mock this in unit tests (no real HTTP calls)
/// 2. Single responsibility — HTTP logic stays in Infrastructure
/// 3. If ProductService URL changes, only one place needs updating
/// </summary>
public interface IProductServiceClient
{
    Task<ProductResponseDto?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default);
}
