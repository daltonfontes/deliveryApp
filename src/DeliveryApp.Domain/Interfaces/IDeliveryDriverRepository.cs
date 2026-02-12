namespace DeliveryApp.Domain.Interfaces;

using Entities;

public interface IDeliveryDriverRepository : IRepository<DeliveryDriver>
{
    Task<IEnumerable<DeliveryDriver>> GetAvailableDriversAsync(CancellationToken cancellationToken = default);
}
