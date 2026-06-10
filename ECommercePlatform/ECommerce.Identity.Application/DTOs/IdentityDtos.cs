using System;
using System.Collections.Generic;
using System.Text;
using ECommerce.Identity.Domain.Enums;

namespace ECommerce.Identity.Application.DTOs
{   /// <summary>
    /// What the client sends when registering a new user.
    /// </summary>
    public class RegisterDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// What the client sends to log in.
    /// </summary>
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// What we return after a successful login or token refresh.
    /// </summary>
    public class TokenResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiresAt { get; set; }
        public UserDto User { get; set; } = null!;
    }

    /// <summary>
    /// What the client sends to get a new access token using a refresh token.
    /// </summary>
    public class RefreshTokenDto
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// Safe user data returned to the client (never includes password hash).
    /// </summary>
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// What the client sends to change their password.
    /// </summary>
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

}
