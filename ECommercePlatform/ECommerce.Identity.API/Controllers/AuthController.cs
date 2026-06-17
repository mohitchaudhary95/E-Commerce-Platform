using ECommerce.Identity.Application.DTOs;
using ECommerce.Identity.Application.Features.Commands;
using ECommerce.Shared.Common.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.Identity.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Register a new user. Returns JWT tokens on success.
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<TokenResponseDto>>> Register(
            [FromBody] RegisterDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RegisterUserCommand(dto), cancellationToken);
            return CreatedAtAction(nameof(Register), ApiResponse<TokenResponseDto>.Created(result, "Registration successful."));
        }

        /// <summary>
        /// Login with email and password. Returns JWT tokens.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<TokenResponseDto>>> Login(
            [FromBody] LoginDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new LoginCommand(dto), cancellationToken);
            return Ok(ApiResponse<TokenResponseDto>.Ok(result, "Login successful."));
        }

        /// <summary>
        /// Exchange a valid refresh token for a new access token + refresh token pair.
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<TokenResponseDto>>> Refresh(
            [FromBody] RefreshTokenDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RefreshTokenCommand(dto.RefreshToken), cancellationToken);
            return Ok(ApiResponse<TokenResponseDto>.Ok(result, "Token refreshed."));
        }

        /// <summary>
        /// Change the authenticated user's password.
        /// Requires a valid JWT access token in the Authorization header.
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword(
            [FromBody] ChangePasswordDto dto,
            CancellationToken cancellationToken)
        {
            // Extract userId from JWT claims — set by the token when it was generated
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _mediator.Send(new ChangePasswordCommand(userId, dto), cancellationToken);
            return Ok(ApiResponse.OkNoData("Password changed successfully."));
        }

        /// <summary>
        /// Logout — revokes all refresh tokens for the current user.
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> Logout(CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _mediator.Send(new RevokeTokenCommand(userId), cancellationToken);
            return Ok(ApiResponse.OkNoData("Logged out successfully."));
        }
    }
}
