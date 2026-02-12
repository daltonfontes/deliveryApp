
using DeliveryApp.Domain.Enums;

namespace DeliveryApp.Application.DTOs.DeliveryDrivers;
public record UpdateDeliveryDriverRequest(
    string Name,
    string Phone,
    VehicleType VehicleType,
    bool IsAvailable);
