using ECommerce.Product.Application.DTOs;
using ECommerce.Product.Application.Features.Commands;
using ECommerce.Product.Application.Features.Queries;
using ECommerce.Shared.Common.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Product.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get paginated, filtered product list.
        /// [FromQuery] maps URL query params to the DTO automatically.
        ///
        /// Example: GET /api/products?searchTerm=laptop&categoryId=xxx&minPrice=100&pageNumber=1&pageSize=10
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<PagedResult<ProductDto>>>> GetAll(
            [FromQuery] ProductFilterDto filter,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetProductsQuery(filter), cancellationToken);
            return Ok(ApiResponse<PagedResult<ProductDto>>.Ok(result));
        }

        /// <summary>
        /// Get a single product by ID.
        /// </summary>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetProductByIdQuery(id), cancellationToken);
            return Ok(ApiResponse<ProductDto>.Ok(result));
        }

        /// <summary>
        /// Create a new product. Admin only.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> Create(
            [FromBody] CreateProductDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CreateProductCommand(dto), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                ApiResponse<ProductDto>.Created(result, "Product created successfully."));
        }

        /// <summary>
        /// Update an existing product. Admin only.
        /// Partial update — only sends the fields that changed.
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> Update(
            Guid id,
            [FromBody] UpdateProductDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new UpdateProductCommand(id, dto), cancellationToken);
            return Ok(ApiResponse<ProductDto>.Ok(result, "Product updated successfully."));
        }

        /// <summary>
        /// Soft-delete a product. Admin only.
        /// Sets IsActive = false — product still exists in DB for historical orders.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new DeleteProductCommand(id), cancellationToken);
            return Ok(ApiResponse.OkNoData("Product deleted successfully."));
        }
    }
}
