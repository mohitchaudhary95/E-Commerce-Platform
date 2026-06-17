using ECommerce.Identity.Application.DTOs;
using ECommerce.Identity.Application.Features.Commands;
using ECommerce.Identity.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using static ECommerce.Identity.Application.Interfaces.IIdentityInterfaces;
using ECommerce.Shared.Common.Exceptions;

namespace ECommerce.Identity.Application.Features.Handlers
{
    public class TokenCommandHandlers
    {
        public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, TokenResponseDto>
        {
            private readonly IUserRepository _userRepository;
            private readonly IRefreshTokenRepository _refreshTokenRepository;
            private readonly ITokenService _tokenService;

            public RefreshTokenCommandHandler(
                IUserRepository userRepository,
                IRefreshTokenRepository refreshTokenRepository,
                ITokenService tokenService)
            {
                _userRepository = userRepository;
                _refreshTokenRepository = refreshTokenRepository;
                _tokenService = tokenService;
            }

            public async Task<TokenResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
            {
                var storedToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);

                // Security: vague error message — don't hint what's wrong
                if (storedToken == null || !storedToken.IsActive)
                    throw new BusinessRuleException("Invalid or expired refresh token.");

                var user = await _userRepository.GetByIdAsync(storedToken.UserId, cancellationToken);
                if (user == null || !user.IsActive)
                    throw new BusinessRuleException("Invalid or expired refresh token.");

                // Mark old token as used — token rotation (prevents reuse)
                storedToken.IsUsed = true;
                await _refreshTokenRepository.UpdateAsync(storedToken, cancellationToken);

                // Issue new token pair
                var newAccessToken = _tokenService.GenerateAccessToken(user);
                var newRawRefreshToken = _tokenService.GenerateRefreshToken();

                var newRefreshToken = new RefreshToken
                {
                    Token = newRawRefreshToken,
                    UserId = user.Id,
                    ExpiresAt = DateTime.UtcNow.AddDays(7)
                };

                await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

                return new TokenResponseDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRawRefreshToken,
                    AccessTokenExpiresAt = _tokenService.GetAccessTokenExpiry(),
                    User = new UserDto
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        Role = user.Role,
                        CreatedAt = user.CreatedAt
                    }
                };
            }
        }

        /// <summary>
        /// Handles password change for an authenticated user.
        /// </summary>
        public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
        {
            private readonly IUserRepository _userRepository;
            private readonly IRefreshTokenRepository _refreshTokenRepository;
            private readonly IPasswordHashService _passwordHashService;

            public ChangePasswordCommandHandler(
                IUserRepository userRepository,
                IRefreshTokenRepository refreshTokenRepository,
                IPasswordHashService passwordHashService)
            {
                _userRepository = userRepository;
                _refreshTokenRepository = refreshTokenRepository;
                _passwordHashService = passwordHashService;
            }

            public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
            {
                var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
                    ?? throw new NotFoundException("User", request.UserId);

                // Verify the user knows their current password
                if (!_passwordHashService.Verify(request.Dto.CurrentPassword, user.PasswordHash))
                    throw new BusinessRuleException("Current password is incorrect.");

                user.PasswordHash = _passwordHashService.Hash(request.Dto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user, cancellationToken);

                // Revoke all refresh tokens — force re-login on all devices after password change
                await _refreshTokenRepository.RevokeAllUserTokensAsync(user.Id, cancellationToken);

                return true;
            }
        }

        /// <summary>
        /// Handles logout — revokes all refresh tokens for a user.
        /// </summary>
        public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, bool>
        {
            private readonly IRefreshTokenRepository _refreshTokenRepository;

            public RevokeTokenCommandHandler(IRefreshTokenRepository refreshTokenRepository)
            {
                _refreshTokenRepository = refreshTokenRepository;
            }

            public async Task<bool> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
            {
                await _refreshTokenRepository.RevokeAllUserTokensAsync(request.UserId, cancellationToken);
                return true;
            }
        }
    }
}
