using ECommerce.Cart.Application.DTOs;
using MediatR;

namespace ECommerce.Cart.Application.Features.Queries;

public record GetUserCartQuery(Guid UserId) : IRequest<CartDto>;
