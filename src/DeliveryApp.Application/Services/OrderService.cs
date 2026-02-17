namespace DeliveryApp.Application.Services;

using System.Security.Claims;
using DeliveryApp.Application.DTOs.Orders;
using DeliveryApp.Application.Interfaces;
using DeliveryApp.Application.Mappers;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Domain.Exceptions;
using DeliveryApp.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

public class OrderService(
    IOrderRepository orderRepository,
    ICustomerRepository customerRepository,
    IProductRepository productRepository,
    IDeliveryDriverRepository driverRepository,
    IHttpContextAccessor httpContextAccessor) : IOrderService
{
    public async Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetOrderWithDetailsAsync(id, cancellationToken);
        if (order is null) return null;

        await AuthorizeOrderAccessAsync(order);

        return OrderMapper.MapToResponse(order);
    }

    public async Task<IEnumerable<OrderResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = await orderRepository.GetAllAsync(cancellationToken);
        return orders.Select(OrderMapper.MapToResponse);
    }

    public async Task<IEnumerable<OrderResponse>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        await AuthorizeCustomerAccessAsync(customerId);

        var orders = await orderRepository.GetOrdersByCustomerIdAsync(customerId, cancellationToken);
        return orders.Select(OrderMapper.MapToResponse);
    }

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await ValidateCustomerAsync(request.CustomerId, cancellationToken);
        var orderItems = await CreateOrderItemsAsync(request.Items, cancellationToken);

        var order = Order.Create(customer.Id, request.DeliveryAddress, orderItems);

        await orderRepository.AddAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);

        var created = await orderRepository.GetOrderWithDetailsAsync(order.Id, cancellationToken);
        return OrderMapper.MapToResponse(created!);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Order", id);

        await orderRepository.DeleteAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<OrderResponse> ShipOrderAsync(Guid id, ShipOrderRequest request, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetOrderWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Order", id);

        var driver = await driverRepository.GetByIdAsync(request.DeliveryDriverId, cancellationToken)
            ?? throw new NotFoundException("DeliveryDriver", request.DeliveryDriverId);

        order.Ship(driver.Id);

        await orderRepository.UpdateAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);
        return OrderMapper.MapToResponse(order);
    }


    public async Task<OrderResponse> PayOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetOrderWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Order", id);

        order.Pay();

        await orderRepository.UpdateAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);
        return OrderMapper.MapToResponse(order);
    }

    public async Task<OrderResponse> PrepareOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetOrderWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Order", id);

        order.Prepare();

        await orderRepository.UpdateAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);
        return OrderMapper.MapToResponse(order);
    }

    public async Task<OrderResponse> DeliverOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetOrderWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Order", id);

        order.Deliver();

        await orderRepository.UpdateAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);
        return OrderMapper.MapToResponse(order);
    }

    public async Task<OrderResponse> CancelOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetOrderWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Order", id);

        order.Cancel();

        await orderRepository.UpdateAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);
        return OrderMapper.MapToResponse(order);
    }

    private async Task<Customer> ValidateCustomerAsync(Guid customerId, CancellationToken cancellationToken)
        => await customerRepository.GetByIdAsync(customerId, cancellationToken)
            ?? throw new NotFoundException("Customer", customerId);

    private async Task<List<OrderItem>> CreateOrderItemsAsync(IEnumerable<OrderItemRequest> itemRequests, CancellationToken cancellationToken)
    {
        var requests = itemRequests.ToList();
        var productIds = requests.Select(r => r.ProductId);
        var products = (await productRepository.GetIdsAsync(productIds, cancellationToken))
            .ToDictionary(p => p.Id);

        var orderItems = new List<OrderItem>();
        foreach (var itemRequest in requests)
        {
            if (!products.TryGetValue(itemRequest.ProductId, out var product))
                throw new NotFoundException("Product", itemRequest.ProductId);

            orderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Quantity = itemRequest.Quantity,
                UnitPrice = product.Price
            });
        }
        return orderItems;
    }

    private async Task AuthorizeOrderAccessAsync(Order order)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user is null) throw new ForbiddenException();

        if (user.IsInRole("Admin")) return;

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) throw new ForbiddenException();

        var customer = await customerRepository.GetByUserIdAsync(userId);
        if (customer is null || customer.Id != order.CustomerId)
            throw new ForbiddenException();
    }

    private async Task AuthorizeCustomerAccessAsync(Guid customerId)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user is null) throw new ForbiddenException();

        if (user.IsInRole("Admin")) return;

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) throw new ForbiddenException();

        var customer = await customerRepository.GetByUserIdAsync(userId);
        if (customer is null || customer.Id != customerId)
            throw new ForbiddenException();
    }

}
