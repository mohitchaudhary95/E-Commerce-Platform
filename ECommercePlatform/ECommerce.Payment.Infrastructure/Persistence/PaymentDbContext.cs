using ECommerce.Payment.Domain.Entities;
using ECommerce.Payment.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Payment.Infrastructure.Persistence;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.OrderId).IsRequired();
        builder.Property(p => p.UserId).IsRequired();

        builder.Property(p => p.UserEmail)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.FailureReason).HasMaxLength(500);
        builder.Property(p => p.GatewayTransactionId).HasMaxLength(200);
        builder.Property(p => p.CardLastFour).HasMaxLength(4);

        // One order = one payment record (idempotency enforced at DB level)
        builder.HasIndex(p => p.OrderId).IsUnique();

        // Lookup payments by user
        builder.HasIndex(p => p.UserId);
    }
}
