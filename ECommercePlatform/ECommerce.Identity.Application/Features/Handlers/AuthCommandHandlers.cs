using ECommerce.Identity.Application.DTOs;
using ECommerce.Identity.Application.Features.Commands;
using ECommerce.Identity.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using static ECommerce.Identity.Application.Interfaces.IIdentityInterfaces;
using static ECommerce.Shared.Common.Exceptions.DomainExceptions;

namespace ECommerce.Identity.Application.Features.Handlers
{
    public class AuthCommandHandlers
    {
        public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, TokenResponseDto>
        {
            private readonly IUserRepository _userRepository;
            private readonly IRefreshTokenRepository _refreshTokenRepository;
            private readonly IPasswordHashService _passwordHashService;
            private readonly ITokenService _tokenService;

            public RegisterUserCommandHandler(
                IUserRepository userRepository,
                IRefreshTokenRepository refreshTokenRepository,
                IPasswordHashService passwordHashService,
                ITokenService tokenService)
            {
                _userRepository = userRepository;
                _refreshTokenRepository = refreshTokenRepository;
                _passwordHashService = passwordHashService;
                _tokenService = tokenService;
            }

            public async Task<TokenResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
            {
                // 1. Check for duplicate email
                var emailExists = await _userRepository.EmailExistsAsync(request.Dto.Email, cancellationToken);
                if (emailExists)
                    throw new BusinessRuleException($"Email '{request.Dto.Email}' is already registered.");

                // 2. Create the User domain entity
                var user = new User
                {
                    FirstName = request.Dto.FirstName,
                    LastName = request.Dto.LastName,
                    Email = request.Dto.Email.ToLowerInvariant(), // Always store lowercase
                    PasswordHash = _passwordHashService.Hash(request.Dto.Password)
                };

                // 3. Persist the user
                await _userRepository.AddAsync(user, cancellationToken);

                // 4. Generate tokens
                return await GenerateTokenResponse(user, cancellationToken);
            }

            private async Task<TokenResponseDto> GenerateTokenResponse(User user, CancellationToken cancellationToken)
            {
                var accessToken = _tokenService.GenerateAccessToken(user);
                var rawRefreshToken = _tokenService.GenerateRefreshToken();

                var refreshToken = new RefreshToken
                {
                    Token = rawRefreshToken,
                    UserId = user.Id,
                    ExpiresAt = DateTime.UtcNow.AddDays(7)
                };

                await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

                return new TokenResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = rawRefreshToken,
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
        /// Handles user login.
        ///
        /// Steps:
        ///   1. Find user by email
        ///   2. Verify password using BCrypt (timing-safe comparison)
        ///   3. Generate and return new tokens
        /// </summary>
        public class LoginCommandHandler : IRequestHandler<LoginCommand, TokenResponseDto>
        {
            private readonly IUserRepository _userRepository;
            private readonly IRefreshTokenRepository _refreshTokenRepository;
            private readonly IPasswordHashService _passwordHashService;
            private readonly ITokenService _tokenService;

            public LoginCommandHandler(
                IUserRepository userRepository,
                IRefreshTokenRepository refreshTokenRepository,
                IPasswordHashService passwordHashService,
                ITokenService tokenService)
            {
                _userRepository = userRepository;
                _refreshTokenRepository = refreshTokenRepository;
                _passwordHashService = passwordHashService;
                _tokenService = tokenService;
            }

            public async Task<TokenResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
            {
                // Security tip: always say "Invalid credentials" — never "email not found"
                // Telling which one is wrong helps attackers enumerate valid emails
                var user = await _userRepository.GetByEmailAsync(
                    request.Dto.Email.ToLowerInvariant(), cancellationToken);

                if (user == null || !_passwordHashService.Verify(request.Dto.Password, user.PasswordHash))
                    throw new BusinessRuleException("Invalid email or password.");

                if (!user.IsActive)
                    throw new BusinessRuleException("Your account has been deactivated.");

                var accessToken = _tokenService.GenerateAccessToken(user);
                var rawRefreshToken = _tokenService.GenerateRefreshToken();

                var refreshToken = new RefreshToken
                {
                    Token = rawRefreshToken,
                    UserId = user.Id,
                    ExpiresAt = DateTime.UtcNow.AddDays(7)
                };

                await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

                return new TokenResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = rawRefreshToken,
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

    }
}
