using ECommerce.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Identity.Infrastructure.Persistence.Configurations
{
    public class EntityConfigurations
    {
        public class UserConfiguration : IEntityTypeConfiguration<User>
        {
            public void Configure(EntityTypeBuilder<User> builder)
            {
                builder.ToTable("Users");
                builder.HasKey(u => u.Id);

                builder.Property(u => u.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                builder.Property(u => u.LastName)
                    .IsRequired()
                    .HasMaxLength(50);

                builder.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                // Unique index on Email — prevents duplicate registrations at the DB level
                builder.HasIndex(u => u.Email)
                    .IsUnique();

                builder.Property(u => u.PasswordHash)
                    .IsRequired();

                // Store enum as string ("Customer", "Admin") — readable in SQL queries
                builder.Property(u => u.Role)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                builder.Property(u => u.CreatedAt)
                    .IsRequired();

                // One user has many refresh tokens
                builder.HasMany(u => u.RefreshTokens)
                    .WithOne(rt => rt.User)
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete tokens when user is deleted

                // Ignore computed property — not a DB column
                builder.Ignore(u => u.FullName);
            }
        }

        public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
        {
            public void Configure(EntityTypeBuilder<RefreshToken> builder)
            {
                builder.ToTable("RefreshTokens");
                builder.HasKey(rt => rt.Id);

                builder.Property(rt => rt.Token)
                    .IsRequired()
                    .HasMaxLength(500);

                // Index for fast lookup when validating refresh tokens
                builder.HasIndex(rt => rt.Token);

                builder.Property(rt => rt.ExpiresAt).IsRequired();
                builder.Property(rt => rt.CreatedAt).IsRequired();

                // Ignore computed property — derived from other fields
                builder.Ignore(rt => rt.IsActive);
            }
        }

    }
}
