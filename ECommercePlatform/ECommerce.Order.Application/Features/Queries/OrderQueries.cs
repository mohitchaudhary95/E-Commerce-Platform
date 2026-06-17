using ECommerce.Order.Application.DTOs;
using ECommerce.Shared.Common.Responses;
using MediatR;

namespace ECommerce.Order.Application.Features.Queries;

public record GetOrderByIdQuery(Guid OrderId, Guid UserId) : IRequest<OrderDto>;

public record GetOrderHistoryQuery(Guid UserId, int PageNumber = 1, int PageSize = 10)
    : IRequest<PagedResult<OrderDto>>;
