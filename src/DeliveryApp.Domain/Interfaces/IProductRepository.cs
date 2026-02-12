namespace DeliveryApp.Domain.Interfaces;

using Entities;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default);
}
