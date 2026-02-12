namespace DeliveryApp.Application.DTOs.Customers;

public record CreateCustomerRequest(
    string Name,
    string Email,
    string Phone,
    string Address);
