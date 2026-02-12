namespace DeliveryApp.Data.Repositories;

using DeliveryApp.Data.Context;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public class CategoryRepository(DeliveryAppDbContext context) : Repository<Category>(context), ICategoryRepository
{
    public async Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);

    public async Task<Category?> GetWithProductsAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
}
