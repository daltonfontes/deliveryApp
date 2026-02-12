namespace DeliveryApp.Application.DTOs.Orders;

public record OrderItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);
