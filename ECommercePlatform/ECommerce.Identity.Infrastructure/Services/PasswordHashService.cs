using System;
using System.Collections.Generic;
using System.Text;
using static ECommerce.Identity.Application.Interfaces.IIdentityInterfaces;

namespace ECommerce.Identity.Infrastructure.Services
{
    public class PasswordHashService : IPasswordHashService
    {
        private const int WorkFactor = 12;

        public string Hash(string password)
            => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

        /// <summary>
        /// Timing-safe comparison — BCrypt.Verify takes the same time whether the
        /// password matches or not. This prevents timing attacks.
        /// </summary>
        public bool Verify(string password, string hash)
            => BCrypt.Net.BCrypt.Verify(password, hash);
    }

}
