using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Identity.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public bool IsRevoked { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key back to User
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        // Helper — check both conditions in one place
        public bool IsActive => !IsUsed && !IsRevoked && DateTime.UtcNow < ExpiresAt;
    }

}
