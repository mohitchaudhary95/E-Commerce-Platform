using DomainPayment = ECommerce.Payment.Domain.Entities.Payment;
using ECommerce.Payment.Domain.Entities;

namespace ECommerce.Payment.Application.Interfaces;

public interface IPaymentRepository
{
    Task<DomainPayment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DomainPayment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task AddAsync(DomainPayment payment, CancellationToken cancellationToken = default);
    Task UpdateAsync(DomainPayment payment, CancellationToken cancellationToken = default);
}

/// <summary>
/// Publishes PaymentCompletedEvent after processing.
/// Consumed by OrderService (update status) and NotificationService (send email).
/// </summary>
public interface IPaymentEventPublisher
{
    Task PublishPaymentCompletedAsync(DomainPayment payment, CancellationToken cancellationToken = default);
}

