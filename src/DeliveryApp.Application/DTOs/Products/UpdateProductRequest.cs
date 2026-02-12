namespace DeliveryApp.Application.DTOs.Products;

public record UpdateProductRequest(
    string Name,
    string Description,
    decimal Price,
    string? ImageUrl,
    bool IsActive,
    Guid CategoryId);
