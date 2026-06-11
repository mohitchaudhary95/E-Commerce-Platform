using ECommerce.Product.Application.DTOs;
using ECommerce.Product.Application.Features.Commands;
using ECommerce.Product.Application.Interfaces;
using ECommerce.Shared.Common.Exceptions;
using MediatR;

namespace ECommerce.Product.Application.Features.Handlers;

/// <summary>
/// Creates a new product.
/// Validates that the category exists before creating.
/// </summary>
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public CreateProductCommandHandler(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Validate category exists before creating the product
        var category = await _categoryRepository.GetByIdAsync(request.Dto.CategoryId, cancellationToken)
            ?? throw new NotFoundException("Category", request.Dto.CategoryId);

        var product = new Domain.Entities.Product
        {
            Name = request.Dto.Name,
            Description = request.Dto.Description,
            Price = request.Dto.Price,
            ImageUrl = request.Dto.ImageUrl,
            CategoryId = request.Dto.CategoryId
        };

        await _productRepository.AddAsync(product, cancellationToken);

        return MapToDto(product, category.Name);
    }

    private static ProductDto MapToDto(Domain.Entities.Product p, string categoryName) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        ImageUrl = p.ImageUrl,
        IsActive = p.IsActive,
        CategoryId = p.CategoryId,
        CategoryName = categoryName,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}

/// <summary>
/// Updates an existing product.
/// Only updates fields that are provided (not null) — partial update pattern.
/// This means the client doesn't need to send ALL fields, just the ones changing.
/// </summary>
public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public UpdateProductCommandHandler(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException("Product", request.ProductId);

        var dto = request.Dto;

        // Partial update — only apply non-null values
        if (dto.Name is not null) product.Name = dto.Name;
        if (dto.Description is not null) product.Description = dto.Description;
        if (dto.Price.HasValue) product.Price = dto.Price.Value;
        if (dto.ImageUrl is not null) product.ImageUrl = dto.ImageUrl;
        if (dto.IsActive.HasValue) product.IsActive = dto.IsActive.Value;

        // If category is being changed, validate the new category exists
        if (dto.CategoryId.HasValue)
        {
            var categoryExists = await _categoryRepository.GetByIdAsync(dto.CategoryId.Value, cancellationToken)
                ?? throw new NotFoundException("Category", dto.CategoryId.Value);
            product.CategoryId = dto.CategoryId.Value;
        }

        product.UpdatedAt = DateTime.UtcNow;
        await _productRepository.UpdateAsync(product, cancellationToken);

        // Reload with category name for the response
        var category = await _categoryRepository.GetByIdAsync(product.CategoryId, cancellationToken);

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive,
            CategoryId = product.CategoryId,
            CategoryName = category?.Name ?? string.Empty,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}

/// <summary>
/// Soft-deletes a product by marking IsActive = false.
/// We never hard-delete products — orders may reference them historically.
/// </summary>
public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IProductRepository _productRepository;

    public DeleteProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException("Product", request.ProductId);

        // Soft delete — set IsActive = false, don't remove from DB
        // This preserves historical order data that references this product
        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product, cancellationToken);
        return true;
    }
}
