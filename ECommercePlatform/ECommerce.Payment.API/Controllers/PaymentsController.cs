using ECommerce.Payment.Application.Features.Queries;
using ECommerce.Payment.Application.DTOs;
using ECommerce.Shared.Common.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Payment.API.Controllers;

/// <summary>
/// Read-only endpoints — payment processing is triggered by RabbitMQ events,
/// not direct API calls. Controllers only expose status lookup.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get payment status for a specific order.
    /// Frontend polls this after placing an order to show payment result.
    /// </summary>
    [HttpGet("order/{orderId:guid}")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> GetByOrderId(
        Guid orderId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPaymentByOrderIdQuery(orderId), cancellationToken);
        return Ok(ApiResponse<PaymentDto>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> GetById(
        Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPaymentByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<PaymentDto>.Ok(result));
    }
}
