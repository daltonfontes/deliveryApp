
using DeliveryApp.Domain.Enums;

namespace DeliveryApp.Application.DTOs.DeliveryDrivers;
public record DeliveryDriverResponse(
    Guid Id,
    string Name,
    string Phone,
    VehicleType VehicleType,
    bool IsAvailable,
    DateTime CreatedAt);
