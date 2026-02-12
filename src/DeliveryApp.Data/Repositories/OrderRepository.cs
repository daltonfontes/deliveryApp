
using DeliveryApp.Data.Context;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Enums;
using DeliveryApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Data.Repositories;
public class OrderRepository(DeliveryAppDbContext context) : Repository<Order>(context), IOrderRepository
{
    public async Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.DeliveryDriver)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.DeliveryDriver)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Order>> GetOrdersByDriverIdAsync(Guid driverId, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Where(o => o.DeliveryDriverId == driverId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<Order?> GetOrderWithDetailsAsync(Guid orderId, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(o => o.Customer)
            .Include(o => o.DeliveryDriver)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
}
