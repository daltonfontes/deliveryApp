namespace DeliveryApp.Domain.Entities;

using System.Collections.Generic;
using DeliveryApp.Domain.Enums;

public class Order
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid? DeliveryDriverId { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; private set; }
    public string DeliveryAddress { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public Customer Customer { get; private set; } = null!;
    public DeliveryDriver? DeliveryDriver { get; private set; }
    public ICollection<OrderItem> Items { get; private set; } = [];

    private Order() { }
    public static Order Create(Guid customerId, string deliveryAddress, List<OrderItem> orderItems)
    {

        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID cannot be empty.", nameof(customerId));

        if (string.IsNullOrWhiteSpace(deliveryAddress))
            throw new ArgumentException("Delivery address cannot be empty or whitespace.", nameof(deliveryAddress));

        if (orderItems == null || !orderItems.Any())
            throw new ArgumentException("Order must contain at least one item.", nameof(orderItems));

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            DeliveryAddress = deliveryAddress,
            Items = orderItems,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        order.CalculateTotalAmount();
        return order;
    }

    private void CalculateTotalAmount()
    {
        TotalAmount = Items.Sum(i => i.Quantity * i.UnitPrice);
    }
    public void Pay()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be paid.");

        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Prepare()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed orders can be prepared.");

        Status = OrderStatus.Preparing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Ship(Guid deliveryDriverId)
    {
        if (Status != OrderStatus.Preparing)
            throw new InvalidOperationException("Only preparing orders can be shipped.");

        DeliveryDriverId = deliveryDriverId;
        Status = OrderStatus.Shipped;
        UpdatedAt = DateTime.UtcNow;
    }
    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException("Only shipped orders can be delivered.");

        Status = OrderStatus.Delivered;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Delivered orders cannot be cancelled.");

        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Order is already cancelled.");

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddItem(OrderItem item)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Items can only be added to pending orders.");

        Items.Add(item);
        CalculateTotalAmount();
        UpdatedAt = DateTime.UtcNow;
    }

}
