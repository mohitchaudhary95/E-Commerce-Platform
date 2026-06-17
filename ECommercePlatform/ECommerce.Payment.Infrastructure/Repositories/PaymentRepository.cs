using ECommerce.Payment.Application.Interfaces;
using DomainPayment = ECommerce.Payment.Domain.Entities.Payment;
using ECommerce.Payment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Payment.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;

    public PaymentRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<DomainPayment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Payments.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<DomainPayment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        => await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);

    public async Task AddAsync(DomainPayment payment, CancellationToken cancellationToken = default)
    {
        await _context.Payments.AddAsync(payment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(DomainPayment payment, CancellationToken cancellationToken = default)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
