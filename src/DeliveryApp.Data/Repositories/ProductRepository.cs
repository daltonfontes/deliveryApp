using DeliveryApp.Data.Context;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Data.Repositories;

public class ProductRepository(DeliveryAppDbContext context) : Repository<Product>(context), IProductRepository
{
    public new async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet.Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public new async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Include(p => p.Category)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
}
