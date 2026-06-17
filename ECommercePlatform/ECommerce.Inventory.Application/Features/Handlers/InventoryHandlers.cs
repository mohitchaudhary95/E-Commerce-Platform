using ECommerce.Inventory.Application.DTOs;
using ECommerce.Inventory.Application.Features.Commands;
using ECommerce.Inventory.Application.Features.Queries;
using ECommerce.Inventory.Application.Interfaces;
using ECommerce.Inventory.Domain.Entities;
using ECommerce.Shared.Common.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Inventory.Application.Features.Handlers;

// ─── Set Stock (Admin) ────────────────────────────────────────────────────────

public class SetStockCommandHandler : IRequestHandler<SetStockCommand, InventoryDto>
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IInventoryEventPublisher _eventPublisher;

    public SetStockCommandHandler(IInventoryRepository inventoryRepository, IInventoryEventPublisher eventPublisher)
    {
        _inventoryRepository = inventoryRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<InventoryDto> Handle(SetStockCommand request, CancellationToken cancellationToken)
    {
        var existing = await _inventoryRepository.GetByProductIdAsync(request.Dto.ProductId, cancellationToken);

        if (existing != null)
        {
            // Update existing record
            existing.StockQuantity = request.Dto.Quantity;
            existing.LowStockThreshold = request.Dto.LowStockThreshold;
            existing.UpdatedAt = DateTime.UtcNow;
            await _inventoryRepository.UpdateAsync(existing, cancellationToken);
            await _eventPublisher.PublishInventoryUpdatedAsync(existing, "ManualSet", cancellationToken);
            return MapToDto(existing);
        }

        // Create new inventory record for this product
        var inventory = new Inventory
        {
            ProductId = request.Dto.ProductId,
            ProductName = request.Dto.ProductName,
            StockQuantity = request.Dto.Quantity,
            LowStockThreshold = request.Dto.LowStockThreshold
        };

        await _inventoryRepository.AddAsync(inventory, cancellationToken);
        return MapToDto(inventory);
    }
}

// ─── Adjust Stock (Admin manual adjustment) ───────────────────────────────────

public class AdjustStockCommandHandler : IRequestHandler<AdjustStockCommand, InventoryDto>
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IInventoryEventPublisher _eventPublisher;

    public AdjustStockCommandHandler(IInventoryRepository inventoryRepository, IInventoryEventPublisher eventPublisher)
    {
        _inventoryRepository = inventoryRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<InventoryDto> Handle(AdjustStockCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException("Inventory for product", request.ProductId);

        var newQuantity = inventory.StockQuantity + request.Dto.Quantity;

        if (newQuantity < 0)
            throw new BusinessRuleException(
                $"Cannot reduce stock below zero. Current: {inventory.StockQuantity}, Adjustment: {request.Dto.Quantity}");

        inventory.StockQuantity = newQuantity;
        inventory.UpdatedAt = DateTime.UtcNow;

        await _inventoryRepository.UpdateAsync(inventory, cancellationToken);
        await _eventPublisher.PublishInventoryUpdatedAsync(inventory, request.Dto.Reason, cancellationToken);

        return MapToDto(inventory);
    }
}

// ─── Deduct Stock For Order (triggered by OrderCreatedConsumer) ───────────────

/// <summary>
/// Decreases stock for all items in an order.
/// Called when OrderCreatedEvent is received from RabbitMQ.
///
/// Important: processes all items in one transaction-like sequence.
/// If any item has insufficient stock, we log a warning but still process
/// the rest — in production you'd implement saga/compensation patterns.
/// For a fresher portfolio project, this is more than sufficient.
/// </summary>
public class DeductStockForOrderCommandHandler : IRequestHandler<DeductStockForOrderCommand, bool>
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IInventoryEventPublisher _eventPublisher;
    private readonly ILogger<DeductStockForOrderCommandHandler> _logger;

    public DeductStockForOrderCommandHandler(
        IInventoryRepository inventoryRepository,
        IInventoryEventPublisher eventPublisher,
        ILogger<DeductStockForOrderCommandHandler> logger)
    {
        _inventoryRepository = inventoryRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<bool> Handle(DeductStockForOrderCommand request, CancellationToken cancellationToken)
    {
        foreach (var item in request.Items)
        {
            var inventory = await _inventoryRepository.GetByProductIdAsync(item.ProductId, cancellationToken);

            if (inventory == null)
            {
                _logger.LogWarning(
                    "No inventory record found for Product {ProductId} in Order {OrderId}. Skipping.",
                    item.ProductId, request.OrderId);
                continue;
            }

            if (inventory.AvailableQuantity < item.Quantity)
            {
                _logger.LogWarning(
                    "Insufficient stock for Product {ProductId}. Available: {Available}, Required: {Required}. Order: {OrderId}",
                    item.ProductId, inventory.AvailableQuantity, item.Quantity, request.OrderId);
                // In production: publish StockShortageEvent → trigger compensation
                continue;
            }

            var previousStock = inventory.StockQuantity;
            inventory.StockQuantity -= item.Quantity;
            inventory.UpdatedAt = DateTime.UtcNow;

            await _inventoryRepository.UpdateAsync(inventory, cancellationToken);

            _logger.LogInformation(
                "Stock deducted for Product {ProductId}: {Previous} → {New} (Order {OrderId})",
                item.ProductId, previousStock, inventory.StockQuantity, request.OrderId);

            await _eventPublisher.PublishInventoryUpdatedAsync(inventory, "OrderFulfilled", cancellationToken);
        }

        return true;
    }
}

// ─── Query Handlers ───────────────────────────────────────────────────────────

public class GetStockByProductIdQueryHandler : IRequestHandler<GetStockByProductIdQuery, InventoryDto>
{
    private readonly IInventoryRepository _inventoryRepository;

    public GetStockByProductIdQueryHandler(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<InventoryDto> Handle(GetStockByProductIdQuery request, CancellationToken cancellationToken)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException("Inventory for product", request.ProductId);

        return MapToDto(inventory);
    }
}

public class GetAllInventoryQueryHandler : IRequestHandler<GetAllInventoryQuery, List<InventoryDto>>
{
    private readonly IInventoryRepository _inventoryRepository;

    public GetAllInventoryQueryHandler(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<List<InventoryDto>> Handle(GetAllInventoryQuery request, CancellationToken cancellationToken)
    {
        var items = await _inventoryRepository.GetAllAsync(cancellationToken);
        return items.Select(MapToDto).ToList();
    }
}

public class GetLowStockQueryHandler : IRequestHandler<GetLowStockQuery, List<InventoryDto>>
{
    private readonly IInventoryRepository _inventoryRepository;

    public GetLowStockQueryHandler(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<List<InventoryDto>> Handle(GetLowStockQuery request, CancellationToken cancellationToken)
    {
        var items = await _inventoryRepository.GetLowStockAsync(cancellationToken);
        return items.Select(MapToDto).ToList();
    }
}

// ─── Shared mapper ────────────────────────────────────────────────────────────

file static class InventoryMapper
{
    public static InventoryDto MapToDto(Inventory i) => new()
    {
        Id = i.Id,
        ProductId = i.ProductId,
        ProductName = i.ProductName,
        StockQuantity = i.StockQuantity,
        ReservedQuantity = i.ReservedQuantity,
        AvailableQuantity = i.AvailableQuantity,
        LowStockThreshold = i.LowStockThreshold,
        IsLowStock = i.IsLowStock,
        UpdatedAt = i.UpdatedAt
    };
}
