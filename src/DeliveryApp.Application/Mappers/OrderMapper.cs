
using DeliveryApp.Application.DTOs.Orders;
using DeliveryApp.Domain.Entities;

namespace DeliveryApp.Application.Mappers
{
    public static class OrderMapper
    {
        public static OrderResponse MapToResponse(Order order) =>
        new(
            order.Id,
            order.CustomerId,
            order.Customer?.Name ?? string.Empty,
            order.DeliveryDriverId,
            order.DeliveryDriver?.Name,
            order.Status,
            order.TotalAmount,
            order.DeliveryAddress,
            order.CreatedAt,
            order.UpdatedAt,
            order.Items.Select(item => new OrderItemResponse(
                item.Id,
                item.ProductId,
                item.Product?.Name ?? string.Empty,
                item.Quantity,
                item.UnitPrice)).ToList());

    }
}
