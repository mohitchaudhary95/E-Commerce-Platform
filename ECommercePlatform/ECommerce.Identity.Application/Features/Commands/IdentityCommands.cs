using ECommerce.Identity.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Identity.Application.Features.Commands
{

    public record RegisterUserCommand(RegisterDto Dto) : IRequest<TokenResponseDto>;

    public record LoginCommand(LoginDto Dto) : IRequest<TokenResponseDto>;

    public record RefreshTokenCommand(string RefreshToken) : IRequest<TokenResponseDto>;

    public record ChangePasswordCommand(Guid UserId, ChangePasswordDto Dto) : IRequest<bool>;

    public record RevokeTokenCommand(Guid UserId) : IRequest<bool>;
}
