using ECommerce.Product.Application.DTOs;
using DomainProduct = global::ECommerce.Product.Domain.Entities.Product;
using global::ECommerce.Product.Domain.Entities;
using ECommerce.Shared.Common.Responses;

namespace ECommerce.Product.Application.Interfaces;

/// <summary>
/// Product data access contract.
/// Note GetPagedAsync — this is the key method for the product listing page.
/// It applies filters, sorting, and pagination entirely in SQL (not in memory).
/// </summary>
public interface IProductRepository
{
    Task<DomainProduct?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<DomainProduct>> GetPagedAsync(ProductFilterDto filter, CancellationToken cancellationToken = default);
    Task<List<DomainProduct>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(DomainProduct product, CancellationToken cancellationToken = default);
    Task UpdateAsync(DomainProduct product, CancellationToken cancellationToken = default);
    Task DeleteAsync(DomainProduct product, CancellationToken cancellationToken = default);
}

/// <summary>
/// Category data access contract.
/// </summary>
public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(Category category, CancellationToken cancellationToken = default);
    Task UpdateAsync(Category category, CancellationToken cancellationToken = default);
}

