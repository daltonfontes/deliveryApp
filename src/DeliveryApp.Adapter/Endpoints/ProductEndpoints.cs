
using DeliveryApp.Application.DTOs.Products;
using DeliveryApp.Application.Interfaces;

namespace DeliveryApp.Adapter.Endpoints;
public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapGet("/", async (IProductService service, CancellationToken ct) =>
            Results.Ok(await service.GetAllAsync(ct))).AllowAnonymous();

        group.MapGet("/active", async (IProductService service, CancellationToken ct) =>
            Results.Ok(await service.GetActiveAsync(ct))).AllowAnonymous();

        group.MapGet("/{id:guid}", async (Guid id, IProductService service, CancellationToken ct) =>
            await service.GetByIdAsync(id, ct) is { } product
                ? Results.Ok(product)
                : Results.NotFound()).AllowAnonymous();

        group.MapPost("/", async (CreateProductRequest request, IProductService service, CancellationToken ct) =>
        {
            var product = await service.CreateAsync(request, ct);
            return Results.Created($"/api/products/{product.Id}", product);
        }).RequireAuthorization("Admin");

        group.MapPut("/{id:guid}", async (Guid id, UpdateProductRequest request, IProductService service, CancellationToken ct) =>
            Results.Ok(await service.UpdateAsync(id, request, ct))).RequireAuthorization("Admin");

        group.MapDelete("/{id:guid}", async (Guid id, IProductService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        }).RequireAuthorization("Admin");

        return app;
    }
}
