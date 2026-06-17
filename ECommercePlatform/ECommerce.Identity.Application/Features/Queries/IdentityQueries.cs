using ECommerce.Identity.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Identity.Application.Features.Queries
{
    public record GetUserByIdQuery(Guid UserId) : IRequest<UserDto>;

    public record GetAllUsersQuery : IRequest<List<UserDto>>;
}
