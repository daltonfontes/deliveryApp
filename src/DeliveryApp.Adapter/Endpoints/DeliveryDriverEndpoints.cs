
using DeliveryApp.Application.DTOs.DeliveryDrivers;
using DeliveryApp.Application.Interfaces;

namespace DeliveryApp.Adapter.Endpoints;
public static class DeliveryDriverEndpoints
{
    public static IEndpointRouteBuilder MapDeliveryDriverEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/delivery-drivers").WithTags("DeliveryDrivers").RequireAuthorization("Admin");

        group.MapGet("/", async (IDeliveryDriverService service, CancellationToken ct) =>
            Results.Ok(await service.GetAllAsync(ct)));

        group.MapGet("/available", async (IDeliveryDriverService service, CancellationToken ct) =>
            Results.Ok(await service.GetAvailableAsync(ct)));

        group.MapGet("/{id:guid}", async (Guid id, IDeliveryDriverService service, CancellationToken ct) =>
            await service.GetByIdAsync(id, ct) is { } driver
                ? Results.Ok(driver)
                : Results.NotFound());

        group.MapPost("/", async (CreateDeliveryDriverRequest request, IDeliveryDriverService service, CancellationToken ct) =>
        {
            var driver = await service.CreateAsync(request, ct);
            return Results.Created($"/api/delivery-drivers/{driver.Id}", driver);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateDeliveryDriverRequest request, IDeliveryDriverService service, CancellationToken ct) =>
            Results.Ok(await service.UpdateAsync(id, request, ct)));

        group.MapDelete("/{id:guid}", async (Guid id, IDeliveryDriverService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        });

        return app;
    }
}
