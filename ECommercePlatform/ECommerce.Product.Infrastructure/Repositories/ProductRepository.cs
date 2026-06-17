using ECommerce.Product.Application.DTOs;
using ECommerce.Product.Application.Interfaces;
using ECommerce.Product.Infrastructure.Persistence;
using ECommerce.Shared.Common.Responses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Product.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductDbContext _context;

        public ProductRepository(ProductDbContext context)
        {
            _context = context;
        }

        public async Task<Domain.Entities.Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _context.Products
                .Include(p => p.Category)   // JOIN with Categories table
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        /// <summary>
        /// The most important method — handles the product listing page.
        /// All filtering, sorting, and pagination happens in SQL.
        /// </summary>
        public async Task<PagedResult<Domain.Entities.Product>> GetPagedAsync(
            ProductFilterDto filter,
            CancellationToken cancellationToken = default)
        {
            // Start with a base query — no SQL executed yet
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .AsQueryable();

            // -- Filtering ----------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                // SQL: WHERE LOWER(Name) LIKE '%term%' OR LOWER(Description) LIKE '%term%'
                query = query.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    p.Description.ToLower().Contains(term));
            }

            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);

            // -- Count BEFORE pagination (needed for TotalPages calculation) ---------
            // This runs: SELECT COUNT(*) FROM Products WHERE ...
            var totalCount = await query.CountAsync(cancellationToken);

            // -- Sorting ------------------------------------------------------------
            query = filter.SortBy?.ToLower() switch
            {
                "price" => filter.SortDescending
                    ? query.OrderByDescending(p => p.Price)
                    : query.OrderBy(p => p.Price),
                "name" => filter.SortDescending
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name),
                _ => query.OrderByDescending(p => p.CreatedAt)  // Default: newest first
            };

            // -- Pagination ---------------------------------------------------------
            // SQL: OFFSET (page-1)*size ROWS FETCH NEXT size ROWS ONLY
            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);   // ? SQL executes HERE

            return PagedResult<Domain.Entities.Product>.Create(items, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<List<Domain.Entities.Product>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
            => await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .ToListAsync(cancellationToken);

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
            => await _context.Products.AnyAsync(p => p.Id == id, cancellationToken);

        public async Task AddAsync(Domain.Entities.Product product, CancellationToken cancellationToken = default)
        {
            await _context.Products.AddAsync(product, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Domain.Entities.Product product, CancellationToken cancellationToken = default)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Domain.Entities.Product product, CancellationToken cancellationToken = default)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

}
