using ECommerce.Product.Application.DTOs;
using ECommerce.Shared.Common.Responses;
using MediatR;

namespace ECommerce.Product.Application.Features.Queries;

// ─── Product Queries ──────────────────────────────────────────────────────────

public record GetProductByIdQuery(Guid ProductId) : IRequest<ProductDto>;

/// <summary>
/// Carries the full filter DTO — handler passes it straight to the repository.
/// Returns a PagedResult so the frontend knows total pages, current page, etc.
/// </summary>
public record GetProductsQuery(ProductFilterDto Filter) : IRequest<PagedResult<ProductDto>>;

// ─── Category Queries ─────────────────────────────────────────────────────────

public record GetAllCategoriesQuery : IRequest<List<CategoryDto>>;

public record GetCategoryByIdQuery(Guid CategoryId) : IRequest<CategoryDto>;
