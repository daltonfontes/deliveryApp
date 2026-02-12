namespace DeliveryApp.Application.Interfaces;

using DeliveryApp.Application.DTOs.DeliveryDrivers;

public interface IDeliveryDriverService
{
    Task<DeliveryDriverResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<DeliveryDriverResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<DeliveryDriverResponse>> GetAvailableAsync(CancellationToken cancellationToken = default);
    Task<DeliveryDriverResponse> CreateAsync(CreateDeliveryDriverRequest request, CancellationToken cancellationToken = default);
    Task<DeliveryDriverResponse> UpdateAsync(Guid id, UpdateDeliveryDriverRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
