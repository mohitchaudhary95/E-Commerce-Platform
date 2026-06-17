using ECommerce.Cart.Application.DTOs;
using ECommerce.Cart.Application.Features.Commands;
using ECommerce.Cart.Application.Features.Queries;
using ECommerce.Shared.Common.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.Cart.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CartController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get the current user's cart.
        /// Returns an empty cart (not 404) if user hasn't added anything yet.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<CartDto>>> GetCart(CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new GetUserCartQuery(userId), cancellationToken);
            return Ok(ApiResponse<CartDto>.Ok(result));
        }

        /// <summary>
        /// Add a product to the cart.
        /// Calls ProductService internally to validate product and fetch price.
        /// </summary>
        [HttpPost("items")]
        public async Task<ActionResult<ApiResponse<CartDto>>> AddItem(
            [FromBody] AddToCartDto dto,
            CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new AddToCartCommand(userId, dto), cancellationToken);
            return Ok(ApiResponse<CartDto>.Ok(result, "Item added to cart."));
        }

        /// <summary>
        /// Update the quantity of a specific cart item.
        /// Setting quantity to 0 removes the item.
        /// </summary>
        [HttpPut("items/{itemId:guid}")]
        public async Task<ActionResult<ApiResponse<CartDto>>> UpdateQuantity(
            Guid itemId,
            [FromBody] UpdateQuantityDto dto,
            CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new UpdateQuantityCommand(userId, itemId, dto), cancellationToken);
            return Ok(ApiResponse<CartDto>.Ok(result, "Quantity updated."));
        }

        /// <summary>
        /// Remove a specific item from the cart.
        /// </summary>
        [HttpDelete("items/{itemId:guid}")]
        public async Task<ActionResult<ApiResponse<CartDto>>> RemoveItem(
            Guid itemId,
            CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new RemoveCartItemCommand(userId, itemId), cancellationToken);
            return Ok(ApiResponse<CartDto>.Ok(result, "Item removed from cart."));
        }

        /// <summary>
        /// Clear all items from the cart.
        /// Called after an order is placed successfully.
        /// </summary>
        [HttpDelete]
        public async Task<ActionResult<ApiResponse<object>>> ClearCart(CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            await _mediator.Send(new ClearCartCommand(userId), cancellationToken);
            return Ok(ApiResponse.OkNoData("Cart cleared."));
        }

        // -- Helper ----------------------------------------------------------------

        /// <summary>
        /// Extracts UserId from the JWT token's "sub" claim.
        /// This is set by IdentityService when the token is generated.
        /// </summary>
        private Guid GetUserId()
            => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
