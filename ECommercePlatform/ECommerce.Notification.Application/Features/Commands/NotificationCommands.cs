using ECommerce.Notification.Application.DTOs;
using MediatR;

namespace ECommerce.Notification.Application.Features.Commands;

/// <summary>
/// Commands are dispatched by consumers via MediatR.
/// This keeps consumer code thin — it just builds the command and hands off.
/// All email composition logic lives in handlers.
/// </summary>
public record SendOrderConfirmationEmailCommand(OrderConfirmationEmailDto Dto) : IRequest<bool>;

public record SendPaymentSuccessEmailCommand(PaymentResultEmailDto Dto) : IRequest<bool>;

public record SendPaymentFailureEmailCommand(PaymentResultEmailDto Dto) : IRequest<bool>;
