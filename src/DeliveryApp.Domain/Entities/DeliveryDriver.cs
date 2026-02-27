namespace DeliveryApp.Domain.Entities;

using Enums;

public class DeliveryDriver
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public VehicleType VehicleType { get; private set; }
    public bool IsAvailable { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }

    public ICollection<Order> Orders { get; private set; } = [];

    private DeliveryDriver() { }

    public static DeliveryDriver Create(string name, string phone, VehicleType vehicleType)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Driver name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Driver phone cannot be empty.", nameof(phone));

        return new DeliveryDriver
        {
            Id = Guid.NewGuid(),
            Name = name,
            Phone = phone,
            VehicleType = vehicleType,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string phone, VehicleType vehicleType)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Driver name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Driver phone cannot be empty.", nameof(phone));

        Name = name;
        Phone = phone;
        VehicleType = vehicleType;
    }

    public void MakeAvailable()
    {
        if (IsAvailable)
            throw new InvalidOperationException("Driver is already available.");

        IsAvailable = true;
    }

    public void MakeUnavailable()
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Driver is already unavailable.");

        IsAvailable = false;
    }
}
