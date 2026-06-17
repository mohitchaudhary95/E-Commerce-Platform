using ECommerce.Order.Application.DTOs;
using ECommerce.Order.Application.Features.Commands;
using ECommerce.Order.Application.Features.Queries;
using ECommerce.Order.Application.Interfaces;
using ECommerce.Order.Domain.Enums;
using ECommerce.Shared.Common.Exceptions;
using ECommerce.Shared.Common.Responses;
using MediatR;

namespace ECommerce.Order.Application.Features.Handlers;

// ─── Cancel Order ─────────────────────────────────────────────────────────────

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;

    public CancelOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException("Order", request.OrderId);

        // Security: users can only cancel their OWN orders
        if (order.UserId != request.UserId)
            throw new ForbiddenException("You can only cancel your own orders.");

        // Business rule: can only cancel Pending orders
        if (order.Status != OrderStatus.Pending)
            throw new BusinessRuleException(
                $"Cannot cancel an order with status '{order.Status}'. Only Pending orders can be cancelled.");

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order, cancellationToken);

        return MapToDto(order);
    }
}

// ─── Update Order Status (triggered by PaymentCompletedConsumer) ──────────────

/// <summary>
/// Called internally when PaymentService publishes a PaymentCompletedEvent.
/// NOT exposed as an API endpoint — only RabbitMQ consumer calls this.
///
/// Payment succeeded → Order moves to Processing
/// Payment failed    → Order moves to Cancelled
/// </summary>
public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, bool>
{
    private readonly IOrderRepository _orderRepository;

    public UpdateOrderStatusCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<bool> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order == null)
        {
            // Order not found — could happen if event is processed twice (idempotency)
            // Log warning but don't throw — consumer will ack the message
            return false;
        }

        // Only update if still Pending — guard against duplicate events
        if (order.Status != OrderStatus.Pending)
            return true;

        order.Status = request.PaymentSucceeded ? OrderStatus.Processing : OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order, cancellationToken);
        return true;
    }
}

// ─── Query Handlers ───────────────────────────────────────────────────────────

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException("Order", request.OrderId);

        // Security: users can only view their OWN orders (admins bypass this in controller)
        if (order.UserId != request.UserId)
            throw new ForbiddenException("You can only view your own orders.");

        return MapToDto(order);
    }
}

public class GetOrderHistoryQueryHandler : IRequestHandler<GetOrderHistoryQuery, PagedResult<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderHistoryQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetOrderHistoryQuery request, CancellationToken cancellationToken)
    {
        var pagedOrders = await _orderRepository.GetByUserIdAsync(
            request.UserId, request.PageNumber, request.PageSize, cancellationToken);

        var orderDtos = pagedOrders.Items.Select(MapToDto).ToList();

        return PagedResult<OrderDto>.Create(orderDtos, pagedOrders.TotalCount, request.PageNumber, request.PageSize);
    }
}

// ─── Shared mapper ────────────────────────────────────────────────────────────

file static class OrderMapper
{
    public static OrderDto MapToDto(Order.Domain.Entities.Order o) => new()
    {
        Id = o.Id,
        UserId = o.UserId,
        UserEmail = o.UserEmail,
        ShippingAddress = o.ShippingAddress,
        TotalAmount = o.TotalAmount,
        Status = o.Status,
        CreatedAt = o.CreatedAt,
        UpdatedAt = o.UpdatedAt,
        Items = o.Items.Select(i => new OrderItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            UnitPrice = i.UnitPrice,
            Quantity = i.Quantity,
            TotalPrice = i.TotalPrice
        }).ToList()
    };
}
