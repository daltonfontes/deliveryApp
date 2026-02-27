namespace DeliveryApp.Application.Services;

using DeliveryApp.Application.DTOs.DeliveryDrivers;
using DeliveryApp.Application.Interfaces;
using DeliveryApp.Application.Mappers;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Exceptions;
using DeliveryApp.Domain.Interfaces;

public class DeliveryDriverService(IDeliveryDriverRepository driverRepository) : IDeliveryDriverService
{
    public async Task<DeliveryDriverResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var driver = await driverRepository.GetByIdAsync(id, cancellationToken);
        return driver is null ? null : DeliveryDriverMapper.MapToResponse(driver);
    }

    public async Task<IEnumerable<DeliveryDriverResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var drivers = await driverRepository.GetAllAsync(cancellationToken);
        return drivers.Select(DeliveryDriverMapper.MapToResponse);
    }

    public async Task<IEnumerable<DeliveryDriverResponse>> GetAvailableAsync(CancellationToken cancellationToken = default)
    {
        var drivers = await driverRepository.GetAvailableDriversAsync(cancellationToken);
        return drivers.Select(DeliveryDriverMapper.MapToResponse);
    }

    public async Task<DeliveryDriverResponse> CreateAsync(CreateDeliveryDriverRequest request, CancellationToken cancellationToken = default)
    {
        var driver = DeliveryDriver.Create(request.Name, request.Phone, request.VehicleType);

        await driverRepository.AddAsync(driver, cancellationToken);
        await driverRepository.SaveChangesAsync(cancellationToken);
        return DeliveryDriverMapper.MapToResponse(driver);
    }

    public async Task<DeliveryDriverResponse> UpdateAsync(Guid id, UpdateDeliveryDriverRequest request, CancellationToken cancellationToken = default)
    {
        var driver = await driverRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("DeliveryDriver", id);

        driver.Update(request.Name, request.Phone, request.VehicleType);

        if (request.IsAvailable && !driver.IsAvailable)
            driver.MakeAvailable();
        else if (!request.IsAvailable && driver.IsAvailable)
            driver.MakeUnavailable();

        await driverRepository.UpdateAsync(driver, cancellationToken);
        await driverRepository.SaveChangesAsync(cancellationToken);
        return DeliveryDriverMapper.MapToResponse(driver);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var driver = await driverRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("DeliveryDriver", id);

        await driverRepository.DeleteAsync(driver, cancellationToken);
        await driverRepository.SaveChangesAsync(cancellationToken);
    }
}
