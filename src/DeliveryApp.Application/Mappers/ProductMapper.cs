using DeliveryApp.Application.DTOs.Products;
using DeliveryApp.Domain.Entities;

namespace DeliveryApp.Application.Mappers;

public static class ProductMapper
{
    public static ProductResponse MapToResponse(Product product) =>
        new(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.ImageUrl,
            product.IsActive,
            product.CreatedAt,
            product.UpdatedAt,
            product.CategoryId,
            product.Category?.Name ?? string.Empty);
}
