namespace DeliveryApp.Application.Interfaces;

using DeliveryApp.Application.DTOs.Orders;

public interface IOrderService
{
    Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderResponse>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderResponse> PayOrderAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderResponse> PrepareOrderAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderResponse> ShipOrderAsync(Guid id, ShipOrderRequest request, CancellationToken cancellationToken = default);
    Task<OrderResponse> DeliverOrderAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderResponse> CancelOrderAsync(Guid id, CancellationToken cancellationToken = default);
}
