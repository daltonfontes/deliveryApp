
using DeliveryApp.Application.DTOs.Orders;
using DeliveryApp.Application.Interfaces;

namespace DeliveryApp.Adapter.Endpoints;
public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        group.MapGet("/", async (IOrderService service, CancellationToken ct) =>
            Results.Ok(await service.GetAllAsync(ct))).RequireAuthorization("Admin");

        group.MapGet("/{id:guid}", async (Guid id, IOrderService service, CancellationToken ct) =>
            await service.GetByIdAsync(id, ct) is { } order
                ? Results.Ok(order)
                : Results.NotFound()).RequireAuthorization();

        group.MapGet("/customer/{customerId:guid}", async (Guid customerId, IOrderService service, CancellationToken ct) =>
            Results.Ok(await service.GetByCustomerIdAsync(customerId, ct))).RequireAuthorization();

        group.MapPost("/", async (CreateOrderRequest request, IOrderService service, CancellationToken ct) =>
        {
            var order = await service.CreateAsync(request, ct);
            return Results.Created($"/api/orders/{order.Id}", order);
        }).RequireAuthorization("Customer");

        group.MapPatch("/{id:guid}/status", async (Guid id, UpdateOrderStatusRequest request, IOrderService service, CancellationToken ct) =>
            Results.Ok(await service.UpdateStatusAsync(id, request, ct))).RequireAuthorization("Admin");

        group.MapPatch("/{id:guid}/assign-driver", async (Guid id, AssignDriverRequest request, IOrderService service, CancellationToken ct) =>
            Results.Ok(await service.AssignDriverAsync(id, request, ct))).RequireAuthorization("Admin");

        group.MapDelete("/{id:guid}", async (Guid id, IOrderService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        }).RequireAuthorization("Admin");

        return app;
    }
}
