using ECommerce.Order.Application.DTOs;
using ECommerce.Order.Application.Features.Commands;
using ECommerce.Order.Application.Interfaces;
using DomainOrder = ECommerce.Order.Domain.Entities.Order;
using DomainOrderItem = ECommerce.Order.Domain.Entities.OrderItem;
using ECommerce.Order.Domain.Entities;
using ECommerce.Shared.Common.Exceptions;
using MediatR;

namespace ECommerce.Order.Application.Features.Handlers;

/// <summary>
/// The most important handler in the entire system.
/// This is where the async event chain starts.
///
/// Steps:
///   1. Validate items are provided
///   2. Build the Order entity with a snapshot of item data
///   3. Save to OrderDb
///   4. Publish OrderCreatedEvent to RabbitMQ
///      → PaymentService picks it up → processes payment → publishes PaymentCompletedEvent
///      → InventoryService picks it up → decreases stock
///      → NotificationService picks it up → sends confirmation email
///
/// The handler doesn't wait for any of those to finish — fire and forget.
/// That's what makes this "asynchronous" — OrderService's job ends at step 4.
/// </summary>
public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderEventPublisher _eventPublisher;

    public PlaceOrderCommandHandler(IOrderRepository orderRepository, IOrderEventPublisher eventPublisher)
    {
        _orderRepository = orderRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<OrderDto> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        // ── Step 1: Validate ──────────────────────────────────────────────────
        if (!request.Dto.Items.Any())
            throw new BusinessRuleException("Cannot place an order with no items.");

        if (string.IsNullOrWhiteSpace(request.Dto.ShippingAddress))
            throw new BusinessRuleException("Shipping address is required.");

        // ── Step 2: Build Order entity ────────────────────────────────────────
        var order = new DomainOrder
        {
            UserId = request.UserId,
            UserEmail = request.Dto.UserEmail,
            ShippingAddress = request.Dto.ShippingAddress,
            Items = request.Dto.Items.Select(i => new DomainOrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity
            }).ToList()
        };

        // Lock in total at order time — price changes later won't affect this order
        order.TotalAmount = order.Items.Sum(i => i.UnitPrice * i.Quantity);

        // ── Step 3: Persist ───────────────────────────────────────────────────
        await _orderRepository.AddAsync(order, cancellationToken);

        // ── Step 4: Publish event — kicks off the entire async chain ──────────
        // This sends a message to RabbitMQ and returns immediately.
        // We don't wait for PaymentService, InventoryService, or NotificationService.
        await _eventPublisher.PublishOrderCreatedAsync(order, cancellationToken);

        return MapToDto(order);
    }

    private static OrderDto MapToDto(DomainOrder o) => new()
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


