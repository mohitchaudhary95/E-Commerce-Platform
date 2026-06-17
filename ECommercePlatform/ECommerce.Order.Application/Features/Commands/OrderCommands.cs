using ECommerce.Order.Application.DTOs;
using ECommerce.Shared.Common.Responses;
using MediatR;

namespace ECommerce.Order.Application.Features.Commands;

public record PlaceOrderCommand(Guid UserId, PlaceOrderDto Dto) : IRequest<OrderDto>;

public record CancelOrderCommand(Guid OrderId, Guid UserId) : IRequest<OrderDto>;

// Internal command — triggered by PaymentCompletedConsumer, not the API
public record UpdateOrderStatusCommand(Guid OrderId, bool PaymentSucceeded) : IRequest<bool>;
