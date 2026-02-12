namespace DeliveryApp.Domain.Interfaces;

using DeliveryApp.Domain.Entities;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Category?> GetWithProductsAsync(Guid id, CancellationToken cancellationToken = default);
}
