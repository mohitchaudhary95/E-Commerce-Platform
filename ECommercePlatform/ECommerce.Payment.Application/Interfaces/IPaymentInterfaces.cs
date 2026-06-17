using ECommerce.Payment.Domain.Entities;

namespace ECommerce.Payment.Application.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default);
}

/// <summary>
/// Publishes PaymentCompletedEvent after processing.
/// Consumed by OrderService (update status) and NotificationService (send email).
/// </summary>
public interface IPaymentEventPublisher
{
    Task PublishPaymentCompletedAsync(Payment payment, CancellationToken cancellationToken = default);
}
