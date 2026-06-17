using ECommerce.Inventory.Application.DTOs;
using MediatR;

namespace ECommerce.Inventory.Application.Features.Commands;

// Admin: set initial stock for a product
public record SetStockCommand(SetStockDto Dto) : IRequest<InventoryDto>;

// Admin: manually adjust stock up or down
public record AdjustStockCommand(Guid ProductId, AdjustStockDto Dto) : IRequest<InventoryDto>;

// Internal: triggered by OrderCreatedConsumer — decrease stock for each order item
public record DeductStockForOrderCommand(Guid OrderId, List<OrderLineItem> Items) : IRequest<bool>;

public record OrderLineItem(Guid ProductId, string ProductName, int Quantity);
