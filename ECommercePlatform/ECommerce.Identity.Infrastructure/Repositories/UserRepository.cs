using ECommerce.Identity.Domain.Entities;
using ECommerce.Identity.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;
using ECommerce.Identity.Application.Interfaces;

namespace ECommerce.Identity.Infrastructure.Repositories
{
        public class UserRepository : IIdentityInterfaces.IUserRepository
        {
            private readonly IdentityDbContext _context;

            public UserRepository(IdentityDbContext context)
            {
                _context = context;
            }

            public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
                => await _context.Users
                    .Include(u => u.RefreshTokens) // Eagerly load tokens when fetching user
                    .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

            public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
                => await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
                => await _context.Users
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.CreatedAt)
                    .ToListAsync(cancellationToken);

            public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
                => await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);

            public async Task AddAsync(User user, CancellationToken cancellationToken = default)
            {
                await _context.Users.AddAsync(user, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public class RefreshTokenRepository : IIdentityInterfaces.IRefreshTokenRepository
        {
            private readonly IdentityDbContext _context;

            public RefreshTokenRepository(IdentityDbContext context)
            {
                _context = context;
            }

            public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
                => await _context.RefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

            public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
            {
                await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            public async Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
            {
                _context.RefreshTokens.Update(refreshToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
            {
                // Bulk update — more efficient than loading each token individually
                await _context.RefreshTokens
                    .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(rt => rt.IsRevoked, true),
                    cancellationToken);
            }
        }
    }

