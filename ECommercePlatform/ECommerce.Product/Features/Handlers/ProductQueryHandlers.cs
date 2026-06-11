using ECommerce.Product.Application.DTOs;
using ECommerce.Product.Application.Features.Queries;
using ECommerce.Product.Application.Interfaces;
using ECommerce.Shared.Common.Exceptions;
using ECommerce.Shared.Common.Responses;
using MediatR;

namespace ECommerce.Product.Application.Features.Handlers;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException("Product", request.ProductId);

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}

/// <summary>
/// Handles paginated, filtered product listings.
/// The heavy lifting (filtering + sorting + pagination) is done in the repository
/// using IQueryable — everything runs as a single SQL query, not in memory.
/// </summary>
public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var pagedProducts = await _productRepository.GetPagedAsync(request.Filter, cancellationToken);

        // Map domain entities to DTOs
        var productDtos = pagedProducts.Items.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            ImageUrl = p.ImageUrl,
            IsActive = p.IsActive,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name ?? string.Empty,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToList();

        return PagedResult<ProductDto>.Create(
            productDtos,
            pagedProducts.TotalCount,
            pagedProducts.PageNumber,
            pagedProducts.PageSize);
    }
}

// ─── Category Handlers ────────────────────────────────────────────────────────

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, List<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);

        return categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            IsActive = c.IsActive,
            ProductCount = c.Products.Count(p => p.IsActive)
        }).ToList();
    }
}

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new NotFoundException("Category", request.CategoryId);

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            ProductCount = category.Products.Count(p => p.IsActive)
        };
    }
}

// ─── Category Command Handlers ────────────────────────────────────────────────

public class CreateCategoryCommandHandler : IRequestHandler<Commands.CreateCategoryCommand, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;

    public CreateCategoryCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryDto> Handle(Commands.CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var nameExists = await _categoryRepository.NameExistsAsync(request.Dto.Name, cancellationToken);
        if (nameExists)
            throw new BusinessRuleException($"Category '{request.Dto.Name}' already exists.");

        var category = new Domain.Entities.Category
        {
            Name = request.Dto.Name,
            Description = request.Dto.Description
        };

        await _categoryRepository.AddAsync(category, cancellationToken);

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            ProductCount = 0
        };
    }
}

public class UpdateCategoryCommandHandler : IRequestHandler<Commands.UpdateCategoryCommand, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;

    public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryDto> Handle(Commands.UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new NotFoundException("Category", request.CategoryId);

        if (request.Dto.Name is not null) category.Name = request.Dto.Name;
        if (request.Dto.Description is not null) category.Description = request.Dto.Description;
        if (request.Dto.IsActive.HasValue) category.IsActive = request.Dto.IsActive.Value;

        await _categoryRepository.UpdateAsync(category, cancellationToken);

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            ProductCount = category.Products.Count(p => p.IsActive)
        };
    }
}
