using DomainOrder = ECommerce.Order.Domain.Entities.Order;
using ECommerce.Order.Domain.Entities;
using ECommerce.Order.Domain.Enums;
using ECommerce.Shared.Common.Responses;

namespace ECommerce.Order.Application.Interfaces;

public interface IOrderRepository
{
    Task<DomainOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<DomainOrder>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(DomainOrder order, CancellationToken cancellationToken = default);
    Task UpdateAsync(DomainOrder order, CancellationToken cancellationToken = default);
}

/// <summary>
/// Publishes events to RabbitMQ queues.
/// Defined here in Application so handlers can depend on it without
/// knowing anything about RabbitMQ — that's Infrastructure's job.
/// </summary>
public interface IOrderEventPublisher
{
    Task PublishOrderCreatedAsync(DomainOrder order, CancellationToken cancellationToken = default);
}

