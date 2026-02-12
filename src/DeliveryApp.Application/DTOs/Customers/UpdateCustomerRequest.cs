namespace DeliveryApp.Application.DTOs.Customers;

public record UpdateCustomerRequest(
    string Name,
    string Email,
    string Phone,
    string Address);
