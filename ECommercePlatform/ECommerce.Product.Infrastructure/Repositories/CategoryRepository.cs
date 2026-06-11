using ECommerce.Product.Application.Interfaces;
using ECommerce.Product.Domain.Entities;
using ECommerce.Product.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Product.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ProductDbContext _context;

        public CategoryRepository(ProductDbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _context.Categories
                .Include(c => c.Products) // Include products to calculate ProductCount
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        public async Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _context.Categories
                .Include(c => c.Products)
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);

        public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
            => await _context.Categories.AnyAsync(c => c.Name == name, cancellationToken);

        public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
        {
            await _context.Categories.AddAsync(category, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
