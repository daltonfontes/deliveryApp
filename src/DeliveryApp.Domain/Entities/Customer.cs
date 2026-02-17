namespace DeliveryApp.Domain.Entities;

using DeliveryApp.Domain.Exceptions;

public class Customer
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? UserId { get; private set; }

    public ICollection<Order> Orders { get; private set; } = [];

    private Customer() { }

    public static Customer Create(string name, string email, string phone, string address)
    {
        if (string.IsNullOrWhiteSpace(name))    throw new ValidationException("Name is required.");
        if (string.IsNullOrWhiteSpace(email))   throw new ValidationException("Email is required.");
        if (string.IsNullOrWhiteSpace(phone))   throw new ValidationException("Phone is required.");
        if (string.IsNullOrWhiteSpace(address)) throw new ValidationException("Address is required.");

        return new Customer
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            Phone = phone,
            Address = address,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string email, string phone, string address)
    {
        if (string.IsNullOrWhiteSpace(name))    throw new ValidationException("Name is required.");
        if (string.IsNullOrWhiteSpace(email))   throw new ValidationException("Email is required.");
        if (string.IsNullOrWhiteSpace(phone))   throw new ValidationException("Phone is required.");
        if (string.IsNullOrWhiteSpace(address)) throw new ValidationException("Address is required.");

        Name = name;
        Email = email;
        Phone = phone;
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignUser(string userId) => UserId = userId;
}
