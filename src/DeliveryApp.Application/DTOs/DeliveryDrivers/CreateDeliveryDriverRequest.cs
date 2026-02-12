
using DeliveryApp.Domain.Enums;

namespace DeliveryApp.Application.DTOs.DeliveryDrivers;
public record CreateDeliveryDriverRequest(
    string Name,
    string Phone,
    VehicleType VehicleType);
