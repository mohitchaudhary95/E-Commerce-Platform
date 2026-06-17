using ECommerce.Inventory.Application.Interfaces;
using ECommerce.Inventory.Domain.Entities;
using ECommerce.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Inventory.Infrastructure.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly InventoryDbContext _context;

    public InventoryRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<Inventory?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
        => await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == productId, cancellationToken);

    public async Task<List<Inventory>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Inventories
            .OrderBy(i => i.ProductName)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Returns products whose available stock is at or below the threshold.
    /// Useful for admin dashboard "low stock alerts".
    /// </summary>
    public async Task<List<Inventory>> GetLowStockAsync(CancellationToken cancellationToken = default)
        => await _context.Inventories
            .Where(i => (i.StockQuantity - i.ReservedQuantity) <= i.LowStockThreshold)
            .OrderBy(i => i.StockQuantity)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Inventory inventory, CancellationToken cancellationToken = default)
    {
        await _context.Inventories.AddAsync(inventory, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Inventory inventory, CancellationToken cancellationToken = default)
    {
        _context.Inventories.Update(inventory);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
