namespace DeliveryApp.Application.DTOs.Products;

public record ProductResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string? ImageUrl,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    Guid CategoryId,
    string CategoryName);
