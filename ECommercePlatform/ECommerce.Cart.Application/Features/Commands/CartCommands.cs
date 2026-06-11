using ECommerce.Cart.Application.DTOs;
using MediatR;

namespace ECommerce.Cart.Application.Features.Commands;

public record AddToCartCommand(Guid UserId, AddToCartDto Dto) : IRequest<CartDto>;

public record RemoveCartItemCommand(Guid UserId, Guid CartItemId) : IRequest<CartDto>;

public record UpdateQuantityCommand(Guid UserId, Guid CartItemId, UpdateQuantityDto Dto) : IRequest<CartDto>;

public record ClearCartCommand(Guid UserId) : IRequest<bool>;
