using ECommerce.Product.Application.DTOs;
using MediatR;

namespace ECommerce.Product.Application.Features.Commands;

// ─── Product Commands ─────────────────────────────────────────────────────────

public record CreateProductCommand(CreateProductDto Dto) : IRequest<ProductDto>;

public record UpdateProductCommand(Guid ProductId, UpdateProductDto Dto) : IRequest<ProductDto>;

public record DeleteProductCommand(Guid ProductId) : IRequest<bool>;

// ─── Category Commands ────────────────────────────────────────────────────────

public record CreateCategoryCommand(CreateCategoryDto Dto) : IRequest<CategoryDto>;

public record UpdateCategoryCommand(Guid CategoryId, UpdateCategoryDto Dto) : IRequest<CategoryDto>;
