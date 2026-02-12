namespace DeliveryApp.Domain.Entities;

using Enums;

public class DeliveryDriver
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public VehicleType VehicleType { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<Order> Orders { get; set; } = [];
}
