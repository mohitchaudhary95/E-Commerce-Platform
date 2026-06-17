using ECommerce.Inventory.Application.DTOs;
using MediatR;

namespace ECommerce.Inventory.Application.Features.Queries;

public record GetStockByProductIdQuery(Guid ProductId) : IRequest<InventoryDto>;

public record GetAllInventoryQuery : IRequest<List<InventoryDto>>;

public record GetLowStockQuery : IRequest<List<InventoryDto>>;
