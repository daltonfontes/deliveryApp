namespace DeliveryApp.Domain.Interfaces;

using Entities;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<Product>> GetIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
