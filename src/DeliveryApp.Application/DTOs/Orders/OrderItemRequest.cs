namespace DeliveryApp.Application.DTOs.Orders;

public record OrderItemRequest(
    Guid ProductId,
    int Quantity);
