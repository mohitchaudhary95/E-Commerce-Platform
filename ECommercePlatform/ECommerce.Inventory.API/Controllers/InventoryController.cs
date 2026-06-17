using ECommerce.Inventory.Application.DTOs;
using ECommerce.Inventory.Application.Features.Commands;
using ECommerce.Inventory.Application.Features.Queries;
using ECommerce.Shared.Common.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get stock for a specific product.</summary>
    [HttpGet("product/{productId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> GetByProduct(
        Guid productId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStockByProductIdQuery(productId), cancellationToken);
        return Ok(ApiResponse<InventoryDto>.Ok(result));
    }

    /// <summary>Get all inventory records. Admin only.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<InventoryDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllInventoryQuery(), cancellationToken);
        return Ok(ApiResponse<List<InventoryDto>>.Ok(result));
    }

    /// <summary>Get low stock alerts. Admin only.</summary>
    [HttpGet("low-stock")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<InventoryDto>>>> GetLowStock(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLowStockQuery(), cancellationToken);
        return Ok(ApiResponse<List<InventoryDto>>.Ok(result));
    }

    /// <summary>Set initial stock for a product. Admin only.</summary>
    [HttpPost("set")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> SetStock(
        [FromBody] SetStockDto dto, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SetStockCommand(dto), cancellationToken);
        return Ok(ApiResponse<InventoryDto>.Ok(result, "Stock set successfully."));
    }

    /// <summary>
    /// Manually adjust stock. Admin only.
    /// Positive quantity = restock. Negative = manual write-off.
    /// </summary>
    [HttpPatch("product/{productId:guid}/adjust")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> AdjustStock(
        Guid productId, [FromBody] AdjustStockDto dto, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AdjustStockCommand(productId, dto), cancellationToken);
        return Ok(ApiResponse<InventoryDto>.Ok(result, "Stock adjusted successfully."));
    }
}
