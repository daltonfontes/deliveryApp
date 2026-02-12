namespace DeliveryApp.Application.DTOs.Orders;

public record CreateOrderRequest(
    Guid CustomerId,
    string DeliveryAddress,
    List<OrderItemRequest> Items);
