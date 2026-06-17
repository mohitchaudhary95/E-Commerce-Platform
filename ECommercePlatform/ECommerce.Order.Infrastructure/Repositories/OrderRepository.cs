using ECommerce.Order.Application.Interfaces;
using DomainOrder = ECommerce.Order.Domain.Entities.Order;
using ECommerce.Order.Infrastructure.Persistence;
using ECommerce.Shared.Common.Responses;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Order.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<DomainOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<PagedResult<DomainOrder>> GetByUserIdAsync(
        Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt); // Newest orders first

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<DomainOrder>.Create(items, totalCount, pageNumber, pageSize);
    }

    public async Task AddAsync(DomainOrder order, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(DomainOrder order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
