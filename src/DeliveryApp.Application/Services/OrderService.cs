namespace DeliveryApp.Application.Services;

using DeliveryApp.Application.DTOs.Orders;
using DeliveryApp.Application.Interfaces;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Exceptions;
using DeliveryApp.Domain.Interfaces;

public class OrderService(
    IOrderRepository orderRepository,
    ICustomerRepository customerRepository,
    IProductRepository productRepository,
    IDeliveryDriverRepository driverRepository) : IOrderService
{
    public async Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetOrderWithDetailsAsync(id, cancellationToken);
        return order is null ? null : MapToResponse(order);
    }

    public async Task<IEnumerable<OrderResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = await orderRepository.GetAllAsync(cancellationToken);
        return orders.Select(MapToResponse);
    }

    public async Task<IEnumerable<OrderResponse>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var orders = await orderRepository.GetOrdersByCustomerIdAsync(customerId, cancellationToken);
        return orders.Select(MapToResponse);
    }

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new NotFoundException("Customer", request.CustomerId);

        var orderItems = new List<OrderItem>();

        foreach (var itemRequest in request.Items)
        {
            var product = await productRepository.GetByIdAsync(itemRequest.ProductId, cancellationToken)
                ?? throw new NotFoundException("Product", itemRequest.ProductId);

            orderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Quantity = itemRequest.Quantity,
                UnitPrice = product.Price
            });
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            Status = Domain.Enums.OrderStatus.Pending,
            DeliveryAddress = request.DeliveryAddress,
            TotalAmount = orderItems.Sum(i => i.Quantity * i.UnitPrice),
            CreatedAt = DateTime.UtcNow,
            Items = orderItems
        };

        await orderRepository.AddAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);

        var created = await orderRepository.GetOrderWithDetailsAsync(order.Id, cancellationToken);
        return MapToResponse(created!);
    }

    public async Task<OrderResponse> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetOrderWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Order", id);

        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        await orderRepository.UpdateAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);
        return MapToResponse(order);
    }

    public async Task<OrderResponse> AssignDriverAsync(Guid id, AssignDriverRequest request, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetOrderWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Order", id);

        var driver = await driverRepository.GetByIdAsync(request.DeliveryDriverId, cancellationToken)
            ?? throw new NotFoundException("DeliveryDriver", request.DeliveryDriverId);

        order.DeliveryDriverId = driver.Id;
        order.UpdatedAt = DateTime.UtcNow;

        await orderRepository.UpdateAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);
        return MapToResponse(order);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Order", id);

        await orderRepository.DeleteAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);
    }

    private static OrderResponse MapToResponse(Order o) =>
        new(
            o.Id,
            o.CustomerId,
            o.Customer?.Name ?? string.Empty,
            o.DeliveryDriverId,
            o.DeliveryDriver?.Name,
            o.Status,
            o.TotalAmount,
            o.DeliveryAddress,
            o.CreatedAt,
            o.UpdatedAt,
            o.Items.Select(i => new OrderItemResponse(
                i.Id,
                i.ProductId,
                i.Product?.Name ?? string.Empty,
                i.Quantity,
                i.UnitPrice)).ToList());
}
