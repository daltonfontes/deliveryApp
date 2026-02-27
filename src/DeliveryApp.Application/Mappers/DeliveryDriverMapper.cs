using DeliveryApp.Application.DTOs.DeliveryDrivers;
using DeliveryApp.Domain.Entities;

namespace DeliveryApp.Application.Mappers;

public static class DeliveryDriverMapper
{
    public static DeliveryDriverResponse MapToResponse(DeliveryDriver driver) =>
        new(driver.Id, driver.Name, driver.Phone, driver.VehicleType, driver.IsAvailable, driver.CreatedAt);
}
