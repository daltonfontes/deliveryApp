namespace DeliveryApp.Domain.Entities;

using DeliveryApp.Domain.Enums;

public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? DeliveryDriverId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Customer Customer { get; set; } = null!;
    public DeliveryDriver? DeliveryDriver { get; set; }
    public ICollection<OrderItem> Items { get; set; } = [];
}
