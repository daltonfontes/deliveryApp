namespace DeliveryApp.Application.DTOs.Products;

public record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    string? ImageUrl,
    Guid CategoryId);
