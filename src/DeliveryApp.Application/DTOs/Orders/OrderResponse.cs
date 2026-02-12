
using DeliveryApp.Domain.Enums;

namespace DeliveryApp.Application.DTOs.Orders;
public record OrderResponse(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    Guid? DeliveryDriverId,
    string? DeliveryDriverName,
    OrderStatus Status,
    decimal TotalAmount,
    string DeliveryAddress,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<OrderItemResponse> Items);
