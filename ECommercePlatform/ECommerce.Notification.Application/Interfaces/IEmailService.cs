using ECommerce.Notification.Application.DTOs;

namespace ECommerce.Notification.Application.Interfaces;

/// <summary>
/// Email sending contract.
/// In dev: logs the email to console (no real SMTP needed).
/// In production: swap implementation for SendGrid, Mailtrap, or AWS SES.
/// The consumers never know which — they only depend on this interface.
/// </summary>
public interface IEmailService
{
    Task SendAsync(EmailDto email, CancellationToken cancellationToken = default);
    Task SendOrderConfirmationAsync(OrderConfirmationEmailDto dto, CancellationToken cancellationToken = default);
    Task SendPaymentSuccessAsync(PaymentResultEmailDto dto, CancellationToken cancellationToken = default);
    Task SendPaymentFailureAsync(PaymentResultEmailDto dto, CancellationToken cancellationToken = default);
}
