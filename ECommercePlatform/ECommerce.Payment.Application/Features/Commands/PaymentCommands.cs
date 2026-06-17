using ECommerce.Payment.Application.DTOs;
using MediatR;

namespace ECommerce.Payment.Application.Features.Commands;

/// <summary>
/// Triggered internally by OrderCreatedConsumer — NOT a direct API call.
/// The consumer receives the event, builds this command, and sends it via MediatR.
/// </summary>
public record ProcessPaymentCommand(ProcessPaymentDto Dto) : IRequest<PaymentDto>;
