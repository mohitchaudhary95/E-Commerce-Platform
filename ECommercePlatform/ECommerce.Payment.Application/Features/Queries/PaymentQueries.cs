using ECommerce.Payment.Application.DTOs;
using MediatR;

namespace ECommerce.Payment.Application.Features.Queries;

public record GetPaymentByOrderIdQuery(Guid OrderId) : IRequest<PaymentDto?>;

public record GetPaymentByIdQuery(Guid PaymentId) : IRequest<PaymentDto?>;
