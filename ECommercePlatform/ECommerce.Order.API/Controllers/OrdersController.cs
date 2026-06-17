using ECommerce.Order.Application.DTOs;
using ECommerce.Order.Application.Features.Commands;
using ECommerce.Order.Application.Features.Queries;
using ECommerce.Shared.Common.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.Order.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Place a new order. Publishes OrderCreatedEvent to RabbitMQ on success.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDto>>> PlaceOrder(
        [FromBody] PlaceOrderDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new PlaceOrderCommand(userId, dto), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<OrderDto>.Created(result, "Order placed successfully."));
    }

    /// <summary>
    /// Get a specific order. Users can only see their own orders.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new GetOrderByIdQuery(id, userId), cancellationToken);
        return Ok(ApiResponse<OrderDto>.Ok(result));
    }

    /// <summary>
    /// Get current user's order history with pagination.
    /// GET /api/orders/history?pageNumber=1&pageSize=10
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<PagedResult<OrderDto>>>> GetHistory(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new GetOrderHistoryQuery(userId, pageNumber, pageSize), cancellationToken);
        return Ok(ApiResponse<PagedResult<OrderDto>>.Ok(result));
    }

    /// <summary>
    /// Cancel a pending order. Only the order owner can cancel.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new CancelOrderCommand(id, userId), cancellationToken);
        return Ok(ApiResponse<OrderDto>.Ok(result, "Order cancelled successfully."));
    }

    private Guid GetUserId()
        => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
