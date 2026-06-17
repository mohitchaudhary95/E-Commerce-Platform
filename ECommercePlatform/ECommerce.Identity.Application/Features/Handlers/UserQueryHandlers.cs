using ECommerce.Identity.Application.DTOs;
using ECommerce.Identity.Application.Features.Queries;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using static ECommerce.Identity.Application.Interfaces.IIdentityInterfaces;
using ECommerce.Shared.Common.Exceptions;

namespace ECommerce.Identity.Application.Features.Handlers
{
    public class UserQueryHandlers
    {
        public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
        {
            private readonly IUserRepository _userRepository;

            public GetUserByIdQueryHandler(IUserRepository userRepository)
            {
                _userRepository = userRepository;
            }

            public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
            {
                var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
                    ?? throw new NotFoundException("User", request.UserId);

                return new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                };
            }
        }

        public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
        {
            private readonly IUserRepository _userRepository;

            public GetAllUsersQueryHandler(IUserRepository userRepository)
            {
                _userRepository = userRepository;
            }

            public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
            {
                var users = await _userRepository.GetAllAsync(cancellationToken);

                return users.Select(u => new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt
                }).ToList();
            }
        }
    }
}
