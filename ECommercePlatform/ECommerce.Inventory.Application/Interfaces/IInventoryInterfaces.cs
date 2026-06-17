using DomainInventory = ECommerce.Inventory.Domain.Entities.Inventory;

namespace ECommerce.Inventory.Application.Interfaces;

public interface IInventoryRepository
{
    Task<DomainInventory?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<List<DomainInventory>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<DomainInventory>> GetLowStockAsync(CancellationToken cancellationToken = default);
    Task AddAsync(DomainInventory inventory, CancellationToken cancellationToken = default);
    Task UpdateAsync(DomainInventory inventory, CancellationToken cancellationToken = default);
}

public interface IInventoryEventPublisher
{
    Task PublishInventoryUpdatedAsync(DomainInventory inventory, string reason, CancellationToken cancellationToken = default);
}
