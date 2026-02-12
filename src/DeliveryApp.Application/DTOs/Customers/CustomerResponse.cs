namespace DeliveryApp.Application.DTOs.Customers;

public record CustomerResponse(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    string Address,
    DateTime CreatedAt);
