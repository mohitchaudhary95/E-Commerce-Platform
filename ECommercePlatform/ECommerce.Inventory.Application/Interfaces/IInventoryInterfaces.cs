using ECommerce.Inventory.Domain.Entities;

namespace ECommerce.Inventory.Application.Interfaces;

public interface IInventoryRepository
{
    Task<Inventory?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<List<Inventory>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Inventory>> GetLowStockAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Inventory inventory, CancellationToken cancellationToken = default);
    Task UpdateAsync(Inventory inventory, CancellationToken cancellationToken = default);
}

public interface IInventoryEventPublisher
{
    Task PublishInventoryUpdatedAsync(Inventory inventory, string reason, CancellationToken cancellationToken = default);
}
