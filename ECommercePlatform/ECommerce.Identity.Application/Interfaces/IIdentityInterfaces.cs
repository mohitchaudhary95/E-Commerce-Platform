using System;
using System.Collections.Generic;
using System.Text;
using ECommerce.Identity.Domain.Entities;

namespace ECommerce.Identity.Application.Interfaces
{
    public interface IIdentityInterfaces
    {
        public interface IUserRepository
        {
            Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
            Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
            Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
            Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
            Task AddAsync(User user, CancellationToken cancellationToken = default);
            Task UpdateAsync(User user, CancellationToken cancellationToken = default);
        }

        /// <summary>
        /// Repository interface for RefreshToken data access.
        /// </summary>
        public interface IRefreshTokenRepository
        {
            Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
            Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
            Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
            Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
        }

        /// <summary>
        /// Handles JWT access token generation and refresh token creation.
        /// Implemented in Infrastructure because it depends on JwtSettings from config.
        /// </summary>
        public interface ITokenService
        {
            string GenerateAccessToken(User user);
            string GenerateRefreshToken();
            DateTime GetAccessTokenExpiry();
        }

        /// <summary>
        /// Wraps BCrypt password hashing.
        /// Keeping this behind an interface makes unit testing easy (mock it).
        /// </summary>
        public interface IPasswordHashService
        {
            string Hash(string password);
            bool Verify(string password, string hash);
        }

    }
}
