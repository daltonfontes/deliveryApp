
using DeliveryApp.Data.Context;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Data.Repositories;
public class DeliveryDriverRepository(DeliveryAppDbContext context) : Repository<DeliveryDriver>(context), IDeliveryDriverRepository
{
    public async Task<IEnumerable<DeliveryDriver>> GetAvailableDriversAsync(CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Where(d => d.IsAvailable)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
}
