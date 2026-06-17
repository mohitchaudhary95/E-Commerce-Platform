using ECommerce.Notification.Application.DTOs;
using ECommerce.Notification.Application.Features.Commands;
using ECommerce.Notification.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Notification.Application.Features.Handlers;

/// <summary>
/// Handles order confirmation email.
/// Triggered when OrderCreatedEvent is consumed.
/// Builds an HTML email with order summary and sends it.
/// </summary>
public class SendOrderConfirmationEmailCommandHandler
    : IRequestHandler<SendOrderConfirmationEmailCommand, bool>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<SendOrderConfirmationEmailCommandHandler> _logger;

    public SendOrderConfirmationEmailCommandHandler(
        IEmailService emailService,
        ILogger<SendOrderConfirmationEmailCommandHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<bool> Handle(
        SendOrderConfirmationEmailCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        _logger.LogInformation("Sending order confirmation email to {Email} for Order {OrderId}",
            dto.UserEmail, dto.OrderId);

        await _emailService.SendOrderConfirmationAsync(dto, cancellationToken);
        return true;
    }
}

/// <summary>
/// Handles payment success email.
/// Triggered when PaymentCompletedEvent (IsSuccess=true) is consumed.
/// </summary>
public class SendPaymentSuccessEmailCommandHandler
    : IRequestHandler<SendPaymentSuccessEmailCommand, bool>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<SendPaymentSuccessEmailCommandHandler> _logger;

    public SendPaymentSuccessEmailCommandHandler(
        IEmailService emailService,
        ILogger<SendPaymentSuccessEmailCommandHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<bool> Handle(
        SendPaymentSuccessEmailCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending payment success email to {Email} for Order {OrderId}",
            request.Dto.UserEmail, request.Dto.OrderId);

        await _emailService.SendPaymentSuccessAsync(request.Dto, cancellationToken);
        return true;
    }
}

/// <summary>
/// Handles payment failure email.
/// Triggered when PaymentCompletedEvent (IsSuccess=false) is consumed.
/// </summary>
public class SendPaymentFailureEmailCommandHandler
    : IRequestHandler<SendPaymentFailureEmailCommand, bool>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<SendPaymentFailureEmailCommandHandler> _logger;

    public SendPaymentFailureEmailCommandHandler(
        IEmailService emailService,
        ILogger<SendPaymentFailureEmailCommandHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<bool> Handle(
        SendPaymentFailureEmailCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending payment failure email to {Email} for Order {OrderId}",
            request.Dto.UserEmail, request.Dto.OrderId);

        await _emailService.SendPaymentFailureAsync(request.Dto, cancellationToken);
        return true;
    }
}
