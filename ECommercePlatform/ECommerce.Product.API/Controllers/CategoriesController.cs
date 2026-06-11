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
    public class CategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAllCategoriesQuery(), cancellationToken);
            return Ok(ApiResponse<List<CategoryDto>>.Ok(result));
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCategoryByIdQuery(id), cancellationToken);
            return Ok(ApiResponse<CategoryDto>.Ok(result));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> Create(
            [FromBody] CreateCategoryDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CreateCategoryCommand(dto), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                ApiResponse<CategoryDto>.Created(result, "Category created successfully."));
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> Update(
            Guid id,
            [FromBody] UpdateCategoryDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new UpdateCategoryCommand(id, dto), cancellationToken);
            return Ok(ApiResponse<CategoryDto>.Ok(result, "Category updated successfully."));
        }
    }
}
