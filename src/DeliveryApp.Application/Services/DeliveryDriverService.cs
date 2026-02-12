namespace DeliveryApp.Application.Services;

using DeliveryApp.Application.DTOs.DeliveryDrivers;
using DeliveryApp.Application.Interfaces;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Exceptions;
using DeliveryApp.Domain.Interfaces;

public class DeliveryDriverService(IDeliveryDriverRepository driverRepository) : IDeliveryDriverService
{
    public async Task<DeliveryDriverResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var driver = await driverRepository.GetByIdAsync(id, cancellationToken);
        return driver is null ? null : MapToResponse(driver);
    }

    public async Task<IEnumerable<DeliveryDriverResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var drivers = await driverRepository.GetAllAsync(cancellationToken);
        return drivers.Select(MapToResponse);
    }

    public async Task<IEnumerable<DeliveryDriverResponse>> GetAvailableAsync(CancellationToken cancellationToken = default)
    {
        var drivers = await driverRepository.GetAvailableDriversAsync(cancellationToken);
        return drivers.Select(MapToResponse);
    }

    public async Task<DeliveryDriverResponse> CreateAsync(CreateDeliveryDriverRequest request, CancellationToken cancellationToken = default)
    {
        var driver = new DeliveryDriver
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Phone = request.Phone,
            VehicleType = request.VehicleType,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };

        await driverRepository.AddAsync(driver, cancellationToken);
        await driverRepository.SaveChangesAsync(cancellationToken);
        return MapToResponse(driver);
    }

    public async Task<DeliveryDriverResponse> UpdateAsync(Guid id, UpdateDeliveryDriverRequest request, CancellationToken cancellationToken = default)
    {
        var driver = await driverRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("DeliveryDriver", id);

        driver.Name = request.Name;
        driver.Phone = request.Phone;
        driver.VehicleType = request.VehicleType;
        driver.IsAvailable = request.IsAvailable;

        await driverRepository.UpdateAsync(driver, cancellationToken);
        await driverRepository.SaveChangesAsync(cancellationToken);
        return MapToResponse(driver);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var driver = await driverRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("DeliveryDriver", id);

        await driverRepository.DeleteAsync(driver, cancellationToken);
        await driverRepository.SaveChangesAsync(cancellationToken);
    }

    private static DeliveryDriverResponse MapToResponse(DeliveryDriver d) =>
        new(d.Id, d.Name, d.Phone, d.VehicleType, d.IsAvailable, d.CreatedAt);
}
