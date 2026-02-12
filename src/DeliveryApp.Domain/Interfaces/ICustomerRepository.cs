namespace DeliveryApp.Domain.Interfaces;

using DeliveryApp.Domain.Entities;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
