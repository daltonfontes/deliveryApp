namespace DeliveryApp.Application.DTOs.Categories;

public record CategoryResponse(Guid Id, string Name, string Description, DateTime CreatedAt);
