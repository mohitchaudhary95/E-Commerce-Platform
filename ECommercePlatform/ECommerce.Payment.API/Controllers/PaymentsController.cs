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
    /// Returns success:false (not 404) when payment not yet created — this lets the
    /// frontend keep polling without treating "not found yet" as a hard error.
    /// </summary>
    [HttpGet("order/{orderId:guid}")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> GetByOrderId(
        Guid orderId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPaymentByOrderIdQuery(orderId), cancellationToken);

        if (result == null)
            return Ok(ApiResponse<PaymentDto>.Fail("Payment not yet processed."));

        return Ok(ApiResponse<PaymentDto>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> GetById(
        Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPaymentByIdQuery(id), cancellationToken);

        if (result == null)
            return Ok(ApiResponse<PaymentDto>.Fail("Payment not found."));

        return Ok(ApiResponse<PaymentDto>.Ok(result));
    }
}
