
using DeliveryApp.Domain.Enums;

namespace DeliveryApp.Application.DTOs.Orders;
public record UpdateOrderStatusRequest(OrderStatus Status);
