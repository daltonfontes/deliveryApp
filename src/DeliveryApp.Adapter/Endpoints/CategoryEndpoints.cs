
using DeliveryApp.Application.DTOs.Categories;
using DeliveryApp.Application.Interfaces;

namespace DeliveryApp.Adapter.Endpoints;
public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories").WithTags("Categories");

        group.MapGet("/", async (ICategoryService service, CancellationToken ct) =>
            Results.Ok(await service.GetAllAsync(ct))).AllowAnonymous();

        group.MapGet("/{id:guid}", async (Guid id, ICategoryService service, CancellationToken ct) =>
            await service.GetByIdAsync(id, ct) is { } category
                ? Results.Ok(category)
                : Results.NotFound()).AllowAnonymous();

        group.MapPost("/", async (CreateCategoryRequest request, ICategoryService service, CancellationToken ct) =>
        {
            var category = await service.CreateAsync(request, ct);
            return Results.Created($"/api/categories/{category.Id}", category);
        }).RequireAuthorization("Admin");

        group.MapPut("/{id:guid}", async (Guid id, UpdateCategoryRequest request, ICategoryService service, CancellationToken ct) =>
            Results.Ok(await service.UpdateAsync(id, request, ct))).RequireAuthorization("Admin");

        group.MapDelete("/{id:guid}", async (Guid id, ICategoryService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        }).RequireAuthorization("Admin");

        return app;
    }
}
