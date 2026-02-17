
using DeliveryApp.Data.Context;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Data.Repositories;
public class CustomerRepository(DeliveryAppDbContext context) : Repository<Customer>(context), ICustomerRepository
{
    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email == email, cancellationToken);

    public async Task<Customer?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
}
